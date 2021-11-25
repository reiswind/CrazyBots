﻿using Engine.Interface;
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
            CurrentGameCommand = gameCommand;
        }

        public void OnDestroyed()
        {
            ResetGameCommand();
        }
        public void ResetGameCommand()
        {
            if (CurrentGameCommand != null)
            {
                if (Owner.PlayerModel.Id != 0)
                {
                    Player player = Game.Players[Owner.PlayerModel.Id];
                    foreach (GameCommand gameCommand in player.GameCommands)
                    {
                        foreach (GameCommandItem blueprintCommandItem in gameCommand.GameCommandItems)
                        {
                            if (blueprintCommandItem.AttachedUnitId == UnitId)
                            {
                                blueprintCommandItem.AttachedUnitId = null;
                            }
                        }
                    }
                }

                if (CurrentGameCommand.AttachedUnitId == UnitId)
                {
                    if (CurrentGameCommand.DeleteWhenDestroyed)
                    {
                        CurrentGameCommand.SetStatus("DeleteBecauseDestroyed");
                        CurrentGameCommand.GameCommand.GameCommandItems.Remove(CurrentGameCommand);                        
                    }
                    else
                    {
                        CurrentGameCommand.SetStatus("Removed: " + UnitId);
                    }
                    CurrentGameCommand.AttachedUnitId = null;
                }
                if (CurrentGameCommand.FactoryUnitId == UnitId)
                    CurrentGameCommand.FactoryUnitId = null;
                if (CurrentGameCommand.TargetUnitId == UnitId)
                    CurrentGameCommand.TargetUnitId = null;
                Changed = true;
                CurrentGameCommand = null;
            }
        }
        public void ClearGameCommand()
        {
            if (CurrentGameCommand != null)
            {
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

            if (Container != null && Container.TileContainer != null && Container.TileContainer.Contains(TileObjectType.All))
            {
                moveRecipeIngredient.TileObjectType = Container.TileContainer.TileObjects[0].TileObjectType;
                moveRecipeIngredient.Position = Pos;
                moveRecipeIngredient.Source = TileObjectType.PartContainer;
                return moveRecipeIngredient;
            }
            return null;
        }

        public MoveRecipeIngredient ConsumeIngredient(TileObjectType tileObjectType, bool searchNeighbors)
        {
            MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.TileObjectType = tileObjectType;
            moveRecipeIngredient.Count = 1;

            if (Assembler != null && Assembler.TileContainer != null && Assembler.TileContainer.Contains(tileObjectType))
            {
                if (tileObjectType == TileObjectType.All)
                    moveRecipeIngredient.TileObjectType = Assembler.TileContainer.TileObjects[0].TileObjectType;
                moveRecipeIngredient.Position = Pos;
                moveRecipeIngredient.Source = TileObjectType.PartAssembler;
                return moveRecipeIngredient;
            }
            if (Container != null && Container.TileContainer != null && Container.TileContainer.Contains(tileObjectType))
            {
                if (tileObjectType == TileObjectType.All)
                    moveRecipeIngredient.TileObjectType = Container.TileContainer.TileObjects[0].TileObjectType;
                moveRecipeIngredient.Position = Pos;
                moveRecipeIngredient.Source = TileObjectType.PartContainer;
                return moveRecipeIngredient;
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
                        moveRecipeIngredient = t.Unit.ConsumeIngredient(tileObjectType, false);
                        if (moveRecipeIngredient != null)
                            return moveRecipeIngredient;
                    }
                }
            }
            return null;
        }

        

        public bool AreAllIngredientsAvailable(Player player, Recipe recipe)
        {
            List<MoveRecipeIngredient> reservedIngredients = new List<MoveRecipeIngredient>();
            bool allFound = true;

            foreach (RecipeIngredient recipeIngredient in recipe.Ingredients)
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
            if (ingredient.Position == Pos)
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
                    if (n3.Pos == ingredient.Position)
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
            if (ingredient.Position == Pos)
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
                    if (n3.Pos == ingredient.Position)
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

        public MoveRecipeIngredient FindIngredient(TileObjectType tileObjectType, bool searchNeighbors)
        {
            MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.TileObjectType = tileObjectType;
            moveRecipeIngredient.Count = 1;

            if (Assembler != null && Assembler.TileContainer != null && Assembler.TileContainer.Contains(tileObjectType))
            {
                if (tileObjectType == TileObjectType.All)
                    moveRecipeIngredient.TileObjectType = Assembler.TileContainer.TileObjects[0].TileObjectType;
                moveRecipeIngredient.Position = Pos;
                moveRecipeIngredient.Source = TileObjectType.PartAssembler;
                return moveRecipeIngredient;
            }
            if (Container != null && Container.TileContainer != null && Container.TileContainer.Contains(tileObjectType))
            {
                if (tileObjectType == TileObjectType.All)
                    moveRecipeIngredient.TileObjectType = Container.TileContainer.TileObjects[0].TileObjectType;
                moveRecipeIngredient.Position = Pos;
                moveRecipeIngredient.Source = TileObjectType.PartContainer;
                return moveRecipeIngredient;
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

        public TileObject ConsumeIngredient(MoveRecipeIngredient ingredient, Dictionary<Position2, Unit> changedUnits)
        {
            TileObject tileObject = null;
            if (ingredient.Position == Pos)
            {
                if (Assembler != null && Assembler.TileContainer != null && Assembler.TileContainer.Contains(ingredient.TileObjectType))
                {
                    tileObject = Assembler.TileContainer.RemoveTileObject(ingredient.TileObjectType);
                }
                if (tileObject == null &&
                    Container != null && Container.TileContainer != null && Container.TileContainer.Contains(ingredient.TileObjectType))
                {
                    tileObject = Container.TileContainer.RemoveTileObject(ingredient.TileObjectType);
                }
                if (tileObject == null &&
                    ingredient.Source == TileObjectType.PartReactor &&
                    Reactor != null && Reactor.TileContainer != null && Reactor.TileContainer.Contains(ingredient.TileObjectType))
                {
                    tileObject = Reactor.TileContainer.RemoveTileObject(ingredient.TileObjectType);
                }
                if (tileObject == null &&
                    ingredient.Source == TileObjectType.PartWeapon &&
                    Weapon != null && Weapon.TileContainer != null && Weapon.TileContainer.Contains(ingredient.TileObjectType))
                {
                    tileObject = Weapon.TileContainer.RemoveTileObject(ingredient.TileObjectType);
                }
                if (tileObject != null && changedUnits != null && !changedUnits.ContainsKey(Pos))
                    changedUnits.Add(Pos, this);
            }
            else
            {
                Position3 position3 = new Position3(Pos);
                foreach (Position3 n3 in position3.Neighbors)
                {
                    if (n3.Pos == ingredient.Position)
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
            if (Engine == null)
            {
                if (Container != null) metal += Container.TileContainer.Count;

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
                    Weapon = new Weapon(this, 1);
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
                    Container = new Container(this, 1);
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
                    Reactor = new Reactor(this, 1);
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
                    Radar = new Radar(this, 1);
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

                    if (Assembler.BuildQueue != null)
                    {
                        moveUpdateUnitPart.BildQueue = new List<string>();
                        moveUpdateUnitPart.BildQueue.AddRange(Assembler.BuildQueue);
                    }
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

            if (CurrentGameCommand != null)
            {
                stats.MoveUpdateStatsCommand = new MoveUpdateStatsCommand();
                stats.MoveUpdateStatsCommand.GameCommandType = CurrentGameCommand.GameCommand.GameCommandType;
                stats.MoveUpdateStatsCommand.TargetPosition = CurrentGameCommand.GameCommand.TargetPosition;
                stats.MoveUpdateStatsCommand.AttachedUnitId = CurrentGameCommand.AttachedUnitId;
                stats.MoveUpdateStatsCommand.FactoryUnitId = CurrentGameCommand.FactoryUnitId;
                stats.MoveUpdateStatsCommand.Status = CurrentGameCommand.Status;
                stats.MoveUpdateStatsCommand.Alert = CurrentGameCommand.Alert;
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
                MoveRecipeIngredient realIngredient = ConsumeIngredient(moveRecipeIngredient.TileObjectType, true);
                if (realIngredient == null)
                {
                    missingIngredient = true;
                    break;
                }
                realIngredients.Add(realIngredient);
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
