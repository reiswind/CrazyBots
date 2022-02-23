
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
                List<Move> possibleFireMoves = new List<Move>();
                cntrlUnit.Weapon.ComputePossibleMoves(possibleFireMoves, null, MoveFilter.Fire);
                if (possibleFireMoves.Count > 0)
                    return false;
            }
            List<Move> possiblemoves = new List<Move>();
            List<Position2> includedPositions = null;

            if (cntrlUnit.CurrentGameCommand != null &&
                cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Collect &&
                cntrlUnit.CurrentGameCommand.TransportUnit.UnitId == Ant.Unit.UnitId)
            {
                includedPositions = new List<Position2>();
                includedPositions.Add(cntrlUnit.CurrentGameCommand.TargetPosition);
            }

            if (cntrlUnit.CurrentGameCommand != null &&
                cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Unload &&
                cntrlUnit.CurrentGameCommand.AttachedUnit.UnitId == Ant.Unit.UnitId)
            {
                includedPositions = new List<Position2>();
                includedPositions.Add(cntrlUnit.CurrentGameCommand.TargetPosition);
                if (cntrlUnit.Extractor != null)
                {
                    cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, includedPositions, MoveFilter.Unload);
                }
            }
            else
            {
                if (cntrlUnit.Extractor != null && cntrlUnit.Extractor.CanExtract)
                {
                    cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, includedPositions, MoveFilter.Extract);
                }
            }
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
            
            
            return false;
        }
    }
}
