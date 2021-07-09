using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class CubePosition
    {
        public CubePosition()
        {

        }

        public CubePosition(int q, int r, int s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
        }
        /// <summary>
        /// x
        /// </summary>
        public int q { get; set; } // x
        /// <summary>
        /// y
        /// </summary>
        public int r { get; set; } // y
        /// <summary>
        /// z
        /// </summary>
        public int s { get; set; } // z

        public void MoveRightDown(Map map, int range)
        {
            q += range;
            r -= range;
            while (!IsValid(map))
            {
                q--;
                r++;
            }
        }
        public void MoveLeftUp(Map map, int range)
        {
            q -= range;
            r += range;
            while (!IsValid(map))
            {
                q++;
                r--;
            }
        }
        public void MoveRightUp(Map map, int range)
        {
            q += range;
            s -= range;
            while (!IsValid(map))
            {
                q--;
                s++;
            }
        }
        public void MoveLeftDown(Map map, int range)
        {
            q -= range;
            s += range;
            while (!IsValid(map))
            {
                q++;
                s--;
            }
        }
        public void MoveUp(Map map, int range)
        {
            r += range;
            s -= range;
            while (!IsValid(map))
            {
                r--;
                s++;
            }
        }
        public void MoveDown(Map map, int range)
        {
            r -= range;
            s += range;
            while (!IsValid(map))
            {
                r++;
                s--;
            }
        }

        public bool IsValid(Map map)
        {
            return true;
            /*
            if (Math.Abs(q) <= map.Model.MapHeight &&
                Math.Abs(r) <= map.Model.MapHeight &&
                Math.Abs(s) <= map.Model.MapHeight)
            {
                return true;
            }
            return false;*/
        }

        public Position Pos
        {
            get
            {
                return new Position(q, s + (q - (q & 1)) / 2);
            }
        }
    }

    public class Position
    {
        public Position()
        {

        }
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Position(Position p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        private CubePosition cube;
        public CubePosition GetCubePosition()
        {
                //if (cube == null)
                {
                    cube = new CubePosition();
                    cube.q = X;
                    cube.s = Y - (X - (X & 1)) / 2;
                    cube.r = -cube.q - cube.s;
                }

                //q = x;
                //r = y - (x - (x & 1)) / 2;
                //s = -q - r;

                return cube;
            
        }

        public double GetDistanceTo(Position pos)
        {
            double x;
            double y;

            x = pos.X - X;
            y = pos.Y - Y;

            if (pos.X % 2 != 0 && X % 2 == 0)
            {
                y += 0.5;
            }
            return Math.Sqrt(x * x +  y * y);
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Position p = obj as Position;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (X == p.X) && (Y == p.Y);
        }
        public bool Equals(Position p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.X) && (y == p.Y);
        }

        public static bool operator ==(Position a, Position b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Position a, Position b)
        {
            return !(a == b);
        }



        private int x;
        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
                cube = null;
            }
        }

        private int y;
        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                cube = null;
            }
        }
        private int z;
        public int Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
                cube = null;
            }
        }

        public override int GetHashCode()
        {
            return (y * 1000000) + (x * 1000) + z;
        }
        public override string ToString()
        {
            return x.ToString() + ", " + y.ToString();
        }
    }
}
