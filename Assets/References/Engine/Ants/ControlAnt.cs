//#define MEASURE_TIMINGS
#define MEASURE_TIMINGS1

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
                if (move.MoveType == MoveType.Add)
                {
                    Unit unit = player.Game.Map.Units.FindUnit(move.UnitId);
                    if (unit != null && unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                    {
                        // Add only own units
                        Ant ant = new Ant(this, unit);
                        ant.UnderConstruction = unit.UnderConstruction;
                        ant.CreateAntParts();
                        Ants.Add(unit.UnitId, ant);                        
                    }
                }
                else if (move.MoveType == MoveType.Build)
                {
                    Unit unit = player.Game.Map.Units.FindUnit(move.UnitId);
                    if (unit != null && unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                    {
                        // Add only own units
                        Ant ant = new Ant(this, unit);
                        ant.UnderConstruction = unit.UnderConstruction;
                        ant.CreateAntParts();
                        Ants.Add(unit.UnitId, ant);
                    }
                }
                else if (move.MoveType == MoveType.Upgrade)
                {
                    Unit unit = player.Game.Map.Units.FindUnit(move.OtherUnitId);
                    if (unit != null && unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                    {
                        Ant ant = Ants[unit.UnitId] as Ant;
                        if (ant != null)
                        {
                            ant.CreateAntParts();
                        }
                    }
                }
                else if (move.MoveType == MoveType.Delete)
                {
                    Unit unit = player.Game.Map.Units.FindUnit(move.UnitId);
                    if (unit != null && unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                    {
                        Ants.Remove(move.UnitId);
                    }
                }
            }
        }

        private GameCommand patrolCommand;

        public void CreateCommands(Player player)
        {
            if (patrolCommand == null)
            {
                foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                {
                    if (blueprintCommand.Name == "Fighter")
                    {
                        patrolCommand = new GameCommand(blueprintCommand.Units[0]);
                        patrolCommand.Priority = 0;
                        patrolCommand.FollowPheromones = true;
                        patrolCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                        patrolCommand.AttachedUnit.SetStatus("PatrolFollowPheromones");
                        
                        player.GameCommands.Add(patrolCommand);
                        break;
                    }
                }
            }
            else
            {
                if (MapPlayerInfo.TotalMinerals > 40 && MapPlayerInfo.TotalPower < 80 && MapPlayerInfo.PowerOutInTurns > 40)
                {
                    bool commandComplete = true;

                    if (string.IsNullOrEmpty(patrolCommand.AttachedUnit.UnitId))
                    {
                        commandComplete = false;
                    }

                    if (commandComplete)
                    {
                        patrolCommand.BlueprintName = "Bomber";
                        patrolCommand.FollowPheromones = true;
                        patrolCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;
                        patrolCommand.AttachedUnit.SetStatus("AddPatrolFollowPheromones");
                    }
                }
            }

            if (player.Discoveries.Count > 0)
            {
                List<Position2> enemyUnits = new List<Position2>();

                foreach (PlayerVisibleInfo playerVisibleInfo in player.Discoveries.Values)
                {
                    if (playerVisibleInfo.NumberOfCollectables > 2)
                    {
                        // Collect it
                        CreateCollectCommand(player, playerVisibleInfo.Pos);
                    }
                    if (playerVisibleInfo.Unit != null &&
                        playerVisibleInfo.Unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                    {
                        //if (player.PlayerModel.Id == 1)
                        //    Debug.WriteLine("Enemy " + playerVisibleInfo.Unit.Blueprint.Name + " at " + playerVisibleInfo.Pos.ToString());
                        enemyUnits.Add(playerVisibleInfo.Pos);
                    }
                }
                int maxAttacks = 1;
                int countAttacks = 0;

                //List<GameCommand> notOnEnemy = new List<GameCommand>();
                foreach (GameCommand gameCommand in player.GameCommands)
                {
                    if (gameCommand.GameCommandType == GameCommandType.AttackMove)
                    {
                        countAttacks++;
                        if (enemyUnits.Contains(gameCommand.TargetPosition))
                        {
                            // Command active
                            enemyUnits.Remove(gameCommand.TargetPosition);
                        }
                        else
                        {
                            //gameCommand.CommandCanceled = true;
                            //notOnEnemy.Add(gameCommand);
                        }
                    }
                }
                if (countAttacks < maxAttacks)
                {
                    // List of enemys not attacked
                    foreach (Position2 position2 in enemyUnits)
                    {

                        // 
                        CreateAttackCommand(player, position2);
                    }
                }
            }
        }

        public void UpdatePheromones(Player player)
        {

            /*

            List<Position2> deposits = new List<Position2>();
            deposits.AddRange(staticMineralDeposits.Keys);

            foreach (Position2 pos in player.VisiblePositions.Keys)
            {
                break;

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

                    PheromoneDeposit mineralDeposit;
                    if (deposits.Contains(pos))
                    {
                        deposits.Remove(pos);
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
                        mineralDeposit = new PheromoneDeposit();

                        int sectorSize = player.Game.Map.SectorSize;

                        mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, pos, sectorSize, PheromoneType.Mineral, intensity, 0.01f);
                        mineralDeposit.Intensitiy = intensity;
                        mineralDeposit.Pos = pos;

                        staticMineralDeposits.Add(pos, mineralDeposit);
                    }
                    /*
                    AntCollect antCollect;
                    if (!AntCollects.TryGetValue(tile.ZoneId, out antCollect))
                    {
                        antCollect = new AntCollect();
                        AntCollects.Add(tile.ZoneId, antCollect);
                    }
                    antCollect.Minerals += mins;* /
                }
            }

            foreach (Position2 pos in deposits)
            {
                PheromoneDeposit mineralDeposit = staticMineralDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticMineralDeposits.Remove(pos);
            }
            */

            // Detect missing units with this lines
            List<Position2> deposits = new List<Position2>();
            deposits.AddRange(staticContainerDeposits.Keys);

            foreach (Ant ant in Ants.Values)
            {
                if (ant.Unit != null &&
                    ant.Unit.IsComplete() &&
                    ant.AntPartEngine == null &&
                    ant.AntPartContainer != null && ant.AntPartContainer.Container.TileContainer.IsFreeSpace)
                {
                    int sectorSize = player.Game.Map.SectorSize;

                    float percentFilled = 0;
                    if (ant.AntPartContainer.Container.TileContainer.Capacity > 0)
                        percentFilled = (ant.AntPartContainer.Container.TileContainer.Count * 100) / ant.AntPartContainer.Container.TileContainer.Capacity;

                    float intensity = 0.1f;
                    intensity = (100 - percentFilled) / 1000;
                    Position2 pos = ant.Unit.Pos;

                    PheromoneDeposit mineralDeposit;
                    if (staticContainerDeposits.TryGetValue(pos, out mineralDeposit))
                    {
                        deposits.Remove(pos);

                        float upper = mineralDeposit.Intensitiy + 0.01f;
                        float lower = mineralDeposit.Intensitiy - 0.01f;

                        if (intensity > mineralDeposit.Intensitiy + 0.01f || intensity < mineralDeposit.Intensitiy - 0.01f)
                        {
                            // update
                            player.Game.Pheromones.UpdatePheromones(mineralDeposit.DepositId, intensity, 0.01f);
                            mineralDeposit.Intensitiy = intensity;
                        }
                    }
                    else
                    {
                        mineralDeposit = new PheromoneDeposit();
                        mineralDeposit.Intensitiy = intensity;
                        mineralDeposit.Pos = pos;
                        mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, ant.Unit.Pos, sectorSize, PheromoneType.Container, intensity, 0.01f);
                        staticContainerDeposits.Add(ant.Unit.Pos, mineralDeposit);
                    }
                }
            }
            foreach (Position2 pos in deposits)
            {
                PheromoneDeposit mineralDeposit = staticContainerDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticContainerDeposits.Remove(pos);
            }

            deposits = new List<Position2>();
            deposits.AddRange(staticReactorDeposits.Keys);

            foreach (Ant ant in Ants.Values)
            {
                if (ant.Unit != null &&
                    ant.Unit.IsComplete() &&
                    ant.AntPartEngine == null &&
                    ant.AntPartReactor != null)
                {
                    float intensity = 1f;
                    Position2 pos = ant.Unit.Pos;

                    PheromoneDeposit pheromoneDeposit;
                    if (staticReactorDeposits.TryGetValue(pos, out pheromoneDeposit))
                    {
                        if (ant.Unit.Reactor.AvailablePower > 0)
                        {
                            deposits.Remove(pos);

                            if (intensity > pheromoneDeposit.Intensitiy + 0.1f || intensity < pheromoneDeposit.Intensitiy - 0.1f)
                            {
                                // update
                                player.Game.Pheromones.UpdatePheromones(pheromoneDeposit.DepositId, intensity);
                                pheromoneDeposit.Intensitiy = intensity;
                            }
                        }
                    }
                    else
                    {
                        if (ant.Unit.Reactor.AvailablePower > 0)
                        {
                            pheromoneDeposit = new PheromoneDeposit();
                            pheromoneDeposit.Intensitiy = intensity;
                            pheromoneDeposit.Pos = pos;
                            pheromoneDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, ant.Unit.Pos, ant.Unit.Reactor.Range, PheromoneType.Energy, intensity);
                            staticReactorDeposits.Add(ant.Unit.Pos, pheromoneDeposit);
                        }
                    }
                }
            }
            foreach (Position2 pos in deposits)
            {
                PheromoneDeposit mineralDeposit = staticReactorDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticReactorDeposits.Remove(pos);
            }
        }
        public void CreateAttackCommand(Player player, Position2 pos)
        {
            bool commandActive = false;
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.AttackMove)
                {
                    if (gameCommand.TargetPosition == pos)
                    {
                        commandActive = true;
                    }
                }
            }
            if (!commandActive)
            {
                // Is it in powered zone?
                Pheromone pheromone = player.Game.Pheromones.FindAt(pos);
                if (pheromone == null || pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Energy) == 0)
                {
                    // Cannot build here, no power
                    return;
                }

                // Create a command to attack the enemy
                foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                {
                    if (blueprintCommand.GameCommandType == GameCommandType.AttackMove)
                    {
                        GameCommand gameCommand = new GameCommand(blueprintCommand.Units[0]);

                        gameCommand.GameCommandType = blueprintCommand.GameCommandType;
                        gameCommand.TargetPosition = pos;
                        gameCommand.PlayerId = player.PlayerModel.Id;
                        gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                        //if (player.PlayerModel.Id == 1)
                        //    Debug.WriteLine("Create Attack at " + pos.ToString());

                        player.GameCommands.Add(gameCommand);
                        break;
                    }
                }
            }
        }

        public void CreateCollectCommand(Player player, Position2 pos)
        {
            bool commandActive = false;
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Collect) // && gameCommand.TargetPosition == pos)
                {
                    if (gameCommand.IncludedPositions.ContainsKey(pos))
                    {
                        commandActive = true;
                        break;
                    }
                }
            }
            if (!commandActive)
            {
                // Is it in powered zone?
                Pheromone pheromone = player.Game.Pheromones.FindAt(pos);
                if (pheromone == null || pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Energy) == 0)
                {
                    // Cannot build here, no power
                    return;
                }

                // Create a command to collect the resources
                foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                {
                    if (blueprintCommand.GameCommandType == GameCommandType.Collect)
                    {
                        GameCommand gameCommand = new GameCommand(blueprintCommand.Units[0]);

                        gameCommand.GameCommandType = GameCommandType.Collect;
                        gameCommand.Radius = 4;
                        gameCommand.TargetPosition = pos;
                        gameCommand.PlayerId = player.PlayerModel.Id;
                        gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                        gameCommand.IncludedPositions = player.Game.Map.EnumerateTiles(gameCommand.TargetPosition, gameCommand.Radius, true);

                        player.GameCommands.Add(gameCommand);
                        break;
                    }
                }
            }

        }

        public void CreateCommandForContainerInZone(Player player, int zoneId)
        {
#if MEASURE_TIMINGS1
            DateTime start;
            double timetaken;

            start = DateTime.Now;
#endif


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
                    (gameCommand.BlueprintName == "Container" || gameCommand.BlueprintName == "Outpost") &&
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
                    if (ant.Unit.Assembler != null && ant.Unit.Engine == null)
                    {
                        // Not too far away
                        int d = Position3.Distance(mapZone.Center, ant.Unit.Pos);
                        if (d > 20) continue;

                        neighborAssemblers.Add(ant);
                    }
                    if (ant.Unit.Container != null && ant.Unit.Engine == null)
                    {
                        // Not too far away
                        int d = Position3.Distance(mapZone.Center, ant.Unit.Pos);
                        if (d > 20) continue;

                        neighborContainers.Add(ant);
                    }
                }
            }

#if MEASURE_TIMINGS1
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                start = DateTime.Now;
            }
#endif

            int bestScore = 0;
            List<Position2> possibleBuildLocations = new List<Position2>();

            List<Position2> suggestedBuildLocations = new List<Position2>();
            suggestedBuildLocations.Add(mapZone.Center);

            Position3 center = new Position3(mapZone.Center);
            AddBuildLocation(suggestedBuildLocations, center, Direction.S);
            AddBuildLocation(suggestedBuildLocations, center, Direction.SE);
            AddBuildLocation(suggestedBuildLocations, center, Direction.SW);
            AddBuildLocation(suggestedBuildLocations, center, Direction.N);
            AddBuildLocation(suggestedBuildLocations, center, Direction.NW);
            AddBuildLocation(suggestedBuildLocations, center, Direction.NE);

#if MEASURE_TIMINGS1
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                start = DateTime.Now;
            }
#endif

            int loopCount = 0;

            foreach (Position2 suggestedBuildLocation in suggestedBuildLocations)
            {
                foreach (Ant ant in neighborContainers)
                {
                    loopCount++;

                    // Draw a line from each container
                    Position3 to = new Position3(suggestedBuildLocation);
                    Position3 from = new Position3(ant.Unit.Pos);

                    List<Position3> line = FractionalHex.HexLinedraw(from, to);
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
                        bool alreadyBuilding = false;
                        foreach (GameCommand gameCommand in player.GameCommands)
                        {
                            if (gameCommand.GameCommandType == GameCommandType.Build &&
                                gameCommand.TargetPosition == tile.Pos)
                            {
                                alreadyBuilding = true;
                                break;
                            }
                        }
                        if (alreadyBuilding)
                            continue;

                        // Can the location be reached?
                        /*
                        bool pathPossible = false;
                        foreach (Ant antAssembler in neighborAssemblers)
                        {
                            List<Position2> positions = player.Game.FindPath(antAssembler.Unit.Pos, tile.Pos, null);
                            if (positions == null)
                                continue;
                            pathPossible = true;
                            break;
                        }
                        if (!pathPossible)
                            continue;
                        */

                        // Count number of containers in range
                        int score = 0;
                        foreach (Ant antContainer in neighborContainers)
                        {
                            // Not too far away
                            int d = Position3.Distance(tile.Pos, antContainer.Unit.Pos);
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
#if MEASURE_TIMINGS1
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 15)
            {
                start = DateTime.Now;
            }
#endif
            if (loopCount > 100)
            {
            }
            if (possibleBuildLocations.Count > 0)
            {
                // pick a rondom of the best
                int idx = player.Game.Random.Next(possibleBuildLocations.Count);
                Position2 buildPosition = possibleBuildLocations[idx];

                int chanceoutpost = player.Game.Random.Next(10);
                //chanceoutpost = 1;
                foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                {
                    if (chanceoutpost < 4 && blueprintCommand.Name == "Outpost")
                    {
                        GameCommand gameCommand = new GameCommand(blueprintCommand.Units[0]);

                        // Simple the first one BUILD-STEP1 (KI: Select fixed blueprint)
                        gameCommand.GameCommandType = GameCommandType.Build;
                        gameCommand.TargetPosition = buildPosition;
                        gameCommand.PlayerId = player.PlayerModel.Id;
                        gameCommand.TargetZone = zoneId;
                        gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                        player.GameCommands.Add(gameCommand);

                        break;
                    }
                    else if (blueprintCommand.Name == "Container")
                    {
                        GameCommand gameCommand = new GameCommand(blueprintCommand.Units[0]);

                        // Simple the first one BUILD-STEP1 (KI: Select fixed blueprint)
                        gameCommand.GameCommandType = GameCommandType.Build;
                        gameCommand.TargetPosition = buildPosition;
                        gameCommand.PlayerId = player.PlayerModel.Id;                        
                        gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                        player.GameCommands.Add(gameCommand);
                        
                        break;
                    }
                }
            }
        }

        public void AddBuildLocation(List<Position2> suggestedBuildLocations, Position3 cubePosition, Direction direction)
        {
            Position3 ndir = cubePosition;

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
        /*
        public bool WillBeOccupied(Player player, List<Move> moves, Position2 destination)
        {
            bool occupied = false;

            // Check if the
            //foreach (PlayerUnit currentUnit in player.PlayerUnits.Values)
            foreach (Unit unit in player.Game.Map.Units.List.Values)
            {
                if (unit.Pos == destination)
                {
                    if (unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                        unit.Engine != null)
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
        }*/
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

        public bool IsBeingExtracted(List<Move> moves, Position2 pos)
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

        //private Dictionary<Position2, MineralDeposit> mineralsDeposits = new Dictionary<Position2, MineralDeposit>();
        private Dictionary<Position2, PheromoneDeposit> staticMineralDeposits = new Dictionary<Position2, PheromoneDeposit>();
        private Dictionary<Position2, PheromoneDeposit> staticContainerDeposits = new Dictionary<Position2, PheromoneDeposit>();
        private Dictionary<Position2, PheromoneDeposit> staticReactorDeposits = new Dictionary<Position2, PheromoneDeposit>();
        //private Dictionary<Position2, int> workDeposits = new Dictionary<Position2, int>();
        //private Dictionary<Position2, int> enemyDeposits = new Dictionary<Position2, int>();

        /*
        public void RemoveEnemyFound(Player player, int id)
        {
            foreach (Position2 pos in enemyDeposits.Keys)
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
            foreach (Position2 pos in staticMineralDeposits.Keys)
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
        public int EnemyFound(Player player, Position2 pos, bool isStatic)
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
            public int MineralsFound(Player player, Position2 pos, bool isStatic)
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
            public void WorkFound(Player player, Position2 pos)
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

        public bool IsOccupied(Player player, List<Move> moves, Position2 destination)
        {
            bool occupied = false;

            // Check if the

            Tile dest = player.Game.Map.GetTile(destination);
            Unit unitAtDestination = dest.Unit;

            //foreach (PlayerUnit currentUnit in player.PlayerUnits.Values)
            //foreach (Ant ant in Ants.Values)
            {
                if (unitAtDestination != null)
                {
                    if (unitAtDestination.Owner.PlayerModel.Id == player.PlayerModel.Id && unitAtDestination.Engine != null)
                    {
                        occupied = true;

                        // Our own unit, that has engine may move away
                        foreach (Move intendedMove in moves)
                        {
                            if ((intendedMove.MoveType == MoveType.Move || intendedMove.MoveType == MoveType.Add || intendedMove.MoveType == MoveType.Build) &&
                                unitAtDestination.UnitId == intendedMove.UnitId)
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
                    //break;
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

        public Position2 FindReactor(Player player, Ant antWorker)
        {
            return Position2.Null;
            /*
            List<Position2> bestPositions = null;

            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit == null || ant.PlayerUnit.Unit.Reactor == null || ant.PlayerUnit.Unit.Reactor.AvailablePower == 0)
                    continue;

                // Distance at all
                Position2 posFactory = ant.PlayerUnit.Unit.Pos;
                //double d = posFactory.GetDistanceTo(antWorker.PlayerUnit.Unit.Pos);
                //int d = CubePosition2.Distance(posFactory, antWorker.PlayerUnit.Unit.Pos);
                //if (d < 28)
                {
                    List<Position2> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, posFactory, antWorker.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
            }
            return MakePathFromPositions(bestPositions, antWorker);
            */
        }

        public Position2 FindCommandTarget(Player player, Ant antWorker)
        {
            List<Position2> bestPositions = null;
            if (antWorker.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
            {
                // Need neighbor pos
                Tile t = player.Game.Map.GetTile(antWorker.Unit.CurrentGameCommand.TargetPosition);
                foreach (Tile n in t.Neighbors)
                {
                    bestPositions = player.Game.FindPath(antWorker.Unit.Pos, n.Pos, antWorker.Unit);
                    if (bestPositions != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                // Compute route to target
                bestPositions = player.Game.FindPath(antWorker.Unit.Pos, antWorker.Unit.CurrentGameCommand.TargetPosition, antWorker.Unit);
            }
            return MakePathFromPositions(bestPositions, antWorker);
        }
    

        public Position2 FindContainer(Player player, Ant antWorker)
        {

            Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(antWorker.Unit.Pos, 3, false, matcher: tile =>
            {
                // If engine is not null, could be a friendly unit that needs refuel.
                if (tile.Unit != null && tile.Unit.IsComplete() &&
                        tile.Unit.Engine == null &&
                        tile.Unit.CanFill())
                    return true;
                return false;
            });

            List<Position2> bestPositions = null;

            foreach (TileWithDistance t in tiles.Values)
            {
                List<Position2> positions = player.Game.FindPath(antWorker.Unit.Pos, t.Pos, antWorker.Unit);
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

                    //if (ant is AntWorker)
                    //    continue;

                    if (ant.Unit.IsComplete() &&
                        ant.Unit.Engine == null &&
                        ant.Unit.CanFill())
                    {
                        // Distance at all
                        Position2 posFactory = ant.Unit.Pos;
                        //double d = posFactory.GetDistanceTo(antWorker.PlayerUnit.Unit.Pos);
                        int d = Position3.Distance(posFactory, antWorker.Unit.Pos);
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

                            List<Position2> positions = player.Game.FindPath(antWorker.Unit.Pos, posFactory, antWorker.Unit);
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

        private List<Position2> FindMineralForCommand(Player player, Ant ant, List<Position2> bestPositions)
        {
            return null;
            /*
            Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition, player.Game.Map.SectorSize, false, matcher: tile =>
            {
                if (tile.Minerals > 0 ||
                    (tile.Unit != null && (tile.Unit.ExtractMe || tile.Unit.Owner.PlayerModel.Id == 0)))
                {
                    List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, tile.Pos, ant.PlayerUnit.Unit);
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
        private List<Position2> FindMineralOnMap(Player player, Ant ant, List<Position2> bestPositions)
        {
            // NOT GOOD! TO MUCH TIME
            return null;
            /*
            
            foreach (Position2 pos in player.VisiblePositions) // TileWithDistance t in tiles.Values)
            {
                Tile tile = player.Game.Map.GetTile(pos);
                if (tile.Minerals > 0 ||
                    (tile.Unit != null && (tile.Unit.ExtractMe || tile.Unit.Owner.PlayerModel.Id == 0)))
                {
                    List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, tile.Pos, ant.PlayerUnit.Unit);
                    if (bestPositions == null || bestPositions.Count > positions?.Count)
                    {
                        bestPositions = positions;
                    }
                }
            }
            return bestPositions;
            */
        }

        private List<Position2> FindMineralDeposit(Player player, Ant ant, List<Position2> bestPositions)
        {
            return null;
            /*
            // ALSO BAD
            foreach (Position2 pos in staticMineralDeposits.Keys)
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

                    List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
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
                foreach (Position2 pos in mineralsDeposits.Keys)
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

                        List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
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

        private List<Position2> FindMineralContainer(Player player, Ant ant, List<Position2> bestPositions)
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
                    List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, antContainer.PlayerUnit.Unit.Pos, ant.PlayerUnit.Unit);
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

        private Position2 MakePathFromPositions(List<Position2> bestPositions, Ant ant)
        {
            ant.FollowThisRoute = null;

            if (bestPositions != null)
            {
                if (bestPositions.Count > 1)
                {
                    if (bestPositions.Count > 2)
                    {
                        ant.FollowThisRoute = new List<Position2>();
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
            return Position2.Null;
        }

        public Position2 FindMineral(Player player, Ant ant)
        {
            //return Position2.Null;
            
            List<Position2> bestPositions = null;

            if (ant.AntWorkerType == AntWorkerType.Worker)
            {
                if (ant.Unit.CurrentGameCommand != null)
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
                //if (bestPosition2s == null)
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
                //if (bestPosition2s == null)
                    bestPositions = FindMineralContainer(player, ant, bestPositions);
            }
            else
            {
                bestPositions = FindMineralContainer(player, ant, bestPositions);
                //if (bestPosition2s == null)
                    bestPositions = FindMineralOnMap(player, ant, bestPositions);
                //if (bestPosition2s == null)                
                    bestPositions = FindMineralDeposit(player, ant, bestPositions);
            }
            return MakePathFromPositions(bestPositions, ant);
            
        }

        public Position2 LevelGround(List<Move> moves, Player player, Ant ant)
        {
            Tile cliff = null;
            Tile tile = player.Game.Map.GetTile(ant.Unit.Pos);
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
            Dictionary<Position2, TileWithDistance> tilesx = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.CurrentGameCommand.TargetPosition2, 3, false, matcher: tile =>
            {
                totalHeight += tile.Tile.Height;
                return true;
            });
            totalHeight /= tilesx.Count;
                */

                if (ant.Unit.Extractor != null &&
                    ant.Unit.Extractor.CanExtractDirt)
                {
                    /*
                    Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 1, false, matcher: tile =>
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
                else if (ant.Unit.Weapon != null &&
                    ant.Unit.Weapon.WeaponLoaded)
                {
                    // Can't extract. Shot somewhere
                    Dictionary<Position2, TileWithDistance> tiles = ant.Unit.Game.Map.EnumerateTiles(ant.Unit.Pos, ant.Unit.Weapon.Range, false, matcher: tilex =>
                    {
                        if (tilex.Unit != null)
                            return false;

                        return true;
                    });

                    /*
                    Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 2, false, matcher: tile =>
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

                        move.Position2s = new List<Position2>();
                        move.Position2s.Add(ant.PlayerUnit.Unit.Pos);
                        move.Position2s.Add(lowestTile.Tile.Pos);

                        moves.Add(move);
                        */
                    }
                }
            }
            return Position2.Null;
        }
        public Position2 FindEnemy(Player player, Ant ant)
        {
            return Position2.Null;
            /*
            Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                if (tile.Unit != null &&
                    tile.Unit.Owner.PlayerModel.Id != player.PlayerModel.Id &&
                    tile.Unit.IsComplete())
                    return true;
                return false;
            });

            List<Position2> bestPosition2s = null;

            foreach (TileWithDistance t in tiles.Values)
            {
                List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, t.Pos, ant.PlayerUnit.Unit);
                if (bestPosition2s == null || bestPosition2s.Count > positions?.Count)
                {
                    bestPosition2s = positions;
                    //break;
                }
            }
            if (bestPosition2s == null)
            {
                foreach (Position2 pos in enemyDeposits.Keys)
                {
                    // Distance at all
                    //double d = pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                    //int d = CubePosition2.Distance(pos, ant.PlayerUnit.Unit.Pos);
                    //if (d < 18)
                    {
                        List<Position2> positions = player.Game.FindPath(ant.PlayerUnit.Unit.Pos, pos, ant.PlayerUnit.Unit);
                        if (positions != null && positions.Count > 2)
                        {
                            if (bestPosition2s == null || bestPosition2s.Count > positions?.Count)
                            {
                                bestPosition2s = positions;
                            }
                        }
                    }
                }
            }
            return MakePathFromPositions(bestPosition2s, ant);
            */
        }

        /*
        internal void UpdateUnitCounters(Ant ant)
        {
            if (ant.AntWorkerType == AntWorkerType.Worker)
                NumberOfWorkers++;
            if (ant.AntWorkerType == AntWorkerType.Fighter)
                NumberOfFighter++;
            if (ant.AntWorkerType == AntWorkerType.Assembler)
                NumberOfAssembler++;
            if (ant.Unit != null)
            {
                if (ant.Unit.Reactor != null)
                {
                    NumberOfReactors++;
                }
            }
        }*/

        private void SacrificeAnt(Player player, List<Ant> ants)
        {
            // Is already sacified?
            foreach (Ant ant in ants)
            {
                if (ant.Unit.Engine != null &&
                    ant.Unit.ExtractMe)
                {
                    // This one is on its way hopefully
                    return;
                }
            }

            foreach (Ant ant in ants)
            {
                if (ant.AntWorkerType != AntWorkerType.Worker)
                {
                    if (ant.Unit.Engine != null && !ant.Unit.EndlessPower)
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
            if (gameCommand.TargetPosition != Position2.Null)
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
                    commandMove.Positions = new List<Position2>();
                    commandMove.Positions.Add(gameCommand.TargetPosition);
                    moves.Add(commandMove);

                    return true;
                }
            }
            return false;
        }*/

        private void FinishCompleteCommands(Player player)
        {
            List<GameCommand> removeCommands = new List<GameCommand>();
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.CommandCanceled)
                {
                    //removeCommands.Add(gameCommand);
                }
                else if (gameCommand.GameCommandType == GameCommandType.Fire)
                {
                    if (gameCommand.CommandComplete)
                    {
                        removeCommands.Add(gameCommand);

                        Ant ant;
                        if (Ants.TryGetValue(gameCommand.UnitId, out ant))
                        {
                            ant.Unit.ResetGameCommand();
                        }
                    }
                }
                else if (gameCommand.GameCommandType == GameCommandType.ItemOrder)
                {
                    removeCommands.Add(gameCommand);
                    Ant ant;
                    if (Ants.TryGetValue(gameCommand.UnitId, out ant))
                    {
                        foreach (UnitItemOrder newUnitItemOrder in gameCommand.UnitItemOrders)
                        {
                            foreach (UnitItemOrder unitItemOrder in ant.Unit.UnitOrders.unitItemOrders)
                            {
                                if (unitItemOrder.TileObjectType == newUnitItemOrder.TileObjectType)
                                {
                                    unitItemOrder.TileObjectState = newUnitItemOrder.TileObjectState;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (gameCommand.GameCommandType == GameCommandType.Automate)
                {
                    removeCommands.Add(gameCommand);
                    gameCommand.CommandComplete = true;
                    gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                    Ant ant;
                    if (Ants.TryGetValue(gameCommand.UnitId, out ant))
                    {
                        if (ant.Unit.CurrentGameCommand != null)
                            ant.Unit.CurrentGameCommand.CommandCanceled = true;
                    }
                }
                else if (gameCommand.GameCommandType == GameCommandType.Cancel)
                {
                    removeCommands.Add(gameCommand);
                    gameCommand.CommandComplete = true;
                    gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                    gameCommand.AttachedUnit.SetStatus("Canceled");

                    foreach (GameCommand otherGameCommand in player.GameCommands)
                    {
                        if (otherGameCommand.GameCommandType != GameCommandType.Cancel &&
                            otherGameCommand.CommandId == gameCommand.CommandId)
                        {
                            otherGameCommand.CommandComplete = true;
                            otherGameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                            break;
                        }
                    }
                }
                else if (gameCommand.GameCommandType == GameCommandType.Extract)
                {
                    removeCommands.Add(gameCommand);
                    gameCommand.CommandComplete = true;
                    gameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;

                    Ant ant;
                    if (Ants.TryGetValue(gameCommand.UnitId, out ant))
                    {
                        ant.Unit.ResetGameCommand();
                        ant.Unit.ExtractUnit();
                    }
                }
            }
            foreach (GameCommand removeCommand in removeCommands)
            {
                player.GameCommands.Remove(removeCommand);
            }
        }

        private static int attachCounter;

        private void AttachGamecommands(Player player, List<Ant> unmovedAnts, List<Move> moves, int priority)
        {
            attachCounter++;
            if (attachCounter == 126)
            {

            }

            Ant bestAnt;
            int bestDistance;

            // Attach gamecommands to idle units
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.Priority != priority)
                    continue;

                if (gameCommand.CommandId == 33)
                {
                }

                if (player.CompletedCommands.Contains((GameCommand)gameCommand))
                    continue;
                if (gameCommand.CommandCanceled || gameCommand.CommandComplete)
                {
                    if (gameCommand.GameCommandType != GameCommandType.Cancel)
                        player.CompletedCommands.Add((GameCommand)gameCommand);
                    continue;
                }


                bool requestUnit = false;

                /* Done by transport
                if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                    gameCommandItem.AttachedUnit.UnitId == null && gameCommandItem.TransportUnit.UnitId != null)
                {
                    // Transporter without source
                    if (!gameCommandItem.DeliverContent)
                        requestUnit = true;
                }

                if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                    gameCommandItem.TransportUnit.UnitId == null && gameCommandItem.TargetUnit.UnitId != null)
                {
                    // Target waiting, assign transporter
                    requestUnit = true;
                }
                */

                if (gameCommand.UnitId != null)
                {
                    Ant ant;
                    if (Ants.TryGetValue(gameCommand.UnitId, out ant))
                    {
                        Unit unit = ant.Unit;
                        if (gameCommand.CommandCanceled)
                        {
                            unit.ResetGameCommand();
                        }
                        else
                        {
                            if (unit.CurrentGameCommand == null)
                            {

                            }
                            else if (unit.CurrentGameCommand == gameCommand)
                            {
                                // Command already attached
                            }
                            else if (unit.CurrentGameCommand != null)
                            {
                                unit.ResetGameCommand();
                            }

                            if (unit.CurrentGameCommand == null)
                            {
                                if (gameCommand.GameCommandType == GameCommandType.Build)
                                {
                                    if (gameCommand.FactoryUnit.UnitId == null)
                                    {
                                        gameCommand.FactoryUnit.SetUnitId(gameCommand.UnitId);
                                        unit.SetGameCommand(gameCommand);
                                    }
                                    else
                                    {
                                        // Already built
                                    }
                                }
                                else if (gameCommand.GameCommandType == GameCommandType.AttackMove)
                                {
                                    if (gameCommand.AttachedUnit.UnitId == null)
                                    {
                                        gameCommand.AttachedUnit.SetUnitId(unit.UnitId);
                                        ant.FollowThisRoute = null;
                                        unit.SetGameCommand(gameCommand);
                                    }
                                }
                                else if (gameCommand.GameCommandType == GameCommandType.HoldPosition)
                                {
                                    if (gameCommand.AttachedUnit.UnitId == null)
                                    {
                                        gameCommand.AttachedUnit.SetUnitId(unit.UnitId);
                                        ant.FollowThisRoute = null;
                                        unit.SetGameCommand(gameCommand);
                                    }
                                }
                                else if (gameCommand.GameCommandType == GameCommandType.Unload)
                                {
                                    if (gameCommand.AttachedUnit.UnitId == null)
                                    {
                                        gameCommand.AttachedUnit.SetUnitId(unit.UnitId);
                                        ant.FollowThisRoute = null;
                                        unit.SetGameCommand(gameCommand);
                                    }
                                }
                                else if (gameCommand.GameCommandType == GameCommandType.Collect)
                                {
                                    if (gameCommand.TransportUnit.UnitId == null)
                                    {
                                        if (gameCommand.Radius == 0)
                                        {
                                            // Unit is assigend to collect from this position, so it transports
                                            gameCommand.TransportUnit.SetUnitId(unit.UnitId);
                                        }
                                        else
                                        {
                                            // Unit is assigend to collect in an area.
                                            gameCommand.AttachedUnit.SetUnitId(unit.UnitId);
                                        }
                                        ant.FollowThisRoute = null;
                                        unit.SetGameCommand(gameCommand);
                                    }
                                }
                                else
                                {
                                    int xx = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        gameCommand.CommandCanceled = true;
                    }


                }
                else
                {
                    /* Not now
                    if (gameCommand.GameCommandType == GameCommandType.AttackMove &&
                        gameCommand.AttachedUnit.UnitId == null && gameCommand.FactoryUnit.UnitId == null)
                    {
                        requestUnit = true;
                    }

                    else */

                    if (gameCommand.GameCommandType == GameCommandType.Build &&
                        gameCommand.AttachedUnit.UnitId != null && gameCommand.FactoryUnit.UnitId == null)
                    {
                        // Check if the unit to be build is there
                        Unit unit = player.Game.Map.Units.FindUnit(gameCommand.AttachedUnit.UnitId);
                        if (unit == null)
                        {
                            requestUnit = true;
                        }
                        else
                        {
                            if (!unit.IsComplete())
                            {
                                requestUnit = true;
                            }
                        }
                    }
                    else if (gameCommand.AttachedUnit.UnitId == null && gameCommand.FactoryUnit.UnitId == null)
                    {
                        if (gameCommand.GameCommandType == GameCommandType.Build && gameCommand.TargetPosition != Position2.Null)
                        {
                            // Check if the target to be build is already there, if so, ignore this command
                            Tile t = player.Game.Map.GetTile(gameCommand.TargetPosition);
                            if (t.Unit == null)
                            {
                                requestUnit = true;
                            }
                            else
                            {
                                if (t.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id && t.Unit.Blueprint.Name == gameCommand.BlueprintName)
                                {
                                    // The building to build exists already, do not request a builder
                                }
                                else
                                {
                                    // Own unit or 
                                    if (t.Unit.Engine != null)
                                    {
                                        // Unit may drive away
                                        requestUnit = true;
                                    }
                                    else
                                    {
                                        // Another building is there
                                    }
                                }
                            }
                        }
                        else if (gameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            if (gameCommand.TransportUnit.UnitId == null)
                                requestUnit = true;
                        }
                        else if (gameCommand.GameCommandType == GameCommandType.Collect)
                        {
                            requestUnit = true;
                        }
                    }
                }
                if (requestUnit)
                {
                    // Find a existing unit that can do/deliver it
                    foreach (Ant ant in unmovedAnts)
                    {
                        if (!ant.Unit.UnderConstruction && !ant.Unit.ExtractMe)
                        {
                            /*
                            if (ant.Unit.Engine != null &&
                                ant.Unit.Engine.AttackPosition != Position2.Null)
                            {
                                // Unit is on hold
                                continue;
                            }*/
                            if (ant.Unit.CurrentGameCommand == null)
                            {
                                // Done by transport
                                if (gameCommand.GameCommandType == GameCommandType.ItemRequest)
                                {
                                    if (ant.Unit.Blueprint.Name == "Worker")
                                    {
                                        gameCommand.TransportUnit.SetUnitId(ant.Unit.UnitId);
                                        gameCommand.TransportUnit.SetStatus("Transport for " + gameCommand.GameCommandType);
                                        ant.Unit.SetGameCommand(gameCommand);
                                        requestUnit = false;
                                    }
                                }
                                else if (gameCommand.GameCommandType == GameCommandType.Build)
                                {
                                    if (ant.Unit.Blueprint.Name == "Builder")
                                    {
                                        gameCommand.FactoryUnit.SetUnitId(ant.Unit.UnitId);
                                        gameCommand.FactoryUnit.SetStatus("BuildAssembler for " + gameCommand.GameCommandType);
                                        ant.Unit.SetGameCommand(gameCommand);
                                        requestUnit = false;
                                    }
                                }
                                else if (gameCommand.GameCommandType == GameCommandType.Collect)
                                {
                                    if (ant.Unit.Blueprint.Name == "Worker")
                                    {
                                        gameCommand.AttachedUnit.SetUnitId(ant.Unit.UnitId);
                                        gameCommand.AttachedUnit.SetStatus("Collect for " + gameCommand.GameCommandType);
                                        ant.Unit.SetGameCommand(gameCommand);
                                        requestUnit = false;
                                    }
                                }
                                /* Not now
                                else if (gameCommand.GameCommandType == GameCommandType.AttackMove)
                                {
                                    if (ant.Unit.Blueprint.Name == gameCommand.BlueprintName)
                                    {
                                        gameCommand.AttachedUnit.SetUnitId(ant.Unit.UnitId);
                                        gameCommand.AttachedUnit.SetStatus("AttachedUnitId: " + gameCommand.AttachedUnit.UnitId);

                                        ant.Unit.SetGameCommand(gameCommand);
                                        requestUnit = false;
                                    }
                                }*/
                            }
                            else
                            {
                                /* Done by transport
                                if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                                    ant.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Collect)
                                {
                                    // Is there a worker collecting stuff, that can be used to satisfy the delivery request?
                                    int score = ant.GetDeliveryScore(gameCommandItem.GameCommand);
                                    if (score > bestDeliveryScore)
                                    {
                                        bestDeliveryScore = score;
                                        deliverySourceAnt = ant;
                                    }
                                }
                                */
                            }
                        }
                        if (requestUnit == false)
                            break;
                    }
                    /* Done by transport
                    if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                    {
                        if (deliverySourceAnt == null)
                        {
                            // No source found, that can deliver the items. Do not create a transporter
                            requestUnit = false;
                        }
                        else
                        {
                            // deliverySourceAnt is the ant, that has the items requested
                            if (deliverySourceAnt.AntPartEngine != null)
                            {
                                if (gameCommandItem.AttachedUnit.UnitId == null)
                                {
                                    gameCommandItem.AttachedUnit.SetUnitId(deliverySourceAnt.Unit.UnitId);
                                    gameCommandItem.AttachedUnit.SetStatus("TransportAndDelivery: " + gameCommandItem.AttachedUnit.UnitId);
                                }
                                gameCommandItem.TransportUnit.SetUnitId(deliverySourceAnt.Unit.UnitId);
                                gameCommandItem.TransportUnit.SetStatus("TransportUnitId for Delivery: " + gameCommandItem.AttachedUnit.UnitId);

                                deliverySourceAnt.Unit.SetGameCommand(gameCommandItem);
                                deliverySourceAnt.Unit.Changed = true;
                                requestUnit = false;
                            }
                            else
                            {
                                if (gameCommandItem.TransportUnit.UnitId == null)
                                {
                                    Ant transportAnt = FindTransporter(gameCommandItem);
                                    if (transportAnt != null)
                                    {
                                        // The attached unit is the one, who delivers the content (Need resevation!)
                                        gameCommandItem.AttachedUnit.SetUnitId(deliverySourceAnt.Unit.UnitId);
                                        gameCommandItem.TransportUnit.SetUnitId(transportAnt.Unit.UnitId);
                                        gameCommandItem.TransportUnit.SetStatus("AttachedTransport: " + gameCommandItem.FactoryUnit.UnitId + " take from " + gameCommandItem.AttachedUnit.UnitId);

                                        transportAnt.Unit.SetGameCommand(gameCommandItem);
                                        transportAnt.Unit.Changed = true;
                                        requestUnit = false;
                                    }
                                    else
                                    {
                                        if (deliverySourceAnt.Unit.Assembler != null && deliverySourceAnt.Unit.Engine != null)
                                        {

                                        }
                                        if (gameCommandItem.AttachedUnit.UnitId != null)
                                        {
                                            gameCommandItem.AttachedUnit.ClearUnitId(player.Game.Map.Units);
                                        }
                                        // The attached unit is the one, who delivers the content (Need resevation!)
                                        gameCommandItem.AttachedUnit.SetUnitId(deliverySourceAnt.Unit.UnitId);
                                        gameCommandItem.AttachedUnit.SetStatus("WaitingForTransporterToPickupItems");
                                        deliverySourceAnt.Unit.Changed = true;

                                        // Need unit for transport
                                        requestUnit = true;
                                    }
                                }
                                else
                                {
                                    if (deliverySourceAnt.Unit.Assembler != null && deliverySourceAnt.Unit.Engine != null)
                                    {

                                    }
                                    // The attached unit is the one, who delivers the content (Need resevation!)
                                    gameCommandItem.AttachedUnit.SetUnitId(deliverySourceAnt.Unit.UnitId);
                                    requestUnit = false;
                                }
                            }
                        }
                    }*/
                }
                if (requestUnit)
                {
                    // Find a factory
                    bestAnt = null;
                    bestDistance = 0;

                    foreach (Ant ant in unmovedAnts)
                    {
                        // Currently only structures, not moving assemblers
                        if (ant.AntPartAssembler != null && ant.AntPartEngine == null)
                        {
                            if (!ant.Unit.UnderConstruction && !ant.Unit.ExtractMe)
                            {
                                if (ant.Unit.CurrentGameCommand != null && ant.Unit.CurrentGameCommand.FactoryUnit.UnitId != null)
                                {
                                    // Unit is already creating something
                                    continue;
                                }

                                if (!ant.Unit.Assembler.CanProduce())
                                {
                                    continue;
                                }

                                if (ant.Unit.Pos == gameCommand.TargetPosition)
                                {
                                    bestAnt = ant;
                                    break;
                                }
                                else
                                {
                                    int distance = Position3.Distance(ant.Unit.Pos, gameCommand.TargetPosition);
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
                        gameCommand.FactoryUnit.SetUnitId(bestAnt.Unit.UnitId);
                        gameCommand.FactoryUnit.SetStatus("BuildingUnit for " + gameCommand.GameCommandType.ToString());
                        bestAnt.Unit.SetGameCommand(gameCommand);
                        bestAnt.Unit.Changed = true;
                    }
                }

            }

        }

        private void RemoveCompletedCommands(Player player)
        {
            foreach (GameCommand gameCommand in player.CompletedCommands)
            {
                Unit unit;
                if (gameCommand.AttachedUnit.UnitId != null)
                {
                    unit = player.Game.Map.Units.FindUnit(gameCommand.AttachedUnit.UnitId);
                    if (unit != null)
                        unit.ResetGameCommand();
                }
                if (gameCommand.FactoryUnit.UnitId != null)
                {
                    unit = player.Game.Map.Units.FindUnit(gameCommand.FactoryUnit.UnitId);
                    if (unit != null)
                        unit.ResetGameCommand();
                }
                if (gameCommand.TransportUnit.UnitId != null)
                {
                    unit = player.Game.Map.Units.FindUnit(gameCommand.TransportUnit.UnitId);
                    if (unit != null)
                        unit.ResetGameCommand();
                }
                if (gameCommand.TargetUnit.UnitId != null)
                {
                    unit = player.Game.Map.Units.FindUnit(gameCommand.TargetUnit.UnitId);
                    if (unit != null)
                        unit.ResetGameCommand();
                }
                /*
                if (gameCommand.FollowUpUnitCommand == FollowUpUnitCommand.DeleteCommand)
                {
                    foreach (Ant ant in Ants.Values)
                    {
                        if (ant.Unit.CurrentGameCommand != null &&
                            ant.Unit.CurrentGameCommand == gameCommand)
                        {
                            ant.Unit.ResetGameCommand();
                        }
                    }
                    
                }*/
                player.GameCommands.Remove(gameCommand);
            }
        }

        

        public Ant FindTransporter(GameCommand gameCommand)
        {
            foreach (Ant otherAnt in Ants.Values)
            {
                // Must be complete
                if (otherAnt.UnderConstruction) continue;
                // Must not have antoher job
                if (otherAnt.Unit.CurrentGameCommand != null) continue;
                // Must be moving
                if (otherAnt.Unit.Engine == null) continue;
                // Must have a container
                if (otherAnt.Unit.Container == null) continue;


                // Must not contain more than 10% (If filled! but also need empty transporter
                //if (otherAnt.Unit.Container.TileContainer.Count > (otherAnt.Unit.Container.TileContainer.Capacity / 10)) continue;

                //double distance = otherAnt.PlayerUnit.Unit.Pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                int distance = Position3.Distance(otherAnt.Unit.Pos, gameCommand.TargetPosition);

                if (distance > 9) continue;

                return otherAnt;
            }
            return null;
        }

        public bool CheckTransportMove(Ant ant, List<Move> moves)
        {
            bool unitMoved = false;
            Unit cntrlUnit = ant.Unit;

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
                if (otherAnt.Unit.Engine != null) continue;
                // Must be a owned
                if (otherAnt.Unit.Owner != ant.Unit.Owner) continue;

                //double distance = otherAnt.PlayerUnit.Unit.Pos.GetDistanceTo(ant.PlayerUnit.Unit.Pos);
                int distance = Position3.Distance(otherAnt.Unit.Pos, ant.Unit.Pos);
                

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
            foreach (Ant ant in Ants.Values)
            {
                ant.MoveAttempts = 0;
                ant.Alive = false;
            }
            //foreach (PlayerUnit playerUnit in player.PlayerUnits.Values)
            foreach (Unit unit in player.Game.Map.Units.List.Values)
            {
                Unit cntrlUnit = unit;
                if (cntrlUnit.Owner.PlayerModel.Id == PlayerModel.Id)
                {
                    if (Ants.ContainsKey(cntrlUnit.UnitId))
                    {
                        Ant ant = Ants[cntrlUnit.UnitId];
                        ant.Alive = true;

                        if (ant.AntWorkerType == AntWorkerType.None)
                        {
                            ant.GuessWorkerType();
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
                        if (cntrlUnit.Blueprint.Name == "Assembler" ||
                            cntrlUnit.Blueprint.Name == "Fighter" ||
                            cntrlUnit.Blueprint.Name == "Builder" ||
                            cntrlUnit.Blueprint.Name == "Worker" ||
                            cntrlUnit.Blueprint.Name == "Bomber")
                        {
                            Ant antWorker = new Ant(this);
                            antWorker.Alive = true;

                            if (cntrlUnit.Direction == Direction.C)
                            {
                                cntrlUnit.Direction = Direction.SW;
                            }
                            antWorker.GuessWorkerType();
                            Ants.Add(cntrlUnit.UnitId, antWorker);
                        }
                        else if (cntrlUnit.Blueprint.Name == "Outpost" ||
                                 cntrlUnit.Blueprint.Name == "Factory")
                        {
                            //AntFactory antFactory = new AntFactory(this, playerUnit);
                            Ant antFactory = new Ant(this, cntrlUnit);
                            antFactory.Alive = true;
                            Ants.Add(cntrlUnit.UnitId, antFactory);
                        }
                        else
                        {
                            Ant ant = new Ant(this, cntrlUnit);
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
                    //player.Game.Pheromones.DropPheromones(player, cntrlUnit.Pos, 15, PheromoneType.Enemy, 0.05f);
                }
            }
        }

        public List<Move> Turn(Player player)
        {
#if MEASURE_TIMINGS
            DateTime start;
            double timetaken;

            start = DateTime.Now;
#endif

            moveNr++;
            if (moveNr == 441)
            {

            }
            player.CompletedCommands.Clear();

            // Returned moves
            List<Move> moves = new List<Move>();

            MapInfo mapInfo = player.Game.GetDebugMapInfo();

            if (!mapInfo.PlayerInfo.ContainsKey(player.PlayerModel.Id))
            {
                foreach (GameCommand gameCommand in player.GameCommands)
                {
                    gameCommand.CommandCanceled = true;
                    player.CompletedCommands.Add(gameCommand);
                }
                player.GameCommands.Clear();
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
            //UpdateAntList(player);

            NumberOfWorkers = 0;
            NumberOfFighter = 0;
            NumberOfAssembler = 0;

            NumberOfReactors = 0;

            List<Ant> movableAnts = new List<Ant>();
            List<Ant> killedAnts = new List<Ant>();
            List<Ant> unmovedAnts = new List<Ant>();

            foreach (Ant ant in Ants.Values)
            {
                if (ant.Unit.Power == 0)
                {
                    killedAnts.Add(ant);
                }
                else if (ant.Unit.IsComplete())
                {
                    // Kill useless units
                    if (!player.PlayerModel.IsHuman)
                    {
                        if (ant.Unit.CurrentGameCommand == null)
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
                    }

                    //ant.CreateAntParts();

                    if (ant.UnderConstruction)
                    {
                        if (ant.Unit.CurrentGameCommand != null)
                        {
                            if (ant.Unit.CurrentGameCommand.FactoryUnit.UnitId != null)
                            {
                                if (ant.Unit.CurrentGameCommand.FactoryUnit.UnitId != ant.Unit.UnitId)
                                {
                                    Unit factoryUnit = player.Game.Map.Units.FindUnit(ant.Unit.CurrentGameCommand.FactoryUnit.UnitId);
                                    if (factoryUnit != null)
                                    {
                                        // Leave the command alone, just remove the command from the factory.
                                        factoryUnit.CurrentGameCommand.FactoryUnit.ResetUnitId();
                                        factoryUnit.ResetGameCommandOnly();

                                        // Kill the moving assembler (No more)
                                        // If the assembler is kept, it will have no content and hang around useless

                                        if (ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                                            factoryUnit.Engine != null)
                                        {
                                            // Let the new unit extract the assembler
                                            factoryUnit.ExtractUnit();
                                        }
                                    }
                                    if (ant.Unit.CurrentGameCommand.AssemblerToBuild)
                                    {
                                        if (ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                                        {
                                            // The attached unit is the transporter to execute the build
                                            ant.Unit.CurrentGameCommand.TransportUnit.SetUnitId(ant.Unit.UnitId);
                                            ant.Unit.CurrentGameCommand.AssemblerToBuild = false;
                                            ant.Unit.CurrentGameCommand.AttachedUnit.ResetUnitId();
                                        }
                                        else if (ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                                        {
                                            // The attached unit is the assmbler to execute the build
                                            ant.Unit.CurrentGameCommand.FactoryUnit.SetUnitId(ant.Unit.UnitId);
                                            ant.Unit.CurrentGameCommand.AssemblerToBuild = false;
                                            ant.Unit.CurrentGameCommand.AttachedUnit.ResetUnitId();
                                        }
                                    }
                                    else if (ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                                    {
                                        // The transport unit is the one, who delivers the content (Need resevation!)
                                        ant.Unit.CurrentGameCommand.TransportUnit.SetStatus("PickUpFrom: " + ant.Unit.CurrentGameCommand.AttachedUnit.UnitId);
                                    }
                                    else if (ant.Unit.Blueprint.Name == "xxAssembler")
                                    {
                                        ant.Unit.CurrentGameCommand.AttachedUnit.ResetUnitId(); 
                                        ant.Unit.CurrentGameCommand.FactoryUnit.SetUnitId(ant.Unit.UnitId);
                                        ant.Unit.CurrentGameCommand.FactoryUnit.SetStatus("Assemble", false);
                                        ant.Unit.Changed = true;
                                    }
                                    else if (ant.Unit.Blueprint.Name == "Builder")
                                    {
                                        ant.Unit.CurrentGameCommand.AttachedUnit.ResetUnitId();
                                        ant.Unit.CurrentGameCommand.FactoryUnit.SetUnitId(ant.Unit.UnitId);
                                        ant.Unit.CurrentGameCommand.FactoryUnit.SetStatus("Assemble", false);
                                        ant.Unit.Changed = true;
                                    }
                                    else
                                    {
                                        //ant.Unit.CurrentGameCommand.FactoryUnit.ClearUnitId(); // player.Game.Map.Units);
                                        if (ant.Unit.CurrentGameCommand.NextGameCommand != null)
                                        {
                                            GameCommand currentGameCommand = ant.Unit.CurrentGameCommand;
                                            GameCommand nextGameCommand = ant.Unit.CurrentGameCommand.NextGameCommand;

                                            nextGameCommand.PlayerId = currentGameCommand.PlayerId;

                                            // Finish the current Command
                                            currentGameCommand.NextGameCommand = null;
                                            currentGameCommand.CommandComplete = true;

                                            // Attach the next command
                                            player.GameCommands.Add(nextGameCommand);
                                            nextGameCommand.AttachedUnit.SetUnitId(ant.Unit.UnitId);
                                            ant.Unit.SetGameCommand(nextGameCommand);
                                        }
                                        else if (ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                                        {
                                            if (ant.Unit.CurrentGameCommand.FollowUpUnitCommand == FollowUpUnitCommand.Attack ||
                                                ant.Unit.CurrentGameCommand.FollowUpUnitCommand == FollowUpUnitCommand.HoldPosition)
                                            {
                                                ant.Unit.CurrentGameCommand.GameCommandType = GameCommandType.AttackMove;
                                                ant.Unit.CurrentGameCommand.AttachedUnit.SetStatus("AttackMoveToTarget");
                                                // This command is only for this unit
                                                ant.Unit.CurrentGameCommand.UnitId = ant.Unit.UnitId;
                                                ant.Unit.Changed = true;
                                            }
                                            else if (ant.Unit.CurrentGameCommand.FollowUpUnitCommand == FollowUpUnitCommand.DeleteCommand)
                                            {
                                                // Unit has been build. Eiher keep the command, so it is rebuild, oder remove the command.
                                                // But the command cannot stay at the build units, since this blocks all further commands.
                                                ant.Unit.CurrentGameCommand.CommandComplete = true;
                                                ant.Unit.CurrentGameCommand.AttachedUnit.SetStatus("CommandComplete");
                                                ant.Unit.Changed = true;
                                            }
                                            else
                                            {
                                                ant.Unit.CurrentGameCommand.CommandComplete = true;
                                                ant.Unit.ResetGameCommand();
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                        }
                        // First time the unit is complete
                        if (ant.Unit.Engine == null)
                        {
                            //ConnectNearbyAnts(ant);
                        }
                        ant.UnderConstruction = false;
                    }
                    //UpdateUnitCounters(ant);

                    movableAnts.Add(ant);
                }
                else if (ant.Unit.UnderConstruction)
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
                        //UpdateUnitCounters(ant);
                        // cannot move until complete
                        //movableAnts.Add(ant);
                    }
                }
                else
                {
                    // Damaged Unit
                    if (ant.Unit.CurrentGameCommand != null &&
                        ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                        ant.Unit.CurrentGameCommand.FactoryUnit.UnitId == ant.Unit.UnitId)
                    {
                        // This is the assembler, building a structure. let the unit complete the command.
                        ant.CreateAntParts();
                        movableAnts.Add(ant);
                    }
                    else
                    {
                        // Another ant has to take this task
                        if (ant.Unit.CurrentGameCommand != null)
                        {
                            if (ant.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                                ant.Unit.CurrentGameCommand.FactoryUnit.UnitId == ant.Unit.UnitId)
                            {
                                // This is the assembler, building a structure. let the unit complete the command.
                                ant.CreateAntParts();
                                movableAnts.Add(ant);
                            }
                            else
                            {
                                ant.Unit.ResetGameCommand();
                            }
                        }
                        ant.StuckCounter++;
                        if (ant.StuckCounter > 10)
                            ant.AbandonUnit(player);

                        if (ant.Unit.Engine != null)
                        {
                            // The unit may block a position needed. Let the unit move somewhere
                            ant.CreateAntParts();
                            movableAnts.Add(ant);
                        }
                    }
                    // If extracted immediatly, a assembler has no chance to repair
                    //ant.AbandonUnit(player);

                    /* If unit is kept it must be aligend with attachcommands
                    if (ant.Unit.Engine != null)
                    {
                        if (ant.Unit.Extractor == null &&
                            ant.Unit.Weapon != null &&
                            !ant.Unit.Weapon.WeaponLoaded)
                        {
                            // Cannot refill to fire, useless unit
                            ant.Unit.ExtractUnit();
                        }
                        ant.CreateAntParts();
                        movableAnts.Add(ant);
                    }
                    else
                    {
                        ant.AbandonUnit(player);
                    }*/
                }
            }
            unmovedAnts.AddRange(movableAnts);

#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Collected Ants " + timetaken);
                start = DateTime.Now;
            }
#endif

            UpdatePheromones(player);
            if (!player.PlayerModel.IsHuman)
            {
                CreateCommands(player);
            }
            // DEBUG! 
            //if (NumberOfWorkers > 0) CreateCommandForContainerInZone(player, player.StartZone.ZoneId);

            if (MapPlayerInfo.PowerOutInTurns < 10)
            {
                // Sacrifice a unit (Count is wrong)
                //SacrificeAnt(player, unmovedAnts);
            }
            FinishCompleteCommands(player);
            AttachGamecommands(player, unmovedAnts, moves, 1);
            AttachGamecommands(player, unmovedAnts, moves, 0);
            RemoveCompletedCommands(player);

#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Commands " + timetaken);
                start = DateTime.Now;
            }
#endif


#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Execute1 " + timetaken);
                start = DateTime.Now;
            }
#endif

            // Any other moves
            unmovedAnts.Clear();
            unmovedAnts.AddRange(movableAnts);
            foreach (Ant ant in unmovedAnts)
            {
                // Move structures first
                if (ant.AntPartEngine == null && ant.MoveStructure(player, moves))
                {
                    movableAnts.Remove(ant);
                }
            }
            unmovedAnts.Clear();
            unmovedAnts.AddRange(movableAnts);
            foreach (Ant ant in unmovedAnts)
            {
                // Move units next
                if (ant.AntPartEngine != null && ant.MoveUnit(player, moves))
                {
                    movableAnts.Remove(ant);
                }
            }
#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Execute2 " + timetaken);
                start = DateTime.Now;
            }
#endif

            movableAnts.Clear();           
            unmovedAnts.Clear();

            // Execute extract moves at the end if no other move was possible.
            /*
            foreach (Ant ant in unmovedAnts)
            {
                // Far transport... not included now
                if (ant.AntPartContainer != null)
                {
                    if (CheckTransportMove(ant, moves))
                    {
                        movableAnts.Remove(ant);
                        continue;
                    }
                }
            }
            */
            foreach (Ant ant in killedAnts)
            {
                ant.OnDestroy(player);
            }

#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Execute3 " + timetaken);
                start = DateTime.Now;
            }
#endif

            // Count capacities 

            foreach (Ant ant in Ants.Values)
            {
                if (ant.AntPartContainer != null)
                {
                    Tile tile = player.Game.Map.GetTile(ant.Unit.Pos);
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

#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Count capacities  " + timetaken);
                start = DateTime.Now;
            }
#endif

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (gameCommand.BlueprintName == "Container" || gameCommand.BlueprintName == "Outpost")
                    {
                        AntCollect antCollect;
                        if (exceedingMinerals.TryGetValue(gameCommand.TargetZone, out antCollect))
                        {
                            if (gameCommand.BlueprintName == "Outpost")
                                antCollect.TotalCapacity += 24;
                            else
                                // Container in build
                                antCollect.TotalCapacity += 72;
                        }
                    }

                }
            }
#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log(" player.GameCommands " + timetaken);
                start = DateTime.Now;
            }
#endif

            foreach (KeyValuePair<int, AntCollect> keypair in exceedingMinerals)
            {
                int zoneId = keypair.Key;
                AntCollect antCollect = keypair.Value;

                if (antCollect.TotalCapacity < 10 && antCollect.AllCollectables > 5)
                {
                    CreateCommandForContainerInZone(player, zoneId);
                }
            }

#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
            {
                UnityEngine.Debug.Log("Turn Done " + timetaken);
                start = DateTime.Now;
            }
#endif
            return moves;
        }
    }
}
