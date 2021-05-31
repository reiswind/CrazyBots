
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
	internal float hexCellHeight = 0.25f;

	public int gridWidth = 20;
	public int gridHeight = 20;
	public float GameSpeed = 0.01f;

	public Dictionary<Position, HexCell> GroundCells { get; private set; }
	public Dictionary<string, UnitFrame> Units { get; private set; }

	// Shared with backgound tread
	internal IGameController game;
	private bool windowClosed;
	private List<Move> newMoves;
	public EventWaitHandle WaitForTurn = new EventWaitHandle(false, EventResetMode.AutoReset);
	public EventWaitHandle WaitForDraw = new EventWaitHandle(false, EventResetMode.AutoReset);
	private Thread computeMoves = null;

	private bool useThread = true;

	public MapInfo MapInfo;

	void Awake()
	{
	}

	internal void StartGame()
	{ 
		if (GameSpeed == 0)
			GameSpeed = 0.01f;

		//gridCanvas = GetComponentInChildren<Canvas>();

		UnityEngine.Object gameModelContent = Resources.Load("Models/Simple");
		//UnityEngine.Object gameModelContent = Resources.Load("Models/UnittestFight");
		//UnityEngine.Object gameModelContent = Resources.Load("Models/Unittest");
		//UnityEngine.Object gameModelContent = Resources.Load("Models/UnittestOutpost");

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

			gameModel.MapHeight = gridWidth;
			gameModel.MapWidth = gridHeight;

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


	internal List<GameObject> smallTrees = new List<GameObject>();
	internal List<GameObject> smallRocks = new List<GameObject>();

	public void AddTree(string name, List<GameObject> trees, float scale)
    {
		GameObject treePrefab = Resources.Load<GameObject>("LowPolyTreePack/Prefabs/" + name);

		Vector3 sc = new Vector3();
		sc.x = scale;
		sc.y = scale;
		sc.z = scale;
		treePrefab.transform.localScale = sc;

		trees.Add(treePrefab);
	}

	public void AddRock(string name, List<GameObject> rocks, float scale)
	{
		GameObject treePrefab = Resources.Load<GameObject>("LowPolyRockPack/Prefabs/" + name);

		Vector3 sc = new Vector3();
		sc.x = scale;
		sc.y = scale;
		sc.z = scale;
		treePrefab.transform.localScale = sc;

		rocks.Add(treePrefab);
	}

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

		AddTree("Tree Type0 03", smallTrees, 0.2f);
		AddTree("Tree Type2 05", smallTrees, 0.2f);
		AddTree("Tree Type2 02", smallTrees, 0.2f);
		AddTree("Tree Type4 04", smallTrees, 0.2f);
		AddTree("Tree Type4 05", smallTrees, 0.2f);
		AddTree("Tree Type0 02", smallTrees, 0.2f);


		//AddRock("Rock Type2 03", smallRocks, 0.3f);
		AddRock("Rock Type1 01", smallRocks, 0.3f);
		AddRock("Rock Type1 02", smallRocks, 0.3f);
		AddRock("Rock Type1 03", smallRocks, 0.3f);
		AddRock("Rock Type1 04", smallRocks, 0.3f);

		AddRock("Rock Type2 01", smallRocks, 0.3f);
		AddRock("Rock Type2 02", smallRocks, 0.3f);
		AddRock("Rock Type2 03", smallRocks, 0.3f);
		AddRock("Rock Type2 04", smallRocks, 0.3f);

		AddRock("Rock Type3 01", smallRocks, 0.3f);
		AddRock("Rock Type3 02", smallRocks, 0.3f);
		AddRock("Rock Type3 03", smallRocks, 0.3f);
		AddRock("Rock Type3 04", smallRocks, 0.3f);

		AddRock("Rock Type4 01", smallRocks, 0.3f);
		AddRock("Rock Type4 02", smallRocks, 0.3f);
		AddRock("Rock Type4 03", smallRocks, 0.3f);
		AddRock("Rock Type4 04", smallRocks, 0.3f);

		AddRock("Rock Type3 01", smallRocks, 0.3f);
		AddRock("Rock Type3 02", smallRocks, 0.3f);
		AddRock("Rock Type3 03", smallRocks, 0.3f);
		AddRock("Rock Type3 04", smallRocks, 0.3f);

		AddTree("Tree Type7 01", smallRocks, 0.35f);
		AddTree("Tree Type7 02", smallRocks, 0.35f);
		AddTree("Tree Type7 03", smallRocks, 0.35f);
		AddTree("Tree Type7 04", smallRocks, 0.35f);

		GameObject cellPrefab = (GameObject)Resources.Load("Prefabs/Terrain/HexCell 2");

		// Render ground
		for (int y = 0; y < game.Map.MapHeight; y++)
		{
			for (int x = 0; x < game.Map.MapWidth; x++)
			{
				Position pos = new Position(x, y);
				Tile t = game.Map.GetTile(pos);
				HexCell hexCell = CreateCell(t, cellPrefab);

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
		/*
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
						hexCell.UpdateGround();
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
		}*/
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

					MapInfo = game.GetDebugMapInfo();
				}
				//lastMapInfo = game.Map.GetMapInfo();
				
				// New move is ready, continue with next move
				WaitForTurn.Set();
			}
		}
		catch (Exception)
		{
			//throw new Exception("Game move wrecked " + err.Message);
		}
	}

    private void OnDestroy()
    {
		windowClosed = true;
    }

	private List<Position> updatedPositions = new List<Position>();

	void invoke()
	{
		if (WaitForTurn.WaitOne(10))
		{
			List<Position> newUpdatedPositions = new List<Position>();

			foreach (Position pos in MapInfo.Pheromones.Keys)
			{
				MapPheromone mapPheromone = MapInfo.Pheromones[pos];
				HexCell hexCell = GroundCells[pos];
				hexCell.Update(mapPheromone);

				newUpdatedPositions.Add(pos);
				updatedPositions.Remove(pos);
			}
			foreach (Position pos in updatedPositions)
            {
				HexCell hexCell = GroundCells[pos];
				hexCell.Update(null);
			}
			updatedPositions = newUpdatedPositions;

			// 
			foreach (MapPlayerInfo mapPlayerInfo in MapInfo.PlayerInfo.Values)
            {
			}

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
					if (Units.ContainsKey(move.UnitId))
					{
						// Happend in player view
					}
					else
					{
						CreateUnit(move);
					}
				}
				else if (move.MoveType == MoveType.UpdateStats)
				{
					if (Units.ContainsKey(move.UnitId))
					{
						UnitFrame unit = Units[move.UnitId];
						unit.UpdateStats(move.Stats);
					}
					else
					{
						// Happend in player view
					}
				}
				else if (move.MoveType == MoveType.Move || 
					     move.MoveType == MoveType.Extract ||
						 move.MoveType == MoveType.Fire)
				{
					if (Units.ContainsKey(move.UnitId)) // Playervision
					{
						UnitFrame unit = Units[move.UnitId];
						unit.NextMove = move;
					}
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
					hexCell.UpdateGround();
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

		if (move.Stats.EngineLevel == 0)
		{
			// Cannot move to targetpos
			unit.currentPos = move.Positions[move.Positions.Count-1];
		}
		else
		{
			// Move to targetpos
			unit.currentPos = move.Positions[0];
		}
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

	private HexCell CreateCell(Tile t, GameObject cellPrefabx)
	{
		int x = t.Pos.X;
		int y = t.Pos.Y;

		GameObject gameObjectCell = Instantiate(cellPrefabx);
		gameObjectCell.hideFlags = HideFlags.DontSave; //.enabled = false;
		gameObjectCell.transform.SetParent(transform, false);
		gameObjectCell.name = "Ground " + x.ToString() + "," + y.ToString();

		HexCell cell = new HexCell();
		cell.Cell = gameObjectCell;
		cell.HexGrid = this;

		Vector2 gridPos = new Vector2(x, y);
		Vector3 gridPos3 = CalcWorldPos(gridPos);

		cell.Tile = t;

		double height = t.Height;
		float tileY;

		string materialName;
		if (t.IsSand())
		{
			materialName = "Materials/Sand";
			tileY = 0.3f;
		}
		else if (t.IsDarkSand())
		{
			materialName = "Materials/DarkSand";
			tileY = 0.4f;
		}
		else if (t.IsDarkWood())
		{
			materialName = "Materials/DarkWood";
			tileY = 0.9f;
		}
		else if (t.IsWood())
		{
			materialName = "Materials/Wood";
			tileY = 0.8f;
		}
		else if (t.IsLightWood())
		{
			materialName = "Materials/LightWood";
			tileY = 0.7f;
		}
		else if (t.IsGrassDark())
		{
			materialName = "Materials/GrassDark";
			tileY = 0.6f;
		}
		else
		{
			materialName = "Materials/Grass";
			tileY = 0.5f;
		}

		//gridPos3.y = tileY / 2;
		gridPos3.y = ((float)height * 5);
		gameObjectCell.transform.localPosition = gridPos3;

		//
		Material materialResource = Resources.Load<Material>(materialName);

		MeshRenderer meshRenderer = gameObjectCell.GetComponent<MeshRenderer>();
		meshRenderer.material = materialResource;

		cell.CreateMinerals();
		cell.CreateTrees();

		return cell;
	}
}