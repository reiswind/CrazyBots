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
                    Position pos = move.Positions[0];
                    Tile tile = player.Game.Map.GetTile(pos);

                    MineralDeposit mineralDeposit;
                    if (tile.Metal == 0)
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
                        float intensity = 1f * ((float)tile.Metal / 12);
                        if (intensity > 1) intensity = 1;

                        if (mineralsDeposits.ContainsKey(pos))
                        {
                            mineralDeposit = mineralsDeposits[pos];
                            if (mineralDeposit.Minerals != tile.Metal)
                            {
                                mineralDeposit.Minerals = tile.Metal;

                                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                                mineralDeposit.DepositId = player.Game.Pheromones.DropPheromones(player, pos, 5, PheromoneType.Mineral, intensity, true);
                            }
                        }
                        else
                        {
                            mineralDeposit = new MineralDeposit();

                            mineralDeposit.Minerals = tile.Metal;
                            mineralDeposit.Pos = pos;
                            mineralDeposit.DepositId = player.Game.Pheromones.DropPheromones(player, pos, 5, PheromoneType.Mineral, intensity, true);

                            mineralsDeposits.Add(pos, mineralDeposit);
                        }
                    }
                }

                if (move.MoveType == MoveType.Extract)
                {
                    Position pos = move.Positions[move.Positions.Count - 1];

                }
                if (move.MoveType == MoveType.Build)
                {
                    Position pos = move.Positions[move.Positions.Count - 1];
                    if (CreatedAnts.ContainsKey(pos))
                    {
                        Ant ant = CreatedAnts[pos];
                        CreatedAnts.Remove(pos);
                        Ants.Add(move.UnitId, ant);
                    }
                    else if (player.Units.ContainsKey(pos))
                    {
                        PlayerUnit playerUnit = player.Units[pos];
                        AntFactory ant = Ants[playerUnit.Unit.UnitId] as AntFactory;
                        if (ant != null)
                        {
                            player.UnitsInBuild.Remove(pos);
                            ant.PlayerUnit.Unit.Assembler.Build(move.Stats.BlueprintName);
                        }
                        move.MoveType = MoveType.Skip;
                    }
                    else
                    {
                        //WorkFound(player, pos);
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
        }
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
                ant.PlayerUnit.Unit.Reactor.Container.Mineral < ant.PlayerUnit.Unit.Reactor.Container.Capacity)
            {

                intensity = 1;
                intensity -= (float)ant.PlayerUnit.Unit.Reactor.Container.Mineral / ant.PlayerUnit.Unit.Reactor.Container.Capacity;
                range = 5;
            }

            // Container depends on neighbors
            if (ant.PlayerUnit.Unit.Container != null &&
                ant.PlayerUnit.Unit.Engine == null &&
                ant.PlayerUnit.Unit.Container.Mineral < ant.PlayerUnit.Unit.Container.Capacity)
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
                    ant.PheromoneDepositNeedMinerals = player.Game.Pheromones.DropPheromones(player, ant.PlayerUnit.Unit.Pos, range, PheromoneType.Container, intensity, true);
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
                        intendedMove.UnitId == move.UnitId)
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

        public Position FindCommandTarget(Player player, AntWorker antWorker)
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
                    if (ant is AntWorker)
                        continue;

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
        

        private List<Position> FindMineralForCommand(Player player, AntWorker ant, List<Position> bestPositions)
        {
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition, 3, false, matcher: tile =>
            {
                if (tile.Metal > 0 ||
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

        private List<Position> FindMineralOnMap(Player player, AntWorker ant, List<Position> bestPositions)
        {
            //List<Position> bestPositions = null;
            foreach (Position pos in player.VisiblePositions) // TileWithDistance t in tiles.Values)
            {
                Tile tile = player.Game.Map.GetTile(pos);
                if (tile.Metal > 0 ||
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

        private List<Position> FindMineralDeposit(Player player, AntWorker ant, List<Position> bestPositions)
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

        private List<Position> FindMineralContainer(Player player, AntWorker ant, List<Position> bestPositions)
        {
            // Look for Container with mineraly to refill
            foreach (Ant possibleAnt in Ants.Values)
            {
                AntContainer antContainer = possibleAnt as AntContainer;
                if (antContainer != null &&
                    antContainer.PlayerUnit.Unit.Container != null &&
                    antContainer.PlayerUnit.Unit.Container.Mineral > 0)
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

        public Position FindMineral(Player player, AntWorker ant)
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
                        Move move = new Move();
                        move.MoveType = MoveType.Fire;
                        move.UnitId = ant.PlayerUnit.Unit.UnitId;
                        if (lowestTile.Tile.NumberOfDestructables > 0)
                            move.OtherUnitId = "Destructable";
                        else
                            move.OtherUnitId = "Dirt";

                        move.Positions = new List<Position>();
                        move.Positions.Add(ant.PlayerUnit.Unit.Pos);
                        move.Positions.Add(lowestTile.Tile.Pos);

                        moves.Add(move);
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
                    tile.Unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
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
            AntWorker antWorker = ant as AntWorker;
            if (antWorker != null)
            {
                if (antWorker.AntWorkerType == AntWorkerType.Worker)
                    NumberOfWorkers++;
                if (antWorker.AntWorkerType == AntWorkerType.Fighter)
                    NumberOfFighter++;
                if (antWorker.AntWorkerType == AntWorkerType.Assembler)
                    NumberOfAssembler++;
            }
        }

        private void SacrificeAnt(List<Ant> ants)
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
                AntWorker antWorker = ant as AntWorker;
                if (antWorker != null && antWorker.AntWorkerType != AntWorkerType.Worker)
                {
                    if (ant.PlayerUnit.Unit.Engine != null)
                    {
                        ant.PlayerUnit.Unit.ExtractMe = true;
                        break;
                    }
                }
            }
        }

        private void AttachGamecommands(Player player, List<Ant> unmovedAnts)
        {
            List<GameCommand> assignedCommands = new List<GameCommand>();

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Cancel)
                {
                    foreach (Ant ant in unmovedAnts)
                    {
                        AntWorker antWorker = ant as AntWorker;
                        if (antWorker != null && antWorker.PlayerUnit.Unit.CurrentGameCommand != null)
                        {
                            if (antWorker.PlayerUnit.Unit.CurrentGameCommand.TargetPosition == gameCommand.TargetPosition)
                            {
                                antWorker.PlayerUnit.Unit.CurrentGameCommand = null;
                            }
                        }
                        if (antWorker != null && antWorker.GameCommandDuringCreation != null)
                        {
                            if (antWorker.GameCommandDuringCreation.TargetPosition == gameCommand.TargetPosition)
                            {
                                antWorker.GameCommandDuringCreation = null;
                            }
                        }
                    }
                    assignedCommands.Add(gameCommand);
                }
                if (gameCommand.GameCommandType == GameCommandType.Extract)
                {
                    // TODO

                    assignedCommands.Add(gameCommand);
                }
            }

            // Attach gamecommands to idle units
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Attack ||
                    gameCommand.GameCommandType == GameCommandType.Defend ||
                    gameCommand.GameCommandType == GameCommandType.Scout ||
                    gameCommand.GameCommandType == GameCommandType.Collect)
                {
                    Ant bestAnt = null;
                    double bestDistance = 0;

                    foreach (Ant ant in unmovedAnts)
                    {
                        AntWorker antWorker = ant as AntWorker;
                        if (antWorker != null)
                        {
                            if (gameCommand.GameCommandType == GameCommandType.Attack && antWorker.AntWorkerType == AntWorkerType.Fighter ||
                                gameCommand.GameCommandType == GameCommandType.Defend && antWorker.AntWorkerType == AntWorkerType.Fighter ||
                                gameCommand.GameCommandType == GameCommandType.Scout && antWorker.AntWorkerType == AntWorkerType.Fighter ||
                                gameCommand.GameCommandType == GameCommandType.Collect && antWorker.AntWorkerType == AntWorkerType.Worker)
                            {
                                if (antWorker.PlayerUnit.Unit.CurrentGameCommand == null &&
                                    !ant.PlayerUnit.Unit.UnderConstruction &&
                                    !ant.PlayerUnit.Unit.ExtractMe)
                                {
                                    if (antWorker.PlayerUnit.Unit.Pos == gameCommand.TargetPosition)
                                    {
                                        bestAnt = antWorker;
                                        break;
                                    }
                                    else
                                    {
                                        double distance = antWorker.PlayerUnit.Unit.Pos.GetDistanceTo(gameCommand.TargetPosition);
                                        if (bestAnt == null || distance < bestDistance)
                                        {
                                            bestDistance = distance;
                                            bestAnt = ant;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (bestAnt != null)
                    {
                        assignedCommands.Add(gameCommand);
                        bestAnt.PlayerUnit.Unit.CurrentGameCommand = gameCommand;
                    }
                }
            }
            foreach (GameCommand gameCommand in assignedCommands)
            {
                player.GameCommands.Remove(gameCommand);
            }
        }


        private static int moveNr;
        public MapPlayerInfo MapPlayerInfo { get; set; }

        public List<Move> Turn(Player player)
        {
            moveNr++;
            if (moveNr > 28)
            {

            }


            player.Game.Pheromones.Evaporate();

            // Returned moves
            List<Move> moves = new List<Move>();

            if (!player.Game.GetDebugMapInfo().PlayerInfo.ContainsKey(player.PlayerModel.Id))
            {
                // Player is dead, no more units
                return moves;
            }
            if (player.Game.GetDebugMapInfo().PlayerInfo.Count == 1)
            {
                // Only one Player left. Won the game.
            }

            MapPlayerInfo = player.Game.GetDebugMapInfo().PlayerInfo[player.PlayerModel.Id];

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

                        AntWorker antWorker = ant as AntWorker;
                        if (antWorker != null)
                        {
                            if (antWorker.AntWorkerType == AntWorkerType.None)
                            {
                                if (playerUnit.Unit.Blueprint.Name == "Assembler")
                                {
                                    antWorker.AntWorkerType = AntWorkerType.Assembler;
                                }
                                if (playerUnit.Unit.Blueprint.Name == "Fighter" || playerUnit.Unit.Blueprint.Name == "Bomber")
                                {
                                    antWorker.AntWorkerType = AntWorkerType.Fighter;
                                }
                                if (playerUnit.Unit.Blueprint.Name == "Worker")
                                {
                                    antWorker.AntWorkerType = AntWorkerType.Worker;
                                }
                            }
                        }

                        if (ant.PlayerUnit == null)
                        {
                            // Turned from Ghost to real
                            ant.PlayerUnit = playerUnit;
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
                                AntWorker antWorker = new AntWorker(this);
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
                                AntFactory antFactory = new AntFactory(this, playerUnit);
                                antFactory.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antFactory);
                            }
                            else if (playerUnit.Unit.Blueprint.Name == "Container")
                            {
                                AntContainer antContainer = new AntContainer(this, playerUnit);
                                antContainer.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antContainer);
                            }
                            else if (playerUnit.Unit.Blueprint.Name == "Turret")
                            {
                                AntTurret antTurret = new AntTurret(this, playerUnit);
                                antTurret.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antTurret);
                            }
                            else if (playerUnit.Unit.Blueprint.Name == "Reactor")
                            {
                                AntReactor antReactor = new AntReactor(this, playerUnit);
                                antReactor.Alive = true;
                                Ants.Add(cntrlUnit.UnitId, antReactor);
                            }
                            /*else if (playerUnit.Unit.Engine != null)
                            {
                                AntWorker antWorker = new AntWorker(this);
                                antWorker.PlayerUnit = playerUnit;
                                antWorker.Alive = true;
                                if (playerUnit.Unit.Weapon == null)
                                    antWorker.AntWorkerType = AntWorkerType.Worker;
                                else
                                    antWorker.AntWorkerType = AntWorkerType.Fighter;
                                Ants.Add(cntrlUnit.UnitId, antWorker);
                            }
                            else if (playerUnit.Unit.Weapon != null)
                            {
                                // Defense?
                                AntWorker antWorker = new AntWorker(this);
                                antWorker.PlayerUnit = playerUnit;
                                antWorker.Alive = true;
                                antWorker.AntWorkerType = AntWorkerType.None;
                                Ants.Add(cntrlUnit.UnitId, antWorker);
                            }*/
                        }
                    }
                }
                else if (cntrlUnit.Owner.PlayerModel.Id == 0)
                {
                    // Neutral.
                }
                else
                {
                    player.Game.Pheromones.DropPheromones(player, cntrlUnit.Pos, 15, PheromoneType.Enemy, 0.05f, false);
                }
            }

            CreatedAnts.Clear();

            NumberOfWorkers = 0;
            NumberOfFighter = 0;
            NumberOfAssembler = 0;

            List<Ant> movableAnts = new List<Ant>();
            List<Ant> killedAnts = new List<Ant>();
            List<Ant> unmovedAnts = new List<Ant>();

            foreach (Ant ant in Ants.Values)
            {
                if (!ant.Alive)
                {
                    if (ant.PlayerUnit == null)
                    {
                        // Ghost Ant
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
                        UpdateUnitCounters(ant);

                        ant.UpdateContainerDeposits(player);

                        if (ant.PlayerUnit.Unit.Reactor != null)
                        {
                            if (ant.PheromoneDepositEnergy == 0)
                            {
                                if (ant.PlayerUnit.Unit.Reactor.AvailablePower > 0)
                                {
                                    ant.PheromoneDepositEnergy = player.Game.Pheromones.DropPheromones(player, ant.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Reactor.Range, PheromoneType.Energy, 1, true); //, 0.2f);
                                }
                            }
                            else
                            {
                                if (ant.PlayerUnit.Unit.Reactor.AvailablePower == 0)
                                //if (player.Game.Random.Next(2) == 0)
                                {
                                    player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositEnergy);
                                    ant.PheromoneDepositEnergy = 0;
                                }
                                //ant.PlayerUnit.Unit.Reactor.Power -= 0.01f;
                                //player.Game.Pheromones.UpdatePheromones(ant.PheromoneDepositEnergy, ant.PlayerUnit.Unit.Reactor.Power, 0.2f);
                            }
                        }
                        movableAnts.Add(ant);
                    }
                    else if (ant.PlayerUnit.Unit.UnderConstruction)
                    {
                        UpdateUnitCounters(ant);
                        movableAnts.Add(ant);
                    }
                    else
                    {
                        // Another ant has to take this task
                        if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                        {
                            player.GameCommands.Add(ant.PlayerUnit.Unit.CurrentGameCommand);
                            ant.PlayerUnit.Unit.CurrentGameCommand = null;
                        }

                        if (ant.PlayerUnit.Unit.Engine != null)
                        {
                            movableAnts.Add(ant);
                            ant.PlayerUnit.Unit.ExtractMe = true;
                        }
                        else
                        {
                            ant.PlayerUnit.Unit.ExtractMe = true;
                        }
                    }
                }
            }
            unmovedAnts.AddRange(movableAnts);

            if (MapPlayerInfo.TotalPower == 0)
            {
                // Sacrifice a unit
                SacrificeAnt(unmovedAnts);
            }

            AttachGamecommands(player, unmovedAnts);

            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntFactory)
                {
                    //ant.HandleGameCommands(player);

                    ant.Move(player, moves);
                    movableAnts.Remove(ant);
                }
            }

            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntTurret)
                {
                    ant.Move(player, moves);
                    movableAnts.Remove(ant);
                }
            }

            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntReactor)
                {
                    ant.Move(player, moves);
                    movableAnts.Remove(ant);
                }
            }

            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntContainer)
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
                    if (ant is AntWorker)
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
                /*
                if (workDeposits.ContainsKey(ant.PlayerUnit.Unit.Pos))
                {
                    player.Game.Pheromones.DeletePheromones(workDeposits[ant.PlayerUnit.Unit.Pos]);
                    workDeposits.Remove(ant.PlayerUnit.Unit.Pos);
                }*/

                if (ant.PheromoneDepositEnergy != 0)
                {
                    player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositEnergy);
                    ant.PheromoneDepositEnergy = 0;
                }
                if (ant.PheromoneDepositNeedMinerals != 0)
                {
                    player.Game.Pheromones.DeletePheromones(ant.PheromoneDepositNeedMinerals);
                    ant.PheromoneDepositNeedMinerals = 0;
                }
                if (ant.PheromoneWaypointAttack != 0)
                {
                    player.Game.Pheromones.DeletePheromones(ant.PheromoneWaypointAttack);
                    ant.PheromoneWaypointAttack = 0;
                }
                if (ant.PheromoneWaypointMineral != 0)
                {
                    player.Game.Pheromones.DeletePheromones(ant.PheromoneWaypointMineral);
                    ant.PheromoneWaypointMineral = 0;
                }
                // Another ant has to take this task
                if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                {
                    player.GameCommands.Add(ant.PlayerUnit.Unit.CurrentGameCommand);
                    ant.PlayerUnit.Unit.CurrentGameCommand = null;
                }
                if (ant.GameCommandDuringCreation != null)
                {
                    player.GameCommands.Add(ant.GameCommandDuringCreation);
                    ant.GameCommandDuringCreation = null;
                }
                ant.OnDestroy(player);
                Ants.Remove(ant.PlayerUnit.Unit.UnitId);
            }            
            return moves;
        }
    }
}
