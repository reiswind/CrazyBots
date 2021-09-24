using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public enum RequestType
    {
        Support,
        Scout,
        Attack
    }

    public class DispatcherRequestUnit
    {
        public DispatcherRequestUnit()
        {
            FavoriteUnits = new List<PlayerUnit>();
        }

        public Command Command { get; set; }
        public UnitType UnitType { get; set; }
        public List<PlayerUnit> FavoriteUnits { get; private set; }
    }

    public class DispatcherRequestAttack
    {
        public Command Command { get; set; }
        public PlayerUnit EnemyUnit { get; set; }
    }

    public class DispatcherRequestMove
    {

        public Command Command { get; set; }
        public PlayerUnit PlayerUnit { get; set; }
        public Position Pos { get; set; }
    }

    public class DispatcherRequestExtract
    {
        public PlayerUnit Extractor { get; set; }
        public Dictionary<Position, TileWithDistance> PossibleExtrationTiles { get; set; }
    }

    public class Dispatcher
    {
        public List<PlayerUnit> DefectUnits = new List<PlayerUnit>();
        public List<PlayerUnit> SupportUnits = new List<PlayerUnit>();
        public List<PlayerUnit> AttackUnits = new List<PlayerUnit>();

        public List<PlayerUnit> RequestedUpgrades = new List<PlayerUnit>();
        public List<DispatcherRequestUnit> RequestedUnits = new List<DispatcherRequestUnit>();

        public List<DispatcherRequestMove> MoveThisUnits = new List<DispatcherRequestMove>();

        public List<DispatcherRequestAttack> AttackThisUnits = new List<DispatcherRequestAttack>();

        public IGameController GameController { get; set; }

        public void Run(Player player, List<PlayerUnit> moveableUnits)
        {
            List<Command> commandsToClose = new List<Command>();
            foreach (Command command in player.Commands)
            {
                if (command.CanBeClosed())
                    commandsToClose.Add(command);
                else
                    command.AttachUnits(this, player, moveableUnits);
            }
            foreach (Command command in commandsToClose)
            {
                player.DeleteCommand(command.CommandId);
                //player.Commands.Remove(command);
            }
        }

        public void RequestAttack(Command command, PlayerUnit enemyUnit)
        {
            DispatcherRequestAttack dispatcherRequestAttack = new DispatcherRequestAttack();
            dispatcherRequestAttack.Command = command;
            dispatcherRequestAttack.EnemyUnit = enemyUnit;

            AttackThisUnits.Add(dispatcherRequestAttack);
        }

        public void RequestUnit(Command command, UnitType unitType, PlayerUnit favoriteUnit)
        {
            DispatcherRequestUnit dispatcherRequestUnit = new DispatcherRequestUnit();
            dispatcherRequestUnit.UnitType = unitType;
            dispatcherRequestUnit.Command = command;
            if (favoriteUnit != null)
                dispatcherRequestUnit.FavoriteUnits.Add(favoriteUnit);
            RequestedUnits.Add(dispatcherRequestUnit);
        }

        public void RequestUpgrade(Command command, PlayerUnit playerUnit)
        {
            RequestedUpgrades.Add(playerUnit);
        }

        public void ExtractStrayUnits(Player player, PlayerUnit extractorUnit, List<PlayerUnit> moveableUnits)
        {
            Dictionary<Position, TileWithDistance> possibleExtrationTiles = extractorUnit.Unit.Extractor.CollectExtractionTiles();
            if (possibleExtrationTiles.Count == 0)
                return;

            foreach (PlayerUnit playerUnit in moveableUnits)
            {
                if (playerUnit.Unit.IsComplete())
                {
                    if (playerUnit.Unit.ExtractMe)
                    {
                        // This is a unassigend container. Collect it!
                    }
                    else
                    {
                        continue;
                    }
                }
                if (player.IsAttached(playerUnit.Unit.UnitId, null))
                    continue;
                if (playerUnit.PossibleMoves.Count > 0)
                    continue;

                // Do not extract units under construction
                if (playerUnit.Unit.UnderConstruction)
                    continue;

                if (playerUnit.Unit.ExtractMe == false &&
                    playerUnit.Unit.Engine != null)
                {
                    playerUnit.Unit.ExtractUnit();
                    if (extractorUnit.Unit.Engine == null)
                    {
                        if (!possibleExtrationTiles.ContainsKey(playerUnit.Unit.Pos))
                        {
                            // Move unit to only extractor if the extractor has no engine.
                            MoveUnit(null, playerUnit, extractorUnit.Unit.Pos);

                            // Well, if it reaches the extractor...
                        }
                        else
                        {
                            // Is already next to an extractor. Wait for termination...
                        }
                    }
                    
                }
                else
                {
                    if (playerUnit.Unit.ExtractMe && playerUnit.Unit.Engine != null)
                    {
                        // Move unit to only extractor if the extractor has no engine.
                        if (extractorUnit.Unit.Engine == null)
                        {
                            if (!possibleExtrationTiles.ContainsKey(playerUnit.Unit.Pos))
                            {
                                // Move unit to extractor
                                MoveUnit(null, playerUnit, extractorUnit.Unit.Pos);
                            }
                        }
                    }
                    // Stuck unit cannot move away
                    playerUnit.Unit.ExtractUnit();
                }
            }
        }


        public static Command SearchForAssembler(List<Command> checkedCommands, Command parent)
        {
            if (!checkedCommands.Contains(parent))
            {
                checkedCommands.Add(parent);
                foreach (CommandSource commandSource in parent.CommandSources)
                {
                    foreach (PlayerUnit playerUnit in commandSource.Child.AssigendPlayerUnits)
                    {
                        if (playerUnit.Unit.Engine == null && playerUnit.Unit.Extractor != null && playerUnit.Unit.Extractor.CanExtract)
                        {
                            // Reassign this unit, but only if this unit is still assigend (avoid duplicates)
                            return commandSource.Child;
                        }
                    }
                    Command cmd = SearchForAssembler(checkedCommands, commandSource.Parent);
                    if (cmd != null)
                        return cmd;
                    cmd = SearchForAssembler(checkedCommands, commandSource.Child);
                    if (cmd != null)
                        return cmd;
                }
            }
            return null;
        }

        public bool CreateRequestedUnit(PlayerUnit assembler, DispatcherRequestUnit dispatcherRequestUnit, Command command)
        {
            List<Move> possibleAssemblemoves = new List<Move>();
            assembler.Unit.Assembler.ComputePossibleMoves(possibleAssemblemoves, null, MoveFilter.Assemble);

            // Make new unit.
            if (dispatcherRequestUnit.FavoriteUnits.Count == 0)
            {
                List<Position> preferedPosition = new List<Position>();

                if (dispatcherRequestUnit.UnitType.MinContainerLevel > 0)
                {
                    // Build only next to another container if possible
                    foreach (PlayerUnit possibleContainer in command.AssigendPlayerUnits)
                    {
                        if (possibleContainer.Unit.Container != null)
                        {
                            Tile t = GameController.GetTile(possibleContainer.Unit.Pos);
                            foreach (Tile n in t.Neighbors)
                            {
                                if (n.Unit == null)
                                    preferedPosition.Add(n.Pos);
                            }
                        }
                    }
                }

                if (preferedPosition.Count > 0)
                {
                    // Build only at prefered position
                    foreach (Move move in possibleAssemblemoves)
                    {
                        if (preferedPosition.Contains(move.Positions[1]))
                        {
                            if (Assembler.DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, null))
                            {
                                PlayerMove playerMove = new PlayerMove(move);
                                playerMove.Command = dispatcherRequestUnit.Command;
                                playerMove.NewUnitId = move.UnitId;

                                assembler.PossibleMoves.Add(playerMove);

                                return true;
                            }
                        }
                    }
                }
                else
                {

                    foreach (Move move in possibleAssemblemoves)
                    {
                        //if (IsOccupied(player, moves, move.Positions[move.Positions.Count - 1]))
                        //    continue;

                        if (Assembler.DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, null))
                        {
                            PlayerMove playerMove = new PlayerMove(move);
                            playerMove.Command = dispatcherRequestUnit.Command;
                            playerMove.NewUnitId = move.UnitId;

                            assembler.PossibleMoves.Add(playerMove);

                            return true;

                        }
                    }
                }
            }
            return false;
        }

        public void DispatchCreationRequests(Player player, List<DispatcherRequestUnit> remainingCreationRequests, List<PlayerUnit> moveableUnits)
        {
            List<DispatcherRequestUnit> requests = new List<DispatcherRequestUnit>();
            requests.AddRange(remainingCreationRequests);

            foreach (DispatcherRequestUnit dispatcherRequestUnit in requests)
            {
                // No upgrades
                if (dispatcherRequestUnit.FavoriteUnits.Count > 0)
                    continue;

                foreach (CommandSource commandSource in dispatcherRequestUnit.Command.CommandSources)
                {
                    // Do not process unit requests as long as the factory is building itself
                    if (commandSource.Parent.WaitingForBuilder || commandSource.Parent.WaitingForDeconstrcut)
                        continue;

                    PlayerUnit assembler = null;
                    foreach (PlayerUnit playerUnit in commandSource.Parent.AssigendPlayerUnits)
                    {
                        if (playerUnit.PossibleMoves.Count > 0)
                            continue;

                        if (playerUnit.Unit.Assembler != null && playerUnit.Unit.Assembler.CanProduce())
                        {
                            // Leave the assigend command to produce the unit
                            assembler = playerUnit;
                            break;
                        }
                    }
                    if (assembler != null)
                    {
                        if (CreateRequestedUnit(assembler, dispatcherRequestUnit, commandSource.Parent))
                        {
                            remainingCreationRequests.Remove(dispatcherRequestUnit);
                            break;
                        }
                    }
                }
            }

            // Handle all unit requests that could not be satiesfied by the parent in the command source
            requests.Clear();
            requests.AddRange(remainingCreationRequests);

            foreach (DispatcherRequestUnit dispatcherRequestUnit in requests)
            {
                // No upgrades
                if (dispatcherRequestUnit.FavoriteUnits.Count > 0)
                    continue;

                PlayerUnit assembler = null;
                foreach (PlayerUnit playerUnit in moveableUnits)
                {
                    if (playerUnit.PossibleMoves.Count > 0)
                        continue;

                    if (playerUnit.Unit.Assembler != null && playerUnit.Unit.Assembler.CanProduce())
                    {
                        // Leave the assigend command to produce the unit
                        assembler = playerUnit;
                        break;
                    }
                }
                if (assembler != null)
                {
                    // Find the commad of this assembler
                    foreach (Command command in player.Commands)
                    {
                        // Do not process unit requests as long as the factory is building itself
                        if (command.WaitingForBuilder || command.WaitingForDeconstrcut)
                            continue;

                        if (command.AssigendPlayerUnits.Contains(assembler))
                        {
                            if (CreateRequestedUnit(assembler, dispatcherRequestUnit, command))
                            {
                                remainingCreationRequests.Remove(dispatcherRequestUnit);
                                break;
                            }
                        }
                    }
                }
            }
        }

            // Handle unit requests, that cannot not be satisfied in the own command area
        public void HandleUnitRequests(Player player, List<PlayerUnit> moveableUnits)
        {
            List<DispatcherRequestUnit> remainingUpgradeRequests = new List<DispatcherRequestUnit>();
            List<DispatcherRequestUnit> remainingCreationRequests = new List<DispatcherRequestUnit>();
            foreach (DispatcherRequestUnit dispatcherRequest in RequestedUnits)
            {
                if (dispatcherRequest.UnitType.MinEngineLevel == 0)
                {
                    // Does not make sense to produce a unit that cannot move to the destination
                    continue;
                }

                if (dispatcherRequest.FavoriteUnits != null &&
                    dispatcherRequest.FavoriteUnits.Count > 0)
                {
                    remainingUpgradeRequests.Add(dispatcherRequest);
                }
                else
                {
                    remainingCreationRequests.Add(dispatcherRequest);
                }
            }

            // Handle requests randomly but upgrade first and create only if no upgrades needed to
            // prevent that all output positions are locked by unfinished units.

            // Step 1: Upgrade
            while (remainingUpgradeRequests.Count > 0)
            {
                int idx = GameController.Random.Next(remainingUpgradeRequests.Count);
                DispatcherRequestUnit dispatcherRequestUnit = remainingUpgradeRequests[idx];

                bool handled = false;

                foreach (Command command in player.Commands)
                {
                    PlayerUnit playerUnit = dispatcherRequestUnit.FavoriteUnits[0];
                    if (command.UnitsAlreadyInArea != null &&
                        command.UnitsAlreadyInArea.Contains(playerUnit))
                    {
                        //foreach (CommandSource commandSource in command.CommandSources)
                        {
                            foreach (PlayerUnit assembler in command.AssigendPlayerUnits)
                            {
                                if (assembler.PossibleMoves.Count > 0)
                                    continue;

                                if (assembler.Unit.Assembler != null)
                                {
                                    if (moveableUnits.Contains(playerUnit) && assembler.Unit.Assembler.CanProduce())
                                    {
                                        List<Move> possibleUpgrademoves = new List<Move>();
                                        assembler.Unit.Assembler.ComputePossibleMoves(possibleUpgrademoves, null, MoveFilter.Upgrade);

                                        foreach (Move move in possibleUpgrademoves)
                                        {
                                            if (Assembler.DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, playerUnit.Unit))
                                            {
                                                // Take first option or random but dicide here
                                                assembler.PossibleMoves.Add(new PlayerMove(move));
                                                handled = true;
                                                break;
                                            }
                                            //Tile tile = playerUnit.Unit.Owner.Game.Map.GetTile(move.Positions[1]);
                                            //if (player.IsAttached(tile.Unit.UnitId, command))
                                            //    continue;
                                            /*
                                            if (assembler.Unit.Assembler.HandleRequestUnit(playerUnit, dispatcherRequestUnit, move))
                                        {
                                            handled = true;
                                            break;
                                        }*/
                                        }
                                    }
                                }
                                if (handled)
                                    break;
                            }
                            if (handled)
                                break;
                            //}
                            //if (handled)
                            //    break;
                        }
                    }
                }
                remainingUpgradeRequests.Remove(dispatcherRequestUnit);
            }

            // 
            if (remainingCreationRequests.Count > 0)
            {
                // Dispatch the remaining requests evenly across all assemblers
                DispatchCreationRequests(player, remainingCreationRequests, moveableUnits);
            }
            

            // Step 2: Create
            /*
            while (remainingCreationRequests.Count > 0)
            {
                int idx = GameController.Random.Next(remainingCreationRequests.Count);
                DispatcherRequestUnit dispatcherRequestUnit = remainingCreationRequests[idx];

                bool handled = false;

                foreach (CommandSource commandSource in dispatcherRequestUnit.Command.CommandSources)
                {
                    foreach (PlayerUnit playerUnit in commandSource.Parent.UnitsAlreadyInArea)
                    {
                        if (playerUnit.PossibleMoves.Count > 0)
                            continue;

                        Unit cntrlUnit = playerUnit.Unit;
                        if (cntrlUnit.Assembler != null)
                        {
                            if (moveableUnits.Contains(playerUnit) && cntrlUnit.Assembler.CanProduce)
                            {
                                List<Move> possibleAssemblemoves = new List<Move>();
                                cntrlUnit.Assembler.ComputePossibleMoves(possibleAssemblemoves, null, MoveFilter.Assemble);

                                // Make new unit.
                                if (dispatcherRequestUnit.FavoriteUnits.Count == 0)
                                {
                                    List<Position> preferedPosition = new List<Position>();

                                    if (dispatcherRequestUnit.UnitType.MinContainerLevel > 0)
                                    {
                                        // Build only next to another container if possible
                                        foreach (PlayerUnit possibleContainer in commandSource.Parent.AssigendPlayerUnits)
                                        {
                                            if (possibleContainer.Unit.Container != null)
                                            {
                                                Tile t = GameController.GetTile(possibleContainer.Unit.Pos);
                                                foreach (Tile n in t.Neighbors)
                                                {
                                                    if (n.Unit == null)
                                                        preferedPosition.Add(n.Pos);
                                                }
                                            }
                                        }
                                    }

                                    if (preferedPosition.Count > 0)
                                    {
                                        // Build only at prefered position
                                        foreach (Move move in possibleAssemblemoves)
                                        {
                                            if (preferedPosition.Contains(move.Positions[1]))
                                            {
                                                if (Assembler.DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, null))
                                                {
                                                    PlayerMove playerMove = new PlayerMove(move);
                                                    playerMove.Command = dispatcherRequestUnit.Command;
                                                    playerMove.NewUnitId = move.UnitId;

                                                    playerUnit.PossibleMoves.Add(playerMove);
                                                    handled = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {

                                        foreach (Move move in possibleAssemblemoves)
                                        {
                                            //if (IsOccupied(player, moves, move.Positions[move.Positions.Count - 1]))
                                            //    continue;

                                            if (Assembler.DoesMoveMinRequest(move, dispatcherRequestUnit.UnitType, null))
                                            {
                                                PlayerMove playerMove = new PlayerMove(move);
                                                playerMove.Command = dispatcherRequestUnit.Command;
                                                playerMove.NewUnitId = move.UnitId;

                                                playerUnit.PossibleMoves.Add(playerMove);
                                                handled = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (handled)
                            break;
                    }
                    if (handled)
                        break;
                }
                remainingCreationRequests.Remove(dispatcherRequestUnit);
            }*/
        }

        public void MoveStrayCompleteUnits(Player player, List<PlayerUnit> moveableUnits)
        {
            List<DispatcherRequestUnit> handledRequests = new List<DispatcherRequestUnit>();

            foreach (DispatcherRequestUnit dispatcherRequestUnit in RequestedUnits)
            {
                // Do not assign a stray unit if the command has a assigned unit
                if (dispatcherRequestUnit.FavoriteUnits.Count > 0)
                    continue;

                List<PossiblePath> possiblePaths = new List<PossiblePath>();

                foreach (PlayerUnit playerUnit in moveableUnits)
                {
                    if (!playerUnit.Unit.IsComplete())
                        continue;
                    if (player.IsAttached(playerUnit.Unit.UnitId, null))
                        continue;
                    if (playerUnit.Unit.Engine == null)
                        continue;
                    if (playerUnit.PossibleMoves.Count > 0)
                        continue;
                    if (playerUnit.Unit.ExtractMe)
                        // Will be collected by the extractor
                        continue;

                    if (dispatcherRequestUnit.UnitType.Matches(playerUnit))
                    {
                        List<Position> path = GameController.FindPath(playerUnit.Unit.Pos, dispatcherRequestUnit.Command.Center, playerUnit.Unit);
                        if (path != null && path.Count > 1)
                        {
                            PossiblePath possiblePath = new PossiblePath();
                            possiblePath.Path = path;
                            possiblePath.PlayerUnit = playerUnit;
                            possiblePaths.Add(possiblePath);
                        }
                        else
                        {
                            // Not reachable? Ignore request

                        }  
                    }
                }

                int shortestPath = 999;
                PossiblePath selectedPath = null;
                foreach (PossiblePath possiblePath in possiblePaths)
                {
                    if (possiblePath.Path.Count < shortestPath)
                    {
                        selectedPath = possiblePath;
                        shortestPath = possiblePath.Path.Count;
                    }
                }
                if (selectedPath != null)
                {
                    if (selectedPath.PlayerUnit.Unit.ExtractMe)
                    {
                        // Rescued!
                        selectedPath.PlayerUnit.Unit.ExtractUnit();
                    }
                    dispatcherRequestUnit.Command.AssignUnit(selectedPath.PlayerUnit.Unit.UnitId);
                    MoveUnit(dispatcherRequestUnit.Command, selectedPath.PlayerUnit, selectedPath.Path[selectedPath.Path.Count - 1]);
                    moveableUnits.Remove(selectedPath.PlayerUnit);
                    handledRequests.Add(dispatcherRequestUnit);
                    break;
                }
            }

            foreach (DispatcherRequestUnit unit in handledRequests)
            {
                RequestedUnits.Remove(unit);
            }
        }

        public void MoveUnit(Command command, PlayerUnit playerUnit, Position pos)
        {
            // Check
#if DEBUG
            foreach (DispatcherRequestMove requestMove in MoveThisUnits)
            {
                if (requestMove.PlayerUnit.Unit.UnitId == playerUnit.Unit.UnitId)
                {
                    //throw new Exception("Doublemove");
                }
            }
#endif
            DispatcherRequestMove dispatcherRequestMove = new DispatcherRequestMove();

            dispatcherRequestMove.Command = command;
            dispatcherRequestMove.PlayerUnit = playerUnit;
            dispatcherRequestMove.Pos = pos;

            MoveThisUnits.Add(dispatcherRequestMove);
        }
    }

    public class PossiblePath
    {
        public List<Position> Path { get; set; }
        public PlayerUnit PlayerUnit { get; set; }
    }

}
