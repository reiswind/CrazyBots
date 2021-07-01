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

        public int Metal
        {
            get
            {
                return tile.Metal;
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

    public class Tile
    {
        internal Map Map;

        public Position Pos { get; set; }
        private List<Tile> neighbors;

        public double Height { get; set; }

        private int minerals;
        public int Metal 
        {  
            get
            {
                return minerals;
            }
        }
        public int ZoneId { get; set; }
        public int Plates { get; set; }

        public int NumberOfDestructables { get; set; }
        public int NumberOfObstacles { get; set; }

        public int Owner { get; set; }

        public bool IsBorder { get; set; }

        public void AddMinerals(int mins)
        {
            Map.Zones[ZoneId].TotalMinerals += mins;
            minerals += mins;

        }

        internal Tile(Map map, Position pos)
        {
            Map = map;
            Pos = pos;
        }

        /*
        internal Unit UnitInBuild
        {
            get
            {
                return Map.UnitsInBuild.GetUnitAt(Pos);
            }
        }*/

        internal Unit Unit
        {
            get
            {
                return Map.Units.GetUnitAt(Pos);
            }
        }

        public bool IsDarkWood()
        {
            return Height > 0.48 && Height <= 0.53;
        }

        public bool IsWood()
        {
            return ((Height > 0.41 && Height <= 0.48) || (Height > 0.53 && Height <= 0.60));
        }

        public bool IsLightWood()
        {
            return ((Height > 0.34 && Height <= 0.41) || (Height > 0.60 && Height <= 0.67));
            //return Height >= 0.40 && Height <= 0.47;
        }
        public bool IsGrassDark()
        {
            return ((Height > 0.27 && Height <= 0.34) || (Height > 0.67 && Height <= 0.74));
            //return Height >= 0.35 && Height <= 0.40;
        }
        public bool IsGras()
        {
            return ((Height > 0.20 && Height <= 0.27) || (Height > 0.74 && Height <= 0.81));
            //return Height >= 0.26 && Height <= 0.32;
        }
        public bool IsDarkSand()
        {
            return ((Height > 0.13 && Height <= 0.20) || (Height > 0.81 && Height <= 0.88));
            //return Height >= 0.26 && Height <= 0.32;
        }
        public bool IsSand()
        {
            return ((Height > 0.0 && Height <= 0.13) || (Height > 0.88 && Height <= 1));
            //return Height > 0.27 && Height < 0.33;
        }
        

        public bool CanMoveTo()
        {
            if (NumberOfObstacles > 0 || NumberOfDestructables > 0)
            {
                return false;
            }
            
            if (Metal >= 20)
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
