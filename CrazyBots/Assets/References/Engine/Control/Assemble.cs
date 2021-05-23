using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class Assemble : Command
    {
        public Assemble()
        {
            Range = 1;

            RequestBuilderUnit();
            /*
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
            */
        }

        public override void DemandStartupUnits()
        {
            // Assembler
            UnitType unitType = new UnitType();
            unitType.MinAssemblerLevel = 3;
            unitType.MaxAssemblerLevel = 3;
            unitType.MinExtractorLevel = 1;
            unitType.MaxExtractorLevel = 1;
            DemandedUnitTypes.Add(unitType);

            // Container
            unitType = new UnitType();
            unitType.MinContainerLevel = 3;
            unitType.MaxContainerLevel = 3;
            unitType.MinExtractorLevel = 1;
            unitType.MaxExtractorLevel = 1;
            DemandedUnitTypes.Add(unitType);

            // Reactor
            unitType = new UnitType();
            unitType.MinRadarLevel = 1;
            unitType.MaxRadarLevel = 2;
            unitType.MinReactorLevel = 1;
            unitType.MaxReactorLevel = 2;
            DemandedUnitTypes.Add(unitType);
        }

        public override void AttachUnits(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            base.AttachUnits(dispatcher, player, moveableUnits);
            if (WaitingForBuilder)
            {
                WaitForBuilder(dispatcher, moveableUnits);
            }
            else if (WaitingForDeconstrcut)
            {
                Deconstrcut(dispatcher, player, moveableUnits);
            }
            else
            {
                SanityCheck(dispatcher, player, moveableUnits);
                
                HandlyUnitsToExtract(dispatcher, player, moveableUnits);
                
            }
#if old
            UnitsAlreadyInArea = CollectUnitsAlreadyInArea(player, Range);

            currentDemandedUnitTypes = new List<UnitType>();
            currentDemandedUnitTypes.AddRange(DemandedUnitTypes);

            List<string> deadUnits = new List<string>();

            foreach (string unitId in AssignedUnits)
            {
                bool playerUnitFound = false;

                foreach (PlayerUnit playerUnit in player.Units.Values)
                {
                    if (playerUnit.Unit.UnitId == unitId)
                    {
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
                                }
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
                                                int x = 0;
                                            }
                                            else
                                            {
                                                // called when new factory 
                                                // Units are produced
                                                dispatcher.RequestUnit(this, unitType, playerUnit);
                                                currentDemandedUnitTypes.Remove(unitType);
                                            }
                                        }
                                        else
                                        {
                                            // Not called

                                            if (UnitsAlreadyInArea.Contains(playerUnit))
                                            {
                                                // Discard connection to demaged unit
                                                deadUnits.Add(playerUnit.Unit.UnitId);
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
                // called when new factory 
                List<UnitType> remainingUnitTypes = new List<UnitType>();
                remainingUnitTypes.AddRange(currentDemandedUnitTypes);

                foreach (PlayerUnit playerUnit in UnitsAlreadyInArea)
                {
                    if (!moveableUnits.Contains(playerUnit))
                        continue;

                    bool unitMatches = false;
                    foreach (UnitType unitType in currentDemandedUnitTypes)
                    {
                        if (unitType.Matches(playerUnit))
                        {
                            remainingUnitTypes.Remove(unitType);
                            unitMatches = true;
                            break;
                        }
                    }
                    if (!unitMatches)
                    {
                        // Unmatching unit in area. Do not request another unit, wait for clear
                        remainingUnitTypes.Clear();
                    }
                }
                foreach (UnitType unitType in remainingUnitTypes)
                {
                    currentDemandedUnitTypes.Remove(unitType);
                    dispatcher.RequestUnit(this, unitType, null);
                }
            }
           
            string builderUnitId = null;
            if (WaitingForBuilder)
            {
                foreach (PlayerUnit playerUnit in UnitsAlreadyInArea)
                {
                    if (playerUnit.PossibleMoves.Count > 0)
                        continue;

                    if (playerUnit.Unit.Pos == Center)
                    {
                        if (DemandedUnitTypes[0].Fits(playerUnit))
                        {
                            // Builder arrived. Build factory
                            builderUnitId = playerUnit.Unit.UnitId;
                            WaitingForBuilder = false;
                            DemandedUnitTypes.Clear();

                            DemandStartupUnits();
                            currentDemandedUnitTypes.Clear();
                            currentDemandedUnitTypes.AddRange(DemandedUnitTypes);
                            break;
                        }
                    }
                }
            }
            if (currentDemandedUnitTypes.Count == DemandedUnitTypes.Count)
            {
                // Nothing fits Starting condition?
                foreach (PlayerUnit playerUnit in UnitsAlreadyInArea)
                {
                    if (playerUnit.PossibleMoves.Count > 0)
                        continue;

                    if (builderUnitId == playerUnit.Unit.UnitId &&
                        playerUnit.Unit.Engine != null)
                    {
                        // Builder unit
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
                        break;
                    }
                    if (playerUnit.Unit.Assembler != null &&
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
                        break;
                    }

                    if (playerUnit.PossibleMoves.Count == 0 &&
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
                        break;
                    }
                    if (playerUnit.PossibleMoves.Count > 0)
                        break;
                }
            }

            PlayerUnit assemblerUnit = null;

            // units are missing or demaged, try to recover
            if (currentDemandedUnitTypes.Count > 0)
            {
                // Find a unit to produce
                foreach (PlayerUnit unit in UnitsAlreadyInArea)
                {
                    if (unit.Unit.Assembler != null && unit.Unit.Assembler.CanProduce)
                    {
                        // produce missing part
                        assemblerUnit = unit;
                        break;
                    }
                }
            }

            // Step 1: Doing Upgrades (used when building itself)
            if (assemblerUnit != null && assemblerUnit.PossibleMoves.Count == 0)
            {
                List<Move> possiblemoves = new List<Move>();
                assemblerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);

                // check what is missing
                bool found = false;
                foreach (UnitType unitType in currentDemandedUnitTypes)
                {
                    foreach (PlayerUnit playerUnit in UnitsAlreadyInArea)
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
                    if (found) break;
                }
            }

            // Step 2: Create new units
            if (assemblerUnit != null && assemblerUnit.PossibleMoves.Count == 0)
            {
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

            // Keep request
            if (assemblerUnit != null && assemblerUnit.PossibleMoves.Count == 0)
            {
                // not called

                // Still remaining demands?
                foreach (UnitType unitType in currentDemandedUnitTypes)
                {
                    if (unitType.MinRadarLevel >= 1)
                        dispatcher.RequestUnit(this, unitType, null);
                }
            }
#endif
        }

        public override string ToString()
        {
            return "Assemble";
        }

    }
}
