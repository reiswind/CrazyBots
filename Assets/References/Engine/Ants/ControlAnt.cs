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
                    if (blueprintCommand.Name == "Units")
                    {
                        patrolCommand = new GameCommand(blueprintCommand);
                        foreach (GameCommandItem gameCommandItem in patrolCommand.GameCommandItems)
                        {
                            gameCommandItem.DeleteWhenDestroyed = true;
                            gameCommandItem.FollowPheromones = true;
                        }
                        player.GameCommands.Add(patrolCommand);
                        break;
                    }
                }
            }
            else
            {
                if (MapPlayerInfo.TotalMinerals > 40)
                {
                    bool commandComplete = true;
                    foreach (GameCommandItem gameCommandItem in patrolCommand.GameCommandItems)
                    {
                        if (string.IsNullOrEmpty(gameCommandItem.AttachedUnitId))
                        {
                            commandComplete = false;
                            break;
                        }
                    }
                    if (commandComplete)
                    {
                        GameCommandItem gameCommandItem = new GameCommandItem(patrolCommand);

                        gameCommandItem.BlueprintName = "Bomber";
                        gameCommandItem.DeleteWhenDestroyed = true;
                        gameCommandItem.FollowPheromones = true;

                        patrolCommand.GameCommandItems.Add(gameCommandItem);
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
                    if (gameCommand.GameCommandType == GameCommandType.Attack)
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

            //AntCollects.Clear();

            List<Position2> minerals = new List<Position2>();
            minerals.AddRange(staticMineralDeposits.Keys);

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
                    /*
                    AntCollect antCollect;
                    if (!AntCollects.TryGetValue(tile.ZoneId, out antCollect))
                    {
                        antCollect = new AntCollect();
                        AntCollects.Add(tile.ZoneId, antCollect);
                    }
                    antCollect.Minerals += mins;*/
                }
            }

            foreach (Position2 pos in minerals)
            {
                MineralDeposit mineralDeposit = staticMineralDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticMineralDeposits.Remove(pos);
            }

            minerals = new List<Position2>();
            minerals.AddRange(staticContainerDeposits.Keys);

            foreach (Ant ant in Ants.Values)
            {
                if (ant.Unit != null &&
                    ant.Unit.IsComplete() &&
                    ant.AntPartEngine == null &&
                    ant.AntPartContainer != null && ant.AntPartContainer.Container.TileContainer.IsFreeSpace)
                {
                    int sectorSize = player.Game.Map.SectorSize;
                    float intensity = 0.1f;
                    Position2 pos = ant.Unit.Pos;

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
                        mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, ant.Unit.Pos, sectorSize, PheromoneType.Container, intensity, 0.01f);
                        staticContainerDeposits.Add(ant.Unit.Pos, mineralDeposit);
                    }
                }
            }
            foreach (Position2 pos in minerals)
            {
                MineralDeposit mineralDeposit = staticContainerDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticContainerDeposits.Remove(pos);
            }

            minerals = new List<Position2>();
            minerals.AddRange(staticReactorDeposits.Keys);

            foreach (Ant ant in Ants.Values)
            {
                if (ant.Unit != null &&
                    ant.Unit.IsComplete() &&
                    ant.AntPartEngine == null &&
                    ant.AntPartReactor != null)
                {
                    if (ant.Unit.Reactor.AvailablePower > 0)
                    {
                        float intensity = 1f;
                        Position2 pos = ant.Unit.Pos;

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
                            mineralDeposit.DepositId = player.Game.Pheromones.DropStaticPheromones(player, ant.Unit.Pos, ant.Unit.Reactor.Range, PheromoneType.Energy, intensity);
                            staticReactorDeposits.Add(ant.Unit.Pos, mineralDeposit);
                        }
                    }
                }
            }
            foreach (Position2 pos in minerals)
            {
                MineralDeposit mineralDeposit = staticReactorDeposits[pos];
                player.Game.Pheromones.DeletePheromones(mineralDeposit.DepositId);
                staticReactorDeposits.Remove(pos);
            }

        }
        public void CreateAttackCommand(Player player, Position2 pos)
        {
            bool commandActive = false;
            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Attack)
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
                    if (blueprintCommand.GameCommandType == GameCommandType.Attack)
                    {
                        GameCommand gameCommand = new GameCommand(blueprintCommand);

                        gameCommand.GameCommandType = blueprintCommand.GameCommandType;
                        gameCommand.TargetPosition = pos;
                        gameCommand.PlayerId = player.PlayerModel.Id;
                        gameCommand.DeleteWhenFinished = true;

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
                        GameCommand gameCommand = new GameCommand(blueprintCommand);

                        gameCommand.GameCommandType = GameCommandType.Collect;
                        //gameCommand.TargetZone = zoneId;
                        gameCommand.Radius = 4;
                        gameCommand.TargetPosition = pos;
                        gameCommand.PlayerId = player.PlayerModel.Id;
                        gameCommand.DeleteWhenFinished = true;

                        gameCommand.IncludedPositions = player.Game.Map.EnumerateTiles(gameCommand.TargetPosition, gameCommand.Radius, true);

                        player.GameCommands.Add(gameCommand);
                        break;
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

            foreach (Position2 suggestedBuildLocation in suggestedBuildLocations)
            {
                foreach (Ant ant in neighborContainers)
                {
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

                        // Can the location be reached?
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

            if (possibleBuildLocations.Count > 0)
            {
                // pick a rondom of the best
                int idx = player.Game.Random.Next(possibleBuildLocations.Count);
                Position2 buildPosition = possibleBuildLocations[idx];

                int chanceoutpost = player.Game.Random.Next(10);
                chanceoutpost = 0;
                foreach (BlueprintCommand blueprintCommand in player.Game.Blueprints.Commands)
                {
                    if (chanceoutpost == 1 && blueprintCommand.Name == "Outpost")
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
                    else if (blueprintCommand.Name == "Container")
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
        private Dictionary<Position2, MineralDeposit> staticMineralDeposits = new Dictionary<Position2, MineralDeposit>();
        private Dictionary<Position2, MineralDeposit> staticContainerDeposits = new Dictionary<Position2, MineralDeposit>();
        private Dictionary<Position2, MineralDeposit> staticReactorDeposits = new Dictionary<Position2, MineralDeposit>();
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
            
            //foreach (PlayerUnit currentUnit in player.PlayerUnits.Values)
            foreach (Ant ant in Ants.Values)
            {
                if (ant.Unit.Pos == destination)
                {
                    if (ant.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                        ant.Unit.Engine != null)
                    {
                        occupied = true;

                        // Our own unit, that has engine may move away
                        foreach (Move intendedMove in moves)
                        {
                            if ((intendedMove.MoveType == MoveType.Move || intendedMove.MoveType == MoveType.Add || intendedMove.MoveType == MoveType.Build) &&
                                ant.Unit.UnitId == intendedMove.UnitId)
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
            if (antWorker.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
            {
                // Need neighbor pos
                Tile t = player.Game.Map.GetTile(antWorker.Unit.CurrentGameCommand.GameCommand.TargetPosition);
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
                bestPositions = player.Game.FindPath(antWorker.Unit.Pos, antWorker.Unit.CurrentGameCommand.GameCommand.TargetPosition, antWorker.Unit);
            }
            return MakePathFromPositions(bestPositions, antWorker);
        }
    

        public Position2 FindContainer(Player player, Ant antWorker)
        {
            return Position2.Null;
            /*
            Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(antWorker.PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
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
                List<Position2> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, t.Pos, antWorker.PlayerUnit.Unit);
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
                        Position2 posFactory = ant.PlayerUnit.Unit.Pos;
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

                            List<Position2> positions = player.Game.FindPath(antWorker.PlayerUnit.Unit.Pos, posFactory, antWorker.PlayerUnit.Unit);
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
            return Position2.Null;
            /*
            List<Position2> bestPositions = null;

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
            */
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
        }

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
                    if (ant.Unit.Engine != null)
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

        private void AttachGamecommands(Player player, List<Ant> unmovedAnts, List<Move> moves)
        {
            List<GameCommand> completedCommands = new List<GameCommand>();
            List<GameCommand> cancelCommands = new List<GameCommand>();
            List<GameCommand> removeCommands = new List<GameCommand>();

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.AddUnits)
                {
                    gameCommand.CommandComplete = true;
                    gameCommand.DeleteWhenFinished = true;

                    foreach (GameCommand otherGameCommand in player.GameCommands)
                    {
                        if (otherGameCommand != gameCommand &&
                            otherGameCommand.TargetPosition == gameCommand.TargetPosition)
                        {
                            foreach (GameCommandItem gameCommandItem in gameCommand.GameCommandItems)
                            {
                                GameCommandItem otherGameCommandItem = new GameCommandItem(otherGameCommand);
                                otherGameCommandItem.Direction = gameCommandItem.Direction;
                                otherGameCommandItem.BlueprintName = gameCommandItem.BlueprintName;
                                otherGameCommandItem.Position3 = gameCommandItem.Position3;
                                otherGameCommandItem.RotatedDirection = gameCommandItem.RotatedDirection;
                                otherGameCommandItem.RotatedPosition3 = gameCommandItem.RotatedPosition3;
                                
                                otherGameCommand.GameCommandItems.Add(otherGameCommandItem);
                            }
                            break;
                        }
                    }
                }
                if (gameCommand.GameCommandType == GameCommandType.Move)
                {
                    foreach (GameCommand moveGameCommand in player.GameCommands)
                    {
                        if (moveGameCommand.TargetPosition == gameCommand.TargetPosition)
                        {
                            foreach (GameCommandItem moveGameCommandItem in moveGameCommand.GameCommandItems)
                            {
                                foreach (GameCommandItem gameCommandItem in gameCommand.GameCommandItems)
                                {
                                    if (gameCommandItem.Position3 == moveGameCommandItem.Position3)
                                    {
                                        moveGameCommandItem.Position3 = gameCommandItem.RotatedPosition3;
                                        moveGameCommandItem.Direction = gameCommandItem.RotatedDirection;
                                        break;
                                    }

                                    /*
                                    foreach (BlueprintCommandItem moveBlueprintCommandItem in moveGameCommand.BlueprintCommand.Units)
                                    {


                                    }*/
                                }
                            }
                            moveGameCommand.TargetPosition = gameCommand.MoveToPosition;
                            moveGameCommand.IncludedPositions = gameCommand.IncludedPositions;
                            
                            gameCommand.CommandComplete = true;
                            removeCommands.Add(gameCommand);
                            completedCommands.Add(gameCommand);
                            break;
                        }
                    }
                }
                if (gameCommand.GameCommandType == GameCommandType.Cancel)
                {
                    gameCommand.CommandComplete = true;
                    gameCommand.DeleteWhenFinished = true;

                    foreach (GameCommand otherGameCommand in player.GameCommands)
                    {
                        if (otherGameCommand != gameCommand &&
                            otherGameCommand.TargetPosition == gameCommand.TargetPosition &&
                            otherGameCommand.BlueprintName == gameCommand.BlueprintName)
                        {
                            foreach (Ant ant in unmovedAnts)
                            {
                                if (ant.Unit.CurrentGameCommand != null &&
                                    ant.Unit.CurrentGameCommand.GameCommand == otherGameCommand)
                                {
                                    ant.Unit.ResetGameCommand();
                                }
                            }
                            otherGameCommand.CommandComplete = true;
                            otherGameCommand.DeleteWhenFinished = true;
                            completedCommands.Add(otherGameCommand);
                            cancelCommands.Add(otherGameCommand);
                        }
                    }
                }
                if (gameCommand.GameCommandType == GameCommandType.Collect)
                {

                }
                if (gameCommand.GameCommandType == GameCommandType.Extract)
                {
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
                    bool requestUnit = false;

                    if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.Build &&
                        gameCommandItem.AttachedUnitId != null && gameCommandItem.FactoryUnitId == null)
                    {
                        // Check if the unit to be build is there
                        Unit unit = player.Game.Map.Units.FindUnit(gameCommandItem.AttachedUnitId);
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
                    if (gameCommandItem.AttachedUnitId == null && gameCommandItem.FactoryUnitId == null)
                    {
                        if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.Build)
                        {
                            // Check if the target to be build is already there, if so, ignore this command
                            Tile t = player.Game.Map.GetTile(gameCommandItem.GameCommand.TargetPosition);
                            if (t.Unit == null)
                            {
                                requestUnit = true;
                            }
                            else
                            {
                                if (t.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id && t.Unit.Blueprint.Name == gameCommandItem.BlueprintName)
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
                        else
                        {
                            requestUnit = true;
                        }
                    }
                    if (requestUnit)
                    {
                        // Find a existing unit that can do it
                        foreach (Ant ant in unmovedAnts)
                        {
                            if (ant.Unit.CurrentGameCommand == null && !ant.Unit.UnderConstruction && !ant.Unit.ExtractMe)
                            {
                                if (gameCommandItem.GameCommand.GameCommandType == GameCommandType.Build)
                                {
                                    if (ant.Unit.Blueprint.Name == "Assembler")
                                    {
                                        gameCommandItem.FactoryUnitId = ant.Unit.UnitId;
                                        ant.Unit.SetGameCommand(gameCommandItem);
                                        requestUnit = false;
                                    }
                                }
                                else
                                {
                                    if (ant.Unit.Blueprint.Name == gameCommandItem.BlueprintName)
                                    {
                                        gameCommandItem.AttachedUnitId = ant.Unit.UnitId;
                                        ant.Unit.SetGameCommand(gameCommandItem);
                                        requestUnit = false;
                                    }
                                }
                            }
                            if (requestUnit == false)
                                break;
                        }
                    }
                    if (requestUnit)
                    { 
                        // Find a factory
                        bestAnt = null;
                        bestDistance = 0;

                        foreach (Ant ant in unmovedAnts)
                        {
                            if (ant.AntPartAssembler != null) //.AntWorkerType == AntWorkerType.Assembler)
                            {
                                if (ant.Unit.CurrentGameCommand == null &&
                                    !ant.Unit.UnderConstruction &&
                                    !ant.Unit.ExtractMe)
                                {
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
                            bestAnt.Unit.SetGameCommand(gameCommandItem);
                            gameCommandItem.FactoryUnitId = bestAnt.Unit.UnitId;
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
                                        //double distance = ant.PlayerUnit.Unit.Pos.GetDistanceTo(gameCommand.TargetPosition2);
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
            foreach (GameCommand gameCommand in completedCommands)
            {
                if (gameCommand.DeleteWhenFinished)
                {
                    foreach (Ant ant in Ants.Values)
                    {
                        if (ant.Unit.CurrentGameCommand != null &&
                            ant.Unit.CurrentGameCommand.GameCommand == gameCommand)
                        {
                            ant.Unit.ResetGameCommand();
                        }
                    }
                    player.GameCommands.Remove(gameCommand);
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
            List<Tile> possiblePosition2s = new List<Tile>();

            // Find all reactors
            foreach (Ant ant in Ants.Values)
            {
                if (ant.PlayerUnit != null && ant.PlayerUnit.Unit.Reactor != null)
                {
                    // Find build location
                    Dictionary<Position2, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(ant.PlayerUnit.Unit.Pos, 6, false);
                    foreach (TileWithDistance tileWithDistance in tiles.Values)
                    {
                        if (tileWithDistance.Distance < 6)
                            continue;
                        if (tileWithDistance.Tile.CanMoveTo(ant.PlayerUnit.Unit.Pos))
                        {
                            possiblePosition2s.Add(tileWithDistance.Tile);
                        }
                    }
                }
            }
            if (possiblePosition2s.Count > 0)
            {
                int idx = player.Game.Random.Next(possiblePosition2s.Count);
                Tile t = possiblePosition2s[idx];

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
                            if (cntrlUnit.Blueprint.Name == "Assembler")
                            {
                                ant.AntWorkerType = AntWorkerType.Assembler;
                            }
                            if (cntrlUnit.Blueprint.Name == "Fighter" || cntrlUnit.Blueprint.Name == "Bomber")
                            {
                                ant.AntWorkerType = AntWorkerType.Fighter;
                            }
                            if (cntrlUnit.Blueprint.Name == "Worker")
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
                        if (cntrlUnit.Blueprint.Name == "Assembler" ||
                            cntrlUnit.Blueprint.Name == "Fighter" ||
                            cntrlUnit.Blueprint.Name == "Worker" ||
                            cntrlUnit.Blueprint.Name == "Bomber")
                        {
                            Ant antWorker = new Ant(this);
                            //antWorker.PlayerUnit = playerUnit;
                            antWorker.Alive = true;

                            if (cntrlUnit.Direction == Direction.C)
                            {
                                cntrlUnit.Direction = Direction.SW;
                            }

                            if (cntrlUnit.Blueprint.Name == "Assembler")
                                antWorker.AntWorkerType = AntWorkerType.Assembler;
                            else if (cntrlUnit.Blueprint.Name == "Fighter" || cntrlUnit.Blueprint.Name == "Bomber")
                                antWorker.AntWorkerType = AntWorkerType.Fighter;
                            else if (cntrlUnit.Blueprint.Name == "Worker")
                                antWorker.AntWorkerType = AntWorkerType.Worker;
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

                    //ant.CreateAntParts();

                    if (ant.UnderConstruction)
                    {
                        if (ant.Unit.CurrentGameCommand != null)
                        {
                            if (ant.Unit.CurrentGameCommand.FactoryUnitId != null)
                            {
                                if (ant.Unit.CurrentGameCommand.FactoryUnitId != ant.Unit.UnitId)
                                {
                                    Unit factoryUnit = player.Game.Map.Units.FindUnit(ant.Unit.CurrentGameCommand.FactoryUnitId);
                                    if (factoryUnit != null)
                                    {
                                        factoryUnit.ResetGameCommand();
                                    }
                                    if (ant.Unit.Blueprint.Name == "Assembler")
                                    {
                                        ant.Unit.CurrentGameCommand.AttachedUnitId = null;
                                        ant.Unit.CurrentGameCommand.FactoryUnitId = ant.Unit.UnitId;
                                    }
                                    else
                                    {
                                        ant.Unit.CurrentGameCommand.FactoryUnitId = null;
                                        //ant.PlayerUnit.Unit.CurrentGameCommand.AttachedUnitId = ant.PlayerUnit.Unit.UnitId;

                                        if (ant.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
                                        {
                                            // Unit has been build. Eiher keep the command, so it is rebuild, oder remove the command.
                                            // But the command cannot stay at the build units, since this blocks all further commands.
                                            if (ant.Unit.CurrentGameCommand.GameCommand.DeleteWhenFinished)
                                            {
                                                ant.Unit.CurrentGameCommand.GameCommand.CommandComplete = true;
                                            }
                                            else
                                            {
                                                ant.Unit.ResetGameCommand();
                                            }
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
                    UpdateUnitCounters(ant);

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
                        UpdateUnitCounters(ant);
                        // cannot move until complete
                        //movableAnts.Add(ant);
                    }
                }
                else
                {
                    // Damaged Unit

                    // Another ant has to take this task
                    if (ant.Unit.CurrentGameCommand != null)
                    {
                        //ant.PlayerUnit.Unit.CurrentGameCommand.AttachedUnits.Remove(ant.PlayerUnit.Unit.UnitId);
                        ant.RemoveAntFromAllCommands(player);
                        ant.Unit.ResetGameCommand();
                    }

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
                    }
                }
            }
            unmovedAnts.AddRange(movableAnts);

            UpdatePheromones(player);
            if (!player.PlayerModel.IsHuman)
            {
                CreateCommands(player);
            }
            // DEBUG! 
            //if (NumberOfWorkers > 0) CreateCommandForContainerInZone(player, player.StartZone.ZoneId);

            if (MapPlayerInfo.TotalPower == 0)
            {
                // Sacrifice a unit
                SacrificeAnt(player, unmovedAnts);
            }

            AttachGamecommands(player, unmovedAnts, moves);

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
                        if (ant.CurrentGameCommand == null && ant.HoldPosition2)
                        {
                            movableAnts.Remove(ant);
                            continue;
                        }*/
                        if (IsBeingExtracted(moves, ant.Unit.Pos))
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
                //Ants.Remove(ant.Unit.UnitId);
            }

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

            foreach (GameCommand gameCommand in player.GameCommands)
            {
                if (gameCommand.GameCommandType == GameCommandType.Build)
                {
                    //foreach (BlueprintCommandItem blueprintCommandItem in gameCommand.BlueprintCommand.Units)
                    foreach (GameCommandItem gameCommandItem in gameCommand.GameCommandItems)
                    {
                        if (gameCommandItem.BlueprintName == "Container" || gameCommandItem.BlueprintName == "Outpost")
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
