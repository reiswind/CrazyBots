
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
        [Header("Unit Settings")]
        public GameObject UnitStunned;

        [Header("Reactor Settings")]
        public GameObject ReactorBurnMineral;
        public GameObject ReactorBurnWood;

        [Header("Explosion Settings")]
        public GameObject UnitHitByMineral1;
        public GameObject UnitHitByMineral2;
        public GameObject GroundHit;

        [Header("Game Settings")]
        public float GameSpeed = 0.01f;

        internal float hexCellHeight = 0.0f;

        internal Dictionary<Position2, GroundCell> GroundCells { get; private set; }
        internal Dictionary<string, UnitBase> BaseUnits { get; private set; }

        /// <summary>
        /// All human commands?
        /// </summary>
        internal Dictionary<int,CommandPreview> CommandPreviews { get; private set; }
        internal List<CommandPreview> CreatedCommandPreviews { get; private set; }

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

        internal bool ShowDebuginfo = true;

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
            UnityEngine.Object gameModelContent = Resources.Load("Models/TestSingleUnit");
            //UnityEngine.Object gameModelContent = Resources.Load("Models/TestShoot");
            //UnityEngine.Object gameModelContent = Resources.Load("Models/TestDelivery");
            //UnityEngine.Object gameModelContent = Resources.Load("Models/Test");


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
                gameModel.Players = new List<PlayerModel>();

                PlayerModel p = new PlayerModel();
                p.ControlLevel = 1;
                p.Id = 1;
                p.Name = "WebPLayer";
                gameModel.Players.Add(p);
            }
            CreateGame(gameModel);

            InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);
        }

        public void RunFaster()
        {
            if (GameSpeed > 0.1f)
            {
                CancelInvoke();
                GameSpeed -= 0.1f;
                IsPause = false;
                InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);

                Debug.Log("Gamespeed set to " + GameSpeed);
            }
            else
            {
                GameSpeed = 0.01f;
            }
        }
        public bool IsPause { get; private set; }
        public void Pause()
        {
            if (IsPause)
            {
                Debug.Log("Resume");
                IsPause = false;
                InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);
            }
            else            
            {
                Debug.Log("Pause");
                IsPause = true;
                CancelInvoke();
            }
        }
        public void RunSlower()
        {
            if (GameSpeed < 10)
            {
                CancelInvoke();
                GameSpeed += 0.1f;
                IsPause = false;
                InvokeRepeating(nameof(invoke), 0.5f, GameSpeed);
                Debug.Log("Gamespeed set to " + GameSpeed);
            }
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
            particlesResources.Clear();
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
                if (key.StartsWith("xTree"))
                    treeResources.Add(key, allResources[key]);
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

            if (tileObject.TileObjectType == TileObjectType.Wood)
            {
                prefab = GetResource("ShellWood");
            }
            else if (tileObject.TileObjectType == TileObjectType.Stone)
            {
                prefab = GetResource("ShellStone");
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

            if (tileObject.TileObjectType == TileObjectType.Wood)
            {
                prefab = GetResource("ItemWood");
            }
            else if (tileObject.TileObjectType == TileObjectType.Stone)
            {
                prefab = GetResource("ItemStone");
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

        public GameObject CreateDestructable(Transform transform, TileObject tileObject, CollectionType collectionType)
        {
            GameObject prefab;
            float x = 0;
            float y = 0;
            float z = 0;

            bool scale = false;
            Vector2 randomPos = UnityEngine.Random.insideUnitCircle;

            if (tileObject.TileObjectType == TileObjectType.Tree)
            {
                scale = true;
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
                scale = true;
                int idx = Random.Next(bushResources.Count);
                prefab = bushResources.Values.ElementAt(idx);
                y = prefab.transform.position.y;
            }
            else if (tileObject.TileObjectType == TileObjectType.Wood)
            {
                prefab = GetResource("ItemWood");
            }
            else if (tileObject.TileObjectType == TileObjectType.Stone)
            {
                if (collectionType == CollectionType.Block)
                {
                    scale = true;
                    int idx = Random.Next(rockResources.Count);
                    prefab = rockResources.Values.ElementAt(idx);
                    y = prefab.transform.position.y;
                }
                else if (collectionType == CollectionType.Many)
                {
                    prefab = GetResource("ItemStoneLarge");
                }

                else
                {
                    prefab = GetResource("ItemStone");
                }
                y = 0.05f;
                x = (randomPos.x * 0.5f);
                z = (randomPos.y * 0.7f);
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
                y = 0.02f;
                //x = (randomPos.x * 0.05f);
                //z = (randomPos.y * 0.07f);
            }
            else if (tileObject.TileObjectType == TileObjectType.Mineral)
            {
                if (collectionType == CollectionType.Block)
                {
                    prefab = GetResource("ItemCrystalBlock");
                }
                else if (collectionType == CollectionType.Many)
                {
                    prefab = GetResource("ItemCrystalLarge");
                }
                else
                {
                    prefab = GetResource("ItemCrystal");
                }
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

                if (scale)
                {
                    float scalex = UnityEngine.Random.value / 10;
                    float scaley = UnityEngine.Random.value / 10;
                    float scalez = UnityEngine.Random.value / 10;

                    Vector3 scaleChange;
                    scaleChange = new Vector3(scalex, scaley, scalez);
                    gameTileObject.transform.localScale += scaleChange;
                }
                unitPos3.x += x;
                unitPos3.z += z;
                unitPos3.y += y;
                gameTileObject.transform.position = unitPos3;

                gameTileObject.name = tileObject.TileObjectType.ToString() + tileObject.Direction.ToString();
            }
            return gameTileObject;
        }

        private List<Position2> visiblePositions = new List<Position2>();
        private Position2 nextVisibleCenter;
        public void UpdateVisibleCenter(Position2 pos)
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
            CommandPreviews = new Dictionary<int, CommandPreview>();
            CreatedCommandPreviews = new List<CommandPreview>();

            GroundCells = new Dictionary<Position2, GroundCell>();
            BaseUnits = new Dictionary<string, UnitBase>();
            hitByBullets = new List<HitByBullet>();

            GameObject cellPrefab = GetResource("HexCell");
            foreach (Engine.Master.Tile t in game.Map.Tiles.Values)
            {
                if (!GroundCells.ContainsKey(t.Pos))
                {
                    Move move = new Move();
                    move.Stats = new MoveUpdateStats();
                    game.CollectGroundStats(t.Pos, move);
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
            CommandPreviews = new Dictionary<int, CommandPreview>();
            CreatedCommandPreviews = new List<CommandPreview>();
            GroundCells = new Dictionary<Position2, GroundCell>();
            BaseUnits = new Dictionary<string, UnitBase>();
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

                var request = new UnityWebRequest(serverUrl + "GameMove/" + remoteGameIndex, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();

                //using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "GameMove/" + remoteGameIndex, body))
                {
                    //yield return www.SendWebRequest();
                    GameCommands.Clear();
                    if (request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.Log(request.error);
                    }
                    else
                    {
                        string movesJson = request.downloadHandler.text;
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
                                //UpdateMoveCommand(gameCommand);
                            }
                        }
                    }

                   
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
        private List<Position2> updatedPositions = new List<Position2>();
        private List<GroundCellBorder> groundCellBorders = new List<GroundCellBorder>();

        private bool startPositionSet = false;
        private int moveCounter;
        public int MoveCounter
        {
            get
            {
                return moveCounter;
            }
        }

        private void ProcessNewMoves()
        {
            moveCounter++;
            if (moveCounter == 112)
            {

            }
            List<Position2> newUpdatedPositions = new List<Position2>();

            try
            {

                if (MapInfo != null)
                {
                    
                    foreach (Position2 pos in MapInfo.Pheromones.Keys)
                    {
                        MapPheromone mapPheromone = MapInfo.Pheromones[pos];
                        GroundCell hexCell = GroundCells[pos];
                        hexCell.UpdatePheromones(mapPheromone);

                        newUpdatedPositions.Add(pos);
                        updatedPositions.Remove(pos);
                    }
                    foreach (Position2 pos in updatedPositions)
                    {
                        GroundCell hexCell = GroundCells[pos];
                        hexCell.UpdatePheromones(null);
                    }
                    updatedPositions = newUpdatedPositions;

                    /*
                    if (GroundCells.Count > 0)
                    {
                        foreach (Position2 pos in groundcellsWithCommands)
                        {
                            GroundCell hexCell = GroundCells[pos];
                            hexCell.UntouchCommands();
                        }

                        foreach (MapPlayerInfo mapPlayerInfo in MapInfo.PlayerInfo.Values)
                        {
                            // Only for current player
                            if (!ShowDebuginfo)
                            {
                                if (mapPlayerInfo.PlayerId != 1)
                                    continue;
                            }
                            if (mapPlayerInfo.GameCommands != null && mapPlayerInfo.GameCommands.Count > 0)
                            {
                                foreach (MapGameCommand gameCommand in mapPlayerInfo.GameCommands)
                                {
                                    if (gameCommand.TargetPosition != Position2.Null)
                                    {
                                        GroundCell hexCell = GroundCells[gameCommand.TargetPosition];
                                        CommandPreview commandPreview = hexCell.UpdateCommands(gameCommand, null);

                                        if (commandPreview != null && commandPreview.GameCommand.TargetPosition != Position2.Null)
                                        {
                                            if (!groundcellsWithCommands.Contains(commandPreview.GameCommand.TargetPosition))
                                                groundcellsWithCommands.Add(commandPreview.GameCommand.TargetPosition);
                                        }
                                    }
                                }
                            }
                        }
                        List<Position2> clearedPositions = new List<Position2>();
                        foreach (Position2 pos in groundcellsWithCommands)
                        {
                            GroundCell hexCell = GroundCells[pos];
                            if (hexCell.ClearCommands())
                            {
                                clearedPositions.Add(pos);
                            }
                        }
                        foreach (Position2 pos in clearedPositions)
                        {
                            groundcellsWithCommands.Remove(pos);
                        }
                    }
                    */

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
                    if (unitBase.DestinationPos != Position2.Null)
                    {
                        unitBase.CurrentPos = unitBase.DestinationPos;
                        unitBase.DestinationPos = Position2.Null;
                    }
                    unitBase.Direction = unitBase.TurnIntoDirection;

                    unitBase.PutAtCurrentPosition(true, false);
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
                List<Position2> groundCellBorderChanged = new List<Position2>();

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
                        HitMove(move);
                    }
                    else if (move.MoveType == MoveType.Burn)
                    {
                        if (BaseUnits.ContainsKey(move.UnitId))
                        {
                            UnitBase unit = BaseUnits[move.UnitId];
                            unit.BurnMove(move);
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
                                int countBefore = CountItems(unit.MoveUpdateStats);
                                int countAfter = CountItems(move.Stats);
                                if (countBefore < countAfter)
                                {
                                    // The unit has more items than before. Update the stats at the begining of the next move, so that
                                    // the animation has time to complete and the new items appear at the end of the animation
                                    HitByBullet hitByBullet = new HitByBullet(move.Positions[0]);
                                    hitByBullet.UpdateUnitStats = move.Stats;
                                    hitByBullet.TargetUnit = unit;
                                    hitByBullet.UpdateStats = true;
                                    hitByBullets.Add(hitByBullet);
                                }
                                else
                                {
                                    unit.UpdateStats(move.Stats);
                                }
                            }
                        }
                    }
                    else if (move.MoveType == MoveType.Move)
                    {
                        if (BaseUnits.ContainsKey(move.UnitId))
                        {
                            UnitBase unit = BaseUnits[move.UnitId];
                            if (move.Positions.Count == 1)
                            {
                                unit.TurnTo(move.Stats.Direction);
                            }
                            else if (move.Positions.Count > 1)
                            {
                                unit.TurnTo(move.Stats.Direction);
                                //unit.Direction = move.Stats.Direction;
                                unit.MoveTo(move.Positions[1]);
                            }
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
                    else if (move.MoveType == MoveType.Command)
                    {
                        CommandMove(move);
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
                                if (!useThread)
                                {
                                    hexCell.Visible = true;
                                    hexCell.VisibleByPlayer = true;
                                }
                                else
                                {
                                    if (HexGrid.MainGrid.ShowDebuginfo)
                                    {
                                        hexCell.Visible = move.Stats.MoveUpdateGroundStat.VisibilityMask != 0;
                                        hexCell.VisibleByPlayer = (move.Stats.MoveUpdateGroundStat.VisibilityMask & 1) != 0;
                                    }
                                    else
                                    {
                                        hexCell.Visible = (move.Stats.MoveUpdateGroundStat.VisibilityMask & 1) != 0;
                                        hexCell.VisibleByPlayer = (move.Stats.MoveUpdateGroundStat.VisibilityMask & 1) != 0;
                                    }
                                }
                                //Debug show all
                                //hexCell.Visible = true;
                                //hexCell.VisibleByPlayer = true;
                                
                                Position2 borderPos = hexCell.UpdateGround(move.Stats);
                                if (borderPos != Position2.Null)
                                    groundCellBorderChanged.Add(borderPos);
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
                                    hitByBullet.Deleted = true;
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
                //groundCellBorderChanged.Clear();
                //groundCellBorderChanged.AddRange(GroundCells.Keys);
                GroundCell.CreateBorderLines(groundCellBorders, groundCellBorderChanged);
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
                if (WaitForTurn.WaitOne(100))
                {
                    try
                    {

                        DateTime tStart = DateTime.Now;

                        ProcessNewMoves();

                        if (newGameCommands == null)
                            newGameCommands = new List<MapGameCommand>();
                        newGameCommands.Clear();
                        if (GameCommands.Count > 0)
                        {
                            Debug.Log("TransferCommands");

                            newGameCommands.AddRange(GameCommands);
                            GameCommands.Clear();
                        }

                        double mstotal = (DateTime.Now - tStart).TotalMilliseconds;
                        if (mstotal > 20)
                            Debug.Log("ProcessNewMoves: " + mstotal);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    WaitForDraw.Set();
                }
            }
        }

        private int CountItems(MoveUpdateStats moveUpdateStats)
        {
            if (moveUpdateStats == null || moveUpdateStats.UnitParts == null)
                return 0;
            int count = 0;
            foreach (MoveUpdateUnitPart moveUpdateUnitPart in moveUpdateStats.UnitParts)
            {
                if (moveUpdateUnitPart.TileObjects != null)
                {
                    count += moveUpdateUnitPart.TileObjects.Count;
                }
            }
            return count;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Type Safety", "UNT0014:Invalid type for call to GetComponent", Justification = "<Pending>")]
        public T InstantiatePrefab<T>(string name)
        {
            GameObject prefab = allResources[name];
            GameObject instance = Instantiate(prefab, transform);

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

        private List<TransitObject> tileObjectsInTransit = new List<TransitObject>();
        private List<HitByBullet> hitByBullets;

        public void FadeOutGameObject(GameObject gameObject, Vector3 targetDirection, float speed)
        {
            TransitObject transitObject = new TransitObject();
            transitObject.DestroyAtArrival = true;
            transitObject.GameObject = gameObject;
            transitObject.TargetDirection = targetDirection;
            transitObject.Speed = speed;

            transitObject.StartAfterThis = Time.time + (0.6f * HexGrid.MainGrid.GameSpeed);
            transitObject.EndAfterThis = Time.time + (2.5f * HexGrid.MainGrid.GameSpeed);

            tileObjectsInTransit.Add(transitObject);
        }

        public HitByBullet Fire(UnitBase fireingUnit, TileObject ammo)
        {
            HitByBullet hitByBullet = new HitByBullet(fireingUnit.CurrentPos);
            hitByBullet.HitTime = Time.unscaledTime + 2;
            hitByBullet.Bullet = ammo;
            hitByBullets.Add(hitByBullet);
            return hitByBullet;
        }
        public void CommandMove(Move move)
        {
            Debug.Log("Command " + move.Command.TargetPosition.ToString());
            MapGameCommand gameCommand = move.Command;

            if (gameCommand.CommandCanceled || gameCommand.CommandComplete)
            {
                CommandPreview commandPreview;
                if (CommandPreviews.TryGetValue(gameCommand.CommandId, out commandPreview))
                {
                    commandPreview.Delete();
                    CommandPreviews.Remove(gameCommand.CommandId);
                }
            }
            else
            {
                CommandPreview commandPreview;
                if (!CommandPreviews.TryGetValue(gameCommand.CommandId, out commandPreview))
                {
                    foreach (CommandPreview existingPreview in CreatedCommandPreviews)
                    {
                        if (existingPreview.GameCommand.TargetPosition == gameCommand.TargetPosition &&
                            existingPreview.GameCommand.PlayerId == gameCommand.PlayerId &&
                            existingPreview.GameCommand.GameCommandType == gameCommand.GameCommandType)
                        {
                            commandPreview = existingPreview;
                            CommandPreviews.Add(gameCommand.CommandId, commandPreview);
                            CreatedCommandPreviews.Remove(existingPreview);
                            break;
                        }
                    }
                }

                if (commandPreview == null)
                {
                    // New (form other player)
                    commandPreview = new CommandPreview();

                    commandPreview.CreateCommandPreview(gameCommand);
                    commandPreview.IsPreview = false;
                    commandPreview.SetActive(false);
                    if (gameCommand.TargetPosition != Position2.Null)
                        commandPreview.UpdatePositions(GroundCells[gameCommand.TargetPosition]);

                    CommandPreviews.Add(gameCommand.CommandId, commandPreview);
                }
                if (gameCommand.TargetPosition != Position2.Null)
                {
                    if (commandPreview.UpdateCommandPreview(gameCommand))
                    {
                        commandPreview.SetPosition(GroundCells[gameCommand.TargetPosition]);
                    }
                }
            }
        }
        public void HitMove(Move move)
        {
            Position2 fireingPostion = move.Positions[0];
            Position2 targetPostion = move.Positions[1];

            bool found = false;
            foreach (HitByBullet hitByBullet in hitByBullets)
            {
                if (hitByBullet.FireingPosition == fireingPostion && hitByBullet.TargetPosition == Position2.Null)
                {
                    if (move.UnitId != null)
                    {
                        hitByBullet.TargetUnit = BaseUnits[move.UnitId];
                    }
                    if (move.OtherUnitId != null)
                    {
                        if (move.OtherUnitId == "Shield")
                        {
                            hitByBullet.ShieldHit = true;
                        }
                        else
                        { 
                            int level;
                            hitByBullet.HitPartTileObjectType = TileObject.GetTileObjectTypeFromString(move.OtherUnitId, out level);
                        }
                    }

                    hitByBullet.HitTime = Time.unscaledTime + 2;
                    hitByBullet.TargetPosition = targetPostion;
                    hitByBullet.UpdateUnitStats = move.Stats;

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
            int mask = LayerMask.GetMask("Units");

            List<UnitBase> hitOtherUnits = new List<UnitBase>();

            Vector3 explosionPos = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPos, 2, mask);
            foreach (Collider hit in colliders)
            {
                UnitBase unitBase = UnitBase.GetUnitFrameColilder(hit);
                if (unitBase != null && !hitOtherUnits.Contains(unitBase))
                    hitOtherUnits.Add(unitBase);
            }
            foreach (UnitBase unit in hitOtherUnits)
            {
                GameObject stun = HexGrid.Instantiate<GameObject>(HexGrid.MainGrid.UnitStunned, unit.transform);
                HexGrid.Destroy(stun, 1);

                // No real good effect
                Rigidbody rb = unit.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.AddExplosionForce(70, explosionPos, 2, 3.0F);
            }

            GameObject animation = HexGrid.Instantiate<GameObject>(HexGrid.MainGrid.GroundHit, transform);
            HexGrid.Destroy(animation, 1.5f);

            GameObject debrisDirt = GetResource("DebrisUnit");

            for (int i = 0; i < 30; i++)
            {
                GameObject debris = Instantiate(debrisDirt);

                Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
                Vector3 unitPos3 = transform.position;
                unitPos3.x += (randomPos.x * 0.25f);
                unitPos3.z += (randomPos.y * 0.27f);
                unitPos3.y += 1;
                debris.transform.SetParent(transform);
                debris.transform.position = unitPos3;
                debris.transform.rotation = UnityEngine.Random.rotation;

                Vector3 vector3 = transform.position;
                vector3.y -= 0.2f;
                Rigidbody otherRigid = debris.GetComponent<Rigidbody>();
                otherRigid.AddExplosionForce(3.5f, vector3, 1);

                FadeOutGameObject(debris, Vector3.down, 0.1f);
            }
        }

        public void HitUnitPartAnimation(Transform transform)
        {
            GameObject debrisDirt = GetResource("DebrisUnit");
            for (int i = 0; i < 30; i++)
            {
                GameObject debris = Instantiate(debrisDirt);

                Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
                Vector3 unitPos3 = transform.position;
                unitPos3.x += (randomPos.x * 0.25f);
                unitPos3.z += (randomPos.y * 0.27f);
                unitPos3.y += 1;
                debris.transform.SetParent(transform);
                debris.transform.position = unitPos3;
                debris.transform.rotation = UnityEngine.Random.rotation;

                Vector3 vector3 = transform.position;
                vector3.y -= 0.2f;
                Rigidbody otherRigid = debris.GetComponent<Rigidbody>();
                otherRigid.AddExplosionForce(3.5f, vector3, 1);

                FadeOutGameObject(debris, Vector3.down, 0.1f);
            }
        }

        public void HasBeenHit(HitByBullet hitByBullet)
        {
            if (hitByBullet.BulletImpact == false)
            {
                //Debug.Log("No IMPACT!");
            }
            GroundCell hexCell = null;
            if (hitByBullet.UpdateGroundStats != null)
            {
                if (GroundCells.TryGetValue(hitByBullet.TargetPosition, out hexCell))
                {
                    if (hexCell.Visible)
                    {
                        if (hitByBullet.UpdateGroundStats.MoveUpdateGroundStat != null)
                        {
                            hexCell.UpdateGround(hitByBullet.UpdateGroundStats);
                        }
                    }
                }
            }
            if (hitByBullet.TargetUnit == null)
            {
                if (hexCell != null)
                {
                    // No replace...
                    //Destroy(hexCell.gameObject);
                    //GroundCells.Remove(hitByBullet.TargetPosition);
                    //GameObject cellPrefab = GetResource("HexCellCrate");
                    //hexCell = CreateCell(hexCell.Pos, hexCell.Stats, cellPrefab);
                    //GroundCells.Add(hitByBullet.TargetPosition, hexCell);

                    HitGroundAnimation(hexCell.transform);
                }
            }
            else
            {
                if (hitByBullet.UpdateStats)
                {
                    hitByBullet.TargetUnit.UpdateStats(hitByBullet.UpdateUnitStats);
                }
                else
                {
                    hitByBullet.TargetUnit.HitByShell();

                    if (hitByBullet.ShieldHit)
                    {
                        hitByBullet.TargetUnit.UpdateStats(hitByBullet.UpdateUnitStats);
                    }
                    else
                    {
                        UnitBasePart unitBasePart = hitByBullet.TargetUnit.PartHitByShell(hitByBullet.HitPartTileObjectType, hitByBullet.UpdateUnitStats);
                        hitByBullet.TargetUnit.UpdateStats(hitByBullet.UpdateUnitStats);

                        if (hitByBullet.Deleted)
                        {
                            hitByBullet.TargetUnit.HasBeenDestroyed = true;
                            Destroy(hitByBullet.TargetUnit.gameObject, 10);
                        }
                    }
                }
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
            tileObjectsInTransit.Add(transitObject);
        }
        public void FinishTransits()
        {
            List<TransitObject> finishedTransits = new List<TransitObject>();
            foreach (TransitObject transitObject in tileObjectsInTransit)
            {
                if (FinishTransit(transitObject))
                {
                    finishedTransits.Add(transitObject);
                }
            }
            foreach (TransitObject transitObject1 in finishedTransits)
                tileObjectsInTransit.Remove(transitObject1);
        }

        public bool FinishTransit(TransitObject transitObject)
        {
            if (transitObject.EndAfterThis.HasValue && !transitObject.TargetReached)
                return false;

            if (transitObject.DestroyAtArrival)
                Destroy(transitObject.GameObject);
            else if (transitObject.HideAtArrival)
                transitObject.GameObject.SetActive(false);

            if (transitObject.ActivateAtArrival != null)
                transitObject.ActivateAtArrival.SetActive(true);

            return true;
        }

        private void Update()
        {
            HandleImpacts();
            MoveTransits();
        }

        private void MoveTransits()
        {
            foreach (TransitObject transitObject in tileObjectsInTransit)
            {
                if (transitObject.EndAfterThis.HasValue)
                {
                    if (Time.time > transitObject.EndAfterThis)
                    {
                        transitObject.TargetReached = true;
                    }
                }

                if (transitObject.StartAfterThis.HasValue)
                {
                    if (Time.time < transitObject.StartAfterThis)
                    {
                        //Debug.Log("Skip Transit " + Time.time + " < " + transitObject.StartAfterThis);

                        continue;
                    }
                    //Debug.Log("Transit " + Time.time);
                }
                if (!transitObject.RigidBodyDeactivated)
                {
                    if (transitObject.GameObject != null)
                    {
                        Rigidbody otherRigid = transitObject.GameObject.GetComponent<Rigidbody>();
                        if (otherRigid != null) otherRigid.isKinematic = true;

                        if (transitObject.TargetDirection.HasValue)
                        {
                            Vector3 vector3 = transitObject.GameObject.transform.position + transitObject.TargetDirection.Value;
                            transitObject.TargetPosition = vector3;
                        }
                    }
                    transitObject.RigidBodyDeactivated = true;
                }

                if (transitObject.GameObject == null)
                {
                    transitObject.TargetReached = true;
                }
                else
                {
                    Vector3 vector3 = transitObject.TargetPosition;

                    float speed = transitObject.Speed / GameSpeed;
                    float step = speed * Time.deltaTime;

                    if (transitObject.ScaleUp)
                    {
                        Vector3 scaleChange = transitObject.GameObject.transform.localScale;
                        if (scaleChange.x < 1)
                            scaleChange.x *= 1.05f;
                        if (scaleChange.y < 1)
                            scaleChange.y *= 1.05f;
                        if (scaleChange.z < 1)
                            scaleChange.z *= 1.05f;
                        transitObject.GameObject.transform.localScale = scaleChange;
                    }
                    if (transitObject.ScaleDown)
                    {
                        Vector3 scaleChange = transitObject.GameObject.transform.localScale;
                        if (scaleChange.x > 0.1f)
                            scaleChange.x *= 0.98f;
                        if (scaleChange.y > 0.1f)
                            scaleChange.y *= 0.98f;
                        if (scaleChange.z > 0.1f)
                            scaleChange.z *= 0.98f;
                        transitObject.GameObject.transform.localScale = scaleChange;
                    }
                    if (false && transitObject.ScaleDown)
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
                }
            }
        }

        public UnitBase CreateTempUnit(Blueprint blueprint, int playerId)
        {
            UnitBase unit = InstantiatePrefab<UnitBase>(blueprint.Layout);
            unit.PlayerId = playerId;
            unit.name = blueprint.Name;
            unit.Temporary = true;
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
                moveUpdateUnitPart.Range = blueprintPart.Range;
                moveUpdateUnitPart.Capacity = blueprintPart.Capacity;

                stats.UnitParts.Add(moveUpdateUnitPart);
            }
            unit.MoveUpdateStats = stats;
            //StartCoroutine(AnimateAssembleGhost(unit));
            unit.Assemble(true, true);
            
            return unit;
        }

        private IEnumerator AnimateAssembleGhost(UnitBase unit)
        {
            yield return new WaitForSeconds(0.01f);
            unit.Assemble(true, true);
            yield break;
        }

        /// <summary>
        /// Called from Editor only
        /// </summary>
        /// <param name="masterunit"></param>
        void CreateUnit(Engine.Master.Unit masterunit)
        {
            Blueprint blueprint = game.Blueprints.FindBlueprint(masterunit.Blueprint.Name);
            if (blueprint == null)
            {
                return;
            }
            UnitBase unit = InstantiatePrefab<UnitBase>(blueprint.Layout);

            //unit.gameObject.layer = LayerMask.GetMask("UI");

            unit.CurrentPos = masterunit.Pos;
            GroundCell targetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(unit.CurrentPos, out targetCell))
            {
                Vector3 unitPos3 = targetCell.transform.localPosition;
                unitPos3.y += HexGrid.MainGrid.hexCellHeight + 0.2f;
                unit.transform.position = unitPos3;
            }

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
            unit.Direction = masterunit.Direction;
            unit.Assemble(masterunit.UnderConstruction, masterunit.UnderConstruction);

            BaseUnits.Add(masterunit.UnitId, unit);

            StartCoroutine(TeleportUnitToPosition(unit));
        }

        private IEnumerator TeleportUnitToPosition(UnitBase unit)
        {
            yield return new WaitForSeconds(0.01f);

            unit.UpdateParts();
            unit.ActivateUnit();
            
            yield break;
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

            // Hide parts, so the do not appear to early
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                child.SetActive(false);
            }

            if (move.MoveType == MoveType.Build)
                unit.CurrentPos = move.Positions[1];
            else
                unit.CurrentPos = move.Positions[0];
            unit.Direction = move.Stats.Direction;
            unit.TurnIntoDirection = move.Stats.Direction;
            unit.PlayerId = move.PlayerId;
            unit.MoveUpdateStats = move.Stats;
            unit.UnitId = move.UnitId;
            unit.gameObject.name = move.UnitId;
            
            BaseUnits.Add(move.UnitId, unit);

            StartCoroutine(AnimateFactoryOutput(move, unit));
        }

        private IEnumerator AnimateFactoryOutput(Move move, UnitBase unit)
        {
            yield return new WaitForSeconds(0.01f);
            unit.Assemble(move.MoveType == MoveType.Build);
            
            if (move.MoveType == MoveType.Build)
            {
                UnitBase factory;
                if (BaseUnits.TryGetValue(move.OtherUnitId, out factory))
                {
                    factory.Upgrade(move, unit);
                }
                unit.DectivateUnit();
            }
            else
            {
                unit.UpdateParts();
                unit.ActivateUnit();
            }
            yield break;
        }

        private Vector3 CalcWorldPos(GroundCell groundCell)
        {
            float gridSizeX = 1.50f;
            float gridSizeY = 1.75f;
            float halfGridSize = 0.86f;

            int x = groundCell.Pos.X;
            int y = groundCell.Pos.Y;

            if ((x & 1) == 0)
            {
                return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
            }
            return new Vector3((x * gridSizeX), 0, -y * gridSizeY);
        }

        public CommandPreview FindCommandForUnit(UnitBase unitBase)
        {
            foreach (CommandPreview mapGameCommand in CommandPreviews.Values)
            {
                foreach (CommandAttachedItem commandAttachedUnit in mapGameCommand.PreviewUnits)
                {
                    if (commandAttachedUnit.AttachedUnit.GhostUnit == unitBase)
                    {
                        // Preview Ghost unit
                        return mapGameCommand;
                    }
                }
                foreach (MapGameCommandItem mapGameCommandItem in mapGameCommand.GameCommand.GameCommandItems)
                {
                    if (mapGameCommandItem.AttachedUnit.UnitId == unitBase.UnitId)
                        return mapGameCommand;
                }
            }
            return null;
        }

        private GroundCell CreateCell(Position2 pos, MoveUpdateStats stats, GameObject cellPrefabx)
        {
            int x = pos.X;
            int y = pos.Y;

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
                if (ShowDebuginfo)
                {
                    groundCell.Visible = stats.MoveUpdateGroundStat.VisibilityMask != 0;
                    groundCell.VisibleByPlayer = (stats.MoveUpdateGroundStat.VisibilityMask & 1) != 0;
                }
                else
                {
                    groundCell.Visible = (stats.MoveUpdateGroundStat.VisibilityMask & 1) != 0;
                    groundCell.VisibleByPlayer = (stats.MoveUpdateGroundStat.VisibilityMask & 1) != 0;
                }
                float height = stats.MoveUpdateGroundStat.Height;
                gridPos3.y += height + 0.3f;
                gameObjectCell.transform.localPosition = gridPos3;

                groundCell.CreateDestructables(true);
            }
            return groundCell;
        }
    }
}