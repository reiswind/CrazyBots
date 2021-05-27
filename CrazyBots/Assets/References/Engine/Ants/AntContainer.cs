using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntContainer : Ant
    {
        public AntContainer(ControlAnt control) : base(control)
        {

        }

        public AntContainer(ControlAnt control, PlayerUnit playerUnit) : base(control, playerUnit)
        {

        }

        public override bool Move(Player player, List<Move> moves)
        {
            bool unitMoved = false;

            Unit cntrlUnit = PlayerUnit.Unit;
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
                                // YES not extract from attached factory
                                // continue;
                                int x = 0;
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
