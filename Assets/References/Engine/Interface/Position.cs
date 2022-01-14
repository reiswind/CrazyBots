using Engine.Master;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

    public class Dir
    {
        public static Direction TurnAround(Direction direction)
        {
            if (direction == Direction.N) return Direction.S;
            if (direction == Direction.NE) return Direction.SW;
            if (direction == Direction.SE) return Direction.NW;
            if (direction == Direction.S) return Direction.N;
            if (direction == Direction.SW) return Direction.NE;
            if (direction == Direction.NW) return Direction.SE;
            return Direction.C;
        }
        public static Direction TurnLeft(Direction direction)
        {
            if (direction == Direction.N) return Direction.NW;
            if (direction == Direction.NW) return Direction.SW;
            if (direction == Direction.SW) return Direction.S;
            if (direction == Direction.S) return Direction.SE;
            if (direction == Direction.SE) return Direction.NE;
            if (direction == Direction.NE) return Direction.N;
            return Direction.C;
        }
        public static Direction TurnRight(Direction direction)
        {
            if (direction == Direction.N) return Direction.NE;
            if (direction == Direction.NE) return Direction.SE;
            if (direction == Direction.SE) return Direction.S;
            if (direction == Direction.S) return Direction.SW;
            if (direction == Direction.SW) return Direction.NW;
            if (direction == Direction.NW) return Direction.N;
            return Direction.C;
        }
    }

    public readonly struct Position3 //: IEquatable<Position3>
    {
        public Position3(Position2 pos)
        {
            Direction = Direction.C;
            int pX = pos.X;
            int pY = pos.Y;
            q = pX;
            s = pY - (pX + (pX & 1)) / 2;
            r = -q - s;
        }
        
        public Position3(Position2 pos, Direction direction)
        {
            Direction = direction;
            int pX = pos.X;
            int pY = pos.Y;
            q = pX;
            s = pY - (pX + (pX & 1)) / 2;
            r = -q - s;
        }
        public Position3(int q, int r, int s)
        {
            Direction = Direction.C;
            this.q = q;
            this.r = r;
            this.s = s;
        }
        
        public Position3(int q, int r, int s, Direction direction)
        {
            Direction = direction;
            this.q = q;
            this.r = r;
            this.s = s;
        }

        private readonly int q;
        private readonly int r;
        private readonly int s;

        public Direction Direction { get; }

        /// <summary>
        /// x
        /// </summary>
        public int Q { get { return q;  }  } 
        /// <summary>
        /// y
        /// </summary>
        public int R { get { return r; } } 
        /// <summary>
        /// z
        /// </summary>
        public int S { get { return s; } }

        
        public bool Equals(Position3 other)
        {
            return q == other.q && r == other.r && s == other.s;
        }
        public static bool operator ==(Position3 a, Position3 b) => a.Equals(b);

        public static bool operator !=(Position3 a, Position3 b) => !a.Equals(b);

        public override bool Equals(object obj)
        {
            if (!(obj is Position3)) return false;
            var other = (Position3)obj;
            return q == other.q && r == other.r && s == other.s;
        }
        /* Leave it to c# (fit 3 int into 1)*/
        public override int GetHashCode()
        {
            return base.GetHashCode();
            /*
            BitConverter.
            int b = sizeof(int);
            return q*16 + r*8 + s;*/
        }

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
        public Position3 Move(Direction direction, int lenght)
        {
            Position3 cubePosition = this;
            while (lenght-- > 0)
                cubePosition = cubePosition.GetNeighbor(direction);
            return cubePosition;
        }

        public List<Position3> DrawLine(Position2 pos)
        {
            Position3 to = new Position3(pos);
            return DrawLine(to);
        }
        public List<Position3> DrawLine(Position3 to)
        { 
            List<Position3> line = FractionalHex.HexLinedraw(this, to);
            return line;
        }
        public List<Position3> CreateRing(int radius)
        {
            List<Position3> results = new List<Position3>();
            Position3 cube = Move(Direction.NW, radius);
            
            Position3 firstcube = new Position3(cube.Q, cube.R, cube.S, Direction.N);

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        
                    }
                    else
                    {
                        results.Add(cube);
                    }
                    cube = cube.GetNeighbor((Direction)i);
                }
            }
            results.Add(firstcube);
            return results;
        }

        public Position3 Add(Position3 b, Direction direction)
        {
            return new Position3(Q + b.Q, R + b.R, S + b.S, direction);
        }        
        public Position3 Add(Position3 b)
        {
            return new Position3(Q + b.Q, R + b.R, S + b.S);
        }
        public Position3 Add(int q1, int r1, int s1)
        {
            return new Position3(Q + q1, R + r1, S + s1);
        }

        public Position3 Subtract(Position3 b)
        {
            return new Position3(Q - b.Q, R - b.R, S - b.S);
        }

        public Position3 Scale(int k)
        {
            return new Position3(Q * k, R * k, S * k);
        }

        public Position3 RotateLeft()
        {
            return new Position3(-S, -Q, -R);
        }

        public Position3 RotateRight()
        {
            return new Position3(-R, -S, -Q);
        }
        static public List<Position3> directions = new List<Position3>
        {
            new Position3(1, 0, -1),
            new Position3(1, -1, 0),
            new Position3(0, -1, 1),
            new Position3(-1, 0, 1),
            new Position3(-1, 1, 0),
            new Position3(0, 1, -1)
        };
        static public Position3 GetDirection(Direction direction)
        {
            int dir = ((int)direction);
            return Position3.directions[dir];
        }
        /*
        static public Position3 GetDirection(int direction)
        {
            return Position3.directions[direction];
        }
        
        public Position3 GetNeighbor(int direction)
        {
            return Add(Position3.GetDirection(direction));
        }*/
        public Position3 GetNeighbor(Direction direction)
        {
            return Add(Position3.GetDirection(direction), direction);
        }

        static public List<Position3> diagonals = new List<Position3>
        {
            new Position3(2, -1, -1),
            new Position3(1, -2, 1),
            new Position3(-1, -1, 2),
            new Position3(-2, 1, 1),
            new Position3(-1, 2, -1),
            new Position3(1, 1, -2)
        };

        public Position3 DiagonalNeighbor(int direction)
        {
            return Add(Position3.diagonals[direction]);
        }
        public int Length()
        {
            return (int)((Math.Abs(Q) + Math.Abs(R) + Math.Abs(S)) / 2);
        }

        public int Distance(Position3 b)
        {
            return Subtract(b).Length();
        }

        public static int Distance(Position2 a, Position2 b)
        {
            Position3 ca = new Position3(a);
            Position3 cb = new Position3(b);
            return ca.Distance(cb);
        }

        public static Direction CalcDirection(Position2 from, Position2 to)
        {
            Position3 ca = new Position3(from);
            if (ca.GetNeighbor(Direction.N).Pos == to) return Direction.N;
            if (ca.GetNeighbor(Direction.NE).Pos == to) return Direction.NE;
            if (ca.GetNeighbor(Direction.SE).Pos == to) return Direction.SE;
            if (ca.GetNeighbor(Direction.S).Pos == to) return Direction.S;
            if (ca.GetNeighbor(Direction.SW).Pos == to) return Direction.SW;
            if (ca.GetNeighbor(Direction.NW).Pos == to) return Direction.NW;

            return Direction.C;
        }

        public List<Position3> GetNeighbors(int map_radius)
        {
            if (map_radius == 1)
                return Neighbors;

            List<Position3> neighbors = new List<Position3>();
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

        public List<Position3> Neighbors
        {
            get
            {
                List<Position3> neighbors = new List<Position3>();
                neighbors.Add(GetNeighbor(Direction.NE));
                neighbors.Add(GetNeighbor(Direction.SE));
                neighbors.Add(GetNeighbor(Direction.S));
                neighbors.Add(GetNeighbor(Direction.SW));
                neighbors.Add(GetNeighbor(Direction.NW));
                neighbors.Add(GetNeighbor(Direction.N));
                return neighbors;
            }
        }

        /// <summary>
        /// evenq_to_cube(hex)
        /// </summary>
        public Position2 Pos
        {
            get
            {
                var col = Q;
                var row = S + (Q + (Q & 1)) / 2;
                return new Position2(col, row);
            }
        }

        public override string ToString()
        {
            return "Pos: " + Pos.ToString() + " " + Direction.ToString();
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

        public Position3 HexRound()
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
            return new Position3(qi, ri, si);
        }


        public FractionalHex HexLerp(FractionalHex b, double t)
        {
            return new FractionalHex(q * (1.0 - t) + b.q * t, r * (1.0 - t) + b.r * t, s * (1.0 - t) + b.s * t);
        }


        static public List<Position3> HexLinedraw(Position3 a, Position3 b)
        {
            int N = a.Distance(b);
            FractionalHex a_nudge = new FractionalHex(a.Q + 1e-06, a.R + 1e-06, a.S - 2e-06);
            FractionalHex b_nudge = new FractionalHex(b.Q + 1e-06, b.R + 1e-06, b.S - 2e-06);
            List<Position3> results = new List<Position3> { };
            double step = 1.0 / Math.Max(N, 1);
            for (int i = 0; i <= N; i++)
            {
                results.Add(a_nudge.HexLerp(b_nudge, step * i).HexRound());
            }
            return results;
        }

    }

    public class Position2Converter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dict = new List<Position2>();

            int x = 0;
            int y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string readerValue = reader.Value.ToString();
                    if (reader.Read() && reader.TokenType == JsonToken.Integer)
                    {
                        if (readerValue == "X")
                            x = Convert.ToInt32(reader.Value);
                        if (readerValue == "Y")
                        {
                            y = Convert.ToInt32(reader.Value);
                            dict.Add(new Position2(x, y));
                        }
                    }
                }
                if (reader.TokenType == JsonToken.EndArray) 
                    return dict;
            }
            return dict;
        }

        public override bool CanConvert(Type objectType)
        {
            return true; // typeof(List<Position2>.IsAssignableFrom(objectType);
        }
    }

    [DataContract]
    public struct Position2 : IEquatable<Position2>
    {
        public static readonly int BlockPathItemCount = 20;

        public Position2(int x, int y)
        {
            uint ux = ((uint)x << 16);
            uint uy = ((uint)y & 0x0000ffff);

            position = (int)(ux | uy);
            if (position == 0)
                position = int.MaxValue;
        }
        public Position2(int pos)
        {
            position = pos;
        }
        private readonly int position;
        [DataMember]
        public int X
        {
            get
            {
                if (position == int.MaxValue)
                    return 0;

                uint ux = ((uint)position & 0xffff0000);
                ux >>= 16;
                short x16 = (short)ux;
                return (int)x16;
            }
        }
        [DataMember]
        public int Y
        {
            get
            {
                if (position == int.MaxValue)
                    return 0;

                uint uy = ((uint)position & 0x0000ffff);
                uy |= 0xffff0000;
                short y16 = (short)uy;
                return y16;
            }
        }

        public static Direction GetDirection(Position2 p1, Position2 p2)
        {
            Position3 source = new Position3(p1);
            Position3 target = new Position3(p2);
            if (source.GetNeighbor(Direction.N) == target)
                return Direction.N;
            if (source.GetNeighbor(Direction.NE) == target)
                return Direction.NE;
            if (source.GetNeighbor(Direction.NW) == target)
                return Direction.NW;
            if (source.GetNeighbor(Direction.S) == target)
                return Direction.S;
            if (source.GetNeighbor(Direction.SE) == target)
                return Direction.SE;
            if (source.GetNeighbor(Direction.SW) == target)
                return Direction.SW;

            return Direction.C;
        }
        public bool Equals(Position2 other)
        {
            return position == other.position;
        }
        public static bool operator ==(Position2 a, Position2 b) => a.Equals(b);

        public static bool operator !=(Position2 a, Position2 b) => !a.Equals(b);

        public override bool Equals(object obj)
        {
            if (!(obj is Position2)) return false;
            var other = (Position2)obj;
            return position == other.position;
        }
        public override int GetHashCode()
        {
            return position;
        }
        public override string ToString()
        {
            return X + "," + Y;
        }
        public static Position2 Null
        {
            get
            {
                return new Position2(0);
            }
        }
        
        public static Position2 ParsePosition(string pos)
        {
            int p = pos.IndexOf(',');
            int x = Convert.ToInt32(pos.Substring(0,p));
            int y = Convert.ToInt32(pos.Substring(p+1));
            return new Position2(x,y);
        }
        
    }

}

