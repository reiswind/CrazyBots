
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
        internal Tile GetClosestTile(Position pos, Dictionary<Position, Tile> tiles)
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
        public Dictionary<Position, Tile> Tiles = new Dictionary<Position, Tile>();
        /// <summary>
        /// Tiles next to another player
        /// </summary>
        public Dictionary<Position, Tile> BorderTiles = new Dictionary<Position, Tile>();
        /// <summary>
        /// Tiles into nowhere
        /// </summary>
        public Dictionary<Position, Tile> ForeignBorderTiles = new Dictionary<Position, Tile>();
        
        public List<PlayerUnit> Units = new List<PlayerUnit>();
        public int PlayerId;
        public int Range;
    }
}
