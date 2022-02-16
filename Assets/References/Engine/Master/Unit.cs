using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Master
{
    // Rectangle
    // 0 Points in Engine: Cannot move
    // 1 Points in Engine: Move slow
    // 2 Points in Engine: Move fast
    // 3 Points in Engine: Move very fast

    // Halfcircle?
    // 0 Points in Armor: None
    // 1 Points in Armor: Armor 1
    // 2 Points in Armor: Armor 2
    // 3 Points in Armor: Armor 3

    // Rectangle + Gun
    // 0 Points in Weapon: Cannot fire
    // 1 Points in Weapon: Damage 1
    // 2 Points in Weapon: Damage 2
    // 3 Points in Weapon: Damage 3

    // Hex
    // 0 Points in Assembler: None
    // 1 Points in Assembler: Produce Level 1 Items
    // 2 Points in Assembler: Produce Level 1 Items + Upgrade
    // 3 Points in Assembler: Produce Level 1 Items + Upgrade + Faster

    // Triangle on moving units, Hex on Ground on buildings
    // Extract from Enemy if Next, Ground with Range, from own Unit to destroy
    // 0 Points in Extractor: None
    // 1 Points in Extractor: Range 1
    // 2 Points in Extractor: Range 2
    // 3 Points in Extractor: Range 3

    // Circle (12 corners)
    // 0 Points in Container: None
    // 1 Points in Container: Storage 1
    // 2 Points in Container: Storage 2
    // 3 Points in Container: Storage 3

    // Square
    // 0 Points in Reactor: None
    // 1 Points in Reactor: Range 1 Efficiency
    // 2 Points in Reactor: Range 2
    // 3 Points in Reactor: Range 3

    // Turning rectangle
    // 0 Points in Radar: None
    // 1 Points in Radar: Range 1
    // 2 Points in Radar: Range 2
    // 3 Points in Radar: Range 3




    // Total Points: 4 increase with xp
    public class UnitItemOrder
    {
        public TileObjectType TileObjectType { get; set; }
        public TileObjectState TileObjectState { get; set; }
    }

    public class UnitOrders
    {
        public UnitOrders()
        {
            unitItemOrders = new List<UnitItemOrder>();
        }
        public List<UnitItemOrder> unitItemOrders { get; set; }

        public static int GetAcceptedAmount(Unit unit, TileObjectType tileObjectType)
        {
            int maxTransferAmount;
            int numberOfRequests = 0;

            foreach (UnitItemOrder targetUnitItemOrder in unit.UnitOrders.unitItemOrders)
            {
                if (targetUnitItemOrder.TileObjectState == TileObjectState.Accept)
                    numberOfRequests++;

                if (targetUnitItemOrder.TileObjectType == tileObjectType &&
                    targetUnitItemOrder.TileObjectState == TileObjectState.Deny)
                {
                    return 0;
                }
            }
            int countInUnit = unit.CountTileObjectsInContainer(tileObjectType);
            if (unit.Container != null)
            {
                if (numberOfRequests > 0)
                    maxTransferAmount = (unit.Container.TileContainer.Capacity / numberOfRequests) - countInUnit;
                else
                    maxTransferAmount = unit.Container.TileContainer.Capacity - countInUnit;
            }
            else if (unit.Weapon != null)
            {
                maxTransferAmount = unit.Weapon.TileContainer.Capacity - countInUnit;
            }
            else
            {
                maxTransferAmount = 0;
            }
            if (maxTransferAmount < 0)
                maxTransferAmount = 0;
            return maxTransferAmount;
        }

        

    }

    [DataContract]
    public class Unit
    {
        public Unit(Game game, string startCode)
        {
            Game = game;
            Pos = Position2.Null;
            Power = 20;
            MaxPower = 20;
            Blueprint = game.Blueprints.FindBlueprint(startCode);
            if (Blueprint != null)
            {
                UnitId = game.GetNextUnitId("unit");
                UnderConstruction = true;

                if (Blueprint.BlueprintUnitOrders != null)
                {
                    UnitOrders = new UnitOrders();
                    foreach (BlueprintUnitItemOrder blueprintUnitItemOrder in Blueprint.BlueprintUnitOrders.BlueprintItemOrders)
                    {
                        UnitItemOrder unitItemOrder = new UnitItemOrder();
                        unitItemOrder.TileObjectType = blueprintUnitItemOrder.TileObjectType;
                        unitItemOrder.TileObjectState = blueprintUnitItemOrder.TileObjectState;
                        UnitOrders.unitItemOrders.Add(unitItemOrder);
                    }
                }
            }
        }

        private Position2 pos;

        public Position2 Pos
        {
            get
            {
                return pos;
            }
            set
            {
                pos = value;
            }
        }

        public Player Owner { get; set; }

        public Weapon Weapon { get; set; }
        public Engine Engine { get; set; }
        public Armor Armor { get; set; }
        public Assembler Assembler { get; set; }
        public Extractor Extractor { get; set; }
        public Container Container { get; set; }
        public Reactor Reactor { get; set; }
        public Radar Radar { get; set; }

        public UnitOrders UnitOrders { get; set; }
        public string UnitId { get; set; }

        internal GameCommand CurrentGameCommand { get; private set; }

        internal void SetTempGameCommand(GameCommand gameCommand)
        {
            CurrentGameCommand = gameCommand;
        }        
        internal void SetGameCommand(GameCommand gameCommand)
        {
            if (gameCommand != null && gameCommand.BuildPositionReached)
            {
                if (gameCommand.FactoryUnit.UnitId == UnitId)
                    gameCommand.BuildPositionReached = false;
            }
            CurrentGameCommand = gameCommand;
            Changed = true;
        }

        public void OnDestroyed()
        {
            ResetGameCommand();
        }
        public void ResetGameCommandOnly()
        {
            Changed = true;
            CurrentGameCommand = null;
        }
        public void ResetGameCommand()
        {
            Changed = true;
            if (CurrentGameCommand != null)
            {
                if (Owner.PlayerModel.Id != 0)
                {
                    Player player = Game.Players[Owner.PlayerModel.Id];
                    foreach (GameCommand gameCommand in player.GameCommands)
                    {

                        if (gameCommand == CurrentGameCommand)
                            continue;

                        if (gameCommand.AttachedUnit.UnitId == UnitId)
                        {
                            gameCommand.AttachedUnit.ResetStatus();
                            gameCommand.AttachedUnit.ResetUnitId();
                        }
                        if (gameCommand.TransportUnit.UnitId == UnitId)
                        {
                            gameCommand.TransportUnit.ResetStatus();
                            gameCommand.TransportUnit.ResetUnitId();
                        }
                        if (gameCommand.TargetUnit.UnitId == UnitId)
                        {
                            gameCommand.TargetUnit.ResetStatus();
                            gameCommand.TargetUnit.ResetUnitId();
                        }
                        if (gameCommand.FactoryUnit.UnitId == UnitId)
                        {
                            gameCommand.FactoryUnit.ResetStatus();
                            gameCommand.FactoryUnit.ResetUnitId();
                        }

                    }
                }

                if (CurrentGameCommand.AttachedUnit.UnitId == UnitId)
                {
                    if (CurrentGameCommand.DeleteWhenDestroyed)
                    {
                        CurrentGameCommand.AttachedUnit.SetStatus("DeleteBecauseDestroyed");                       
                    }
                    else
                    {
                        CurrentGameCommand.AttachedUnit.SetStatus("Removed: " + UnitId);
                    }
                    CurrentGameCommand.AttachedUnit.ResetUnitId();
                }
                if (CurrentGameCommand.TransportUnit.UnitId == UnitId)
                {
                    CurrentGameCommand.TransportUnit.ResetStatus();
                    CurrentGameCommand.TransportUnit.ResetUnitId();
                }
                if (CurrentGameCommand.FactoryUnit.UnitId == UnitId)
                {
                    CurrentGameCommand.FactoryUnit.ResetStatus();
                    CurrentGameCommand.FactoryUnit.ResetUnitId();
                }
                if (CurrentGameCommand.TargetUnit.UnitId == UnitId)
                {
                    CurrentGameCommand.TargetUnit.ResetStatus();
                    CurrentGameCommand.TargetUnit.ResetUnitId();
                }
                Changed = true;
                CurrentGameCommand = null;
            }
        }

        public int PrevPower { get; set; }
        public int Power { get; set; }
        public bool EndlessPower { get; set; }
        public bool Changed { get; set; }
        public int MaxPower { get; set; }

        // Unit can be extracted
        public bool ExtractMe { 
            get; private set;
            }

        public void ExtractUnit()
        {
            if (Armor != null)
            {
                Armor.RemoveShield();
            }
            //ResetGameCommand();
            CurrentGameCommand = null;
            ExtractMe = true;
        }

        public bool UnderConstruction { get; set; }

        public bool BuilderWaitForMetal { get; set; }

        public Direction Direction { get; set; }
        public Game Game { get; set; }

        public Blueprint Blueprint { get; set; }

        public int Stunned { get; set; }

        public TileObject FindAmmoTileObject(TileContainer tileContainer)
        {
            TileObject bestTileObject = null;
            int bestscore = 0;

            foreach (TileObject tileObject in tileContainer.TileObjects)
            {
                int score = TileObject.GetDeliveryScoreForAmmoType(tileObject.TileObjectType);
                if (score > bestscore || bestTileObject == null)
                {
                    bestTileObject = tileObject;
                    bestscore = score;
                }
            }
            
            return bestTileObject;
        }

        public MoveRecipeIngredient FindRefillAmmo(bool searchNeighbors = true)
        {
            // Near transport, possible with extractor, prefer external ammo source
            MoveRecipeIngredient moveRecipeIngredient;
            if (searchNeighbors && Extractor != null)
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    Tile t = Game.Map.GetTile(n3.Pos);
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id)
                    {
                        moveRecipeIngredient = t.Unit.FindRefillAmmo(false);
                        if (moveRecipeIngredient != null)
                            return moveRecipeIngredient;
                    }
                }
            }
            // Extract ammo from container
            moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.Count = 1;

            if (Container != null && Container.TileContainer != null)
            {
                TileObject tileObject = FindAmmoTileObject(Container.TileContainer);

                //TileObject tileObject = Container.TileContainer.GetMatchingTileObject(TileObjectType.Ammo, null);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = TileObjectType.PartContainer;
                    return moveRecipeIngredient;
                }
            }
            return null;
        }

        public void DeliveryRequest(TileObjectType tileObjectType, int capacity)
        {
            foreach (GameCommand gameCommand1 in Owner.GameCommands)
            {
                if (gameCommand1.TargetPosition == Pos &&
                    gameCommand1.GameCommandType == GameCommandType.ItemRequest)
                {
                    // Already requested
                    return;
                }
            }

            GameCommand gameCommand = new GameCommand();
            gameCommand.GameCommandType = GameCommandType.ItemRequest;
            gameCommand.Layout = "UIDelivery";
            gameCommand.TargetPosition = Pos;
            gameCommand.DeleteWhenFinished = true;
            gameCommand.PlayerId = Owner.PlayerModel.Id;
            gameCommand.TargetUnit.SetUnitId(UnitId);
            gameCommand.TargetUnit.SetStatus(Blueprint.Name + " WaitingForDelivery");
            Changed = true;

            /*
            gameCommand.RequestedItems = new List<RecipeIngredient>();
            RecipeIngredient recipeIngredient = new RecipeIngredient(tileObjectType, capacity);
            gameCommand.RequestedItems.Add(recipeIngredient);*/

            SetGameCommand(gameCommand);

            Owner.GameCommands.Add(gameCommand);
        }

        public void FillWithTileObjects(TileObjectType tileObjectType, int count)
        {
            for (int i = 0; i < count; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = tileObjectType;
                tileObject.Direction = Direction.C;

                if (Container != null && Container.TileContainer.IsFreeSpace)
                {
                    Container.TileContainer.Add(tileObject);
                }
                if (Assembler != null && Assembler.TileContainer.IsFreeSpace)
                {
                    Container.TileContainer.Add(tileObject);
                }
                if (Reactor != null && Reactor.TileContainer.IsFreeSpace)
                {
                    Reactor.TileContainer.Add(tileObject);
                }
                if (Weapon != null && Weapon.TileContainer.IsFreeSpace)
                {
                    Weapon.TileContainer.Add(tileObject);
                }
            }
        }

        public MoveRecipeIngredient GetConsumableIngredient(TileObjectType tileObjectType, bool searchNeighbors)
        {
            MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.TileObjectType = tileObjectType;
            moveRecipeIngredient.Count = 1;

            if (Assembler != null && Assembler.TileContainer != null)
            {
                TileObject tileObject = Assembler.TileContainer.GetMatchingTileObject(tileObjectType, null);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = TileObjectType.PartAssembler;
                    return moveRecipeIngredient;
                }
            }
            if (Container != null && Container.TileContainer != null)
            {
                TileObject tileObject = Container.TileContainer.GetMatchingTileObject(tileObjectType, null);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = TileObjectType.PartContainer;
                    return moveRecipeIngredient;
                }
            }
            // Do not pick ingredients from weapon or reactor

            // Near transport, possible with extractor
            if (searchNeighbors && Extractor != null)
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    Tile t = Game.Map.GetTile(n3.Pos);
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id)
                    {
                        moveRecipeIngredient = t.Unit.GetConsumableIngredient(tileObjectType, false);
                        if (moveRecipeIngredient != null)
                            return moveRecipeIngredient;
                    }
                }
            }
            return null;
        }

        public bool AreAllIngredientsAvailable(List<RecipeIngredient> ingredients)
        {
            List<TileObject> foundIngredients = new List<TileObject>();
            bool allFound = true;

            foreach (RecipeIngredient recipeIngredient in ingredients)
            {
                int count = recipeIngredient.Count;
                while (count-- > 0)
                {
                    MoveRecipeIngredient moveRecipeIngredient;
                    moveRecipeIngredient = FindIngredient(recipeIngredient.TileObjectType, true, foundIngredients);
                    if (moveRecipeIngredient == null)
                    {
                        allFound = false;
                        break;
                    }
                    //ReserveIngredient(moveRecipeIngredient);
                    //foundIngredients.Add(moveRecipeIngredient);
                }
            }
            /*
            if (allFound == false)
            {
                foreach (MoveRecipeIngredient moveRecipeIngredient in reservedIngredients)
                {
                    //ReleaseReservedIngredient(moveRecipeIngredient);
                }
            }*/
            return allFound;
        }
        
        private void CollectBurnableIngredientsFromContainer (List<MoveRecipeIngredient> allIngredients, TileContainer tileContainer, TileObjectType sourceContainerType)
        {
            foreach (TileObject tileObject in tileContainer.TileObjects)
            {
                if (TileObject.GetPowerForTileObjectType(tileObject.TileObjectType) > 0)
                {
                    MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = sourceContainerType;
                    moveRecipeIngredient.Count = 1;
                    allIngredients.Add(moveRecipeIngredient);
                }
            }
        }
                
        private void CollectAmmoIngredientsFromContainer(List<MoveRecipeIngredient> allIngredients, TileContainer tileContainer, TileObjectType sourceContainerType)
        {
            foreach (TileObject tileObject in tileContainer.TileObjects)
            {
                if (TileObject.IsAmmo(tileObject.TileObjectType))
                {
                    MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = sourceContainerType;
                    moveRecipeIngredient.Count = 1;
                    allIngredients.Add(moveRecipeIngredient);
                }
            }
        }

        private void CollectBurnableIngredients(List<MoveRecipeIngredient> allIngredients)
        {
            if (Container != null && Container.TileContainer != null)
            {
                CollectBurnableIngredientsFromContainer(allIngredients, Container.TileContainer, TileObjectType.PartContainer);
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                CollectBurnableIngredientsFromContainer(allIngredients, Assembler.TileContainer, TileObjectType.PartAssembler);
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                CollectBurnableIngredientsFromContainer(allIngredients, Weapon.TileContainer, TileObjectType.PartWeapon);
            }
            if (Reactor != null && Reactor.TileContainer != null)
            {
                CollectBurnableIngredientsFromContainer(allIngredients, Reactor.TileContainer, TileObjectType.PartReactor);
            }
        }
        public MoveRecipeIngredient FindIngredientToBurn(Unit excludeUnit)
        {
            List<MoveRecipeIngredient> allIngredients = new List<MoveRecipeIngredient>();

            CollectBurnableIngredients(allIngredients);

            // Near transport, possible with extractor
            if (Extractor != null)
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    Tile t = Game.Map.GetTile(n3.Pos);
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id && t.Unit != excludeUnit)
                    {
                        t.Unit.CollectBurnableIngredients(allIngredients);
                    }
                }
            }
            // Find best ingredient
            MoveRecipeIngredient bestIngredient = null;
            int bestScore = 0;

            foreach (MoveRecipeIngredient moveRecipeIngredient in allIngredients)
            {
                int currentScore;

                // Prefer anything but minerals
                currentScore = TileObject.GetDeliveryScoreForBurnType(moveRecipeIngredient.TileObjectType);
                if (currentScore == 0) 
                    continue;
                /*
                if (moveRecipeIngredient.TileObjectType == TileObjectType.Mineral)
                    currentScore += 20;
                else if (moveRecipeIngredient.TileObjectType == TileObjectType.Tree)
                    currentScore += 10;
                else if (moveRecipeIngredient.TileObjectType == TileObjectType.Bush)
                    currentScore += 10;
                */
                if (moveRecipeIngredient.SourcePosition != Pos)
                {
                    if (moveRecipeIngredient.Source == TileObjectType.PartContainer)
                    {
                        // From neighbor
                        currentScore += 10;
                        
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartAssembler)
                    {
                        currentScore += 5;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartWeapon)
                    {
                        currentScore += 4;
                    }
                }
                else
                {
                    if (moveRecipeIngredient.Source == TileObjectType.PartContainer)
                    {
                        // Own container
                        currentScore += 90;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartAssembler)
                    {
                        currentScore += 8;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartWeapon)
                    {
                        currentScore += 7;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartReactor)
                    {
                        // Use own stuff
                        currentScore += 1;
                    }
                }
                if (bestIngredient == null || currentScore > bestScore)
                {
                    bestIngredient = moveRecipeIngredient;
                    bestScore = currentScore;
                }
            }
            return bestIngredient;
        }
        private void CollectAmmoIngredients(List<MoveRecipeIngredient> allIngredients)
        {
            if (Container != null && Container.TileContainer != null)
            {
                CollectAmmoIngredientsFromContainer(allIngredients, Container.TileContainer, TileObjectType.PartContainer);
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                CollectAmmoIngredientsFromContainer(allIngredients, Assembler.TileContainer, TileObjectType.PartAssembler);
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                CollectAmmoIngredientsFromContainer(allIngredients, Weapon.TileContainer, TileObjectType.PartWeapon);
            }
            if (Reactor != null && Reactor.TileContainer != null)
            {
                CollectAmmoIngredientsFromContainer(allIngredients, Reactor.TileContainer, TileObjectType.PartReactor);
            }
        }
        public MoveRecipeIngredient FindIngredientForAmmo(Unit excludeUnit)
        {
            List<MoveRecipeIngredient> allIngredients = new List<MoveRecipeIngredient>();

            CollectAmmoIngredients(allIngredients);

            // Near transport, possible with extractor
            if (Extractor != null)
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    Tile t = Game.Map.GetTile(n3.Pos);
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id && t.Unit != excludeUnit)
                    {
                        t.Unit.CollectAmmoIngredients(allIngredients);
                    }
                }
            }
            // Find best ingredient
            MoveRecipeIngredient bestIngredient = null;
            int bestScore = 0;

            foreach (MoveRecipeIngredient moveRecipeIngredient in allIngredients)
            {
                int currentScore = 0;

                // Prefer anything but minerals
                if (moveRecipeIngredient.TileObjectType == TileObjectType.Mineral)
                    currentScore += 10;
                else
                    currentScore += TileObject.GetDeliveryScoreForAmmoType(moveRecipeIngredient.TileObjectType);

                if (moveRecipeIngredient.SourcePosition != Pos)
                {
                    if (moveRecipeIngredient.Source == TileObjectType.PartContainer)
                    {
                        // From neighbor
                        currentScore += 10;

                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartAssembler)
                    {
                        currentScore += 5;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartWeapon)
                    {
                        currentScore += 4;
                    }
                }
                else
                {
                    if (moveRecipeIngredient.Source == TileObjectType.PartContainer)
                    {
                        // Own container
                        currentScore += 90;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartAssembler)
                    {
                        currentScore += 8;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartWeapon)
                    {
                        currentScore += 7;
                    }
                    if (moveRecipeIngredient.Source == TileObjectType.PartReactor)
                    {
                        // Use own stuff
                        currentScore += 1;
                    }
                }
                if (bestIngredient == null || currentScore > bestScore)
                {
                    bestIngredient = moveRecipeIngredient;
                    bestScore = currentScore;
                }
            }
            return bestIngredient;
        }

        public MoveRecipeIngredient FindIngredient(TileObjectType tileObjectType, bool searchNeighbors, List<TileObject> excludeIngredients)
        {
            MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.TileObjectType = tileObjectType;
            moveRecipeIngredient.Count = 1;

            if (Container != null && Container.TileContainer != null)
            {
                TileObject tileObject = Container.TileContainer.GetMatchingTileObject(tileObjectType, excludeIngredients);
                if (tileObject != null)
                {
                    if (excludeIngredients != null)
                        excludeIngredients.Add(tileObject);

                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = TileObjectType.PartContainer;
                    return moveRecipeIngredient;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                TileObject tileObject = Assembler.TileContainer.GetMatchingTileObject(tileObjectType, excludeIngredients);
                if (tileObject != null)
                {
                    if (excludeIngredients != null)
                        excludeIngredients.Add(tileObject);

                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.SourceUnitId = UnitId;
                    moveRecipeIngredient.Source = TileObjectType.PartAssembler;
                    return moveRecipeIngredient;
                }

            }

            // Do not pick ingredients from weapon or reactor

            // Near transport, possible with extractor
            if (searchNeighbors && Extractor != null)
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    Tile t = Game.Map.GetTile(n3.Pos);
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id)
                    {
                        moveRecipeIngredient = t.Unit.FindIngredient(tileObjectType, false, excludeIngredients);
                        if (moveRecipeIngredient != null)
                            return moveRecipeIngredient;
                    }
                }
            }
            return null;
        }

        public bool AcceptsIngredient(MoveRecipeIngredient ingredient)
        {
            TileObject tileObject = new TileObject();
            tileObject.TileObjectType = ingredient.TileObjectType;
            tileObject.TileObjectKind = ingredient.TileObjectKind;
            tileObject.Direction = Direction.C;

            int asdfgsdgfsdfg = 0;
            /*
            if (Reactor != null && Reactor.TileContainer != null &&
                Reactor.TileContainer.Count < Reactor.TileContainer.Capacity &&
                Reactor.TileContainer.Accepts(tileObject))
            {
                return true;
            }
            else if (Assembler != null && Assembler.TileContainer != null &&
                Assembler.TileContainer.Count < Assembler.TileContainer.Capacity &&
                Assembler.TileContainer.Accepts(tileObject))
            {
                return true;
            }
            else if (Container != null && Container.TileContainer != null &&
                Container.TileContainer.Count < Container.TileContainer.Capacity &&
                Container.TileContainer.Accepts(tileObject))
            {
                return true;
            }
            else if (Weapon != null && Weapon.TileContainer != null &&
                Weapon.TileContainer.Count < Weapon.TileContainer.Capacity &&
                Weapon.TileContainer.Accepts(tileObject))
            {
                return true;
            }*/
            return false;
        }

        public void AddIngredient(MoveRecipeIngredient ingredient)
        {
            TileObject tileObject = new TileObject();
            tileObject.TileObjectType = ingredient.TileObjectType;
            tileObject.TileObjectKind = ingredient.TileObjectKind;
            tileObject.Direction = Direction.C;

            if (Reactor != null && Reactor.TileContainer != null &&
                Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
            {
                ingredient.Target = TileObjectType.PartReactor;
                Reactor.TileContainer.Add(tileObject);
            }
            else if (Assembler != null && Assembler.TileContainer != null &&
                Assembler.TileContainer.Count < Assembler.TileContainer.Capacity)
            {
                ingredient.Target = TileObjectType.PartAssembler;
                Assembler.TileContainer.Add(tileObject);
            }
            else if (Container != null && Container.TileContainer != null &&
                Container.TileContainer.Count < Container.TileContainer.Capacity)
            {
                ingredient.Target = TileObjectType.PartContainer;
                Container.TileContainer.Add(tileObject);
            }
            else if (Weapon != null && Weapon.TileContainer != null &&
                Weapon.TileContainer.Count < Weapon.TileContainer.Capacity)
            {
                ingredient.Target = TileObjectType.PartWeapon;
                Weapon.TileContainer.Add(tileObject);
            }
            else
            {
                throw new Exception("no space");
            }
        }

        private TileObject RemovePart(TileObjectType tileObjectType)
        {
            TileObject tileObject = null;
            if (tileObjectType == TileObjectType.PartEngine)
            {
                tileObject = Engine.PartTileObjects[0];
                Engine.PartTileObjects.RemoveAt(0);
                Engine.Level--;
                if (Engine.Level == 0)
                    Engine = null;
            }
            if (tileObjectType == TileObjectType.PartContainer)
            {
                tileObject = Container.PartTileObjects[0];
                Container.PartTileObjects.RemoveAt(0);
                Container.Level--;
                if (Container.Level == 0)
                    Container = null;
            }
            if (tileObjectType == TileObjectType.PartExtractor)
            {
                tileObject = Extractor.PartTileObjects[0];
                Extractor.PartTileObjects.RemoveAt(0);
                Extractor.Level--;
                if (Extractor.Level == 0)
                    Extractor = null;
            }
            if (tileObjectType == TileObjectType.PartArmor)
            {
                tileObject = Armor.PartTileObjects[0];
                Armor.PartTileObjects.RemoveAt(0);
                Armor.Level--;
                if (Armor.Level == 0)
                    Armor = null;
            }
            if (tileObjectType == TileObjectType.PartAssembler)
            {
                tileObject = Assembler.PartTileObjects[0];
                Assembler.PartTileObjects.RemoveAt(0);
                Assembler.Level--;
                if (Assembler.Level == 0)
                    Assembler = null;
            }
            if (tileObjectType == TileObjectType.PartReactor)
            {
                tileObject = Reactor.PartTileObjects[0];
                Reactor.PartTileObjects.RemoveAt(0);
                Reactor.Level--;
                if (Reactor.Level == 0)
                    Reactor = null;
            }
            if (tileObjectType == TileObjectType.PartRadar)
            {
                tileObject = Radar.PartTileObjects[0];
                Radar.PartTileObjects.RemoveAt(0);
                Radar.Level--;
                if (Radar.Level == 0)
                    Radar = null;
            }
            CountParts();
            return tileObject;
        }

        public TileObject ConsumeIngredient(MoveRecipeIngredient ingredient, Dictionary<Position2, Unit> changedUnits)
        {
            TileObject tileObject = null;
            if (ingredient.SourcePosition == Pos)
            {
                if (TileObject.CanConvertTileObjectIntoMineral(ingredient.TileObjectType))
                {
                    tileObject = RemovePart(ingredient.TileObjectType);
                }
                else
                {
                    if (ingredient.Source == TileObjectType.PartAssembler && Assembler != null && Assembler.TileContainer != null)
                    {
                        tileObject = Assembler.TileContainer.GetMatchingTileObject(ingredient.TileObjectType, null);
                        if (tileObject != null)
                            Assembler.TileContainer.Remove(tileObject);
                    }
                    if (ingredient.Source == TileObjectType.PartContainer && Container != null && Container.TileContainer != null)
                    {
                        tileObject = Container.TileContainer.GetMatchingTileObject(ingredient.TileObjectType, null);
                        if (tileObject != null)
                            Container.TileContainer.Remove(tileObject);
                    }
                    if (ingredient.Source == TileObjectType.PartReactor && Reactor != null && Reactor.TileContainer != null)
                    {
                        tileObject = Reactor.TileContainer.GetMatchingTileObject(ingredient.TileObjectType, null);
                        if (tileObject != null)
                            Reactor.TileContainer.Remove(tileObject);
                    }
                    if (ingredient.Source == TileObjectType.PartWeapon && Weapon != null && Weapon.TileContainer != null)
                    {
                        tileObject = Weapon.TileContainer.GetMatchingTileObject(ingredient.TileObjectType, null);
                        if (tileObject != null)
                            Weapon.TileContainer.Remove(tileObject);
                    }
                }
                if (tileObject == null)
                {
                    throw new Exception("Consume failed");
                }

                // Take it from container, whatever
                /*
                if (tileObject == null && Container != null && Container.TileContainer != null)
                {
                    tileObject = Container.TileContainer.GetMatchingTileObject(ingredient.TileObjectType);
                    if (tileObject != null)
                        Container.TileContainer.Remove(tileObject);
                }*/
                if (changedUnits != null && !changedUnits.ContainsKey(Pos))
                    changedUnits.Add(Pos, this);
            }
            else
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    if (n3.Pos == ingredient.SourcePosition)
                    {
                        Tile t = Game.Map.GetTile(n3.Pos);
                        if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id)
                        {
                            tileObject = t.Unit.ConsumeIngredient(ingredient, changedUnits);
                        }
                        break;
                    }
                }
            }
            return tileObject;
        }

        public int CountMineral()
        {
            int mineral = 0;

            if (Container != null)
            {
                mineral += Container.TileContainer.Minerals;
                mineral += Container.PartTileObjects.Count;
            }

            // Every part is one metal
            if (Engine != null) 
                mineral += Engine.PartTileObjects.Count;
            if (Armor != null) 
                mineral += Armor.PartTileObjects.Count;
            if (Weapon != null)
            {
                if (Weapon.TileContainer != null)
                    mineral += Weapon.TileContainer.Minerals;
                mineral += Weapon.PartTileObjects.Count;
            }
            if (Assembler != null)
            {
                if (Assembler.TileContainer != null)
                    mineral += Assembler.TileContainer.Minerals;
                mineral += Assembler.PartTileObjects.Count;
            }
            if (Extractor != null)
                mineral += Extractor.PartTileObjects.Count;
            if (Reactor != null)
            {
                if (Reactor.TileContainer != null)
                    mineral += Reactor.TileContainer.Minerals;
                mineral+= Reactor.PartTileObjects.Count;
            }
            if (Radar != null)
                mineral+= Radar.PartTileObjects.Count;

            return mineral;
        }

        public int CountMineralsInContainer()
        {
            int metal = 0;
            if (Engine == null)
            {
                if (Container != null) 
                    metal += Container.TileContainer.Minerals;

                if (Weapon != null)
                {
                    if (Weapon.TileContainer != null)
                        metal += Weapon.TileContainer.Minerals;
                }
                if (Assembler != null)
                {
                    if (Assembler.TileContainer != null)
                        metal += Assembler.TileContainer.Minerals;
                }
                if (Reactor != null)
                {
                    if (Reactor.TileContainer != null)
                        metal += Reactor.TileContainer.Minerals;
                }
            }
            return metal;
        }

        public int CountTileObjectsInContainer(TileObjectType tileObjectType)
        {
            int count = 0;

            if (Container != null)
            {
                count += Container.TileContainer.CountTileObjects(tileObjectType);
            }
            if (Weapon != null)
            {
                if (Weapon.TileContainer != null)
                    count += Weapon.TileContainer.CountTileObjects(tileObjectType);
            }
            if (Assembler != null)
            {
                if (Assembler.TileContainer != null)
                    count += Assembler.TileContainer.CountTileObjects(tileObjectType);
            }
            if (Reactor != null)
            {
                if (Reactor.TileContainer != null)
                    count += Reactor.TileContainer.CountTileObjects(tileObjectType);
            }

            return count;
        }

        public int CountCapacity()
        {
            int capacity = 0;
            if (Container != null) 
                capacity += Container.TileContainer.Capacity;

            if (Weapon != null)
            {
                if (Weapon.TileContainer != null)
                    capacity += Weapon.TileContainer.Capacity;
            }
            if (Assembler != null)
            {
                if (Assembler.TileContainer != null)
                    capacity += Assembler.TileContainer.Capacity;
            }
            if (Reactor != null)
            {
                if (Reactor.TileContainer != null)
                    capacity += Reactor.TileContainer.Capacity;
            }
            
            return capacity;
        }

        private int numberOfParts;

        public int CountParts()
        {
            numberOfParts = 0;

            if (Engine != null) numberOfParts += Engine.Level;
            if (Armor != null) numberOfParts += Armor.Level;
            if (Weapon != null) numberOfParts += Weapon.Level;
            if (Assembler != null) numberOfParts += Assembler.Level;
            if (Extractor != null) numberOfParts += Extractor.Level;
            if (Container != null) numberOfParts += Container.Level;
            if (Reactor != null) numberOfParts += Reactor.Level;
            if (Radar != null) numberOfParts += Radar.Level;

            return numberOfParts;
        }

        public bool IsComplete()
        {
            return numberOfParts >= 4;
        }

        public bool HasParts()
        {
            if (Engine != null) return false;
            if (Armor != null) return false;
            if (Weapon != null) return false;
            if (Assembler != null) return false;
            if (Extractor != null) return false;
            if (Container != null) return false;
            if (Reactor != null) return false;
            if (Radar != null) return false;

            return true;
        }

        private Ability CreateBlueprintPart(BlueprintPart blueprintPart, bool fillContainer, TileObject tileObject)
        {
            int level = blueprintPart.Level;

            Ability createdAbility = null;
            if (blueprintPart.PartType == TileObjectType.PartEngine)
            {
                if (Engine == null)
                {
                    Engine = new Engine(this, 1);
                }
                else if (level > Engine.Level)
                    Engine.Level++;
                createdAbility = Engine;
            }

            else if (blueprintPart.PartType == TileObjectType.PartArmor)
            {
                if (Armor == null)
                {
                    Armor = new Armor(this, 1);
                }
                else if (level > Armor.Level)
                    Armor.Level++;
                createdAbility = Armor;
            }


            else if (blueprintPart.PartType == TileObjectType.PartExtractor)
            {
                if (Extractor == null)
                {
                    Extractor = new Extractor(this, 1);
                }
                else if (level > Extractor.Level)
                    Extractor.Level++;
                createdAbility = Extractor;
            }

            else if (blueprintPart.PartType == TileObjectType.PartAssembler)
            {
                if (Assembler == null)
                {
                    Assembler = new Assembler(this, 1);
                    if (blueprintPart.Capacity.HasValue && blueprintPart.Capacity.Value > 0)
                        Assembler.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer && Assembler.TileContainer != null)
                    {
                        Assembler.TileContainer.CreateMinerals(Assembler.TileContainer.Capacity);
                    }
                }
                else if (level > Assembler.Level)
                    Assembler.Level++;
                createdAbility = Assembler;
            }
            else if (blueprintPart.PartType == TileObjectType.PartWeapon)
            {
                if (Weapon == null)
                {
                    Weapon = new Weapon(this, 1, blueprintPart.Range);
                    if (blueprintPart.Capacity.HasValue && blueprintPart.Capacity.Value > 0)
                        Weapon.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer && Weapon.TileContainer != null)
                    {
                        Weapon.TileContainer.CreateMinerals(Weapon.TileContainer.Capacity);
                    }
                }
                else if (level > Weapon.Level)
                    Weapon.Level++;
                createdAbility = Weapon;
            }

            else if (blueprintPart.PartType == TileObjectType.PartContainer)
            {
                if (Container == null)
                {
                    Container = new Container(this, 1, blueprintPart.Range);
                    if (blueprintPart.Capacity.HasValue && blueprintPart.Capacity.Value > 0)
                        Container.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer && Container.TileContainer != null)
                    {
                        // TESTEXTRACT
                        if (Container.TileContainer.Capacity < 20)
                            Container.TileContainer.CreateMinerals(Container.TileContainer.Capacity);
                        else
                            //Container.TileContainer.CreateMinerals(20);
                            Container.TileContainer.CreateMinerals(Container.TileContainer.Capacity);

                    }
                }
                else if (level > Container.Level)
                    Container.Level++;
                createdAbility = Container;
            }

            else if (blueprintPart.PartType == TileObjectType.PartReactor)
            {
                if (Reactor == null)
                {
                    Reactor = new Reactor(this, 1, blueprintPart.Range);
                    if (blueprintPart.Capacity.HasValue && blueprintPart.Capacity.Value > 0)
                        Reactor.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer && Reactor.TileContainer != null)
                    {
                        Reactor.TileContainer.CreateMinerals(Reactor.TileContainer.Capacity);
                    }
                }
                else if (level > Reactor.Level)
                    Reactor.Level++;
                createdAbility = Reactor;
            }

            else if (blueprintPart.PartType == TileObjectType.PartRadar)
            {
                if (Radar == null)
                {
                    Radar = new Radar(this, 1, blueprintPart.Range);
                }
                else if (level > Radar.Level)
                    Radar.Level++;
                createdAbility = Radar;
            }
            if (createdAbility != null)
            {
                if (createdAbility.PartTileObjects.Count > createdAbility.Level - 1)
                {
                    throw new Exception("duplicate part");
                }

                TileObject newTileObject = new TileObject();
                newTileObject.TileObjectType = createdAbility.PartType;
                newTileObject.Direction = tileObject.Direction;
                createdAbility.PartTileObjects.Add(newTileObject);
            }
            else
            {
                throw new Exception();
            }
            CountParts();
            return createdAbility;
        }

        public bool IsInstalled(BlueprintPart blueprintPart, int level)
        {
            if (blueprintPart.PartType == TileObjectType.PartEngine)
                return Engine != null && Engine.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartArmor)
                return Armor != null && Armor.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartWeapon)
                return Weapon != null && Weapon.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartAssembler)
                return Assembler != null && Assembler.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartExtractor)
                return Extractor != null && Extractor.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartContainer)
                return Container != null && Container.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartReactor)
                return Reactor != null && Reactor.Level == level;

            if (blueprintPart.PartType == TileObjectType.PartRadar)
                return Radar != null && Radar.Level == level;

            return false;
        }

        public void CreateAllPartsFromBlueprint()
        {
            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                Ability ability;
                do
                {
                    TileObject tileObject = new TileObject();
                    tileObject.Direction = Direction.C;
                    tileObject.TileObjectType = TileObjectType.Mineral;
                    
                    ability = CreateBlueprintPart(blueprintPart, true, tileObject);
                }
                while (ability.Level < blueprintPart.Level);
            }
            
            UnderConstruction = false;
        }

        public bool CanFill()
        {
            if (Weapon != null)
            {
                if (Weapon.TileContainer != null && Weapon.TileContainer.Count < Weapon.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Assembler != null)
            {
                if (Assembler.TileContainer != null && Assembler.TileContainer.Count < Assembler.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Reactor != null)
            {
                if (Reactor.TileContainer != null && Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Container != null && Container.TileContainer.Count < Container.TileContainer.Capacity)
            {
                return true;
            }
            return false;
        }
        
        public void Upgrade(Move move, TileObject tileObject)
        {
            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                if (move.MoveRecipe.Result == blueprintPart.PartType)
                {
                    CreateBlueprintPart(blueprintPart, false, tileObject);
                    break;
                }
            }
            if (UnderConstruction && IsComplete())
            {
                UnderConstruction = false;
            }
            ExtractMe = false;
        }

        private List<TileObject> CopyContainer(TileContainer tileContainer)
        {
            List<TileObject> target = new List<TileObject>();
            target.AddRange(tileContainer.TileObjects);
            return target;
        }

        public MoveUpdateStats CollectStats()
        {
            MoveUpdateStats stats = new MoveUpdateStats();
            stats.BlueprintName = Blueprint.Name;
            stats.MarkedForExtraction = ExtractMe;
            stats.Direction = Direction;
            stats.UnitParts = new List<MoveUpdateUnitPart>();

            if (CurrentGameCommand != null)
            {
                stats.Automatic = false;
            }
            else
            {
                stats.Automatic = true;
            }

            Game.SetVisibilityMask(pos, stats);

            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                MoveUpdateUnitPart moveUpdateUnitPart = new MoveUpdateUnitPart();

                moveUpdateUnitPart.Name = blueprintPart.Name;
                moveUpdateUnitPart.Exists = IsInstalled(blueprintPart, blueprintPart.Level);
                moveUpdateUnitPart.PartType = blueprintPart.PartType;
                moveUpdateUnitPart.CompleteLevel = blueprintPart.Level;
                moveUpdateUnitPart.Range = blueprintPart.Range;

                if (blueprintPart.PartType == TileObjectType.PartWeapon && Weapon != null)
                {
                    moveUpdateUnitPart.Level = Weapon.Level;
                    if (Weapon.TileContainer != null)
                    {
                        moveUpdateUnitPart.TileObjects = CopyContainer(Weapon.TileContainer);
                        moveUpdateUnitPart.Capacity = Weapon.TileContainer.Capacity;
                    }
                }
                else if (blueprintPart.PartType == TileObjectType.PartAssembler && Assembler != null)
                {
                    moveUpdateUnitPart.Level = Assembler.Level;
                    if (Assembler.TileContainer != null)
                    {
                        moveUpdateUnitPart.TileObjects = CopyContainer(Assembler.TileContainer);
                        moveUpdateUnitPart.Capacity = Assembler.TileContainer.Capacity;
                    }
                }
                else if (blueprintPart.PartType == TileObjectType.PartContainer && Container != null)
                {
                    moveUpdateUnitPart.Level = Container.Level;
                    if (Container.TileContainer != null)
                    {
                        moveUpdateUnitPart.TileObjects = CopyContainer(Container.TileContainer);
                        moveUpdateUnitPart.Capacity = Container.TileContainer.Capacity;
                    }
                }
                else if (blueprintPart.PartType == TileObjectType.PartReactor && Reactor != null)
                {
                    moveUpdateUnitPart.Level = Reactor.Level;
                    moveUpdateUnitPart.AvailablePower = Reactor.AvailablePower;
                    if (Reactor.TileContainer != null)
                    {
                        moveUpdateUnitPart.TileObjects = CopyContainer(Reactor.TileContainer);
                        moveUpdateUnitPart.Capacity = Reactor.TileContainer.Capacity;
                    }
                }
                else if (blueprintPart.PartType == TileObjectType.PartArmor && Armor != null)
                {
                    moveUpdateUnitPart.Level = Armor.Level;
                    moveUpdateUnitPart.ShieldActive = Armor.ShieldActive;
                    moveUpdateUnitPart.ShieldPower = Armor.ShieldPower;
                }
                else if (blueprintPart.PartType == TileObjectType.PartRadar && Radar != null)
                {
                    moveUpdateUnitPart.Level = Radar.Level;
                }
                else if (blueprintPart.PartType == TileObjectType.PartExtractor && Extractor != null)
                {
                    moveUpdateUnitPart.Level = Extractor.Level;
                }
                else if (blueprintPart.PartType == TileObjectType.PartEngine && Engine != null)
                {
                    moveUpdateUnitPart.Level = Engine.Level;
                }
                stats.UnitParts.Add(moveUpdateUnitPart);
            }

            stats.Power = Power;
            stats.Stunned = Stunned;

            if (CurrentGameCommand != null)
            {
                stats.MoveUpdateStatsCommand = new MoveUpdateStatsCommand();
                stats.MoveUpdateStatsCommand.GameCommandType = CurrentGameCommand.GameCommandType;
                stats.MoveUpdateStatsCommand.TargetPosition = CurrentGameCommand.TargetPosition;

                stats.MoveUpdateStatsCommand.AttachedUnit = new MapGameCommandItemUnit();
                stats.MoveUpdateStatsCommand.AttachedUnit.UnitId = CurrentGameCommand.AttachedUnit.UnitId;
                stats.MoveUpdateStatsCommand.AttachedUnit.Status = CurrentGameCommand.AttachedUnit.Status;
                stats.MoveUpdateStatsCommand.AttachedUnit.Alert = CurrentGameCommand.AttachedUnit.Alert;

                stats.MoveUpdateStatsCommand.FactoryUnit = new MapGameCommandItemUnit();
                stats.MoveUpdateStatsCommand.FactoryUnit.UnitId = CurrentGameCommand.FactoryUnit.UnitId;
                stats.MoveUpdateStatsCommand.FactoryUnit.Status = CurrentGameCommand.FactoryUnit.Status;
                stats.MoveUpdateStatsCommand.FactoryUnit.Alert = CurrentGameCommand.FactoryUnit.Alert;

                stats.MoveUpdateStatsCommand.TransportUnit = new MapGameCommandItemUnit();
                stats.MoveUpdateStatsCommand.TransportUnit.UnitId = CurrentGameCommand.TransportUnit.UnitId;
                stats.MoveUpdateStatsCommand.TransportUnit.Status = CurrentGameCommand.TransportUnit.Status;
                stats.MoveUpdateStatsCommand.TransportUnit.Alert = CurrentGameCommand.TransportUnit.Alert;

                stats.MoveUpdateStatsCommand.TargetUnit = new MapGameCommandItemUnit();
                stats.MoveUpdateStatsCommand.TargetUnit.UnitId = CurrentGameCommand.TargetUnit.UnitId;
                stats.MoveUpdateStatsCommand.TargetUnit.Status = CurrentGameCommand.TargetUnit.Status;
                stats.MoveUpdateStatsCommand.TargetUnit.Alert = CurrentGameCommand.TargetUnit.Alert;
            }

            stats.MoveUnitItemOrders = new List<MoveUnitItemOrder>();
            foreach (UnitItemOrder unitItemOrder in UnitOrders.unitItemOrders)
            {
                MoveUnitItemOrder moveUnitItemOrder = new MoveUnitItemOrder();
                moveUnitItemOrder.TileObjectType = unitItemOrder.TileObjectType;
                moveUnitItemOrder.TileObjectState = unitItemOrder.TileObjectState;
                stats.MoveUnitItemOrders.Add(moveUnitItemOrder);
            }

            return stats;
        }

        public void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            if (Assembler != null)
            {
                Assembler.ComputePossibleMoves(possibleMoves, includedPosition2s, moveFilter);
            }
            if (Extractor != null)
            {
                Extractor.ComputePossibleMoves(possibleMoves, includedPosition2s, moveFilter);
            }
            if (Weapon != null)
            {
                Weapon.ComputePossibleMoves(possibleMoves, includedPosition2s, moveFilter);
            }
            if (Engine != null)
            {
                Engine.ComputePossibleMoves(possibleMoves, includedPosition2s, moveFilter);
            }
        }

        public List<TileObject> ConsumeIngredients(MoveRecipe moveRecipe,  Dictionary<Position2, Unit> changedUnits)
        {
            List<MoveRecipeIngredient> realIngredients = new List<MoveRecipeIngredient>();

            bool missingIngredient = false;
            foreach (MoveRecipeIngredient moveRecipeIngredient in moveRecipe.Ingredients)
            {
                if (TileObject.CanConvertTileObjectIntoMineral(moveRecipeIngredient.TileObjectType))
                {
                    // Consume a part of the unit
                    moveRecipeIngredient.TargetPosition = pos;
                    realIngredients.Add(moveRecipeIngredient);
                }
                else
                {
                    MoveRecipeIngredient realIngredient = GetConsumableIngredient(moveRecipeIngredient.TileObjectType, true);
                    if (realIngredient == null)
                    {
                        missingIngredient = true;
                        break;
                    }
                    realIngredient.TargetPosition = pos;
                    realIngredients.Add(realIngredient);
                }
            }
            if (missingIngredient)
                return null;

            // Replace suggested ingredients with real ones
            moveRecipe.Ingredients.Clear();
            foreach (MoveRecipeIngredient realIngredient in realIngredients)
            {
                TileObject consumedObject = ConsumeIngredient(realIngredient, changedUnits);
                moveRecipe.Ingredients.Add(realIngredient);
            }

            List<TileObject> results = new List<TileObject>();

            TileObject tileObject = new TileObject();
            tileObject.TileObjectType = moveRecipe.Result;
            tileObject.Direction = Direction.C;
            results.Add(tileObject);

            return results;
        }

        public Ability HitBy(bool ignoreShield)
        {
            if (IsDead())
            {
                // Happens, but why? Ignore for now, skip move
                //throw new Exception("Dead unit hit...");
                return null;
            }
            Ability partHit = null;

            if (!ignoreShield && Armor != null)
            {
                if (Armor.ShieldActive)
                {
                    Armor.ShieldHit();

                    Shield shield = new Shield(this, 1);
                    partHit = shield;
                }
            }
            // Revers
            for (int i = Blueprint.Parts.Count - 1; i >= 0 && partHit == null; i--)
            {
                BlueprintPart blueprintPart = Blueprint.Parts[i];

                int level = blueprintPart.Level;
                while (level > 0)
                {
                    if (IsInstalled(blueprintPart, level))
                    {
                        if (blueprintPart.PartType == TileObjectType.PartArmor) partHit = Armor;
                        if (blueprintPart.PartType == TileObjectType.PartAssembler) partHit = Assembler;
                        if (blueprintPart.PartType == TileObjectType.PartContainer) partHit = Container;
                        if (blueprintPart.PartType == TileObjectType.PartEngine) partHit = Engine;
                        if (blueprintPart.PartType == TileObjectType.PartExtractor) partHit = Extractor;
                        if (blueprintPart.PartType == TileObjectType.PartRadar) partHit = Radar;
                        if (blueprintPart.PartType == TileObjectType.PartReactor) partHit = Reactor;
                        if (blueprintPart.PartType == TileObjectType.PartWeapon) partHit = Weapon;

                        partHit.Level--;
                        if (partHit.TileContainer != null && blueprintPart.Capacity.HasValue)
                        {
                            partHit.TileContainer.Capacity = (blueprintPart.Capacity.Value / blueprintPart.Level) * partHit.Level;
                        }
                        if (partHit.Level == 0)
                        {
                            if (blueprintPart.PartType == TileObjectType.PartArmor) Armor = null;
                            if (blueprintPart.PartType == TileObjectType.PartAssembler) Assembler = null;
                            if (blueprintPart.PartType == TileObjectType.PartContainer) Container = null;
                            if (blueprintPart.PartType == TileObjectType.PartEngine) Engine = null;
                            if (blueprintPart.PartType == TileObjectType.PartExtractor) Extractor = null;
                            if (blueprintPart.PartType == TileObjectType.PartRadar) Radar = null;
                            if (blueprintPart.PartType == TileObjectType.PartReactor) Reactor = null;
                            if (blueprintPart.PartType == TileObjectType.PartWeapon) Weapon = null;
                        }

                        //
                        break;
                    }
                    level--;
                }
            }
            CountParts();
            return partHit;
        }
       
        public bool RemoveTileObjects(List<TileObject> tileObjects, int numberOfObjects, TileObjectType tileObjectType)
        {
            bool removed = false;
            if (Container != null)
            {
                while (numberOfObjects > 0)
                {
                    TileObject tileObject;
                    tileObject = Container.TileContainer.RemoveTileObject(tileObjectType);

                    if (tileObject == null)
                    {
                        break;
                    }
                    else
                    {
                        numberOfObjects--;
                        tileObjects.Add(tileObject);
                        removed = true;
                    }
                }
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                while (numberOfObjects > 0)
                {
                    TileObject tileObject;
                    tileObject = Weapon.TileContainer.RemoveTileObject(tileObjectType);

                    if (tileObject == null)
                    {
                        break;
                    }
                    else
                    {
                        numberOfObjects--;
                        tileObjects.Add(tileObject);
                        removed = true;
                    }
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                while (numberOfObjects > 0)
                {
                    TileObject tileObject;
                    tileObject = Assembler.TileContainer.RemoveTileObject(tileObjectType);

                    if (tileObject == null)
                    {
                        break;
                    }
                    else
                    {
                        numberOfObjects--;
                        tileObjects.Add(tileObject);
                        removed = true;
                    }
                }
            }
            if (Reactor != null && Reactor.TileContainer != null)
            {
                while (numberOfObjects > 0)
                {
                    TileObject tileObject;
                    tileObject = Reactor.TileContainer.RemoveTileObject(tileObjectType);

                    if (tileObject == null)
                    {
                        break;
                    }
                    else
                    {
                        numberOfObjects--;
                        tileObjects.Add(tileObject);
                        removed = true;
                    }
                }
            }
            return removed;
        }

        public bool IsSpaceForIngredient(MoveRecipeIngredient realIndigrient)
        {
            if (Reactor != null && Reactor.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Container != null)
            {
                //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                if (Container.TileContainer.Count < Container.TileContainer.Capacity)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsSpaceForTileObject(TileObjectType tileObjectType)
        {
            // Check capacity
            if (Reactor != null && Reactor.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Container != null)
            {
                //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                if (Container.TileContainer.Count < Container.TileContainer.Capacity)
                {
                    return true;
                }
            }
            return false;
        }
        /*
        public bool IsSpaceForTileObject(TileObject tileObject)
        {
            if (Reactor != null && Reactor.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Container != null)
            {
                //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                if (Container.TileContainer.Count < Container.TileContainer.Capacity)
                {
                    return true;
                }
            }
            return false;
        }*/

        public void AddTileObjects(List<TileObject> tileObjects)
        {
            List<TileObject> currentTileObjects = new List<TileObject>();
            currentTileObjects.AddRange(tileObjects);

            foreach (TileObject tileObject in currentTileObjects)
            {
                if (Reactor != null && Reactor.TileContainer != null)
                {
                    //while (tileObjects.Count > 0 && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                    if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity)
                    {
                        Reactor.TileContainer.Add(tileObject);
                        tileObjects.Remove(tileObject);

                        //Reactor.BurnIfNeccessary();
                        continue;
                    }
                }
                if (Assembler != null && Assembler.TileContainer != null)
                {
                    //while (tileObjects.Count > 0 && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                    if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity)
                    {
                        Assembler.TileContainer.Add(tileObject);
                        tileObjects.Remove(tileObject);
                        continue;
                    }
                }
                if (Weapon != null && Weapon.TileContainer != null)
                {
                    //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                    if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity)
                    {
                        Weapon.TileContainer.Add(tileObject);
                        tileObjects.Remove(tileObject);
                        continue;
                    }
                }
                if (Container != null)
                {
                    //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                    if (Container.TileContainer.Count < Container.TileContainer.Capacity)
                    {
                        Container.TileContainer.Add(tileObject);
                        tileObjects.Remove(tileObject);
                        continue;
                    }
                }
            }
        }
        

        public bool IsDead()
        {
            if (Extractor == null && Armor == null && Weapon == null && Engine == null && Assembler == null && Container == null && Reactor == null && Radar == null)
            {
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return UnitId + " " + Owner.PlayerModel.Name + ": " + pos.ToString() + " " + Blueprint.Name;
        }
    }
}
