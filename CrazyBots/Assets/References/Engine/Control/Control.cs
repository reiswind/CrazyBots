using Engine.Algorithms;
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
    internal class ControlLevelI : IControl
    {
        public PlayerModel PlayerModel { get; set; }
        public GameModel GameModel { get; set; }
        private IGameController GameController;


        public ControlLevelI(IGameController gameController, PlayerModel playerModel, GameModel gameModel)
        {
            GameController = gameController;
            PlayerModel = playerModel;
            GameModel = gameModel;
        }
        
        public void ProcessMoves(Player player, List<Move> moves)
        {

        }
        
        public static bool FiredAt(Player player, List<Move> possibleMoves, Position destination)
        {
            bool occupied = false;


            foreach (Move intendedMove in possibleMoves)
            {
                if (intendedMove.MoveType == MoveType.Fire)
                {
                    if (intendedMove.Positions[1] == destination)
                    {
                        occupied = true;
                        break;
                    }
                }
            }

            return occupied;
        }

        public static bool HasMoved(List<Move> moves, Unit unit)
        {
            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Move ||
                    intendedMove.MoveType == MoveType.Add ||
                    intendedMove.MoveType == MoveType.Build)
                {
                    if (intendedMove.UnitId == unit.UnitId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsOccupied(Player player, List<Move> moves, Position destination)
        {
            bool occupied = false;

            // Check if the
            foreach (PlayerUnit currentUnit in player.Units.Values)
            {
                if (currentUnit.Unit.Pos == destination)
                {
                    // cannot move here or did unit move away?
                    occupied = true;
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


        // Things, that units do by themselves
        private void ComputeAutoUnitMove(List<PlayerUnit> moveableUnits)
        {
            //List<PlayerUnit> unitsThathaveBeenMoved = new List<PlayerUnit>();

            foreach (PlayerUnit playerUnit in moveableUnits)
            {
                Unit cntrlUnit = playerUnit.Unit;

                //bool autoMove = false;                    
                if (cntrlUnit.Weapon != null)
                {
                    if (cntrlUnit.Weapon.WeaponLoaded)
                    {
                        List<Move> possiblemoves = new List<Move>();
                        cntrlUnit.Weapon.ComputePossibleMoves(possiblemoves, null, MoveFilter.Fire);
                        if (possiblemoves.Count > 0)
                        {
                            int idx = GameController.Random.Next(possiblemoves.Count);
                            playerUnit.PossibleMoves.Add(new PlayerMove(possiblemoves[idx]));

                            //unitsThathaveBeenMoved.Add(playerUnit);
                            continue;
                        }
                        /*
                        foreach (Move move in possiblemoves)
                        {
                            //if (!FiredAt(player, possiblemoves, move.Positions[1]))
                            {
                                moves.Add(move);
                                playerUnit.PossibleMoves.Add(move);
                                //autoMove = false;
                                break;
                            }
                        }*/
                    }
                }
                if (cntrlUnit.Extractor != null) // && cntrlUnit.Metal == 0)
                {
                    // Reload somehow
                    List<Move> possiblemoves = new List<Move>();
                    cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, null, MoveFilter.Extract);
                    if (possiblemoves.Count > 0)
                    {
                        int idx = GameController.Random.Next(possiblemoves.Count);                        
                        playerUnit.PossibleMoves.Add(new PlayerMove(possiblemoves[idx]));
                        //unitsThathaveBeenMoved.Add(playerUnit);
                        continue;
                    }
                }
            }
            /*
            foreach (PlayerUnit playerUnit in unitsThathaveBeenMoved)
            {
                moveableUnits.Remove(playerUnit);
            }*/
        }

        private void GenerateAiActions(Player player)
        {
            /*
            bool scoutFound = false;
            foreach (Command command in player.Commands)
            {
                if (command is Scout)
                {
                    scoutFound = true;
                    break;
                }
            }
            if (!scoutFound)
            {
                foreach (Area area in player.Game.Areas)
                {
                    if (player.PlayerModel.Id == area.PlayerId)
                    {
                        int idx = GameController.Random.Next(area.ForeignBorderTiles.Count);
                        Tile t = area.ForeignBorderTiles.ElementAt(idx).Value;

                        Scout scout = new Scout();
                        scout.Center = t.Pos;
                        scout.Map = player.Game.Map;

                        player.Commands.Add(scout);

                        break;
                    }
                }
            }
            */
            List<Attack> commandsAtBorder = new List<Attack>();

            List<Assemble> availableAssembler = new List<Assemble>();
            foreach (Command command in player.Commands)
            {
                Assemble assemble = command as Assemble;
                if (assemble != null)
                {
                    availableAssembler.Add(assemble);
                }
            }
            if (availableAssembler.Count == 0)
                return;
            if (player.Game.Areas != null)
            {
                foreach (Area area in player.Game.Areas)
                {
                    if (player.PlayerModel.Id == area.PlayerId)
                    {
                        foreach (Tile borderTile in area.BorderTiles.Values)
                        {
                            bool commandFound = false;
                            foreach (Command command in player.Commands)
                            {
                                Attack attack = command as Attack;
                                if (attack != null && attack.Center == borderTile.Pos)
                                {
                                    commandsAtBorder.Add(attack);
                                    commandFound = true;
                                    break;
                                }
                            }
                            if (!commandFound)
                            {
                                // Add Attack command
                                Attack attack = new Attack();
                                attack.Center = borderTile.Pos;
                                attack.Map = player.Game.Map;

                                attack.Livetime = 500000;

                                Assemble assemble = availableAssembler[0];

                                List<Position> path = GameController.FindPath(assemble.Center, attack.Center, null);
                                if (path != null && path.Count > 4)
                                {
                                    // Start a little away from the fac
                                    for (int i = 0; i < 3 && path.Count > 0; i++)
                                        path.RemoveAt(0);

                                    CommandSource commandSource = new CommandSource();
                                    commandSource.Parent = assemble;
                                    commandSource.Child = attack;
                                    commandSource.Path = path;
                                    attack.CommandSources.Add(commandSource);

                                    commandSource = new CommandSource();
                                    commandSource.Parent = attack;
                                    commandSource.Child = assemble;
                                    commandSource.Path = path;
                                    assemble.CommandSources.Add(commandSource);

                                    commandsAtBorder.Add(attack);
                                    player.Commands.Insert(0, attack);
                                }                                
                            }
                        }
                    }
                }
            }
            List<Command> comandsToRemove = new List<Command>();
            foreach (Command command in player.Commands)
            {
                Attack attack = command as Attack;
                if (attack != null && !commandsAtBorder.Contains(attack))
                {
                    comandsToRemove.Add(attack);
                }
                if (command.StuckCounter > 3)
                {
                    Console.WriteLine("Remove Command. StuckCounter > 3");

                    // Units cannot reach this point. Remove this command
                    comandsToRemove.Add(attack);
                }
            }

            foreach (Command command in comandsToRemove)
            {
                player.DeleteCommand(command.CommandId);
            }
        }

        private static int moveNr;

        public List<Move> Turn(Player player)
        {
            moveNr++;

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

            foreach (PlayerUnit playerUnit in player.Units.Values)
            {
                Unit cntrlUnit = playerUnit.Unit;
                if (cntrlUnit.Owner.PlayerModel.Id == PlayerModel.Id)
                {
                    playerUnit.PossibleMoves.Clear();
                    moveableUnits.Add(playerUnit);
                }
            }
            // Units own brain moves
            ComputeAutoUnitMove(moveableUnits);

            if (player.PlayerModel.ControlLevel != 0)
            {
                GenerateAiActions(player);
            }

            Dispatcher dispatcher = new Dispatcher();
            dispatcher.GameController = GameController;
            dispatcher.Run(player, moveableUnits);

            //dispatcher.MoveStrayCompleteUnits(player, moveableUnits);

            // Convert attack requests into attack commands            
            foreach (DispatcherRequestAttack enemyUnit in dispatcher.AttackThisUnits)
            {
                bool commandAttached = false;

                // Check if order is already given
                foreach (Command command in player.Commands)
                {
                    Attack attack = command as Attack;
                    if (attack != null)
                    {
                        if (attack.PosititionsInArea != null &&
                            attack.PosititionsInArea.ContainsKey(enemyUnit.EnemyUnit.Unit.Pos))
                        {
                            commandAttached = true;
                            break;
                        }
                    }
                }
                if (!commandAttached)
                {
                    Attack attack = new Attack();
                    attack.Center = enemyUnit.EnemyUnit.Unit.Pos;
                    attack.Map = player.Game.Map;

                    CommandSource commandSource = new CommandSource();
                    commandSource.Parent = enemyUnit.Command;
                    commandSource.Child = attack;

                    attack.CommandSources.Add(commandSource);

                    player.Commands.Insert(0, attack);
                }
            }

            dispatcher.HandleUnitRequests(player, moveableUnits);

            // Collect defect units
            /*
            foreach (PlayerUnit playerUnit in moveableUnits) // dispatcher.SupportUnits)
            {
                if (!moveableUnits.Contains(playerUnit))
                    continue;

                if (playerUnit.PossibleMoves.Count > 0)
                    continue;

                Unit cntrlUnit = playerUnit.Unit;
                if (cntrlUnit.Extractor != null && cntrlUnit.Extractor.CanExtract)
                {
                    // Collect stray units
                    dispatcher.ExtractStrayUnits(player, playerUnit, moveableUnits);
                }
            }*/



            foreach (DispatcherRequestMove requestMove in dispatcher.MoveThisUnits)
            {
                if (HasMoved(moves, requestMove.PlayerUnit.Unit))
                {

                }

                // Would otherwise overried extrace move
                if (requestMove.PlayerUnit.PossibleMoves.Count > 0)
                    continue;

                if (requestMove.PlayerUnit.Unit.Engine == null)
                {
                    throw new Exception("Not possible");
                }
                Move move = null;
                bool skipMove = false;
                if (requestMove.Command != null &&
                    requestMove.Command.CommandSources.Count > 90 &&
                    requestMove.Command.UnitReachedCommandDoNotFollowPath == false)
                {
                    // Pick a source?
                    CommandSource commandSource = requestMove.Command.CommandSources[0];
                    if (commandSource.Path != null)
                    {
                        for (int i = 0; i < commandSource.Path.Count; i++)
                        {
                            Position pos = commandSource.Path[i];
                            // 
                            if (pos == requestMove.PlayerUnit.Unit.Pos && i < commandSource.Path.Count-1)
                            {
                                Position destination;
                                destination = commandSource.Path[i + 1];

                                Tile t = GameController.GetTile(destination);
                                if (!t.CanMoveTo(pos) || t.Unit != null)
                                {
                                    requestMove.Command.StuckCounter++;
                                    if (requestMove.Command.StuckCounter > 2)
                                    {
                                        // Recalc the unreachable path
                                        List<Position> path = GameController.FindPath(commandSource.Parent.Center, commandSource.Child.Center, requestMove.PlayerUnit.Unit);
                                        if (path != null && path.Count > 4)
                                        {
                                            // Start a little away from the fac
                                            for (int ix = 0; ix < 3 && path.Count > 0; ix++)
                                                path.RemoveAt(0);

                                            commandSource.Path = path;
                                            requestMove.Command.StuckCounter = 0;
                                            Console.WriteLine("Command path failed recalculate");
                                        }
                                        else
                                        {
                                            Console.WriteLine("Command path failed and no path found");
                                        }
                                    }
                                    else
                                    {
                                        // Cannot reach target.
                                        Console.WriteLine("Move stuck");
                                        skipMove = true;
                                    }
                                }
                                else
                                {
                                    List<Position> route = new List<Position>(2);
                                    route.Add(requestMove.PlayerUnit.Unit.Pos);
                                    route.Add(destination);

                                    // Follow path
                                    move = new Move();
                                    move.PlayerId = requestMove.PlayerUnit.Unit.Owner.PlayerModel.Id;
                                    move.MoveType = MoveType.Move;
                                    move.Positions = route;
                                    move.UnitId = requestMove.PlayerUnit.Unit.UnitId;

                                    //Console.WriteLine("Move with command path");
                                    //move = dispatcher.GameController.MoveTo(requestMove.PlayerUnit.Unit.Pos, destination, requestMove.PlayerUnit.Unit.Engine);
                                }
                                break;
                            }
                        }
                        if (move == null && commandSource.Path != null && commandSource.Path.Count > 0)
                        {
                            //Console.WriteLine("Move with pathfinder");

                            // Find a way to beginning of path (or to target?)
                            //requestMove.Pos = commandSource.Path[0];
                            move = dispatcher.GameController.MoveTo(requestMove.PlayerUnit.Unit.Pos, commandSource.Path[0], requestMove.PlayerUnit.Unit.Engine);
                        }
                    }
                }
                if (move == null && !skipMove)
                {
                    // Move to target using pathfinding
                    move = dispatcher.GameController.MoveTo(requestMove.PlayerUnit.Unit.Pos, requestMove.Pos, requestMove.PlayerUnit.Unit.Engine);
                }
                if (move != null)
                {
                    if (IsOccupied(player, moves, move.Positions[1]))
                    {
                        //int x = 0;
                    }
                    else
                    {
                        requestMove.PlayerUnit.PossibleMoves.Add(new PlayerMove(move));
                    }
                }
            }

            // Avoid moves to same position
            List<Position> movedToThisPositions = new List<Position>();

            List<PlayerUnit> movedUnits = new List<PlayerUnit>();
            foreach (PlayerUnit playerUnit in moveableUnits)
            {
                while (playerUnit.PossibleMoves.Count > 0)
                {
                    // Handle requested support
                    int idx = GameController.Random.Next(playerUnit.PossibleMoves.Count);
                    PlayerMove move = playerUnit.PossibleMoves[idx];

                    if ((move.Move.MoveType == MoveType.Move || move.Move.MoveType == MoveType.Add || move.Move.MoveType == MoveType.Build) &&
                        IsOccupied(player, moves, move.Move.Positions[move.Move.Positions.Count - 1]))
                    {
                        // Skip this move, otherwise collision an no unit moves
                        playerUnit.PossibleMoves.Remove(move);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(move.NewUnitId))
                    {
                        string newUnitId = playerUnit.Unit.Owner.Game.GetNextUnitId("unit");
                        playerUnit.PossibleMoves[idx].Move.UnitId = newUnitId + ":" + move.NewUnitId;
                        // Add to the command if the move is selected
                        if (move.Command != null)
                            move.Command.AssignUnit(newUnitId);
                    }

                    moves.Add(playerUnit.PossibleMoves[idx].Move);
                    movedUnits.Add(playerUnit);
                    break;
                }
            }
            foreach (PlayerUnit playerUnit in movedUnits)
                moveableUnits.Remove(playerUnit);

                //foreach (PlayerUnit playerUnit in dispatcher.AttackUnits)

                // Units own brain moves
                /*
                foreach (PlayerUnit playerUnit in player.Units.Values)
                {
                    Unit cntrlUnit = playerUnit.Unit;
                    if (cntrlUnit.Owner.PlayerModel.Id == PlayerModel.Id)
                    {
                        if (playerUnit.PossibleMoves.Count > 0)
                            continue;

                        //bool autoMove = false;                    
                        if (cntrlUnit.Weapon != null)
                        {
                            if (cntrlUnit.Weapon.WeaponLoaded)
                            {
                                List<Move> possiblemoves = new List<Move>();
                                cntrlUnit.Weapon.ComputePossibleMoves(possiblemoves, null, MoveFilter.Fire);
                                foreach (Move move in possiblemoves)
                                {
                                    //if (!FiredAt(player, possiblemoves, move.Positions[1]))
                                    {
                                        moves.Add(move);
                                        //autoMove = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {

                                // Reload somehow
                                if (cntrlUnit.Extractor != null)
                                {
                                    List<Move> possiblemoves = new List<Move>();
                                    cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, null, MoveFilter.Extract);
                                    if (possiblemoves.Count > 0)
                                    {
                                        int idx = Game.Random.Next(possiblemoves.Count);
                                        moves.Add(possiblemoves[idx]);
                                        //autoMove = false;
                                    }
                                }
                            }
                        }

                        if (cntrlUnit.Engine != null && autoMove)
                        {
                            Move move = null;
                            Unit enemy = GetClosestEnemyOutofRange(player, cntrlUnit.Pos, 1);
                            if (enemy != null)
                            {
                                // Automove
                                move = GameController.MoveTo(cntrlUnit.Pos, enemy.Pos, cntrlUnit.Engine);
                                if (move != null)
                                {
                                    if (!IsOccupied(player, player.Game.Map, possibleMoves, move.Positions[1]))
                                    {
                                        possibleMoves.Add(move);
                                    }
                                }
                            }
                            if (move == null)
                            {
                                // Random whatever
                                CubePosition cubePos = null;

                                int destinationsPossible = 6;

                                while (destinationsPossible > 0)
                                {
                                    cubePos = cntrlUnit.Pos.GetCubePosition();

                                    switch (Game.Random.Next(6))
                                    {
                                        case 0:
                                            cubePos.MoveRightDown(cntrlUnit.Game.Map, cntrlUnit.Engine.Range);
                                            break;
                                        case 1:
                                            cubePos.MoveRightUp(cntrlUnit.Game.Map, cntrlUnit.Engine.Range);
                                            break;
                                        case 2:
                                            cubePos.MoveUp(cntrlUnit.Game.Map, cntrlUnit.Engine.Range);
                                            break;
                                        case 3:
                                            cubePos.MoveDown(cntrlUnit.Game.Map, cntrlUnit.Engine.Range);
                                            break;
                                        case 4:
                                            cubePos.MoveLeftUp(cntrlUnit.Game.Map, cntrlUnit.Engine.Range);
                                            break;
                                        case 5:
                                            cubePos.MoveLeftDown(cntrlUnit.Game.Map, cntrlUnit.Engine.Range);
                                            break;
                                    }

                                    if (IsOccupied(player, player.Game.Map, possibleMoves, cubePos.Pos))
                                        destinationsPossible--;
                                    else
                                        break;
                                }
                                if (destinationsPossible > 0)
                                {
                                    move = new Move();
                                    move.PlayerId = PlayerModel.Id;
                                    move.MoveType = MoveType.Move;
                                    move.Positions = new List<Position>();
                                    move.Positions.Add(cntrlUnit.Pos);
                                    move.Positions.Add(cubePos.Pos);
                                    move.UnitId = playerUnit.Unit.UnitId;

                                    possibleMoves.Add(move);
                                }
                            }
                        }
                    }*/

                return moves;
        }
    }
}
