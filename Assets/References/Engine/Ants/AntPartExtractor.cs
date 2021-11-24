
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartExtractor : AntPart
    {
        public Extractor Extractor { get; private set; }
        public AntPartExtractor(Ant ant, Extractor extractor) : base(ant)
        {
            Extractor = extractor;
        }
        public override string ToString()
        {
            return "AntPartExtractor";
        }
        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            Unit cntrlUnit = Extractor.Unit;

            if (cntrlUnit.Weapon != null && cntrlUnit.Weapon.TileContainer.Count > 0)
            {
                // Prefer Fight, do not extract if can fire
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Weapon.ComputePossibleMoves(possiblemoves, null, MoveFilter.Fire);
                if (possiblemoves.Count > 0)
                    return false;
            }

            if (cntrlUnit.Extractor != null && cntrlUnit.Extractor.CanExtract)
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, null, MoveFilter.Extract);
                if (possiblemoves.Count > 0)
                {
                    // Assume Minerals for now
                    List<Move> mineralmoves = new List<Move>();
                    foreach (Move mineralMove in possiblemoves)
                    {
                        if (Ant.AntWorkerType == AntWorkerType.Worker && Ant.Unit.CurrentGameCommand == null)
                        {
                            // Worker will only extract minerals if no command is attached.
                            if (mineralMove.OtherUnitId != "Mineral")
                            {
                                continue;
                            }
                        }

                        // Everything
                        mineralmoves.Add(mineralMove);
                    }
                    if (mineralmoves.Count > 0)
                    {
                        int idx = player.Game.Random.Next(mineralmoves.Count);
                        Move move = mineralmoves[idx];
                        moves.Add(move);

                        Ant.FollowThisRoute = null;

                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}
