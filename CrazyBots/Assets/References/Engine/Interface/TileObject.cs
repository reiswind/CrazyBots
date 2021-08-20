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
        C,
        N,
        S,
        NE,
        NW,
        SE,
        SW
    }
    public enum TileObjectType
    { 
        Gras,
        Bush,
        Tree
    }

    public class TileObject
    {
        public TileObjectType TileObjectType { get; set; }

        public Direction Direction;
    }
}
