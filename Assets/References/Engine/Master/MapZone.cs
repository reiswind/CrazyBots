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
    internal class MapVegetation
    {
        public MapVegetation(int percent, TileFitType tileFitType)
        {
            Percent = percent;
            TileFitType = tileFitType;
        }
        public int Percent { get; set; }
        public TileFitType TileFitType { get; set; }
    }
    public class MapZone
    {
        public int ZoneId { get; set; }
        //public int TotalMinerals { get; set; }
        public int MaxMinerals { get; set; }
        public ulong Center { get; set; }
        public Player Player { get; set; }

        public bool IsUnderwater;
        public bool UnderwaterTilesCreated;

        internal List<MapVegetation> Vegetation = new List<MapVegetation>();

        public Dictionary<ulong, Tile> Tiles { get; set; }

        private List<Tile> openTiles;

        public void StartObjectGenerator(Map map)
        {
            Tile startTile = map.GetTile(Center);
            if (startTile != null)
            {
                openTiles = new List<Tile>();

                startTile.IsOpenTile = true;
                openTiles.Add(startTile);
            }
            /*
            foreach (Tile n in startTile.Neighbors)
            {
                if (n.TileContainer.Count == 0)
                {
                    n.IsOpenTile = true;
                    openTiles.Add(n);
                }
            }*/
            /*
            while (openTiles.Count > 0)
                CreateTerrainTile(map);*/
        }
        /*
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
        */

        public ulong CreateTerrainTile(Map map)
        {
            //if (map.OpenTileObjects.Count == 0)
            //    return null;

            //if (ZoneId != 2 && ZoneId != 4)
            //    return null;

            if (openTiles == null)
                return Position.Null;

            List<Tile> unopenTiles = new List<Tile>();
            ulong pos = Position.Null;

            if (openTiles.Count > 0)
            {
                TileFit randomTileFit = CreateRandomObjects(map);
                if (randomTileFit == null || randomTileFit.TileObjects == null)
                    return Position.Null;

                // Find best tile
                List<TileFit> bestTilesFit = new List<TileFit>();
                float bestScore = 0;

                foreach (Tile tile in openTiles)
                {
                    if (!tile.IsOpenTile)
                    {
                        // Filled by someone else
                        unopenTiles.Add(tile);
                    }
                    else
                    {
                        TileFit tileFit = tile.CalcFit(tile, randomTileFit);

                        if (bestTilesFit.Count == 0 || tileFit.Score > bestScore)
                        {
                            bestTilesFit.Clear();
                            bestTilesFit.Add(tileFit);
                            bestScore = tileFit.Score;
                        }
                        else if (bestTilesFit.Count > 0 && tileFit.Score == bestScore)
                        {
                            bestTilesFit.Add(tileFit);
                        }
                    }
                }
                foreach (Tile tile in unopenTiles)
                {
                    if (tile.Pos != Center)
                    {
                        if (!map.Game.changedGroundulongs.ContainsKey(tile.Pos))
                            map.Game.changedGroundulongs.Add(tile.Pos, null);

                        tile.IsOpenTile = false;
                        openTiles.Remove(tile);
                    }
                }

                if (bestTilesFit.Count > 0)
                {
                    int rnd = map.Game.Random.Next(bestTilesFit.Count);
                    TileFit bestTileFit = bestTilesFit[rnd];
                    Tile bestTile = bestTileFit.Tile;

                    if (!bestTile.IsOpenTile)
                    {
                        throw new Exception();
                    }

                    //if (HexCell == null)
                    {
                        if (bestTileFit.TileFitType == TileFitType.Water)
                        {
                            bestTile.Height = 0f;
                            bestTile.IsUnderwater = true;
                        }
                        else if (bestTileFit.TileFitType == TileFitType.Sand)
                        {
                            bestTile.Height = 0.1f;
                        }
                        else if (bestTileFit.TileFitType == TileFitType.Stone)
                        {
                            bestTile.Height = 0.15f;
                        }
                        else if (bestTileFit.TileFitType == TileFitType.Gras)
                        {
                            bestTile.Height = 0.2f;
                            //bestTile.TerrainTypeIndex = 1;
                            //bestTile.PlantLevel = 1;
                        }
                        else if (bestTileFit.TileFitType == TileFitType.BushGras)
                        {
                            bestTile.Height = 0.3f;
                            //bestTile.TerrainTypeIndex = 1;
                            //bestTile.PlantLevel = 2;
                        }
                        else if (bestTileFit.TileFitType == TileFitType.TreeBush)
                        {
                            bestTile.Height = 0.4f;
                            //bestTile.TerrainTypeIndex = 3;
                            //bestTile.PlantLevel = 1;
                        }
                        else if (bestTileFit.TileFitType == TileFitType.Tree)
                        {
                            bestTile.Height = 0.5f;
                            //bestTile.TerrainTypeIndex = 3;
                            //bestTile.PlantLevel = 2;
                        }
                        else
                        {
                            bestTile.Height = 1f;
                            //bestTile.TerrainTypeIndex = 4;
                        }

                        bestTile.Height += map.Game.Random.NextDouble() / 50;
                    }
                    bestTile.AddRange(bestTileFit.TileObjects);
                    pos = bestTile.Pos;

                    if (!openTiles.Remove(bestTile))
                    {
                        throw new Exception();
                    }
                    if (!map.Game.changedGroundulongs.ContainsKey(bestTile.Pos))
                        map.Game.changedGroundulongs.Add(bestTile.Pos, null);
                    bestTile.IsOpenTile = false;

                    foreach (Tile n in bestTile.Neighbors)
                    {
                        

                        if (CubePosition.Distance(n.Pos, Center) > 20)
                            continue;

                        // Nothing under water
                        if (n.IsUnderwater)
                            continue;

                        if (openTiles.Count > 80)
                            break;

                        if (!openTiles.Contains(n)) // && n.CanBuild()) // && Tiles.ContainsKey(n.Pos)) // Only in zone (creates circles)
                        {
                            if (n.ZoneId != bestTile.ZoneId)
                            {
                                if (map.Zones[n.ZoneId].IsUnderwater != IsUnderwater)
                                {
                                    //continue;
                                }
                            }
                            /*
                            if (HexCell != null &&
                                n.ZoneId != 0 &&
                                n.ZoneId != bestTile.ZoneId)
                            {
                                if (map.Zones[n.ZoneId].HexCell.TerrainTypeIndex != HexCell.TerrainTypeIndex ||

                                    (map.Zones[n.ZoneId].HexCell.TerrainTypeIndex == HexCell.TerrainTypeIndex && 
                                     map.Zones[n.ZoneId].HexCell.PlantLevel != HexCell.PlantLevel))
                                {
                                    continue;
                                }
                            }
                            */

                            bool allTilesEmpty = true;
                            foreach (TileObject tileObject in n.TileObjects)
                            {
                                //if (TileObject.IsTileObjectTypeGrow(tileObject.TileObjectType))
                                if (tileObject.TileObjectType != TileObjectType.Mineral)
                                    allTilesEmpty = false;
                            }
                            if (allTilesEmpty)
                            {
                                if (!map.Game.changedGroundulongs.ContainsKey(n.Pos))
                                    map.Game.changedGroundulongs.Add(n.Pos, null);

                                n.IsOpenTile = true;
                                openTiles.Add(n);
                            }
                        }
                    }
                }
            }
            return pos;
        }

        internal Direction CreateObjects(List<TileObject> tileObjects, Map map, TileObjectType tileObjectType, Direction direction, int count)
        {
            /*
            int bio = TileObject.GetBioMass(tileObjectType);

            if (bio * count > map.BioMass)
            {
                return Direction.C;
            }
            */
            while (count-- > 0)
            {
                //map.BioMass -= bio;

                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = tileObjectType;
                tileObject.Direction = direction;
                tileObjects.Add(tileObject);

                direction = AntPartEngine.TurnLeft(direction);

            }
            return direction;
        }


        internal List<TileObject> CreateTreeObjects(Map map, int count)
        {
            List<TileObject> tileObjects = new List<TileObject>();

            TileObjectType tileObjectType = TileObjectType.Tree;
            int rnd = map.Game.Random.Next(1);
            if (rnd == 0)
                tileObjectType = TileObjectType.Tree;

            Direction direction = Direction.N;
            direction = CreateObjects(tileObjects, map, tileObjectType, direction, count);
            if (direction != Direction.C)
                return tileObjects;

            return null;
        }

        internal List<TileObject> CreateTreeToBushObjects(Map map)
        {
            List<TileObject> tileObjects = new List<TileObject>();

            Direction direction = Direction.N;
            direction = CreateObjects(tileObjects, map, TileObjectType.Tree, direction, 3);
            if (direction != Direction.C)
                direction = CreateObjects(tileObjects, map, TileObjectType.Bush, direction, 3);
            if (direction != Direction.C)
                return tileObjects;

            return null;
        }

        internal List<TileObject> CreateBushToGrasObjects(Map map)
        {
            List<TileObject> tileObjects = new List<TileObject>();

            Direction direction = Direction.N;
            direction = CreateObjects(tileObjects, map, TileObjectType.Bush, direction, 3);
            if (direction != Direction.C)
                direction = CreateObjects(tileObjects, map, TileObjectType.Gras, direction, 3);
            if (direction != Direction.C)
                return tileObjects;

            return null;
        }

        internal List<TileObject> CreateObjects(Map map, TileObjectType tileObjectType, int count)
        {
            List<TileObject> tileObjects = new List<TileObject>();

            Direction direction = Direction.N;
            direction = CreateObjects(tileObjects, map, tileObjectType, direction, count);
            if (direction != Direction.C)
                return tileObjects;

            if (tileObjects.Count == 0) tileObjects = null;
            return tileObjects;
        }

        internal TileFit CreateRandomObjects(Map map)
        {
            TileFit tileFit = new TileFit();
            List<TileObject> tileObjects = new List<TileObject>();

            int rnd = map.Game.Random.Next(100);
            int currentPercent = 0;
            foreach (MapVegetation mapVegetation in Vegetation)
            {
                currentPercent += mapVegetation.Percent;
                if (rnd < currentPercent)
                {
                    int count = 0;
                    Direction direction = Direction.N;
                    tileFit.TileFitType = mapVegetation.TileFitType;

                    TileObjectType tileObjectType = TileObjectType.None;

                    if (mapVegetation.TileFitType == TileFitType.Water)
                    {
                        count = 1;
                        direction = Direction.C;
                        tileObjectType = TileObjectType.Water;
                    }
                    else if (mapVegetation.TileFitType == TileFitType.Sand)
                    {
                        count = 1;
                        direction = Direction.C;
                        tileObjectType = TileObjectType.Sand;
                    }
                    else if (mapVegetation.TileFitType == TileFitType.Stone)
                    {
                        count = map.Game.Random.Next(3)+1;
                        tileObjectType = TileObjectType.Rock;
                    }
                    else if (mapVegetation.TileFitType == TileFitType.Tree)
                    {
                        count = 4;
                        tileObjectType = TileObjectType.Tree;
                    }
                    else if (mapVegetation.TileFitType == TileFitType.BushGras)
                    {
                        direction = CreateObjects(tileObjects, map, TileObjectType.Gras, direction, 2);
                        count = 2;
                        tileObjectType = TileObjectType.Bush;
                    }
                    else if (mapVegetation.TileFitType == TileFitType.TreeBush)
                    {
                        direction = CreateObjects(tileObjects, map, TileObjectType.Bush, direction, 2);
                        count = 2;
                        tileObjectType = TileObjectType.Tree;
                    }
                    else if (mapVegetation.TileFitType == TileFitType.Gras)
                    {
                        count = 6;
                        tileObjectType = TileObjectType.Gras;

                        return CreateGrasObjects(map);
                    }
                    CreateObjects(tileObjects, map, tileObjectType, direction, count);
                    break;
                }
            }
            tileFit.TileObjects = tileObjects;
            return tileFit;
        }

        internal TileFit CreateRandomTreeObjects(Map map)
        {
            TileFit tileFit = new TileFit();
            List<TileObject> tileObjects;

            tileFit.TileFitType = TileFitType.Tree;
            tileObjects = CreateObjects(map, TileObjectType.Tree, 3);

            int rndKind = map.Game.Random.Next(16);
            if (rndKind == 1 && tileObjects != null)
            {
                foreach (TileObject tileObject in tileObjects)
                    tileObject.TileObjectKind = TileObjectKind.LeaveTree;
            }


            tileFit.TileObjects = tileObjects;
            return tileFit;
        }

        internal TileFit CreateGrasObjects(Map map)
        {
            TileFit tileFit = new TileFit();
            List<TileObject> tileObjects;

            tileFit.TileFitType = TileFitType.Gras;
            tileObjects = CreateObjects(map, TileObjectType.Gras, 6);

            int rndKind = map.Game.Random.Next(10);
            if (rndKind == 1)
            {
                tileObjects[0].TileObjectKind = TileObjectKind.LightGras;
                tileObjects[1].TileObjectKind = TileObjectKind.LightGras;
                tileObjects[2].TileObjectKind = TileObjectKind.LightGras;
            }
            if (rndKind == 2)
            {
                tileObjects[0].TileObjectKind = TileObjectKind.DarkGras;
                tileObjects[1].TileObjectKind = TileObjectKind.DarkGras;
                tileObjects[2].TileObjectKind = TileObjectKind.DarkGras;
            }

            TileObject tileObject = new TileObject();
            tileObject.TileObjectType = TileObjectType.Gras;
            tileObject.Direction = Direction.C;
            tileObjects.Add(tileObject);

            tileFit.TileObjects = tileObjects;
            return tileFit;
        }

        internal TileFit CreateRandomGrasObjects(Map map)
        {
            TileFit tileFit = new TileFit();
            List<TileObject> tileObjects = null;

            for (int i = 0; i < 4 && tileObjects == null; i++)
            {
                int rnd = map.Game.Random.Next(8);

                if (map.BioMass <= 3)
                {
                    // Sprinkles sand
                    if (rnd != 7)
                        rnd = 3;
                }
                if (rnd == 0)
                {
                    tileFit.TileFitType = TileFitType.Tree;
                    tileObjects = CreateTreeObjects(map, 6);

                    int rndKind = map.Game.Random.Next(5);
                    if (rndKind == 1 && tileObjects != null)
                    {
                        foreach (TileObject tileObject in tileObjects)
                            tileObject.TileObjectKind = TileObjectKind.LeaveTree;
                    }
                }
                else if (rnd == 1)
                {
                    tileFit.TileFitType = TileFitType.TreeBush;
                    tileObjects = CreateTreeToBushObjects(map);

                    int rndKind = map.Game.Random.Next(5);
                    if (rndKind == 1 && tileObjects != null)
                    {
                        foreach (TileObject tileObject in tileObjects)
                        {
                            if (tileObject.TileObjectType == TileObjectType.Tree)
                                tileObject.TileObjectKind = TileObjectKind.LeaveTree;
                        }
                    }
                }
                else if (rnd == 2)
                {
                    tileFit.TileFitType = TileFitType.BushGras;
                    tileObjects = CreateBushToGrasObjects(map);
                }

                else if (rnd >= 3 && rnd < 5)
                {
                    tileFit.TileFitType = TileFitType.Gras;
                    tileObjects = CreateObjects(map, TileObjectType.Gras, 6);

                    int rndKind = map.Game.Random.Next(12);
                    if (rndKind == 1)
                    {
                        tileObjects[0].TileObjectKind = TileObjectKind.LightGras;
                        tileObjects[1].TileObjectKind = TileObjectKind.LightGras;
                        tileObjects[2].TileObjectKind = TileObjectKind.LightGras;
                    }
                    if (rndKind == 2)
                    {
                        tileObjects[0].TileObjectKind = TileObjectKind.DarkGras;
                        tileObjects[1].TileObjectKind = TileObjectKind.DarkGras;
                        tileObjects[2].TileObjectKind = TileObjectKind.DarkGras;
                    }

                    TileObject tileObject = new TileObject();
                    tileObject.TileObjectType = TileObjectType.Gras;
                    tileObject.Direction = Direction.C;
                    tileObjects.Add(tileObject);
                }
                else if (rnd == 6)
                {
                    tileFit.TileFitType = TileFitType.Water;
                    tileObjects = CreateObjects(map, TileObjectType.Water, 6);
                }
                else if (rnd == 7)
                {
                    tileFit.TileFitType = TileFitType.Sand;
                    tileObjects = CreateObjects(map, TileObjectType.Sand, 6);
                }
            }
            tileFit.TileObjects = tileObjects;
            return tileFit;
        }
    }
}
