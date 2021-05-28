using Engine.Algorithms;
using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{


    

    internal class ControlAnt : IControl
    {
        public PlayerModel PlayerModel { get; set; }
        public GameModel GameModel { get; set; }
        private IGameController GameController;

        public Dictionary<string, Ant> Ants = new Dictionary<string, Ant>();

        public Dictionary<Position, Ant> CreatedAnts = new Dictionary<Position, Ant>();

        public int MaxWorker = 5;
        public int NumberOfWorkers;

        public ControlAnt(IGameController gameController, PlayerModel playerModel, GameModel gameModel)
        {
            GameController = gameController;
            PlayerModel = playerModel;
            GameModel = gameModel;
        }
        
        public void ProcessMoves(Player player, List<Move> moves)
        {

        }
        

        public static bool HasMoved(List<Move> moves, Unit unit)
        {
            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Move ||
                    intendedMove.MoveType == MoveType.Add)
                {
                    if (intendedMove.UnitId == unit.UnitId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool WillBeOccupied(Player player, List<Move> moves, Position destination)
        {
            bool occupied = false;

            // Check if the
            foreach (PlayerUnit currentUnit in player.Units.Values)
            {
                if (currentUnit.Unit.Pos == destination)
                {
                    if (currentUnit.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                        currentUnit.Unit.Engine != null)
                    {
                        // Our own unit, that has engine may move away
                    }
                    else
                    {
                        // cannot move here or did unit move away?
                        occupied = true;
                    }
                    break;
                }
            }
            if (!occupied)
            {
                foreach (Move intendedMove in moves)
                {
                    if (intendedMove.MoveType == MoveType.Move ||
                        intendedMove.MoveType == MoveType.Add)
                    {
                        if (intendedMove.Positions[intendedMove.Positions.Count - 1] == destination)
                        {
                            occupied = true;
                            break;
                        }
                    }
                }
            }
            return occupied;
        }
        public bool IsExtractable(Player player, Move possibleMove, List<Move> moves)
        {
            bool extractable = true;

            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Upgrade)
                {
                    if (possibleMove.UnitId == intendedMove.UnitId)
                    {
                        extractable = false;
                        break;
                    }
                }
            }
            
            return extractable;
        }

        public bool IsBeingExtracted(List<Move> moves, Position pos)
        {
            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Extract)
                {
                    if (intendedMove.Positions[intendedMove.Positions.Count - 1] == pos)
                    {
                        // Unit should not move until empty
                        return true;
                    }
                }
            }
            return false;
        }

        private Dictionary<Position, int> mineralsDeposits = new Dictionary<Position, int>();

        private Dictionary<Position, int> enemyDeposits = new Dictionary<Position, int>();

        public void EnemyFound(Player player, Position pos)
        {
            if (enemyDeposits.ContainsKey(pos))
            {
                // Update
                player.Game.Pheromones.UpdatePheromones(enemyDeposits[pos], 1);
            }
            else
            {
                int id = player.Game.Pheromones.DropPheromones(player, pos, 3, PheromoneType.Enemy, 1, false);
                enemyDeposits.Add(pos, id);
            }
        }

        public void MineralsFound(Player player, Position pos)
        {
            if (mineralsDeposits.ContainsKey(pos))
            {
                // Update
                player.Game.Pheromones.UpdatePheromones(mineralsDeposits[pos], 1);
            }
            else
            {
                int id = player.Game.Pheromones.DropPheromones(player, pos, 3, PheromoneType.ToFood, 1, false);
                mineralsDeposits.Add(pos, id);
            }
        }

        private void UpdateContainerDeposits(Player player, Ant ant)
        {
            if (ant.PlayerUnit.Unit.Container != null &&
                ant.PlayerUnit.Unit.Engine == null &&
                ant.PlayerUnit.Unit.Container.Metal < ant.PlayerUnit.Unit.Container.Capacity)
            {
                // Standing containers
                if (ant.PheromoneDepositNeedMinerals != 0 &&
                    ant.PheromoneDepositNeedMineralsLevel != ant.PlayerUnit.Unit.Container.Level)
                {
                    player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositNeedMinerals);
                    ant.PheromoneDepositNeedMinerals = 0;
                }
                if (ant.PheromoneDepositNeedMinerals == 0)
                {
                    int range = 2;
                    if (ant.PlayerUnit.Unit.Container.Level == 2)
                        range = 3;
                    else if (ant.PlayerUnit.Unit.Container.Level == 3)
                        range = 4;

                    ant.PheromoneDepositNeedMineralsLevel = ant.PlayerUnit.Unit.Container.Level;
                    int intensity = (ant.PlayerUnit.Unit.Container.Metal * 100 / ant.PlayerUnit.Unit.Container.Capacity) / 100;
                    ant.PheromoneDepositNeedMinerals = player.Game.Pheromones.DropPheromones(player, ant.PlayerUnit.Unit.Pos, range, PheromoneType.ToHome, 1, true);
                }
                else
                {
                    int intensity = (ant.PlayerUnit.Unit.Container.Metal * 100 / ant.PlayerUnit.Unit.Container.Capacity) / 100;
                    player.Game.Pheromones.UpdatePheromones(ant.PheromoneDepositNeedMinerals, 1);
                }
            }
            if (ant.PheromoneDepositNeedMinerals != 0 &&
                (ant.PlayerUnit.Unit.Container == null || ant.PlayerUnit.Unit.Container.Metal >= ant.PlayerUnit.Unit.Container.Capacity))
            {
                // Exits no longer. Remove deposit.
                player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositNeedMinerals);
                ant.PheromoneDepositNeedMinerals = 0;
            }
        }

        public bool IsOccupied(Player player, List<Move> moves, Position destination)
        {
            bool occupied = false;

            // Check if the
            foreach (PlayerUnit currentUnit in player.Units.Values)
            {
                if (currentUnit.Unit.Pos == destination)
                {
                    if (currentUnit.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                        currentUnit.Unit.Engine != null)
                    {
                        occupied = true;

                        // Our own unit, that has engine may move away
                        foreach (Move intendedMove in moves)
                        {
                            if ( (intendedMove.MoveType == MoveType.Move || intendedMove.MoveType == MoveType.Add) &&
                                currentUnit.Unit.UnitId == intendedMove.UnitId)
                            {
                                if (intendedMove.Positions[0] == destination)
                                {
                                    // Unit moves away
                                    occupied = false;
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        // cannot move here or did unit move away?
                        occupied = true;
                    }
                    break;
                }
            }
            if (!occupied)
            {
                foreach (Move intendedMove in moves)
                {
                    if (intendedMove.MoveType == MoveType.Move ||
                        intendedMove.MoveType == MoveType.Add)
                    {
                        if (intendedMove.Positions[intendedMove.Positions.Count-1] == destination)
                        {
                            occupied = true;
                            break;
                        }
                    }
                }
            }
            return occupied;
        }

        private static int moveNr;

        public List<Move> Turn(Player player)
        {
            moveNr++;
            if (moveNr > 17)
            {

            }

            //player.Game.Pheromones.RemoveAllStaticPheromones(player, PheromoneType.Energy);

            player.Game.Pheromones.Evaporate();

            // Returned moves
            List<Move> moves = new List<Move>();

            // List of all units that can be moved
            List<PlayerUnit> moveableUnits = new List<PlayerUnit>();

            if (player.PlayerModel.ControlLevel != 0 && player.WonThisGame())
            {
                // Clean up?
                //player.Commands.Clear();

                // Add Clean area command
                //player.Commands.Add();
            }

            // Remove all spotted enemys
            foreach (int id in enemyDeposits.Values)
            {
                player.Game.Pheromones.DeletePheromones(id);
            }
            enemyDeposits.Clear();

            // Update spotted minerals
            List<Position> removeSpottedMinerals = new List<Position>();
            foreach (Position pos in mineralsDeposits.Keys)
            {
                Tile t = player.Game.Map.GetTile(pos);
                if (t.Metal == 0)
                {
                    removeSpottedMinerals.Add(pos);
                }
            }
            foreach (Position pos in removeSpottedMinerals)
            {
                player.Game.Pheromones.DeletePheromones(mineralsDeposits[pos]);
                mineralsDeposits.Remove(pos);
            }
            enemyDeposits.Clear();

            foreach (Ant ant in Ants.Values)
            {
                ant.MoveAttempts = 0;
                ant.Alive = false;
            }
            foreach (PlayerUnit playerUnit in player.Units.Values)
            {
                Unit cntrlUnit = playerUnit.Unit;
                if (cntrlUnit.Owner.PlayerModel.Id == PlayerModel.Id)
                {
                    playerUnit.PossibleMoves.Clear();
                    moveableUnits.Add(playerUnit);

                    if (Ants.ContainsKey(cntrlUnit.UnitId))
                    {
                        Ant ant = Ants[cntrlUnit.UnitId];
                        ant.Alive = true;
                    }
                    else
                    {
                        if (CreatedAnts.ContainsKey(cntrlUnit.Pos))
                        {
                            // Attach unit
                            Ant ant = CreatedAnts[cntrlUnit.Pos];
                            ant.Alive = true;
                            ant.PlayerUnit = playerUnit;
                            Ants.Add(cntrlUnit.UnitId, ant);
                        }
                        else
                        {
                            if (playerUnit.Unit.Assembler != null)
                            {
                                AntFactory antFactory = new AntFactory(this, playerUnit);
                                antFactory.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antFactory);

                                //Pheromones.DropStaticPheromones(player, cntrlUnit.Pos, 20, PheromoneType.ToHome);
                            }
                            /*
                            else if (playerUnit.Unit.Engine != null)
                            {
                                AntWorker antWorker = new AntWorker(this, playerUnit);
                                antWorker.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antWorker);
                            }*/
                        }
                    }
                }
                else
                {
                    EnemyFound(player, cntrlUnit.Pos);
                }
            }

            CreatedAnts.Clear();

            NumberOfWorkers = 0;

            List<Ant> movableAnts = new List<Ant>();
            List<Ant> killedAnts = new List<Ant>();
            List<Ant> unmovedAnts = new List<Ant>();

            foreach (Ant ant in Ants.Values)
            {
                if (!ant.Alive)
                {
                    killedAnts.Add(ant);
                }
                else
                {
                    if (ant.PlayerUnit.Unit.IsComplete())
                    {
                        AntWorker antWorker = ant as AntWorker;
                        if (antWorker != null)
                        {
                            if (antWorker.IsWorker)
                                NumberOfWorkers++;
                        }

                        UpdateContainerDeposits(player, ant);

                        if (ant.PlayerUnit.Unit.Reactor != null)
                        {
                            if (ant.PheromoneDepositEnergy == 0)
                            {
                                ant.PlayerUnit.Unit.Reactor.Power = 1;
                                ant.PheromoneDepositEnergy = player.Game.Pheromones.DropPheromones(player, ant.PlayerUnit.Unit.Pos, 20, PheromoneType.Energy, ant.PlayerUnit.Unit.Reactor.Power, true);
                            }
                            else
                            {
                                ant.PlayerUnit.Unit.Reactor.Power -= 0.01f;
                                player.Game.Pheromones.UpdatePheromones(ant.PheromoneDepositEnergy, ant.PlayerUnit.Unit.Reactor.Power);
                            }
                        }
                        movableAnts.Add(ant);
                    }
                    else if (ant.PlayerUnit.Unit.UnderConstruction)
                    {
                        if (ant.PlayerUnit.Unit.Engine != null)
                            NumberOfWorkers++;

                        movableAnts.Add(ant);
                    }
                    else
                    {
                        ant.PlayerUnit.Unit.ExtractMe = true;
                    }
                }
            }
            unmovedAnts.AddRange(movableAnts);

            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntFactory)
                {
                    ant.Move(player, moves);
                    movableAnts.Remove(ant);
                }
            }

            unmovedAnts.Clear();    
            unmovedAnts.AddRange(movableAnts);
            while (unmovedAnts.Count > 0)
            {
                foreach (Ant ant in unmovedAnts)
                {
                    if (!(ant is AntFactory))
                    {
                        if (!ant.PlayerUnit.Unit.IsComplete())
                        {
                            movableAnts.Remove(ant);
                            continue;
                        }
                        if (IsBeingExtracted(moves, ant.PlayerUnit.Unit.Pos))
                        {
                            movableAnts.Remove(ant);
                            continue;
                        }

                        if (ant.Move(player, moves))
                        {
                            ant.StuckCounter = 0;
                            movableAnts.Remove(ant);
                        }
                        else
                        {
                            ant.StuckCounter++;
                            if (ant.MoveAttempts == 0)
                            {
                                ant.MoveAttempts++;
                            }
                            else
                            {
                                movableAnts.Remove(ant);
                            }
                        }
                    }
                }
                unmovedAnts.Clear();
                if (movableAnts.Count > 0)
                {
                    unmovedAnts.AddRange(movableAnts);
                }
            }

            foreach (Ant ant in killedAnts)
            {
                Ants.Remove(ant.PlayerUnit.Unit.UnitId);
            }            
            return moves;
        }
    }
}
