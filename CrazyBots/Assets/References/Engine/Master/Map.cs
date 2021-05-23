﻿using Engine.Interface;
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

namespace Engine.Master
{
    public class MapPlayerInfo
    {
        public int TotalMetal { get; set; }
        public int TotalUnits{ get; set; }
    }
    public class MapInfo
    {
        public MapInfo()
        {
            PlayerInfo = new Dictionary<int, MapPlayerInfo>();
        }
        public int TotalMetal { get; set; }
        public Dictionary<int, MapPlayerInfo> PlayerInfo { get; private set; }
    }
    public class Map
    {
        public Dictionary<Position, Tile> Tiles = new Dictionary<Position, Tile>();
        public Units Units { get; private set; }
        private int seed;
        public Game Game { get; private set; }

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }


        public Map(Game game, int seed)
        {
            this.seed = seed;
            Game = game;
            Units = new Units(this);
            MapWidth = game.GameModel.MapWidth;
            MapHeight = game.GameModel.MapHeight;

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
        private int checkTotalMetal;

        public MapInfo GetMapInfo()
        {
            MapInfo mapInfo = new MapInfo();

            foreach (Tile t in Tiles.Values)
            {
                mapInfo.TotalMetal += t.Metal;
                if (t.Unit != null)
                {
                    int unitMetal = t.Unit.CountMetal();
                    mapInfo.TotalMetal += unitMetal;

                    MapPlayerInfo mapPlayerInfo;
                    if (mapInfo.PlayerInfo.ContainsKey(t.Unit.Owner.PlayerModel.Id))
                    {
                        mapPlayerInfo = mapInfo.PlayerInfo[t.Unit.Owner.PlayerModel.Id];
                    }
                    else
                    {
                        mapPlayerInfo = new MapPlayerInfo();
                        mapInfo.PlayerInfo.Add(t.Unit.Owner.PlayerModel.Id, mapPlayerInfo);
                    }
                    mapPlayerInfo.TotalMetal += unitMetal;
                    mapPlayerInfo.TotalUnits++;
                }
            }

            if (checkTotalMetal != 0)
            {
                if (checkTotalMetal != mapInfo.TotalMetal)
                {
                    //throw new Exception("Metal changed");
                }
            }

            return mapInfo;
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

                        t.Height = terrain.Data[x, y] / 2;

                        /*
                        if (t.Height < 0.05)
                        {

                        }
                        else if (t.Height < 0.1)
                        {

                        }
                        else if (t.Height < 0.3)
                        {
                            t.Metal = Game.Random.Next(2);
                        }
                        else if (t.Height <= 0.7)
                        {
                            t.Metal = Game.Random.Next(3);
                        }
                        else if (t.Height > 0.7)
                        {
                            t.Metal = Game.Random.Next(3);
                        }
                        else if (t.Height > 0.9)
                        {
                            
                        }
                        else
                        {

                        }
                        */
                        if (t.Height >= 0.45 && t.Height <= 0.55)
                        {
                            t.Metal = 0;
                        }
                        else if (t.Height > 0.27 && t.Height < 0.33)
                        {
                            t.Metal = 0;
                        }
                        else
                        {
                            if (Game.Random.Next(80) == 0)
                                //if (t.Pos.X == 30 && t.Pos.Y == 15)
                                t.Metal = 100;
                            else
                                t.Metal = 0; // Game.Random.Next(3);
                        }
                        totalMetal += t.Metal;
                    }
                }
                checkTotalMetal = GetMapInfo().TotalMetal;
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
            var generator1 = new FlatGenerator(200, 200, 1);
            var generator2 = new DSNoiseGenerator(200, 200, seed: seed);

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
