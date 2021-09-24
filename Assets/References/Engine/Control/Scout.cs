using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class ScoutPosition
    {
        public Position Pos { get; set; }
        public int MovesNotVisited { get; set; }
    }

    public class DegreeComparer : IComparer<int>
    {

        public int Compare(int x, int y)
        {
            if (x < y)
                return -1;
            else
                return 1;
        }

    }

    public class Scout : Command
    {
        public bool IsPeaceFul;

        public Scout()
        {
            Range = 4;
            IsPeaceFul = true;
        }

        public List<ScoutPosition> ScoutingPositions = new List<ScoutPosition>();
        public List<Position> VisiblePostitions = new List<Position>();

        public void FindScoutingPositions(Dispatcher dispatcher, Player player)
        {
            VisiblePostitions.Clear();


            foreach (TileWithDistance t in CollectIncludedPositions(Center, 3).Values)
            {
                VisiblePostitions.Add(t.Pos);
            }

            bool otherTilesFound = false;

            Dictionary<Position, TileWithDistance> positionsToScount = Map.EnumerateTiles(Center, Range);
            foreach (TileWithDistance t in positionsToScount.Values)
            {
                if (VisiblePostitions.Contains(t.Pos))
                    continue;
                otherTilesFound = true;

                ScoutPosition scoutPosition = new ScoutPosition();
                scoutPosition.Pos = t.Pos;
                ScoutingPositions.Add(scoutPosition);
                foreach (TileWithDistance tx in CollectIncludedPositions(t.Pos, 3).Values)
                {
                    VisiblePostitions.Add(tx.Pos);
                }
            }
            if (!otherTilesFound)
            {
                ScoutPosition scoutPosition = new ScoutPosition();
                scoutPosition.Pos = Center;
                ScoutingPositions.Add(scoutPosition);
            }
            int unitsNeeded = 3; // 2 + ScoutingPositions.Count / 4;
            while (DemandedUnitTypes.Count < unitsNeeded)
            {
                UnitType unitType = new UnitType();

                unitType.MinEngineLevel = 1;
                unitType.MinArmorLevel = 1;
                unitType.MinWeaponLevel = 1;
                unitType.MinExtractorLevel = 1;

                unitType.MaxEngineLevel = 1;
                unitType.MaxArmorLevel = 1;
                unitType.MaxWeaponLevel = 1;
                unitType.MaxExtractorLevel = 1;

                unitType.MaxContainerLevel = 0;
                unitType.MaxRadarLevel = 0;
                unitType.MaxReactorLevel = 0;
                unitType.MaxAssemblerLevel = 0;

                DemandedUnitTypes.Add(unitType);
            }
        }

        public override void AttachUnits(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            if (ScoutingPositions.Count == 0)
                FindScoutingPositions(dispatcher, player);

            List<Position> unseenTiles = new List<Position>();

            Dictionary<Position, TileWithDistance> positionsToScount = Map.EnumerateTiles(Center, Range);
            // Search +2 to get units move behind the border
            List<PlayerUnit> unitsAlreadyInArea = CollectUnitsAlreadyInArea(player, Range + 2);

            if (EnemyUnits.Count > 0)
            {
                // Enemy detected
                dispatcher.RequestAttack(this, EnemyUnits[0]);
                IsPeaceFul = false;

                ScoutingPositions.Clear();
                Center = EnemyUnits[0].Unit.Pos;
                return;
            }

            List<UnitType> remainingUnitTypes = new List<UnitType>();
            remainingUnitTypes.AddRange(DemandedUnitTypes);

            List<PlayerUnit> unitsAlreadyInAreaMatchingDemand = new List<PlayerUnit>();
            List<PlayerUnit> unitsAlreadyInAreaMatchingButDamaged = new List<PlayerUnit>();


            List<string> deadUnits = new List<string>();

            foreach (string unitId in AssignedUnits)
            {
                bool playerUnitFound = false;

                foreach (PlayerUnit playerUnit in player.Units.Values)
                {
                    if (playerUnit.Unit.UnitId == unitId)
                    {
                        playerUnitFound = true;

                        if (moveableUnits.Contains(playerUnit))
                        {
                            foreach (UnitType unitType in remainingUnitTypes)
                            {
                                if (unitType.Matches(playerUnit))
                                {
                                    unitsAlreadyInAreaMatchingDemand.Add(playerUnit);
                                    //dispatcher.ClaimUnit(this, playerUnit, RequestType.Scout);
                                    remainingUnitTypes.Remove(unitType);
                                }
                                break;
                            }
                        }
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

            // Do not scout around. Attack!
            //if (!IsPeaceFul) return;



            // Collect all fit units
            /*
            foreach (PlayerUnit playerUnit in unitsAlreadyInArea)
            {
                if (!moveableUnits.Contains(playerUnit))
                    continue;
                if (dispatcher.ClaimedUnits.Contains(playerUnit))
                    continue;
                //if (!playerUnit.Unit.IsComplete())
                //    continue;

                foreach (UnitType unitType in remainingUnitTypes)
                {
                    if (unitType.Matches(playerUnit))
                    {
                        unitsAlreadyInAreaMatchingDemand.Add(playerUnit);
                        dispatcher.ClaimUnit(this, playerUnit, RequestType.Scout);
                        remainingUnitTypes.Remove(unitType);
                    }
                    break;
                }
            }*/

            // Collect all demaged units that may fit
            /*
            foreach (PlayerUnit playerUnit in unitsAlreadyInArea)
            {
                if (!moveableUnits.Contains(playerUnit))
                    continue;

                foreach (UnitType unitType in remainingUnitTypes)
                {
                    if (unitType.Matches(playerUnit))
                    {
                        if (!playerUnit.Unit.IsComplete())
                        {
                            // Do not collect damaged units. Request a new one.
                            //unitsAlreadyInAreaMatchingButDamaged.Add(playerUnit);
                            //remainingUnitTypes.Remove(unitType);
                        }

                    }
                    break;
                }
            }*/

            // Request new units if needed
            foreach (UnitType unitType in remainingUnitTypes)
            {
                dispatcher.RequestUnit(this, unitType, null);
            }
            foreach (PlayerUnit playerUnit in unitsAlreadyInAreaMatchingButDamaged)
            {
                //dispatcher.RequestUpgrade(this, playerUnit);
            }

            // Priorise scout position order
            SortedList<int, ScoutPosition> sortedScoutPositions = new SortedList<int, ScoutPosition>(new DegreeComparer());
            foreach (ScoutPosition scoutPosition in ScoutingPositions)
            {
                scoutPosition.MovesNotVisited++;
                sortedScoutPositions.Add(scoutPosition.MovesNotVisited, scoutPosition);
            }

            if (unitsAlreadyInAreaMatchingDemand.Count > 0)
            {
                // Reverse
                for (int i = sortedScoutPositions.Values.Count - 1; i > 0; i--)
                {
                    ScoutPosition scoutPosition = sortedScoutPositions.Values[i];
                    Map.EnumerateTiles(scoutPosition.Pos, Range, stopper: p =>
                    {
                        foreach (PlayerUnit playerUnit in unitsAlreadyInAreaMatchingDemand)
                        {
                            if (!moveableUnits.Contains(playerUnit))
                                continue;

                            if (playerUnit.Unit.Pos == p.Pos)
                            {
                                if (p.Distance <= 1)
                                {
                                    // Close enough unit counts a visited
                                    scoutPosition.MovesNotVisited = 0;
                                }


                                // Closest unit to scout point. Move closer, remove unit from list if the unit isn't there already
                                if (scoutPosition.Pos != playerUnit.Unit.Pos)
                                {
                                    //unitsAlreadyInAreaMatchingDemand.Remove(playerUnit);
                                    dispatcher.MoveUnit(this, playerUnit, scoutPosition.Pos);

                                    /* Select a path that does not leave the area. Needed?
                                    Move move = dispatcher.GameController.MoveTo(playerUnit.Unit.Pos, scoutPosition.Pos, playerUnit.Unit.Engine);
                                    if (move != null)
                                    {
                                        bool pathOutsideOfArea = false;
                                        foreach (Position path in move.Positions)
                                        {
                                            // Is each position in path in scout area?
                                            if (!positionsToScount.ContainsKey(path))
                                            {
                                                pathOutsideOfArea = true;
                                            }
                                        }
                                        if (pathOutsideOfArea)
                                        {
                                            // unreachable, try another
                                        }
                                        else
                                        {
                                            dispatcher.MoveUnit(this, playerUnit, move);
                                            unitsAlreadyInAreaMatchingDemand.Remove(playerUnit);
                                        }
                                    }
                                    else
                                    {
                                        // unreachable, try another
                                    }*/
                                }

                                // Stop searching for this point
                                return true;

                            }
                        }
                        return false;


                    });
                }


                // Move any other attached units closer
                foreach (PlayerUnit playerUnit in unitsAlreadyInAreaMatchingDemand)
                {
                    if (!moveableUnits.Contains(playerUnit))
                        continue;

                    int ix;

#pragma warning disable CS0162 // Unreachable code detected
                    for (ix = sortedScoutPositions.Values.Count - 1; ix > 0; ix--)
#pragma warning restore CS0162 // Unreachable code detected
                    {
                        ScoutPosition scoutPosition = sortedScoutPositions.Values[ix];
                        dispatcher.MoveUnit(this, playerUnit, scoutPosition.Pos);

                        /*
                        Move move = dispatcher.GameController.MoveTo(playerUnit.Unit.Pos, scoutPosition.Pos, playerUnit.Unit.Engine);
                        if (move != null)
                        {
                            dispatcher.MoveUnit(this, playerUnit, move);
                        }
                        else
                        {
                            // Cannot reach, move unit to center, hope the best
                            //dispatcher.MoveUnit(this, playerUnit, Center);
                        }
                        */
                        break;
                    }

                }
            }
        }
    }
}
