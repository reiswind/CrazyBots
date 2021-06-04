
using Engine.Interface;
using Newtonsoft.Json;
using System;
//using Engine.Master;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour 
{
	internal float hexCellHeight = 0.25f;

	public int gridWidth = 20;
	public int gridHeight = 20;
	public float GameSpeed = 0.01f;

	internal Dictionary<Position, GroundCell> GroundCells { get; private set; }
	//internal Dictionary<string, UnitFrame> Units { get; private set; }
	internal Dictionary<string, UnitBase> BaseUnits { get; private set; }

	// Filled in UI Thread
	internal List<GameCommand> GameCommands { get; private set; }

	// Shared with backgound thread
	internal IGameController game;
	private bool windowClosed;
	private List<Move> newMoves;
	private List<GameCommand> newGameCommands;
	internal EventWaitHandle WaitForTurn = new EventWaitHandle(false, EventResetMode.AutoReset);
	internal EventWaitHandle WaitForDraw = new EventWaitHandle(false, EventResetMode.AutoReset);
	private Thread computeMoves;

	private bool useThread;

	private string remoteGameIndex;


	public MapInfo MapInfo;

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

		InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);

	}


	internal List<GameObject> smallTrees = new List<GameObject>();
	internal List<GameObject> smallRocks = new List<GameObject>();
	internal List<GameObject> obstacles = new List<GameObject>();

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
		{
			game = gameModel.CreateGame(gameModel.Seed.Value);
		}
		else
		{
			game = gameModel.CreateGame();
			gameModel.Seed = game.Seed;
		}

		GameCommands = new List<GameCommand>();
		GroundCells = new Dictionary<Position, GroundCell>();
		BaseUnits = new Dictionary<string, UnitBase>();

		AddRock("Rock Type1 01", obstacles, 0.8f);
		AddRock("Rock Type1 02", obstacles, 0.8f);
		AddRock("Rock Type5 02", obstacles, 0.3f);
		AddRock("Rock Type3 02", obstacles, 0.8f);
		AddRock("Rock Type6 04", obstacles, 0.8f);

		AddTree("Tree Type0 03", smallTrees, 0.2f);
		AddTree("Tree Type2 05", smallTrees, 0.2f);
		AddTree("Tree Type2 02", smallTrees, 0.2f);
		AddTree("Tree Type4 04", smallTrees, 0.2f);
		AddTree("Tree Type4 05", smallTrees, 0.2f);
		AddTree("Tree Type0 02", smallTrees, 0.2f);

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
				GroundCell hexCell = CreateCell(t, cellPrefab);

				GroundCells.Add(pos, hexCell);
			}
		}

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			
		}
		else
        {
			useThread = true;
		}

		if (useThread)
		{
			// Start game engine
			computeMoves = new Thread(new ThreadStart(ComputeMove));
			computeMoves.Start();
			WaitForDraw.Set();
		}
		else
        {
			StartCoroutine(StartRemoteGame(gameModel));
		}
	}

	private bool readyForNextMove;

	IEnumerator<object> StartRemoteGame(GameModel gameModel)
	{
		string body = JsonConvert.SerializeObject(gameModel);

		using (UnityWebRequest www = UnityWebRequest.Post("https://fastfertig.net/api/GameEngine", body))
		{
			yield return www.SendWebRequest();

			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(www.error);
			}
			else
			{
				remoteGameIndex = www.downloadHandler.text;
				readyForNextMove = true;
			}
		}
	}

	IEnumerator<object> GetRemoteMove()
	{
		if (string.IsNullOrEmpty(remoteGameIndex))
		{
			yield return null;
		}
		else
		{
			string body = "";
			using (UnityWebRequest www = UnityWebRequest.Post("https://fastfertig.net/api/GameMove/" + remoteGameIndex, body))
			{
				yield return www.SendWebRequest();

				if (www.result != UnityWebRequest.Result.Success)
				{
					Debug.Log(www.error);
				}
				else
				{
					string movesJson = www.downloadHandler.text;
					if (!string.IsNullOrEmpty(movesJson))
					{
						List<Move> current = JsonConvert.DeserializeObject(movesJson, typeof(List<Move>)) as List<Move>;

						newMoves.Clear();
						newMoves.AddRange(current);

						ProcessNewMoves();

						readyForNextMove = true;
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
				if (newGameCommands != null)
				{
					foreach (GameCommand gameCommand in newGameCommands)
					{
						if (gameCommand.Append)
							UnityEngine.Debug.Log("NEW to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y + " SHIFT");
						else
							UnityEngine.Debug.Log("NEW to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y);
					}
				}

				List<Move> current = game.ProcessMove(id, nextMove, newGameCommands);
				newGameCommands = null;

				if (newMoves.Count > 0)
				{
					// Happens during shutdown, ignore
					windowClosed = true;
				}
				else
				{
					newMoves.Clear();
					newMoves.AddRange(current);

					MapInfo = game.GetDebugMapInfo();
				}
				
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

	private void ProcessNewMoves()
	{
		List<Position> newUpdatedPositions = new List<Position>();

		if (MapInfo != null)
		{
			foreach (Position pos in MapInfo.Pheromones.Keys)
			{
				MapPheromone mapPheromone = MapInfo.Pheromones[pos];
				GroundCell hexCell = GroundCells[pos];
				hexCell.UpdatePheromones(mapPheromone);

				newUpdatedPositions.Add(pos);
				updatedPositions.Remove(pos);
			}
			foreach (Position pos in updatedPositions)
			{
				GroundCell hexCell = GroundCells[pos];
				hexCell.UpdatePheromones(null);
			}
			updatedPositions = newUpdatedPositions;

			// 
			foreach (MapPlayerInfo mapPlayerInfo in MapInfo.PlayerInfo.Values)
			{
			}
		}
		
		foreach (UnitBase unitBase in BaseUnits.Values)
		{
			if (unitBase.DestinationPos != null)
			{
				unitBase.CurrentPos = unitBase.DestinationPos;
				unitBase.DestinationPos = null;
				unitBase.PutAtCurrentPosition();
			}
		}


		foreach (Move move in newMoves)
		{
			if (move.MoveType == MoveType.Add)
			{
				if (BaseUnits.ContainsKey(move.UnitId))
				{
					// Happend in player view
				}
				else
				{
					CreateUnit(move);
				}
			}
			else if (move.MoveType == MoveType.Build)
			{
				CreateUnit(move);
			}
			/*
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
			}*/
			else if (move.MoveType == MoveType.Extract ||
					 move.MoveType == MoveType.Fire ||
					 move.MoveType == MoveType.Hit ||
				     move.MoveType == MoveType.UpdateStats)
			{
				if (BaseUnits.ContainsKey(move.UnitId))
				{
					UnitBase unit = BaseUnits[move.UnitId];
					unit.UpdateStats(move.Stats);

					if (move.MoveType == MoveType.Extract)
                    {
						unit.Extract(move);
                    }
					if (move.MoveType == MoveType.Fire)
					{
						unit.Fire(move);
					}
				}
				
			}
			else if (move.MoveType == MoveType.Move)
			{
				if (BaseUnits.ContainsKey(move.UnitId))
				{
					UnitBase unit = BaseUnits[move.UnitId];
					unit.MoveTo(move.Positions[1]);
				}
				
			}
			else if (move.MoveType == MoveType.Upgrade)
			{
				if (BaseUnits.ContainsKey(move.OtherUnitId))
				{
					// 
					UnitBase unit = BaseUnits[move.OtherUnitId];
					unit.Upgrade(move);
				}
				
			}
			else if (move.MoveType == MoveType.UpdateGround)
			{
				GroundCell hexCell = GroundCells[move.Positions[0]];
				hexCell.NextMove = move;
				hexCell.UpdateGround();
			}
			else if (move.MoveType == MoveType.Delete)
			{
				if (BaseUnits.ContainsKey(move.UnitId))
				{
					UnitBase unit = BaseUnits[move.UnitId];
					unit.Delete();
					BaseUnits.Remove(move.UnitId);
				}
				/*
				if (Units.ContainsKey(move.OtherUnitId))
				{
					UnitFrame unit = Units[move.UnitId];
					unit.NextMove = null;
					unit.Delete();
					Units.Remove(move.UnitId);
				}*/
			}
		}
		newMoves.Clear();
	}

	void invoke()
	{
		if (!useThread)
        {
			if (readyForNextMove)
			{
				readyForNextMove = false;
				StartCoroutine(GetRemoteMove());
			}
		}
		if (useThread)
		{
			if (WaitForTurn.WaitOne(10))
			{
				newGameCommands = GameCommands;
				GameCommands = new List<GameCommand>();

				ProcessNewMoves();
				WaitForDraw.Set();
			}
		}
	}

	public void MyDestroy(GameObject x)
    {
		Destroy(x);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Type Safety", "UNT0014:Invalid type for call to GetComponent", Justification = "<Pending>")]
    public T InstantiatePrefab<T>(string name)
    {
		GameObject prefab = (GameObject)Resources.Load("Prefabs/Unit/" + name);
		GameObject instance = Instantiate(prefab);

		T script = instance.GetComponent<T>();
		return script;
	}

	public GameObject InstantiatePrefab(string name)
	{
		GameObject prefab = (GameObject)Resources.Load("Prefabs/Unit/" + name);
		GameObject instance = Instantiate(prefab);
		return instance;
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
		string unitModel;

		if (move.Stats.BlueprintName.StartsWith("Fighter") ||
			move.Stats.BlueprintName.StartsWith("Worker"))
		{
			unitModel = "MovableUnitBigPart";
		}
		else
        {
			unitModel = "GroundUnit";
		}
		UnitBase unit = InstantiatePrefab<UnitBase>(unitModel);

		unit.HexGrid = this;
		unit.CurrentPos = move.Positions[0];

		unit.PlayerId = move.PlayerId;
		unit.MoveUpdateStats = move.Stats;
		unit.UnitId = move.UnitId;

		unit.Assemble();
		unit.PutAtCurrentPosition();

		if (move.Positions.Count > 1)
		{
			// Move to targetpos
			unit.DestinationPos = move.Positions[move.Positions.Count - 1];
		}
		BaseUnits.Add(move.UnitId, unit);

		
		/*
		else
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
				unit.currentPos = move.Positions[move.Positions.Count - 1];
			}
			else
			{
				// Move to targetpos
				unit.currentPos = move.Positions[0];
			}
			unit.Assemble();
			Units.Add(move.UnitId, unit);
		}
		*/
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

	private GroundCell CreateCell(Tile t, GameObject cellPrefabx)
	{
		int x = t.Pos.X;
		int y = t.Pos.Y;

		GameObject gameObjectCell = Instantiate(cellPrefabx);
		gameObjectCell.hideFlags = HideFlags.DontSave; //.enabled = false;	
		gameObjectCell.transform.SetParent(transform, false);
		gameObjectCell.name = "Ground " + x.ToString() + "," + y.ToString();

		GroundCell groundCell = gameObjectCell.GetComponent<GroundCell>();

		//HexCell cell = new HexCell();
		//cell.Cell = gameObjectCell;
		groundCell.HexGrid = this;

		Vector2 gridPos = new Vector2(x, y);
		Vector3 gridPos3 = CalcWorldPos(gridPos);

		groundCell.Tile = t;

		double height = t.Height;

		string materialName;
		if (t.IsSand())
		{
			materialName = "Materials/Sand";
		}
		else if (t.IsDarkSand())
		{
			materialName = "Materials/DarkSand";
		}
		else if (t.IsDarkWood())
		{
			materialName = "Materials/DarkWood";
		}
		else if (t.IsWood())
		{
			materialName = "Materials/Wood";
		}
		else if (t.IsLightWood())
		{
			materialName = "Materials/LightWood";
		}
		else if (t.IsGrassDark())
		{
			materialName = "Materials/GrassDark";
		}
		else
		{
			materialName = "Materials/Grass";
		}

		//gridPos3.y = tileY / 2;
		gridPos3.y = ((float)height * 5);
		gameObjectCell.transform.localPosition = gridPos3;

		//
		Material materialResource = Resources.Load<Material>(materialName);

		MeshRenderer meshRenderer = gameObjectCell.GetComponent<MeshRenderer>();
		meshRenderer.material = materialResource;

		groundCell.CreateMinerals();
		groundCell.CreateDestructables();
		groundCell.CreateObstacles();

		return groundCell;
	}
}