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
        Container,
        Mineral,
        Enemy,
        Energy,
        AwayFromEnergy,
        AwayFromEnemy,
        Work
    }

    public class Pheromones
    {
        private Dictionary<Position, Pheromone> Items = new Dictionary<Position, Pheromone>();
        public int Age { get; private set; }

        public void Clear()
        {
            Items.Clear();
        }

        public IEnumerable<Pheromone> AllPhromones
        {
            get
            {
                return Items.Values;
            }
        }

        public void Add(Pheromone pheromone)
        {
            Items.Add(pheromone.Pos, pheromone);
        }

        public PheromoneItem Deposit(Player player, Position pos, PheromoneType pheromoneType, float intensity, bool isStatic)
        {
            Pheromone pheromone;

            if (!Items.ContainsKey(pos))
            {
                pheromone = new Pheromone();
                pheromone.Pos = pos;
                player.Game.Pheromones.Add(pheromone);
            }
            else
            {
                pheromone = Items[pos];
            }
            return pheromone.Deposit(player.PlayerModel.Id, intensity, pheromoneType, isStatic);
        }

        public Pheromone FindAt(Position pos)
        {
            if (pos != null && Items.ContainsKey(pos))
                return Items[pos];
            return null;
        }

        public void RemoveAllStaticPheromones(Player player, PheromoneType pheromoneType)
        {
            List<Pheromone> tobeRemoved = new List<Pheromone>();

            foreach (Pheromone pheromone in Items.Values)
            {
                List<PheromoneItem> itemTobeRemoved = new List<PheromoneItem>();
                foreach (PheromoneItem pheromoneItem in pheromone.PheromoneItems)
                {
                    if (pheromoneItem.PlayerId == player.PlayerModel.Id &&
                        pheromoneItem.PheromoneType == pheromoneType)
                    {
                        itemTobeRemoved.Add(pheromoneItem);
                    }
                }
                foreach (PheromoneItem pheromoneItem1 in itemTobeRemoved)
                {
                    pheromone.PheromoneItems.Remove(pheromoneItem1);
                }
                if (pheromone.PheromoneItems.Count == 0)
                {
                    tobeRemoved.Add(pheromone);
                }
            }
            foreach (Pheromone pheromone1 in tobeRemoved)
            {
                Items.Remove(pheromone1.Pos);
            }
        }

        private Dictionary<int, PheromoneStack> pheromoneStacks = new Dictionary<int, PheromoneStack>();

        public void DeletePheromones(int id)
        {
            PheromoneStack pheromoneStack = pheromoneStacks[id];
            foreach (PheromoneStackItem pheromoneStackItem in pheromoneStack.PheromoneItems)
            {
                Pheromone pheromone = pheromoneStackItem.Pheromone;
                if (!pheromone.PheromoneItems.Remove(pheromoneStackItem.PheromoneItem))
                {

                }
                if (pheromone.PheromoneItems.Count == 0)
                {
                    Items.Remove(pheromone.Pos);
                }                
            }
            pheromoneStacks.Remove(id);
        }

        public void UpdatePheromones(int id, float intensity, float minIntensity = 0)
        {
            PheromoneStack pheromoneStack = pheromoneStacks[id];
            foreach (PheromoneStackItem pheromoneStackItem in pheromoneStack.PheromoneItems)
            {
                float relativIntensity;

                relativIntensity = pheromoneStackItem.Distance * intensity;

                if (relativIntensity > 1)
                    relativIntensity = 1;

                if (relativIntensity < 0)
                    relativIntensity = 0;

                if (relativIntensity < minIntensity)
                    relativIntensity = minIntensity;
                pheromoneStackItem.PheromoneItem.Intensity = relativIntensity;
            }
        }

        public int DropPheromones(Player player, Position pos, int range, PheromoneType pheromoneType, float intensity, bool isStatic, float minIntensity = 0)
        {
            PheromoneStack pheromoneStack = new PheromoneStack();

            PheromoneStack.pheromoneStackCounter++;
            int counter = PheromoneStack.pheromoneStackCounter;
            pheromoneStacks.Add(counter, pheromoneStack);

            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(pos, range, true);
            foreach (TileWithDistance tileWithDistance in tiles.Values)
            {
                float totaldistance = range - tileWithDistance.Distance;
                float distance = (totaldistance * 100) / range / 100;

                if (distance == 0)
                    continue;

                float relativIntensity;
                relativIntensity = distance * intensity;
                if (relativIntensity < minIntensity)
                    relativIntensity = minIntensity;

                if (relativIntensity == 0)
                {
                    continue;
                }

                PheromoneStackItem pheromoneStackItem = new PheromoneStackItem();

                Pheromone pheromone;
                pheromone = FindAt(tileWithDistance.Pos);
                if (pheromone == null)
                {
                    pheromone = new Pheromone();
                    pheromone.Pos = tileWithDistance.Pos;
                    player.Game.Pheromones.Add(pheromone);
                }

                pheromoneStackItem.Pheromone = pheromone;
                pheromoneStackItem.Distance = distance;

                if (isStatic)
                {
                    pheromoneStackItem.PheromoneItem = pheromone.Deposit(player.PlayerModel.Id, relativIntensity, pheromoneType, isStatic);
                }
                else
                {
                    //if (pheromone.GetIntensityF(player.PlayerModel.Id, pheromoneType) < 0.8f)
                    {
                        pheromoneStackItem.PheromoneItem = pheromone.Deposit(player.PlayerModel.Id, relativIntensity, pheromoneType, isStatic);
                    }
                }
                pheromoneStack.PheromoneItems.Add(pheromoneStackItem);
            }

            return counter;
         }

        public void Evaporate()
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


    public class PheromoneStack
    {
        public PheromoneStack()
        {
            PheromoneItems = new List<PheromoneStackItem>();
        }
        public static int pheromoneStackCounter;

        public List<PheromoneStackItem> PheromoneItems { get; private set; }

    }

    public class PheromoneStackItem
    {
        public float Distance { get; set; }
        public Pheromone Pheromone { get; set; }
        public PheromoneItem PheromoneItem { get; set; }
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
                if (PheromoneType == PheromoneType.Energy)
                    Intensity -= Intensity * 0.1f;
                else if (PheromoneType == PheromoneType.Mineral)
                    Intensity -= Intensity * 0.02f;
                else if (PheromoneType == PheromoneType.Container)
                    Intensity -= Intensity * 0.05f; // FOOD_TRAIL_FORGET_RATE
                else if (PheromoneType == PheromoneType.Enemy)
                    Intensity -= Intensity * 0.1f;
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

        public PheromoneItem Deposit(int playerId, float intensity, PheromoneType pheromoneType, bool isStatic)
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
            return pheromoneItem;
        }
        public float GetIntensityF(int playerId, PheromoneType pheromoneType)
        {
            PheromoneType lookForThis = pheromoneType;
            if (pheromoneType == PheromoneType.AwayFromEnergy)
                lookForThis = PheromoneType.Energy;
            if (pheromoneType == PheromoneType.AwayFromEnemy)
                lookForThis = PheromoneType.Enemy;

            float intensity = 0;

            foreach (PheromoneItem pheromoneItem in PheromoneItems)
            {
                if ((playerId == 0 || pheromoneItem.PlayerId == playerId) &&
                    pheromoneItem.PheromoneType == lookForThis)
                {
                    intensity += pheromoneItem.Intensity;
                }
            }
            if (intensity > 1)
                intensity = 1;

            if (pheromoneType == PheromoneType.AwayFromEnergy || pheromoneType == PheromoneType.AwayFromEnemy)
            {
                intensity = 1 - intensity;
                if (intensity < 0.1f)
                    intensity = 0.1f;
            }
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
            List<PheromoneItem> tobeRemoved = new List<PheromoneItem>();

            foreach (PheromoneItem pheromoneItem in PheromoneItems)
            {
                if (pheromoneItem.Evaporate())
                    tobeRemoved.Add(pheromoneItem);
            }
            foreach (PheromoneItem pheromoneItem in tobeRemoved)
            {
                PheromoneItems.Remove(pheromoneItem);
            }
            return PheromoneItems.Count == 0;
        }
    }
}


