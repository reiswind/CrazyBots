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

        public void ConnectWithAnt(Ant otherAnt)
        {
            foreach (AntPart antPart1 in AntParts)
            {
                foreach (AntPart antPart2 in otherAnt.AntParts)
                {
                    AntNetworkConnect antNetworkConnect = new AntNetworkConnect();
                    antNetworkConnect.AntPartTarget = antPart1;
                    antNetworkConnect.AntPartSource = antPart2;
                    antPart1.AntNetworkNode.Connections.Add(antNetworkConnect);
                }
            }
        }

        public void ConnectAntParts()
        {
            // Inside a ant, everything is connected with everything
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
            }
        }

        public void CreateAntParts()
        {
            bool changed = false;
            if (PlayerUnit.Unit.Assembler != null)
            {
                if (AntPartAssembler == null)
                {
                    AntPartAssembler = new AntPartAssembler(this, PlayerUnit.Unit.Assembler);
                    AntParts.Add(AntPartAssembler);
                    changed = true;
                }
            }
            else
            {
                if (AntPartAssembler != null)
                {
                    AntPartAssembler = null;
                    AntParts.Remove(AntPartAssembler);
                    changed = true;
                }
            }

            if (PlayerUnit.Unit.Reactor != null)
            {
                if (AntPartReactor == null)
                {
                    AntPartReactor = new AntPartReactor(this, PlayerUnit.Unit.Reactor);
                    AntParts.Add(AntPartReactor);
                    changed = true;
                }
            }
            else
            {
                if (AntPartReactor != null)
                {
                    AntPartReactor = null;
                    AntParts.Remove(AntPartReactor);
                    changed = true;
                }
            }

            if (PlayerUnit.Unit.Container != null)
            {
                if (AntPartContainer == null)
                {
                    AntPartContainer = new AntPartContainer(this, PlayerUnit.Unit.Container);
                    AntParts.Add(AntPartContainer);
                    changed = true;
                }
            }
            else
            {
                if (AntPartContainer != null)
                {
                    AntPartContainer = null;
                    AntParts.Remove(AntPartContainer);
                    changed = true;
                }
            }

            if (changed)
                ConnectAntParts();
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

        public void AbendonUnit(Player player)
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
            foreach (AntPart antPart in AntParts)
            {
                antPart.Move(Control, player, moves);
            }
            return true;
        }
        public virtual void UpdateContainerDeposits(Player player)
        {
        }
        public virtual void OnDestroy(Player player)
        {
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
            if (PlayerUnit.Unit.CurrentGameCommand != null)
            {
                player.GameCommands.Add(PlayerUnit.Unit.CurrentGameCommand);
                PlayerUnit.Unit.CurrentGameCommand = null;
            }
            if (GameCommandDuringCreation != null)
            {
                player.GameCommands.Add(GameCommandDuringCreation);
                GameCommandDuringCreation = null;
            }
        }

        internal GameCommand GameCommandDuringCreation;
        public AntWorkerType AntWorkerType { get; set; }

        public bool Extract(Player player, List<Move> moves)
        {
            Unit cntrlUnit = PlayerUnit.Unit;

            if (AntWorkerType == AntWorkerType.Fighter && cntrlUnit.Weapon != null &&
                cntrlUnit.Weapon.TileContainer.Loaded >= cntrlUnit.Weapon.TileContainer.Capacity)
            {
                // Fight, do not extract if can fire
            }
            else
            {
                // only if enemy is close...
                if (false && cntrlUnit.Armor != null && cntrlUnit.Armor.ShieldActive == false && AntWorkerType != AntWorkerType.Worker)
                {
                    // Run away, extract later 
                }
                else
                {
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
                                /*
                                if (mineralMove.OtherUnitId == "Mineral")
                                    mineralmoves.Add(mineralMove);
                                if (mineralMove.OtherUnitId.StartsWith("unit"))*/
                                // Everything
                                mineralmoves.Add(mineralMove);
                            }
                            if (mineralmoves.Count > 0)
                            {
                                int idx = player.Game.Random.Next(mineralmoves.Count);
                                Move move = mineralmoves[idx];
                                moves.Add(move);

                                FollowThisRoute = null;

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
