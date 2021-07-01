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
                                // YES extract from attached factory
                            }
                            moves.Add(possibleMove);
                            return true;
                        }
                    }
                }
            }

            if (cntrlUnit.Container != null)
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Container.ComputePossibleMoves(possiblemoves, null, MoveFilter.Transport);
                if (possiblemoves.Count > 0)
                {
                    foreach (Move possibleMove in possiblemoves)
                    {
                        bool skipMove = false;

                        foreach (Move intendedMove in moves)
                        {
                            if (intendedMove.MoveType == MoveType.Extract)
                            {
                                if (intendedMove.Positions[0] == possibleMove.Positions[1])
                                {
                                    // Unit is currently extracting, no need to fill it
                                    skipMove = true;
                                    break;
                                }
                            }
                            if (intendedMove.MoveType == MoveType.Transport)
                            {
                                if (intendedMove.Positions[1] == possibleMove.Positions[1])
                                {
                                    // Unit is filled by transport
                                    skipMove = true;
                                    break;
                                }
                            }
                        }
                        if (skipMove)
                            continue;

                        moves.Add(possibleMove);
                        return true;
                    }
                }
            }

            return unitMoved;
        }
    }

}
