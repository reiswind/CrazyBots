using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{
    public class Collect : Command
    {
        public Collect()
        {
            Range = 3;

            UnitType unitType = new UnitType();
            unitType.MinEngineLevel = 1;
            unitType.MaxEngineLevel = 1;

            unitType.MinExtractorLevel = 2;
            unitType.MaxExtractorLevel = 2;

            unitType.MinContainerLevel = 1;
            unitType.MaxContainerLevel = 1;

            DemandedUnitTypes.Add(unitType);


        }

        private bool nomoreMetalFound;
        //private TileWithDistance nextTile;

        public override bool CanBeClosed()
        {
            return nomoreMetalFound;
        }

        public override void AttachUnits(Dispatcher dispatcher, Player player, List<PlayerUnit> moveableUnits)
        {
            base.AttachUnits(dispatcher, player, moveableUnits);
            
            List<string> deadUnits = new List<string>();

            foreach (string unitId in AssignedUnits)
            {
                bool playerUnitFound = false;
                foreach (PlayerUnit playerUnit in player.Units.Values)
                {
                    if (playerUnit.Unit.UnitId == unitId)
                    {
                        if (playerUnit.PossibleMoves.Count > 0)
                        {
                            playerUnitFound = true;
                            break;
                        }

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
                                if (playerUnit.Unit.Extractor.CanExtract)
                                {
                                    TileWithDistance nextTile = null;
                                    if (nextTile == null || nextTile.Tile.TileContainer.Minerals == 0)
                                    {
                                        // Move that punk to metal
                                        Dictionary<Position, TileWithDistance> tiles = Map.EnumerateTiles(Center, Range, true, matcher: tile =>
                                        {
                                            if (!this.PosititionsInArea.ContainsKey(tile.Pos))
                                                return false;

                                            if (tile.Pos != playerUnit.Unit.Pos)
                                            {
                                                // Extract from others, not the extractor
                                                if (tile.Unit != null)
                                                {
                                                    if (tile.Unit.Owner.PlayerModel.Id != playerUnit.Unit.Owner.PlayerModel.Id)
                                                    {
                                                        // Extract from eneny? Why not.
                                                        return true;
                                                    }
                                                    else
                                                    {
                                                        if (tile.Unit.ExtractMe)
                                                        {
                                                            return true;
                                                        }
                                                        else
                                                        {
                                                            return false;
                                                        }
                                                    }
                                                }
                                            }
                                            return tile.Tile.TileContainer.Minerals > 0;
                                        });

                                        nextTile = null;
                                        if (tiles.Count > 0)
                                        {
                                            foreach (TileWithDistance possibleTile in tiles.Values)
                                            {
                                                if (possibleTile.Tile.CanMoveTo(possibleTile.Tile)) // && */possibleTile.Unit == null)
                                                {
                                                    nextTile = possibleTile;
                                                    break;
                                                }
                                                else
                                                {
                                                    // If cannot move on tile, move next to it
                                                    foreach (Tile tx in possibleTile.Neighbors)
                                                    {
                                                        if (tx.CanMoveTo(possibleTile.Tile))
                                                        {
                                                            nextTile = new TileWithDistance(tx, 0);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (nextTile != null)
                                    {
                                        playerUnit.Unit.ExtractUnit();
                                        UnitReachedCommandDoNotFollowPath = true;
                                        if (nextTile.Pos != playerUnit.Unit.Pos)
                                            dispatcher.MoveUnit(this, playerUnit, nextTile.Pos);

                                        playerUnitFound = true;
                                    }
                                    else
                                    {
                                        // Release container. No more metal to collect. The stray collector should catch this
                                        playerUnit.Unit.ExtractUnit();
                                        nomoreMetalFound = true;

                                        foreach (CommandSource commandSource in CommandSources)
                                        {
                                            // Assign it to the source. The source will take care of extraction
                                            commandSource.Parent.AssignUnit(playerUnit.Unit.UnitId);
                                        }
                                    }
                                }
                                else
                                {
                                    // Container full. Release it
                                    playerUnit.Unit.ExtractUnit();

                                    foreach (CommandSource commandSource in CommandSources)
                                    {
                                        // Assign it to the source. The source will take care of extraction
                                        commandSource.Parent.AssignUnit(playerUnit.Unit.UnitId);
                                    }
                                }
                            }
                            else
                            {
                                if (!UnitReachedCommandDoNotFollowPath)
                                {
                                    // Not arrived yet. In production?
                                    playerUnitFound = true;
                                }
                                else
                                {
                                    // Otherwise: Has arrived, was collecting and is now damaged. Unassign and mark for extraction
                                    if (!playerUnit.Unit.UnderConstruction)
                                    {
                                        playerUnit.Unit.ExtractUnit();

                                        foreach (CommandSource commandSource in CommandSources)
                                        {
                                            // Assign it to the source. The source will take care of extraction
                                            commandSource.Parent.AssignUnit(playerUnit.Unit.UnitId);
                                        }
                                    }
                                    else
                                    {
                                        playerUnitFound = true;
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
        }

        public override string ToString()
        {
            return "Collect";
        }

    }
}
