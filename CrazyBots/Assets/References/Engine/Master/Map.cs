using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Engine.Interface
{
    public class MapPlayerInfo
    {
        public int TotalCapacity { get; set; }
        public int TotalMetal { get; set; }
        public int TotalUnits { get; set; }
        public int TotalPower { get; set; }
    }

    public class MapPheromoneItem
    {
        public MapPheromoneItem()
        {

        }
        public int PlayerId { get; set; }
        public float Intensity { get; set; }
        public bool IsStatic { get; set; }
        public PheromoneType PheromoneType { get; set; }
    }

    public class MapPheromone
    {
        public MapPheromone()
        {
            PheromoneItems = new List<MapPheromoneItem>();
        }
        public Position Pos { get; set; }
        public float IntensityToWork { get; set; }
        public float IntensityContainer { get; set; }
        public float IntensityToMineral { get; set; }
        public float IntensityToEnemy { get; set; }
        public List<MapPheromoneItem> PheromoneItems { get; private set; }
    }

    public class MapInfo
    {
        public MapInfo()
        {
            PlayerInfo = new Dictionary<int, MapPlayerInfo>();
            Pheromones = new Dictionary<Position, MapPheromone>();
        }
        public int TotalMetal { get; set; }


        public Dictionary<int, MapPlayerInfo> PlayerInfo { get; private set; }

        public Dictionary<Position, MapPheromone> Pheromones { get; private set; }

        internal void ComputeMapInfo(Game game, List<Move> moves)
        {
            foreach (Pheromone pheromone in game.Pheromones.AllPhromones)
            {
                MapPheromone mapPheromone = new MapPheromone();
                mapPheromone.Pos = pheromone.Pos;

                mapPheromone.IntensityContainer = pheromone.GetIntensityF(0, PheromoneType.Container);
                mapPheromone.IntensityToMineral = pheromone.GetIntensityF(0, PheromoneType.Mineral);
                mapPheromone.IntensityToEnemy = pheromone.GetIntensityF(0, PheromoneType.Enemy);
                mapPheromone.IntensityToWork = pheromone.GetIntensityF(0, PheromoneType.Work);
                

                foreach (PheromoneItem pheromoneItem in pheromone.PheromoneItems)
                {
                    MapPheromoneItem mapPheromoneItem = new MapPheromoneItem();

                    mapPheromoneItem.PlayerId = pheromoneItem.PlayerId;
                    mapPheromoneItem.PheromoneType = pheromoneItem.PheromoneType;
                    mapPheromoneItem.Intensity = pheromoneItem.Intensity;
                    mapPheromoneItem.IsStatic = pheromoneItem.IsStatic;

                    mapPheromone.PheromoneItems.Add(mapPheromoneItem);
                }
                Pheromones.Add(mapPheromone.Pos, mapPheromone);
            }


            foreach (Tile t in game.Map.Tiles.Values)
            {
                TotalMetal += t.Minerals;
                if (t.Unit != null)
                {
                    TotalMetal += t.Unit.CountMineral();

                    MapPlayerInfo mapPlayerInfo;
                    if (PlayerInfo.ContainsKey(t.Unit.Owner.PlayerModel.Id))
                    {
                        mapPlayerInfo = PlayerInfo[t.Unit.Owner.PlayerModel.Id];
                    }
                    else
                    {
                        mapPlayerInfo = new MapPlayerInfo();
                        PlayerInfo.Add(t.Unit.Owner.PlayerModel.Id, mapPlayerInfo);
                    }
                    mapPlayerInfo.TotalCapacity += t.Unit.CountCapacity();
                    mapPlayerInfo.TotalMetal += t.Unit.CountMineralsInContainer();
                    mapPlayerInfo.TotalUnits++;
                }
            }
            
            if (moves != null)
            {
                foreach (Move move in moves)
                {
                    if ((move.MoveType == MoveType.Extract) &&
                        move.Stats != null &&
                        move.Stats.MoveUpdateGroundStat != null)
                    {
                        foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
                        {
                            // Depends when the Part is changing to mineral. In this case, the extractor collects all tileobjects as they are. The are converted
                            // dureing the insert into a container
                            if (tileObject.TileObjectType == TileObjectType.Mineral || TileObject.ConvertTileObjectIntoMineral(tileObject.TileObjectType))
                            {
                                TotalMetal++;
                            }
                        }
                    }
                }
            }
            /*
            if (checkTotalMetal != 0)
            {
                if (checkTotalMetal != mapInfo.TotalMetal)
                {
                    //throw new Exception("Metal changed");
                }
            }*/
        }
    }
}
namespace Engine.Master
{
    public class MapZone
    {
        public int ZoneId { get; set; }
        public int TotalMinerals { get; set; }
        public int MaxMinerals { get; set; }
        public Position Center { get; set; }
        public Player Player { get; set; }

        public Dictionary<Position, TileWithDistance> Tiles { get; set; }

        private List<Tile> openTiles;

        public void StartObjectGenerator(Map map)
        {
            Tile startTile = map.GetTile(Center);
            if (startTile.TileContainer.Count == 0)
            {
                //List<TileObject> tileObjects = CreateRandomObjects(map);
                //startTile.TileContainer.AddRange(tileObjects);
            }
            openTiles = new List<Tile>();
            foreach (Tile n in startTile.Neighbors)
            {
                if (n.TileContainer.Count == 0)
                {
                    n.IsOpenTile = true;
                    openTiles.Add(n);
                }
            }
            /*
            while (openTiles.Count > 0)
                CreateTerrainTile(map);*/
        }

        public void AddTerrainTile(Tile t)
        {
            t.IsOpenTile = true;
            openTiles.Add(t);
        }

        public void AddOpenTile(Tile tile)
        {
            if (Tiles != null && Tiles.ContainsKey(tile.Pos))
            {
                openTiles.Add(tile);
                tile.IsOpenTile = true;
            }
        }


        public Position CreateTerrainTile(Map map)
        {
            if (map.OpenTileObjects.Count == 0)
                return null;

            //if (ZoneId != 2 && ZoneId != 4)
            //    return null;

            if (openTiles == null)
                return null;

            List<Tile> unopenTiles = new List<Tile>();

            Position pos = null;

            if (openTiles.Count > 0)
            {
                List<TileObject> tileObjects = CreateRandomObjects(map);
                //if (tileObjects.Count == 0) return null;
                
                // Find best tile
                TileFit bestTileFit = null;
                Tile bestTile = null;
                foreach (Tile tile in openTiles)
                {
                    if (!tile.IsOpenTile)
                    {
                        // Filled by soimeone else
                        unopenTiles.Add(tile);
                    }
                    else
                    {
                        foreach (Tile n in tile.Neighbors)
                        {
                            if (!n.IsOpenTile)
                                continue;
                            //if (n.TileContainer.TileObjects.Count == 0)
                            //    continue;
                            //if (n.Unit != null && tileObjects.Count > 1)
                            //    continue;

                            TileFit tileFit = n.CalcFit(tileObjects);

                            if (bestTileFit == null || tileFit.Score > bestTileFit.Score)
                            {
                                bestTileFit = tileFit;
                                bestTile = tile;
                            }
                        }
                    }
                }
                foreach (Tile tile in unopenTiles)
                {
                    openTiles.Remove(tile);
                }

                if (bestTile != null)
                {
                    if (bestTileFit != null)
                    {
                        if (!bestTile.IsOpenTile)
                        {
                            throw new Exception();
                        }
                        foreach (TileObject tileObject in bestTileFit.TileObjects)
                        {
                            if (!map.OpenTileObjects.Remove(tileObject))
                            {
                                throw new Exception();
                            }
                        }
                        bestTile.TileContainer.AddRange(bestTileFit.TileObjects);
                        pos = bestTile.Pos;

                        bool removeOpenTile = false;

                        //if (!bestTile.CanBuild())
                            removeOpenTile = true;
                        if (removeOpenTile)
                        {
                            if (!openTiles.Remove(bestTile))
                            {
                                throw new Exception();
                            }
                            if (!map.Game.changedGroundPositions.ContainsKey(bestTile.Pos))
                                map.Game.changedGroundPositions.Add(bestTile.Pos, null);
                            bestTile.IsOpenTile = false;

                            foreach (Tile n in bestTile.Neighbors)
                            {
                                if (!openTiles.Contains(n) && n.CanBuild()) // && Tiles.ContainsKey(n.Pos)) // Only in zone (creates circles)
                                {
                                    if (!map.Game.changedGroundPositions.ContainsKey(n.Pos))
                                        map.Game.changedGroundPositions.Add(n.Pos, null);

                                    n.IsOpenTile = true;
                                    openTiles.Add(n);
                                }
                            }
                        }
                    }
                }
           }
           return pos;
        }

        internal List<TileObject> CreateRandomObjects(Map map)
        {
            List<TileObject> tileObjects = new List<TileObject>();

            TileObject tileObject = null;
            int mapIndex = 0;

            int rnd = map.Game.Random.Next(5);
            rnd -= 2;
            while (rnd-- > 0)
            {
                tileObject = null;
                if (Player == null)
                {
                    if (mapIndex >= map.OpenTileObjects.Count)
                        break;

                    tileObject = map.OpenTileObjects[mapIndex];
                    mapIndex++;
                }
                else
                {
                    while (tileObject == null && mapIndex < map.OpenTileObjects.Count)
                    {
                        if (map.OpenTileObjects[mapIndex].TileObjectType == TileObjectType.Mineral)
                        {
                            tileObject = map.OpenTileObjects[mapIndex];
                        }
                        mapIndex++;
                    }
                }
                if (tileObject == null)
                    break;

                bool dirExists;
                do
                {
                    dirExists = false;
                    int dir = map.Game.Random.Next(6);

                    if (dir == 0) tileObject.Direction = Direction.N;
                    else if (dir == 1) tileObject.Direction = Direction.NE;
                    else if (dir == 2) tileObject.Direction = Direction.SE;
                    else if (dir == 3) tileObject.Direction = Direction.S;
                    else if (dir == 4) tileObject.Direction = Direction.SW;
                    else if (dir == 5) tileObject.Direction = Direction.NW;

                    foreach (TileObject alreadyAdded in tileObjects)
                    {
                        if (alreadyAdded == tileObject)
                        {
                            // Duplicate object in list
                            break;
                        }
                        if (alreadyAdded.Direction == tileObject.Direction)
                            dirExists = true;

                    }
                } while (dirExists);

                tileObjects.Add(tileObject);
            }
            return tileObjects;
        }
    }

    public class Map
    {
        public Dictionary<int, MapZone> Zones = new Dictionary<int, MapZone>();
        public Dictionary<Position, MapSector> Sectors = new Dictionary<Position, MapSector>();
        public Dictionary<Position, Tile> Tiles = new Dictionary<Position, Tile>();

        public Units Units { get; private set; }

        private int seed;
        public Game Game { get; private set; }

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public string MapType { get; set; }
        private int zoneCounter;
        //private int zoneWidth;
        //private int maxZones;

        public List<TileObject> OpenTileObjects { get; private set; }

        public void AddOpenTileObject(TileObject tileObject)
        {
            if (TileObject.ConvertTileObjectIntoMineral(tileObject.TileObjectType))
            {
                tileObject.TileObjectType = TileObjectType.Mineral;
            }
            OpenTileObjects.Add(tileObject);
        }

        public int DefaultMinerals
        {
            get
            {
                return (Zones.Count-1) * 40;
            }
        }

        private MapGenerator.HexMapGenerator mapGenerator;

        public Map(Game game, int seed)
        {
            this.seed = seed;
            Game = game;
            Units = new Units(this);
            MapWidth = game.GameModel.MapWidth;
            MapHeight = game.GameModel.MapHeight;
            MapType = game.GameModel.MapType;

            OpenTileObjects = new List<TileObject>();
            //zoneWidth = (MapWidth / 15);
            //maxZones = zoneWidth * (MapHeight / 15) - 1;

            /*
            Matrix = new byte[gameModel.MapWidth, gameModel.MapHeight];
            for (int y = 0; y < Matrix.GetUpperBound(1); y++)
                for (int x = 0; x < Matrix.GetUpperBound(0); x++)
                    Matrix[x, y] = 1;

            if (!string.IsNullOrEmpty(gameModel.Obstacles))
            {
                string[] aPositions = gameModel.Obstacles.Split(new char[] { ';' });
                foreach (string pos in aPositions)
                {
                    string[] coords = pos.Split(new char[] { ',' });
                    int x = Convert.ToInt32(coords[0]);
                    int y = Convert.ToInt32(coords[1]);
                    Matrix[x, y] = 0;
                }
            }*/
        }

        private int sectorSize = 12;

        public Tile GetTile(Position pos)
        {
            if (Tiles.ContainsKey(pos))
                return Tiles[pos];

            return null;
        }
        /*
        public MapSector GetSector(Position pos)
        {
            int sectorX = pos.X / sectorSize;
            int sectorY = pos.Y / sectorSize;
            Position sectorPos = new Position(sectorX, sectorY);

            MapSector mapSector = null;
            if (Sectors.ContainsKey(sectorPos))
                mapSector = Sectors[sectorPos];

            return mapSector;
        }*/

        private void CopyValues(MapGenerator.HexCell hexCell, Tile t)
        {
            t.Height = hexCell.Elevation;
            t.TerrainTypeIndex = hexCell.TerrainTypeIndex;
            t.IsUnderwater = hexCell.IsUnderwater;
            t.PlantLevel = hexCell.PlantLevel;
        }

        public void GenerateTiles()
        {
            for (int z = 0, i = 0; z < mapGenerator.cellCountZ; z++)
            {
                for (int x = 0; x < mapGenerator.cellCountX; x++)
                {
                    MapGenerator.HexCell hexCell = mapGenerator.GetCell(i++);

                    Position sectorPos = new Position(x, z);
                    MapSector mapSector = new MapSector();
                    mapSector.Center = new Position(sectorPos.X * sectorSize, sectorPos.Y * sectorSize);
                    mapSector.HexCell = hexCell;
                    Sectors.Add(sectorPos, mapSector);

                    //MapGenerator.HexCell hexCellE = hexCell.GetNeighbor(MapGenerator.HexDirection.E);
                    MapGenerator.HexCell hexCellW = hexCell.GetNeighbor(MapGenerator.HexDirection.W);

                    MapGenerator.HexCell hexCellSW = hexCell.GetNeighbor(MapGenerator.HexDirection.SW);
                    MapGenerator.HexCell hexCellSE = hexCell.GetNeighbor(MapGenerator.HexDirection.SE);

                    //MapGenerator.HexCell hexCellNE = hexCell.GetNeighbor(MapGenerator.HexDirection.NE);
                    //MapGenerator.HexCell hexCellSE = hexCell.GetNeighbor(MapGenerator.HexDirection.SE);

                    int startx = sectorPos.X * sectorSize - 2;
                    int starty = sectorPos.Y * sectorSize - 2;
                    int widthx = sectorPos.X * sectorSize + sectorSize + 4;
                    int widthy = sectorPos.Y * sectorSize + sectorSize + 4;

                    if ((x % 2) == 0)
                    {
                        starty -= 1; // sectorSize / 2;
                        widthy -= 1; // sectorSize / 2;
                    }
                    if ((z % 2) == 0)
                    {
                        startx -= 1; // sectorSize / 2;
                        widthx -= 1; // sectorSize / 2;
                    }
                    for (int sectorX = startx; sectorX < widthx; sectorX++)
                    {
                        for (int sectorY = starty; sectorY < widthy; sectorY++)
                        {
                            Position sectorTilePos = new Position(sectorX, sectorY);

                            Tile t = null;
                            if (Tiles.ContainsKey(sectorTilePos))
                            {
                                t = Tiles[sectorTilePos];
                            }
                            else
                            {
                                t = new Tile(this, sectorTilePos);
                                Tiles.Add(sectorTilePos, t);
                            }
                            CopyValues(hexCell, t);

                            int borderWidth = 3;

                            // Senkrechte kante
                            
                            if (hexCellW != null && sectorX < sectorPos.X * sectorSize + 1)
                            {
                                if (Game.Random.Next(2) == 0)
                                {
                                    CopyValues(hexCellW, t);
                                    t.Height = (hexCell.Elevation + hexCellW.Elevation) / 2;
                                }
                            }
                            
                            // Corner
                            if (hexCellSW != null && sectorX < sectorPos.X * sectorSize + borderWidth && sectorY < sectorPos.Y * sectorSize + borderWidth)
                            {
                                if (Game.Random.Next(2) == 0)
                                {
                                    CopyValues(hexCellSW, t);
                                }
                            }

                            /* Not working
                            if (hexCellNW != null && sectorX < sectorPos.X * sectorSize + borderWidth && sectorY < sectorPos.Y * sectorSize + sectorSize - borderWidth)
                            {
                                if (Game.Random.Next(2) == 0)
                                {
                                    CopyValues(hexCellNW, t);
                                }
                            }*/

                            // Corner
                            if (hexCellSE != null && sectorX > sectorPos.X * sectorSize + sectorSize - borderWidth && sectorY < sectorPos.Y * sectorSize + borderWidth)
                            {
                                if (Game.Random.Next(2) == 0)
                                {
                                    CopyValues(hexCellSE, t);
                                }
                            }

                            /*
                            if (hexCellSE != null && sectorX > sectorPos.X * sectorSize + sectorSize - borderWidth && sectorY < sectorPos.Y * sectorSize + sectorSize - borderWidth)
                            {
                                if (Game.Random.Next(2) == 0)
                                {
                                    CopyValues(hexCellSE, t);
                                }
                            }                           
                            if (hexCellE != null && sectorX > sectorPos.X * sectorSize + sectorSize - borderWidth)
                            {
                                if (Game.Random.Next(2) == 0)
                                {
                                    CopyValues(hexCellE, t);
                                }
                            }
                            */
                            //if (!t.IsUnderwater)
                            if (t.Height > 0)
                            {
                                if (hexCell.TerrainTypeIndex == 0)
                                {
                                    /*
                                    t.NumberOfDestructables = hexCell.PlantLevel;
                                    if (Game.Random.Next(6) == 0)
                                        t.NumberOfDestructables++;
                                    */
                                }
                                if (hexCell.TerrainTypeIndex == 1)
                                {
                                    /*
                                    if (Game.Random.Next(12) == 0)
                                        t.NumberOfDestructables = hexCell.PlantLevel;
                                    */
                                }

                                if (hexCell.TerrainTypeIndex == 3)
                                {
                                    /*
                                    t.NumberOfDestructables = hexCell.PlantLevel;
                                    if (Game.Random.Next(6) == 0)
                                        t.NumberOfDestructables++;
                                    */
                                }

                                if (hexCell.TerrainTypeIndex >= 4)
                                {
                                    t.Height += 3;
                                }
                            }
                            t.Height /= Game.Random.Next(5, 7);
                            if (t.Height < 0.1f)
                                t.Height = 0.1f;

                        }
                    }
                }
            }

            /*
            mineralDwells = new Dictionary<Position, int>();

            int zone = 0;
            while (zone < maxZones)
            {
                int minX = (zone % zoneWidth) * 15;
                int minY = (zone / zoneWidth) * 15;
                zone++;

                int x = Game.Random.Next(10) + 3;
                int y = Game.Random.Next(10) + 3;

                Position dwell = new Position(minX + x, minY + y);
                //Tile t = GetTile(dwell);
                //t.Height = 0.5f;

                mineralDwells.Add(dwell, 0);
            }*/


            //checkTotalMetal = GetMapInfo().TotalMetal;

        }

        public void CreateFlat()
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    Position sectorPos = new Position(x, y);
                    MapSector mapSector = new MapSector();
                    mapSector.Center = new Position(sectorPos.X * sectorSize, sectorPos.Y * sectorSize);

                    int startx = sectorPos.X;
                    int starty = sectorPos.Y;
                    int widthx = sectorPos.X * sectorSize + sectorSize;
                    int widthy = sectorPos.Y * sectorSize + sectorSize;

                    for (int sectorX = startx; sectorX < widthx; sectorX++)
                    {
                        for (int sectorY = starty; sectorY < widthy; sectorY++)
                        {
                            Position sectorTilePos = new Position(sectorX, sectorY);
                            Tile t;
                            if (Tiles.ContainsKey(sectorTilePos))
                            {
                                t = Tiles[sectorTilePos];
                            }
                            else
                            {
                                t = new Tile(this, sectorTilePos);
                                t.TerrainTypeIndex = 1;
                                t.Height = Game.Random.NextDouble() / 4;
                                Tiles.Add(sectorTilePos, t);
                            }
                        }
                    }

                    Sectors.Add(sectorPos, mapSector);
                    /*
                    Position sectorTilePos = new Position(x, y);
                    Tile t = new Tile(this, sectorTilePos);
                    Tiles.Add(sectorTilePos, t);*/
                }
            }
            /*
            MapSector mapSector = new MapSector();
            mapSector.Center = new Position(MapWidth / 2, MapHeight / 2);
            Sectors.Add(mapSector.Center, mapSector);
            */
            /*
            Tile startTile = GetTile(mapSector.Center);
            startTile.TileObjects = CreateRandomObjects(1);

            openTiles = new List<Tile>();
            openTiles.AddRange(startTile.Neighbors);

            while (openTiles.Count > 0)
                CreateTerrainTile();*/

            for (int i=0; i < 400; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = TileObjectType.Tree;
                OpenTileObjects.Add(tileObject);
            }
            for (int i = 0; i < 100; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = TileObjectType.Bush;
                OpenTileObjects.Add(tileObject);
            }
            /*
            for (int i = 0; i < 15; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = TileObjectType.Tree;
                OpenTileObjects.Add(tileObject);
            }
            for (int i = 0; i < 15; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = TileObjectType.Bush;
                OpenTileObjects.Add(tileObject);
            }*/
        }

        public void CreateTerrain(Game game)
        {
            mapGenerator = new MapGenerator.HexMapGenerator();
            mapGenerator.Random = game.Random;
            mapGenerator.GenerateMap(MapWidth, MapHeight, false);
            GenerateTiles();
        }
        public void CreateZones()
        {
            MapZone mapDefaultZone = new MapZone();
            mapDefaultZone.ZoneId = 0;
            mapDefaultZone.MaxMinerals = -1;
            Zones.Add(mapDefaultZone.ZoneId, mapDefaultZone);

            // Create startup zone for player
            Position startPosition = null;
            foreach (MapSector mapSector in Sectors.Values)
            {
                if (MapType != "2")
                {
                    if (mapSector.Center.X < 8 || mapSector.Center.Y < 8)
                        continue;
                }
                if (mapSector.IsPossibleStart(this))
                {
                    Position zoneCenter = new Position(mapSector.Center.X + sectorSize / 2, mapSector.Center.Y + sectorSize / 2);
                    AddZone(zoneCenter);
                    startPosition = mapSector.Center;

                    break;
                }
            }

            if (startPosition != null)
            {
                // Any other zones with minerals?
                foreach (MapSector mapSector in Sectors.Values)
                {
                    if (MapType == "2" || this.Game.Random.Next(4) == 0)
                    {
                        if (mapSector.Center != startPosition &&
                            mapSector.IsPossibleStart(this))
                        {
                            Position zoneCenter = new Position(mapSector.Center.X + sectorSize / 2, mapSector.Center.Y + sectorSize / 2);

                            AddZone(zoneCenter);
                        }
                    }
                }
            }

            //AddZone(new Position(10, 10));
            //AddZone(new Position(10, 30));
            //AddZone(new Position(30, 30));

            /*
            mapDefaultZone.Tiles = new Dictionary<Position, TileWithDistance>(); 
            foreach (Tile t in Tiles.Values)
            {
                mapDefaultZone.Tiles.Add(t.Pos, new TileWithDistance(t,0));
            }*/
        }
        public void AddTerrainTile(Tile t)
        {
            MapZone zone;
            if (t.ZoneId == 0)
            {
                // Will do for now
                zone = Zones[1];
            }
            else
            {
                zone = Zones[t.ZoneId];
            }
            zone.AddTerrainTile(t);
        }

        public void AddOpenTile(Tile tile)
        {
            foreach (MapZone mapZone in Zones.Values)
            {
                mapZone.AddOpenTile(tile);
            }
        }

        private void AddZone(Position pos)
        {
            int zoneId = Zones.Count;

            MapZone mapZone = new MapZone();
            mapZone.ZoneId = zoneId;
            mapZone.MaxMinerals = 40;
            mapZone.Center = pos;

            mapZone.Tiles = EnumerateTiles(pos, sectorSize/2, true);
            foreach (TileWithDistance t in mapZone.Tiles.Values)
            {
                t.Tile.ZoneId = zoneId;
            }
            Zones.Add(mapZone.ZoneId, mapZone);

            mapZone.StartObjectGenerator(this);
        }

        public void DistributeTileObject(TileObject tileObject)
        {
            if (Zones.Count <= 1)
                return;

            if (zoneCounter == 0)
                zoneCounter = 1;

            int max = Zones.Count;
            while (max-- > 0)
            {
                KeyValuePair<int, MapZone> zoneItem = Zones.ElementAt(zoneCounter);
                MapZone mapZone = zoneItem.Value;

                if (++zoneCounter >= Zones.Count)
                    zoneCounter = 1;

                if (mapZone.MaxMinerals == -1 || mapZone.TotalMinerals < mapZone.MaxMinerals)
                {
                    int idx = Game.Random.Next(mapZone.Tiles.Count);

                    TileWithDistance t = mapZone.Tiles.ElementAt(idx).Value;
                    if (t != null && t.Minerals < 20)
                    {
                        if (t.Unit != null && t.Unit.Engine == null)
                        {
                            // Dont drop on buildings
                        }
                        else
                        {
                            if (!Game.changedGroundPositions.ContainsKey(t.Pos))
                                Game.changedGroundPositions.Add(t.Pos, null);

                            //tileObject = OpenTileObjects[0];
                            //OpenTileObjects.RemoveAt(0);

                            t.Tile.TileContainer.Add(tileObject);
                            break;
                        }
                    }
                }
            }
            /*
            int minX, minY;

            int retryZones = 5;
            while (retryZones-- > 0)
            {
                minX = (zoneCounter % zoneWidth) * 10;
                minY = (zoneCounter / zoneWidth) * 10;

                zoneCounter++;
                if (zoneCounter > maxZones)
                    zoneCounter = 0;

                int retryMinerals = 30;
                while (retryMinerals-- > 0)
                {
                    int x = Game.Random.Next(10);
                    int y = Game.Random.Next(10);

                    Position pos = new Position(minX + x, minY + y);
                    Tile t = GetTile(pos);
                    if (t == null || t.Unit != null || t.Metal >= 20)
                        continue;

                    if (!Game.changedGroundPositions.ContainsKey(t.Pos))
                        Game.changedGroundPositions.Add(t.Pos, null);

                    t.Metal++;
                    retryZones = 0;
                    break;
                }
            }*/
        }


        public Dictionary<Position, TileWithDistance> EnumerateTiles(Position startPos, int range, bool includeStartPos = true, Func<TileWithDistance, bool> stopper = null, Func<TileWithDistance, bool> matcher = null)
        {
            Dictionary<Position, TileWithDistance> resultList = new Dictionary<Position, TileWithDistance>();

            List<TileWithDistance> openList = new List<TileWithDistance>();
            Dictionary<Position, TileWithDistance> reachedTiles = new Dictionary<Position, TileWithDistance>();

            Tile startTilePos = GetTile(startPos);
            if (startTilePos == null) return null;

            TileWithDistance startTile = new TileWithDistance(startTilePos, 0);

            openList.Add(startTile);
            reachedTiles.Add(startTile.Pos, startTile);

            if (includeStartPos)
            {
                if (matcher == null || matcher(startTile))
                    resultList.Add(startTile.Pos, startTile);
            }

            while (openList.Count > 0)
            {
                TileWithDistance tile = openList[0];
                openList.RemoveAt(0);

                // Distance at all
                if (tile.Distance > range)
                    continue;

                foreach (Tile n in tile.Neighbors)
                {
                    if (n.Pos == startPos)
                        continue;

                    if (!reachedTiles.ContainsKey(n.Pos))
                    {
                        TileWithDistance neighborsTile = new TileWithDistance(GetTile(n.Pos), tile.Distance + 1);

                        reachedTiles.Add(neighborsTile.Pos, neighborsTile);

                        // Distance at all
                        if (neighborsTile.Distance <= range)
                        {
                            openList.Add(neighborsTile);

                            if (matcher == null || matcher(neighborsTile))
                            {
                                if (includeStartPos)
                                    resultList.Add(neighborsTile.Pos, neighborsTile);
                                else if (neighborsTile.Pos != startPos)
                                    resultList.Add(neighborsTile.Pos, neighborsTile);
                            }
                            if (stopper != null && stopper(neighborsTile))
                                return resultList;
                        }
                    }
                }
            }
            return resultList;
        }

        //private HeightMap terrain;

        public void CollectGroundStats(Position pos, Move move, List<TileObject> tileObjects)
        {
            CollectGroundStats(pos, move);

            foreach (TileObject tileObject in tileObjects)
            {
                TileObject newTileObject = new TileObject();
                newTileObject.TileObjectType = tileObject.TileObjectType;
                newTileObject.Direction = tileObject.Direction;
                move.Stats.MoveUpdateGroundStat.TileObjects.Add(newTileObject);
            }
        }
        
        public void CollectGroundStats(Position pos, Move move)
        {
            if (move.Stats == null)
                move.Stats = new MoveUpdateStats();

            if (move.Stats.MoveUpdateGroundStat == null)
                move.Stats.MoveUpdateGroundStat = new MoveUpdateGroundStat();

            MoveUpdateGroundStat moveUpdateGroundStat = move.Stats.MoveUpdateGroundStat;

            Tile t = GetTile(pos);
            moveUpdateGroundStat.Owner = t.Owner;
            moveUpdateGroundStat.IsBorder = t.IsBorder;
            moveUpdateGroundStat.PlantLevel = t.PlantLevel;
            moveUpdateGroundStat.TerrainTypeIndex = t.TerrainTypeIndex;
            moveUpdateGroundStat.IsUnderwater = t.IsUnderwater;
            moveUpdateGroundStat.TileObjects = new List<TileObject>();
            moveUpdateGroundStat.TileObjects.AddRange(t.TileContainer.TileObjects);
            moveUpdateGroundStat.Height = (float)t.Height;
            moveUpdateGroundStat.IsOpenTile = t.IsOpenTile;

            //MoveUpdateStats moveUpdateStats;
            //moveUpdateStats = new MoveUpdateStats();
            //moveUpdateStats.MoveUpdateGroundStat = moveUpdateGroundStat;
            
        }

        /*
        private HeightMap GenerateTerrain(int? seed = null)
        {
            var generator1 = new FlatGenerator(1000, 1000, 1);
            var generator2 = new DSNoiseGenerator(1000, 1000, seed: seed);

            var node1 = new GeneratorNode(generator1);
            var node2 = new GeneratorNode(generator2);

            var blend = new BlendingNode(BlendModes.Difference);
            blend.AddDependency(node1);
            blend.AddDependency(node2);
            blend.Execute();

            return blend.Result;
        }*/


        private int[] GenerateTerrainx()
        {
            int width = 100;
            int xeight = 100;
            int [] terrainContour = new int[width * xeight];

            //Make Random Numbers
            double rand1 = Game.Random.NextDouble() + 1;
            double rand2 = Game.Random.NextDouble() + 2;
            double rand3 = Game.Random.NextDouble() + 3;

            //Variables, Play with these for unique results!
            float peakheight = 20;
            float flatness = 50;
            int offset = 30;

            //Generate basic terrain sine
            for (int x = 0; x < width; x++)
            {

                double dblheight = peakheight / rand1 * Math.Sin((float)x / flatness * rand1 + rand1);
                dblheight += peakheight / rand2 * Math.Sin((float)x / flatness * rand2 + rand2);
                dblheight += peakheight / rand3 * Math.Sin((float)x / flatness * rand3 + rand3);

                dblheight += offset;

                terrainContour[x] = (int)dblheight;
            }
            return terrainContour;
        }

    }
}
