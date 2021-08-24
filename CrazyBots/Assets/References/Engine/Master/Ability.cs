using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public abstract class Ability
    {
        public Unit Unit;

        public Ability(Unit unit)
        {
            Unit = unit;
            PartTileObjects = new List<TileObject>();
        }
        public List<TileObject> PartTileObjects { get; set; }
        public abstract string Name { get; }
        public TileContainer TileContainer { get; set; }
        public int Level { get; set; }

        public virtual void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {

        }
    }
}
