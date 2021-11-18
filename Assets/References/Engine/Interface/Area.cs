

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
        internal Tile GetClosestTile(Position2 pos, Dictionary<Position2, Tile> tiles)
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
        }
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
        }*/

        public override string ToString()
        {
            return "Area: " + PlayerId + " Units: " + Units.Count;
        }

        public int AreaNr;
        public Dictionary<Position2, Tile> Tiles = new Dictionary<Position2, Tile>();
        /// <summary>
        /// Tiles next to another player
        /// </summary>
        public Dictionary<Position2, Tile> BorderTiles = new Dictionary<Position2, Tile>();
        /// <summary>
        /// Tiles into nowhere
        /// </summary>
        public Dictionary<Position2, Tile> ForeignBorderTiles = new Dictionary<Position2, Tile>();
        
        public List<Unit> Units = new List<Unit>();
        public int PlayerId;
        public int Range;
    }
}
