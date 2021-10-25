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
        private ulong pos;
        // ulong before any moves have been processed
        [DataMember]
        public ulong Pos
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

        [DataMember]
        public Player Owner { get; set; }

        public Weapon Weapon { get; set; }
        public Engine Engine { get; set; }
        public Armor Armor { get; set; }
        public Assembler Assembler { get; set; }
        public Extractor Extractor { get; set; }
        public Container Container { get; set; }
        public Reactor Reactor { get; set; }
        public Radar Radar { get; set; }

        [DataMember]
        public string UnitId { get; set; }

        internal GameCommand CurrentGameCommand { get; private set; }

        internal void SetGameCommand(GameCommand gameCommand)
        {
            CurrentGameCommand = gameCommand;
        }
        public void ResetGameCommand()
        {
            CurrentGameCommand = null;
        }

        public int Power { get; set; }
        public bool EndlessPower { get; set; }
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
        // Just a plan, not built yet
        public bool IsGhost { get; set; }

        public bool BuilderWaitForMetal { get; set; }

        public Direction Direction { get; set; }
        public Game Game { get; set; }

        public Blueprint Blueprint { get; set; }

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
                if (Container != null) metal += Container.TileContainer.Minerals;

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

        public int CountParts()
        {
            int parts = 0;

            if (Engine != null) parts += Engine.Level;
            if (Armor != null) parts += Armor.Level;
            if (Weapon != null) parts += Weapon.Level;
            if (Assembler != null) parts += Assembler.Level;
            if (Extractor != null) parts += Extractor.Level;
            if (Container != null) parts += Container.Level;
            if (Reactor != null) parts += Reactor.Level;
            if (Radar != null) parts += Radar.Level;

            return parts;
        }

        public bool IsComplete()
        {
            return CountParts() >= 4;
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

        private Ability CreateBlueprintPart(BlueprintPart blueprintPart, int level, bool fillContainer, TileObject tileObject)
        {
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
                    
                    ability = CreateBlueprintPart(blueprintPart, blueprintPart.Level, true, tileObject);
                }
                while (ability.Level < blueprintPart.Level);
            }
            UnderConstruction = false;
        }

        public Unit(Game game, string startCode)
        {
            Game = game;

            Blueprint = game.Blueprints.FindBlueprint(startCode);
            if (Blueprint != null)
            {
                UnitId = game.GetNextUnitId("unit");
                UnderConstruction = true;
            }
            else
            {
                int p = startCode.IndexOf(':');
                if (p > 0)
                {
                    UnitId = startCode.Substring(0, p);
                    startCode = startCode.Substring(p + 1);
                }
                else
                {
                    UnitId = game.GetNextUnitId("unit");
                }
                string unitCode = "";

                string[] parts;
                if (startCode == "Engine")
                {
                    unitCode = "1";
                }
                else if (startCode == "Armor")
                {
                    unitCode = "0;1";
                }
                else if (startCode == "Weapon")
                {
                    unitCode = "0;0;1";
                }
                else if (startCode == "Assembler")
                {
                    unitCode = "0;0;0;1";
                }
                else if (startCode == "Extractor")
                {
                    unitCode = "0;0;0;0;1";
                }
                else if (startCode == "Container")
                {
                    unitCode = "0;0;0;0;0;1";
                }
                else if (startCode == "Reactor")
                {
                    unitCode = "0;0;0;0;0;0;1";
                }
                else if (startCode == "Radar")
                {
                    unitCode = "0;0;0;0;0;0;0;1";
                }
                else if (startCode == "StartFactory")
                {
                    unitCode = "1;0;0;1;1;1;0";
                }
                else if (startCode == "StartColony")
                {
                    unitCode = "0;0;0;1;1;1;1";
                }
                else if (startCode == "StartContainer")
                {
                    unitCode = "0;1;0;0;0;3";
                }
                else
                {
                    unitCode = startCode;
                }
                parts = unitCode.Split(';');

                int level;
                if (parts.Length >= 1)
                {
                    level = Convert.ToInt32(parts[0]);
                    if (level > 0)
                        this.Engine = new Engine(this, level);
                }
                if (parts.Length >= 2)
                {
                    level = Convert.ToInt32(parts[1]);
                    if (level > 0)
                        this.Armor = new Armor(this, level);
                }
                if (parts.Length >= 3)
                {
                    level = Convert.ToInt32(parts[2]);
                    if (level > 0)
                        this.Weapon = new Weapon(this, level);
                }
                if (parts.Length >= 4)
                {
                    level = Convert.ToInt32(parts[3]);
                    if (level > 0)
                        Assembler = new Assembler(this, level);
                }
                if (parts.Length >= 5)
                {
                    level = Convert.ToInt32(parts[4]);
                    if (level > 0)
                        Extractor = new Extractor(this, level);
                }
                if (parts.Length >= 6)
                {
                    if (parts[5].StartsWith("F"))
                        level = Convert.ToInt32(parts[5].Substring(1));
                    else
                        level = Convert.ToInt32(parts[5]);
                    if (level > 0)
                    {
                        Container = new Container(this, level);

                        //if (startCode == "StartFactory" || startCode == "StartColony")
                        if (parts[5].StartsWith("F"))
                        {
                            // TODOMIN
                            //Container.Mineral = Container.Capacity;
                            //Container.Capacity = 100000;
                        }
                    }
                }
                if (parts.Length >= 7)
                {
                    level = Convert.ToInt32(parts[6]);
                    if (level > 0)
                        Reactor = new Reactor(this, level);
                }
                if (parts.Length >= 8)
                {
                    level = Convert.ToInt32(parts[7]);
                    if (level > 0)
                        Radar = new Radar(this, level);
                }
                UnderConstruction = true;
            }
        }

        public bool CanFill()
        {
            if (Weapon != null)
            {
                if (Weapon.TileContainer != null && Weapon.TileContainer.Loaded < Weapon.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Assembler != null)
            {
                if (Assembler.TileContainer != null && Assembler.TileContainer.Loaded < Assembler.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Reactor != null)
            {
                if (Reactor.TileContainer != null && Reactor.TileContainer.Loaded < Reactor.TileContainer.Capacity)
                {
                    return true;
                }
            }
            if (Container != null && Container.TileContainer.Loaded < Container.TileContainer.Capacity)
            {
                return true;
            }
            return false;
        }
        


        public void Upgrade(Move move, TileObject tileObject)
        {
            MoveUpdateUnitPart moveUpdateUnitPart = move.Stats.UnitParts[0];

            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                if (moveUpdateUnitPart.PartType == blueprintPart.PartType)
                {
                    CreateBlueprintPart(blueprintPart, moveUpdateUnitPart.Level, false, tileObject);

                    moveUpdateUnitPart.TileObjects.Add(tileObject);

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
            stats.Direction = ((int)Direction);
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
                stats.MoveUpdateStatsCommand.GameCommandType = CurrentGameCommand.GameCommandType;
                stats.MoveUpdateStatsCommand.TargetPosition = CurrentGameCommand.TargetPosition;
            }

            return stats;
        }

        public void ComputePossibleMoves(List<Move> possibleMoves, List<ulong> includedulongs, MoveFilter moveFilter)
        {
            if (Assembler != null)
            {
                Assembler.ComputePossibleMoves(possibleMoves, includedulongs, moveFilter);
            }
            if (Extractor != null)
            {
                Extractor.ComputePossibleMoves(possibleMoves, includedulongs, moveFilter);
            }
            if (Weapon != null)
            {
                Weapon.ComputePossibleMoves(possibleMoves, includedulongs, moveFilter);
            }
            if (Engine != null)
            {
                Engine.ComputePossibleMoves(possibleMoves, includedulongs, moveFilter);
            }
        }

        public Ability HitBy()
        {
            if (IsDead())
            {
                throw new Exception("Dead unit hit...");
            }
            Ability partHit = null;

            if (Armor != null)
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

                        Reactor.BurnIfNeccessary();
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
            string unitinfo = " ";
            if (this.Container != null)
                unitinfo = "Container";
            if (this.Reactor != null)
                unitinfo = "Reactor";
            if (this.Assembler != null)
                unitinfo = "Assembler";
            return UnitId + " " + Owner.PlayerModel.Name + ": " + Position.GetX(pos) + "," + Position.GetY(pos) + unitinfo;
        }
    }
}
