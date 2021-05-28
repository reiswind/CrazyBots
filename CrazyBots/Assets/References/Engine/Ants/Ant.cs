using Engine.Control;
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class Ant
    {
        public Ant(ControlAnt control)
        {
            Control = control;
        }

        public Ant(ControlAnt control, PlayerUnit playerUnit)
        {
            PlayerUnit = playerUnit;
            Control = control;

            Energy = MaxEnergy;
        }

        static public float HomeTrailDepositRate = 0.5f;
        static public float FoodTrailDepositRate = 0.2f;
        static public float MaxEnergy = 200;
        public float Energy { get; set; }

        static public float MaxFoodIntensity = 100;
        public float FoodIntensity { get; set; }

        public int MoveAttempts { get; set; }
        public int StuckCounter { get; set; }
        public bool Alive { get; set; }
        public PlayerUnit PlayerUnit { get; set; }
        public ControlAnt Control { get; set; }

        public int PheromoneDepositEnergy { get; set; }
        public int PheromoneDepositNeedMinerals { get; set; }
        public int PheromoneDepositNeedMineralsLevel { get; set; }

        public List<Position> FollowThisRoute { get; set; }
        public virtual bool Move(Player player, List<Move> moves)
        {
            return true;
        }
    }

}
