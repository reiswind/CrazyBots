using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public enum Direction
    {
        C = 6,
        N = 5,
        S = 2,
        NE = 0,
        NW = 4,
        SE = 1,
        SW = 3
    }

    public class CubePosition
    {
        public CubePosition()
        {

        }
        public CubePosition(ulong pos)
        {
            int pX = Position.GetX(pos);
            int pY = Position.GetY(pos);
            Q = pX;
            S = pY - (pX + (pX & 1)) / 2;
            R = -Q - S;
        }
        public CubePosition(int q, int r, int s)
        {
            Q = q;
            R = r;
            S = s;
        }
        /// <summary>
        /// x
        /// </summary>
        public int Q { get; private set; } 
        /// <summary>
        /// y
        /// </summary>
        public int R { get; private set; } 
        /// <summary>
        /// z
        /// </summary>
        public int S { get; private set; }

        /*
        public void MoveRightDown(Map map, int range)
        {
            Q += range;
            R -= range;
            while (!IsValid(map))
            {
                Q--;
                R++;
            }
        }
        public void MoveLeftUp(Map map, int range)
        {
            Q -= range;
            R += range;
            while (!IsValid(map))
            {
                Q++;
                R--;
            }
        }
        public void MoveRightUp(Map map, int range)
        {
            Q += range;
            S -= range;
            while (!IsValid(map))
            {
                Q--;
                S++;
            }
        }
        public void MoveLeftDown(Map map, int range)
        {
            Q -= range;
            S += range;
            while (!IsValid(map))
            {
                Q++;
                S--;
            }
        }
        public void MoveUp(Map map, int range)
        {
            R += range;
            S -= range;
            while (!IsValid(map))
            {
                R--;
                S++;
            }
        }
        public void MoveDown(Map map, int range)
        {
            R -= range;
            S += range;
            while (!IsValid(map))
            {
                R++;
                S--;
            }
        }*/
        public CubePosition Move(Direction direction, int lenght)
        {
            CubePosition cubePosition = this;
            while (lenght-- > 0)
                cubePosition = cubePosition.GetNeighbor(direction);
            return cubePosition;
        }

        public List<CubePosition> DrawLine(ulong pos)
        {
            CubePosition to = new CubePosition(pos);
            return DrawLine(to);
        }
        public List<CubePosition> DrawLine(CubePosition to)
        { 
            List<CubePosition> line = FractionalHex.HexLinedraw(this, to);
            return line;
        }
        public List<CubePosition> CreateRing(int radius)
        {
            List<CubePosition> results = new List<CubePosition>();
            CubePosition cube = Move(Direction.NW, radius);

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    results.Add(cube);
                    cube = cube.GetNeighbor(i);
                }
            }
            return results;
        }

        public CubePosition Add(CubePosition b)
        {
            return new CubePosition(Q + b.Q, R + b.R, S + b.S);
        }
        public CubePosition Add(int q1, int r1, int s1)
        {
            return new CubePosition(Q + q1, R + r1, S + s1);
        }

        public CubePosition Subtract(CubePosition b)
        {
            return new CubePosition(Q - b.Q, R - b.R, S - b.S);
        }

        public CubePosition Scale(int k)
        {
            return new CubePosition(Q * k, R * k, S * k);
        }

        public CubePosition RotateLeft()
        {
            return new CubePosition(-S, -Q, -R);
        }

        public CubePosition RotateRight()
        {
            return new CubePosition(-R, -S, -Q);
        }
        static public List<CubePosition> directions = new List<CubePosition>
        {
            new CubePosition(1, 0, -1),
            new CubePosition(1, -1, 0),
            new CubePosition(0, -1, 1),
            new CubePosition(-1, 0, 1),
            new CubePosition(-1, 1, 0),
            new CubePosition(0, 1, -1)
        };
        static public CubePosition GetDirection(Direction direction)
        {
            int dir = ((int)direction);
            return CubePosition.directions[dir];
        }
        static public CubePosition GetDirection(int direction)
        {
            return CubePosition.directions[direction];
        }
        public CubePosition GetNeighbor(int direction)
        {
            return Add(CubePosition.GetDirection(direction));
        }
        public CubePosition GetNeighbor(Direction direction)
        {
            return Add(CubePosition.GetDirection(direction));
        }

        static public List<CubePosition> diagonals = new List<CubePosition>
        {
            new CubePosition(2, -1, -1),
            new CubePosition(1, -2, 1),
            new CubePosition(-1, -1, 2),
            new CubePosition(-2, 1, 1),
            new CubePosition(-1, 2, -1),
            new CubePosition(1, 1, -2)
        };

        public CubePosition DiagonalNeighbor(int direction)
        {
            return Add(CubePosition.diagonals[direction]);
        }


        public int Length()
        {
            return (int)((Math.Abs(Q) + Math.Abs(R) + Math.Abs(S)) / 2);
        }

        public int Distance(CubePosition b)
        {
            return Subtract(b).Length();
        }

        public static int Distance(ulong a, ulong b)
        {
            CubePosition ca = new CubePosition(a);
            CubePosition cb = new CubePosition(b);
            return ca.Distance(cb);
        }

        public static Direction CalcDirection(ulong from, ulong to)
        {
            CubePosition ca = new CubePosition(from);
            if (ca.GetNeighbor(Direction.N).Pos == to) return Direction.N;
            if (ca.GetNeighbor(Direction.NE).Pos == to) return Direction.NE;
            if (ca.GetNeighbor(Direction.SE).Pos == to) return Direction.SE;
            if (ca.GetNeighbor(Direction.S).Pos == to) return Direction.S;
            if (ca.GetNeighbor(Direction.SW).Pos == to) return Direction.SW;
            if (ca.GetNeighbor(Direction.NW).Pos == to) return Direction.NW;

            return Direction.C;
        }

        public List<CubePosition> GetNeighbors(int map_radius)
        {
            if (map_radius == 1)
                return Neighbors;

            List<CubePosition> neighbors = new List<CubePosition>();
            for (int q = -map_radius; q <= map_radius; q++)
            {
                int r1 = Math.Max(-map_radius, -q - map_radius);
                int r2 = Math.Min(map_radius, -q + map_radius);
                for (int r = r1; r <= r2; r++)
                {
                    neighbors.Add(Add(q, r, -q - r));
                }
            }
            return neighbors;
        }

        public List<CubePosition> Neighbors
        {
            get
            {
                List<CubePosition> neighbors = new List<CubePosition>();
                neighbors.Add(GetNeighbor(0));
                neighbors.Add(GetNeighbor(1));
                neighbors.Add(GetNeighbor(2));
                neighbors.Add(GetNeighbor(3));
                neighbors.Add(GetNeighbor(4));
                neighbors.Add(GetNeighbor(5));
                return neighbors;
            }
        }

        /// <summary>
        /// evenq_to_cube(hex)
        /// </summary>
        public ulong Pos
        {
            get
            {
                var col = Q;
                var row = S + (Q + (Q & 1)) / 2;
                return Position.CreatePosition(col, row);
            }
        }

        public override string ToString()
        {
            return "Pos: " + Position.GetX(Pos) + "," + Position.GetY(Pos);
        }
    }

    public struct FractionalHex
    {
        public FractionalHex(double q, double r, double s)
        {
            this.q = q;
            this.r = r;
            this.s = s;
            if (Math.Round(q + r + s) != 0) throw new ArgumentException("q + r + s must be 0");
        }
        public readonly double q;
        public readonly double r;
        public readonly double s;

        public CubePosition HexRound()
        {
            int qi = (int)(Math.Round(q));
            int ri = (int)(Math.Round(r));
            int si = (int)(Math.Round(s));
            double q_diff = Math.Abs(qi - q);
            double r_diff = Math.Abs(ri - r);
            double s_diff = Math.Abs(si - s);
            if (q_diff > r_diff && q_diff > s_diff)
            {
                qi = -ri - si;
            }
            else
                if (r_diff > s_diff)
            {
                ri = -qi - si;
            }
            else
            {
                si = -qi - ri;
            }
            return new CubePosition(qi, ri, si);
        }


        public FractionalHex HexLerp(FractionalHex b, double t)
        {
            return new FractionalHex(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
        }


        static public List<CubePosition> HexLinedraw(CubePosition a, CubePosition b)
        {
            int N = a.Distance(b);
            FractionalHex a_nudge = new FractionalHex(a.Q + 1e-06, a.R + 1e-06, a.S - 2e-06);
            FractionalHex b_nudge = new FractionalHex(b.Q + 1e-06, b.R + 1e-06, b.S - 2e-06);
            List<CubePosition> results = new List<CubePosition> { };
            double step = 1.0 / Math.Max(N, 1);
            for (int i = 0; i <= N; i++)
            {
                results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
            }
            return results;
        }

    }

    public class Position
    {
        public static ulong ParsePosition(string pos)
        {
            int p = pos.IndexOf(',');
            int x = Convert.ToInt32(pos.Substring(0,p));
            int y = Convert.ToInt32(pos.Substring(p+1));
            return CreatePosition(x,y);
        }
        public static ulong CreatePosition(int x, int y)
        {
            ulong ux = (ulong)x & 0x000000000000ffff;
            ux = ux << 16;
            ulong uy = (ulong)y & 0x000000000000ffff;

            ulong upos = ux | uy;
            if (upos == 0)
                upos = ulong.MaxValue;
            return upos;
        }
        public static int GetX(ulong position)
        {
            if (position == ulong.MaxValue)
                return 0;
            ulong ux = position & 0x00000000ffff0000;
            ux = ux >> 16;
            return (int)ux;
        }
        public static int GetY(ulong position)
        {
            if (position == ulong.MaxValue)
                return 0;
            ulong uy = position & 0x000000000000ffff;
            return (int)uy;
        }
        public static ulong Null
        {
            get
            {
                return 0;
            }
        }
    }

}

