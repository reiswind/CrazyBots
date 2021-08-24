using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntTurret : Ant
    {
        public AntTurret(ControlAnt control) : base(control)
        {

        }

        public AntTurret(ControlAnt control, PlayerUnit playerUnit) : base(control, playerUnit)
        {

        }

        public override bool Move(Player player, List<Move> moves)
        {
            bool unitMoved = false;

            Unit cntrlUnit = PlayerUnit.Unit;

            if (cntrlUnit.Weapon != null)
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Weapon.ComputePossibleMoves(possiblemoves, null, MoveFilter.Fire);
                if (possiblemoves.Count > 0)
                {
                    int idx = player.Game.Random.Next(possiblemoves.Count);
                    if (cntrlUnit.Engine == null)
                    {
                        // Do not fire at trees
                        while (possiblemoves.Count > 0 && possiblemoves[idx].OtherUnitId == "Destructable")
                        {
                            possiblemoves.RemoveAt(idx);
                            idx = player.Game.Random.Next(possiblemoves.Count);
                        }
                    }
                    if (possiblemoves.Count > 0)
                    {
                        moves.Add(possiblemoves[idx]);
                        unitMoved = true;
                        return unitMoved;
                    }
                }
            }


            return unitMoved;
        }
    }

}
