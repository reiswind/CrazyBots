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
            if (Unit.CurrentGameCommand != null && Unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
            {
                // Only for units who collect in the area, not for factory units
                if (Unit.CurrentGameCommand.AttachedUnit.UnitId == Unit.UnitId)
                {
                    includePositions = Unit.CurrentGameCommand.IncludedPositions;
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

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPositions, MoveFilter moveFilter)
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
                if (includedPositions != null && !includedPositions.Contains(t.Pos))
                {
                    continue;
                }
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
                            Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest &&
                            Unit.CurrentGameCommand.AttachedUnit.UnitId != null)
                        {
                            // Do not pickup stuff. Move to pickup location
                            continue;
                        }
                        if (Unit.CurrentGameCommand != null &&
                            Unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect &&
                            Unit.CurrentGameCommand.TransportUnit.UnitId == Unit.UnitId)
                        {
                            // Do not pickup stuff. Move to pickup location
                            continue;
                        }

                        if (UnitOrders.GetAcceptedAmount(Unit, tileObjectType) > 0)
                        {
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
                                if (Unit.CurrentGameCommand != null && Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                                {
                                    if (Unit.UnitId == Unit.CurrentGameCommand.TargetUnit.UnitId &&
                                        Unit.CurrentGameCommand.TransportUnit.UnitId == t.Unit.UnitId)
                                    {
                                        // This is the transporter who has reached the target
                                        Move move = CreateExtractMoveIfPossible(t.Unit);
                                        if (move == null)
                                        {
                                            // Cannot extract what has been delivered
                                            Unit.CurrentGameCommand.CommandCanceled = true;
                                            //Unit.ResetGameCommand();
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
                                    if (t.Unit.CurrentGameCommand != null && 
                                        t.Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest &&
                                        t.Unit.CurrentGameCommand.TargetUnit.UnitId != Unit.UnitId)
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
                                        else if (Unit.Weapon != null)
                                        {
                                            // Turret from Container
                                            Move move = CreateExtractMoveIfPossible(t.Unit);
                                            if (move != null)
                                            {
                                                possibleMoves.Add(move);
                                            }
                                        }
                                        else if (Unit.Container != null)
                                        {
                                            // Transport from Container to Container
                                            // Check if the targetUnit accepts at least one of the available item. 
                                            foreach (TileObject tileObject in t.Unit.Container.TileContainer.TileObjects)
                                            {
                                                if (UnitOrders.GetAcceptedAmount(Unit, tileObject.TileObjectType) > 0)
                                                {
                                                    if (Unit.IsSpaceForTileObject(tileObject.TileObjectType))
                                                    {
                                                        Move move = CreateExtractMove(t.Unit);
                                                        if (move != null)
                                                        {
                                                            possibleMoves.Add(move);
                                                        }
                                                        break;
                                                    }
                                                }
                                            }
                                            /*
                                            Move move = CreateExtractMoveIfPossible(t.Unit);
                                            if (move != null)
                                            {
                                                possibleMoves.Add(move);
                                            }*/
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
                                    if (Unit.Weapon != null)
                                    {
                                        // Fighter extract from container
                                        Move move = CreateExtractMoveIfPossible(t.Unit);
                                        if (move != null)
                                        {
                                            possibleMoves.Add(move);
                                        }
                                    }
                                }
                                // The command is to collect from this unit
                                if (Unit.CurrentGameCommand != null && 
                                    Unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect &&
                                    Unit.UnitId == Unit.CurrentGameCommand.TransportUnit.UnitId &&
                                    t.Unit.UnitId == Unit.CurrentGameCommand.TargetUnit.UnitId)
                                {
                                    Move move = CreateExtractMove(t.Unit);
                                    if (move != null)
                                    {
                                        possibleMoves.Add(move);
                                    }
                                }
                                if (Unit.CurrentGameCommand != null && Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                                {
                                    if (Unit.UnitId == Unit.CurrentGameCommand.TransportUnit.UnitId)
                                    {
                                        if (t.Unit.CurrentGameCommand != null &&
                                            t.Unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest &&
                                            t.Unit.UnitId == Unit.CurrentGameCommand.TargetUnit.UnitId)
                                        {
                                            // Deliver

                                            /*
                                            Move move = CreateExtractMove(t.Unit);
                                            if (move != null)
                                            {
                                                possibleMoves.Add(move);
                                            }*/
                                        }
                                        else
                                        {
                                            // Pickup

                                            // This is the transporter, that should extract from container to deliver it
                                            // Assembler extract from Container. 

                                            // Extract only if the other unit does not have a task (Or the task is to collect)
                                            if (t.Unit.CurrentGameCommand == null ||
                                                t.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
                                            {
                                                Unit targetUnit = Unit.Game.Map.Units.FindUnit(Unit.CurrentGameCommand.TargetUnit.UnitId);
                                                if (targetUnit != null)
                                                {
                                                    // Check if the targetUnit accepts at least one of the available item. These are put in the worker.
                                                    foreach (TileObject tileObject in t.Unit.Container.TileContainer.TileObjects)
                                                    {
                                                        if (UnitOrders.GetAcceptedAmount(targetUnit, tileObject.TileObjectType) > 0)
                                                        {
                                                            if (Unit.IsSpaceForTileObject(tileObject.TileObjectType))
                                                            {
                                                                Move move = CreateExtractMove(t.Unit);
                                                                if (move != null)
                                                                {
                                                                    possibleMoves.Add(move);
                                                                }
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    
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
            // Check if the targetUnit accepts at least one of the available item. 
            foreach (TileObject tileObject in otherInit.Container.TileContainer.TileObjects)
            {
                if (UnitOrders.GetAcceptedAmount(Unit, tileObject.TileObjectType) > 0)
                {
                    if (Unit.IsSpaceForTileObject(tileObject.TileObjectType))
                    {
                        bool otherUnitWillGiveTheItem = true;
                        foreach (UnitItemOrder unitItemOrder in otherInit.UnitOrders.unitItemOrders)
                        {
                            if (unitItemOrder.TileObjectState == TileObjectState.Accept)
                            {
                                otherUnitWillGiveTheItem = false;
                                break;
                            }
                        }
                        if (otherUnitWillGiveTheItem)
                        {
                            Move move = CreateExtractMove(otherInit);
                            if (move != null)
                            {
                                return move;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Move CreateExtractMove(Unit otherInit)
        {
            Move move;

            move = new Move();

            move.MoveType = MoveType.Extract;

            move.UnitId = Unit.UnitId;
            move.OtherUnitId = otherInit.UnitId;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(otherInit.Pos);
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

        public int ExtractFromUnit(Unit unit, Unit otherUnit, List<MoveRecipeIngredient> extractedItems, int capacity, Dictionary<Position2, Unit> changedUnits)
        {
            if (!changedUnits.ContainsKey(otherUnit.Pos))
                changedUnits.Add(otherUnit.Pos, otherUnit);

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
                            unitIndigrient.SourceUnitId = otherUnit.UnitId;
                            unitIndigrient.TargetPosition = Unit.Pos;
                            unitIndigrient.TileObjectType = tileObject.TileObjectType;
                            unitIndigrient.Source = unitIndigrient.TileObjectType;

                            // Add it to target
                            if (Unit.IsSpaceForIngredient(unitIndigrient))
                            {                                
                                Unit.AddIngredient(unitIndigrient);
                                capacity--;
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
                indigrient.SourceUnitId = otherUnit.UnitId;
                indigrient.TargetPosition = Unit.Pos;
                indigrient.TileObjectType = removedTileObject.TileObjectType;
                indigrient.Source = removedTileObject.TileObjectType;
                extractedItems.Add(indigrient);

                MoveRecipeIngredient realIndigrient = new MoveRecipeIngredient();
                realIndigrient.Count = 1;
                realIndigrient.SourcePosition = otherUnit.Pos;
                realIndigrient.SourceUnitId = otherUnit.UnitId;
                realIndigrient.TargetPosition = Unit.Pos;
                realIndigrient.TileObjectType = TileObjectType.Mineral;
                realIndigrient.Source = removedTileObject.TileObjectType;

                // Add it to target
                if (Unit.IsSpaceForIngredient(realIndigrient))
                {
                    Unit.AddIngredient(realIndigrient);
                    capacity--;
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
            return capacity;
        }


        public bool ExtractInto(Unit unit, Move move, Tile fromTile, Unit otherUnit, Dictionary<Position2, Unit> changedUnits)
        {
            List<MoveRecipeIngredient> extractedItems = new List<MoveRecipeIngredient>();

            if (otherUnit != null)
            {
                int capacity = Unit.CountCapacity();
                int minsInContainer = Unit.CountTileObjectsInContainer(TileObjectType.All);
                capacity -= minsInContainer;

                if (otherUnit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    bool extractAnything = true;
                    Unit targetUnit = unit;

                    if (unit.CurrentGameCommand != null)
                    {
                        // The command is to collect from this unit
                        if (Unit.CurrentGameCommand != null &&
                            Unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect &&
                            Unit.UnitId == Unit.CurrentGameCommand.TransportUnit.UnitId &&
                            otherUnit.UnitId == Unit.CurrentGameCommand.TargetUnit.UnitId)
                        {
                            // Pick up from container, unit is the transporter. otherUnit is delivering
                            capacity = ExtractFromOtherContainer(targetUnit, otherUnit, changedUnits, extractedItems, capacity, true);
                        }
                        if (unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            if (unit.CurrentGameCommand.TargetUnit.UnitId == unit.UnitId &&
                                unit.CurrentGameCommand.TransportUnit.UnitId == otherUnit.UnitId)
                            {
                                //extractAnything = false;
                                unit.CurrentGameCommand.CommandComplete = true;

                                // unit is the recipient. otherunit is transporter
                                /*
                                foreach (RecipeIngredient moveRecipeIngredient in unit.CurrentGameCommand.GameCommand.RequestedItems)
                                {
                                    if (moveRecipeIngredient.TileObjectType == TileObjectType.Burn ||
                                        moveRecipeIngredient.TileObjectType == TileObjectType.Ammo ||
                                        moveRecipeIngredient.TileObjectType == TileObjectType.Mineral)
                                    {
                                        capacity = ExtractFromOtherUnit(unit, otherUnit, moveRecipeIngredient.TileObjectType, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                                    }
                                }
                                */
                            }
                            if (unit.CurrentGameCommand.TransportUnit.UnitId == unit.UnitId)
                            {
                                extractAnything = false;

                                targetUnit = Unit.Game.Map.Units.FindUnit(Unit.CurrentGameCommand.TargetUnit.UnitId);
                                if (targetUnit != null)
                                {
                                    // Pick up from container, unit is the transporter. otherUnit is delivering
                                    capacity = ExtractFromOtherContainer(targetUnit, otherUnit, changedUnits, extractedItems, capacity);
                                }
                                // Remove the pickup location => deliver the items
                                //unit.CurrentGameCommand.AttachedUnit.ClearUnitId(); // unit.Owner.Game.Map.Units);
                                //unit.CurrentGameCommand.DeliverContent = true;
                            }
                        }
                    }
                    if (extractAnything)
                    {
                        // friendly container, share 
                        capacity = ExtractFromOtherContainer(targetUnit, otherUnit, changedUnits, extractedItems, capacity);
                    }

                    if (otherUnit.ExtractMe && !otherUnit.IsDead() && capacity > 0)
                    {
                        capacity = ExtractFromUnit(targetUnit, otherUnit, extractedItems, capacity, changedUnits);
                    }

                    // Near Field Delivery. Extract all items from the transporter and place it in nearby untis
                    if (unit.Extractor != null && otherUnit.Engine != null && otherUnit.Container != null && capacity > 0)
                    {
                        Tile unitTile = Unit.Game.Map.GetTile(Unit.Pos);

                        List<TileObject> availableTileObjects = new List<TileObject>();
                        availableTileObjects.AddRange(otherUnit.Container.TileContainer.TileObjects);

                        foreach (TileObject tileObject in availableTileObjects)
                        {
                            foreach (Tile n in unitTile.Neighbors)
                            {
                                if (n.Unit != null &&
                                    n.Unit != Unit &&
                                    n.Unit != otherUnit &&
                                    n.Unit.IsComplete() &&
                                    n.Unit.Container != null &&
                                    n.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                                {
                                    capacity = ExtractFromOtherContainer(targetUnit, n.Unit, changedUnits, extractedItems, capacity);
                                    if (capacity <= 0)
                                        break;
                                    /*
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
                                    }*/
                                }
                            }
                            if (capacity <= 0)
                                break;
                        }
                    }
                }
                else
                {
                    // enemy unit
                    if (!otherUnit.IsDead())
                    {
                        capacity = ExtractFromOtherContainer(unit, otherUnit, changedUnits, extractedItems, capacity);
                        ExtractFromUnit(unit, otherUnit, extractedItems, capacity, changedUnits);
                    }
                }
            }
            else
            {
                ExtractFromGround(fromTile, extractedItems);
            }

            /* This check has been shifted to AntContainer move
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
                }
            }*/

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

            if (didRemove && Unit.CurrentGameCommand != null && Unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
            { 
                Unit.CurrentGameCommand.GameCommandState = GameCommandState.Collecting;
                Unit.CurrentGameCommand.CommandComplete = false;
                Unit.CurrentGameCommand.AttachedUnit.SetStatus("Collecting");
                Unit.Changed = true;
            }

            return didRemove;
        }

        private void ExtractFromGround(Tile fromTile, List<MoveRecipeIngredient> extractedItems)
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
                if (UnitOrders.GetAcceptedAmount(Unit, collectedTileObjectType) > 0)
                {
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
        }
        private int TransferTileObjects(Unit otherUnit, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, UnitItemOrder pullItemOrder, int capacity, List<TileObject> excludeTileObjects)
        {
            while (capacity > 0 && extractedItems.Count < 12) // 12 max transfer
            {
                // ok, give it all to the other container
                MoveRecipeIngredient realIndigrient;
                realIndigrient = otherUnit.FindIngredient(pullItemOrder.TileObjectType, false, excludeTileObjects);
                if (realIndigrient == null) break;

                otherUnit.ConsumeIngredient(realIndigrient, changedUnits);
                capacity--;

                Unit.AddIngredient(realIndigrient);

                if (!changedUnits.ContainsKey(Unit.Pos))
                    changedUnits.Add(Unit.Pos, Unit);

                realIndigrient.TargetPosition = Unit.Pos;
                extractedItems.Add(realIndigrient);
            }

            return capacity;
        }

        private int PullFromOtherContainer(Unit unit, Unit otherUnit, Dictionary<Position2, Unit> changedUnits, 
            List<MoveRecipeIngredient> extractedItems, UnitItemOrder pullItemOrder, int capacity, bool force = false)
        {
            List<TileObject> excludeTileObjects = new List<TileObject>();
            foreach (UnitItemOrder unitItemOrder in otherUnit.UnitOrders.unitItemOrders)
            {
                if (unitItemOrder.TileObjectType == pullItemOrder.TileObjectType)
                {
                    if (!force && unitItemOrder.TileObjectState == pullItemOrder.TileObjectState && otherUnit.Engine == null && unit.Engine == null)
                    {
                        BalanceWithOtherContainer(unit, otherUnit, changedUnits, extractedItems, pullItemOrder, capacity);
                    }
                    else if (unitItemOrder.TileObjectState == TileObjectState.Accept)
                    {
                        if (force)
                        {
                            int transferAmount = capacity;

                            int maxTransferAmount = UnitOrders.GetAcceptedAmount(unit, pullItemOrder.TileObjectType);
                            if (transferAmount > maxTransferAmount)
                                transferAmount = maxTransferAmount;
                            capacity = TransferTileObjects(otherUnit, changedUnits, extractedItems, pullItemOrder, transferAmount, excludeTileObjects);
                        }
                    }
                    else if (unitItemOrder.TileObjectState == TileObjectState.None || unitItemOrder.TileObjectState == TileObjectState.Deny)
                    {
                        int transferAmount = capacity;

                        int maxTransferAmount = UnitOrders.GetAcceptedAmount(unit, pullItemOrder.TileObjectType);
                        if (transferAmount > maxTransferAmount)
                            transferAmount = maxTransferAmount;
                        
                        capacity = TransferTileObjects(otherUnit, changedUnits, extractedItems, pullItemOrder, transferAmount, excludeTileObjects);
                    }
                }
            }
            return capacity;
        }

        private int BalanceWithOtherContainer(Unit unit, Unit otherUnit, Dictionary<Position2, Unit> changedUnits,
            List<MoveRecipeIngredient> extractedItems, UnitItemOrder pullItemOrder, int capacity)
        {

            List<TileObject> excludeTileObjects = new List<TileObject>();
            foreach (UnitItemOrder unitItemOrder in otherUnit.UnitOrders.unitItemOrders)
            {
                if (unitItemOrder.TileObjectType == pullItemOrder.TileObjectType)
                {
                    if (unitItemOrder.TileObjectState == TileObjectState.Deny)
                    {
                        // Transfer all 
                        capacity = TransferTileObjects(otherUnit, changedUnits, extractedItems, pullItemOrder, capacity, excludeTileObjects);
                    }
                    else if (unitItemOrder.TileObjectState == TileObjectState.None ||
                        unitItemOrder.TileObjectState == pullItemOrder.TileObjectState)
                    {
                        // Balance the content
                        int countInUnit = unit.CountTileObjectsInContainer(pullItemOrder.TileObjectType);
                        int countInOtherUnit = otherUnit.CountTileObjectsInContainer(pullItemOrder.TileObjectType);

                        if (countInOtherUnit > countInUnit)
                        {
                            int transfer = ((countInOtherUnit + countInUnit) / 2) - countInUnit;
                            if (transfer > capacity)
                                transfer = capacity;
                            if (transfer > 0)
                            {
                                capacity = TransferTileObjects(otherUnit, changedUnits, extractedItems, pullItemOrder, transfer, excludeTileObjects);
                            }
                        }
                    }
                }
            }
            return capacity;
        }

        private int ExtractFromOtherContainer(Unit unit, Unit otherUnit, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, int capacity, bool forceExtract = false)
        {
            foreach (UnitItemOrder unitItemOrder in unit.UnitOrders.unitItemOrders)
            {
                if (unitItemOrder.TileObjectState == TileObjectState.None)
                {
                    if (unit.CurrentGameCommand != null && unit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest ||
                        unit.CurrentGameCommand != null && unit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        capacity = PullFromOtherContainer(unit, otherUnit, changedUnits, extractedItems, unitItemOrder, capacity, forceExtract);
                    }
                    else
                    {
                        // Dont care
                        if (otherUnit.Engine != null)
                        {
                            // Pull everything from a worker
                            capacity = PullFromOtherContainer(unit, otherUnit, changedUnits, extractedItems, unitItemOrder, capacity);
                        }
                        else if (unit.Engine == null && otherUnit.Engine == null)
                        {
                            // Share with other container if both 
                            capacity = BalanceWithOtherContainer(unit, otherUnit, changedUnits, extractedItems, unitItemOrder, capacity);
                        }
                    }
                }
                if (unitItemOrder.TileObjectState == TileObjectState.Deny)
                {
                    // Push it to other container
                    continue;
                }
                if (unitItemOrder.TileObjectState == TileObjectState.Accept)
                {
                    // Pull it from other container
                    capacity = PullFromOtherContainer(unit, otherUnit, changedUnits, extractedItems, unitItemOrder, capacity);
                }
            }
            return capacity;
        }
        /*
        private int TransferObjects(int transferMinerals, TileObjectType tileObjectType, Unit otherUnit, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, int capacity)
        { 
            List<TileObject> excludeTileObjects = new List<TileObject>();
            while (transferMinerals > 0 && capacity > 0)
            {
                MoveRecipeIngredient realIndigrient = null;
                realIndigrient = otherUnit.FindIngredient(tileObjectType, false, excludeTileObjects);
                if (realIndigrient == null) break;

                // Avoid the bug where the transfer fills a container with mins that would accept wood and the remaining wood is not accepted
                if (!Unit.AcceptsIngredient(realIndigrient))
                    break;

                otherUnit.ConsumeIngredient(realIndigrient, changedUnits);
                capacity--;
                transferMinerals--;

                Unit.AddIngredient(realIndigrient);

                if (!changedUnits.ContainsKey(Unit.Pos))
                    changedUnits.Add(Unit.Pos, Unit);

                realIndigrient.TargetPosition = Unit.Pos;
                extractedItems.Add(realIndigrient);
            }
            return capacity;
        }
        
        private int ExtractFromOtherUnit(Unit unit, Unit otherUnit, TileObjectType tileObjectType, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, int capacity, int max)
        {
            List<TileObject> excludeTileObjects = new List<TileObject>();
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
                    realIndigrient = otherUnit.FindIngredient(TileObjectType.All, extractNeighbors, excludeTileObjects);
                if (tileObjectType == TileObjectType.Burn)
                    realIndigrient = otherUnit.FindIngredientToBurn(unit);
                if (tileObjectType == TileObjectType.Ammo)
                    realIndigrient = otherUnit.FindIngredientForAmmo(unit);

                if (realIndigrient == null) break;
                if (!Unit.IsSpaceForIngredient(realIndigrient))
                {
                    //otherUnit.ReserveIngredient(realIndigrient);
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
            //otherUnit.ClearReservations();
            return capacity;
        }
        
        private int PickFromContainer(Unit unit, Unit otherUnit, Dictionary<Position2, Unit> changedUnits, List<MoveRecipeIngredient> extractedItems, int capacity)
        {
            foreach (RecipeIngredient moveRecipeIngredient in unit.CurrentGameCommand.GameCommand.RequestedItems)
            {
                if (moveRecipeIngredient.TileObjectType == TileObjectType.Ammo)
                {
                    ExtractFromOtherUnit(unit, otherUnit, TileObjectType.Ammo, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);

                }
                else if (moveRecipeIngredient.TileObjectType == TileObjectType.Burn)
                {
                    ExtractFromOtherUnit(unit, otherUnit, TileObjectType.Ammo, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                    
                }
                else
                {
                    ExtractFromOtherUnit(unit, otherUnit, TileObjectType.All, changedUnits, extractedItems, capacity, moveRecipeIngredient.Count);
                    
                }
            }

            return capacity;
        }*/
    }
}
