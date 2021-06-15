using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntReactor : Ant
    {
        public AntReactor(ControlAnt control) : base(control)
        {

        }

        public AntReactor(ControlAnt control, PlayerUnit playerUnit) : base(control, playerUnit)
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

            if (cntrlUnit.Extractor != null)
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, null, MoveFilter.Extract);
                if (possiblemoves.Count > 0)
                {
                    foreach (Move possibleMove in possiblemoves)
                    {
                        if (Control.IsExtractable(player, possibleMove, moves))
                        {
                            Tile n = player.Game.Map.GetTile(possibleMove.Positions[1]);
                            if (n.Unit != null && n.Unit.Assembler != null && !n.Unit.ExtractMe)
                            {
                                // YES extract from attached factory
                            }
                            moves.Add(possibleMove);
                            return true;
                        }
                    }
                }
            }
            return unitMoved;
        }
    }

}
