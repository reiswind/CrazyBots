
using Engine.Interface;

using Newtonsoft.Json;
using System;
using System.Collections;
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

        public Button CreateItems;

        internal Dictionary<ulong, GroundCell> GroundCells { get; private set; }
        internal Dictionary<string, UnitBase> BaseUnits { get; private set; }
        
        /// <summary>
        /// All human commands?
        /// </summary>
        internal List<CommandPreview> CommandPreviews { get; private set; }


        /// <summary>
        /// Filled in UI Thread and transfered on next move
        /// </summary>
        internal List<MapGameCommand> GameCommands { get; private set; }
        /// <summary>
        /// Transfer from Game to UI
        /// </summary>
        private List<MapGameCommand> newGameCommands;
        internal System.Random Random { get; private set; }

        // Shared with backgound thread
        internal IGameController game;
        private bool windowClosed;
        private List<Move> newMoves;
        internal EventWaitHandle WaitForTurn = new EventWaitHandle(false, EventResetMode.AutoReset);
        internal EventWaitHandle WaitForDraw = new EventWaitHandle(false, EventResetMode.AutoReset);
        private Thread computeMoves;

        private bool useThread;

        private string remoteGameIndex;

        private static HexGrid hexGrid;
        public static HexGrid MainGrid
        {
            get
            {
                return hexGrid;

            }
        }
        public HexGrid()
        {
            hexGrid = this;
        }

        public MapInfo MapInfo;

        internal void StartGame()
        {
            RemoveAllChildren();
            Random = new System.Random();
            if (GameSpeed == 0)
                GameSpeed = 0.01f;
            //gridCanvas = GetComponentInChildren<Canvas>();

            //UnityEngine.Object gameModelContent = Resources.Load("Models/Simple");
            //UnityEngine.Object gameModelContent = Resources.Load("Models/UnittestFight");
            //UnityEngine.Object gameModelContent = Resources.Load("Models/Unittest");
            //UnityEngine.Object gameModelContent = Resources.Load("Models/TestSingleUnit");
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

            InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);
        }


        private Dictionary<string, GameObject> allResources = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> treeResources = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> leaveTreeResources = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> rockResources = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> bushResources = new Dictionary<string, GameObject>();

        private Dictionary<string, Material> materialResources = new Dictionary<string, Material>();
        private Dictionary<string, GameObject> particlesResources = new Dictionary<string, GameObject>();

        private void InitMaterials()
        {
            materialResources.Clear();
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

            allResources.Clear();
            treeResources.Clear();
            leaveTreeResources.Clear();
            rockResources.Clear();
            bushResources.Clear();
            InitResources(allResources, "Prefabs");

            foreach (string key in allResources.Keys)
            {
                if (key.StartsWith("Tree"))
                    treeResources.Add(key, allResources[key]);
            }
            foreach (string key in allResources.Keys)
            {
                if (key.StartsWith("Leave"))
                    leaveTreeResources.Add(key, allResources[key]);
            }
            foreach (string key in allResources.Keys)
            {
                if (key.StartsWith("Rock"))
                    rockResources.Add(key, allResources[key]);
            }
            foreach (string key in allResources.Keys)
            {
                if (key.StartsWith("Bush"))
                    bushResources.Add(key, allResources[key]);
            }
        }

        public GameObject GetResource(string name)
        {
            if (allResources.ContainsKey(name))
                return allResources[name];
            return null;
        }
        public Material GetMaterial(string name)
        {
            if (materialResources.ContainsKey(name))
                return materialResources[name];
            return null;
        }
        public GameObject CreateShell(Transform transform, TileObject tileObject)
        {
            GameObject prefab;

            if (tileObject.TileObjectType == TileObjectType.Tree)
            {
                prefab = GetResource("ShellTree");
            }
            else if (tileObject.TileObjectType == TileObjectType.Mineral)
            {
                prefab = GetResource("ShellMineral");
            }
            else
            {
                prefab = GetResource("ShellMineral");
                //prefab = null;
            }
            GameObject gameTileObject = null;
            if (prefab != null)
            {
                gameTileObject = Instantiate(prefab, transform, false);
            }
            return gameTileObject;
        }

        public GameObject CreateTileObject(Transform transform, TileObject tileObject)
        {
            GameObject prefab;

            if (tileObject.TileObjectType == TileObjectType.Tree)
            {
                prefab = GetResource("ItemWood");
            }
            else if (tileObject.TileObjectType == TileObjectType.Bush)
            {
                prefab = GetResource("ItemBush");
            }
            else if (tileObject.TileObjectType == TileObjectType.Mineral)
            {
                prefab = GetResource("ItemCrystal");
            }
            else
            {
                prefab = GetResource("Marker");
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
            float x = 0;
            float y;
            float z = 0;

            Vector2 randomPos = UnityEngine.Random.insideUnitCircle;

            if (tileObject.TileObjectType == TileObjectType.Tree)
            {
                if (tileObject.TileObjectKind == TileObjectKind.LeaveTree)
                {
                    int idx = Random.Next(leaveTreeResources.Count);
                    prefab = leaveTreeResources.Values.ElementAt(idx);
                    y = prefab.transform.position.y;
                }
                else
                {
                    int idx = Random.Next(treeResources.Count);
                    prefab = treeResources.Values.ElementAt(idx);
                    y = prefab.transform.position.y;
                }
            }
            else if (tileObject.TileObjectType == TileObjectType.Bush)
            {
                int idx = Random.Next(bushResources.Count);
                prefab = bushResources.Values.ElementAt(idx);
                y = prefab.transform.position.y;
            }
            else if (tileObject.TileObjectType == TileObjectType.Rock)
            {
                int idx = Random.Next(rockResources.Count);
                prefab = rockResources.Values.ElementAt(idx);
                y = prefab.transform.position.y;
            }
            else if (tileObject.TileObjectType == TileObjectType.Gras)
            {
                if (tileObject.TileObjectKind == TileObjectKind.LightGras)
                {
                    prefab = GetResource("Gras2");
                }
                else if (tileObject.TileObjectKind == TileObjectKind.DarkGras)
                {
                    prefab = GetResource("Gras3");
                }
                else
                {
                    prefab = GetResource("Gras1");
                }
                y = 0.05f;
                x = (randomPos.x * 0.05f);
                z = (randomPos.y * 0.07f);
            }
            else if (tileObject.TileObjectType == TileObjectType.Mineral)
            {
                prefab = GetResource("ItemCrystal");
                y = 0.05f;
                x = (randomPos.x * 0.5f);
                z = (randomPos.y * 0.7f);
            }
            else if (tileObject.TileObjectType == TileObjectType.TreeTrunk)
            {
                prefab = GetResource("Trunk");
                y = prefab.transform.position.y;
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

                Vector3 unitPos3 = transform.position;

                if (tileObject.Direction == Direction.N)
                {
                    unitPos3.z += 0.5f;
                }
                else if (tileObject.Direction == Direction.NW)
                {
                    unitPos3.z += 0.3f;
                    unitPos3.x += 0.5f;
                }
                else if (tileObject.Direction == Direction.SW)
                {
                    unitPos3.z -= 0.3f;
                    unitPos3.x += 0.5f;
                }
                else if (tileObject.Direction == Direction.S)
                {
                    unitPos3.z -= 0.5f;
                }
                else if (tileObject.Direction == Direction.SE)
                {
                    unitPos3.z -= 0.3f;
                    unitPos3.x -= 0.5f;
                }
                else if (tileObject.Direction == Direction.NE)
                {
                    unitPos3.z += 0.3f;
                    unitPos3.x -= 0.5f;
                }

                float scalex = UnityEngine.Random.value / 10;
                float scaley = UnityEngine.Random.value / 10;
                float scalez = UnityEngine.Random.value / 10;

                Vector3 scaleChange;
                scaleChange = new Vector3(scalex, scaley, scalez);

                gameTileObject.transform.localScale += scaleChange;

                unitPos3.x += x;
                unitPos3.z += z;
                unitPos3.y += y;
                gameTileObject.transform.position = unitPos3;
                gameTileObject.name = tileObject.TileObjectType.ToString();
            }
            return gameTileObject;
        }

        private List<ulong> visiblePositions = new List<ulong>();
        private ulong nextVisibleCenter;
        public void UpdateVisibleCenter(ulong pos)
        {
            nextVisibleCenter = pos;
        }

#if KIILLFPS
		private Position visibleCenter;
		public IEnumerator RenderSurroundingCells()
		{
			while (true)
			{
				if (visibleCenter == nextVisibleCenter)
				{
					yield return new WaitForSeconds(0.01f);
				}
				else
				{
					// Kills FPS

					int maxActives = 0;
					bool interrupted = false;

					List<Position> positions = new List<Position>();
					positions.AddRange(visiblePositions);

					Dictionary<Position, TileWithDistance> tiles = game.Map.EnumerateTiles(nextVisibleCenter, 32, true);
					if (tiles != null)
					{
						foreach (TileWithDistance t in tiles.Values)
						{
							positions.Remove(t.Pos);
							if (!visiblePositions.Contains(t.Pos))
							{
								GroundCell groundCell;
								if (GroundCells.TryGetValue(t.Pos, out groundCell))
								{
									if (!groundCell.Visible)
									{
										maxActives++;
										groundCell.Visible = true;
										visiblePositions.Add(t.Pos);
									}
								}
							}
							if (visibleCenter != null && maxActives > 10)
							{
								interrupted = true;
								break;
							}
						}
					}
					foreach (Position pos1 in positions)
					{
						GroundCell groundCell;
						if (GroundCells.TryGetValue(pos1, out groundCell))
						{
							/*
							if (pos1.GetDistanceTo(nextVisibleCenter) > 50)
							{
								groundCell.Visible = false;
								visiblePositions.Remove(pos1);
							}*/
						}
					}
					if (interrupted)
					{
						yield return new WaitForSeconds(0.01f);
					}
					else
					{
						visibleCenter = nextVisibleCenter;
					}

				}
			}
		}
#endif
        internal void RemoveAllChildren()
        {
            while (transform.childCount > 0)
            {
                GameObject child = transform.GetChild(0).gameObject;
                DestroyImmediate(child);
            }
        }
        internal void CreateMapInEditor()
        {
            Debug.Log("CreateMapInEditor");
            RemoveAllChildren();

            UnityEngine.Object gameModelContent = Resources.Load("Models/TestInEditor");
            var serializer = new DataContractJsonSerializer(typeof(GameModel));
            GameModel gameModel;
            MemoryStream mem = new MemoryStream(Encoding.UTF8.GetBytes(gameModelContent.ToString()));
            gameModel = (GameModel)serializer.ReadObject(mem);
            InitResources();
            if (gameModel.Seed.HasValue)
            {
                game = gameModel.CreateGame(gameModel.Seed.Value);
            }
            else
            {
                game = gameModel.CreateGame();
                gameModel.Seed = game.Seed;
            }
            game.CreateUnits();

            GameCommands = new List<MapGameCommand>();
            CommandPreviews = new List<CommandPreview>();
            GroundCells = new Dictionary<ulong, GroundCell>();
            BaseUnits = new Dictionary<string, UnitBase>();
            //UnitsInBuild = new Dictionary<ulong, UnitBase>();
            hitByBullets = new List<HitByBullet>();

            GameObject cellPrefab = GetResource("HexCell");
            foreach (Tile t in game.Map.Tiles.Values)
            {
                if (!GroundCells.ContainsKey(t.Pos))
                {
                    Move move = new Move();
                    move.Stats = new MoveUpdateStats();
                    game.CollectGroundStats(t.Pos, move, null);
                    GroundCell hexCell = CreateCell(t.Pos, null, cellPrefab);
                    GroundCells.Add(t.Pos, hexCell);
                }
            }
            foreach (Engine.Master.Unit unit in game.Map.Units.List.Values)
            {
                CreateUnit(unit);
            }
            Debug.Log("Tiles done");
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


            GameCommands = new List<MapGameCommand>();
            CommandPreviews = new List<CommandPreview>();
            GroundCells = new Dictionary<ulong, GroundCell>();
            BaseUnits = new Dictionary<string, UnitBase>();
            //UnitsInBuild = new Dictionary<ulong, UnitBase>();
            hitByBullets = new List<HitByBullet>();

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

        private void SelectStartPosition()
        {
            foreach (MapZone mapZone in game.Map.Zones.Values)
            {
                if (mapZone.Player != null)
                {
                    // This is the start zone
                    UpdateVisibleCenter(mapZone.Center);

                    UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
                    foreach (GameObject gameObject in scene.GetRootGameObjects())
                    {
                        if (gameObject.name == "Strategy Camera")
                        {
                            StrategyCamera strategyCamera = gameObject.GetComponentInChildren<StrategyCamera>();

                            strategyCamera.JumpTo(this, mapZone.Center);
                        }
                    }

                    return;
                }
            }
        }

        private bool readyForNextMove;

        private string serverUrl = "https://fastfertig.net/api/";
        //private string serverUrl = "http://localhost:10148/api/";

        IEnumerator<object> StartRemoteGame(GameModel gameModel)
        {
            string body = JsonConvert.SerializeObject(gameModel);

            using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "GameEngine", body))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ProtocolError)
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

                if (GameCommands != null && GameCommands.Count > 0)
                    body = JsonConvert.SerializeObject(GameCommands);

                using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "GameMove/" + remoteGameIndex, body))
                {
                    yield return www.SendWebRequest();
                    GameCommands.Clear();
                    if (www.result == UnityWebRequest.Result.ProtocolError)
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
                        foreach (MapGameCommand gameCommand in newGameCommands)
                        {
                            if (gameCommand.GameCommandType == GameCommandType.Move)
                            {
                                UpdateMoveCommand(gameCommand);
                            }
                        }
                    }

                    long iTicks = DateTime.Now.Ticks;
                    DateTime tStart = DateTime.Now;

                    List<Move> current = game.ProcessMove(id, nextMove, newGameCommands);
                    newGameCommands = null;

                    double mstotal = (DateTime.Now - tStart).TotalMilliseconds;
                    if (mstotal > 20)
                        Debug.Log("Move Time: " + mstotal);

                    //Debug.Log("Move Time: " + (DateTime.Now.Ticks - iTicks).ToString());

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
        private List<ulong> updatedPositions = new List<ulong>();
        private List<ulong> groundcellsWithCommands = new List<ulong>();
        private bool startPositionSet = false;
        private int moveCounter;

        public void UpdateMoveCommand(MapGameCommand gameCommand)
        {
            CommandPreview commandPreview = null;

            groundcellsWithCommands.Remove(gameCommand.TargetPosition);
            GroundCell gc;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(gameCommand.TargetPosition, out gc))
            {
                commandPreview = gc.RemoveGameCommand(gameCommand);
            }
            commandPreview.GameCommand.TargetPosition = gameCommand.MoveToPosition;
            groundcellsWithCommands.Add(gameCommand.MoveToPosition);

            if (HexGrid.MainGrid.GroundCells.TryGetValue(gameCommand.MoveToPosition, out gc))
            {
                gc.UpdateMoveCommand(commandPreview);
            }
        }

        private void ProcessNewMoves()
        {
            moveCounter++;
            List<ulong> newUpdatedPositions = new List<ulong>();

            try
            {

                if (MapInfo != null)
                {
                    /*
                    foreach (ulong pos in MapInfo.Pheromones.Keys)
                    {
                        MapPheromone mapPheromone = MapInfo.Pheromones[pos];
                        GroundCell hexCell = GroundCells[pos];
                        hexCell.UpdatePheromones(mapPheromone);

                        newUpdatedPositions.Add(pos);
                        updatedPositions.Remove(pos);
                    }
                    foreach (ulong pos in updatedPositions)
                    {
                        GroundCell hexCell = GroundCells[pos];
                        hexCell.UpdatePheromones(null);
                    }*/
                    updatedPositions = newUpdatedPositions;
                    if (GroundCells.Count > 0)
                    {
                        foreach (ulong pos in groundcellsWithCommands)
                        {
                            GroundCell hexCell = GroundCells[pos];
                            hexCell.UntouchCommands();
                        }

                        foreach (MapPlayerInfo mapPlayerInfo in MapInfo.PlayerInfo.Values)
                        {
                            if (mapPlayerInfo.GameCommands != null && mapPlayerInfo.GameCommands.Count > 0)
                            {
                                foreach (MapGameCommand gameCommand in mapPlayerInfo.GameCommands)
                                {
                                    if (gameCommand.TargetPosition != Position.Null)
                                    {
                                        GroundCell hexCell = GroundCells[gameCommand.TargetPosition];
                                        CommandPreview commandPreview = hexCell.UpdateCommands(gameCommand, null);

                                        if (commandPreview != null && commandPreview.GameCommand.TargetPosition != Position.Null)
                                        {
                                            if (!groundcellsWithCommands.Contains(commandPreview.GameCommand.TargetPosition))
                                                groundcellsWithCommands.Add(commandPreview.GameCommand.TargetPosition);
                                        }
                                    }
                                }
                            }
                        }
                        List<ulong> clearedPositions = new List<ulong>();
                        foreach (ulong pos in groundcellsWithCommands)
                        {
                            GroundCell hexCell = GroundCells[pos];
                            if (hexCell.ClearCommands())
                            {
                                clearedPositions.Add(pos);
                            }
                        }
                        foreach (ulong pos in clearedPositions)
                        {
                            groundcellsWithCommands.Remove(pos);
                        }
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

            }
            catch (Exception err)
            {
                Debug.Log("FATAL in ProcessMoves. Mapinfo" + err.Message);
                throw;
            }

            try
            {
                foreach (UnitBase unitBase in BaseUnits.Values)
                {
                    if (unitBase.DestinationPos != Position.Null)
                    {
                        unitBase.CurrentPos = unitBase.DestinationPos;
                        unitBase.DestinationPos = Position.Null;
                        unitBase.PutAtCurrentPosition(true, false);
                    }
                }
                // Finish all open hits
                foreach (HitByBullet hitByBullet in hitByBullets)
                {
                    HasBeenHit(hitByBullet);
                }
                hitByBullets.Clear();
                FinishTransits();
            }
            catch (Exception err)
            {
                Debug.Log("FATAL in ProcessMoves. Finish" + err.Message);
                throw;
            }
            Move lastmove = null;
            try
            {
                foreach (Move move in newMoves)
                {
                    lastmove = move;

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
                        if (move.UnitId == null)
                        {
                            HitMove(null, move);
                        }
                        else
                        {
                            if (BaseUnits.ContainsKey(move.UnitId))
                            {
                                UnitBase unit = BaseUnits[move.UnitId];
                                HitMove(unit, move);
                            }
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
                        bool skip = false;
                        foreach (HitByBullet hitByBullet in hitByBullets)
                        {
                            if (hitByBullet.TargetPosition == move.Positions[0])
                            {
                                hitByBullet.UpdateUnitStats = move.Stats;
                                skip = true;
                            }
                        }
                        if (!skip)
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
                    }
                    else if (move.MoveType == MoveType.Move)
                    {
                        if (BaseUnits.ContainsKey(move.UnitId))
                        {
                            UnitBase unit = BaseUnits[move.UnitId];
                            unit.Direction = (Direction)move.Stats.Direction;
                            unit.MoveTo(move.Positions[1]);
                        }
                    }
                    else if (move.MoveType == MoveType.CommandComplete)
                    {
                        /*
                        if (UnitsInBuild.ContainsKey(move.Positions[0]))
                        {
                            // Remove Ghost from command
                            UnitBase unit = UnitsInBuild[move.Positions[0]];
                            if (unit != null)
                                unit.Delete();
                            GroundCell hexCell = GroundCells[move.Positions[0]];
                            UnitsInBuild.Remove(move.Positions[0]);
                        }*/
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
                        bool skip = false;
                        foreach (HitByBullet hitByBullet in hitByBullets)
                        {
                            if (hitByBullet.TargetPosition == move.Positions[0])
                            {
                                hitByBullet.UpdateGroundStats = move.Stats;
                                skip = true;
                            }
                        }
                        if (!skip)
                        {
                            GroundCell hexCell;
                            if (GroundCells.TryGetValue(move.Positions[0], out hexCell))
                            {
                                hexCell.Stats = move.Stats;
                                hexCell.Visible = move.Stats.MoveUpdateGroundStat.VisibilityMask != 0;
                                hexCell.UpdateGround();
                            }
                            else
                            {
                                GameObject cellPrefab = GetResource("HexCell");
                                hexCell = CreateCell(move.Positions[0], move.Stats, cellPrefab);
                                GroundCells.Add(move.Positions[0], hexCell);
                            }
                        }
                    }
                    else if (move.MoveType == MoveType.Delete)
                    {
                        if (BaseUnits.ContainsKey(move.UnitId))
                        {
                            UnitBase unit = BaseUnits[move.UnitId];
                            bool isInHits = false;
                            foreach (HitByBullet hitByBullet in hitByBullets)
                            {
                                if (hitByBullet.TargetUnit == unit)
                                {
                                    isInHits = true;
                                }
                            }
                            if (!isInHits)
                                unit.Delete();
                            BaseUnits.Remove(move.UnitId);
                        }
                    }
                }
                newMoves.Clear();
            }
            catch (Exception err)
            {
                Debug.Log("FATAL in ProcessMoves. " + moveCounter.ToString() + " Move " + lastmove.MoveType.ToString() + " Error_" + err.Message);
                throw;
            }

            try
            {

                if (startPositionSet == false && GroundCells.Count > 0)
                {
                    startPositionSet = true;
                    SelectStartPosition();

                    /* Debug all visible*/
                    /*
                    foreach (GroundCell groundCell1 in GroundCells.Values)
                    {
                        groundCell1.Visible = true;

                    }*/

                    //StartCoroutine(RenderSurroundingCells());
                }
                if (startPositionSet)
                {

                }
            }
            catch (Exception err)
            {
                Debug.Log("FATAL in ProcessMoves. SelectStartPosition" + err.Message);
                throw;
            }
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
                    try
                    {
                        if (newGameCommands == null)
                            newGameCommands = new List<MapGameCommand>();
                        newGameCommands.Clear();
                        if (GameCommands.Count > 0)
                        {
                            newGameCommands.AddRange(GameCommands);
                            GameCommands.Clear();
                        }
                        ProcessNewMoves();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    WaitForDraw.Set();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Type Safety", "UNT0014:Invalid type for call to GetComponent", Justification = "<Pending>")]
        public T InstantiatePrefab<T>(string name)
        {
            GameObject prefab = allResources[name];
            GameObject instance = Instantiate(prefab);

            T script = instance.GetComponent<T>();
            return script;
        }

        public GameObject InstantiatePrefab(string name)
        {
            if (!allResources.ContainsKey(name))
            {
                Debug.Log("Missing Resource: " + name);
                name = "Marker";
            }
            GameObject prefab = allResources[name];
            GameObject instance = Instantiate(prefab);
            return instance;
        }

        public Light CreateSelectionLight(GameObject gameObject)
        {
            GameObject prefabSpotlight = GetResource("Spot Light");
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
        private List<HitByBullet> hitByBullets;

        public HitByBullet Fire(UnitBase fireingUnit, TileObject ammo)
        {
            HitByBullet hitByBullet = new HitByBullet(fireingUnit.CurrentPos);
            hitByBullet.HitTime = Time.unscaledTime + 2;
            hitByBullet.Bullet = ammo;
            hitByBullets.Add(hitByBullet);
            return hitByBullet;
        }

        public void HitMove(UnitBase hitUnit, Move move)
        {
            ulong fireingPostion = move.Positions[0];
            ulong targetPostion = move.Positions[1];

            bool found = false;
            foreach (HitByBullet hitByBullet in hitByBullets)
            {
                if (hitByBullet.FireingPosition == fireingPostion && hitByBullet.TargetPosition == Position.Null)
                {
                    if (move.UnitId != null)
                    {
                        hitByBullet.TargetUnit = BaseUnits[move.UnitId];
                    }
                    if (move.OtherUnitId != null)
                    {
                        int level;
                        hitByBullet.HitPartTileObjectType = TileObject.GetTileObjectTypeFromString(move.OtherUnitId, out level);
                    }

                    hitByBullet.HitTime = Time.unscaledTime + 2;
                    hitByBullet.TargetPosition = targetPostion;

                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // First fire, than hit.
                //throw new Exception();
            }
        }

        public void HitGroundAnimation(Transform transform)
        {
            GameObject debrisDirt = GetResource("DebrisDirt");

            for (int i = 0; i < 40; i++)
            {
                GameObject debris = Instantiate(debrisDirt, transform, false);

                Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
                Vector3 unitPos3 = transform.position;
                unitPos3.x += (randomPos.x * 0.25f);
                unitPos3.z += (randomPos.y * 0.27f);
                debris.transform.position = unitPos3;
                debris.transform.rotation = UnityEngine.Random.rotation;



                /*
				Vector3 vector3 = new Vector3();
				vector3.y = 0.1f;
				vector3.x = UnityEngine.Random.value;
				vector3.z = UnityEngine.Random.value;

				Rigidbody otherRigid = debris.GetComponent<Rigidbody>();
				otherRigid.velocity = vector3;
				otherRigid.rotation = UnityEngine.Random.rotation;*/

                StartCoroutine(DelayFadeOutDebris(debris, transform.position.y - 0.1f));
                //Destroy(debris, 12 * UnityEngine.Random.value);
            }
        }

        private IEnumerator DelayFadeOutDebris(GameObject gameObject, float sinkTo)
        {
            if (gameObject != null)
            {
                yield return new WaitForSeconds(12 + 12 * (UnityEngine.Random.value));
                yield return StartCoroutine(FadeOutDebris(gameObject, sinkTo));
            }
        }
        private IEnumerator FadeOutDebris(GameObject gameObject, float sinkTo)
        {
            if (gameObject != null)
            {
                Rigidbody otherRigid = gameObject.GetComponent<Rigidbody>();
                if (otherRigid != null) otherRigid.isKinematic = true;

                while (true)
                {
                    if (gameObject.transform.position.y > sinkTo)
                    {
                        Vector3 pos = gameObject.transform.position;
                        pos.y -= 0.01f;
                        gameObject.transform.position = pos;
                        yield return new WaitForSeconds(0.01f);
                    }
                    else
                    {
                        Destroy(gameObject);
                        yield break;
                    }
                }
            }
        }

        public void HitUnitPartAnimation(Transform transform)
        {
            GameObject debrisDirt = GetResource("DebrisUnit");

            for (int i = 0; i < 40; i++)
            {
                GameObject debris = Instantiate(debrisDirt);

                Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
                Vector3 unitPos3 = transform.position;
                unitPos3.x += (randomPos.x * 0.25f);
                unitPos3.z += (randomPos.y * 0.27f);
                unitPos3.y += 1;
                debris.transform.position = unitPos3;
                debris.transform.rotation = UnityEngine.Random.rotation;


                Vector3 vector3 = new Vector3();
                vector3.y = 1f;
                vector3.x = UnityEngine.Random.value;
                vector3.z = UnityEngine.Random.value;

                Rigidbody otherRigid = debris.GetComponent<Rigidbody>();
                otherRigid.velocity = vector3;
                otherRigid.rotation = UnityEngine.Random.rotation;

                StartCoroutine(DelayFadeOutDebris(debris, debris.transform.position.y - 0.1f));
            }
        }

        public void HasBeenHit(HitByBullet hitByBullet)
        {
            if (hitByBullet.TargetUnit == null)
            {
                if (hitByBullet.UpdateGroundStats != null)
                {
                    GroundCell hexCell;

                    if (GroundCells.TryGetValue(hitByBullet.TargetPosition, out hexCell))
                    {
                        if (hexCell.Visible)
                        {
                            if (hitByBullet.UpdateGroundStats.MoveUpdateGroundStat != null)
                            {
                                hexCell.Stats = hitByBullet.UpdateGroundStats;
                                hexCell.UpdateGround();
                            }

                            Destroy(hexCell.gameObject);
                            GroundCells.Remove(hitByBullet.TargetPosition);

                            GameObject cellPrefab = GetResource("HexCellCrate");
                            hexCell = CreateCell(hexCell.Pos, hexCell.Stats, cellPrefab);
                            GroundCells.Add(hitByBullet.TargetPosition, hexCell);

                            HitGroundAnimation(hexCell.transform);
                        }
                    }
                }
            }
            else
            {
                hitByBullet.TargetUnit.HitByShell();

                UnitBasePart unitBasePart = hitByBullet.TargetUnit.PartHitByShell(hitByBullet.HitPartTileObjectType, hitByBullet.UpdateUnitStats);
                if (unitBasePart != null)
                    HitUnitPartAnimation(unitBasePart.UnitBase.transform);
            }
        }

        public void HandleImpacts()
        {
            if (hitByBullets != null)
            {
                List<HitByBullet> currentHitByBullets = new List<HitByBullet>();
                currentHitByBullets.AddRange(hitByBullets);
                foreach (HitByBullet hitByBullet in currentHitByBullets)
                {
                    if (hitByBullet.BulletImpact)
                    {
                        HasBeenHit(hitByBullet);
                        hitByBullets.Remove(hitByBullet);
                    }
                }
            }
        }

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
                    FinishTransit(transitObject);
                }
                tileObjectsInTransit = null;
            }
        }
        public void FinishTransit(TransitObject transitObject)
        {
            if (transitObject.DestroyAtArrival)
                Destroy(transitObject.GameObject);
            else if (transitObject.HideAtArrival)
                transitObject.GameObject.SetActive(false);

            if (transitObject.ActivateAtArrival != null)
                transitObject.ActivateAtArrival.SetActive(true);
        }

        private void Update()
        {
            HandleImpacts();
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
                            if (mesh != null)
                            {
                                // if larger
                                if (mesh.bounds.size.y > 0.2f || mesh.bounds.size.x > 0.2f || mesh.bounds.size.z > 0.2f)
                                {
                                    // but not to small
                                    if (mesh.bounds.size.y > 0.1f && mesh.bounds.size.x > 0.1f && mesh.bounds.size.z > 0.1f)
                                    {
                                        float scalex = mesh.bounds.size.x / 200;
                                        float scaley = mesh.bounds.size.y / 200;
                                        float scalez = mesh.bounds.size.z / 200;

                                        Vector3 scaleChange;
                                        scaleChange = new Vector3(-scalex, -scaley, -scalez);

                                        transitObject.GameObject.transform.localScale += scaleChange;
                                    }
                                }
                            }
                        }
                        transitObject.GameObject.transform.position = Vector3.MoveTowards(transitObject.GameObject.transform.position, vector3, step);
                        if (transitObject.GameObject.transform.position == transitObject.TargetPosition)
                        {
                            FinishTransit(transitObject);
                            /*
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
							*/
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
                moveUpdateUnitPart.CompleteLevel = blueprintPart.Level;
                moveUpdateUnitPart.Capacity = blueprintPart.Capacity;

                stats.UnitParts.Add(moveUpdateUnitPart);
            }
            unit.MoveUpdateStats = stats;
            unit.Assemble(true, true);

            return unit;
        }

        void CreateUnit(Engine.Master.Unit masterunit)
        {
            Blueprint blueprint = game.Blueprints.FindBlueprint(masterunit.Blueprint.Name);
            if (blueprint == null)
            {
                return;
            }
            UnitBase unit = InstantiatePrefab<UnitBase>(blueprint.Layout);

            unit.CurrentPos = masterunit.Pos;

            unit.PlayerId = masterunit.Owner.PlayerModel.Id;
            unit.MoveUpdateStats = masterunit.CollectStats();
            if (masterunit.UnderConstruction)
            {
                foreach (MoveUpdateUnitPart moveUpdateUnitPart in unit.MoveUpdateStats.UnitParts)
                {
                    moveUpdateUnitPart.Exists = false;
                }
            }

            unit.UnitId = masterunit.UnitId;
            unit.gameObject.name = masterunit.UnitId;

            unit.Assemble(masterunit.UnderConstruction, masterunit.UnderConstruction);
            unit.PutAtCurrentPosition(false, true);

            Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.Sleep();
            }
            BaseUnits.Add(masterunit.UnitId, unit);
        }

        void CreateUnit(Move move)
        {
            Blueprint blueprint = game.Blueprints.FindBlueprint(move.Stats.BlueprintName);
            if (blueprint == null)
            {
                return;
            }
            UnitBase unit = InstantiatePrefab<UnitBase>(blueprint.Layout);
            if (unit == null) return;

            unit.CurrentPos = move.Positions[0];
            unit.Direction = (Direction)move.Stats.Direction;
            unit.PlayerId = move.PlayerId;
            unit.MoveUpdateStats = move.Stats;
            unit.UnitId = move.UnitId;
            unit.gameObject.name = move.UnitId;

            unit.Assemble(move.MoveType == MoveType.Build);
            unit.PutAtCurrentPosition(false, true);

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
                UnitBase.DeactivateRigidbody(unit.gameObject);
                BaseUnits.Add(move.UnitId, unit);
            }
        }

        private Vector3 CalcWorldPos(GroundCell groundCell)
        {
            float gridSizeX = 1.50f;
            float gridSizeY = 1.75f;
            float halfGridSize = 0.86f;

            int x = Position.GetX(groundCell.Pos);
            int y = Position.GetY(groundCell.Pos);

            if ((x & 1) == 0)
            {
                return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
            }
            return new Vector3((x * gridSizeX), 0, -y * gridSizeY);
        }

        public CommandPreview FindCommandForUnit(UnitBase unitBase)
        {
            foreach (CommandPreview mapGameCommand in CommandPreviews)
            {
                foreach (MapGameCommandItem mapGameCommandItem in mapGameCommand.GameCommand.GameCommandItems)
                {
                    if (mapGameCommandItem.AttachedUnitId == unitBase.UnitId)
                        return mapGameCommand;
                }
            }
            return null;
        }

        private GroundCell CreateCell(ulong pos, MoveUpdateStats stats, GameObject cellPrefabx)
        {
            int x = Position.GetX(pos);
            int y = Position.GetY(pos);

            GameObject gameObjectCell = Instantiate(cellPrefabx);
            gameObjectCell.hideFlags = HideFlags.DontSave; //.enabled = false;	
            gameObjectCell.transform.SetParent(transform, false);
            gameObjectCell.name = "Ground " + x.ToString() + "," + y.ToString();

            GroundCell groundCell = gameObjectCell.GetComponent<GroundCell>();
            groundCell.Stats = stats;
            groundCell.Pos = pos;

            Vector3 gridPos3 = CalcWorldPos(groundCell);

            if (stats == null)
            {
                groundCell.Visible = true;
                gridPos3.y += 0.3f;
                gameObjectCell.transform.localPosition = gridPos3;
            }
            else
            {
                groundCell.Visible = stats.MoveUpdateGroundStat.VisibilityMask != 0;

                float height = stats.MoveUpdateGroundStat.Height;
                gridPos3.y += height + 0.3f;
                gameObjectCell.transform.localPosition = gridPos3;
                groundCell.CreateDestructables();
            }
            return groundCell;
        }
    }
}