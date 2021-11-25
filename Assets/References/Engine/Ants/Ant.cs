
using Engine.Interface;
using Engine.Master;
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
            Energy = MaxEnergy;

            UnderConstruction = true;
            Control = control;
            AntParts = new List<AntPart>();
        }

        public Ant(ControlAnt control, Unit unit)
        {
            UnderConstruction = true;
            Unit = unit;
            Control = control;
            Energy = MaxEnergy;
            AntParts = new List<AntPart>();


            if (unit.Blueprint.Name == "Assembler")
                AntWorkerType = AntWorkerType.Assembler;
            else if (unit.Blueprint.Name == "Fighter" || unit.Blueprint.Name == "Bomber")
                AntWorkerType = AntWorkerType.Fighter;
            else if (unit.Blueprint.Name == "Worker")
                AntWorkerType = AntWorkerType.Worker;

        }

        public List<AntPart> AntParts { get; private set; }
        public AntPartAssembler AntPartAssembler { get; set; }
        public AntPartReactor AntPartReactor { get; set; }
        public AntPartContainer AntPartContainer { get; set; }
        public AntPartExtractor AntPartExtractor { get; set; }
        public AntPartEngine AntPartEngine { get; set; }
        public AntPartWeapon AntPartWeapon { get; set; }
        public AntPartRadar AntPartRadar { get; set; }
        public AntPartArmor AntPartArmor { get; set; }

        public void ConnectWithAnt(Ant otherAnt)
        {
            return;
            /*
            foreach (AntPart antPart1 in AntParts)
            {
                foreach (AntPart antPart2 in otherAnt.AntParts)
                {
                    AntNetworkConnect antNetworkConnect = new AntNetworkConnect();
                    antNetworkConnect.AntPartTarget = antPart1;
                    antNetworkConnect.AntPartSource = antPart2;
                    antPart1.AntNetworkNode.Connections.Add(antNetworkConnect);

                    antNetworkConnect = new AntNetworkConnect();
                    antNetworkConnect.AntPartTarget = antPart2;
                    antNetworkConnect.AntPartSource = antPart1;
                    antPart2.AntNetworkNode.Connections.Add(antNetworkConnect);

                }
            }*/
        }

        public void ConnectAntParts()
        {
            return;

            // Inside a ant, everything is connected with everything
            /*
            foreach (AntPart antPart1 in AntParts)
            {
                antPart1.AntNetworkNode.Connections.Clear();

                foreach (AntPart antPart2 in AntParts)
                {
                    if (antPart1 != antPart2)
                    {
                        AntNetworkConnect antNetworkConnect = new AntNetworkConnect();
                        antNetworkConnect.AntPartTarget = antPart1;
                        antNetworkConnect.AntPartSource = antPart2;
                        antPart1.AntNetworkNode.Connections.Add(antNetworkConnect);
                    }
                }
            }*/
        }
        public int GetDeliveryScoreForTileObjectType(TileObjectType presentType)
        {
            if (presentType == TileObjectType.Tree)
                return 10;
            if (presentType == TileObjectType.Bush)
                return 10;
            if (presentType == TileObjectType.Mineral)
                return 1;
            return 0;
        }
        public int GetDeliveryScoreForTileObjectType(TileObjectType requestType, TileObjectType presentType)
        {
            if (requestType == presentType)
                return GetDeliveryScoreForTileObjectType(presentType);
            if (requestType == TileObjectType.Burn)
            {
                if (presentType == TileObjectType.Tree) 
                    return GetDeliveryScoreForTileObjectType(presentType);
                if (presentType == TileObjectType.Bush) 
                    return GetDeliveryScoreForTileObjectType(presentType);
                if (presentType == TileObjectType.Mineral)
                    return GetDeliveryScoreForTileObjectType(presentType);
            }
            return 0;
        }
        public int GetDeliveryScore(GameCommand gameCommand)
        {
            int score = 0;

            if (AntPartContainer != null)
            {
                List<TileObject> countedObjects = new List<TileObject>();
                foreach (RecipeIngredient recipeIngredient in gameCommand.RequestedItems)
                {
                    int requestCount = recipeIngredient.Count;
                    foreach (TileObject tileObject in AntPartContainer.Container.TileContainer.TileObjects)
                    {
                        if (countedObjects.Contains(tileObject))
                            continue;

                        int s = GetDeliveryScoreForTileObjectType(recipeIngredient.TileObjectType, tileObject.TileObjectType);
                        if (s > 0)
                        {
                            score += s;
                            countedObjects.Add(tileObject);

                            requestCount--;
                            if (requestCount == 0)
                                break;
                        }
                    }
                }
            }
            if (score > 0)
            {
                if (AntPartEngine != null)
                {
                    // Can deliver immediatly
                    score += 10;
                }
            }
            return score;
        }


        public void CreateAntParts()
        {
            bool changed = false;
            if (Unit.Assembler != null)
            {
                if (AntPartAssembler == null)
                {
                    AntPartAssembler = new AntPartAssembler(this, Unit.Assembler);
                    changed = true;
                }
            }
            else
            {
                if (AntPartAssembler != null)
                {
                    AntPartAssembler = null;
                    changed = true;
                }
            }

            if (Unit.Reactor != null)
            {
                if (AntPartReactor == null)
                {
                    AntPartReactor = new AntPartReactor(this, Unit.Reactor);
                    changed = true;
                }
            }
            else
            {
                if (AntPartReactor != null)
                {
                    AntPartReactor = null;
                    changed = true;
                }
            }

            if (Unit.Container != null)
            {
                if (AntPartContainer == null)
                {
                    AntPartContainer = new AntPartContainer(this, Unit.Container);
                    changed = true;
                }
            }
            else
            {
                if (AntPartContainer != null)
                {
                    AntPartContainer = null;
                    changed = true;
                }
            }

            if (Unit.Extractor != null)
            {
                if (AntPartExtractor == null)
                {
                    AntPartExtractor = new AntPartExtractor(this, Unit.Extractor);
                    changed = true;
                }
            }
            else
            {
                if (AntPartExtractor != null)
                {
                    AntPartExtractor = null;
                    changed = true;
                }
            }

            if (Unit.Engine != null)
            {
                if (AntPartEngine == null)
                {
                    AntPartEngine = new AntPartEngine(this, Unit.Engine);
                    changed = true;
                }
            }
            else
            {
                if (AntPartEngine != null)
                {
                    AntPartEngine = null;
                    changed = true;
                }
            }

            if (Unit.Weapon != null)
            {
                if (AntPartWeapon == null)
                {
                    AntPartWeapon = new AntPartWeapon(this, Unit.Weapon);
                    changed = true;
                }
            }
            else
            {
                if (AntPartWeapon != null)
                {
                    AntPartWeapon = null;
                    changed = true;
                }
            }

            if (Unit.Armor != null)
            {
                if (AntPartArmor == null)
                {
                    AntPartArmor = new AntPartArmor(this, Unit.Armor);
                    changed = true;
                }
            }
            else
            {
                if (AntPartArmor != null)
                {
                    AntPartArmor = null;
                    changed = true;
                }
            }

            if (Unit.Radar != null)
            {
                if (AntPartRadar == null)
                {
                    AntPartRadar = new AntPartRadar(this, Unit.Radar);
                    changed = true;
                }
            }
            else
            {
                if (AntPartRadar != null)
                {
                    AntPartRadar = null;
                    changed = true;
                }
            }


            if (changed)
            {
                AntParts.Clear();
                if (AntPartWeapon != null) AntParts.Add(AntPartWeapon);
                if (AntPartExtractor != null) AntParts.Add(AntPartExtractor);
                if (AntPartReactor != null) AntParts.Add(AntPartReactor);
                if (AntPartAssembler != null) AntParts.Add(AntPartAssembler);
                if (AntPartContainer != null) AntParts.Add(AntPartContainer);
                if (AntPartEngine != null) AntParts.Add(AntPartEngine);
                if (AntPartArmor != null) AntParts.Add(AntPartArmor);
                if (AntPartRadar != null) AntParts.Add(AntPartRadar);

                ConnectAntParts();
            }
        }


        static public float HomeTrailDepositRate = 0.5f;
        static public float FoodTrailDepositRate = 0.2f;
        static public float MaxEnergy = 200;
        public float Energy { get; set; }

        static public float MaxFoodIntensity = 100;
        public float FoodIntensity { get; set; }

        //public bool HoldPosition2 { get; set; }
        public int MoveAttempts { get; set; }
        public int StuckCounter { get; set; }
        public int MovesWithoutCommand { get; set; }
        public bool Alive { get; set; }
        public bool UnderConstruction { get; set; }
        public Unit Unit { get; set; }
        public ControlAnt Control { get; set; }

        //public int PheromoneDepositEnergy { get; set; }
        //public int PheromoneDepositNeedMinerals { get; set; }
        //public int PheromoneWaypointMineral { get; set; }
        //public int PheromoneWaypointAttack { get; set; }
        public bool BuildPositionReached { get; set; }
        public void AbandonUnit(Player player)
        {
            OnDestroy(player);
            if (Unit != null)
                Unit.ExtractUnit();
        }

        public override string ToString()
        {
            if (Unit == null)
            {
                return "Under Construction";
            }
            else
            {
                return Unit.ToString();
            }
        }

        public List<Position2> FollowThisRoute { get; set; }
        public virtual bool Move(Player player, List<Move> moves)
        {
            bool moved = false;
            foreach (AntPart antPart in AntParts)
            {
                if (antPart.Move(Control, player, moves))
                {
                    moved = true;
                    break;
                }
            }
            return moved;
        }

        public virtual void OnDestroy(Player player)
        {
            Unit.ResetGameCommand();
            //RemoveAntFromAllCommands(player);
            foreach (AntPart antPart in AntParts)
            {
                antPart.OnDestroy(player);
            }
            
        }

        public AntWorkerType AntWorkerType { get; set; }
    }

    public class AntCollect
    {
        public int Minerals { get; set; }
        public int AllCollectables { get; set; }
        public int TotalCapacity { get; set; }

        public override string ToString()
        {
            return "Mins: " + Minerals + " " + AllCollectables + "/" + TotalCapacity;
        }
    }

    internal enum AntWorkerType
    {
        None,
        Worker,
        Fighter,
        Assembler
    }
}
