using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Ability
    {
        public Unit Unit;

        public Ability(Unit unit)
        {
            Unit = unit;
        }

        public virtual void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {

        }
    }
}
