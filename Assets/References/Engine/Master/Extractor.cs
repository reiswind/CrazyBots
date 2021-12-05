using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Extractor : Ability
    {
        public override string Name { get { return "Extractor"; } }

        public Extractor(Unit owner, int level) : base(owner, TileObjectType.PartExtractor)
        {
            Level = level;
        }
        private int MetalCollectionRange
        {
            get
            {

                if (Level == 3)
                    return 3;
                if (Level == 2)
                    return 2;
                if (Level == 1)
                    return 1;

                return 0;
            }
        }
        public Dictionary<Position2, TileWithDistance> CollectExtractionTiles()
        {
            return Unit.Game.Map.EnumerateTiles(Unit.Pos, MetalCollectionRange, false);
        }

        public Dictionary<Position2, TileWithDistance> CollectExtractableTiles()
        {
            Dictionary<Position2, TileWithDistance> includePositions = null;
            if (Unit.CurrentGameCommand != null && Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Collect)
            {
                // Only for units who collect in the area, not for factory units
                if (Unit.CurrentGameCommand.AttachedUnit.UnitId == Unit.UnitId)
                {
                    includePositions = Unit.CurrentGameCommand.GameCommand.IncludedPositions;
                }
            }

            return Unit.Game.Map.EnumerateTiles(Unit.Pos, MetalCollectionRange, false, matcher: tile =>
            {
                if (includePositions != null && !includePositions.ContainsKey(tile.Pos))
                {
                    // If command is active, extract only in the area
                    return false;
                }
                if (tile.Unit != null)
                {
                    if (tile.Unit.Owner.PlayerModel.Id != Unit.Owner.PlayerModel.Id)
                    {
                        // Extract from enemy- Yeah!
                        return true;
                    }
                    if (tile.Unit.Container != null && tile.Unit.Container.TileContainer.Count > 0)
                    {
                        // Extract from a friendly container
                        return true;
                    }
                    if (tile.Unit.ExtractMe)
                    {
                        // Extract from a friendly unit, marked for extraction
                        return true;
                    }
                }
                if (tile.Tile.HasTileObjects)
                {
                    foreach (TileObject tileObject in tile.Tile.TileObjects)
                    {
                        if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                            return true;
                    }
                }
                return false;

            });
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Extract) == 0)
                return;
            /* TODOMIN
            if (CanExtractDirt)
            {
                Tile highest = null;
                Tile t = Unit.Game.Map.GetTile(Unit.Pos);
                foreach (Tile n in t.Neighbors)
                {
                    bool possible = false;
                    if (n.Unit == null &&
                        !n.IsUnderwater &&
                        n.Height >= 0.2f &&
                        n.Height -0.1f > t.Height)
                    {
                        possible = true;
                    }
                    if (!possible)
                    {
                        if (n.TileObjects.Count > 0)
                            possible = true;
                    }

                    if (possible)
                    {
                        if (highest == null)
                            highest = n;
                        else
                        {
                            if (n.Height > highest.Height)
                                highest = n;
                        }
                    }
                }
                if (highest != null)
                {
                    Move move = new Move();

                    move.MoveType = MoveType.Extract;

                    move.UnitId = Unit.UnitId;
                    move.OtherUnitId = "Dirt";
                    move.Position2s = new List<Position2>();
                    move.Position2s.Add(Unit.Pos);
                    move.Position2s.Add(highest.Pos);

                    possibleMoves.Add(move);

                }
                // Dirt is good enough
                if (possibleMoves.Count > 0)
                {
                    return;
                }
            }

            if (!CanExtractMinerals)
                // Unit full Not possible to extract
                return;
            */
            if (Unit.ExtractMe)
                return;

            bool enemyfound = false;
            Dictionary<Position2, TileWithDistance> resultList = CollectExtractableTiles();

            foreach (TileWithDistance t in resultList.Values)
            {
                if (!enemyfound)
                {
                    foreach (TileObject tileObject in t.Tile.TileObjects)
                    {
                        if (!TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                            continue;

                        if (Unit.CurrentGameCommand != null &&
                            Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                            Unit.CurrentGameCommand.AttachedUnit.UnitId != null)
                        {
                            // Do not pickup stuff. Move to pickup location
                            continue;
                        }

                        if (Unit.IsSpaceForTileObject(tileObject))
                        {
                            Move move = new Move();

                            move.MoveType = MoveType.Extract;

                            move.UnitId = Unit.UnitId;
                            move.OtherUnitId = tileObject.TileObjectType.ToString();
                            move.Positions = new List<Position2>();
                            move.Positions.Add(Unit.Pos);
                            move.Positions.Add(t.Pos);

                            possibleMoves.Add(move);
                        }
                    }
                }
                if (t.Pos == Unit.Pos)
                {
                    // Extract from ourselves? Not.
                }
                else if (t.Unit != null)
                {
                    // Extract from tile with unit?
                    if (!enemyfound && Unit.Owner.PlayerModel.Id == t.Unit.Owner.PlayerModel.Id)
                    {
                        // Extract from own unit?
                        if (t.Unit.ExtractMe)
                        {
                            // Extract everything
                            Move move = new Move();

                            move.MoveType = MoveType.Extract;

                            move.UnitId = Unit.UnitId;
                            move.OtherUnitId = t.Unit.UnitId;
                            move.Positions = new List<Position2>();
                            move.Positions.Add(Unit.Pos);
                            move.Positions.Add(t.Pos);

                            possibleMoves.Add(move);
                        }
                        else if (t.Unit.Container != null)
                        {
                            if (Unit.Engine == null)
                            {
                                bool added = false;
                                if (Unit.CurrentGameCommand != null && Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                                {
                                    if (Unit.UnitId == Unit.CurrentGameCommand.TargetUnit.UnitId &&
                                        Unit.CurrentGameCommand.TransportUnit.UnitId == t.Unit.UnitId)
                                    {
                                        // This is the transporter who has reached the target
                                        Move move = CreateExtractMoveIfPossible(t.Unit);
                                        if (move == null)
                                        {
                                            // Cannot extract what hat been delivered
                                            Unit.CurrentGameCommand.GameCommand.CommandCanceled = true;
                                            Unit.ResetGameCommand();
                                        }
                                        else
                                        {
                                            possibleMoves.Add(move);
                                            added = true;
                                        }
                                    }
                                }
                                if (!added)
                                {
                                    if (t.Unit.CurrentGameCommand != null && t.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                                    {
                                        // Container should not extract from a worker that is used to deliver items.
                                    }
                                    else
                                    {
                                        // Container extracting from worker
                                        if (t.Unit.Engine != null)
                                        {
                                            Move move = CreateExtractMoveIfPossible(t.Unit);
                                            if (move != null)
                                            {
                                                possibleMoves.Add(move);
                                            }
                                        }
                                        else
                                        {
                                            // Extract from friendly structure next
                                            if (Unit.Weapon != null)
                                            {
                                                // Turret from Container
                                                Move move = CreateExtractMoveIfPossible(t.Unit);
                                                if (move != null)
                                                {
                                                    possibleMoves.Add(move);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Unit.Assembler != null && t.Unit.Container != null)
                                {
                                    // Assembler extract from Container
                                    Move move = CreateExtractMoveIfPossible(t.Unit);
                                    if (move != null)
                                    {
                                        possibleMoves.Add(move);
                                    }
                                }
                                if (Unit.Weapon != null)
                                {
                                    // Fighter extract from container
                                    Move move = CreateExtractMoveIfPossible(t.Unit);
                                    if (move != null)
                                    {
                                        possibleMoves.Add(move);
                                    }
                                }
                                if (Unit.CurrentGameCommand != null)
                                {
                                    if (Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                                    {
                                        if (Unit.UnitId == Unit.CurrentGameCommand.TransportUnit.UnitId) // FactoryUnit
                                        {
                                            // This is the transporter, that should extract from container to deliver it
                                            // Assembler extract from Container. 

                                            // Extract only if the other unit does not have a task (Or the task is to collect)
                                            if (t.Unit.CurrentGameCommand == null ||
                                                t.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Collect)
                                            {
                                                Move move = CreateExtractMoveIfPossible(t.Unit);
                                                if (move != null)
                                                {
                                                    possibleMoves.Add(move);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Cannot extract if enemy shield is up
                        if (t.Unit.Armor == null || !t.Unit.Armor.ShieldActive)
                        {
                            // If it is possible to extract from enemy, do it. Nothing else.
                            possibleMoves.Clear();
                            enemyfound = true;

                            // Extract from enemy? Always an option
                            Move move = new Move();

                            move.MoveType = MoveType.Extract;

                            move.UnitId = Unit.UnitId;
                            move.OtherUnitId = t.Unit.UnitId;
                            move.Positions = new List<Position2>();
                            move.Positions.Add(Unit.Pos);
                            move.Positions.Add(t.Pos);

                            possibleMoves.Add(move);
                        }
                    }
                }
            }
        }

        private Move CreateExtractMoveIfPossible(Unit otherInit)
        {
            bool possibleItem = false;
            foreach (TileObject tileObject in otherInit.Container.TileContainer.TileObjects)
            {
                if (!TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                    continue;

                if (Unit.IsSpaceForTileObject(tileObject))
                {
                    possibleItem = true;
                    break;
                }
            }
            Move move = null;
            if (possibleItem)
            {
                move = new Move();

                move.MoveType = MoveType.Extract;

                move.UnitId = Unit.UnitId;
                move.OtherUnitId = otherInit.UnitId;
                move.Positions = new List<Position2>();
                move.Positions.Add(Unit.Pos);
                move.Positions.Add(otherInit.Pos);
            }
            return move;
        }

        public bool CanExtract
        {
            get
            {
                return CanExtractTileObject || CanExtractDirt;
            }
        }

        public bool CanExtractDirt
        {
            get
            {
                if (Unit.Power == 0)
                    return false;

                if (Unit.Weapon != null)
                {
                    // Dirt
                    if (Unit.Weapon.TileContainer != null && Unit.Weapon.TileContainer.Count < Unit.Weapon.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CanExtractTileObject
        {
            get
            {
                if (Unit.Power == 0)
                    return false;

                if (Unit.Assembler != null)
                {
                    if (Unit.Assembler.TileContainer != null && Unit.Assembler.TileContainer.Count < Unit.Assembler.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Reactor != null)
                {
                    if (Unit.Reactor.TileContainer != null && Unit.Reactor.TileContainer.Count < Unit.Reactor.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Weapon != null)
                {
                    if (Unit.Weapon.TileContainer != null && Unit.Weapon.TileContainer.Count < Unit.Weapon.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Container != null && Unit.Container.TileContainer.Count < Unit.Container.TileContainer.Capacity)
                {
                    return true;
                }
                return false;
            }
        }

        public void ExtractFromUnit(Move move, Unit otherUnit, List<TileObject> removeTileObjects)
        {
            Ability hitPart = otherUnit.HitBy(true);

            if (hitPart.Level == 0 && hitPart.TileContainer != null)
            {
                if (hitPart.TileContainer.TileObjects.Count > 0)
                {
                    removeTileObjects.AddRange(hitPart.TileContainer.TileObjects);
                    hitPart.TileContainer.Clear();
                }
            }

            TileObject removedTileObject = hitPart.PartTileObjects[0];
            hitPart.PartTileObjects.Remove(removedTileObject);
            removeTileObjects.Add(removedTileObject);

            foreach (TileObject removed in removeTileObjects)
            {
                if (!TileObject.IsTileObjectTypeCollectable(removed.TileObjectType))
                {
                    if (!TileObject.CanConvertTileObjectIntoMineral(removed.TileObjectType))
                    {
                        throw new Exception();
                    }
                }
            }

            move.Stats = new MoveUpdateStats();
            Unit.Game.CollectGroundStats(otherUnit.Pos, move, removeTileObjects);

            if (otherUnit.IsDead())
            {
                if (hitPart.PartTileObjects.Count > 0)
                    throw new Exception();
            }
        }

        public bool ExtractInto(Unit unit, Move move, Tile fromTile, Game game, Unit otherUnit, string otherUnitId)
        {
            List<TileObject> removeTileObjects = new List<TileObject>();

            if (otherUnit != null)
            {
                if (otherUnit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    int capacity = Unit.CountCapacity();
                    int minsInContainer = Unit.CountTileObjectsInContainer();

                    capacity -= minsInContainer;

                    // friendly unit
                    while (capacity > 0)
                    {
                        if (!otherUnit.RemoveTileObjects(removeTileObjects, 1, TileObjectType.All, unit))
                        {
                            break;
                        }
                        capacity--;
                    }
                    if (unit.CurrentGameCommand != null)
                    {
                        if (unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            if (unit.CurrentGameCommand.TargetUnit.UnitId == unit.UnitId)
                            {
                                // This is the transporter??. It has extracted the content into the target, command is complete
                                unit.CurrentGameCommand.GameCommand.CommandComplete = true;
                            }
                        }
                    }

                    if (otherUnit.ExtractMe && !otherUnit.IsDead() && capacity > 0)
                    {
                        ExtractFromUnit(move, otherUnit, removeTileObjects);
                    }
                }
                else
                {
                    // enemy unit
                    if (!otherUnit.IsDead())
                    {
                        ExtractFromUnit(move, otherUnit, removeTileObjects);
                    }
                }
            }
            else
            {
                foreach (TileObject tileObject in fromTile.TileObjects)
                {
                    if (!TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                        continue;

                    if (Unit.IsSpaceForTileObject(tileObject))
                    {
                        fromTile.ExtractTileObject(tileObject);
                        removeTileObjects.Add(tileObject);
                        break;
                    }
                }
            }

            // The removed tileobjects will be in the move until the next move
            move.Stats = unit.CollectStats();
            Unit.Game.CollectGroundStats(unit.Pos, move, removeTileObjects);

            bool didRemove = removeTileObjects.Count > 0;

            return didRemove;
        }
    }
}
