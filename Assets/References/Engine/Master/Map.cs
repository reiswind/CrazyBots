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
                            if (tileObject.TileObjectType == TileObjectType.Mineral || TileObject.CanConvertTileObjectIntoMineral(tileObject.TileObjectType))
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

    

    public class Map
    {
        public Dictionary<int, MapZone> Zones = new Dictionary<int, MapZone>();
        public Dictionary<Position, MapSector> Sectors = new Dictionary<Position, MapSector>();
        public Dictionary<Position, Tile> LargeMapTiles = new Dictionary<Position, Tile>();
        public Dictionary<MapGenerator.HexCell, MapSector> HexCellSectors = new Dictionary<MapGenerator.HexCell, MapSector>();
        public Dictionary<Position, Tile> Tiles = new Dictionary<Position, Tile>();

        public Units Units { get; private set; }

        private int seed;
        public Game Game { get; private set; }

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public string MapType { get; set; }
        private int zoneCounter;

        public int BioMass { get; set; }

        public void AddOpenTileObject(TileObject tileObject)
        {
            if (TileObject.CanConvertTileObjectIntoMineral(tileObject.TileObjectType))
            {
                tileObject.TileObjectType = TileObjectType.Mineral;
            }
            if (tileObject.TileObjectType == TileObjectType.Mineral)
            {
                DistributeTileObject(tileObject);
            }
            else
            {
                BioMass += TileObject.GetBioMass(tileObject.TileObjectType);
            }
        }

        public int DefaultMinerals
        {
            get
            {
                if (MapType == "2")
                {
                    int cnt = 0;
                    foreach (MapZone mapZone in Zones.Values)
                    {
                        cnt += mapZone.MaxMinerals;
                    }
                    return cnt;
                }
                return 0;
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

            if (MapType == "2")
                sectorSize = 8;
            else
                sectorSize = 10;

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

        private int sectorSize;

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
            //t.TerrainTypeIndex = hexCell.TerrainTypeIndex;
            t.IsUnderwater = hexCell.IsUnderwater;
            //t.PlantLevel = hexCell.PlantLevel;
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

                    Position center = CalcTilePos(sectorPos);

                    mapSector.Center = center;
                    mapSector.HexCell = hexCell;
                    Sectors.Add(sectorPos, mapSector);
                    HexCellSectors.Add(hexCell, mapSector);

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

        internal Position CalcTilePos(Position sectorPos)
        {
            int x = sectorPos.X;
            int y = sectorPos.Y;

            x = x * sectorSize;
            y = y * sectorSize;

            if ((sectorPos.X % 2) != 0)
            {
                y += sectorSize / 2;
            }
            return new Position(x, y);
            if ((sectorPos.X % 2) != 0)
            {
                //x = (x * sectorSize) - (sectorSize / 2);
                //x -= (sectorSize / 2) + 1;
                if ((sectorPos.Y % 2) != 0)
                {
                    //x -= sectorSize;
                    x -= sectorSize / 2; // NE
                    //x += sectorSize / 2; // SE NW
                    y += sectorSize / 2;

                    //x *= sectorSize;
                    //y = -(y * sectorSize) - (sectorSize / 2);
                    //return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
                }
                else
                {
                    // N/S
                    //x -= sectorSize / 2;
                }
            }
            else if ((sectorPos.Y % 2) != 0)
            {
                //if ((sectorPos.X % 2) != 0)
                {
                    x -= sectorSize / 2; // NW
                    y += sectorSize / 2;
                }
                //x *= sectorSize;
                //y = -(y * sectorSize) - (sectorSize / 2);
                //return new Vector3((x * gridSizeX), 0, -(y * gridSizeY) - halfGridSize);
            }
            return new Position(x, y);

        }

        public void CreateFlat()
        {
            int mapWidth = MapWidth;

            int offsetx = 10;
            int offsety = 10;

            int map_radius = mapWidth;
            for (int q = -map_radius; q <= map_radius; q++)
            {
                int r1 = Math.Max(-map_radius, -q - map_radius);
                int r2 = Math.Min(map_radius, -q + map_radius);
                for (int r = r1; r <= r2; r++)
                {
                    CubePosition cubePosition = new CubePosition(q, r, -q - r);
                    Position position = new Position(cubePosition.Pos.X + offsetx, cubePosition.Pos.Y + offsety);

                    Tile largeTile = new Tile(this, position);
                    LargeMapTiles.Add(largeTile.Pos, largeTile);
                }
            }

            // Add water border
            map_radius = mapWidth + 3;
            for (int q = -map_radius; q <= map_radius; q++)
            {
                int r1 = Math.Max(-map_radius, -q - map_radius);
                int r2 = Math.Min(map_radius, -q + map_radius);
                for (int r = r1; r <= r2; r++)
                {
                    CubePosition cubePosition = new CubePosition(q, r, -q - r);
                    Position position = new Position(cubePosition.Pos.X + offsetx, cubePosition.Pos.Y + offsety);
                    if (!LargeMapTiles.ContainsKey(position))
                    {
                        Tile largeTile = new Tile(this, position);
                        largeTile.IsUnderwater = true;
                        LargeMapTiles.Add(largeTile.Pos, largeTile);
                    }
                }
            }



            foreach (Tile tile1 in LargeMapTiles.Values)
            {
                Dictionary<Position, Tile> createdTiles = new Dictionary<Position, Tile>();
                Position center = CalcTilePos(tile1.Pos);

                //https://www.redblobgames.com/grids/hexagons/implementation.html
                map_radius = (sectorSize / 2) + 1;
                for (int q = -map_radius; q <= map_radius; q++)
                {
                    int r1 = Math.Max(-map_radius, -q - map_radius);
                    int r2 = Math.Min(map_radius, -q + map_radius);
                    for (int r = r1; r <= r2; r++)
                    {
                        CubePosition cubePosition = new CubePosition(q, r, -q - r);
                        Position position = new Position(cubePosition.Pos.X + center.X, cubePosition.Pos.Y + center.Y);

                        Tile tile;
                        if (!Tiles.TryGetValue(position, out tile))
                        {
                            tile = new Tile(this, position);
                            Tiles.Add(tile.Pos, tile);
                            createdTiles.Add(tile.Pos, tile);
                        }
                    }
                }

                int zoneId = Zones.Count;

                foreach (Tile addedTile in createdTiles.Values)
                {
                    addedTile.ZoneId = zoneId;
                }

                MapZone mapZone = new MapZone();
                mapZone.ZoneId = zoneId;
                mapZone.Center = center;
                mapZone.Tiles = createdTiles;
                Zones.Add(mapZone.ZoneId, mapZone);
                mapZone.IsUnderwater = tile1.IsUnderwater;

                foreach (Player player in Game.Players.Values)
                {
                    if (player.PlayerModel.Zone == zoneId)
                    {
                        player.StartZone = mapZone;
                        mapZone.Player = player;
                    }
                }

                if (mapZone.IsUnderwater)
                {
                    mapZone.Vegetation.Add(new MapVegetation(100, TileFitType.Water));
                }
                else
                {
                    if (mapZone.Player == null)
                    {
                        int rnd = Game.Random.Next(4);
                        if (rnd == 0)
                        {
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(2), TileFitType.Stone));
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(2), TileFitType.Water));
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(7), TileFitType.Sand));
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(8) + 2, TileFitType.BushGras));
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(8) + 2, TileFitType.TreeBush));
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(8), TileFitType.Tree));
                            mapZone.Vegetation.Add(new MapVegetation(100, TileFitType.Gras));
                        }
                        else if (rnd == 1)
                        {
                            mapZone.Vegetation.Add(new MapVegetation(3, TileFitType.Water));
                            mapZone.Vegetation.Add(new MapVegetation(5, TileFitType.Stone));
                            mapZone.Vegetation.Add(new MapVegetation(100, TileFitType.Sand));
                        }
                        else if (rnd == 3)
                        {
                            mapZone.Vegetation.Add(new MapVegetation(Game.Random.Next(2), TileFitType.Stone));
                            mapZone.Vegetation.Add(new MapVegetation(20, TileFitType.Gras));
                            mapZone.Vegetation.Add(new MapVegetation(30, TileFitType.TreeBush));
                            mapZone.Vegetation.Add(new MapVegetation(100, TileFitType.Tree));
                        }
                        else if (rnd == 4)
                        {
                            mapZone.Vegetation.Add(new MapVegetation(30, TileFitType.BushGras));
                            mapZone.Vegetation.Add(new MapVegetation(100, TileFitType.Gras));
                        }
                    }
                    else
                    {
                        mapZone.Vegetation.Add(new MapVegetation(10, TileFitType.Sand));
                        mapZone.Vegetation.Add(new MapVegetation(90, TileFitType.Gras));                        
                    }

                    //BioMass += 300;
                    mapZone.MaxMinerals = 20;
                    mapZone.StartObjectGenerator(this);
                }
            }

            /*
            int startx = tile.Pos.X;
            int starty = tile.Pos.Y;
            int widthx = tile.Pos.X * sectorSize + sectorSize;
            int widthy = tile.Pos.Y * sectorSize + sectorSize;

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
                        //t.Height = Game.Random.NextDouble() / 4;
                        Tiles.Add(sectorTilePos, t);
                    }
                }
            }
            */
            //AddNeigbors(tile, tiles);

            /*

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
                                //t.Height = Game.Random.NextDouble() / 4;
                                Tiles.Add(sectorTilePos, t);
                            }
                        }
                    }

                    Sectors.Add(sectorPos, mapSector);
                    

                }
            }
            
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

            // ADDTILES
            //BioMass = 3000;
            /*
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
            for (int i = 0; i < 100; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = TileObjectType.Dirt;
                OpenTileObjects.Add(tileObject);
            }*/
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
            /*
            foreach (Tile tile in LargeMapTiles.Values)
            {
                Position zoneCenter = CalcTilePos(tile.Pos);
                AddZone(tile, zoneCenter);
            }*/

            /*
            MapZone mapDefaultZone = new MapZone();
            mapDefaultZone.ZoneId = 0;
            mapDefaultZone.MaxMinerals = -1;
            Zones.Add(mapDefaultZone.ZoneId, mapDefaultZone);
            */

            // Create startup zone for player
            /*
            MapSector startSector = null;
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
                    AddZone(mapSector, zoneCenter);
                    startSector = mapSector;
                    break;
                }
            }

            if (startSector != null)
            {
                // Test, add zone for each sektor
                if (MapType == "2")
                {
                    foreach (MapSector mapSector in Sectors.Values)
                    {
                        if (mapSector == startSector)
                            continue;

                        Position zoneCenter = new Position(mapSector.Center.X + sectorSize / 2, mapSector.Center.Y + sectorSize / 2);
                        AddZone(mapSector, zoneCenter);
                    }
                }
                else
                {
                    List<Position> positions = new List<Position>();
                    List<MapSector> mapSectors = new List<MapSector>();
                    AddNeigbors(startSector, mapSectors, positions);

                    foreach (MapSector mapSector in mapSectors)
                    {
                        List<MapSector> mapSectorN = new List<MapSector>();
                        AddNeigbors(mapSector, mapSectorN, positions);
                    }
                }
            
            }
            */
        }

        private void AddNeigbors(MapSector mapSector, List<MapSector> mapSectors, List<Position> positions)
        {
            MapSector nextSector;
            nextSector = AddNeigbor(mapSector, MapGenerator.HexDirection.E, positions);
            if (nextSector != null) mapSectors.Add(nextSector);

            nextSector = AddNeigbor(mapSector, MapGenerator.HexDirection.NE, positions);
            if (nextSector != null) mapSectors.Add(nextSector);

            nextSector = AddNeigbor(mapSector, MapGenerator.HexDirection.NW, positions);
            if (nextSector != null) mapSectors.Add(nextSector);

            nextSector = AddNeigbor(mapSector, MapGenerator.HexDirection.SE, positions);
            if (nextSector != null) mapSectors.Add(nextSector);

            nextSector = AddNeigbor(mapSector, MapGenerator.HexDirection.SW, positions);
            if (nextSector != null) mapSectors.Add(nextSector);

            nextSector = AddNeigbor(mapSector, MapGenerator.HexDirection.W, positions);
            if (nextSector != null) mapSectors.Add(nextSector);
        }

        private MapSector AddNeigbor(MapSector mapSector, MapGenerator.HexDirection hexDirection, List<Position> positions)
        {
            MapSector nextSector = null;
            MapGenerator.HexCell n = mapSector.HexCell.GetNeighbor(hexDirection);
            if (n != null)
            {
                nextSector = HexCellSectors[n];

                if (!positions.Contains(nextSector.Center))
                {
                    positions.Add(nextSector.Center);

                    Position zoneCenter = new Position(nextSector.Center.X + sectorSize / 2, nextSector.Center.Y + sectorSize / 2);
                    //AddZone(nextSector, zoneCenter);
                }
            }
            return nextSector;
        }

        /*
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
        */

        /*
        private void AddZone(Tile largeTile, Position pos)
        {
            int zoneId = Zones.Count;

            MapZone mapZone = new MapZone();
            mapZone.ZoneId = zoneId;
            mapZone.Center = pos;
            //mapZone.HexCell = mapSector.HexCell;

            /*
            mapZone.Tiles = EnumerateTiles(pos, sectorSize / 2, true);
            if (mapZone.Tiles != null)
            {
                foreach (TileWithDistance t in mapZone.Tiles.Values)
                {
                    t.Tile.ZoneId = zoneId;
                }
            }* /
            Zones.Add(mapZone.ZoneId, mapZone);

            if (mapZone.HexCell == null)
            {
                mapZone.MaxMinerals = 40;
                mapZone.StartObjectGenerator(this);
            }
            else
            {
                if (mapZone.HexCell.IsUnderwater)
                {

                }
                else
                {
                    BioMass += 100;

                    mapZone.MaxMinerals = 0; // * sectorSize;
                    mapZone.StartObjectGenerator(this);
                }
            }
        }*/

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

                if (mapZone.Tiles != null && mapZone.Tiles.Count > 0 &&
                    (mapZone.MaxMinerals == -1 || mapZone.TotalMinerals < mapZone.MaxMinerals))
                {
                    int idx = Game.Random.Next(mapZone.Tiles.Count);

                    Tile t = mapZone.Tiles.ElementAt(idx).Value;
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

                            t.TileContainer.Add(tileObject);
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
            if (tileObjects != null)
            {
                move.Stats.MoveUpdateGroundStat.TileObjects.Clear();
                foreach (TileObject tileObject in tileObjects)
                {
                    TileObject newTileObject = tileObject.Copy();
                    move.Stats.MoveUpdateGroundStat.TileObjects.Add(newTileObject);
                }
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
            moveUpdateGroundStat.IsUnderwater = t.IsUnderwater;
            moveUpdateGroundStat.TileObjects = new List<TileObject>();
            moveUpdateGroundStat.TileObjects.AddRange(t.TileContainer.TileObjects);
            moveUpdateGroundStat.Height = (float)t.Height;
            moveUpdateGroundStat.IsOpenTile = t.IsOpenTile;
            moveUpdateGroundStat.ZoneId = t.ZoneId;
            //moveUpdateGroundStat.TerrainTypeIndex = t.TerrainTypeIndex;
            //moveUpdateGroundStat.PlantLevel = t.PlantLevel;

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
