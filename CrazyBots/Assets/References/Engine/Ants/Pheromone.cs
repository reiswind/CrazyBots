using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    public enum PheromoneType
    {
        None,
        ToHome,
        ToFood,
        Enemy
    }

    public class Pheromones
    {
        private static Dictionary<Position, Pheromone> Items = new Dictionary<Position, Pheromone>();
        public static int Age { get; private set; }

        public static void Clear()
        {
            Items.Clear();
        }

        public static IEnumerable<Pheromone> AllPhromones
        {
            get
            {
                return Items.Values;
            }
        }

        public static void Add(Pheromone pheromone)
        {
            Items.Add(pheromone.Pos, pheromone);
        }

        public static void Deposit(int playerId, Position pos, PheromoneType pheromoneType, float intensity, bool isStatic)
        {
            Pheromone pheromone;

            if (!Items.ContainsKey(pos))
            {
                pheromone = new Pheromone();
                pheromone.Pos = pos;
                Pheromones.Add(pheromone);
            }
            else
            {
                pheromone = Items[pos];
            }
            pheromone.Deposit(playerId, intensity, pheromoneType, isStatic);

        }

        public static Pheromone FindAt(Position pos)
        {
            if (pos != null && Items.ContainsKey(pos))
                return Items[pos];
            return null;
        }

        public static void DropStaticPheromones(Player player, Position pos, int range, PheromoneType pheromoneType)
        {
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(pos, range, false);

            foreach (TileWithDistance tileWithDistance in tiles.Values)
            {
                float distance = range - tileWithDistance.Distance;
                float intensity = (distance * 100) / range / 100;

                //float intensity = 1f / (tileWithDistance.Distance) * (2f - tileWithDistance.Distance / 10f);

                Deposit(player.PlayerModel.Id, tileWithDistance.Pos, pheromoneType, intensity, true);
            }
         }

        public static void Evaporate()
        {
            Age++;

            List<Pheromone> tobeRemoved = new List<Pheromone>();
            foreach (Pheromone pheromone in Items.Values)
            {
                if (pheromone.EvaporateSteps(Age))
                {
                    tobeRemoved.Add(pheromone);
                }
            }
            foreach (Pheromone pheromone1 in tobeRemoved)
            {
                Items.Remove(pheromone1.Pos);
            }
        }
    }

    public class PheromoneItem
    {
        public int PlayerId { get; set; }
        public float Intensity { get; set; }
        public bool IsStatic { get; set; }
        public PheromoneType PheromoneType { get; set; }

        public bool Evaporate()
        {
            if (!IsStatic)
            {
                if (PheromoneType == PheromoneType.ToFood)
                    Intensity -= Intensity * 0.02f;
                else if (PheromoneType == PheromoneType.ToHome)
                    Intensity -= Intensity * 0.05f; // FOOD_TRAIL_FORGET_RATE

                if (Intensity < 0.01f)
                    Intensity = 0;
            }

            return Intensity == 0;
        }
        /*
        IntensityHome -= IntensityHome * 0.01f;
        if (IntensityHome < 0.001f) IntensityHome = 0;

        // Must be aligend with food trail

        IntensityFood -= IntensityFood * 0.03f; // FOOD_TRAIL_FORGET_RATE
        if (IntensityFood < 0.001f) IntensityFood = 0;

        return IntensityHome == 0 && IntensityFood == 0;*/
    }

    public class Pheromone
    {
        public Position Pos { get; set; }
        public List<PheromoneItem> PheromoneItems { get; private set; }

        //public float IntensityFood { get; set; }
        //public float IntensityHome { get; set; }

        public Pheromone()
        {
            PheromoneItems = new List<PheromoneItem>();
        }

        public void Deposit(int playerId, float intensity, PheromoneType pheromoneType, bool isStatic)
        {
            PheromoneItem pheromoneItem = new PheromoneItem();
            pheromoneItem.PlayerId = playerId;
            pheromoneItem.Intensity = intensity;
            pheromoneItem.PheromoneType = pheromoneType;
            pheromoneItem.IsStatic = isStatic;
            PheromoneItems.Add(pheromoneItem);

            /*
            if (pheromoneType == PheromoneType.ToFood)
            {
                IntensityHome += intensity;
                if (IntensityHome > 1)
                    IntensityHome = 1; 
            }
            if (pheromoneType == PheromoneType.ToHome)
            {
                IntensityFood += intensity;
                if (IntensityFood > 1)
                    IntensityFood = 1;
            }*/
        }
        public float GetIntensityF(int playerId, PheromoneType pheromoneType)
        {
            float intensity = 0;

            foreach (PheromoneItem pheromoneItem in PheromoneItems)
            {
                if (pheromoneItem.PlayerId == playerId &&
                    pheromoneItem.PheromoneType == pheromoneType)
                {
                    intensity += pheromoneItem.Intensity;
                }
            }
            if (intensity > 1)
                intensity = 1;
            /*
            if (pheromoneType == PheromoneType.ToFood)
                return IntensityFood;
            
            if (pheromoneType == PheromoneType.ToHome)
                return IntensityHome;
            */
            return intensity;
        }


        /*
        public int DistanceToHome
        {
            get
            {
                return StepsFromHome.Count;
                int distanceToHome = 0;

                if (StepsFromHome.Count > 0)
                {
                    foreach (int distance in StepsFromHome)
                        distanceToHome += distance;

                    distanceToHome /= StepsFromHome.Count;
                }
                return distanceToHome;
            }
        }

        public int DistanceToFood
        {
            get
            {
                return StepsFromFood.Count;
            }
        }*/

        //private List<int> StepsFromHome = new List<int>();
        //private List<int> StepsFromFood = new List<int>();

        /*
         * 
GridObjResult Pheromone::Update(float delta_time)
{
	if (m_type == food) m_strength -= m_strength * Settings::FOOD_TRAIL_FORGET_RATE * delta_time;
	if (m_type == home) m_strength -= m_strength * Settings::HOME_TRAIL_FORGET_RATE * delta_time;
	SetColor();

	if (m_strength <= 0.0f) m_strength = 0.0f;
	return GridObject2D::Update(delta_time);
}

        */
        public bool EvaporateSteps(int age)
        {
            bool remove = true;
            foreach (PheromoneItem pheromoneItem in PheromoneItems)
            {
                if (!pheromoneItem.Evaporate())
                    remove = false;
            }
            /*
            IntensityHome -= IntensityHome * 0.01f;
            if (IntensityHome < 0.001f) IntensityHome = 0;
            
            // Must be aligend with food trail

            IntensityFood -= IntensityFood * 0.03f; // FOOD_TRAIL_FORGET_RATE
            if (IntensityFood < 0.001f) IntensityFood = 0;

            return IntensityHome == 0 && IntensityFood == 0;
            */
            return remove;

        }


    }


}


