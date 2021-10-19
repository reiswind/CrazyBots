using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class DeleteCommand : Command
    {
    }
        public class Attack : Command
    {
        public Attack()
        {
            Range = 0;
            Livetime = 40;

            for (int i = 0; i < 1; i++)
            {
                // Tank
                UnitType unitType = new UnitType();

                unitType.MinArmorLevel = 1;
                unitType.MaxArmorLevel = 1;

                unitType.MinEngineLevel = 1;
                unitType.MinEngineLevel = 1;
                unitType.MaxEngineLevel = 1;

                unitType.MinWeaponLevel = 1;
                unitType.MaxWeaponLevel = 1;

                unitType.MinAssemblerLevel = 0;
                unitType.MaxAssemblerLevel = 0;
                unitType.MinExtractorLevel = 1;
                unitType.MaxExtractorLevel = 1;

                DemandedUnitTypes.Add(unitType);
            }
        }

        public override string ToString()
        {
            return "Attack";
        }

        public int Livetime { get; set; }

        private int? remainingLivetime;
        public override bool CanBeClosed()
        {
            if (!remainingLivetime.HasValue)
                remainingLivetime = Livetime;
            if (remainingLivetime-- < 0) return true;
            return false;
        }


        public override void AttachUnits(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            Dictionary<ulong, TileWithDistance> positionsToScount = CollectIncludedPositions();
            List<PlayerUnit> unitsAlreadyInArea = CollectUnitsAlreadyInArea(player, Range);

            List<UnitType> currentDemandedUnitTypes = new List<UnitType>();
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
                                    playerUnitFound = true;
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
                                        currentDemandedUnitTypes.Remove(unitType);
                                        if (playerUnit.Unit.Pos != Center && playerUnit.Unit.Engine != null)
                                        {
                                            dispatcher.MoveUnit(this, playerUnit, Center);
                                        }
                                        playerUnitFound = true;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (playerUnit.Unit.UnderConstruction)
                                {
                                    // Wait for completion
                                    foreach (UnitType unitType in DemandedUnitTypes)
                                    {
                                        if (unitType.Fits(playerUnit))
                                        {
                                            if (unitsAlreadyInArea.Contains(playerUnit))
                                            {
                                                // Discard connection to demaged unit.
                                                deadUnits.Add(playerUnit.Unit.UnitId);
                                            }
                                            else
                                            {
                                                // Unit in production
                                                // Request the unit again, so the factorys will produce it, but add this unit as the favourite
                                                // so only this unit will be supplied and not a new unit
                                                dispatcher.RequestUnit(this, unitType, playerUnit);
                                                playerUnitFound = true;

                                                // It not attached, the extractor will eat this unit in the factory...
                                                //deadUnits.Add(playerUnit.Unit.UnitId);
                                            }
                                            currentDemandedUnitTypes.Remove(unitType);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        
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

                foreach (PlayerUnit playerUnit in unitsAlreadyInArea)
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
                    dispatcher.RequestUnit(this, unitType, null);
                }
            }
        }
    }
}
