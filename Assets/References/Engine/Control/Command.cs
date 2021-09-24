using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class CommandGroup
    {
        public CommandGroup()
        {
            Commands = new List<Command>();
        }
        public List<Command> Commands { get; private set; }
        public bool SingleCommand { get; set; }
    }

    public class CommandSource
    {
        public Command Child { get; set; }
        public Command Parent { get; set; }
        public List<Position> Path { get; set; }
    }
    public class Command
    {
        // Command waits for a build unit to arrive
        public bool WaitingForBuilder { get; set; }

        // Command deconstructs the builder unit into requested parts
        public bool WaitingForDeconstrcut { get; set; }

        public int StuckCounter { get; set; }

        private static int cntr;
        public static int GroupCntr { get; set; }
        internal Command ()
        {
            cntr++;
            CommandId = "cmd" + cntr.ToString();
        }

        public List<UnitType> DemandedUnitTypes = new List<UnitType>();

        public Position Center { get; set; }
        public int Range { get; set; }
        public string CommandId { get;  set; }
        public string GroupId { get;  set; }
        public int PlayerId { get; set; }

        public bool UnitReachedCommandDoNotFollowPath { get; set; }
        public Map Map { get; set; }

        public List<string> AssignedUnits = new List<string>();

        public List<CommandSource> CommandSources = new List<CommandSource>();

        public void AssignUnit(string unitId)
        {
            if (AssignedUnits.Count > 0)
            {
                if (AssignedUnits.Count == 1 && AssignedUnits[0] == unitId)
                {
                    // Assigen again... needed?
                }
                else
                {
                    //int x = 0;
                }
            }

            if (AssignedUnits.Contains(unitId))
            {
                
            }
            else
            {
                AssignedUnits.Add(unitId);
            }
        }

        public void RequestBuilderUnit()
        {
            // Moving Assembler
            UnitType unitType = new UnitType();
            unitType.MinEngineLevel = 1;
            unitType.MaxEngineLevel = 1;

            unitType.MinAssemblerLevel = 1;
            unitType.MaxAssemblerLevel = 1;

            unitType.MinExtractorLevel = 1;
            unitType.MaxExtractorLevel = 1;

            unitType.MinContainerLevel = 1;
            unitType.MaxContainerLevel = 1;

            DemandedUnitTypes.Add(unitType);
            WaitingForBuilder = true;
        }

        public virtual void DemandStartupUnits()
        {

        }

        public void WaitForBuilder(Dispatcher dispatcher, List<PlayerUnit> moveableUnits)
        {
            if (!WaitingForBuilder) return;

            if (AssignedUnits.Count == 0) return;

            foreach (PlayerUnit playerUnit in moveableUnits)
            {
                if (playerUnit.PossibleMoves.Count > 0)
                    continue;
                if (!playerUnit.Unit.IsComplete())
                    continue;

                if (playerUnit.Unit.UnitId == AssignedUnits[0])
                {
                    if (playerUnit.Unit.Pos == Center)
                    {
                        if (DemandedUnitTypes[0].Fits(playerUnit))
                        {
                            // Builder arrived. Build factory
                            WaitingForBuilder = false;
                            WaitingForDeconstrcut = true;

                            DemandedUnitTypes.Clear();
                            DemandStartupUnits();
                            break;
                        }
                    }

                    else
                    {
                        // Wait for some metal before moving away
                        if (playerUnit.Unit.Container != null && playerUnit.Unit.Container.TileContainer.Minerals >= 16)
                        {
                            // Move the unit here
                            dispatcher.MoveUnit(this, playerUnit, Center);
                        }
                        else
                        {
                            playerUnit.Unit.BuilderWaitForMetal = true;
                        }
                    }
                }
            }            
        }


        public void Deconstrcut(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            if (!WaitingForDeconstrcut) return;

            if (AssignedUnits.Count == 0) return;

            foreach (PlayerUnit playerUnit in moveableUnits)
            {
                if (playerUnit.PossibleMoves.Count > 0)
                    continue;

                if (playerUnit.Unit.UnitId == AssignedUnits[0] && playerUnit.Unit.Pos == Center)
                {
                    if (playerUnit.Unit.Engine != null)
                    {
                        // Builder unit, remove engine first
                        List<Move> possiblemoves = new List<Move>();
                        playerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                        if (possiblemoves.Count > 0)
                        {
                            // possiblemoves contains possible output places
                            foreach (Move possibleMove in possiblemoves)
                            {
                                if (possibleMove.UnitId == "Engine")
                                {
                                    PlayerMove playerMove = new PlayerMove(possibleMove);
                                    playerMove.NewUnitId = "RemoveEngine";
                                    playerUnit.PossibleMoves.Add(playerMove);
                                    break;
                                }
                            }
                        }
                    }
                    else if (playerUnit.Unit.Assembler != null &&
                        playerUnit.Unit.Extractor != null &&
                        playerUnit.Unit.Reactor != null)
                    {
                        List<Move> possiblemoves = new List<Move>();
                        playerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                        if (possiblemoves.Count > 0)
                        {
                            // possiblemoves contains possible output places
                            foreach (Move possibleMove in possiblemoves)
                            {
                                if (possibleMove.UnitId == "Reactor")
                                {
                                    PlayerMove playerMove = new PlayerMove(possibleMove);
                                    playerMove.Command = this;
                                    playerMove.NewUnitId = "RemoveReactor";
                                    playerUnit.PossibleMoves.Add(playerMove);
                                    break;
                                }
                            }
                        }
                    }

                    else if (playerUnit.PossibleMoves.Count == 0 &&
                        playerUnit.Unit.Assembler != null &&
                        playerUnit.Unit.Extractor != null &&
                        playerUnit.Unit.Container != null)
                    {
                        List<Move> possiblemoves = new List<Move>();
                        playerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                        if (possiblemoves.Count > 0)
                        {
                            // possiblemoves contains possible output places
                            foreach (Move possibleMove in possiblemoves)
                            {
                                if (possibleMove.UnitId == "Container")
                                {
                                    PlayerMove playerMove = new PlayerMove(possibleMove);
                                    playerMove.Command = this;
                                    playerMove.NewUnitId = "RemoveContainer";
                                    playerUnit.PossibleMoves.Add(playerMove);
                                    break;
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        // Deconstruction finished
                        WaitingForDeconstrcut = false;
                    }

                    break;
                }
            }

        }

        public List<PlayerUnit> AssigendPlayerUnits = new List<PlayerUnit>();

        public void HandlyUnitsToExtract(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            List<PlayerUnit> unitsToExtract = new List<PlayerUnit>();
            foreach (PlayerUnit playerUnit in AssigendPlayerUnits)
            {
                if (playerUnit.Unit.ExtractMe)
                {
                    unitsToExtract.Add(playerUnit);
                }
            }

            List<DispatcherRequestExtract> availableExtractors = new List<DispatcherRequestExtract>();

            if (unitsToExtract.Count > 0)
            {
                // Find all extractors and do not move units that are in extraction range
                foreach (PlayerUnit playerUnit in AssigendPlayerUnits)
                {
                    if (playerUnit.Unit.Engine == null && playerUnit.Unit.Extractor != null && playerUnit.Unit.Extractor.CanExtract)
                    {
                        DispatcherRequestExtract dispatcherRequestExtract = new DispatcherRequestExtract();

                        dispatcherRequestExtract.Extractor = playerUnit;
                        dispatcherRequestExtract.PossibleExtrationTiles = playerUnit.Unit.Extractor.CollectExtractionTiles();
                        if (dispatcherRequestExtract.PossibleExtrationTiles.Count == 0)
                            return;

                        availableExtractors.Add(dispatcherRequestExtract);

                        foreach (PlayerUnit unitToExtract in unitsToExtract)
                        {
                            if (dispatcherRequestExtract.PossibleExtrationTiles.ContainsKey(unitToExtract.Unit.Pos))
                            {
                                // Should be extracted automatically, do not move this units.
                                unitsToExtract.Remove(unitToExtract);
                                break;
                            }
                        }
                    }
                }
            }

            // Units that are not in range
            if (unitsToExtract.Count > 0)
            {
                foreach (DispatcherRequestExtract dispatcherRequestExtract in availableExtractors)
                {
                    foreach (PlayerUnit playerUnit in unitsToExtract)
                    {
                        // Select closest extractor?
                        if (playerUnit.Unit.Engine != null)
                        {

                            // Using first for now
                            dispatcher.MoveUnit(this, playerUnit, dispatcherRequestExtract.Extractor.Unit.Pos);

                            // Move every unit only once
                            unitsToExtract.Remove(playerUnit);
                            break;
                        }
                    }
                }
            }

            // Still extractable units but no extractor.
            if (unitsToExtract.Count > 0)
            {
                List<Command> checkedCommands = new List<Command>();
                Command cmd = SearchForExtractor(checkedCommands, this);

                if (cmd != null)
                {
                    foreach (PlayerUnit unitToExtract in unitsToExtract)
                    {
                        foreach (PlayerUnit playerUnit in cmd.AssigendPlayerUnits)
                        {
                            if (playerUnit.Unit.Engine == null && playerUnit.Unit.Extractor != null && playerUnit.Unit.Extractor.CanExtract)
                            {
                                // Reassign this unit, but only if this unit is still assigend (avoid duplicates)
                                if (AssignedUnits.Contains(unitToExtract.Unit.UnitId))
                                {
                                    AssignedUnits.Remove(unitToExtract.Unit.UnitId);
                                    cmd.AssignUnit(unitToExtract.Unit.UnitId);
                                }
                                break;
                            }
                        }
                    }
                }
                /*
                foreach (PlayerUnit unitToExtract in unitsToExtract)
                {
                    foreach (CommandSource commandSource in CommandSources)
                    {
                        foreach (PlayerUnit playerUnit in commandSource.Child.AssigendPlayerUnits)
                        {
                            if (playerUnit.Unit.Engine == null && playerUnit.Unit.Extractor != null && playerUnit.Unit.Extractor.CanExtract)
                            {
                                // Reassign this unit, but only if this unit is still assigend (avoid duplicates)
                                if (AssignedUnits.Contains(unitToExtract.Unit.UnitId))
                                {
                                    AssignedUnits.Remove(unitToExtract.Unit.UnitId);
                                    commandSource.Child.AssignUnit(unitToExtract.Unit.UnitId);
                                }
                                break;
                            }
                        }
                    }
                }*/
            }
        }

        public static Command SearchForExtractor(List<Command> checkedCommands, Command parent)
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
                    Command cmd = SearchForExtractor(checkedCommands, commandSource.Parent);
                    if (cmd != null)
                        return cmd;
                    cmd = SearchForExtractor(checkedCommands, commandSource.Child);
                    if (cmd != null)
                        return cmd;
                }
            }
            return null;
        }

        // Check if the units in this command are healty. If not, fix them
        public bool SanityCheck(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            PlayerUnit assemblerUnit = null;

            bool mustFixUnits = false;

            // units are missing or demaged, try to recover
            foreach (PlayerUnit playerUnit in AssigendPlayerUnits)
            {
                if (!playerUnit.Unit.IsComplete())
                {
                    mustFixUnits = true;
                }
            }

            // Find a unit to produce
            foreach (PlayerUnit unit in UnitsAlreadyInArea)
            {
                if (unit.Unit.Assembler != null && unit.Unit.Assembler.CanProduce())
                {
                    // produce missing part
                    assemblerUnit = unit;
                    break;
                }
            }
            if (assemblerUnit == null)
            {
                // No unit here to assemble missing parts. Nothing to do.
                return false;
            }

            // Remove unittypes that are already present and healty
            currentDemandedUnitTypes = new List<UnitType>();
            currentDemandedUnitTypes.AddRange(DemandedUnitTypes);

            foreach (PlayerUnit playerUnit in AssigendPlayerUnits)
            {
                foreach (UnitType unitType in currentDemandedUnitTypes)
                {
                    if (playerUnit.Unit.IsComplete())
                    {
                        if (unitType.Fits(playerUnit))
                        {
                            currentDemandedUnitTypes.Remove(unitType);
                            break;
                        }
                    }
                }
            }

            if (mustFixUnits)
            {
                // Upgrade units
                List<Move> possiblemoves = new List<Move>();
                assemblerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);

                // check what is missing
                bool found = false;
                foreach (UnitType unitType in currentDemandedUnitTypes)
                {
                    foreach (PlayerUnit playerUnit in AssigendPlayerUnits)
                    {
                        if (!playerUnit.Unit.IsComplete())
                        {
                            if (unitType.Fits(playerUnit))
                            {
                                foreach (Move move in possiblemoves)
                                {
                                    if (move.Positions[1] != playerUnit.Unit.Pos)
                                        continue;

                                    // Will this move upgrade the unit so it fits into the unittype wanted
                                    if (Assembler.DoesMoveMinRequest(move, unitType, playerUnit.Unit))
                                    {
                                        assemblerUnit.PossibleMoves.Add(new PlayerMove(move));
                                        currentDemandedUnitTypes.Remove(unitType);
                                        found = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (found) break;
                }
            }
            else if (currentDemandedUnitTypes.Count > 0)
            { 
                // Step 2: Create new units
                // not called
                List<Move> possibleAssemblemoves = new List<Move>();
                assemblerUnit.Unit.Assembler.ComputePossibleMoves(possibleAssemblemoves, null, MoveFilter.Assemble);

                // check what is missing
                bool found = false;
                foreach (UnitType unitType in currentDemandedUnitTypes)
                {
                    foreach (Move move in possibleAssemblemoves)
                    {
                        //if (IsOccupied(player, moves, move.Positions[move.Positions.Count - 1]))
                        //    continue;

                        if (Assembler.DoesMoveMinRequest(move, unitType, null))
                        {
                            PlayerMove playerMove = new PlayerMove(move);
                            playerMove.Command = this;
                            playerMove.NewUnitId = move.UnitId;

                            assemblerUnit.PossibleMoves.Add(playerMove);
                            currentDemandedUnitTypes.Remove(unitType);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }


            return true;
        }

        public Dictionary<Position, TileWithDistance> CollectIncludedPositions()
        {
            // Filter?
            return Map.EnumerateTiles(Center, Range);
        }

        public Dictionary<Position, TileWithDistance> CollectIncludedPositions(Position pos, int range)
        {
            // Filter?
            return Map.EnumerateTiles(pos, range);
        }

        protected List<PlayerUnit> EnemyUnits = new List<PlayerUnit>();
        public Dictionary<Position, TileWithDistance> PosititionsInArea;

        public List<PlayerUnit> CollectUnitsAlreadyInArea(Player player, int range)
        {
            EnemyUnits.Clear();
            List<PlayerUnit> unitsAlreadyInArea = new List<PlayerUnit>();
            /*
            PosititionsInArea = Map.EnumerateTiles(Center, Range);

            foreach (TileWithDistance t in PosititionsInArea.Values)
            {
                if (player.Units.ContainsKey(t.Pos))
                {
                    PlayerUnit unit = player.Units[t.Pos];
                    if (unit.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                    {
                        unitsAlreadyInArea.Add(unit);
                    }
                    else
                    {
                        // Enemy or neutral unit
                        EnemyUnits.Add(unit);
                    }
                }
            }*/
            return unitsAlreadyInArea;
        }

        public List<PlayerUnit> AttachedUnits = new List<PlayerUnit>();

        public virtual bool CanBeClosed()
        {
            return false;
        }

        protected List<UnitType> currentDemandedUnitTypes;
        public List<PlayerUnit> UnitsAlreadyInArea { get; protected set; }

        public virtual void AttachUnits(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            UnitsAlreadyInArea = CollectUnitsAlreadyInArea(player, Range);

            currentDemandedUnitTypes = new List<UnitType>();
            currentDemandedUnitTypes.AddRange(DemandedUnitTypes);

            List<string> deadUnits = new List<string>();

            AssigendPlayerUnits.Clear();

            foreach (string unitId in AssignedUnits)
            {
                bool playerUnitFound = false;

                foreach (PlayerUnit playerUnit in player.Units.Values)
                {
                    if (playerUnit.Unit.UnitId == unitId)
                    {
                        AssigendPlayerUnits.Add(playerUnit);

                        if (!moveableUnits.Contains(playerUnit))
                        {
                            // Attached unit found, but is busy doing something else
                            // Keep unit, no request
                            foreach (UnitType unitType in DemandedUnitTypes)
                            {
                                if (unitType.Matches(playerUnit))
                                {
                                    currentDemandedUnitTypes.Remove(unitType);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (playerUnit.Unit.IsComplete())
                            {
                                // Move that punk here or keep it
                                /* Different in collect and perhaps scoout, fits attack
                                foreach (UnitType unitType in DemandedUnitTypes)
                                {
                                    if (unitType.Matches(playerUnit))
                                    {
                                        //dispatcher.ClaimUnit(this, playerUnit, RequestType.Attack);
                                        currentDemandedUnitTypes.Remove(unitType);
                                        if (playerUnit.Unit.Pos != Center && playerUnit.Unit.Engine != null)
                                        {
                                            dispatcher.MoveUnit(this, playerUnit, Center);
                                        }
                                        break;
                                    }
                                }*/
                            }
                            else
                            {
                                // Wait for completion
                                foreach (UnitType unitType in DemandedUnitTypes)
                                {
                                    if (unitType.Fits(playerUnit))
                                    {
                                        if (this is Assemble)
                                        {
                                            if (UnitsAlreadyInArea.Contains(playerUnit))
                                            {
                                                // Units are produced
                                            }
                                            else
                                            {
                                                // Units are produced
                                                dispatcher.RequestUnit(this, unitType, playerUnit);
                                                currentDemandedUnitTypes.Remove(unitType);
                                            }
                                        }
                                        else
                                        {
                                            if (UnitsAlreadyInArea.Contains(playerUnit))
                                            {
                                                // Discard connection to damaged unit
                                                //deadUnits.Add(playerUnit.Unit.UnitId);

                                                // Units under production in the same area are detached before finished. Instead, request update
                                                dispatcher.RequestUnit(this, unitType, playerUnit);
                                            }
                                            else
                                            {
                                                // Unit in production
                                                // Request the unit again, so the factorys will produce it, but add this unit as the favourite
                                                // so only this unit will be supplied and not a new unit
                                                dispatcher.RequestUnit(this, unitType, playerUnit);

                                                // It not attached, the extractor will eat this unit in the factory...
                                                //deadUnits.Add(playerUnit.Unit.UnitId);
                                            }
                                            currentDemandedUnitTypes.Remove(unitType);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        playerUnitFound = true;
                        break;
                    }
                }
                if (!playerUnitFound)
                {
                    // Unit no longer exists
                    deadUnits.Add(unitId);
                }
            }

            foreach (string deadUnitId in deadUnits)
            {
                AssignedUnits.Remove(deadUnitId);
            }

            if (currentDemandedUnitTypes.Count > 0 && AssignedUnits.Count == 0)
            {
                List<UnitType> remainingUnitTypes = new List<UnitType>();
                remainingUnitTypes.AddRange(currentDemandedUnitTypes);

                foreach (PlayerUnit playerUnit in UnitsAlreadyInArea)
                {
                    if (!moveableUnits.Contains(playerUnit))
                        continue;
                    // Do not reattach units marked for extraction
                    if (playerUnit.Unit.ExtractMe)
                        continue;

                    bool unitMatches = false;
                    foreach (UnitType unitType in currentDemandedUnitTypes)
                    {
                        if (unitType.Matches(playerUnit))
                        {
                            remainingUnitTypes.Remove(unitType);
                            if (AssignedUnits.Count == 0)
                                AssignedUnits.Add(playerUnit.Unit.UnitId);
                            unitMatches = true;
                            break;
                        }
                    }
                    if (!unitMatches)
                    {
                        // Unmatching unit in area. Do not request another unit, wait for clear

                        // The Assembler itself is unmatched in Collect. Does it make sense?
                        //remainingUnitTypes.Clear();
                    }
                }
                foreach (UnitType unitType in remainingUnitTypes)
                {
                    currentDemandedUnitTypes.Remove(unitType);
                    dispatcher.RequestUnit(this, unitType, null);
                }
            }
        }
    
    }
}
