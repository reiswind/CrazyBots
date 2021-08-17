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

        private int depositNeedMinerals;

        public override void OnDestroy(Player player)
        {
            if (depositNeedMinerals != 0)
            {
                player.Game.Pheromones.DeletePheromones(depositNeedMinerals);
                depositNeedMinerals = 0;
            }
        }

        public override void UpdateContainerDeposits(Player player)
        {
            if (depositNeedMinerals != 0)
            {
                player.Game.Pheromones.DeletePheromones(depositNeedMinerals);
                depositNeedMinerals = 0;
            }

            int range;
            float intensity;

            // Reactor demands Minerals
            if (PlayerUnit.Unit.Engine == null &&
                PlayerUnit.Unit.Container != null &&
                PlayerUnit.Unit.Container.Mineral < PlayerUnit.Unit.Container.Capacity)
            {
                intensity = 1;
                intensity -= (float)PlayerUnit.Unit.Container.Mineral / PlayerUnit.Unit.Container.Capacity;
                range = 5;

                if (depositNeedMinerals == 0)
                {
                    depositNeedMinerals = player.Game.Pheromones.DropPheromones(player, PlayerUnit.Unit.Pos, range, PheromoneType.Container, intensity, true);
                }
                else
                {
                    player.Game.Pheromones.UpdatePheromones(depositNeedMinerals, intensity);
                }
            }
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
            return unitMoved;
        }

        
    }

}
