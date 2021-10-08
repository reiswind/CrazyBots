using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class TileWithDistance 
    {
        private Tile tile;
        internal TileWithDistance(Tile tile, int distance) 
        {
            this.tile = tile;
            Distance = distance;
        }

        public Tile Tile
        {
            get
            {
                return tile;
            }
        }

        public Position Pos
        {
            get
            {
                return tile.Pos;
            }
        }

        public Unit Unit
        {
            get
            {
                return tile.Unit;
            }
        }

        public List<Tile> Neighbors
        {
            get
            {
                return tile.Neighbors;
            }     
        }

        public int Minerals
        {
            get
            {
                return tile.Minerals;
            }
        }

        public int ZoneId
        {
            get
            {
                return tile.ZoneId;
            }
        }

        public override string ToString()
        {
            if (this.Unit != null)
            {
                return this.Unit.ToString();
            }
            return "Tile: " + Pos.X + "," + Pos.Y + " : " + Distance;
        }

        public int Distance { get; set; }
    }

    internal enum TileFitType
    {
        Tree,
        TreeBush,
        BushGras,
        Gras,
        Water,
        Sand,
        Stone
    }

    internal class TileFit
    {
        public TileFit()
        {
            
        }
        public TileFit(Tile t)
        {
            Tile = t;
        }
        public TileFitType TileFitType { get; set; }
        public Tile Tile { get; set; }
        public float Score { get; set; }
        public List<TileObject> TileObjects { get; set; }
    }

    public class Tile
    {
        /*
        public static TileObjectType GetObjectType(string id)
        {
            if (id == "Mineral") return TileObjectType.Mineral;
            if (id == "Dirt") return TileObjectType.Dirt;
            if (id == "Gras") return TileObjectType.Gras;
            if (id == "Bush") return TileObjectType.Bush;
            if (id == "Tree") return TileObjectType.Tree;
            if (id == "Sand") return TileObjectType.Sand;
            return TileObjectType.None;
        }*/

        internal Tile(Map map, Position pos)
        {
            Map = map;
            Pos = pos;

            TileContainer = new TileContainer();
        }
        public TileContainer TileContainer { get; private set; }

        internal int Count(TileObjectType tileObjectType)
        {
            int count = 0;
            foreach (TileObject tileObject in TileContainer.TileObjects)
            {
                if (tileObject.TileObjectType == tileObjectType)
                {
                    count++;
                }
            }
            return count;
        }

        internal void HitByBullet(TileObject bulletTileObject)
        {
            if (bulletTileObject.TileObjectType == TileObjectType.Dirt)
            {
                Height += 0.1f;
            }
            else
            {
                // Anything but minerals are distributed
                if (bulletTileObject.TileObjectType != TileObjectType.Mineral)
                {
                    Map.AddOpenTileObject(bulletTileObject);
                }
                else
                {
                    // Minerals stay on hit tile
                    TileContainer.Add(bulletTileObject);
                }
            }
            RemoveBio();
        }

        internal bool RemoveBio()
        {
            bool changed = false;

            foreach (TileObject tileObject in TileContainer.TileObjects)
            {
                if (tileObject.TileObjectType == TileObjectType.Gras ||
                    tileObject.TileObjectType == TileObjectType.TreeTrunk)
                {
                    TileContainer.Remove(tileObject);
                    changed = true;
                    break;
                }
            }
            //if (changed)
            //    AdjustTerrainType();
            return changed;
        }

        internal void ExtractTileObject(TileObject removedTileObject)
        {
            foreach (TileObject tileObject in TileContainer.TileObjects)
            {
                if (tileObject.TileObjectType == removedTileObject.TileObjectType)
                {
                    if (tileObject.TileObjectType == TileObjectType.Tree)
                    {
                        tileObject.TileObjectType = TileObjectType.TreeTrunk;
                        Map.BioMass++;
                    }
                    else if (tileObject.TileObjectType == TileObjectType.Bush)
                    {
                        tileObject.TileObjectType = TileObjectType.Gras;
                        Map.BioMass++;
                    }
                    else
                    {
                        TileContainer.Remove(tileObject);
                    }

                    break;
                }
            }
        }
        
        internal static Direction TurnAround(Direction direction)
        {
            if (direction == Direction.N) return Direction.S;
            if (direction == Direction.NE) return Direction.SW;
            if (direction == Direction.SE) return Direction.NW;
            if (direction == Direction.S) return Direction.N;
            if (direction == Direction.SW) return Direction.NE;
            if (direction == Direction.NW) return Direction.SE;
            return Direction.C;
        }
        internal static Direction TurnLeft(Direction direction)
        {
            if (direction == Direction.N) return Direction.NW;
            if (direction == Direction.NW) return Direction.SW;
            if (direction == Direction.SW) return Direction.S;
            if (direction == Direction.S) return Direction.SE;
            if (direction == Direction.SE) return Direction.NE;
            if (direction == Direction.NE) return Direction.N;
            return Direction.C;
        }
        internal static Direction TurnRight(Direction direction)
        {
            if (direction == Direction.N) return Direction.NE;
            if (direction == Direction.NE) return Direction.SE;
            if (direction == Direction.SE) return Direction.S;
            if (direction == Direction.S) return Direction.SW;
            if (direction == Direction.SW) return Direction.NW;
            if (direction == Direction.NW) return Direction.N;
            return Direction.C;
        }

        internal TileFit CalcFit(Tile openTile, TileFit randomTileFit)
        {
            TileFit tileFit = new TileFit(openTile);
            tileFit.Score = -1;
            tileFit.TileFitType = randomTileFit.TileFitType;
            tileFit.TileObjects = new List<TileObject>();
            foreach (TileObject tileObject in randomTileFit.TileObjects)
            {
                TileObject rotatedTileObject = tileObject.Copy();
                rotatedTileObject.Direction = TurnLeft(tileObject.Direction);

                tileFit.TileObjects.Add(rotatedTileObject);
            }

            int bestRotation = 0;
            for (int i=0; i < 6; i++)
            {
                float score = CalcFitObj(tileFit.TileObjects);
                if (score > tileFit.Score)
                {
                    bestRotation = i;
                    tileFit.Score = score;
                }
                Rotate(tileFit.TileObjects);
            }
            while (bestRotation-- > 0)
                Rotate(tileFit.TileObjects);
            return tileFit;
        }

        internal static void Rotate(List<TileObject> tileObjects)
        {
            foreach (TileObject tileObject in tileObjects)
            {
                tileObject.Direction = TurnLeft(tileObject.Direction);
            }
        }

        internal float GetMatchingScore(TileObjectType t1, TileObjectKind k1, TileObjectType t2, TileObjectKind k2)
        {
            float score = 0;
            if (t1 == t2)
            {
                if (k1 == k2)
                    score = 2;
                else
                    score = 0.7f;
            }

            else if ((t1 == TileObjectType.Tree && t2 == TileObjectType.Bush) || (t1 == TileObjectType.Bush && t2 == TileObjectType.Tree))
                score = 1;

            else if ((t1 == TileObjectType.Tree && t2 == TileObjectType.Gras) || (t1 == TileObjectType.Gras && t2 == TileObjectType.Tree))
                score = 0.5f;

            else if ((t1 == TileObjectType.Gras && t2 == TileObjectType.Bush) || (t1 == TileObjectType.Bush && t2 == TileObjectType.Gras))
                score = 1;

            else if ((t1 == TileObjectType.Gras && t2 == TileObjectType.Sand) || (t1 == TileObjectType.Sand && t2 == TileObjectType.Gras))
                score = 0.3f;

            else if ((t1 == TileObjectType.Water && t2 == TileObjectType.Sand) || (t1 == TileObjectType.Sand && t2 == TileObjectType.Water))
                score = 1;

            return score;
        }

        internal float GetScoreForPos(TileObject tileObject, Position position)
        {
            float score = 0;

            if (position != null)
            {
                Tile forwardTile = Map.GetTile(position);
                if (forwardTile != null && forwardTile.TileContainer != null)
                {
                    Direction backDirection = TurnAround(tileObject.Direction);

                    foreach (TileObject forwardTileObject in forwardTile.TileContainer.TileObjects)
                    {
                        if (forwardTileObject.Direction == backDirection)
                        {
                            score += GetMatchingScore(tileObject.TileObjectType, tileObject.TileObjectKind, forwardTileObject.TileObjectType, forwardTileObject.TileObjectKind);
                            if (score > 0)
                            {
                                if (tileObject.TileObjectKind == forwardTileObject.TileObjectKind)
                                    score += 0.4f;
                            }
                        }
                    }
                }
            }
            return score;
        }

        internal float CalcFitObj(List<TileObject> tileObjects)
        {
            float score = 0;

            foreach (TileObject tileObject in tileObjects)
            {
                Position pos = Ants.AntPartEngine.GetPositionInDirection(Pos, tileObject.Direction);
                score += GetScoreForPos(tileObject, pos);

                pos = Ants.AntPartEngine.GetPositionInDirection(Pos,TurnLeft( tileObject.Direction));
                score += GetScoreForPos(tileObject, pos);

                pos = Ants.AntPartEngine.GetPositionInDirection(Pos, TurnRight(tileObject.Direction));
                score += GetScoreForPos(tileObject, pos);
            }
            return score;
        }


        internal Map Map { get; set; }

        public Position Pos { get; set; }
        private List<Tile> neighbors;

        public double Height { get; set; }

        // Debug
        public bool IsOpenTile { get; set; }

        public int Minerals
        {  
            get
            {
                return TileContainer.Minerals;
            }
        }
        public int ZoneId { get; set; }
        
        public bool IsUnderwater { get; set; }

        public int Owner { get; set; }

        public bool IsBorder { get; set; }



        public bool HasCollectableTileObjects
        {
            get
            {
                foreach (TileObject tileObject in TileContainer.TileObjects)
                {
                    if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                        return true;
                }
                return false;
            }
        }


        internal Unit Unit
        {
            get
            {
                return Map.Units.GetUnitAt(Pos);
            }
        }
        
        public bool CanMoveTo(Position from)
        {
            return CanMoveTo(Map.GetTile(from));            
        }

        public bool IsNeighbor(Position pos)
        {
            foreach (Tile n in neighbors)
            {
                if (n.Pos == pos)
                    return true;
            }
            return false;
        }        

        public bool CanBuild()
        {
            if (IsUnderwater)
                return false;

            int mins = 0;
            foreach (TileObject tileObject in TileContainer.TileObjects)
            {
                if (tileObject.TileObjectType == TileObjectType.Mineral)
                {
                    mins++;
                }
                else if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                    return false;
                else if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                    return false;
            }

            if (mins >= 20)
            {
                return false;
            }
            return true;
        }

        public bool CanMoveTo(Tile from)
        {
            if (from.Pos != Pos && from.Height + 0.4f < Height )
            {
                return false;
            }
            return CanBuild();
        }
        public override string ToString()
        {
            if (this.Unit != null)
            {
                return this.Unit.ToString();
            }
            return "Tile: " + Pos.X + "," + Pos.Y;
        }

        private void AddCube (int q, int r, int s)
        {
            CubePosition c1 = new CubePosition(q,r,s);
            if (c1.IsValid(Map))
            {
                /*if (Math.Abs(c1.q) <= Map.Model.MapHeight &&
                    Math.Abs(c1.r) <= Map.Model.MapHeight &&
                    Math.Abs(c1.s) <= Map.Model.MapHeight)
                {*/
                Tile t = Map.GetTile(c1.Pos);
                if (t != null)
                    neighbors.Add(t);
            }
        }
        /*
        public Tile TileBelow
        {
            get
            {
                CubePosition cube = this.Pos.GetCubePosition();
                CubePosition c1 = new CubePosition(cube.q, cube.r - 1, cube.s + 1);
                if (c1.IsValid(Map))
                {
                    return Map.GetTile(c1.Pos);
                }
                return null;
            }
        }

        public Tile TileBelowRight
        {
            get
            {
                CubePosition cube = this.Pos.GetCubePosition();
                CubePosition c1 = new CubePosition(cube.q + 1, cube.r - 1, cube.s);
                if (c1.IsValid(Map))
                {
                    return Map.GetTile(c1.Pos);
                }
                return null;
            }
        }*/
        public void CreateCube(Dictionary<Position, Tile> createTiles, Dictionary<Position, Tile> tiles, int q, int r, int s)
        {
            CubePosition c1 = new CubePosition(q, r, s);
            Tile t;
            if (!tiles.TryGetValue(c1.Pos, out t))
            {
                t = new Tile(Map, c1.Pos);
                tiles.Add(t.Pos, t);
                createTiles.Add(t.Pos, t);
            }
            neighbors.Add(t);
        }
        public void CreateNeighborsxx(Dictionary<Position, Tile> createTiles, Dictionary<Position, Tile> tiles)
        {
            neighbors = new List<Tile>();

            CubePosition cube = Pos.GetCubePosition();
            CreateCube(createTiles, tiles, cube.q + 1, cube.r - 1, cube.s); // N 11,10
            CreateCube(createTiles, tiles, cube.q + 1, cube.r, cube.s - 1); // NE 11,9
            CreateCube(createTiles, tiles, cube.q, cube.r + 1, cube.s - 1); // NW 10,9

            CreateCube(createTiles, tiles, cube.q - 1, cube.r, cube.s + 1); // S 9,10
            CreateCube(createTiles, tiles, cube.q - 1, cube.r + 1, cube.s); // SE 9,9
            CreateCube(createTiles, tiles, cube.q, cube.r - 1, cube.s + 1);

        }
        public void CreateNeighbors(Dictionary<Position, Tile> createTiles, Dictionary<Position, Tile> tiles)
        {
            neighbors = new List<Tile>();

            CubePosition cube = Pos.GetCubePosition();
            CreateCube(createTiles, tiles, cube.q + 1, cube.r - 1, cube.s);
            CreateCube(createTiles, tiles, cube.q + 1, cube.r, cube.s - 1);
            CreateCube(createTiles, tiles, cube.q, cube.r + 1, cube.s - 1);
            CreateCube(createTiles, tiles, cube.q - 1, cube.r + 1, cube.s);
            CreateCube(createTiles, tiles, cube.q - 1, cube.r, cube.s + 1);
            CreateCube(createTiles, tiles, cube.q, cube.r - 1, cube.s + 1);
        }

        public List<Tile> Neighbors
        {
            get
            {
                if (neighbors == null)
                {
                    neighbors = new List<Tile>();

                    CubePosition cube = this.Pos.GetCubePosition();
                    AddCube(cube.q + 1, cube.r - 1, cube.s);
                    AddCube(cube.q + 1, cube.r, cube.s - 1);
                    AddCube(cube.q, cube.r + 1, cube.s - 1);
                    AddCube(cube.q - 1, cube.r + 1, cube.s);
                    AddCube(cube.q - 1, cube.r, cube.s + 1);
                    AddCube(cube.q, cube.r - 1, cube.s + 1);

                }
                return neighbors;
            }
        }
    }
}
