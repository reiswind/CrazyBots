using Engine.Algorithms;
using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //public int MaxWorker = 5;
        //public int MaxFighter = 35;
        //public int MaxAssembler = 0;

        public int NumberOfWorkers;
        public int NumberOfFighter;
        public int NumberOfAssembler;
        public int NumberOfReactors;

        public ControlAnt(IGameController gameController, PlayerModel playerModel, GameModel gameModel)
        {
            GameController = gameController;
            PlayerModel = playerModel;
            GameModel = gameModel;
        }

        public void ProcessMoves(Player player, List<Move> moves)
        {
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.UpdateGround)
                {
                    // TEST
                    
                    Position pos = move.Positions[0];
                    Tile tile = player.Game.Map.GetTile(pos);
                    if (tile.Minerals > 0)
                    {
                        player.Game.Pheromones.DropPheromones(player, pos, 10, PheromoneType.Mineral, 0.03f, 0.01f);
                    }
                    /*
                    MineralDeposit mineralDeposit;
                    if (tile.Minerals == 0)
                    {
                        if (mineralsDeposits.ContainsKey(pos))
                        {
                            mineralDeposit = mineralsDeposits[pos];
                            player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);

                            mineralsDeposits.Remove(pos);
                        }
                    }
                    else
                    {
                        float intensity = 1f * ((float)tile.Minerals / 12);
                        if (intensity > 1) intensity = 1;

                        if (mineralsDeposits.ContainsKey(pos))
                        {
                            mineralDeposit = mineralsDeposits[pos];
                            if (mineralDeposit.Minerals != tile.Minerals)
                            {
                                mineralDeposit.Minerals = tile.Minerals;

                                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                                mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, pos, 5, PheromoneType.Mineral, intensity);
                            }
                        }
                        else
                        {
                            mineralDeposit = new MineralDeposit();

                            mineralDeposit.Minerals = tile.Minerals;
                            mineralDeposit.Pos = pos;
                            mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, pos, 5, PheromoneType.Mineral, intensity);

                            mineralsDeposits.Add(pos, mineralDeposit);
                        }
                    }*/
                }
                else if (move.MoveType == MoveType.Extract)
                {
                    Position pos = move.Positions[move.Positions.Count - 1];

                }
                else if (move.MoveType == MoveType.Build)
                {
                    Position pos = move.Positions[move.Positions.Count - 1];
                    if (CreatedAnts.ContainsKey(pos))
                    {
                        Ant ant = CreatedAnts[pos];
                        CreatedAnts.Remove(pos);
                        Ants.Add(move.UnitId, ant);
                    }
                    else if (player.Units.ContainsKey(move.UnitId))
                    {
                        PlayerUnit playerUnit = player.Units[move.UnitId];
                        Ant ant = Ants[playerUnit.Unit.UnitId] as Ant;
                        if (ant != null)
                        {
                            player.UnitsInBuild.Remove(playerUnit.Unit.UnitId);
                            ant.PlayerUnit.Unit.Assembler.Build(move.Stats.BlueprintName);
                        }
                        move.MoveType = MoveType.Skip;
                    }
                    else
                    {
                        //WorkFound(player, pos);
                    }
                }
                else if (move.MoveType == MoveType.Upgrade)
                {
                    PlayerUnit playerUnit = player.Units[move.UnitId];
                    Ant ant = Ants[playerUnit.Unit.UnitId] as Ant;
                    if (ant != null)
                    {
                        ant.CreateAntParts();
                    }
                }
            }
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
                        intendedMove.MoveType == MoveType.Build ||
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
                    if (possibleMove.UnitId == intendedMove.OtherUnitId)
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

        private Dictionary<Position, MineralDeposit> mineralsDeposits = new Dictionary<Position, MineralDeposit>();
        private Dictionary<Position, int> staticMineralDeposits = new Dictionary<Position, int>();
        //private Dictionary<Position, int> workDeposits = new Dictionary<Position, int>();

        private Dictionary<Position, int> enemyDeposits = new Dictionary<Position, int>();

        public void RemoveEnemyFound(Player player, int id)
        {
            foreach (Position pos in enemyDeposits.Keys)
            {
                if (enemyDeposits[pos] == id)
                {
                    enemyDeposits.Remove(pos);
                    break;
                }

            }
            player.Game.Pheromones.DeletePheromones(id);
        }
        public void RemoveMineralsFound(Player player, int id)
        {
            foreach (Position pos in staticMineralDeposits.Keys)
            {
                if (staticMineralDeposits[pos] == id)
                {
                    staticMineralDeposits.Remove(pos);
                    break;
                }

            }
            player.Game.Pheromones.DeletePheromones(id);
        }
        /*
        public int EnemyFound(Player player, Position pos, bool isStatic)
        {
            int id;
            if (enemyDeposits.ContainsKey(pos))
            {
                id = enemyDeposits[pos];
                // Update
                player.Game.Pheromones.UpdatePheromones(id, 0.5f);
                if (isStatic)
                    id = 0;

            }
            else
            {
                id = player.Game.Pheromones.DropPheromones(player, pos, 5, PheromoneType.Enemy, 0.5f, isStatic);
                enemyDeposits.Add(pos, id);
            }
            return id;
        }*/
        /*
        public int MineralsFound(Player player, Position pos, bool isStatic)
        {
            int id;
            if (!isStatic && mineralsDeposits.ContainsKey(pos))
            {
                id = mineralsDeposits[pos];
                //player.Game.Pheromones.UpdatePheromones(id, 1);
                if (isStatic)
                    id = 0;
            }
            else
            {
                if (isStatic)
                {
                    id = player.Game.Pheromones.DropPheromones(player, pos, 3, PheromoneType.Mineral, 0.5f, true, 0.5f);
                    staticMineralDeposits.Add(pos, id);
                }
                else
                {
                    id = player.Game.Pheromones.DropPheromones(player, pos, 3, PheromoneType.Mineral, 1, false);
                    mineralsDeposits.Add(pos, id);
                }
            }
            return id;
        }*/

        /*
        public void WorkFound(Player player, Position pos)
        {
            if (workDeposits.ContainsKey(pos))
            {
                // Update
                //player.Game.Pheromones.UpdatePheromones(workDeposits[pos], 1);
            }
            else
            {
                int id = player.Game.Pheromones.DropPheromones(player, pos, 3, PheromoneType.Work, 1, false);
                workDeposits.Add(pos, id);
            }
        }*/

        private void UpdateContainerDeposits(Player player, Ant ant)
        {
            int range = 0;
            float intensity = 0;

            // Reactor demands Minerals
            if (ant.PlayerUnit.Unit.Reactor != null &&
                ant.PlayerUnit.Unit.Engine == null &&
                ant.PlayerUnit.Unit.Reactor.TileContainer.Minerals < ant.PlayerUnit.Unit.Reactor.TileContainer.Capacity)
            {

                intensity = 1;
                intensity -= (float)ant.PlayerUnit.Unit.Reactor.TileContainer.Minerals / ant.PlayerUnit.Unit.Reactor.TileContainer.Capacity;
                range = 5;
            }

            // Container depends on neighbors
            if (ant.PlayerUnit.Unit.Container != null &&
                ant.PlayerUnit.Unit.Engine == null &&
                ant.PlayerUnit.Unit.Container.TileContainer.Minerals < ant.PlayerUnit.Unit.Container.TileContainer.Capacity)
            {
                range = 2;
                if (ant.PlayerUnit.Unit.Container.Level == 2)
                    range = 3;
                else if (ant.PlayerUnit.Unit.Container.Level == 3)
                    range = 4;
            }

            if (range != 0)
            { 
                // Standing containers
                if (ant.PheromoneDepositNeedMinerals != 0)
                {
                    player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositNeedMinerals);
                    ant.PheromoneDepositNeedMinerals = 0;
                }
                if (ant.PheromoneDepositNeedMinerals == 0)
                {
                    //ant.PheromoneDepositNeedMineralsLevel = ant.PlayerUnit.Unit.Container.Level;
                    //int intensity = (ant.PlayerUnit.Unit.Container.Mineral * 100 / ant.PlayerUnit.Unit.Container.Capacity) / 100;
                    ant.PheromoneDepositNeedMinerals = player.Game.Pheromones.DropStaticPheromones(player, ant.PlayerUnit.Unit.Pos, range, PheromoneType.Container, intensity);
                }
                else
                {
                    //int intensity = (ant.PlayerUnit.Unit.Container.Mineral * 100 / ant.PlayerUnit.Unit.Container.Capacity) / 100;
                    player.Game.Pheromones.UpdatePheromones(ant.PheromoneDepositNeedMinerals, intensity);
                }
            }
            if (ant.PheromoneDepositNeedMinerals != 0 && range == 0)
            {
                // Exits no longer. Remove deposit.
                player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositNeedMinerals);
                ant.PheromoneDepositNeedMinerals = 0;
            }
        }

        public bool IsUpgrading(Player player, List<Move> moves, Move move)
        {
            bool occupied = false;

            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Upgrade)
                {
                    if (intendedMove.Positions[intendedMove.Positions.Count - 1] == move.Positions[intendedMove.Positions.Count - 1] &&
                        intendedMove.OtherUnitId == move.OtherUnitId)
                    {
                        occupied = true;
                        break;
                    }
                }
            }
            return occupied;
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
                            if ((intendedMove.MoveType == MoveType.Move || intendedMove.MoveType == MoveType.Add || intendedMove.MoveType == MoveType.Build) &&
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
                    if (intendedMove.MoveType == MoveType.Upgrade)
                    {
                        if (intendedMove.Positions[intendedMove.Positions.Count - 1] == destination)
                        {
                            occupied = true;
                            break;
                        }
                    }
                    if (intendedMove.MoveType == MoveType.Move ||
                        intendedMove.MoveType == MoveType.Add ||
                        intendedMove.MoveType == MoveType.Build)
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

        public Position FindReactor(Player player, Ant antWorker)
        {
            List<Position> bestPositions = null;

            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit == null || ant.PlayerUnit.Unit.Reactor == null || ant.PlayerUnit.Unit.Reactor.AvailablePower == 0)
                    continue;

                // Distance at all
                Position posFactory = ant.PlayerUnit.Unit.Pos;
                double d = posFactory.GetDistanceTo(antWorker.PlayerUnit.Unit.Pos);
                //if (d < 28)
                {
                    List<Position> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, posFactory, antWorker.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
            }
            return MakePathFromPositions(bestPositions, antWorker);
        }

        public Position FindCommandTarget(Player player, Ant antWorker)
        {
            // Compute route to target
            List<Position> bestPositions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, antWorker.PlayerUnit.Unit.CurrentGameCommand.TargetPosition, antWorker.PlayerUnit.Unit);
            if (bestPositions != null && antWorker.AntWorkerType == AntWorkerType.Assembler)
            {
                if (bestPositions.Count <= 2)
                {
                    return null;
                }
                else
                {
                    // Move only next to target       
                    bestPositions.RemoveAt(bestPositions.Count - 1);
                }
            }            
            return MakePathFromPositions(bestPositions, antWorker);
        }
    

        public Position FindContainer(Player player, Ant antWorker)
        {
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(antWorker.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                // If engine is not null, could be a friendly unit that needs refuel.
                if (tile.Unit != null && tile.Unit.IsComplete() &&
                        tile.Unit.Engine == null &&
                        tile.Unit.CanFill())
                    return true;
                return false;
            });

            List<Position> bestPositions = null;

            foreach (TileWithDistance t in tiles.Values)
            {
                List<Position> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, t.Pos, antWorker.PlayerUnit.Unit);
                if (bestPositions == null || bestPositions.Count > positions?.Count)
                {
                    bestPositions = positions;
                    //break;
                }
            }
            if (bestPositions == null)
            {                
                foreach (Ant ant in Ants.Values)
                {
                    if (ant.PlayerUnit == null) // Ghost
                        continue;
                    //if (ant is AntWorker)
                    //    continue;

                    if (ant.PlayerUnit.Unit.IsComplete() &&
                        ant.PlayerUnit.Unit.Engine == null &&
                        ant.PlayerUnit.Unit.CanFill())
                    {
                        // Distance at all
                        Position posFactory = ant.PlayerUnit.Unit.Pos;
                        double d = posFactory.GetDistanceTo(antWorker.PlayerUnit.Unit.Pos);
                        if (d < 18)
                        {
                            List<Position> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, posFactory, antWorker.PlayerUnit.Unit);
                            if (positions != null && positions.Count > 2)
                            {
                                if (bestPositions == null || bestPositions.Count > positions?.Count)
                                {
                                    bestPositions = positions;
                                }
                            }
                        }
                    }
                }
            }
            return MakePathFromPositions(bestPositions, antWorker);
        }

        public Position FindWork(Player player, Ant ant)
        {
            /*
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                // Own damaged units
                if (tile.Unit != null &&
                    tile.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                    !tile.Unit.IsComplete())
                {
                    return true;
                }
                return false;
            });

            List<Position> bestPositions = null;

            foreach (TileWithDistance t in tiles.Values)
            {*/
            List<Position> bestPositions = null;
            foreach (PlayerUnit playerUnit in player.UnitsInBuild.Values)
            {
                List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, playerUnit.Unit.Pos, ant.PlayerUnit.Unit);
                if (bestPositions == null || bestPositions.Count > positions?.Count)
                {
                    bestPositions = positions;
                }
            }
            if (bestPositions == null)
            {
                foreach (Ant otherAnt in Ants.Values)
                {
                    if (otherAnt.PlayerUnit.Unit.UnderConstruction)
                    {
                        List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, otherAnt.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit);
                        if (bestPositions == null || bestPositions.Count > positions?.Count)
                        {
                            bestPositions = positions;
                        }
                    }
                }
            }
            /*
            if (bestPositions == null)
            {
                foreach (Position pos in workDeposits.Keys)
                {
                    // Distance at all
                    double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                    //if (d < 18)
                    {
                        List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                        if (positions != null && positions.Count > 2)
                        {
                            if (bestPositions == null || bestPositions.Count > positions.Count)
                            {
                                bestPositions = positions;
                            }
                        }
                    }
                }
            }*/
            return MakePathFromPositions(bestPositions, ant);
        }
        

        private List<Position> FindMineralForCommand(Player player, Ant ant, List<Position> bestPositions)
        {
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition, 3, false, matcher: tile =>
            {
                if (tile.Minerals > 0 ||
                    (tile.Unit != null && (tile.Unit.ExtractMe || tile.Unit.Owner.PlayerModel.Id == 0)))
                {
                    List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, tile.Pos, ant.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
                return false;
            });

            return bestPositions;
        }

        /// <summary>
        /// Expensive with many units and no minerals
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ant"></param>
        /// <param name="bestPositions"></param>
        /// <returns></returns>
        private List<Position> FindMineralOnMap(Player player, Ant ant, List<Position> bestPositions)
        {
            // TEST
            //return bestPositions;

            
            foreach (Position pos in player.VisiblePositions) // TileWithDistance t in tiles.Values)
            {
                Tile tile = player.Game.Map.GetTile(pos);
                if (tile.Minerals > 0 ||
                    (tile.Unit != null && (tile.Unit.ExtractMe || tile.Unit.Owner.PlayerModel.Id == 0)))
                {
                    List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, tile.Pos, ant.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
            }
            return bestPositions;
        }

        private List<Position> FindMineralDeposit(Player player, Ant ant, List<Position> bestPositions)
        {
            //List<Position> bestPositions = null;

            foreach (Position pos in staticMineralDeposits.Keys)
            {
                // Distance at all
                double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                if (d < 18)
                {
                    List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                    if (positions != null && positions.Count > 2)
                    {
                        if (bestPositions == null || bestPositions.Count > positions?.Count)
                        {
                            bestPositions = positions;
                        }
                    }
                }
            }
            if (bestPositions == null)
            {
                // Check own markers
                foreach (Position pos in mineralsDeposits.Keys)
                {
                    // Distance at all
                    double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                    if (d < 5)
                    {
                        List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                        if (positions != null && positions.Count > 2)
                        {
                            if (bestPositions == null || bestPositions.Count > positions?.Count)
                            {
                                bestPositions = positions;
                            }
                        }
                    }
                }
            }
            return bestPositions;
        }

        private List<Position> FindMineralContainer(Player player, Ant ant, List<Position> bestPositions)
        {
            // Look for Container with mineraly to refill
            foreach (Ant antContainer in Ants.Values)
            {
                if (antContainer != null &&
                    !antContainer.UnderConstruction &&
                    antContainer.PlayerUnit.Unit.Container != null &&
                    antContainer.PlayerUnit.Unit.Container.TileContainer.Minerals > 0)
                {
                    List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, antContainer.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit);
                    if (positions != null && positions.Count > 2)
                    {
                        if (bestPositions == null || bestPositions.Count > positions?.Count)
                        {
                            bestPositions = positions;
                        }
                    }
                }
            }
            return bestPositions;
        }

        private Position MakePathFromPositions(List<Position> bestPositions, Ant ant)
        {
            ant.FollowThisRoute = null;

            if (bestPositions != null)
            {
                if (bestPositions.Count > 1)
                {
                    if (bestPositions.Count > 2)
                    {
                        ant.FollowThisRoute = new List<Position>();
                        for (int i = 2; i < bestPositions.Count - 1; i++)
                        {
                            ant.FollowThisRoute.Add(bestPositions[i]);
                        }
                    }
                    return bestPositions[1];
                }
                else if (bestPositions.Count == 1)
                {
                    return bestPositions[0];
                }
            }
            return null;
        }

        public Position FindMineral(Player player, Ant ant)
        {
            List<Position> bestPositions = null;

            if (ant.AntWorkerType == AntWorkerType.Worker)
            {
                if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                {
                    bestPositions = FindMineralForCommand(player, ant, bestPositions);
                }
                else
                {
                    bestPositions = FindMineralOnMap(player, ant, bestPositions);
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
                }
            }
            else if (ant.AntWorkerType == AntWorkerType.Fighter)
            {
                bestPositions = FindMineralOnMap(player, ant, bestPositions);
                //if (bestPositions == null)
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
                //if (bestPositions == null)
                    bestPositions = FindMineralContainer(player, ant, bestPositions);
            }
            else
            {
                bestPositions = FindMineralContainer(player, ant, bestPositions);
                //if (bestPositions == null)
                    bestPositions = FindMineralOnMap(player, ant, bestPositions);
                //if (bestPositions == null)                
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
            }
            return MakePathFromPositions(bestPositions, ant);
        }

        public Position LevelGround(List<Move> moves, Player player, Ant ant)
        {
            Tile cliff = null;
            Tile tile = player.Game.Map.GetTile(ant.PlayerUnit.Unit.Pos);
            foreach (Tile n in tile.Neighbors)
            {
                if (!n.CanMoveTo(tile))
                {
                    // Cliff found.
                    cliff = n;
                    break;
                }
            }

            if (cliff != null)
            {
                /*
            double totalHeight = 0;
            Dictionary<Position, TileWithDistance> tilesx = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition, 3, false, matcher: tile =>
            {
                totalHeight += tile.Tile.Height;
                return true;
            });
            totalHeight /= tilesx.Count;
                */

                if (ant.PlayerUnit.Unit.Extractor != null &&
                    ant.PlayerUnit.Unit.Extractor.CanExtractDirt)
                {
                    /*
                    Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 1, false, matcher: tile =>
                    {
                        foreach (Tile n in tile.Neighbors)
                        {
                            if (n.Unit != null)
                                return false;

                            if ((n.Height - 0.2f) > totalHeight || n.NumberOfDestructables > 0)
                                return true;
                        }
                        return false;
                    });
                    foreach (TileWithDistance tileWithDistance in tiles.Values)
                    {
                        return tileWithDistance.Tile.Pos;
                    }*/
                }
                else if (ant.PlayerUnit.Unit.Weapon != null &&
                    ant.PlayerUnit.Unit.Weapon.WeaponLoaded)
                {
                    // Can't extract. Shot somewhere
                    Dictionary<Position, TileWithDistance> tiles = ant.PlayerUnit.Unit.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Weapon.Range, false, matcher: tilex =>
                    {
                        if (tilex.Unit != null)
                            return false;

                        return true;
                    });

                    /*
                    Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 2, false, matcher: tile =>
                    {
                        if (tile.Unit != null)
                            return false;

                        if ((tile.Tile.Height + 0.1f) < totalHeight || tile.Tile.NumberOfDestructables > 0)
                            return true;
                        return false;
                    });*/
                    TileWithDistance lowestTile = null;

                    foreach (TileWithDistance tileWithDistance in tiles.Values)
                    {
                        if (lowestTile == null)
                        {
                            lowestTile = tileWithDistance;
                        }
                        else if (lowestTile.Tile.Height > tileWithDistance.Tile.Height)
                        {
                            lowestTile = tileWithDistance;
                        }

                    }
                    if (lowestTile != null)
                    { 
                        // TODOMIN
                        /*
                        Move move = new Move();
                        move.MoveType = MoveType.Fire;
                        move.UnitId = ant.PlayerUnit.Unit.UnitId;
                        if (lowestTile.Tile.TileObjects.Count > 0)
                        {
                            foreach (TileObject tileObject in lowestTile.Tile.TileObjects)
                            {
                                if (tileObject.Direction != Direction.C)
                                {
                                    move.OtherUnitId = "Destructable";
                                }
                            }
                            if (move.OtherUnitId == null)
                            {
                                //return null;
                            }
                        }
                        else
                            move.OtherUnitId = "Dirt";

                        move.Positions = new List<Position>();
                        move.Positions.Add(ant.PlayerUnit.Unit.Pos);
                        move.Positions.Add(lowestTile.Tile.Pos);

                        moves.Add(move);
                        */
                    }
                }
            }
            return null;
        }
        public Position FindEnemy(Player player, Ant ant)
        {
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                if (tile.Unit != null &&
                    tile.Unit.Owner.PlayerModel.Id != player.PlayerModel.Id &&
                    tile.Unit.IsComplete())
                    return true;
                return false;
            });

            List<Position> bestPositions = null;

            foreach (TileWithDistance t in tiles.Values)
            {
                List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, t.Pos, ant.PlayerUnit.Unit);
                if (bestPositions == null || bestPositions.Count > positions?.Count)
                {
                    bestPositions = positions;
                    //break;
                }
            }
            if (bestPositions == null)
            {
                foreach (Position pos in enemyDeposits.Keys)
                {
                    // Distance at all
                    double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                    //if (d < 18)
                    {
                        List<Position> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                        if (positions != null && positions.Count > 2)
                        {
                            if (bestPositions == null || bestPositions.Count > positions?.Count)
                            {
                                bestPositions = positions;
                            }
                        }
                    }
                }
            }
            return MakePathFromPositions(bestPositions, ant);
        }

        internal void UpdateUnitCounters(Ant ant)
        {
            if (ant.AntWorkerType == AntWorkerType.Worker)
                NumberOfWorkers++;
            if (ant.AntWorkerType == AntWorkerType.Fighter)
                NumberOfFighter++;
            if (ant.AntWorkerType == AntWorkerType.Assembler)
                NumberOfAssembler++;
            if (ant.PlayerUnit != null)
            {
                if (ant.PlayerUnit.Unit.Reactor != null)
                {
                    NumberOfReactors++;
                }
            }
        }

        private void SacrificeAnt(Player player, List<Ant> ants)
        {
            // Is already sacified?
            foreach (Ant ant in ants)
            {
                if (ant.PlayerUnit.Unit.Engine != null &&
                    ant.PlayerUnit.Unit.ExtractMe)
                {
                    // This one is on its way hopefully
                    return;
                }
            }

            foreach (Ant ant in ants)
            {
                if (ant.AntWorkerType != AntWorkerType.Worker)
                {
                    if (ant.PlayerUnit.Unit.Engine != null)
                    {
                        ant.AbandonUnit(player);
                        break;
                    }
                }
            }
        }

        private bool HasUnitBeenBuilt(Player player, GameCommand gameCommand, Ant ant, List<Move> moves)
        {
            Tile t = player.Game.Map.GetTile(gameCommand.TargetPosition);
            if (t.Unit != null &&
                t.Unit.IsComplete() &&
                t.Unit.Blueprint.Name == gameCommand.UnitId)
            {
                Move commandMove = new Move();
                commandMove.MoveType = MoveType.CommandComplete;
                if (ant != null)
                    commandMove.UnitId = ant.PlayerUnit.Unit.UnitId;
                commandMove.PlayerId = player.PlayerModel.Id;
                commandMove.Positions = new List<Position>();
                commandMove.Positions.Add(gameCommand.TargetPosition);
                moves.Add(commandMove);

                return true;
            }
            return false;
        }

        private void AttachGamecommands(Player player, List<Ant> unmovedAnts, List<Move> moves)
        {
            List<GameCommand> completedCommands = new List<GameCommand>();

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Cancel)
                {
                    foreach (Ant ant in unmovedAnts)
                    {
                        if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                        {
                            if (ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition == gameCommand.TargetPosition)
                            {
                                ant.PlayerUnit.Unit.CurrentGameCommand = null;
                            }
                        }
                        if (ant.GameCommandDuringCreation != null)
                        {
                            if (ant.GameCommandDuringCreation.TargetPosition == gameCommand.TargetPosition)
                            {
                                ant.GameCommandDuringCreation = null;
                            }
                        }
                    }
                    completedCommands.Add(gameCommand);
                }
                if (gameCommand.GameCommandType == GameCommandType.Extract)
                {
                    // TODO

                    completedCommands.Add(gameCommand);
                }
            }

            foreach (Ant ant in unmovedAnts)
            {
                if (ant.PlayerUnit.Unit.CurrentGameCommand != null &&
                    ant.PlayerUnit.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (HasUnitBeenBuilt(player, ant.PlayerUnit.Unit.CurrentGameCommand, ant, moves))
                    {
                        //ant.PlayerUnit.Unit.CurrentGameCommand = null;
                        ant.AbandonUnit(player);
                    }
                }
                if (ant.GameCommandDuringCreation != null &&
                    ant.GameCommandDuringCreation.GameCommandType == GameCommandType.Build)
                {
                    if (HasUnitBeenBuilt(player, ant.GameCommandDuringCreation, ant, moves))
                    {
                        //ant.GameCommandDuringCreation = null;
                        ant.AbandonUnit(player);
                    }
                }
            }

            // Attach gamecommands to idle units
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.AttachedUnits.Count > 0)
                    continue;

                if (gameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (HasUnitBeenBuilt(player, gameCommand, null, moves))
                    {
                        // Building is there. Command complete.
                        completedCommands.Add(gameCommand);
                    }
                }
                if (gameCommand.GameCommandType == GameCommandType.Build)
                {
                    Ant bestAnt = null;
                    double bestDistance = 0;

                    foreach (Ant ant in unmovedAnts)
                    {
                            if (gameCommand.GameCommandType == GameCommandType.Build && ant.AntWorkerType == AntWorkerType.Assembler)
                            {
                                if (ant.PlayerUnit.Unit.CurrentGameCommand == null &&
                                    !ant.PlayerUnit.Unit.UnderConstruction &&
                                    !ant.PlayerUnit.Unit.ExtractMe)
                                {
                                    if (ant.PlayerUnit.Unit.Pos == gameCommand.TargetPosition)
                                    {
                                        bestAnt = ant;
                                        break;
                                    }
                                    else
                                    {
                                        double distance = ant.PlayerUnit.Unit.Pos.GetDistanceTo(gameCommand.TargetPosition);
                                        if (bestAnt == null || distance < bestDistance)
                                        {
                                            bestDistance = distance;
                                            bestAnt = ant;
                                        }
                                    }
                                }
                            }
                        
                    }
                    if (bestAnt != null)
                    {
                        completedCommands.Add(gameCommand);
                        bestAnt.PlayerUnit.Unit.CurrentGameCommand = gameCommand;
                    }
                }

                if (gameCommand.GameCommandType == GameCommandType.Attack ||
                    gameCommand.GameCommandType == GameCommandType.Defend ||
                    gameCommand.GameCommandType == GameCommandType.Scout ||
                    gameCommand.GameCommandType == GameCommandType.Collect)
                {
                    Ant bestAnt = null;
                    double bestDistance = 0;

                    foreach (Ant ant in unmovedAnts)
                    {
                        if (gameCommand.GameCommandType == GameCommandType.Attack && ant.AntWorkerType == AntWorkerType.Fighter ||
                            gameCommand.GameCommandType == GameCommandType.Defend && ant.AntWorkerType == AntWorkerType.Fighter ||
                            gameCommand.GameCommandType == GameCommandType.Scout && ant.AntWorkerType == AntWorkerType.Fighter ||
                            gameCommand.GameCommandType == GameCommandType.Collect && ant.AntWorkerType == AntWorkerType.Worker)
                        {
                            if (ant.PlayerUnit.Unit.CurrentGameCommand == null &&
                                !ant.PlayerUnit.Unit.UnderConstruction &&
                                !ant.PlayerUnit.Unit.ExtractMe)
                            {
                                if (ant.PlayerUnit.Unit.Pos == gameCommand.TargetPosition)
                                {
                                    bestAnt = ant;
                                    break;
                                }
                                else
                                {
                                    double distance = ant.PlayerUnit.Unit.Pos.GetDistanceTo(gameCommand.TargetPosition);
                                    if (bestAnt == null || distance < bestDistance)
                                    {
                                        bestDistance = distance;
                                        bestAnt = ant;
                                    }
                                }
                            }
                        }
                    }

                    if (bestAnt != null)
                    {
                        bestAnt.PlayerUnit.Unit.CurrentGameCommand = gameCommand;
                        gameCommand.AttachedUnits.Add(bestAnt.PlayerUnit.Unit);
                    }
                }
            }
            
            foreach (GameCommand gameCommand in completedCommands)
            {
                player.GameCommands.Remove(gameCommand);
            }
        }

        private void BuildReactor(Player player)
        {
            List<Tile> possiblePositions = new List<Tile>();

            // Find all reactors
            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit != null && ant.PlayerUnit.Unit.Reactor != null)
                {
                    // Find build location
                    Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 6, false);
                    foreach (TileWithDistance tileWithDistance in tiles.Values)
                    {
                        if (tileWithDistance.Distance < 6)
                            continue;
                        if (tileWithDistance.Tile.CanMoveTo(ant.PlayerUnit.Unit.Pos))
                        {
                            possiblePositions.Add(tileWithDistance.Tile);
                        }
                    }
                }
            }
            if (possiblePositions.Count > 0)
            {
                int idx = player.Game.Random.Next(possiblePositions.Count);
                Tile t = possiblePositions[idx];

                GameCommand gameCommand = new GameCommand();
                gameCommand.GameCommandType = GameCommandType.Build;
                gameCommand.TargetPosition = t.Pos;
                gameCommand.UnitId = "Outpost";

                player.GameCommands.Add(gameCommand);

            }
        }


        public bool CheckTransportMove(Ant ant, List<Move> moves)
        {
            bool unitMoved = false;
            Unit cntrlUnit = ant.PlayerUnit.Unit;

            if (cntrlUnit.Engine == null && cntrlUnit.Container != null)
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Container.ComputePossibleMoves(possiblemoves, null, MoveFilter.Transport);
                if (possiblemoves.Count > 0)
                {
                    foreach (Move possibleMove in possiblemoves)
                    {
                        bool skipMove = false;

                        foreach (Move intendedMove in moves)
                        {
                            if (intendedMove.MoveType == MoveType.Extract)
                            {
                                if (intendedMove.Positions[0] == possibleMove.Positions[1])
                                {
                                    // Unit is currently extracting, no need to fill it
                                    skipMove = true;
                                    break;
                                }
                            }
                            if (intendedMove.MoveType == MoveType.Transport)
                            {
                                if (intendedMove.Positions[1] == possibleMove.Positions[1])
                                {
                                    // Unit is filled by transport
                                    skipMove = true;
                                    break;
                                }
                            }
                        }
                        if (skipMove)
                            continue;

                        moves.Add(possibleMove);
                        return true;
                    }
                }
            }

            return unitMoved;
        }

        internal void ConnectNearbyAnts(Ant ant)
        {
            foreach (Ant otherAnt in Ants.Values)
            {
                // Not the same ant
                if (otherAnt == ant) continue;
                // Must be complete
                if (otherAnt.UnderConstruction) continue;                
                // Must be a building
                if (otherAnt.PlayerUnit.Unit.Engine != null) continue;
                // Must be a owned
                if (otherAnt.PlayerUnit.Unit.Owner != ant.PlayerUnit.Unit.Owner) continue;

                double distance = otherAnt.PlayerUnit.Unit.Pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                if (distance > 9) continue;

                ant.ConnectWithAnt(otherAnt);
            }
        }

        internal bool CanBuildReactor(Player player)
        {
            bool checkBuildReactor = false;

            bool alreadyInProgress = false;
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Build &&
                    gameCommand.UnitId.StartsWith("Outpost"))
                {
                    alreadyInProgress = true;
                    break;
                }
            }
            if (!alreadyInProgress)
            {
                foreach (Ant ant in CreatedAnts.Values)
                {
                    if (ant.GameCommandDuringCreation != null &&
                        ant.GameCommandDuringCreation.GameCommandType == GameCommandType.Build &&
                        ant.GameCommandDuringCreation.UnitId.StartsWith("Outpost"))
                    {
                        alreadyInProgress = true;
                        break;
                    }
                }
            }
            if (!alreadyInProgress)
            {
                foreach (Ant ant in Ants.Values)
                {
                    if (ant.GameCommandDuringCreation != null &&
                        ant.GameCommandDuringCreation.GameCommandType == GameCommandType.Build &&
                        ant.GameCommandDuringCreation.UnitId.StartsWith("Outpost"))
                    {
                        alreadyInProgress = true;
                        break;
                    }
                    if (ant.PlayerUnit != null &&
                        ant.PlayerUnit.Unit.CurrentGameCommand != null &&
                        ant.PlayerUnit.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                        ant.PlayerUnit.Unit.CurrentGameCommand.UnitId.StartsWith("Outpost"))
                    {
                        alreadyInProgress = true;
                        break;
                    }
                }
            }
            if (!alreadyInProgress)
            {
                // Need more reactors
                checkBuildReactor = true;
            }
            return checkBuildReactor;
        }


        private static int moveNr;
        public MapPlayerInfo MapPlayerInfo { get; set; }

        public List<Move> Turn(Player player)
        {
            moveNr++;
            if (moveNr > 28)
            {

            }

            // Returned moves
            List<Move> moves = new List<Move>();

            MapInfo mapInfo = player.Game.GetDebugMapInfo();

            if (!mapInfo.PlayerInfo.ContainsKey(player.PlayerModel.Id))
            {
                // Player is dead, no more units
                return moves;
            }
            if (mapInfo.PlayerInfo.Count == 1)
            {
                // Only one Player left. Won the game.
            }

            MapPlayerInfo = mapInfo.PlayerInfo[player.PlayerModel.Id];

            // List of all units that can be moved
            List<PlayerUnit> moveableUnits = new List<PlayerUnit>();

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

                        if (ant.AntWorkerType == AntWorkerType.None)
                        {
                            if (playerUnit.Unit.Blueprint.Name == "Assembler")
                            {
                                ant.AntWorkerType = AntWorkerType.Assembler;
                            }
                            if (playerUnit.Unit.Blueprint.Name == "Fighter" || playerUnit.Unit.Blueprint.Name == "Bomber")
                            {
                                ant.AntWorkerType = AntWorkerType.Fighter;
                            }
                            if (playerUnit.Unit.Blueprint.Name == "Worker")
                            {
                                ant.AntWorkerType = AntWorkerType.Worker;
                            }
                        }

                        if (ant.PlayerUnit == null)
                        {
                            // Turned from Ghost to real                            
                            ant.PlayerUnit = playerUnit;
                            ant.CreateAntParts();
                            ant.PlayerUnit.Unit.CurrentGameCommand = ant.GameCommandDuringCreation;
                            ant.GameCommandDuringCreation = null;
                        }
                    }
                    else
                    {
                        if (CreatedAnts.ContainsKey(cntrlUnit.Pos))
                        {
                            // Attach unit build by factories
                            Ant ant = CreatedAnts[cntrlUnit.Pos];
                            ant.Alive = true;
                            ant.PlayerUnit = playerUnit;
                            ant.PlayerUnit.Unit.CurrentGameCommand = ant.GameCommandDuringCreation;
                            ant.GameCommandDuringCreation = null;
                            Ants.Add(cntrlUnit.UnitId, ant);
                        }
                        else
                        {
                            // Create unit from model
                            if (playerUnit.Unit.Blueprint.Name == "Assembler" ||
                                playerUnit.Unit.Blueprint.Name == "Fighter" ||
                                playerUnit.Unit.Blueprint.Name == "Worker" ||
                                playerUnit.Unit.Blueprint.Name == "Bomber")
                            {
                                Ant antWorker = new Ant(this);
                                antWorker.PlayerUnit = playerUnit;
                                antWorker.Alive = true;

                                if (playerUnit.Unit.Direction == Direction.C)
                                {
                                    playerUnit.Unit.Direction = Direction.SW;
                                }

                                if (playerUnit.Unit.Blueprint.Name == "Assembler")
                                    antWorker.AntWorkerType = AntWorkerType.Assembler;
                                else if (playerUnit.Unit.Blueprint.Name == "Fighter" || playerUnit.Unit.Blueprint.Name == "Bomber") 
                                    antWorker.AntWorkerType = AntWorkerType.Fighter;
                                else if (playerUnit.Unit.Blueprint.Name == "Worker")
                                    antWorker.AntWorkerType = AntWorkerType.Worker;
                                Ants.Add(cntrlUnit.UnitId, antWorker);
                            }                         
                            else if (playerUnit.Unit.Blueprint.Name == "Outpost" ||
                                     playerUnit.Unit.Blueprint.Name == "Factory")
                            {
                                //AntFactory antFactory = new AntFactory(this, playerUnit);
                                Ant antFactory = new Ant(this, playerUnit);
                                antFactory.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antFactory);
                            }
                            else 
                            {
                                Ant ant = new Ant(this, playerUnit);
                                ant.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, ant);
                            }
                        }
                    }
                }
                else if (cntrlUnit.Owner.PlayerModel.Id == 0)
                {
                    // Neutral.
                }
                else
                {
                    player.Game.Pheromones.DropPheromones(player, cntrlUnit.Pos, 15, PheromoneType.Enemy, 0.05f);
                }
            }

            CreatedAnts.Clear();

            NumberOfWorkers = 0;
            NumberOfFighter = 0;
            NumberOfAssembler = 0;

            NumberOfReactors = 0;

            List<Ant> movableAnts = new List<Ant>();
            List<Ant> killedAnts = new List<Ant>();
            List<Ant> unmovedAnts = new List<Ant>();
            List<Ant> lostAnts = new List<Ant>();
            lostAnts.AddRange(Ants.Values);

            foreach (Ant ant in Ants.Values)
            {
                lostAnts.Remove(ant);
                if (!ant.Alive)
                {
                    if (ant.PlayerUnit == null)
                    {
                        // Ghost Ant. Remove it, if is not build in time
                        if (ant.UnderConstruction)
                        {
                            ant.StuckCounter++;
                            if (ant.StuckCounter > 10)
                            {
                                // Noone builds, abandon
                                ant.AbandonUnit(player);
                            }
                        }
                        UpdateUnitCounters(ant);
                    }
                    else
                    {
                        killedAnts.Add(ant);
                    }
                }
                else
                {
                    if (ant.PlayerUnit.Unit.Power == 0)
                    {
                        killedAnts.Add(ant);
                    }
                    else if (ant.PlayerUnit.Unit.IsComplete())
                    {
                        ant.CreateAntParts();

                        if (ant.UnderConstruction)
                        {
                            // First time the unit is complete
                            if (ant.PlayerUnit.Unit.Engine == null)
                            {
                                ConnectNearbyAnts(ant);
                            }
                            ant.UnderConstruction = false;
                        }
                        UpdateUnitCounters(ant);

                        ant.UpdateContainerDeposits(player);

                        if (ant.PlayerUnit.Unit.Reactor != null)
                        {
                            if (ant.PheromoneDepositEnergy == 0)
                            {
                                if (ant.PlayerUnit.Unit.Reactor.AvailablePower > 0)
                                {
                                    ant.PheromoneDepositEnergy = player.Game.Pheromones.DropStaticPheromones(player, ant.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Reactor.Range, PheromoneType.Energy, 1); //, 0.2f);
                                }
                            }
                            else
                            {
                                if (ant.PlayerUnit.Unit.Reactor.AvailablePower == 0)
                                {
                                    player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositEnergy);
                                    ant.PheromoneDepositEnergy = 0;
                                }
                            }
                        }
                        movableAnts.Add(ant);
                    }
                    else if (ant.PlayerUnit.Unit.UnderConstruction)
                    {
                        ant.StuckCounter++;
                        if (ant.StuckCounter > 10)
                        {
                            // Noone builds, abandon
                            ant.AbandonUnit(player);
                        }
                        else
                        {
                            UpdateUnitCounters(ant);
                            // cannot move until complete
                            //movableAnts.Add(ant);
                        }
                    }
                    else
                    {
                        // Another ant has to take this task
                        if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                        {
                            ant.PlayerUnit.Unit.CurrentGameCommand.AttachedUnits.Remove(ant.PlayerUnit.Unit);
                            //player.GameCommands.Add(ant.PlayerUnit.Unit.CurrentGameCommand);
                            ant.PlayerUnit.Unit.CurrentGameCommand = null;
                        }

                        if (ant.PlayerUnit.Unit.Engine != null)
                        {
                            if (ant.PlayerUnit.Unit.Extractor == null &&
                                ant.PlayerUnit.Unit.Weapon != null &&
                                !ant.PlayerUnit.Unit.Weapon.WeaponLoaded)
                            {
                                // Cannot refill to fire, useless unit
                                ant.PlayerUnit.Unit.ExtractUnit();
                            }
                            ant.CreateAntParts();
                            movableAnts.Add(ant);
                        }
                        else
                        {
                            ant.AbandonUnit(player);
                        }
                    }
                }
            }
            unmovedAnts.AddRange(movableAnts);

            if (MapPlayerInfo.TotalPower == 0)
            {
                // Sacrifice a unit
                SacrificeAnt(player, unmovedAnts);
            }

            AttachGamecommands(player, unmovedAnts, moves);

            // Ants that have no units
            if (lostAnts.Count > 0)
            {
                killedAnts.AddRange(lostAnts);
            }
            // Execute all extrctor moves
            foreach (Ant ant in unmovedAnts)
            {
                if (ant.AntPartExtractor != null)
                {
                    if (ant.AntPartExtractor.Move(this, player, moves))
                    {
                        movableAnts.Remove(ant);
                    }
                }
            }

            // Any other moves
            unmovedAnts.Clear();
            unmovedAnts.AddRange(movableAnts);
            foreach (Ant ant in unmovedAnts)
            {
                if (ant.Move(player, moves))
                {
                    movableAnts.Remove(ant);
                }
            }


            /*
            unmovedAnts.Clear();
            unmovedAnts.AddRange(movableAnts);

            foreach (Ant ant in unmovedAnts)
            {
                if (ant.PlayerUnit.Unit.Extractor != null)
                {
                    if (ant.Extract(player, moves))
                    {
                        movableAnts.Remove(ant);
                    }
                }
            }
            */
            /*
            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntFactory)
                {
                    ant.Move(player, moves);
                    movableAnts.Remove(ant);
                }
            }*/

            /* Noo expand*/
            /*
            foreach (Ant ant in unmovedAnts)
            {
                if (checkBuildReactor &&
                    CheckBuildReactorMove(player, ant, moves))
                {
                    checkBuildReactor = false;
                    movableAnts.Remove(ant);
                }
            }

            foreach (Ant ant in unmovedAnts)
            {
                if (CheckTransportMove(ant, moves))
                {
                    movableAnts.Remove(ant);
                }
            }*/

            movableAnts.Clear();
            
            unmovedAnts.Clear();
            //unmovedAnts.AddRange(movableAnts);

            while (unmovedAnts.Count > 0)
            {
                foreach (Ant ant in unmovedAnts)
                {
                    //if (ant is AntWorker)
                    {
                        /*
                        if (!ant.PlayerUnit.Unit.IsComplete())
                        {
                            movableAnts.Remove(ant);
                            continue;
                        }*/
                        //ant.HandleGameCommands(player);
                        
                        /*
                        if (ant.CurrentGameCommand == null && ant.HoldPosition)
                        {
                            movableAnts.Remove(ant);
                            continue;
                        }*/
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
                ant.OnDestroy(player);
                Ants.Remove(ant.PlayerUnit.Unit.UnitId);
            }            
            return moves;
        }
    }
}
