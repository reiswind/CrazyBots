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
                        if (TileObject.GetWoodForObjectType(tileObject.TileObjectType) > 0)
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
                        TileObjectType tileObjectType = tileObject.TileObjectType;

                        if (!TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                        {
                            if (TileObject.GetWoodForObjectType(tileObject.TileObjectType) == 0)
                            {
                                continue;
                            }
                            tileObjectType = TileObjectType.Wood;
                        }

                        if (Unit.CurrentGameCommand != null &&
                            Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                            Unit.CurrentGameCommand.AttachedUnit.UnitId != null)
                        {
                            // Do not pickup stuff. Move to pickup location
                            continue;
                        }

                        if (Unit.IsSpaceForTileObject(tileObjectType))
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
                                            // Cannot extract what has been delivered
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
                {
                    if (TileObject.GetWoodForObjectType(tileObject.TileObjectType) == 0)
                    {
                        continue;
                    }
                }
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

        public void ExtractFromUnit(Unit unit, Unit otherUnit, List<MoveRecipeIngredient> extractedItems, int capacity, Dictionary<Position2, Unit> changedUnits)
        {
            if (!changedUnits.ContainsKey(otherUnit.Pos))
                changedUnits.Add(otherUnit.Pos, otherUnit);

            capacity = ExtractFromOtherUnit(unit, otherUnit, TileObjectType.All, changedUnits, extractedItems, capacity, capacity);
            /*
            while (capacity > 0)
            {
                MoveRecipeIngredient realIndigrient = otherUnit.FindIngredient(TileObjectType.All, false);
                if (realIndigrient == null) break;

                if (!Unit.IsSpaceForIngredient(realIndigrient))
                {
                    break;
                }
                capacity--;

                otherUnit.ConsumeIngredient(realIndigrient, changedUnits);

                // Add it to target
                Unit.AddIngredient(realIndigrient);

                if (!changedUnits.ContainsKey(Unit.Pos))
                    changedUnits.Add(Unit.Pos, Unit);

                realIndigrient.TargetPosition = Unit.Pos;

                // Report this
                extractedItems.Add(realIndigrient);
            }
            */
            if (capacity > 0)
            {
                Ability hitPart = otherUnit.HitBy(true);
                
                if (hitPart.Level == 0 && hitPart.TileContainer != null)
                {
                    if (hitPart.TileContainer.TileObjects.Count > 0)
                    {
                        int cntrIndex = 0;
                        while (capacity > 1 && hitPart.TileContainer.Count > cntrIndex)
                        {
                            TileObject tileObject = hitPart.TileContainer.TileObjects[cntrIndex];
                            MoveRecipeIngredient unitIndigrient = new MoveRecipeIngredient();
                            unitIndigrient.Count = 1;
                            unitIndigrient.SourcePosition = otherUnit.Pos;
                            unitIndigrient.TargetPosition = Unit.Pos;
                            unitIndigrient.TileObjectType = tileObject.TileObjectType;
                            unitIndigrient.Source = unitIndigrient.TileObjectType;

                            // Add it to target
                            if (Unit.IsSpaceForIngredient(unitIndigrient))
                            {
                                Unit.AddIngredient(unitIndigrient);
                                hitPart.TileContainer.Remove(tileObject);
                            }
                            else
                            {
                                cntrIndex++;
                            }
                        }
                        while (hitPart.TileContainer.Count > 0)
                        {
                            TileObject tileObject = hitPart.TileContainer.TileObjects[0];
                            Unit.Game.Map.AddOpenTileObject(tileObject);
                            hitPart.TileContainer.Remove(tileObject);
                        }
                    }
                }

                TileObject removedTileObject = hitPart.PartTileObjects[0];
                hitPart.PartTileObjects.Remove(removedTileObject);

                MoveRecipeIngredient indigrient = new MoveRecipeIngredient();
                indigrient.Count = 1;
                indigrient.SourcePosition = otherUnit.Pos;
                indigrient.TargetPosition = Unit.Pos;
                indigrient.TileObjectType = removedTileObject.TileObjectType;
                indigrient.Source = removedTileObject.TileObjectType;
                extractedItems.Add(indigrient);

                MoveRecipeIngredient realIndigrient = new MoveRecipeIngredient();
                realIndigrient.Count = 1;
                realIndigrient.SourcePosition = otherUnit.Pos;
                realIndigrient.TargetPosition = Unit.Pos;
                realIndigrient.TileObjectType = TileObjectType.Mineral;
                realIndigrient.Source = removedTileObject.TileObjectType;

                // Add it to target
                if (Unit.IsSpaceForIngredient(realIndigrient))
                {
                    Unit.AddIngredient(realIndigrient);
                }
                else
                {
                    TileObject tileObject = new TileObject();
                    tileObject.TileObjectType = TileObjectType.Mineral;
                    tileObject.Direction = Direction.C;
                    Unit.Game.Map.AddOpenTileObject(tileObject);
                }
                if (otherUnit.IsDead())
                {
                    if (hitPart.PartTileObjects.Count > 0)
                        throw new Exception();
                }
            }
        }


        public bool ExtractInto(Unit unit, Move move, Tile fromTile, Unit otherUnit, Dictionary<Position2, Unit> changedUnits)
        {
            List<MoveRecipeIngredient> extractedItems = new List<MoveRecipeIngredient>();

            if (otherUnit != null)
            {
                int capacity = Unit.CountCapacity();
                int minsInContainer = Unit.CountTileObjectsInContainer();
                capacity -= minsInContainer;

                if (otherUnit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    bool extractAnything = true;

                    if (unit.CurrentGameCommand != null)
                    {
                        if (unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            if (unit.CurrentGameCommand.TargetUnit.UnitId == unit.UnitId &&
                                unit.CurrentGameCommand.TransportUnit.UnitId == otherUnit.UnitId)
                            {
                                extractAnything = false;
                                unit.CurrentGameCommand.GameCommand.CommandComplete = true;

                                // unit is the recipient. otherunit is transporter
                                foreach (RecipeIngredient moveRecipeIngredient in unit.CurrentGameCommand.GameCommand.RequestedItems)
                                {
                                    if (moveRecipeIngredient.TileObjectType == TileObjectType.Burn ||
                                        moveRecipeIngredient.TileObjectType == TileObjectType.Ammo ||
                                        moveRecipeIngredient.TileObjectType == TileObjectType.Mineral)
                                    {
                                        capacity = ExtractFromOtherUnit(unit, otherUnit, moveRecipeIngredient.TileObjectType, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                                        /*
                                        int cnt = moveRecipeIngredient.Count;
                                        while (cnt-- > 0 && capacity > 0)
                                        {
                                            MoveRecipeIngredient realIndigrient = otherUnit.FindIngredient(moveRecipeIngredient.TileObjectType, false);
                                            if (realIndigrient == null) break;
                                            capacity--;
                                            if (!Unit.IsSpaceForIngredient(realIndigrient))
                                            {
                                                break;
                                            }

                                            // Remove it from source
                                            otherUnit.ConsumeIngredient(realIndigrient, changedUnits);

                                            // Add it to target
                                            Unit.AddIngredient(realIndigrient);

                                            if (!changedUnits.ContainsKey(Unit.Pos))
                                                changedUnits.Add(Unit.Pos, Unit);

                                            realIndigrient.TargetPosition = Unit.Pos;

                                            // Report this
                                            extractedItems.Add(realIndigrient);
                                        }*/
                                    }
                                }
                            }
                            if (unit.CurrentGameCommand.TransportUnit.UnitId == unit.UnitId &&
                                unit.CurrentGameCommand.AttachedUnit.UnitId == otherUnit.UnitId)
                            {
                                extractAnything = false;

                                // Pick up from container, unit is the transporter. otherUnit is delivering
                                capacity = PickFromContainer(unit, otherUnit, changedUnits, extractedItems, capacity);

                                // Remove the pickup location => deliver the items
                                unit.CurrentGameCommand.AttachedUnit.ClearUnitId(unit.Owner.Game.Map.Units);
                                unit.CurrentGameCommand.DeliverContent = true;
                            }
                        }
                    }
                    if (extractAnything)
                    {
                        // friendly unit
                        capacity = ExtractFromOtherUnit(unit, otherUnit, TileObjectType.All, changedUnits, extractedItems, capacity, capacity);
                    }

                    if (otherUnit.ExtractMe && !otherUnit.IsDead() && capacity > 0)
                    {
                        ExtractFromUnit(unit, otherUnit, extractedItems, capacity, changedUnits);
                    }

                    // Near Field Delivery. Extract all items from the transporter and place it in nearby untis
                    if (unit.Extractor != null && otherUnit.Engine != null && otherUnit.Container != null)
                    {
                        Tile unitTile = Unit.Game.Map.GetTile(Unit.Pos);

                        List<TileObject> availableTileObjects = new List<TileObject>();
                        availableTileObjects.AddRange(otherUnit.Container.TileContainer.TileObjects);

                        foreach (TileObject tileObject in availableTileObjects)
                        {
                            bool outOfIndigrients = false;
                            foreach (Tile n in unitTile.Neighbors)
                            {
                                if (n.Unit != null &&
                                    n.Unit != Unit &&
                                    n.Unit != otherUnit &&
                                    n.Unit.IsComplete() &&
                                    n.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                                {
                                    MoveRecipeIngredient realIndigrient = otherUnit.GetConsumableIngredient(tileObject.TileObjectType, false);
                                    if (realIndigrient == null)
                                    {
                                        outOfIndigrients = true;
                                        break;
                                    }
                                    if (Unit.IsSpaceForIngredient(realIndigrient))
                                    {                                        
                                        otherUnit.ConsumeIngredient(realIndigrient, changedUnits);

                                        if (!changedUnits.ContainsKey(n.Unit.Pos))
                                            changedUnits.Add(n.Unit.Pos, n.Unit);

                                        realIndigrient.TargetPosition = n.Unit.Pos;
                                        Unit.AddIngredient(realIndigrient);

                                        extractedItems.Add(realIndigrient);
                                    }
                                }
                            }
                            if (outOfIndigrients)
                                break;
                        }
                    }
                }
                else
                {
                    // enemy unit
                    if (!otherUnit.IsDead())
                    {
                        ExtractFromUnit(unit, otherUnit, extractedItems, capacity, changedUnits);
                    }
                }
            }
            else
            {
                foreach (TileObject tileObject in fromTile.TileObjects)
                {
                    TileObjectType collectedTileObjectType;
                    if (tileObject.TileObjectType == TileObjectType.Bush ||
                        tileObject.TileObjectType == TileObjectType.Tree)
                    {
                        collectedTileObjectType = TileObjectType.Wood;
                    }
                    else 
                    {
                        if (!TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                            continue;
                        collectedTileObjectType = tileObject.TileObjectType;
                    }

                    if (Unit.IsSpaceForTileObject(collectedTileObjectType))
                    {
                        if (fromTile.ExtractTileObject(tileObject))
                        {
                            MoveRecipeIngredient indigrient = new MoveRecipeIngredient();
                            indigrient.Count = 1;
                            indigrient.SourcePosition = fromTile.Pos;
                            indigrient.TargetPosition = Unit.Pos;
                            indigrient.TileObjectType = collectedTileObjectType;
                            indigrient.Source = TileObjectType.Ground;
                            extractedItems.Add(indigrient);

                            Unit.AddIngredient(indigrient);
                            break;
                        }
                    }
                }
            }

            if (Unit.CurrentGameCommand != null &&
                Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                !Unit.CurrentGameCommand.GameCommand.CommandComplete &&
                Unit.CurrentGameCommand.TargetUnit.UnitId == Unit.UnitId)
            {
                // This container has been filled, delivery is complete
                foreach (RecipeIngredient recipeIngredient in Unit.CurrentGameCommand.GameCommand.RequestedItems)
                {
                    if (Unit.AreAllIngredientsAvailable(Unit.CurrentGameCommand.GameCommand.RequestedItems))
                    {
                        Unit.CurrentGameCommand.GameCommand.CommandComplete = true;
                        unit.ResetGameCommand();
                    }
                    Unit.ClearReservations();
                }
            }

            // The removed tileobjects will be in the move until the next move
            move.Stats = unit.CollectStats();
            Unit.Game.CollectGroundStats(unit.Pos, move);
            move.MoveRecipe = new MoveRecipe();
            move.MoveRecipe.Ingredients = extractedItems;

            bool didRemove = extractedItems.Count > 0;
            if (didRemove)
            {
                if (!changedUnits.ContainsKey(unit.Pos))
                    changedUnits.Add(unit.Pos, Unit);

                // Book immediatly. Items appear in ui before they arrive. But this is not an issue here
                if (otherUnit != null)
                {
                    if (!changedUnits.ContainsKey(otherUnit.Pos))
                        changedUnits.Add(otherUnit.Pos, otherUnit);
                }
            }
            return didRemove;
        }

        private int ExtractFromOtherUnit(Unit unit, Unit otherUnit, TileObjectType tileObjectType, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, int capacity, int max)
        {
            while (max > 0 && capacity > 0)
            {
                // Extract only from the unit (false)
                bool extractNeighbors = false;
                if (otherUnit.Engine != null)
                {
                    // Depends...
                    extractNeighbors = false;
                }
                MoveRecipeIngredient realIndigrient = null;
                if (tileObjectType == TileObjectType.All)
                    realIndigrient = otherUnit.FindIngredient(TileObjectType.All, extractNeighbors);
                if (tileObjectType == TileObjectType.Burn)
                    realIndigrient = otherUnit.FindIngredientToBurn(unit);
                if (tileObjectType == TileObjectType.Ammo)
                    realIndigrient = otherUnit.FindIngredientForAmmo(unit);

                if (realIndigrient == null) break;
                if (!Unit.IsSpaceForIngredient(realIndigrient))
                {
                    otherUnit.ReserveIngredient(realIndigrient);
                    continue;
                }

                otherUnit.ConsumeIngredient(realIndigrient, changedUnits);
                capacity--;
                max--;

                // Targetposition
                Unit.AddIngredient(realIndigrient);

                if (!changedUnits.ContainsKey(Unit.Pos))
                    changedUnits.Add(Unit.Pos, Unit);

                realIndigrient.TargetPosition = Unit.Pos;
                extractedItems.Add(realIndigrient);
            }
            otherUnit.ClearReservations();
            return capacity;
        }

        private int PickFromContainer(Unit unit, Unit otherUnit, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, int capacity)
        {
            foreach (RecipeIngredient moveRecipeIngredient in unit.CurrentGameCommand.GameCommand.RequestedItems)
            {
                if (moveRecipeIngredient.TileObjectType == TileObjectType.Ammo)
                {
                    ExtractFromOtherUnit(unit, otherUnit, TileObjectType.Ammo, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                    /*
                    int cnt = moveRecipeIngredient.Count;
                    while (cnt-- > 0 && capacity > 0)
                    {
                        MoveRecipeIngredient realIndigrient = otherUnit.FindIngredientForAmmo(unit);
                        if (realIndigrient == null) break;

                        if (!Unit.IsSpaceForIngredient(realIndigrient))
                        {
                            break;
                        }

                        capacity--;

                        // Remove it from source
                        otherUnit.ConsumeIngredient(realIndigrient, changedUnits);

                        // Add it to target
                        Unit.AddIngredient(realIndigrient);

                        if (!changedUnits.ContainsKey(Unit.Pos))
                            changedUnits.Add(Unit.Pos, Unit);

                        realIndigrient.TargetPosition = Unit.Pos;

                        // Report this
                        extractedItems.Add(realIndigrient);
                    }
                    */
                }
                else if (moveRecipeIngredient.TileObjectType == TileObjectType.Burn)
                {
                    ExtractFromOtherUnit(unit, otherUnit, TileObjectType.Ammo, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                    /*
                    int cnt = moveRecipeIngredient.Count;
                    while (cnt-- > 0 && capacity > 0)
                    {
                        MoveRecipeIngredient realIndigrient = otherUnit.FindIngredientToBurn(unit);
                        if (realIndigrient == null) break;

                        if (!Unit.IsSpaceForIngredient(realIndigrient))
                        {
                            break;
                        }

                        capacity--;

                        // Remove it from source
                        otherUnit.ConsumeIngredient(realIndigrient, changedUnits);

                        // Add it to target
                        Unit.AddIngredient(realIndigrient);

                        if (!changedUnits.ContainsKey(Unit.Pos))
                            changedUnits.Add(Unit.Pos, Unit);

                        realIndigrient.TargetPosition = Unit.Pos;

                        // Report this
                        extractedItems.Add(realIndigrient);
                    }
                    */
                }
                else
                {
                    ExtractFromOtherUnit(unit, otherUnit, TileObjectType.All, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                    /*
                    int cnt = moveRecipeIngredient.Count;
                    while (cnt-- > 0 && capacity > 0)
                    {
                        MoveRecipeIngredient realIndigrient = otherUnit.FindIngredient(TileObjectType.All, true);
                        if (realIndigrient == null) break;
                        if (!Unit.IsSpaceForIngredient(realIndigrient))
                        {
                            break;
                        }

                        capacity--;

                        // Remove it from source
                        otherUnit.ConsumeIngredient(realIndigrient, changedUnits);

                        // Add it to target
                        Unit.AddIngredient(realIndigrient);

                        if (!changedUnits.ContainsKey(Unit.Pos))
                            changedUnits.Add(Unit.Pos, Unit);

                        realIndigrient.TargetPosition = Unit.Pos;

                        // Report this
                        extractedItems.Add(realIndigrient);
                    }*/
                }
            }

            return capacity;
        }
    }
}
