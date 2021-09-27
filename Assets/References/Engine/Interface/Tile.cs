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
        Wood,
        WoodBush,
        BushGras,
        Gras,
        Water,
        Sand
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
        public static TileObjectType GetObjectType(string id)
        {
            if (id == "Mineral") return TileObjectType.Mineral;
            if (id == "Dirt") return TileObjectType.Dirt;
            if (id == "Bush") return TileObjectType.Bush;
            if (id == "Tree") return TileObjectType.Tree;
            return TileObjectType.None;
        }

        internal Tile(Map map, Position pos)
        {
            Map = map;
            Pos = pos;

            TileContainer = new TileContainer();
        }
        public TileContainer TileContainer { get; private set; }

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

            List<TileObject> rotatedTileObjects = randomTileFit.TileObjects;

            for (int i=0; i < 6; i++)
            {
                float score = CalcFitObj(rotatedTileObjects);
                if (score > tileFit.Score)
                {
                    tileFit.Score = score;
                    tileFit.TileObjects = rotatedTileObjects;
                }
                rotatedTileObjects = Rotate(rotatedTileObjects);
            }
            return tileFit;
        }

        internal static List<TileObject> Rotate(List<TileObject> tileObjects)
        {
            List<TileObject> rotatedTileObjects = new List<TileObject>();
            foreach (TileObject tileObject in tileObjects)
            {
                TileObject rotatedTileObject = new TileObject();
                rotatedTileObject.TileObjectType = tileObject.TileObjectType;
                rotatedTileObject.Direction = TurnLeft(tileObject.Direction);
                
                rotatedTileObjects.Add(rotatedTileObject);
            }
            return rotatedTileObjects;
        }

        internal float GetMatchingScore(TileObjectType t1, TileObjectType t2)
        {
            float score = 0;
            if (t1 == t2)
            {
                score = 1;
            }

            if (t1 == TileObjectType.LeaveTree) t1 = TileObjectType.Tree;
            if (t2 == TileObjectType.LeaveTree) t2 = TileObjectType.Tree;

            if ((t1 == TileObjectType.Tree && t2 == TileObjectType.Bush) || (t1 == TileObjectType.Bush && t2 == TileObjectType.Tree))
                score = 1;

            if ((t1 == TileObjectType.Tree && t2 == TileObjectType.Dirt) || (t1 == TileObjectType.Dirt && t2 == TileObjectType.Tree))
                score = 0.5f;

            if ((t1 == TileObjectType.Dirt && t2 == TileObjectType.Bush) || (t1 == TileObjectType.Bush && t2 == TileObjectType.Dirt))
                score = 1;

            if ((t1 == TileObjectType.Dirt && t2 == TileObjectType.Sand) || (t1 == TileObjectType.Sand && t2 == TileObjectType.Dirt))
                score = 0.3f;

            if ((t1 == TileObjectType.Water && t2 == TileObjectType.Sand) || (t1 == TileObjectType.Sand && t2 == TileObjectType.Water))
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
                    foreach (TileObject forwardTileObject in forwardTile.TileContainer.TileObjects)
                    {
                        if (tileObject.Direction == TurnAround(forwardTileObject.Direction))
                        {
                            score += GetMatchingScore(tileObject.TileObjectType, forwardTileObject.TileObjectType);
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
        public int TerrainTypeIndex { get; set; }
        public int PlantLevel { get; set; }

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

        /*
        public void AddMinerals(int mins)
        {
            Map.Zones[ZoneId].TotalMinerals += mins;
            Container.CreateMinerals(mins);
        }*/

        public bool CanBuild()
        {
            if (TileContainer.Minerals > 20)
                return false;

            foreach (TileObject tileObject in TileContainer.TileObjects)
            {
                if (tileObject.TileObjectType == TileObjectType.Bush) return false;
                if (tileObject.TileObjectType == TileObjectType.Tree) return false;
            }
            return true;
        }

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

        public bool CanMoveTo(Tile from)
        {
            if (from.Pos != Pos && from.Height + 0.4f < Height )
            {
                return false;
            }
            foreach (TileObject tileObject in TileContainer.TileObjects)
            {
                if (tileObject.Direction != Direction.C)
                    return false;
            }

            if (IsUnderwater)
                return false;

            if (Minerals >= 20)
            {
                return false;
            }
            return true;
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
