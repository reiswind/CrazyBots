
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartWeapon : AntPart
    {
        public Weapon Weapon { get; private set; }
        public AntPartWeapon(Ant ant, Weapon weapon) : base(ant)
        {
            Weapon = weapon;
        }
        public override string ToString()
        {
            return "AntPartWeapon";
        }
        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            Unit cntrlUnit = Weapon.Unit;

            if (Weapon.TileContainer.Count == 0)
            {
                // Request some ammo
                Ant.Unit.DeliveryRequest(TileObjectType.Ammo, Weapon.TileContainer.Capacity);
            }

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
                    Ant.FollowThisRoute = null;

                    moves.Add(possiblemoves[idx]);
                    
                    return true;
                }
            }
            return false;
        }
    }
}
