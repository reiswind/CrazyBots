using Engine.Control;
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

        public Ant(ControlAnt control, PlayerUnit playerUnit)
        {
            UnderConstruction = true;
            PlayerUnit = playerUnit;
            Control = control;

            Energy = MaxEnergy;

            AntParts = new List<AntPart>();
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

        public void CreateAntParts()
        {
            bool changed = false;
            if (PlayerUnit.Unit.Assembler != null)
            {
                if (AntPartAssembler == null)
                {
                    AntPartAssembler = new AntPartAssembler(this, PlayerUnit.Unit.Assembler);
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

            if (PlayerUnit.Unit.Reactor != null)
            {
                if (AntPartReactor == null)
                {
                    AntPartReactor = new AntPartReactor(this, PlayerUnit.Unit.Reactor);
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

            if (PlayerUnit.Unit.Container != null)
            {
                if (AntPartContainer == null)
                {
                    AntPartContainer = new AntPartContainer(this, PlayerUnit.Unit.Container);
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

            if (PlayerUnit.Unit.Extractor != null)
            {
                if (AntPartExtractor == null)
                {
                    AntPartExtractor = new AntPartExtractor(this, PlayerUnit.Unit.Extractor);
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

            if (PlayerUnit.Unit.Engine != null)
            {
                if (AntPartEngine == null)
                {
                    AntPartEngine = new AntPartEngine(this, PlayerUnit.Unit.Engine);
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

            if (PlayerUnit.Unit.Weapon != null)
            {
                if (AntPartWeapon == null)
                {
                    AntPartWeapon = new AntPartWeapon(this, PlayerUnit.Unit.Weapon);
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

            if (PlayerUnit.Unit.Armor != null)
            {
                if (AntPartArmor == null)
                {
                    AntPartArmor = new AntPartArmor(this, PlayerUnit.Unit.Armor);
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

            if (PlayerUnit.Unit.Radar != null)
            {
                if (AntPartRadar == null)
                {
                    AntPartRadar = new AntPartRadar(this, PlayerUnit.Unit.Radar);
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

        //public bool HoldPosition { get; set; }
        public int MoveAttempts { get; set; }
        public int StuckCounter { get; set; }
        public bool Alive { get; set; }
        public bool UnderConstruction { get; set; }
        public PlayerUnit PlayerUnit { get; set; }
        public ControlAnt Control { get; set; }

        public int PheromoneDepositEnergy { get; set; }
        public int PheromoneDepositNeedMinerals { get; set; }
        public int PheromoneWaypointMineral { get; set; }
        public int PheromoneWaypointAttack { get; set; }
        public bool BuildPositionReached { get; set; }
        public void AbandonUnit(Player player)
        {
            OnDestroy(player);
            if (PlayerUnit != null)
                PlayerUnit.Unit.ExtractUnit();
        }

        public override string ToString()
        {
            if (PlayerUnit == null)
            {
                return "Under Construction";
            }
            else
            {
                return PlayerUnit.Unit.ToString();
            }
        }

        public List<Position> FollowThisRoute { get; set; }
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
        public virtual void UpdateContainerDeposits(Player player)
        {
        }
        public virtual void OnDestroy(Player player)
        {
            foreach (AntPart antPart in AntParts)
            {
                antPart.OnDestroy(player);
            }
            if (PheromoneDepositEnergy != 0)
            {
                player.Game.Pheromones.DeletePheromones(PheromoneDepositEnergy);
                PheromoneDepositEnergy = 0;
            }
            if (PheromoneDepositNeedMinerals != 0)
            {
                player.Game.Pheromones.DeletePheromones(PheromoneDepositNeedMinerals);
                PheromoneDepositNeedMinerals = 0;
            }
            if (PheromoneWaypointAttack != 0)
            {
                player.Game.Pheromones.DeletePheromones(PheromoneWaypointAttack);
                PheromoneWaypointAttack = 0;
            }
            if (PheromoneWaypointMineral != 0)
            {
                player.Game.Pheromones.DeletePheromones(PheromoneWaypointMineral);
                PheromoneWaypointMineral = 0;
            }
            // Another ant has to take this task
            if (PlayerUnit != null &&
                PlayerUnit.Unit.CurrentGameCommand != null)
            {
                // Better create new command?
                //player.GameCommands.Add(PlayerUnit.Unit.CurrentGameCommand);
                if (PlayerUnit.Unit.CurrentGameCommand.CommandComplete)
                    player.GameCommands.Remove(PlayerUnit.Unit.CurrentGameCommand);
                PlayerUnit.Unit.CurrentGameCommand = null;
            }
            if (GameCommandDuringCreation != null)
            {
                // Better create new command?
                //player.GameCommands.Add(GameCommandDuringCreation);
                if (GameCommandDuringCreation.CommandComplete)
                    player.GameCommands.Remove(GameCommandDuringCreation);
                GameCommandDuringCreation = null;
            }
        }

        internal GameCommand GameCommandDuringCreation;
        public AntWorkerType AntWorkerType { get; set; }
    }

    internal enum AntWorkerType
    {
        None,
        Worker,
        Fighter,
        Assembler
    }
}
