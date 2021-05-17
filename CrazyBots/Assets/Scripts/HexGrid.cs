using Assets.Scripts;
using Engine.Interface;
using System;
//using Engine.Master;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

	//public Color defaultColor = Color.white;

	public HexCell cellPrefab;
	public Text cellLabelPrefab;
	public Engine1 unitFramePrefab;

	public int gridWidth = 20;
	public int gridHeight = 20;
	public float gap = 0.0f;

	float hexWidth = 1.732f;
	float hexHeight = 2.0f;

	Vector3 startPos;
	Canvas gridCanvas;

	public Dictionary<Position, HexCell> GroundCells { get; private set; }
	public Dictionary<string, Engine1> Units { get; private set; }

	// Shard with backgound tread
	private IGameController game;
	private bool windowClosed;
	private List<Move> newMoves;
	public EventWaitHandle WaitForTurn = new EventWaitHandle(false, EventResetMode.AutoReset);
	public EventWaitHandle WaitForDraw = new EventWaitHandle(false, EventResetMode.AutoReset);
	private Thread computeMoves = null;

	public void CreateGame(GameModel gameModel)
	{
		// (int)DateTime.Now.Ticks; -1789305431
		newMoves = new List<Move>();
		game = gameModel.CreateGame(-1789305431);

		GroundCells = new Dictionary<Position, HexCell>();
		Units = new Dictionary<string, Engine1>();

		// Render ground
		for (int y = 0; y < game.Map.MapHeight; y++)
		{
			for (int x = 0; x < game.Map.MapWidth; x++)
			{
				Position pos = new Position(x, y);
				Tile t = game.Map.GetTile(pos);
				HexCell hexCell = CreateCell(t);

				GroundCells.Add(pos, hexCell);
			}
		}

		// Start game engine
		computeMoves = new Thread(new ThreadStart(ComputeMove));
		computeMoves.Start();
		WaitForDraw.Set();
	}

	public void ComputeMove()
	{
		try
		{
			// Prerender some ground
			//ClientMap.RenderBackgound(Game, allImages, 0, 0, Game.Map.MapWidth, Game.Map.MapHeight, true, gridSize);

			while (!windowClosed)
			{
				long iTicks = DateTime.Now.Ticks;
				DateTime tStart = DateTime.Now;

				while (!windowClosed)
				{
					if (!WaitForDraw.WaitOne(10))
					{
						// Animation not ready
						Thread.Sleep(10);
						continue;
					}
					break;
				}

				int id = 0;

				//lock (allImages)
				{
					Move nextMove = new Move();
					nextMove.MoveType = MoveType.None;

					List<Move> current = game.ProcessMove(id, nextMove);
					if (newMoves.Count > 0)
                    {
						int x = 0;
                    }
					newMoves.Clear();
					newMoves.AddRange(current);

					//ProcessAreas(allImages);
					//ProcessCurrentMoves(newMoves, allImages);
					//lastMapInfo = Game.Map.GetMapInfo();
				}
				// New move is ready, continue with next move
				WaitForTurn.Set();
			}
		}
		catch (Exception err)
		{
			throw new Exception("Game move wrecked " + err.Message);
		}
	}

	void Awake () 
	{
		gridCanvas = GetComponentInChildren<Canvas>();

		GameModel gameModel = new GameModel();
		gameModel.MapHeight = gridWidth;
		gameModel.MapWidth = gridHeight;

		if (gridWidth > 10)
		{
			gameModel.Players = new List<PlayerModel>();

			PlayerModel p = new PlayerModel();
			p.ControlLevel = 1;
			p.Id = 1;
			p.Name = "WebPLayer";
			p.StartPosition = new Position(10, 10);
			gameModel.Players.Add(p);
		}
		CreateGame(gameModel);

		InvokeRepeating("invoke", 1f, 1f);
	}

    private void OnDestroy()
    {
		windowClosed = true;
    }

    void Start()
	{
		CalcStartPos();

		hexWidth += hexWidth * gap;
		hexHeight += hexHeight * gap;
	}

	void invoke()
	{
		if (WaitForTurn.WaitOne(10))
		{
			foreach (Engine1 unitFrame in Units.Values)
            {
				if (unitFrame.FinalDestination != null)
				{
					unitFrame.JumpToTarget(unitFrame.FinalDestination);
					unitFrame.FinalDestination = null;
				}
				unitFrame.NextMove = null;
			}

			foreach (Move move in newMoves)
			{
				if (move.MoveType == MoveType.Add)
				{
					CreateUnit(move);
				}
				else if (move.MoveType == MoveType.Move)
				{
					Engine1 unit = Units[move.UnitId];
					unit.NextMove = move;
				}
			}
			newMoves.Clear();
			WaitForDraw.Set();
		}
	}

	void CreateUnit(Move move)
	{
		Position pos = move.Positions[move.Positions.Count- 1];
		Engine1 unit = Instantiate<Engine1>(unitFramePrefab);

		unit.HexGrid = this;
		unit.transform.SetParent(transform, false);

		//unit.JumpToTarget(pos);
		unit.NextMove = move;

		HexCell targetCell = GroundCells[pos];

		Vector3 unitPos3 = targetCell.transform.localPosition;
		unitPos3.y -= 1;
		unit.transform.position = unitPos3;

		unit.X = pos.X;
		unit.Z = pos.Y;

		Units.Add(move.UnitId, unit);
		/*
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(unitPos3.x, unitPos3.z);
		label.text = "\r\n" + move.UnitId;*/
	}

	/*
	public void ColorCell (Vector3 position, Color color) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = color;
		hexMesh.Triangulate(cells);
	}*/

	public static Position GetPixelPos(int x, int y, int topx, int topy, int gridSize)
	{
		int HalfGridSize = gridSize / 2;
		int gapX = topx * gridSize;
		int gapY = topy * gridSize;

		if (x % 2 != 0 && y % 2 != 0)
		{
			return new Position((x * gridSize) - gapX, (y * gridSize) + HalfGridSize - gapY);
		}
		else if (x % 2 == 0 && y % 2 != 0)
		{
			return new Position((x * gridSize) - gapX, (y * gridSize) - gapY);
		}
		else if (x % 2 != 0 && y % 2 == 0)
		{
			return new Position((x * gridSize) - gapX, (y * gridSize) + HalfGridSize - gapY);
		}
		return new Position((x * gridSize) - gapX, y * gridSize - gapY);
	}

	void CalcStartPos()
	{
		float offset = 0;
		if (gridHeight / 2 % 2 != 0)
			offset = hexWidth / 2;

		float x = -hexWidth * (gridWidth / 2) - offset;
		float z = hexHeight * 0.75f * (gridHeight / 2);

		startPos = new Vector3(0, 0, 0);
	}

	private Vector3 CalcWorldPos(Vector2 gridPos)
	{
		float gridSizeX = 1.50f;
		float gridSizeY = 1.75f;
		float halfGridSize = 0.86f;

		float x = gridPos.x;
		float y = gridPos.y;

		//float x = startPos.x + gridPos.x * hexWidth + offset;
		//float z = startPos.z - gridPos.y * hexHeight * 0.75f;

		if (x % 2 != 0 && y % 2 != 0)
		{
			return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
		}
		else if (x % 2 == 0 && y % 2 != 0)
		{
			return new Vector3((x * gridSizeX), 0, -(y * gridSizeY));
		}
		else if (x % 2 != 0 && y % 2 == 0)
		{
			return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
		}
		return new Vector3((x * gridSizeX), 0,  -y * gridSizeY);
	}

	private HexCell CreateCell(Tile t)
	{
		int x = t.Pos.X;
		int y = t.Pos.Y;

		HexCell cell = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);

		Vector2 gridPos = new Vector2(x, y);
		Vector3 gridPos3 = CalcWorldPos(gridPos);


		//cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.X = t.Pos.X;
		cell.Z = t.Pos.Y;

		double height = t.Height;
		float tileY = 0f;

		string materialName;
		if (height > 0.27 && height < 0.33)
		{
			materialName = "Materials/Sand";
			tileY = 0.3f;
		}
		else if (height >= 0.26 && height <= 0.32)
		{
			materialName = "Materials/DarkSand";
			tileY = 0.4f;
		}
		else if (height > 0.48 && height < 0.52)
		{
			materialName = "Materials/DarkWood";
			tileY = 0.9f;
		}
		else if (height > 0.47 && height < 0.53)
		{
			materialName = "Materials/Wood";
			tileY = 0.8f;
		}
		else if (height >= 0.46 && height <= 0.54)
		{
			materialName = "Materials/LightWood";
			tileY = 0.7f;
		}
		else if (height >= 0.45 && height <= 0.55)
		{
			materialName = "Materials/GrassDark";
			tileY = 0.6f;
		}
		else
		{
			materialName = "Materials/Grass";
			tileY = 0.5f;
		}

		gridPos3.y = tileY / 2;
		cell.transform.localPosition = gridPos3;

		//
		Material material = Resources.Load<Material>(materialName);
		//UnityEngine.Object material = Resources.Load("Materials/Grass");

		MeshRenderer meshRenderer = cell.GetComponent<MeshRenderer>();
		// Get the current material applied on the GameObject
		//Material oldMaterial = meshRenderer.material;
		//Debug.Log("Applied Material: " + oldMaterial.name);
		// Set the new material on the GameObject
		meshRenderer.material = material;

		/*
		if (t != null && t.Height > 0.05f)

			cell.color = Color.green;
		else
			cell.color = defaultColor;
		*/
		/*

				Text label = Instantiate<Text>(cellLabelPrefab);
				label.rectTransform.SetParent(gridCanvas.transform, false);
				label.rectTransform.anchoredPosition = new Vector2(gridPos3.x, gridPos3.z);
				label.text = t.Pos.X.ToString() + "," + t.Pos.Y;
		*/
		return cell;
	}
}