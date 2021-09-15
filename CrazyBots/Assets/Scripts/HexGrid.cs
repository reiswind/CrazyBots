
using Engine.Interface;
using Newtonsoft.Json;
using System;
//using Engine.Master;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class HexGrid : MonoBehaviour
	{
		internal float hexCellHeight = 0.0f;

		public int gridWidth = 20;
		public int gridHeight = 20;
		public float GameSpeed = 0.01f;

		internal Dictionary<Position, GroundCell> GroundCells { get; private set; }
		internal Dictionary<string, UnitBase> BaseUnits { get; private set; }
		internal Dictionary<Position, UnitBase> UnitsInBuild { get; private set; }


		// Filled in UI Thread
		internal Dictionary<Position, GameCommand> GameCommands { get; private set; }
		internal Dictionary<Position, GameCommand> ActiveGameCommands { get; private set; }

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

			//UnityEngine.Object gameModelContent = Resources.Load("Models/Simple");
			//UnityEngine.Object gameModelContent = Resources.Load("Models/UnittestFight");
			//UnityEngine.Object gameModelContent = Resources.Load("Models/Unittest");
			//UnityEngine.Object gameModelContent = Resources.Load("Models/UnittestOutpost");
			UnityEngine.Object gameModelContent = Resources.Load("Models/Test");

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
				gameModel.MapHeight = gridHeight;
				gameModel.MapWidth = gridWidth;

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

			UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
			foreach (GameObject gameObject in scene.GetRootGameObjects())
			{
				if (gameObject.name == "Strategy Camera")
				{
					StrategyCamera strategyCamera = gameObject.GetComponentInChildren<StrategyCamera>();

					strategyCamera.JumpTo(this, game.Players[1].StartZone.Center);
				}
			}
			InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);
		}

		/*
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
		*/
		private Dictionary<string, GameObject> terrainResources = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> treeResources = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> rockResources = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> unitResources = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> bushResources = new Dictionary<string, GameObject>();
		private Dictionary<string, GameObject> obstaclesResources = new Dictionary<string, GameObject>();
		private Dictionary<string, Material> materialResources = new Dictionary<string, Material>();
		private Dictionary<string, GameObject> particlesResources = new Dictionary<string, GameObject>();

		private void InitMaterials()
		{
			UnityEngine.Object[] allResources = Resources.LoadAll("Materials");
			foreach (UnityEngine.Object resource in allResources)
			{
				Material gameObject = resource as Material;
				if (gameObject != null)
				{
					materialResources.Add(gameObject.name, gameObject);

				}
			}
		}
		private void InitParticless()
		{
			UnityEngine.Object[] allResources = Resources.LoadAll("Particles");
			foreach (UnityEngine.Object resource in allResources)
			{
				GameObject gameObject = resource as GameObject;
				if (gameObject != null)
				{
					particlesResources.Add(gameObject.name, gameObject);

				}
			}
		}

		private void InitResources(Dictionary<string, GameObject> resources, string path)
		{
			UnityEngine.Object[] allResources = Resources.LoadAll(path);
			foreach (UnityEngine.Object resource in allResources)
			{
				GameObject gameObject = resource as GameObject;
				if (gameObject != null)
				{
					resources.Add(gameObject.name, gameObject);

				}
			}
		}


		private void InitResources()
		{
			InitMaterials();
			InitParticless();
			InitResources(obstaclesResources, "Prefabs/Obstacles");
			InitResources(terrainResources, "Prefabs/Terrain");
			InitResources(terrainResources, "Prefabs/Items");
			InitResources(treeResources, "Prefabs/Trees");
			InitResources(rockResources, "Prefabs/Rocks");
			InitResources(unitResources, "Prefabs/Unit");
			InitResources(unitResources, "Prefabs/Buildings");
			InitResources(bushResources, "Prefabs/Bushes");
		}

		public GameObject GetTerrainResource(string name)
		{
			if (terrainResources.ContainsKey(name))
				return terrainResources[name];
			return null;
		}
		public GameObject GetUnitResource(string name)
		{
			if (unitResources.ContainsKey(name))
				return unitResources[name];
			return null;
		}
		public Material GetMaterial(string name)
		{
			if (materialResources.ContainsKey(name))
				return materialResources[name];
			return null;
		}

		public GameObject CreateTileObject(Transform transform, TileObject tileObject)
		{
			GameObject prefab;

			if (tileObject.TileObjectType == TileObjectType.Tree)
			{
				prefab = GetTerrainResource("ItemWood");
			}
			else if (tileObject.TileObjectType == TileObjectType.Bush)
			{
				prefab = GetTerrainResource("ItemBush");
			}
			else if (tileObject.TileObjectType == TileObjectType.Mineral)
			{
				prefab = GetTerrainResource("ItemCrystal");
			}
			else
			{
				prefab = GetTerrainResource("Marker");
				//prefab = null;
			}
			GameObject gameTileObject = null;
			if (prefab != null)
			{
				gameTileObject = Instantiate(prefab, transform, false);
			}
			return gameTileObject;
		}

		public GameObject CreateDestructable(Transform transform, TileObject tileObject)
		{
			GameObject prefab;
			float y;

			if (tileObject.TileObjectType == TileObjectType.Tree)
			{
				int idx = game.Random.Next(treeResources.Count);
				prefab = treeResources.Values.ElementAt(idx);
				y = prefab.transform.position.y;
			}
			else if (tileObject.TileObjectType == TileObjectType.Bush)
			{
				int idx = game.Random.Next(bushResources.Count);
				prefab = bushResources.Values.ElementAt(idx);
				y = prefab.transform.position.y;
			}
			else if (tileObject.TileObjectType == TileObjectType.Mineral)
			{
				prefab = GetTerrainResource("ItemCrystal");
				y = 0.05f;
			}
			else
			{
				y = 0f;
				prefab = null;
			}

			GameObject gameTileObject = null;
			if (prefab != null)
			{
				gameTileObject = Instantiate(prefab, transform, false);

				Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
				Vector3 unitPos3 = transform.position;
				unitPos3.x += (randomPos.x * 0.5f);
				unitPos3.z += (randomPos.y * 0.7f);
				unitPos3.y += y;
				gameTileObject.transform.position = unitPos3;
				gameTileObject.name = tileObject.TileObjectType.ToString();
			}
			return gameTileObject;
		}

		public GameObject CreateObstacle(Transform transform)
		{
			int idx = game.Random.Next(obstaclesResources.Count);
			GameObject prefab = obstaclesResources.Values.ElementAt(idx);
			GameObject obstacle = Instantiate(prefab, transform, false);
			return obstacle;
		}

		public void CreateGame(GameModel gameModel)
		{
			InitResources();

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

			GameCommands = new Dictionary<Position, GameCommand>();
			ActiveGameCommands = new Dictionary<Position, GameCommand>();
			GroundCells = new Dictionary<Position, GroundCell>();
			BaseUnits = new Dictionary<string, UnitBase>();
			UnitsInBuild = new Dictionary<Position, UnitBase>();


			GameObject cellPrefab = GetTerrainResource("HexCell");

			//foreach (MapSector mapSector in game.Map.Sectors.Values)

			//MapSector mapSector = game.Map.Sectors.ElementAt(100).Value;
			{
				foreach (Tile t in game.Map.Tiles.Values)
				{
					MoveUpdateStats moveUpdateStats = game.Map.CollectGroundStats(t.Pos);

					GroundCell hexCell = CreateCell(t.Pos, moveUpdateStats.MoveUpdateGroundStat, cellPrefab);
					if (GroundCells.ContainsKey(t.Pos))
					{
					}
					else
					{
						GroundCells.Add(t.Pos, hexCell);
					}
				}
			}
			/*
			mapSector = game.Map.Sectors.ElementAt(101).Value;
			{
				foreach (Tile t in mapSector.Tiles.Values)
				{
					GroundCell hexCell = CreateCell(t, cellPrefab);
					if (!GroundCells.ContainsKey(t.Pos))
						GroundCells.Add(t.Pos, hexCell);
				}
			}*/


			/*
			for (int y = 0; y < game.Map.MapHeight; y++)
			{
				for (int x = 0; x < game.Map.MapWidth; x++)
				{
					Position pos = new Position(x, y);
					Tile t = game.Map.GetTile(pos);
					if (t != null)
					{
						GroundCell hexCell = CreateCell(t, cellPrefab);
						GroundCells.Add(pos, hexCell);
					}
				}
			}*/

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

		private string serverUrl = "https://fastfertig.net/api/";
		//private string serverUrl = "https://localhost:44324/api/";

		IEnumerator<object> StartRemoteGame(GameModel gameModel)
		{
			string body = JsonConvert.SerializeObject(gameModel);

			using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "GameEngine", body))
			{
				yield return www.SendWebRequest();

				if (www.isHttpError)
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
				using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "GameMove/" + remoteGameIndex, body))
				{
					yield return www.SendWebRequest();

					if (www.isHttpError)
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
			catch (Exception err)
			{
				throw new Exception("Game move wrecked " + err.Message);
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
				/*
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
				*/
				updatedPositions = newUpdatedPositions;

				// 
				foreach (MapPlayerInfo mapPlayerInfo in MapInfo.PlayerInfo.Values)
				{
				}

				/* Update all
				foreach (Position pos in GroundCells.Keys)
				{
					MapPheromone mapPheromone = null;
					if (MapInfo.Pheromones.ContainsKey(pos))
						mapPheromone = MapInfo.Pheromones[pos];

					GroundCell hexCell = GroundCells[pos];
					hexCell.UpdatePheromones(mapPheromone);
				}*/
			}
			foreach (UnitBase unitBase in BaseUnits.Values)
			{
				if (unitBase.DestinationPos != null)
				{
					unitBase.CurrentPos = unitBase.DestinationPos;
					unitBase.DestinationPos = null;
					unitBase.PutAtCurrentPosition(true);
				}				
			}
			FinishTransits();
			List<UnitBase> deletedUnits = new List<UnitBase>();

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
						// Gamestart
						CreateUnit(move);
					}
				}
				else if (move.MoveType == MoveType.Build)
				{
					// Build in game
					CreateUnit(move);
				}
				else if (move.MoveType == MoveType.Hit)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						unit.HasBeenHit(move);

						/*
						if (unit.PartsThatHaveBeenHit == null)
							unit.PartsThatHaveBeenHit = new List<string>();
						unit.PartsThatHaveBeenHit.Add(move.OtherUnitId);*/
						//int level;
						//TileObjectType tileObjectType = TileObject.GetTileObjectTypeFromString(move.OtherUnitId, out level);

						//unit.PartExtracted(tileObjectType);

					}
				}
				else if (move.MoveType == MoveType.Fire)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						unit.Fire(move);
					}
				}
				else if (move.MoveType == MoveType.Extract)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						UnitBase otherUnit = null; ;
						if (move.OtherUnitId.StartsWith("unit"))
							otherUnit = BaseUnits[move.OtherUnitId];
						unit.Extract(move, unit, otherUnit);
					}
				}
				else if (move.MoveType == MoveType.Transport)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						unit.Transport(move);
					}
				}
				else if (move.MoveType == MoveType.UpdateStats)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						if (unit.PlayerId != move.PlayerId)
						{
							unit.ChangePlayer(move.PlayerId);
						}
						unit.UpdateStats(move.Stats);
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
				else if (move.MoveType == MoveType.CommandComplete)
				{
					if (UnitsInBuild.ContainsKey(move.Positions[0]))
					{
						// Remove Ghost from command
						UnitBase unit = UnitsInBuild[move.Positions[0]];
						if (unit != null)
							unit.Delete();
						GroundCell hexCell = GroundCells[move.Positions[0]];
						hexCell.SetAttack(false);
						UnitsInBuild.Remove(move.Positions[0]);
					}
				}
				else if (move.MoveType == MoveType.Upgrade)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						UnitBase upgradedUnit = BaseUnits[move.OtherUnitId];
						unit.Upgrade(move, upgradedUnit);

					}
				}
				else if (move.MoveType == MoveType.UpdateGround)
				{
					GroundCell hexCell = GroundCells[move.Positions[0]];
					hexCell.GroundStat = move.Stats.MoveUpdateGroundStat;
					hexCell.UpdateGround();
				}
				else if (move.MoveType == MoveType.Delete)
				{
					if (BaseUnits.ContainsKey(move.UnitId))
					{
						UnitBase unit = BaseUnits[move.UnitId];
						unit.Delete();
						deletedUnits.Add(unit);
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
					if (newGameCommands == null)
						newGameCommands = new List<GameCommand>();
					newGameCommands.Clear();
					newGameCommands.AddRange(GameCommands.Values);

					foreach (KeyValuePair<Position, GameCommand> kv in GameCommands)
					{
						if (kv.Value.GameCommandType == GameCommandType.Attack ||
							kv.Value.GameCommandType == GameCommandType.Defend ||
							kv.Value.GameCommandType == GameCommandType.Collect ||
							kv.Value.GameCommandType == GameCommandType.Scout)
						{
							ActiveGameCommands.Add(kv.Key, kv.Value);
						}
					}
					GameCommands.Clear();

					ProcessNewMoves();
					WaitForDraw.Set();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Type Safety", "UNT0014:Invalid type for call to GetComponent", Justification = "<Pending>")]
		public T InstantiatePrefab<T>(string name)
		{
			GameObject prefab = unitResources[name];
			GameObject instance = Instantiate(prefab);

			T script = instance.GetComponent<T>();
			return script;
		}

		public GameObject InstantiatePrefab(string name)
		{
			GameObject prefab = unitResources[name];
			GameObject instance = Instantiate(prefab);
			return instance;
		}

		public Light CreateSelectionLight(GameObject gameObject)
		{
			GameObject prefabSpotlight = GetUnitResource("Spot Light");
			GameObject instance = Instantiate(prefabSpotlight);
			Vector3 vector3 = gameObject.transform.position;
			vector3.y += 2.5f;
			instance.transform.position = vector3;
			instance.transform.SetParent(gameObject.transform);

			return instance.GetComponent<Light>();
		}

		public ParticleSystem MakeParticleSource(string resource)
		{
			GameObject gameObject = particlesResources[resource];
			ParticleSystem extractSourcePrefab = gameObject.GetComponent<ParticleSystem>();

			ParticleSystem extractSource = Instantiate(extractSourcePrefab);

			return extractSource;
		}

		public ParticleSystemForceField MakeParticleTarget()
		{
			GameObject gameObject = particlesResources["ExtractTarget"];
			ParticleSystemForceField extractPrefab = gameObject.GetComponent<ParticleSystemForceField>();
			ParticleSystemForceField extract = Instantiate(extractPrefab);
			return extract;
		}

		private List<TransitObject> tileObjectsInTransit;

		public void AddTransitTileObject(TransitObject transitObject)
		{
			if (tileObjectsInTransit == null)
				tileObjectsInTransit = new List<TransitObject>();
			tileObjectsInTransit.Add(transitObject);
		}
		public void FinishTransits()
		{
			if (tileObjectsInTransit != null)
			{
				foreach (TransitObject transitObject in tileObjectsInTransit)
				{
					if (transitObject.DestroyAtArrival)
						Destroy(transitObject.GameObject);
					else if (transitObject.HideAtArrival)
						transitObject.GameObject.SetActive(false);
				}
				tileObjectsInTransit = null;
			}
		}

        private void Update()
        {
			MoveTransits();
		}

        private void MoveTransits()
        {
			if (tileObjectsInTransit != null)
			{
				List<TransitObject> transit = new List<TransitObject>();
				transit.AddRange(tileObjectsInTransit);

				foreach (TransitObject transitObject in transit)
				{
					if (transitObject.GameObject == null)
					{
						tileObjectsInTransit.Remove(transitObject);
					}
					else
					{
						Vector3 vector3 = transitObject.TargetPosition;

						float speed = 2.0f / GameSpeed;
						float step = speed * Time.deltaTime;

						if (transitObject.ScaleDown)
						{
							MeshRenderer mesh = transitObject.GameObject.GetComponent<MeshRenderer>();
							if (mesh.bounds.size.y > 0.2f || mesh.bounds.size.x > 0.2f || mesh.bounds.size.z > 0.2f)
							{
								float scalex = mesh.bounds.size.x / 200;
								float scaley = mesh.bounds.size.y / 200;
								float scalez = mesh.bounds.size.z / 200;

								Vector3 scaleChange;
								scaleChange = new Vector3(-scalex, -scaley, -scalez);

								transitObject.GameObject.transform.localScale += scaleChange;
							}
						}
						transitObject.GameObject.transform.position = Vector3.MoveTowards(transitObject.GameObject.transform.position, vector3, step);
						if (transitObject.GameObject.transform.position == transitObject.TargetPosition)
						{
							if (transitObject.DestroyAtArrival)
							{
								Destroy(transitObject.GameObject);
							}
							else if (transitObject.HideAtArrival)
							{
								transitObject.GameObject.SetActive(false);
							}
							else
							{
								// int x=0;
							}
							tileObjectsInTransit.Remove(transitObject);
						}
					}
				}
				if (tileObjectsInTransit.Count == 0)
					tileObjectsInTransit = null;
			}
		}

		public UnitBase CreateTempUnit(Blueprint blueprint)
		{
			UnitBase unit = InstantiatePrefab<UnitBase>(blueprint.Layout);
			unit.name = blueprint.Name;
			unit.Temporary = true;
			unit.HexGrid = this;
			unit.gameObject.SetActive(false);
			unit.PlayerId = 1;

			MoveUpdateStats stats = new MoveUpdateStats();
			stats.BlueprintName = blueprint.Name;

			stats.UnitParts = new List<MoveUpdateUnitPart>();
			foreach (BlueprintPart blueprintPart in blueprint.Parts)
			{
				MoveUpdateUnitPart moveUpdateUnitPart = new MoveUpdateUnitPart();

				moveUpdateUnitPart.Name = blueprintPart.Name;
				moveUpdateUnitPart.Exists = false;
				moveUpdateUnitPart.PartType = blueprintPart.PartType;
				moveUpdateUnitPart.Level = blueprintPart.Level;
				moveUpdateUnitPart.Capacity = blueprintPart.Capacity;

				stats.UnitParts.Add(moveUpdateUnitPart);
			}
			unit.MoveUpdateStats = stats;
			unit.Assemble(false);

			return unit;
		}

		void CreateUnit(Move move)
		{
			Blueprint blueprint = game.Blueprints.FindBlueprint(move.Stats.BlueprintName);
			if (blueprint == null)
			{
				return;
			}

			UnitBase unit = InstantiatePrefab<UnitBase>(blueprint.Layout);

			unit.HexGrid = this;
			unit.CurrentPos = move.Positions[0];

			unit.PlayerId = move.PlayerId;
			unit.MoveUpdateStats = move.Stats;
			unit.UnitId = move.UnitId;
			unit.gameObject.name = move.UnitId;

			unit.Assemble(move.MoveType == MoveType.Build);
			unit.PutAtCurrentPosition(false);

			if (move.Positions.Count > 1)
			{
				// Move to targetpos
				unit.DestinationPos = move.Positions[move.Positions.Count - 1];

			}
			if (move.MoveType == MoveType.Add)
			{
				BaseUnits.Add(move.UnitId, unit);
			}
			else
			{
				Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
				if (rigidbody != null)
				{
					rigidbody.Sleep();
				}
				BaseUnits.Add(move.UnitId, unit);
				/*
				Position pos = move.Positions[move.Positions.Count - 1];
				if (UnitsInBuild.ContainsKey(pos))
				{
					// Command to build was slower than the game.
					UnitBase unitBase = UnitsInBuild[pos];
					unitBase.Delete();
					UnitsInBuild.Remove(pos);
				}
				UnitsInBuild.Add(pos, unit);*/
			}
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
			return new Vector3((x * gridSizeX), 0, -y * gridSizeY);
		}

		private GroundCell CreateCell(Position pos, MoveUpdateGroundStat groundStat, GameObject cellPrefabx)
		{
			int x = pos.X;
			int y = pos.Y;

			GameObject gameObjectCell = Instantiate(cellPrefabx);
			gameObjectCell.hideFlags = HideFlags.DontSave; //.enabled = false;	
			gameObjectCell.transform.SetParent(transform, false);
			gameObjectCell.name = "Ground " + x.ToString() + "," + y.ToString();

			GroundCell groundCell = gameObjectCell.GetComponent<GroundCell>();
			groundCell.HexGrid = this;
			groundCell.GroundStat = groundStat;
			groundCell.Pos = pos;

			Vector2 gridPos = new Vector2(x, y);
			Vector3 gridPos3 = CalcWorldPos(gridPos);

			float height = groundStat.Height;
			gridPos3.y += height + 0.3f;
			gameObjectCell.transform.localPosition = gridPos3;
			groundCell.CreateDestructables();

			return groundCell;
		}
	}
}