using Engine.Algorithms;

using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class ControlAnt : IControl
    {
        public PlayerModel PlayerModel { get; set; }
        public GameModel GameModel { get; set; }

        public Dictionary<string, Ant> Ants = new Dictionary<string, Ant>();

        //public int MaxWorker = 5;
        //public int MaxFighter = 35;
        //public int MaxAssembler = 0;

        public int NumberOfWorkers;
        public int NumberOfFighter;
        public int NumberOfAssembler;
        public int NumberOfReactors;

        public ControlAnt(IGameController gameController, PlayerModel playerModel, GameModel gameModel)
        {
            PlayerModel = playerModel;
            GameModel = gameModel;
        }
        public void ProcessMoves(Player player, List<Move> moves)
        {
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Build)
                {
                    if (player.Units.ContainsKey(move.UnitId))
                    {
                        PlayerUnit createdUnit = player.Units[move.UnitId];
                        Ant ant = new Ant(this, createdUnit);
                        Ants.Add(createdUnit.Unit.UnitId, ant);
                    }
                    else
                    {
                        // Why not?
                    }
                    /*
                    ulong pos = move.Positions[move.Positions.Count - 1];
                    if (player.Units.ContainsKey(move.UnitId))
                    {
                        PlayerUnit playerUnit = player.Units[move.UnitId];
                        Ant ant = Ants[playerUnit.Unit.UnitId] as Ant;
                        if (ant != null)
                        {
                            player.UnitsInBuild.Remove(playerUnit.Unit.UnitId);
                            ant.PlayerUnit.Unit.Assembler.Build(move.Stats.BlueprintName);
                        }
                        move.MoveType = MoveType.Skip;
                    }*/
                    
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

        private Dictionary<int, AntCollect> AntCollects = new Dictionary<int, AntCollect>();

        public void UpdatePheromones(Player player)
        {
            AntCollects.Clear();

            List<ulong> minerals = new List<ulong>();
            minerals.AddRange(staticMineralDeposits.Keys);

            foreach (ulong pos in player.VisiblePositions)
            {
                Tile tile = player.Game.Map.GetTile(pos);
                int mins = 0;
                if (tile.Minerals > 0)
                {
                    mins = tile.Minerals;
                }
                if (tile.Unit != null && tile.Unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                {
                    // Count enemy unit a mineral resource
                    mins += tile.Unit.CountMineral();
                }
                if (mins > 0)
                {
                    float intensity = 0.1f * ((float)mins);

                    MineralDeposit mineralDeposit;
                    if (minerals.Contains(pos))
                    {
                        minerals.Remove(pos);
                        mineralDeposit = staticMineralDeposits[pos];

                        if (intensity > mineralDeposit.Intensitiy + 0.1f || intensity < mineralDeposit.Intensitiy - 0.1f)
                        {
                            // update
                            player.Game.Pheromones.UpdatePheromones(mineralDeposit.DepositId, intensity, 0.01f);
                            mineralDeposit.Intensitiy = intensity;
                        }
                    }
                    else
                    {
                        mineralDeposit = new MineralDeposit();

                        int sectorSize = player.Game.Map.SectorSize;

                        mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, pos, sectorSize, PheromoneType.Mineral, intensity, 0.01f);
                        mineralDeposit.Intensitiy = intensity;
                        mineralDeposit.Pos = pos;

                        staticMineralDeposits.Add(pos, mineralDeposit);
                    }
                    AntCollect antCollect;
                    if (!AntCollects.TryGetValue(tile.ZoneId, out antCollect))
                    {
                        antCollect = new AntCollect();
                        AntCollects.Add(tile.ZoneId, antCollect);
                    }
                    antCollect.Minerals += mins;
                }
            }

            foreach (ulong pos in minerals)
            {
                MineralDeposit mineralDeposit = staticMineralDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticMineralDeposits.Remove(pos);
            }

            minerals = new List<ulong>();
            minerals.AddRange(staticContainerDeposits.Keys);

            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit != null &&
                    ant.PlayerUnit.Unit.IsComplete() &&
                    ant.AntPartEngine == null &&
                    ant.AntPartContainer != null && ant.AntPartContainer.Container.TileContainer.IsFreeSpace)
                {
                    int sectorSize = player.Game.Map.SectorSize;
                    float intensity = 0.1f;
                    ulong pos = ant.PlayerUnit.Unit.Pos;

                    MineralDeposit mineralDeposit;
                    if (minerals.Contains(pos))
                    {
                        minerals.Remove(pos);
                        mineralDeposit = staticContainerDeposits[pos];

                        if (intensity > mineralDeposit.Intensitiy + 0.1f || intensity < mineralDeposit.Intensitiy - 0.1f)
                        {
                            // update
                            player.Game.Pheromones.UpdatePheromones(mineralDeposit.DepositId, intensity, 0.01f);
                            mineralDeposit.Intensitiy = intensity;
                        }
                    }
                    else
                    {
                        mineralDeposit = new MineralDeposit();
                        mineralDeposit.Intensitiy = intensity;
                        mineralDeposit.Pos = pos;
                        mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, ant.PlayerUnit.Unit.Pos, sectorSize, PheromoneType.Container, intensity, 0.01f);
                        staticContainerDeposits.Add(ant.PlayerUnit.Unit.Pos, mineralDeposit);
                    }

                }
            }
            foreach (ulong pos in minerals)
            {
                MineralDeposit mineralDeposit = staticContainerDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticContainerDeposits.Remove(pos);
            }


            minerals = new List<ulong>();
            minerals.AddRange(staticReactorDeposits.Keys);

            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit != null &&
                    ant.PlayerUnit.Unit.IsComplete() &&
                    ant.AntPartEngine == null &&
                    ant.AntPartReactor != null)
                {
                    if (ant.PlayerUnit.Unit.Reactor.AvailablePower > 0)
                    {
                        float intensity = 1f;
                        ulong pos = ant.PlayerUnit.Unit.Pos;

                        MineralDeposit mineralDeposit;
                        if (minerals.Contains(pos))
                        {
                            minerals.Remove(pos);
                            mineralDeposit = staticReactorDeposits[pos];

                            if (intensity > mineralDeposit.Intensitiy + 0.1f || intensity < mineralDeposit.Intensitiy - 0.1f)
                            {
                                // update
                                player.Game.Pheromones.UpdatePheromones(mineralDeposit.DepositId, intensity);
                                mineralDeposit.Intensitiy = intensity;
                            }
                        }
                        else
                        {
                            mineralDeposit = new MineralDeposit();
                            mineralDeposit.Intensitiy = intensity;
                            mineralDeposit.Pos = pos;
                            mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, ant.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Reactor.Range, PheromoneType.Energy, intensity);
                            //player.Game.Pheromones.DropStaticPheromones(player, ant.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Reactor.Range, PheromoneType.Energy, 1); //, 0.2f);
                            staticReactorDeposits.Add(ant.PlayerUnit.Unit.Pos, mineralDeposit);
                        }
                    }
                }
            }
            foreach (ulong pos in minerals)
            {
                MineralDeposit mineralDeposit = staticReactorDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticReactorDeposits.Remove(pos);
            }

        }
        public void CreateCommands(Player player)
        {
            foreach (KeyValuePair<int, AntCollect> value in AntCollects)
            {
                int zoneId = value.Key;
                MapZone mapZone = player.Game.Map.Zones[zoneId];
                AntCollect antCollect = value.Value;

                if (antCollect.Minerals > 5)
                {
                    bool commandActive = false;
                    foreach (GameCommand gameCommand in player.GameCommands)
                    {
                        if (gameCommand.GameCommandType == GameCommandType.Collect &&
                            gameCommand.TargetPosition == mapZone.Center)
                        {
                            commandActive = true;
                            break;
                        }
                    }
                    if (!commandActive)
                    {
                        // Create a command to collect the resources
                        foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                        {
                            if (blueprintCommand.GameCommandType == GameCommandType.Collect)
                            {
                                GameCommand gameCommand = new GameCommand(blueprintCommand);

                                gameCommand.GameCommandType = GameCommandType.Collect;
                                gameCommand.TargetZone = zoneId;
                                gameCommand.TargetPosition = mapZone.Center;
                                gameCommand.PlayerId = player.PlayerModel.Id;
                                gameCommand.DeleteWhenFinished = true;

                                player.GameCommands.Add(gameCommand);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (GameCommand gameCommand in player.GameCommands)
                    {
                        if (gameCommand.GameCommandType == GameCommandType.Collect &&
                            gameCommand.TargetPosition == mapZone.Center)
                        {
                            gameCommand.CommandComplete = true;
                            if (gameCommand.DeleteWhenFinished)
                                player.GameCommands.Remove(gameCommand);

                            break;
                        }

                    }

                }
            }
        }

        public void CreateCommandForContainerInZone(Player player, int zoneId)
        {
            if (player.PlayerModel.IsHuman)
                return;
            MapZone mapZone = player.Game.Map.Zones[zoneId];

            /*
            foreach (Tile tileInZone in mapZone.Tiles.Values)
            {
                if (!tileInZone.CanBuild())
                    continue;
                if (tileInZone.Unit != null &&
                    tileInZone.Unit.Container != null &&
                    tileInZone.Unit.Container.Level == 3 &&
                    (tileInZone.Unit.Container.TileContainer.Capacity - tileInZone.Unit.Container.TileContainer.Count > 10))
                {
                    // Duplicate container check failed. If multiple container appear
                    
                }
            }
            */

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Build &&
                    gameCommand.BlueprintCommand.Name == "Container" &&
                    gameCommand.TargetZone == zoneId)
                {
                    // Is already in progress
                    return;
                }
            }

            // List of nearby containers
            List<Ant> neighborContainers = new List<Ant>();
            List<Ant> neighborAssemblers = new List<Ant>();

            foreach (Ant ant in Ants.Values)
            {
                if (ant != null &&
                    !ant.UnderConstruction)
                {
                    if (ant.PlayerUnit.Unit.Assembler != null && ant.PlayerUnit.Unit.Engine == null)
                    {
                        // Not too far away
                        int d = CubePosition.Distance(mapZone.Center, ant.PlayerUnit.Unit.Pos);
                        if (d > 20) continue;

                        neighborAssemblers.Add(ant);
                    }
                    if (ant.PlayerUnit.Unit.Container != null && ant.PlayerUnit.Unit.Engine == null)
                    {
                        // Not too far away
                        int d = CubePosition.Distance(mapZone.Center, ant.PlayerUnit.Unit.Pos);
                        if (d > 20) continue;

                        neighborContainers.Add(ant);
                    }
                }
            }
            

            int bestScore = 0;
            List<ulong> possibleBuildLocations = new List<ulong>();

            List<ulong> suggestedBuildLocations = new List<ulong>();
            suggestedBuildLocations.Add(mapZone.Center);

            CubePosition center = new CubePosition(mapZone.Center);
            AddBuildLocation(suggestedBuildLocations, center, Direction.S);
            AddBuildLocation(suggestedBuildLocations, center, Direction.SE);
            AddBuildLocation(suggestedBuildLocations, center, Direction.SW);
            AddBuildLocation(suggestedBuildLocations, center, Direction.N);
            AddBuildLocation(suggestedBuildLocations, center, Direction.NW);
            AddBuildLocation(suggestedBuildLocations, center, Direction.NE);

            foreach (ulong suggestedBuildLocation in suggestedBuildLocations)
            {
                foreach (Ant ant in neighborContainers)
                {
                    // Draw a line from each container
                    CubePosition to = new CubePosition(suggestedBuildLocation);
                    CubePosition from = new CubePosition(ant.PlayerUnit.Unit.Pos);

                    List<CubePosition> line = FractionalHex.HexLinedraw(from, to);
                    if (line == null || line.Count < 5)
                    {
                        // Minimal distance
                        continue;
                    }
                    for (int n=5; n < line.Count; n++)
                    {
                        Tile tile = player.Game.Map.GetTile(line[n].Pos);
                        if (tile.ZoneId != mapZone.ZoneId)
                            continue;

                        if (!tile.CanBuild())
                            continue;
                        if (tile.Unit != null)
                            continue;

                        // Is it in powered zone?
                        if (tile.Owner != player.PlayerModel.Id)
                            continue;

                        // Is it in powered zone?
                        Pheromone pheromone = player.Game.Pheromones.FindAt(tile.Pos);
                        if (pheromone == null || pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Energy) == 0)
                        {
                            // Cannot build here, no power
                            continue;
                        }

                        // Can the location be reached?
                        bool pathPossible = false;
                        foreach (Ant antAssembler in neighborAssemblers)
                        {
                            List<ulong> positions = player.Game.FindPath(antAssembler.PlayerUnit.Unit.Pos, tile.Pos, null);
                            if (positions == null)
                                continue;
                            pathPossible = true;
                            break;
                        }
                        if (!pathPossible)
                            continue;

                        // Count number of containers in range
                        int score = 0;
                        foreach (Ant antContainer in neighborContainers)
                        {
                            // Not too far away
                            int d = CubePosition.Distance(tile.Pos, antContainer.PlayerUnit.Unit.Pos);
                            if (d <= 12)
                                score++;
                        }
                        if (score > bestScore)
                        {
                            possibleBuildLocations.Clear();
                            bestScore = score;
                            possibleBuildLocations.Add(tile.Pos);
                            // Check this position and so on
                            //buildPosition = line[n].Pos;
                            //break;
                        }
                        else if (score == bestScore)
                        {
                            possibleBuildLocations.Add(tile.Pos);
                        }
                    }

                    
                }
            }

            if (possibleBuildLocations.Count > 0)
            {
                // pick a rondom of the best
                int idx = player.Game.Random.Next(possibleBuildLocations.Count);
                ulong buildPosition = possibleBuildLocations[idx];

                
                foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                {
                    if (blueprintCommand.Name == "Container")
                    {
                        GameCommand gameCommand = new GameCommand(blueprintCommand);

                        // Simple the first one BUILD-STEP1 (KI: Select fixed blueprint)
                        gameCommand.GameCommandType = GameCommandType.Build;
                        gameCommand.TargetPosition = buildPosition;
                        gameCommand.PlayerId = player.PlayerModel.Id;
                        gameCommand.TargetZone = zoneId;
                        gameCommand.DeleteWhenFinished = true;
                        player.GameCommands.Add(gameCommand);
                        
                        break;
                    }
                }
            }
        }

        public void AddBuildLocation(List<ulong> suggestedBuildLocations, CubePosition cubePosition, Direction direction)
        {
            CubePosition ndir = cubePosition;

            for (int i = 0; i < 8; i++)
                ndir = ndir.GetNeighbor(direction);
            suggestedBuildLocations.Add(ndir.Pos);
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

        public bool WillBeOccupied(Player player, List<Move> moves, ulong destination)
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

        public bool IsBeingExtracted(List<Move> moves, ulong pos)
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

        //private Dictionary<ulong, MineralDeposit> mineralsDeposits = new Dictionary<ulong, MineralDeposit>();
        private Dictionary<ulong, MineralDeposit> staticMineralDeposits = new Dictionary<ulong, MineralDeposit>();
        private Dictionary<ulong, MineralDeposit> staticContainerDeposits = new Dictionary<ulong, MineralDeposit>();
        private Dictionary<ulong, MineralDeposit> staticReactorDeposits = new Dictionary<ulong, MineralDeposit>();
        //private Dictionary<ulong, int> workDeposits = new Dictionary<ulong, int>();
        //private Dictionary<ulong, int> enemyDeposits = new Dictionary<ulong, int>();

        /*
        public void RemoveEnemyFound(Player player, int id)
        {
            foreach (ulong pos in enemyDeposits.Keys)
            {
                if (enemyDeposits[pos] == id)
                {
                    enemyDeposits.Remove(pos);
                    break;
                }

            }
            player.Game.Pheromones.DeletePheromones(id);
        }*/
        /*
        public void RemoveMineralsFound(Player player, int id)
        {
            foreach (ulong pos in staticMineralDeposits.Keys)
            {
                if (staticMineralDeposits[pos] == id)
                {
                    staticMineralDeposits.Remove(pos);
                    break;
                }

            }
            player.Game.Pheromones.DeletePheromones(id);
        }*/
        /*
        public int EnemyFound(Player player, ulong pos, bool isStatic)
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
            public int MineralsFound(Player player, ulong pos, bool isStatic)
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
            public void WorkFound(Player player, ulong pos)
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
            /*
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
        }*/

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

        public bool IsOccupied(Player player, List<Move> moves, ulong destination)
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

        public ulong FindReactor(Player player, Ant antWorker)
        {
            return Position.Null;
            /*
            List<ulong> bestPositions = null;

            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit == null || ant.PlayerUnit.Unit.Reactor == null || ant.PlayerUnit.Unit.Reactor.AvailablePower == 0)
                    continue;

                // Distance at all
                ulong posFactory = ant.PlayerUnit.Unit.Pos;
                //double d = posFactory.GetDistanceTo(antWorker.PlayerUnit.Unit.Pos);
                //int d = Cubeulong.Distance(posFactory, antWorker.PlayerUnit.Unit.Pos);
                //if (d < 28)
                {
                    List<ulong> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, posFactory, antWorker.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
            }
            return MakePathFromPositions(bestPositions, antWorker);
            */
        }

        public ulong FindCommandTarget(Player player, Ant antWorker)
        {
            List<ulong> bestPositions = null;
            if (antWorker.PlayerUnit.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
            {
                // Need neighbor pos
                Tile t = player.Game.Map.GetTile(antWorker.PlayerUnit.Unit.CurrentGameCommand.GameCommand.TargetPosition);
                foreach (Tile n in t.Neighbors)
                {
                    bestPositions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, n.Pos, antWorker.PlayerUnit.Unit);
                    if (bestPositions != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                // Compute route to target
                bestPositions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, antWorker.PlayerUnit.Unit.CurrentGameCommand.GameCommand.TargetPosition, antWorker.PlayerUnit.Unit);
            }
            return MakePathFromPositions(bestPositions, antWorker);
        }
    

        public ulong FindContainer(Player player, Ant antWorker)
        {
            return Position.Null;
            /*
            Dictionary<ulong, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(antWorker.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                // If engine is not null, could be a friendly unit that needs refuel.
                if (tile.Unit != null && tile.Unit.IsComplete() &&
                        tile.Unit.Engine == null &&
                        tile.Unit.CanFill())
                    return true;
                return false;
            });

            List<ulong> bestPositions = null;

            foreach (TileWithDistance t in tiles.Values)
            {
                List<ulong> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, t.Pos, antWorker.PlayerUnit.Unit);
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
                        ulong posFactory = ant.PlayerUnit.Unit.Pos;
                        //double d = posFactory.GetDistanceTo(antWorker.PlayerUnit.Unit.Pos);
                        int d = CubePosition.Distance(posFactory, antWorker.PlayerUnit.Unit.Pos);
                        if (d < 18)
                        {
                            if (bestPositions != null)
                            {
                                if (bestPositions.Count < d)
                                {
                                    // No need to look further
                                    continue;
                                }
                            }

                            List<ulong> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, posFactory, antWorker.PlayerUnit.Unit);
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
            */
        }

        private List<ulong> FindMineralForCommand(Player player, Ant ant, List<ulong> bestPositions)
        {
            return null;
            /*
            Dictionary<ulong, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition, player.Game.Map.SectorSize, false, matcher: tile =>
            {
                if (tile.Minerals > 0 ||
                    (tile.Unit != null && (tile.Unit.ExtractMe || tile.Unit.Owner.PlayerModel.Id == 0)))
                {
                    List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, tile.Pos, ant.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
                return false;
            });

            return bestPositions;
            */
        }

        /// <summary>
        /// Expensive with many units and no minerals
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ant"></param>
        /// <param name="bestPositions"></param>
        /// <returns></returns>
        private List<ulong> FindMineralOnMap(Player player, Ant ant, List<ulong> bestPositions)
        {
            // NOT GOOD! TO MUCH TIME
            return null;
            /*
            
            foreach (ulong pos in player.VisiblePositions) // TileWithDistance t in tiles.Values)
            {
                Tile tile = player.Game.Map.GetTile(pos);
                if (tile.Minerals > 0 ||
                    (tile.Unit != null && (tile.Unit.ExtractMe || tile.Unit.Owner.PlayerModel.Id == 0)))
                {
                    List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, tile.Pos, ant.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
            }
            return bestPositions;
            */
        }

        private List<ulong> FindMineralDeposit(Player player, Ant ant, List<ulong> bestPositions)
        {
            return null;
            /*
            // ALSO BAD
            foreach (ulong pos in staticMineralDeposits.Keys)
            {
                // Distance at all
                //double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                int d = CubePosition.Distance(pos, ant.PlayerUnit.Unit.Pos);
                if (d < 18)
                {
                    if (bestPositions != null)
                    {
                        if (bestPositions.Count < d)
                        {
                            // No need to look further
                            continue;
                        }
                    }

                    List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
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
                foreach (ulong pos in mineralsDeposits.Keys)
                {
                    // Distance at all
                    //double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                    int d = CubePosition.Distance(pos, ant.PlayerUnit.Unit.Pos);
                    if (d < 5)
                    {
                        if (bestPositions != null)
                        {
                            if (bestPositions.Count < d)
                            {
                                // No need to look further
                                continue;
                            }
                        }

                        List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
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
            return bestPositions;*/
        }

        private List<ulong> FindMineralContainer(Player player, Ant ant, List<ulong> bestPositions)
        {
            return null;
            /*
            // Look for Container with mineraly to refill
            foreach (Ant antContainer in Ants.Values)
            {
                if (antContainer != null &&
                    !antContainer.UnderConstruction &&
                    antContainer.PlayerUnit.Unit.Container != null &&
                    antContainer.PlayerUnit.Unit.Container.TileContainer.Minerals > 0)
                {
                    List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, antContainer.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit);
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
            */
        }

        private ulong MakePathFromPositions(List<ulong> bestPositions, Ant ant)
        {
            ant.FollowThisRoute = null;

            if (bestPositions != null)
            {
                if (bestPositions.Count > 1)
                {
                    if (bestPositions.Count > 2)
                    {
                        ant.FollowThisRoute = new List<ulong>();
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
            return Position.Null;
        }

        public ulong FindMineral(Player player, Ant ant)
        {
            return Position.Null;
            /*
            List<ulong> bestPositions = null;

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
                //if (bestulongs == null)
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
                //if (bestulongs == null)
                    bestPositions = FindMineralContainer(player, ant, bestPositions);
            }
            else
            {
                bestPositions = FindMineralContainer(player, ant, bestPositions);
                //if (bestulongs == null)
                    bestPositions = FindMineralOnMap(player, ant, bestPositions);
                //if (bestulongs == null)                
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
            }
            return MakePathFromPositions(bestPositions, ant);
            */
        }

        public ulong LevelGround(List<Move> moves, Player player, Ant ant)
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
            Dictionary<ulong, TileWithDistance> tilesx = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.Targetulong, 3, false, matcher: tile =>
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
                    Dictionary<ulong, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 1, false, matcher: tile =>
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
                    Dictionary<ulong, TileWithDistance> tiles = ant.PlayerUnit.Unit.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Weapon.Range, false, matcher: tilex =>
                    {
                        if (tilex.Unit != null)
                            return false;

                        return true;
                    });

                    /*
                    Dictionary<ulong, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 2, false, matcher: tile =>
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

                        move.ulongs = new List<ulong>();
                        move.ulongs.Add(ant.PlayerUnit.Unit.Pos);
                        move.ulongs.Add(lowestTile.Tile.Pos);

                        moves.Add(move);
                        */
                    }
                }
            }
            return Position.Null;
        }
        public ulong FindEnemy(Player player, Ant ant)
        {
            return Position.Null;
            /*
            Dictionary<ulong, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                if (tile.Unit != null &&
                    tile.Unit.Owner.PlayerModel.Id != player.PlayerModel.Id &&
                    tile.Unit.IsComplete())
                    return true;
                return false;
            });

            List<ulong> bestulongs = null;

            foreach (TileWithDistance t in tiles.Values)
            {
                List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, t.Pos, ant.PlayerUnit.Unit);
                if (bestulongs == null || bestulongs.Count > positions?.Count)
                {
                    bestulongs = positions;
                    //break;
                }
            }
            if (bestulongs == null)
            {
                foreach (ulong pos in enemyDeposits.Keys)
                {
                    // Distance at all
                    //double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                    //int d = Cubeulong.Distance(pos, ant.PlayerUnit.Unit.Pos);
                    //if (d < 18)
                    {
                        List<ulong> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                        if (positions != null && positions.Count > 2)
                        {
                            if (bestulongs == null || bestulongs.Count > positions?.Count)
                            {
                                bestulongs = positions;
                            }
                        }
                    }
                }
            }
            return MakePathFromPositions(bestulongs, ant);
            */
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
        /*
        private bool HasUnitBeenBuilt(Player player, GameCommand gameCommand, Ant ant, List<Move> moves)
        {
            if (gameCommand.TargetPosition != Position.Null)
            {
                Tile t = player.Game.Map.GetTile(gameCommand.TargetPosition);
                if (t.Unit != null &&
                    t.Unit.IsComplete() &&
                    t.Unit.Blueprint.Name == gameCommand.UnitId)
                {
                   gameCommand.CommandComplete = true;
                    
                    Move commandMove = new Move();
                    commandMove.MoveType = MoveType.CommandComplete;
                    if (ant != null)
                        commandMove.UnitId = ant.PlayerUnit.Unit.UnitId;
                    commandMove.PlayerId = player.PlayerModel.Id;
                    commandMove.Positions = new List<ulong>();
                    commandMove.Positions.Add(gameCommand.TargetPosition);
                    moves.Add(commandMove);

                    return true;
                }
            }
            return false;
        }*/

        private void AttachGamecommands(Player player, List<Ant> unmovedAnts, List<Move> moves)
        {
            List<GameCommand> completedCommands = new List<GameCommand>();
            List<GameCommand> cancelCommands = new List<GameCommand>();
            List<GameCommand> removeCommands = new List<GameCommand>();

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Move)
                {
                    foreach (GameCommand moveGameCommand in player.GameCommands)
                    {
                        if (moveGameCommand.TargetPosition == gameCommand.TargetPosition)
                        {
                            moveGameCommand.TargetPosition = gameCommand.MoveToPosition;
                            gameCommand.CommandComplete = true;
                            removeCommands.Add(gameCommand);
                            completedCommands.Add(gameCommand);
                            break;
                        }
                    }
                }
                if (gameCommand.GameCommandType == GameCommandType.Cancel)
                {
                    foreach (Ant ant in unmovedAnts)
                    {
                        if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                        {
                            //if (ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition == gameCommand.TargetPosition)
                            {
                                ant.PlayerUnit.Unit.ResetGameCommand();
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
                    gameCommand.CommandComplete = true;
                    completedCommands.Add(gameCommand);
                    cancelCommands.Add(gameCommand);
                }
                if (gameCommand.GameCommandType == GameCommandType.Extract)
                {
                    // TODO

                    completedCommands.Add(gameCommand);
                }
            }

            foreach (GameCommand removeCommand in removeCommands)
            {
                player.GameCommands.Remove(removeCommand);
            }

            foreach (GameCommand cancelGameCommand in cancelCommands)
            {
                foreach (GameCommand gameCommand in player.GameCommands)
                {
                    if (cancelGameCommand.TargetPosition == gameCommand.TargetPosition &&
                        cancelGameCommand.PlayerId == gameCommand.PlayerId)
                    {
                        gameCommand.CommandCanceled = true;
                        completedCommands.Add(gameCommand);
                    }
                }
            }

            foreach (Ant ant in unmovedAnts)
            {
                /*
                if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                {
                    if (ant.PlayerUnit.Unit.CurrentGameCommand.CommandCanceled)
                        ant.PlayerUnit.Unit.ResetGameCommand();

                    if (ant.PlayerUnit.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                    {
                        if (HasUnitBeenBuilt(player, ant.PlayerUnit.Unit.CurrentGameCommand, ant, moves))
                        {
                            //ant.PlayerUnit.Unit.CurrentGameCommand = null;
                            ant.AbandonUnit(player);
                        }
                    }
                }
                if (ant.GameCommandDuringCreation != null)
                {
                    if (ant.GameCommandDuringCreation.CommandCanceled)
                        ant.GameCommandDuringCreation = null;

                    if (ant.GameCommandDuringCreation.GameCommandType == GameCommandType.Build)
                    {
                        if (HasUnitBeenBuilt(player, ant.GameCommandDuringCreation, ant, moves))
                        {
                            //ant.GameCommandDuringCreation = null;
                            ant.AbandonUnit(player);
                        }
                    }
                }*/
            }
            Ant bestAnt;
            int bestDistance;

            // Attach gamecommands to idle units
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (completedCommands.Contains(gameCommand))
                    continue;
                if (gameCommand.CommandCanceled || gameCommand.CommandComplete)
                {
                    completedCommands.Add(gameCommand);
                    continue;
                }

                foreach (GameCommandItem gameCommandItem in gameCommand.GameCommandItems)
                {
                    bool requestAssembler = false;

                    if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.Build &&
                        gameCommandItem.AttachedUnitId != null && gameCommandItem.FactoryUnitId == null)
                    {
                        // Check if the unit to be build is there
                        Unit unit = player.Game.Map.Units.FindUnit(gameCommandItem.AttachedUnitId);
                        if (unit == null)
                        {
                            requestAssembler = true;
                        }
                        else
                        {
                            if (!unit.IsComplete())
                            {
                                requestAssembler = true;
                            }
                        }
                    }
                    if (gameCommandItem.AttachedUnitId == null && gameCommandItem.FactoryUnitId == null)
                    {
                        requestAssembler = true;
                    }
                    if (requestAssembler)
                    { 
                        // Find a factory
                        bestAnt = null;
                        bestDistance = 0;

                        foreach (Ant ant in unmovedAnts)
                        {
                            if (ant.AntPartAssembler != null) //.AntWorkerType == AntWorkerType.Assembler)
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
                                        int distance = CubePosition.Distance(ant.PlayerUnit.Unit.Pos, gameCommand.TargetPosition);
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
                            // Assign the build command to an assembler COMMAND-STEP2 BUILD-STEP2
                            bestAnt.PlayerUnit.Unit.SetGameCommand(gameCommandItem);
                            gameCommandItem.FactoryUnitId = bestAnt.PlayerUnit.Unit.UnitId;
                            //gameCommand.AttachedUnits.Add(bestAnt.PlayerUnit.Unit.UnitId);
                            //completedCommands.Add(gameCommand);
                        }
                    }

                    /*
                    if (gameCommand.GameCommandType == GameCommandType.Attack ||
                        gameCommand.GameCommandType == GameCommandType.Defend ||
                        gameCommand.GameCommandType == GameCommandType.Scout ||
                        gameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        bestAnt = null;
                        bestDistance = 0;

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
                                        //double distance = ant.PlayerUnit.Unit.Pos.GetDistanceTo(gameCommand.Targetulong);
                                        int distance = CubePosition.Distance(ant.PlayerUnit.Unit.Pos, gameCommand.TargetPosition);
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
                            bestAnt.PlayerUnit.Unit.SetGameCommand(gameCommand);
                            blueprintCommandItem.AttachedUnitId = bestAnt.PlayerUnit.Unit.UnitId;
                            completedCommands.Add(gameCommand);
                        }
                    }*/
                }
            }
#if open
            List<GameCommand> addedCommands = new List<GameCommand>();

            // Still open commands
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (completedCommands.Contains(gameCommand))
                    continue;

                foreach (BlueprintCommandItem blueprintCommandItem in gameCommand.BlueprintCommand.Units)
                {
                    if (blueprintCommandItem.FactoryCommand != null || blueprintCommandItem.AttachedUnitId != null)
                        continue;

                    if (gameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        /*
                        if (gameCommand.AttachedUnits.Count > 0)
                        {
                            continue;
                        }*/

                        // Create a command to build a fighter (COMMAND-STEP1)
                        BlueprintCommand blueprintCommand = new BlueprintCommand();
                        blueprintCommand.GameCommandType = GameCommandType.Build;
                        blueprintCommand.Name = "BuildUnitForAttack";
                        blueprintCommand.Units.AddRange(gameCommand.BlueprintCommand.Units);


                        GameCommand newCommand = new GameCommand();

                        newCommand.GameCommandType = GameCommandType.Build;
                        newCommand.TargetPosition = gameCommand.TargetPosition;
                        newCommand.SetCommand(blueprintCommand);
                        newCommand.PlayerId = player.PlayerModel.Id;
                        newCommand.AttachToThisOnCompletion = gameCommand;
                        newCommand.DeleteWhenFinished = true;
                        addedCommands.Add(newCommand);
                        blueprintCommandItem.FactoryCommand = newCommand;
                        //gameCommand.AttachedUnits.Add("CommandId?");
                    }
                    if (gameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        Pheromone pheromone = player.Game.Pheromones.FindAt(gameCommand.TargetPosition);

                        if (pheromone == null || pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Energy) == 0)
                        {
                            // Cannot send units there, build reactor
                        }
                        else
                        {
                            /*
                            if (gameCommand.AttachedUnits.Count > 0)
                            {
                                continue;
                            }*/

                            // Create a command to build a worker that will collect the resources (COMMAND-STEP1)
                            BlueprintCommand blueprintCommand = new BlueprintCommand();
                            blueprintCommand.GameCommandType = GameCommandType.Build;
                            blueprintCommand.Name = "BuildUnitForCollect";
                            blueprintCommand.Units.AddRange(gameCommand.BlueprintCommand.Units);

                            GameCommand newCommand = new GameCommand();

                            newCommand.GameCommandType = GameCommandType.Build;
                            newCommand.TargetPosition = gameCommand.TargetPosition;
                            newCommand.SetCommand(blueprintCommand);
                            newCommand.PlayerId = player.PlayerModel.Id;
                            newCommand.AttachToThisOnCompletion = gameCommand;
                            newCommand.DeleteWhenFinished = true;
                            addedCommands.Add(newCommand);

                            blueprintCommandItem.FactoryCommand = newCommand;
                            //gameCommand.AttachedUnits.Add("CommandId?");
                        }
                    }
                }
            }
            player.GameCommands.AddRange(addedCommands);
#endif
        }
        /*
        private void BuildReactor(Player player)
        {
            List<Tile> possibleulongs = new List<Tile>();

            // Find all reactors
            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit != null && ant.PlayerUnit.Unit.Reactor != null)
                {
                    // Find build location
                    Dictionary<ulong, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 6, false);
                    foreach (TileWithDistance tileWithDistance in tiles.Values)
                    {
                        if (tileWithDistance.Distance < 6)
                            continue;
                        if (tileWithDistance.Tile.CanMoveTo(ant.PlayerUnit.Unit.Pos))
                        {
                            possibleulongs.Add(tileWithDistance.Tile);
                        }
                    }
                }
            }
            if (possibleulongs.Count > 0)
            {
                int idx = player.Game.Random.Next(possibleulongs.Count);
                Tile t = possibleulongs[idx];

                GameCommand gameCommand = new GameCommand();
                gameCommand.GameCommandType = GameCommandType.Build;
                gameCommand.TargetPosition = t.Pos;
                gameCommand.UnitId = "Outpost";
                gameCommand.DeleteWhenFinished = true;
                player.GameCommands.Add(gameCommand);

            }
        }
        */

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

                //double distance = otherAnt.PlayerUnit.Unit.Pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                int distance = CubePosition.Distance(otherAnt.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit.Pos);
                

                if (distance > 9) continue;

                ant.ConnectWithAnt(otherAnt);
            }
        }
        /*
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
        */

        private static int moveNr;
        public MapPlayerInfo MapPlayerInfo { get; set; }
        internal Dictionary<int, AntCollect> exceedingMinerals = new Dictionary<int, AntCollect>();

        private void UpdateAntList(Player player)
        {
            //List<PlayerUnit> moveableUnits = new List<PlayerUnit>();

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
                    //moveableUnits.Add(playerUnit);

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
#if NOTCALLED
                        if (ant.PlayerUnit == null)
                        {
                            int nozcalled = 0;
                            // Turned from Ghost to real                            
                            ant.PlayerUnit = playerUnit;
                            ant.CreateAntParts();
                            ant.PlayerUnit.Unit.SetGameCommand(ant.GameCommandDuringCreation);
                            ant.GameCommandDuringCreation = null;
                            if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                            {
                                GameCommand gameCommand = ant.PlayerUnit.Unit.CurrentGameCommand;

                                foreach (string unitid in gameCommand.AttachedUnits)
                                {
                                    if (unitid.StartsWith("Assembler"))
                                    {
                                        if (gameCommand.AttachToThisOnCompletion != null)
                                        {
                                            string assemblerUnitId = unitid.Substring(10);
                                            PlayerUnit assembler;
                                            if (player.Units.TryGetValue(assemblerUnitId, out assembler))
                                            {
                                                // Unattach it from the assembling unit
                                                assembler.Unit.ResetGameCommand();
                                            }

                                            // Assembler complete. Complete the temp. build command and assign the original build command BUILD-STEP5
                                            gameCommand.CommandComplete = true;

                                            gameCommand.AttachToThisOnCompletion.AttachedUnits.Clear(); // Works with only one
                                            gameCommand.AttachToThisOnCompletion.AttachedUnits.Add(ant.PlayerUnit.Unit.UnitId);
                                            gameCommand.AttachToThisOnCompletion.WaitingForUnit = false;
                                            ant.PlayerUnit.Unit.SetGameCommand(gameCommand.AttachToThisOnCompletion);
                                        }
                                        else
                                        {
                                            // Remove the attached assembler and replace it with the created unit COMMAND-STEP5
                                            // The Unit will then execute the command. Workflow complete
                                            gameCommand.AttachedUnits.Remove(unitid);
                                            gameCommand.AttachedUnits.Add(ant.PlayerUnit.Unit.UnitId);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
#endif
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

                else if (cntrlUnit.Owner.PlayerModel.Id == 0)
                {
                    // Neutral.
                }
                else
                {
                    player.Game.Pheromones.DropPheromones(player, cntrlUnit.Pos, 15, PheromoneType.Enemy, 0.05f);
                }
            }
        }

        public List<Move> Turn(Player player)
        {
            DateTime start = DateTime.Now;
            
            moveNr++;
            if (moveNr == 106)
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

            exceedingMinerals.Clear();

            // List of all units that can be moved
            UpdateAntList(player);

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
                        // Kill useless units
                        if (ant.PlayerUnit.Unit.CurrentGameCommand == null)
                        {
                            if (ant.AntPartEngine != null)
                            {
                                ant.MovesWithoutCommand++;
                                if (ant.MovesWithoutCommand > 100) // && ant.AntWorkerType != AntWorkerType.Fighter)
                                    ant.AbandonUnit(player);
                            }
                        }
                        else
                        {
                            ant.MovesWithoutCommand = 0;
                        }

                        // Report finished units
                        /*
                        if (ant.PlayerUnit.Unit.FinishCommandWhenCompleted != null)
                        {
                            if (ant.PlayerUnit.Unit.FinishCommandWhenCompleted.DeleteWhenFinished)
                                player.GameCommands.Remove(ant.PlayerUnit.Unit.FinishCommandWhenCompleted);

                            //ant.PlayerUnit.Unit.FinishCommandWhenCompleted.AttachedUnits.Clear();
                            //ant.PlayerUnit.Unit.FinishCommandWhenCompleted.AttachedUnits.Add(ant.PlayerUnit.Unit.UnitId);
                            ant.PlayerUnit.Unit.FinishCommandWhenCompleted.CommandComplete = true;
                            ant.PlayerUnit.Unit.FinishCommandWhenCompleted = null;
                        }

                        // If someone completed the command, forget it
                        if (ant.PlayerUnit.Unit.CurrentGameCommand != null &&
                            ant.PlayerUnit.Unit.CurrentGameCommand.CommandComplete)
                        {
                            ant.PlayerUnit.Unit.ResetGameCommand();
                        }
                        */
                        ant.CreateAntParts();

                        if (ant.UnderConstruction)
                        {
                            if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                            {
                                if (ant.PlayerUnit.Unit.CurrentGameCommand.FactoryUnitId != null)
                                {
                                    if (ant.PlayerUnit.Unit.CurrentGameCommand.FactoryUnitId != ant.PlayerUnit.Unit.UnitId)
                                        ant.PlayerUnit.Unit.CurrentGameCommand.FactoryUnitId = null;
                                }
                            }
                            // First time the unit is complete
                            if (ant.PlayerUnit.Unit.Engine == null)
                            {
                                //ConnectNearbyAnts(ant);
                            }
                            ant.UnderConstruction = false;
                        }
                        UpdateUnitCounters(ant);

                        movableAnts.Add(ant);
                    }
                    else if (ant.PlayerUnit.Unit.UnderConstruction)
                    {
                        // Unit in build

                        ant.StuckCounter++;
                        if (ant.StuckCounter > 10)
                        {
                            // Noone builds, abandon
                            //ant.AbandonUnit(player);
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
                        // Damaged Unit

                        // Another ant has to take this task
                        if (ant.PlayerUnit.Unit.CurrentGameCommand != null)
                        {
                            //ant.PlayerUnit.Unit.CurrentGameCommand.AttachedUnits.Remove(ant.PlayerUnit.Unit.UnitId);
                            ant.RemoveAntFromAllCommands(player);
                            ant.PlayerUnit.Unit.ResetGameCommand();
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

            UpdatePheromones(player);
            if (!player.PlayerModel.IsHuman)
                CreateCommands(player);

            // DEBUG! 
            //if (NumberOfWorkers > 0) CreateCommandForContainerInZone(player, player.StartZone.ZoneId);

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
            // Execute 
            foreach (Ant ant in unmovedAnts)
            {
                if (ant.AntPartExtractor != null)
                {
                    if (ant.AntPartExtractor.Move(this, player, moves))
                    {
                        movableAnts.Remove(ant);
                        continue;
                    }
                }
                if (ant.AntPartContainer != null)
                {
                    if (CheckTransportMove(ant, moves))
                    {
                        movableAnts.Remove(ant);
                        continue;
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

            movableAnts.Clear();           
            unmovedAnts.Clear();
            
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
                        if (ant.CurrentGameCommand == null && ant.Holdulong)
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

            // Count capacities 

            foreach (Ant ant in Ants.Values)
            {
                if (ant.AntPartContainer != null)
                {
                    Tile tile = player.Game.Map.GetTile(ant.PlayerUnit.Unit.Pos);
                    AntCollect antCollect;
                    if (!exceedingMinerals.TryGetValue(tile.ZoneId, out antCollect))
                    {
                        antCollect = new AntCollect();
                        exceedingMinerals.Add(tile.ZoneId, antCollect);
                    }
                    if (ant.AntPartEngine != null)
                    {
                        antCollect.AllCollectables += ant.AntPartContainer.Container.TileContainer.Count;
                    }
                    else
                    {
                        antCollect.TotalCapacity += ant.AntPartContainer.Container.TileContainer.Capacity - ant.AntPartContainer.Container.TileContainer.Count;
                    }
                }
            }

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Build)
                {
                    foreach (BlueprintCommandItem blueprintCommandItem in gameCommand.BlueprintCommand.Units)
                    {
                        if (blueprintCommandItem.BlueprintName == "Container")
                        {
                            AntCollect antCollect;
                            if (exceedingMinerals.TryGetValue(gameCommand.TargetZone, out antCollect))
                            {
                                // Container in build
                                antCollect.TotalCapacity += 72;
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, AntCollect> keypair in exceedingMinerals)
            {
                int zoneId = keypair.Key;
                AntCollect antCollect = keypair.Value;

                if (antCollect.TotalCapacity < 10 && antCollect.AllCollectables > 5)
                {
                    CreateCommandForContainerInZone(player, zoneId);
                }
            }

            double timeTaken = (DateTime.Now - start).TotalMilliseconds;
            if (timeTaken > 100)
            {

                int thisMoveNr = moveNr;
            }

            return moves;
        }
    }
}
