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
                PlayerUnit.Unit.Container.TileContainer.Minerals < PlayerUnit.Unit.Container.TileContainer.Capacity)
            {
                intensity = 1;
                intensity -= (float)PlayerUnit.Unit.Container.TileContainer.Minerals / PlayerUnit.Unit.Container.TileContainer.Capacity;
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

            
            return unitMoved;
        }

        
    }

}
