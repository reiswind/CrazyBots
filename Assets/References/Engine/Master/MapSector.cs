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
        public ulong Center { get; set; }

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
    }
}
