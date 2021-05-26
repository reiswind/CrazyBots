
using Engine.Interface;
using System;
//using Engine.Master;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour 
{
	public HexCell cellPrefab;
	public Text cellLabelPrefab;

	internal float hexCellHeight = 0.25f;

	public int gridWidth = 20;
	public int gridHeight = 20;
	public float GameSpeed = 0.01f;

	private Canvas gridCanvas;

	public Dictionary<Position, HexCell> GroundCells { get; private set; }
	public Dictionary<string, UnitFrame> Units { get; private set; }

	// Shard with backgound tread
	private IGameController game;
	private bool windowClosed;
	private List<Move> newMoves;
	public EventWaitHandle WaitForTurn = new EventWaitHandle(false, EventResetMode.AutoReset);
	public EventWaitHandle WaitForDraw = new EventWaitHandle(false, EventResetMode.AutoReset);
	private Thread computeMoves = null;

	private bool useThread = true;

	public void CreateGame(GameModel gameModel)
	{
		// (int)DateTime.Now.Ticks; -1789305431
		newMoves = new List<Move>();

		if (gameModel.Seed.HasValue)
			game = gameModel.CreateGame(gameModel.Seed.Value);
		else
			game = gameModel.CreateGame();

		GroundCells = new Dictionary<Position, HexCell>();
		Units = new Dictionary<string, UnitFrame>();

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

		if (useThread)
		{
			// Start game engine
			computeMoves = new Thread(new ThreadStart(ComputeMove));
			computeMoves.Start();
			WaitForDraw.Set();
		}
	}

	private float lastDeltaTime;

    public void Update()
    {
		if (!useThread)
        {
			lastDeltaTime += Time.deltaTime;
			if (lastDeltaTime > 1)
			{
				lastDeltaTime = 0;

				Move nextMove = new Move();
				nextMove.MoveType = MoveType.None;

				List<Move> newMoves = game.ProcessMove(0, nextMove);

				foreach (Move move in newMoves)
				{
					if (move.MoveType == MoveType.Add)
					{
						CreateUnit(move);
					}
					else if (move.MoveType == MoveType.UpdateStats)
					{
						UnitFrame unit = Units[move.UnitId];
						unit.UpdateStats(move.Stats);
					}
					else if (move.MoveType == MoveType.Move ||
							 move.MoveType == MoveType.Extract ||
							 move.MoveType == MoveType.Fire)
					{
						UnitFrame unit = Units[move.UnitId];
						unit.NextMove = move;
					}
					else if (move.MoveType == MoveType.Hit)
					{
						UnitFrame unit = Units[move.UnitId];
						unit.NextMove = move;
					}
					else if (move.MoveType == MoveType.Upgrade)
					{
						UnitFrame unit = Units[move.OtherUnitId];
						unit.NextMove = move;
					}
					else if (move.MoveType == MoveType.UpdateGround)
					{
						HexCell hexCell = GroundCells[move.Positions[0]];
						hexCell.NextMove = move;
					}
					else if (move.MoveType == MoveType.Delete)
					{
						Debug.Log("Delete Unit " + move.UnitId);

						UnitFrame unit = Units[move.UnitId];
						unit.NextMove = null;
						unit.Delete();
						Units.Remove(move.UnitId);
					}

				}
			}
		}
	}

    public void ComputeMove()
	{
		try
		{
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

				Move nextMove = new Move();
				nextMove.MoveType = MoveType.None;

				List<Move> current = game.ProcessMove(id, nextMove);
				if (newMoves.Count > 0)
				{
					// Heppens during shutdown, ignore
					windowClosed = true;
				}
				else
				{
					newMoves.Clear();
					newMoves.AddRange(current);
				}
				//lastMapInfo = Game.Map.GetMapInfo();
				
				// New move is ready, continue with next move
				WaitForTurn.Set();
			}
		}
		catch (Exception)
		{
			//throw new Exception("Game move wrecked " + err.Message);
		}
	}

	void Awake () 
	{
		if (GameSpeed == 0)
			GameSpeed = 0.01f;

		gridCanvas = GetComponentInChildren<Canvas>();

		UnityEngine.Object gameModelContent = Resources.Load("Models/Simple");
		//UnityEngine.Object gameModelContent = Resources.Load("Models/UnittestFight");
		//UnityEngine.Object gameModelContent = Resources.Load("Models/Unittest");

		GameModel gameModel;

		//string filename = @"C:\Develop\blazor\Client\Models\SoloAnt.json";
		//string filename = @"C:\Develop\blazor\Client\Models\UnittestFight.json";
		//string filename = @"C:\Develop\blazor\Client\Models\Simple.json";
		//string filename = @"C:\Develop\blazor\Client\Models\Unittest.json";
		if (gameModelContent != null)
		{
			var serializer = new DataContractJsonSerializer(typeof(GameModel));

			MemoryStream mem = new MemoryStream(Encoding.UTF8.GetBytes(gameModelContent.ToString()));
			gameModel = (GameModel)serializer.ReadObject(mem);
			
		}
		else
		{
			gameModel = new GameModel();
			gameModel.MapHeight = gridWidth;
			gameModel.MapWidth = gridHeight;

			if (gridWidth > 10)
			{
				gameModel.Players = new List<PlayerModel>();

				PlayerModel p = new PlayerModel();
				p.ControlLevel = 1;
				p.Id = 1;
				p.Name = "WebPLayer";
				gameModel.Players.Add(p);
			}
		}
		CreateGame(gameModel);

		InvokeRepeating("invoke", 0.5f, GameSpeed);
	}

    private void OnDestroy()
    {
		windowClosed = true;
    }

	void invoke()
	{
		if (WaitForTurn.WaitOne(10))
		{
			foreach (UnitFrame unitFrame in Units.Values)
            {
				//if (unitFrame.FinalDestination != null)
				{
					unitFrame.JumpToTarget(unitFrame.FinalDestination);
					//unitFrame.FinalDestination = null;
				}
				unitFrame.NextMove = null;
			}

			foreach (Move move in newMoves)
			{
				if (move.MoveType == MoveType.Add)
				{
					CreateUnit(move);
				}
				else if (move.MoveType == MoveType.UpdateStats)
				{
					UnitFrame unit = Units[move.UnitId];
					unit.UpdateStats(move.Stats);
				}
				else if (move.MoveType == MoveType.Move || 
					     move.MoveType == MoveType.Extract ||
						 move.MoveType == MoveType.Fire)
				{
					UnitFrame unit = Units[move.UnitId];
					unit.NextMove = move;
				}
				else if (move.MoveType == MoveType.Hit)
				{
					UnitFrame unit = Units[move.UnitId];
					unit.NextMove = move;
				}
				else if (move.MoveType == MoveType.Upgrade)
				{
					UnitFrame unit = Units[move.OtherUnitId];
					unit.NextMove = move;
				}
				else if (move.MoveType == MoveType.UpdateGround)
				{
					HexCell hexCell = GroundCells[move.Positions[0]];
					hexCell.NextMove = move;
				}
				else if (move.MoveType == MoveType.Delete)
				{
					Debug.Log("Delete Unit " + move.UnitId);

					UnitFrame unit = Units[move.UnitId];
					unit.NextMove = null;
					unit.Delete();
					Units.Remove(move.UnitId);
				}

			}
			newMoves.Clear();
			WaitForDraw.Set();
		}
	}

	public void MyDestroy(GameObject x)
    {
		Destroy(x);
    }
	public T InstantiatePrefab<T>(string name)
    {
		GameObject prefab = (GameObject)Resources.Load("Prefabs/Unit/" + name);
		GameObject instance = Instantiate(prefab);

		T script = instance.GetComponent<T>();
		return script;
	}
	public ParticleSystem MakeParticleSource(string resource)
    {
		ParticleSystem extractSourcePrefab = Resources.Load<ParticleSystem>("Particles\\" + resource);
		ParticleSystem extractSource;
		extractSource = Instantiate(extractSourcePrefab);
		return extractSource;
	}

	public ParticleSystemForceField MakeParticleTarget()
	{
		ParticleSystemForceField extractPrefab = Resources.Load<ParticleSystemForceField>("Particles\\" + "ExtractTarget");
		ParticleSystemForceField extract;
		extract = Instantiate(extractPrefab);
		return extract;
	}

	void CreateUnit(Move move)
	{
		UnitFrame unit = new UnitFrame();

		unit.HexGrid = this;
		unit.NextMove = move;

		unit.playerId = move.PlayerId;
		unit.MoveUpdateStats = move.Stats;
		unit.UnitId = move.UnitId;

		unit.currentPos = move.Positions[0];
		unit.Assemble();

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

	/*
	void CalcStartPos()
	{
		float offset = 0;
		if (gridHeight / 2 % 2 != 0)
			offset = hexWidth / 2;

		float x = -hexWidth * (gridWidth / 2) - offset;
		float z = hexHeight * 0.75f * (gridHeight / 2);

		startPos = new Vector3(0, 0, 0);
	}*/

	private Vector3 CalcWorldPos(Vector2 gridPos)
	{
		float gridSizeX = 1.50f;
		float gridSizeY = 1.75f;
		float halfGridSize = 0.86f;

		float x = gridPos.x;
		float y = gridPos.y;

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
		cell.enabled = false;
		//cell.hideFlags = HideFlags.HideInHierarchy;
		cell.transform.SetParent(transform, false);
		cell.gameObject.name = "Ground " + x.ToString() + "," + y.ToString();
		Vector2 gridPos = new Vector2(x, y);
		Vector3 gridPos3 = CalcWorldPos(gridPos);

		cell.Tile = t;

		double height = t.Height;
		float tileY;

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

		MeshRenderer meshRenderer = cell.GetComponent<MeshRenderer>();
		meshRenderer.material = material;

		/*

				Text label = Instantiate<Text>(cellLabelPrefab);
				label.rectTransform.SetParent(gridCanvas.transform, false);
				label.rectTransform.anchoredPosition = new Vector2(gridPos3.x, gridPos3.z);
				label.text = t.Pos.X.ToString() + "," + t.Pos.Y;
		*/
		return cell;
	}
}