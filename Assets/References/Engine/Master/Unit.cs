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

    [DataContract]
    public class Unit
    {
        private Position2 pos;
        // Position2 before any moves have been processed
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

        public string UnitId { get; set; }

        internal GameCommandItem CurrentGameCommand { get; private set; }

        internal void SetTempGameCommand(GameCommandItem gameCommand)
        {
            CurrentGameCommand = gameCommand;
        }        
        internal void SetGameCommand(GameCommandItem gameCommand)
        {
            if (gameCommand.BuildPositionReached)
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
                        foreach (GameCommandItem blueprintCommandItem in gameCommand.GameCommandItems)
                        {
                            if (blueprintCommandItem == CurrentGameCommand)
                                continue;

                            if (blueprintCommandItem.AttachedUnit.UnitId == UnitId)
                            {
                                blueprintCommandItem.AttachedUnit.ResetStatus();
                                blueprintCommandItem.AttachedUnit.UnitId = null;
                            }
                            if (blueprintCommandItem.TransportUnit.UnitId == UnitId)
                            {
                                blueprintCommandItem.TransportUnit.ResetStatus();
                                blueprintCommandItem.TransportUnit.UnitId = null;
                            }
                            if (blueprintCommandItem.TargetUnit.UnitId == UnitId)
                            {
                                blueprintCommandItem.TargetUnit.ResetStatus();
                                blueprintCommandItem.TargetUnit.UnitId = null;
                            }
                            if (blueprintCommandItem.FactoryUnit.UnitId == UnitId)
                            {
                                blueprintCommandItem.FactoryUnit.ResetStatus();
                                blueprintCommandItem.FactoryUnit.UnitId = null;
                            }
                        }
                    }
                }

                if (CurrentGameCommand.AttachedUnit.UnitId == UnitId)
                {
                    if (CurrentGameCommand.DeleteWhenDestroyed)
                    {
                        CurrentGameCommand.AttachedUnit.SetStatus("DeleteBecauseDestroyed");
                        CurrentGameCommand.GameCommand.GameCommandItems.Remove(CurrentGameCommand);                        
                    }
                    else
                    {
                        CurrentGameCommand.AttachedUnit.SetStatus("Removed: " + UnitId);
                    }
                    CurrentGameCommand.AttachedUnit.UnitId = null;
                }
                if (CurrentGameCommand.TransportUnit.UnitId == UnitId)
                {
                    CurrentGameCommand.TransportUnit.ResetStatus();
                    CurrentGameCommand.TransportUnit.UnitId = null;
                }
                if (CurrentGameCommand.FactoryUnit.UnitId == UnitId)
                {
                    CurrentGameCommand.FactoryUnit.ResetStatus();
                    CurrentGameCommand.FactoryUnit.UnitId = null;
                }
                if (CurrentGameCommand.TargetUnit.UnitId == UnitId)
                {
                    CurrentGameCommand.TargetUnit.ResetStatus();
                    CurrentGameCommand.TargetUnit.UnitId = null;
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

        public MoveRecipeIngredient FindAmmo(bool searchNeighbors = true)
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
                        moveRecipeIngredient = t.Unit.FindAmmo(false);
                        if (moveRecipeIngredient != null)
                            return moveRecipeIngredient;
                    }
                }
            }
            // Extract ammo from own unit
            moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.Count = 1;

            if (Container != null && Container.TileContainer != null)
            {
                TileObject tileObject = Container.TileContainer.GetMatchingTileObject(TileObjectType.All);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
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

            BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = Blueprint.Name;
            blueprintCommandItem.Direction = Direction.C;

            GameCommandItem gameCommandItem = new GameCommandItem(gameCommand, blueprintCommandItem);
            gameCommandItem.TargetUnit.UnitId = UnitId;
            gameCommandItem.TargetUnit.SetStatus(Blueprint.Name + " WaitingForDelivery");
            Changed = true;

            gameCommand.RequestedItems = new List<RecipeIngredient>();

            RecipeIngredient recipeIngredient = new RecipeIngredient(tileObjectType, capacity);
            gameCommand.RequestedItems.Add(recipeIngredient);

            SetGameCommand(gameCommandItem);

            gameCommand.GameCommandItems.Add(gameCommandItem);
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
                TileObject tileObject = Assembler.TileContainer.GetMatchingTileObject(tileObjectType);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.Source = TileObjectType.PartAssembler;
                    return moveRecipeIngredient;
                }
            }
            if (Container != null && Container.TileContainer != null)
            {
                TileObject tileObject = Container.TileContainer.GetMatchingTileObject(tileObjectType);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
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
            List<MoveRecipeIngredient> reservedIngredients = new List<MoveRecipeIngredient>();
            bool allFound = true;

            foreach (RecipeIngredient recipeIngredient in ingredients)
            {
                int count = recipeIngredient.Count;
                while (count-- > 0)
                {
                    MoveRecipeIngredient moveRecipeIngredient;
                    moveRecipeIngredient = FindIngredient(recipeIngredient.TileObjectType, true);
                    if (moveRecipeIngredient == null)
                    {
                        allFound = false;
                        break;
                    }
                    ReserveIngredient(moveRecipeIngredient);
                    reservedIngredients.Add(moveRecipeIngredient);
                }
            }
            if (allFound == false)
            {
                foreach (MoveRecipeIngredient moveRecipeIngredient in reservedIngredients)
                {
                    ReleaseReservedIngredient(moveRecipeIngredient);
                }
            }
            return allFound;
        }
        public void ClearReservations()
        {
            if (Assembler != null && Assembler.TileContainer != null)
            {
                Assembler.TileContainer.ClearReservations();
            }
            if (Container != null && Container.TileContainer != null)
            {
                Container.TileContainer.ClearReservations();
            }
            if (Reactor != null && Reactor.TileContainer != null)
            {
                Reactor.TileContainer.ClearReservations();
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                Weapon.TileContainer.ClearReservations();
            }
        }

        public void ReleaseReservedIngredient(MoveRecipeIngredient ingredient)
        {
            if (ingredient.SourcePosition == Pos)
            {
                if (ingredient.Source == TileObjectType.PartAssembler && Assembler != null && Assembler.TileContainer != null)
                {
                    Assembler.TileContainer.ReleaseReservedIngredient(ingredient.TileObjectType);
                }
                if (ingredient.Source == TileObjectType.PartContainer && Container != null && Container.TileContainer != null)
                {
                    Container.TileContainer.ReleaseReservedIngredient(ingredient.TileObjectType);
                }
                if (ingredient.Source == TileObjectType.PartReactor && Reactor != null && Reactor.TileContainer != null)
                {
                    Reactor.TileContainer.ReleaseReservedIngredient(ingredient.TileObjectType);
                }
                if (ingredient.Source == TileObjectType.PartWeapon && Weapon != null && Weapon.TileContainer != null)
                {
                    Weapon.TileContainer.ReleaseReservedIngredient(ingredient.TileObjectType);
                }
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
                            t.Unit.ReleaseReservedIngredient(ingredient);
                        }
                        break;
                    }
                }
            }
        }

        public void ReserveIngredient(MoveRecipeIngredient ingredient)
        {
            if (ingredient.SourcePosition == Pos)
            {
                if (ingredient.Source == TileObjectType.PartAssembler && Assembler != null && Assembler.TileContainer != null)
                {
                    Assembler.TileContainer.ReserveIngredient(ingredient.TileObjectType);
                }
                if (ingredient.Source == TileObjectType.PartContainer && Container != null && Container.TileContainer != null)
                {
                    Container.TileContainer.ReserveIngredient(ingredient.TileObjectType);
                }
                if (ingredient.Source == TileObjectType.PartReactor && Reactor != null && Reactor.TileContainer != null)
                {
                    Reactor.TileContainer.ReserveIngredient(ingredient.TileObjectType);
                }
                if (ingredient.Source == TileObjectType.PartWeapon && Weapon != null && Weapon.TileContainer != null)
                {
                    Weapon.TileContainer.ReserveIngredient(ingredient.TileObjectType);
                }
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
                            t.Unit.ReserveIngredient(ingredient);
                        }
                        break;
                    }
                }
            }
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
        public MoveRecipeIngredient FindIngredientToBurn()
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
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id)
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
        public MoveRecipeIngredient FindIngredientForAmmo()
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
                    if (t.Unit != null && t.Unit.Owner.PlayerModel.Id == Owner.PlayerModel.Id)
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

        public MoveRecipeIngredient FindIngredient(TileObjectType tileObjectType, bool searchNeighbors)
        {
            MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.TileObjectType = tileObjectType;
            moveRecipeIngredient.Count = 1;

            if (Container != null && Container.TileContainer != null)
            {
                TileObject tileObject = Container.TileContainer.GetMatchingTileObject(tileObjectType);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
                    moveRecipeIngredient.Source = TileObjectType.PartContainer;
                    return moveRecipeIngredient;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                TileObject tileObject = Assembler.TileContainer.GetMatchingTileObject(tileObjectType);
                if (tileObject != null)
                {
                    moveRecipeIngredient.TileObjectType = tileObject.TileObjectType;
                    moveRecipeIngredient.TileObjectKind = tileObject.TileObjectKind;
                    moveRecipeIngredient.SourcePosition = Pos;
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
                        moveRecipeIngredient = t.Unit.FindIngredient(tileObjectType, false);
                        if (moveRecipeIngredient != null)
                            return moveRecipeIngredient;
                    }
                }
            }
            return null;
        }
        public void AddIngredient(MoveRecipeIngredient ingredient)
        {
            TileObject tileObject = new TileObject();
            tileObject.TileObjectType = ingredient.TileObjectType;
            tileObject.TileObjectKind = ingredient.TileObjectKind;
            tileObject.Direction = Direction.C;

            if (Reactor != null && Reactor.TileContainer != null &&
                Reactor.TileContainer.Count < Reactor.TileContainer.Capacity &&
                Reactor.TileContainer.Accepts(tileObject))
            {
                ingredient.Target = TileObjectType.PartReactor;
                Reactor.TileContainer.Add(tileObject);
            }
            else if (Assembler != null && Assembler.TileContainer != null &&
                Assembler.TileContainer.Count < Assembler.TileContainer.Capacity &&
                Assembler.TileContainer.Accepts(tileObject))
            {
                ingredient.Target = TileObjectType.PartAssembler;
                Assembler.TileContainer.Add(tileObject);
            }
            else if (Container != null && Container.TileContainer != null &&
                Container.TileContainer.Count < Container.TileContainer.Capacity && 
                Container.TileContainer.Accepts(tileObject))
            {
                ingredient.Target = TileObjectType.PartContainer;
                Container.TileContainer.Add(tileObject);
            }
            else if (Weapon != null && Weapon.TileContainer != null &&
                Weapon.TileContainer.Count < Weapon.TileContainer.Capacity &&
                Weapon.TileContainer.Accepts(tileObject))
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
                        tileObject = Assembler.TileContainer.GetMatchingTileObject(ingredient.TileObjectType);
                        if (tileObject != null)
                            Assembler.TileContainer.Remove(tileObject);
                    }
                    if (ingredient.Source == TileObjectType.PartContainer && Container != null && Container.TileContainer != null)
                    {
                        tileObject = Container.TileContainer.GetMatchingTileObject(ingredient.TileObjectType);
                        if (tileObject != null)
                            Container.TileContainer.Remove(tileObject);
                    }
                    if (ingredient.Source == TileObjectType.PartReactor && Reactor != null && Reactor.TileContainer != null)
                    {
                        tileObject = Reactor.TileContainer.GetMatchingTileObject(ingredient.TileObjectType);
                        if (tileObject != null)
                            Reactor.TileContainer.Remove(tileObject);
                    }
                    if (ingredient.Source == TileObjectType.PartWeapon && Weapon != null && Weapon.TileContainer != null)
                    {
                        tileObject = Weapon.TileContainer.GetMatchingTileObject(ingredient.TileObjectType);
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

        public int CountTileObjectsInContainer()
        {
            int metal = 0;

            if (Container != null) 
                metal += Container.TileContainer.Count;

            if (Weapon != null)
            {
                if (Weapon.TileContainer != null)
                    metal += Weapon.TileContainer.Count;
            }
            if (Assembler != null)
            {
                if (Assembler.TileContainer != null)
                    metal += Assembler.TileContainer.Count;
            }
            if (Reactor != null)
            {
                if (Reactor.TileContainer != null)
                    metal += Reactor.TileContainer.Count;
            }

            return metal;
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
                    if (blueprintPart.Capacity.HasValue)
                        Assembler.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
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
                    if (blueprintPart.Capacity.HasValue)
                        Weapon.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
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
                    if (blueprintPart.Capacity.HasValue)
                        Container.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
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
                    if (blueprintPart.Capacity.HasValue)
                        Reactor.TileContainer.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
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
            }
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
                    moveUpdateUnitPart.TileObjects = CopyContainer(Weapon.TileContainer);
                    moveUpdateUnitPart.Capacity = Weapon.TileContainer.Capacity;
                }
                else if (blueprintPart.PartType == TileObjectType.PartAssembler && Assembler != null)
                {
                    moveUpdateUnitPart.Level = Assembler.Level;
                    moveUpdateUnitPart.TileObjects = CopyContainer(Assembler.TileContainer);
                    moveUpdateUnitPart.Capacity = Assembler.TileContainer.Capacity;
                }
                else if (blueprintPart.PartType == TileObjectType.PartContainer && Container != null)
                {
                    moveUpdateUnitPart.Level = Container.Level;
                    moveUpdateUnitPart.TileObjects = CopyContainer(Container.TileContainer);
                    moveUpdateUnitPart.Capacity = Container.TileContainer.Capacity;
                }
                else if (blueprintPart.PartType == TileObjectType.PartReactor && Reactor != null)
                {
                    moveUpdateUnitPart.Level = Reactor.Level;
                    moveUpdateUnitPart.AvailablePower = Reactor.AvailablePower;
                    moveUpdateUnitPart.TileObjects = CopyContainer(Reactor.TileContainer);
                    moveUpdateUnitPart.Capacity = Reactor.TileContainer.Capacity;
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
                stats.MoveUpdateStatsCommand.GameCommandType = CurrentGameCommand.GameCommand.GameCommandType;
                stats.MoveUpdateStatsCommand.TargetPosition = CurrentGameCommand.GameCommand.TargetPosition;

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
       
        public bool RemoveTileObjects(List<TileObject> tileObjects, int numberOfObjects, TileObjectType tileObjectType, Unit targetUnit)
        {
            bool removed = false;
            if (Container != null)
            {
                while (numberOfObjects > 0)
                {
                    TileObject tileObject;
                    if (targetUnit == null)
                        tileObject = Container.TileContainer.RemoveTileObject(tileObjectType);
                    else
                        tileObject = Container.TileContainer.RemoveTileObjectIfFits(targetUnit);

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
                    if (targetUnit == null)
                        tileObject = Weapon.TileContainer.RemoveTileObject(tileObjectType);
                    else
                        tileObject = Weapon.TileContainer.RemoveTileObjectIfFits(targetUnit);


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
                    if (targetUnit == null)
                        tileObject = Assembler.TileContainer.RemoveTileObject(tileObjectType);
                    else
                        tileObject = Assembler.TileContainer.RemoveTileObjectIfFits(targetUnit);

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
                    if (targetUnit == null)
                        tileObject = Reactor.TileContainer.RemoveTileObject(tileObjectType);
                    else
                        tileObject = Reactor.TileContainer.RemoveTileObjectIfFits(targetUnit);

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
                if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity && Reactor.TileContainer.Accepts(realIndigrient))
                {
                    return true;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity && Assembler.TileContainer.Accepts(realIndigrient))
                {
                    return true;
                }
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity && Weapon.TileContainer.Accepts(realIndigrient))
                {
                    return true;
                }
            }
            if (Container != null)
            {
                //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                if (Container.TileContainer.Count < Container.TileContainer.Capacity && Container.TileContainer.Accepts(realIndigrient))
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsSpaceForTileObject(TileObject tileObject)
        {
            if (Reactor != null && Reactor.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity && Reactor.TileContainer.Accepts(tileObject))
                {
                    return true;
                }
            }
            if (Assembler != null && Assembler.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity && Assembler.TileContainer.Accepts(tileObject))
                {
                    return true;
                }
            }
            if (Weapon != null && Weapon.TileContainer != null)
            {
                //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity && Weapon.TileContainer.Accepts(tileObject))
                {
                    return true;
                }
            }
            if (Container != null)
            {
                //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                if (Container.TileContainer.Count < Container.TileContainer.Capacity && Container.TileContainer.Accepts(tileObject))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddTileObjects(List<TileObject> tileObjects)
        {
            List<TileObject> currentTileObjects = new List<TileObject>();
            currentTileObjects.AddRange(tileObjects);

            foreach (TileObject tileObject in currentTileObjects)
            {
                if (Reactor != null && Reactor.TileContainer != null)
                {
                    //while (tileObjects.Count > 0 && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                    if (Reactor.TileContainer.Count < Reactor.TileContainer.Capacity && Reactor.TileContainer.Accepts(tileObject))
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
                    if (Assembler.TileContainer.Count < Assembler.TileContainer.Capacity && Assembler.TileContainer.Accepts(tileObject))
                    {
                        Assembler.TileContainer.Add(tileObject);
                        tileObjects.Remove(tileObject);
                        continue;
                    }
                }
                if (Weapon != null && Weapon.TileContainer != null)
                {
                    //while (tileObjects.Count > 0 && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                    if (Weapon.TileContainer.Count < Weapon.TileContainer.Capacity && Weapon.TileContainer.Accepts(tileObject))
                    {
                        Weapon.TileContainer.Add(tileObject);
                        tileObjects.Remove(tileObject);
                        continue;
                    }
                }
                if (Container != null)
                {
                    //while (tileObjects.Count > 0 && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
                    if (Container.TileContainer.Count < Container.TileContainer.Capacity && Container.TileContainer.Accepts(tileObject))
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
