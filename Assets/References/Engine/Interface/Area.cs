

using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{

    public enum BorderType
    {
        None,
        Foreign,
        Enemy
    }

    public class Area
    {
        internal Map Map;
        public Area(Map map)
        {
            Map = map;
        }
        /*
        internal Tile GetClosestTile(ulong pos, Dictionary<ulong, Tile> tiles)
        {
            Tile closestTile = null;
            double closestDistance = 0;
            foreach (Tile t in tiles.Values)
            {
                if (t.Unit == null)
                {
                    double distance = t.Pos.GetDistanceTo(pos);
                    if (closestTile == null || distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTile = t;
                    }
                }
            }
            return closestTile;
        }*/
        public bool IsPrimary()
        {
            foreach (PlayerUnit playerUnit in Units)
            {
                if (playerUnit.Unit.Assembler != null)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return "Area: " + PlayerId + " Units: " + Units.Count;
        }

        public int AreaNr;
        public Dictionary<ulong, Tile> Tiles = new Dictionary<ulong, Tile>();
        /// <summary>
        /// Tiles next to another player
        /// </summary>
        public Dictionary<ulong, Tile> BorderTiles = new Dictionary<ulong, Tile>();
        /// <summary>
        /// Tiles into nowhere
        /// </summary>
        public Dictionary<ulong, Tile> ForeignBorderTiles = new Dictionary<ulong, Tile>();
        
        public List<PlayerUnit> Units = new List<PlayerUnit>();
        public int PlayerId;
        public int Range;
    }
}
