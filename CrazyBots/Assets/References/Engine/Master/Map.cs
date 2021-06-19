using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrainQuest.Generator;
using TerrainQuest.Generator.Blending;
using TerrainQuest.Generator.Generators;
using TerrainQuest.Generator.Generators.Noise;
using TerrainQuest.Generator.Graph;

namespace Engine.Interface
{
    public class MapPlayerInfo
    {
        public int TotalCapacity { get; set; }
        public int TotalMetal { get; set; }
        public int TotalUnits { get; set; }
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

        internal void ComputeMapInfo(Game game)
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
                TotalMetal += t.Metal;
                if (t.Unit != null)
                {
                    TotalMetal += t.Unit.CountMetal();

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
    public class Map
    {
        public Dictionary<Position, Tile> Tiles = new Dictionary<Position, Tile>();
        public Units Units { get; private set; }

        private int seed;
        public Game Game { get; private set; }

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }

        private int zoneCounter;
        private int zoneWidth;
        private int maxZones;

        public int DefaultMinerals
        {
            get
            {
                return maxZones * 20;
            }
        }


        public Map(Game game, int seed)
        {
            this.seed = seed;
            Game = game;
            Units = new Units(this);
            MapWidth = game.GameModel.MapWidth;
            MapHeight = game.GameModel.MapHeight;

            zoneWidth = (MapWidth / 10);
            maxZones = zoneWidth * (MapHeight / 10) - 1;

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

        private HeightMap terrain;

        public void DistributeMineral()
        {
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

                    t.Metal++;
                    retryZones = 0;
                    break;
                }
            }
        }

        public Tile GetTile(Position pos)
        {
            if (Tiles.ContainsKey(pos))
                return Tiles[pos];

            if (terrain == null)
            {
                int totalMetal;

                totalMetal = 0;
                Tiles.Clear();
                terrain = GenerateTerrain(seed);

                for (int x = 0; x < Game.GameModel.MapWidth; x++)
                {
                    for (int y = 0; y < Game.GameModel.MapHeight; y++)
                    {
                        Position p = new Position(x, y);
                        Tile t = new Tile(this, p);
                        Tiles.Add(p, t);

                        t.Height = terrain.Data[x, y];

                        if (t.IsDarkWood())
                        {
                            if (Game.Random.Next(8) == 1)
                            {
                                t.NumberOfDestructables = 1;
                                //t.Metal = 1;
                            }
                        }
                        else if (t.IsWood())
                        {
                            if (Game.Random.Next(14) == 0)
                            {
                                t.NumberOfDestructables = 3;
                                //t.Metal = 1;
                            }
                        }
                        else if (t.IsLightWood())
                        {
                            if (Game.Random.Next(25) == 0)
                            {
                                t.NumberOfDestructables = 4;
                                //t.Metal = 1;
                            }
                        }
                        else if (t.IsDarkSand())
                        {
                            if (Game.Random.Next(25) == 0)
                            {
                                t.NumberOfDestructables = 4;
                                //t.Metal = 4;
                            }
                        }
                        else if (t.IsSand())
                        {
                            if (Game.Random.Next(30) == 0)
                            {
                                t.NumberOfDestructables = 3;
                                //t.Metal = 3;
                            }
                            else if (Game.Random.Next(20) == 0)
                            {
                                t.NumberOfObstacles = 2;
                                //t.Metal = 2;
                            }
                        }
                        else if (t.IsGrassDark())
                        {
                            //if (Game.Random.Next(30) == 0)
                            //    t.Metal = 20;
                        }
                        else if (t.IsGras())
                        {
                            //if (Game.Random.Next(20) == 0)
                            //    t.Metal = 20;
                        }

                        
                        totalMetal += t.Metal;
                    }
                }
                //checkTotalMetal = GetMapInfo().TotalMetal;
            }
            if (Tiles.ContainsKey(pos))
                return Tiles[pos];

            return null;
            /* Dynamically expand. Not now
            Tile tile = new Tile(this, pos);
            Tiles.Add(pos, tile);

            return tile;
            */
        }

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
        }


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
