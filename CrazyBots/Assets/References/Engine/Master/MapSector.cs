using Engine.MapGenerator;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class MapSector
    {
        public HexCell HexCell { get; set; }
        public Position Center { get; set; }
        public bool InsideHexagon(float x, float y)
        {
            // Check length (squared) against inner and outer radius
            float l2 = x * x + y * y;
            if (l2 > 1.0f) return false;
            if (l2 < 0.75f) return true; // (sqrt(3)/2)^2 = 3/4

            // Check against borders
            float px = x * 1.15470053838f; // 2/sqrt(3)
            if (px > 1.0f || px < -1.0f) return false;

            float py = 0.5f * px + y;
            if (py > 1.0f || py < -1.0f) return false;

            if (px - py > 1.0f || px - py < -1.0f) return false;

            return true;
        }

        public bool IsPossibleStart(Map map)
        {
            if (HexCell == null)
                return true;

            if (HexCell.IsUnderwater)
                return false;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = HexCell.GetNeighbor(d);
                if (neighbor == null)
                {
                    continue;
                }
                if (neighbor.IsUnderwater)
                    return false;
            }
            return true;
        }
        /*

        public void GenerateTiles(Map map, Position pos, double height)
        {
            int totalMetal;

            totalMetal = 0;

            int centerX = pos.X;
            int centerY = pos.Y;
            int width = 10;

            //Position centerPos = new Position(pos.X * width*2, pos.Y * width*2);
            Position centerPos = new Position(0, 0);

            CubePosition center = centerPos.GetCubePosition();

            //for (int q = center.q - width; q < center.q + width; q++)
            {
                //for (int r = center.r - width; r < center.r + width; r++)
                for (int x = centerPos.X; x < centerPos.X + width; x++)
                {
                    //for (int s = center.s - width; s < center.s + width; s++)
                    for (int y = centerPos.Y; y < centerPos.Y + width; y++)
                    {

                        //CubePosition c1 = new CubePosition(q, r, s); // p.GetCubePosition();
                        Position p = new Position(x, y);

                        if (!InsideHexagon(p.X / 10, p.Y / 10))
                            continue;

                        //if (!(Math.Abs(c1.q) <= width && Math.Abs(c1.r) <= width && Math.Abs(c1.s) <= width))
                        {
                            //continue;
                        }
                        if (Tiles.ContainsKey(p))
                            continue;

                        Tile t = new Tile(map, p);
                        Tiles.Add(p, t);

                        t.Height = height; // terrain.Data[x, y];

                        if (t.IsDarkWood())
                        {
                            if (map.Game.Random.Next(8) == 1)
                            {
                                t.NumberOfDestructables = 1;
                                //t.Metal = 1;
                            }
                        }
                        else if (t.IsWood())
                        {
                            if (map.Game.Random.Next(14) == 0)
                            {
                                t.NumberOfDestructables = 3;
                                //t.Metal = 1;
                            }
                        }
                        else if (t.IsLightWood())
                        {
                            if (map.Game.Random.Next(25) == 0)
                            {
                                t.NumberOfDestructables = 4;
                                //t.Metal = 1;
                            }
                        }
                        else if (t.IsDarkSand())
                        {
                            if (map.Game.Random.Next(25) == 0)
                            {
                                t.NumberOfDestructables = 4;
                                //t.Metal = 4;
                            }
                        }
                        else if (t.IsSand())
                        {
                            if (map.Game.Random.Next(30) == 0)
                            {
                                t.NumberOfDestructables = 3;
                                //t.Metal = 3;
                            }
                            else if (map.Game.Random.Next(20) == 0)
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
            }
        }*/
    }
}
