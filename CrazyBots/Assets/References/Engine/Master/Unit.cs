﻿using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    // 0 Points in Engine: Cannot move
    // 1 Points in Engine: Move slow
    // 2 Points in Engine: Move fast
    // 3 Points in Engine: Move very fast

    // 0 Points in Armor: None
    // 1 Points in Armor: Armor 1
    // 2 Points in Armor: Armor 2
    // 3 Points in Armor: Armor 3

    // 0 Points in Weapon: Cannot fire
    // 1 Points in Weapon: Damage 1
    // 2 Points in Weapon: Damage 2
    // 3 Points in Weapon: Damage 3

    // 0 Points in Assembler: None
    // 1 Points in Assembler: Produce Level 1 Items
    // 2 Points in Assembler: Produce Level 1 Items + Upgrade
    // 3 Points in Assembler: Produce Level 1 Items + Upgrade + Faster

    // Extract from Enemy if Next, Ground with Range, from own Unit to destroy
    // 0 Points in Extractor: None
    // 1 Points in Extractor: Range 1
    // 2 Points in Extractor: Range 2
    // 3 Points in Extractor: Range 3

    // Contains Metal
    // 0 Points in Container: None
    // 1 Points in Container: Storage 1
    // 2 Points in Container: Storage 2
    // 3 Points in Container: Storage 3

    // Produce Energy consuming Metal
    // 0 Points in Reactor: None
    // 1 Points in Reactor: Range 1 Efficiency
    // 2 Points in Reactor: Range 2
    // 3 Points in Reactor: Range 3

    // Dispatch & Radar
    // 0 Points in Dispatcher: None
    // 1 Points in Dispatcher: Range 1
    // 2 Points in Dispatcher: Range 2
    // 3 Points in Dispatcher: Range 3


    public enum Direction
    {
        C,
        N,
        S,
        NE,
        NW,
        SE,
        SW
    }

    // Total Points: 4 increase with xp

    [DataContract]
    public class Unit
    {
        private Position pos;
        // Position before any moves have been processed
        [DataMember]
        public Position Pos
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

        public List<GameCommand> GameCommands { get; set; }
        public int Power { get; set; }
        public int MaxPower { get; set; }

        // Unit can be extracted
        public bool ExtractMe { 
            get; 
            set; }
        public bool UnderConstruction { get; set; }
        // Just a plan, not built yet
        public bool IsGhost { get; set; }

        public bool BuilderWaitForMetal { get; set; }

        public Direction Direction { get; set; }
        public Game Game { get; set; }

        public Blueprint Blueprint { get; set; }

        public int CountMetal()
        {
            int metal = 0;
            if (Container != null)
            {
                metal += Container.Metal;
                metal++;
            }

            // Every part is one metal
            if (Engine != null) metal++;
            if (Armor != null) metal++;
            if (Weapon != null)
            {
                if (Weapon.Container != null)
                    metal += Weapon.Container.Metal;
                metal++;
            }
            if (Assembler != null)
            {
                if (Assembler.Container != null)
                    metal += Assembler.Container.Metal;
                metal++;
            }
            if (Extractor != null) metal++;
            if (Container != null) metal++;
            if (Reactor != null)
            {
                if (Reactor.Container != null)
                    metal += Reactor.Container.Metal;
                metal++;
            }
            if (Radar != null) metal++;

            return metal;
        }

        public int CountMineralsInContainer()
        {
            int metal = 0;
            if (Engine == null)
            {
                if (Container != null) metal += Container.Metal;

                if (Weapon != null)
                {
                    if (Weapon.Container != null)
                        metal += Weapon.Container.Metal;
                }
                if (Assembler != null)
                {
                    if (Assembler.Container != null)
                        metal += Assembler.Container.Metal;
                }
                if (Reactor != null)
                {
                    if (Reactor.Container != null)
                        metal += Reactor.Container.Metal;
                }
            }
            return metal;
        }

        public int CountCapacity()
        {
            int capacity = 0;
            if (Engine == null)
            {
                if (Container != null) capacity += Container.Capacity;

                if (Weapon != null)
                {
                    if (Weapon.Container != null)
                    {
                        if (Weapon.Container.Capacity > 1)
                            capacity += Weapon.Container.Capacity;
                    }
                }
                if (Assembler != null)
                {
                    if (Assembler.Container != null)
                        capacity += Assembler.Container.Capacity;
                }
                if (Reactor != null)
                {
                    if (Reactor.Container != null)
                        capacity += Reactor.Container.Capacity;
                }
            }
            return capacity;
        }

        public bool IsComplete()
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

            return parts >= 4;
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

        private void CreateBlueprintPart(BlueprintPart blueprintPart, bool fillContainer)
        {
            int level = 1;
            if (blueprintPart.PartType.EndsWith("2"))
                level = 2;
            else if (blueprintPart.PartType.EndsWith("3"))
                level = 3;

            if (blueprintPart.PartType.StartsWith("Engine"))
            {
                if (Engine == null)
                {
                    Engine = new Engine(this, 1);
                }
                while (level > Engine.Level)
                    Engine.Level++;
            }

            else if (blueprintPart.PartType.StartsWith("Armor"))
            {
                if (Armor == null)
                {
                    Armor = new Armor(this, 1);
                }
                while (level > Armor.Level)
                    Armor.Level++;
            }


            else if (blueprintPart.PartType.StartsWith("Extractor"))
            {
                if (Extractor == null)
                {
                    Extractor = new Extractor(this, 1);
                }
                while (level > Extractor.Level)
                    Extractor.Level++;
            }

            else if (blueprintPart.PartType.StartsWith("Assembler"))
            {
                if (Assembler == null)
                {
                    Assembler = new Assembler(this, 1);
                    if (blueprintPart.Capacity.HasValue)
                        Assembler.Container.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
                    {
                        Assembler.Container.Metal = Assembler.Container.Capacity;
                    }
                }
                while (level > Assembler.Level)
                    Assembler.Level++;
            }
            else if (blueprintPart.PartType.StartsWith("Weapon"))
            {
                if (Weapon == null)
                {
                    Weapon = new Weapon(this, 1);
                    if (blueprintPart.Capacity.HasValue)
                        Weapon.Container.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
                    {
                        Weapon.Container.Metal = Weapon.Container.Capacity;
                    }
                }
                while (level > Weapon.Level)
                    Weapon.Level++;
            }

            else if (blueprintPart.PartType.StartsWith("Container"))
            {
                if (Container == null)
                {
                    Container = new Container(this, 1);
                    if (blueprintPart.Capacity.HasValue)
                        Container.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
                    {
                        Container.Metal = Container.Capacity;
                    }
                }
                while (level > Container.Level)
                    Container.Level++;
            }

            else if (blueprintPart.PartType.StartsWith("Reactor"))
            {
                if (Reactor == null)
                {
                    Reactor = new Reactor(this, 1);
                    if (blueprintPart.Capacity.HasValue)
                        Reactor.Container.Capacity = blueprintPart.Capacity.Value;
                    if (fillContainer)
                    {
                        Reactor.Container.Metal = Reactor.Container.Capacity;
                    }
                }
                while (level > Reactor.Level)
                    Reactor.Level++;
            }

            else if (blueprintPart.PartType.StartsWith("Radar"))
            {
                if (Radar == null)
                {
                    Radar = new Radar(this, 1);
                }
                while (level > Radar.Level)
                    Radar.Level++;
            }
        }

        public bool IsInstalled(BlueprintPart blueprintPart)
        {
            if (blueprintPart.PartType == "Engine")
                return Engine != null && Engine.Level == 1;
            if (blueprintPart.PartType == "Engine2")
                return Engine != null && Engine.Level == 2;
            if (blueprintPart.PartType == "Engine3")
                return Engine != null && Engine.Level == 3;

            if (blueprintPart.PartType == "Armor")
                return Armor != null && Armor.Level == 1;
            if (blueprintPart.PartType == "Armor2")
                return Armor != null && Armor.Level == 2;
            if (blueprintPart.PartType == "Armor3")
                return Armor != null && Armor.Level == 3;

            if (blueprintPart.PartType == "Weapon")
                return Weapon != null && Weapon.Level == 1;
            if (blueprintPart.PartType == "Weapon2")
                return Weapon != null && Weapon.Level == 2;
            if (blueprintPart.PartType == "Weapon3")
                return Weapon != null && Weapon.Level == 3;

            if (blueprintPart.PartType == "Assembler")
                return Assembler != null && Assembler.Level == 1;
            if (blueprintPart.PartType == "Assembler2")
                return Assembler != null && Assembler.Level == 2;
            if (blueprintPart.PartType == "Assembler3")
                return Assembler != null && Assembler.Level == 3;

            if (blueprintPart.PartType == "Extractor")
                return Extractor != null && Extractor.Level == 1;
            if (blueprintPart.PartType == "Extractor2")
                return Extractor != null && Extractor.Level == 2;
            if (blueprintPart.PartType == "Extractor3")
                return Extractor != null && Extractor.Level == 3;

            if (blueprintPart.PartType == "Container")
                return Container != null && Container.Level == 1;
            if (blueprintPart.PartType == "Container2")
                return Container != null && Container.Level == 2;
            if (blueprintPart.PartType == "Container3")
                return Container != null && Container.Level == 3;

            if (blueprintPart.PartType == "Reactor")
                return Reactor != null && Reactor.Level == 1;
            if (blueprintPart.PartType == "Reactor2")
                return Reactor != null && Reactor.Level == 2;
            if (blueprintPart.PartType == "Reactor3")
                return Reactor != null && Reactor.Level == 3;

            if (blueprintPart.PartType == "Radar")
                return Radar != null && Radar.Level == 1;
            if (blueprintPart.PartType == "Radar2")
                return Radar != null && Radar.Level == 2;
            if (blueprintPart.PartType == "Radar3")
                return Radar != null && Radar.Level == 3;

            return false;
        }

        public void CreateAllPartsFromBlueprint()
        {
            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                CreateBlueprintPart(blueprintPart, true);
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
                            Container.Metal = Container.Capacity;
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
                UnderConstruction = true; // !IsComplete();
            }
        }

        public bool CanFill()
        {
            if (Weapon != null)
            {
                if (Weapon.Container != null && Weapon.Container.Metal < Weapon.Container.Capacity)
                {
                    return true;
                }
            }
            if (Assembler != null)
            {
                if (Assembler.Container != null && Assembler.Container.Metal < Assembler.Container.Capacity)
                {
                    return true;
                }
            }
            if (Reactor != null)
            {
                if (Reactor.Container != null && Reactor.Container.Metal < Reactor.Container.Capacity)
                {
                    return true;
                }
            }
            if (Container != null && Container.Metal < Container.Capacity)
            {
                return true;
            }
            return false;
        }
    

        public void Upgrade(string unitCode)
        {
            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                if (blueprintPart.Name == unitCode)
                {
                    CreateBlueprintPart(blueprintPart, false);
                    break;
                }
            }
            if (UnderConstruction && IsComplete())
            {
                UnderConstruction = false;
            }
            /*
            if (unitCode == "Engine")
            {
                if (Engine == null)
                    Engine = new Engine(this, 1);
                else
                    Engine.Level++;
            }
            if (unitCode == "Armor")
            {
                if (Armor == null)
                    Armor = new Armor(this, 1);
                else
                    Armor.Level++;
            }
            if (unitCode == "Weapon")
            {
                if (Weapon == null)
                    Weapon = new Weapon(this, 1);
                else
                    Weapon.Level++;
            }
            if (unitCode == "Assembler")
            {
                if (Assembler == null)
                    Assembler = new Assembler(this, 1);
                else
                    Assembler.Level++;
            }
            if (unitCode == "Extractor")
            {
                if (Extractor == null)
                    Extractor = new Extractor(this, 1);
                else
                    Extractor.Level++;
            }
            if (unitCode == "Container")
            {
                if (Container == null)
                    Container = new Container(this, 1);
                else
                    Container.Level++;
            }
            if (unitCode == "Reactor")
            {
                if (Reactor == null)
                    Reactor = new Reactor(this, 1);
                else
                    Reactor.Level++;
            }
            if (unitCode == "Radar")
            {
                if (Radar == null)
                    Radar = new Radar(this, 1);
                else
                    Radar.Level++;
            }*/

            /*
            if (IsComplete())
            {
                UnderConstruction = false;
            }
            else
            {
                UnderConstruction = true;
            }*/
            ExtractMe = false;
        }

        public MoveUpdateStats CollectStats()
        {
            MoveUpdateStats stats = new MoveUpdateStats();
            stats.BlueprintName = Blueprint.Name;
            stats.MarkedForExtraction = ExtractMe;

            stats.UnitParts = new List<MoveUpdateUnitPart>();
            foreach (BlueprintPart blueprintPart in Blueprint.Parts)
            {
                MoveUpdateUnitPart moveUpdateUnitPart = new MoveUpdateUnitPart();

                moveUpdateUnitPart.Name = blueprintPart.Name;
                moveUpdateUnitPart.Exists = IsInstalled(blueprintPart);
                moveUpdateUnitPart.PartType = blueprintPart.PartType;

                if (moveUpdateUnitPart.Exists)
                {
                    if (blueprintPart.PartType.StartsWith( "Weapon"))
                    {
                        moveUpdateUnitPart.Minerals = Weapon.Container.Metal;
                        moveUpdateUnitPart.Capacity = Weapon.Container.Capacity;
                    }
                    if (blueprintPart.PartType.StartsWith("Assembler"))
                    {
                        moveUpdateUnitPart.Minerals = Assembler.Container.Metal;
                        moveUpdateUnitPart.Capacity = Assembler.Container.Capacity;

                        if (Assembler.BuildQueue != null)
                        {
                            moveUpdateUnitPart.BildQueue = new List<string>();
                            moveUpdateUnitPart.BildQueue.AddRange(Assembler.BuildQueue);
                        }
                    }
                    if (blueprintPart.PartType.StartsWith("Container"))
                    {
                        moveUpdateUnitPart.Minerals = Container.Metal;
                        moveUpdateUnitPart.Capacity = Container.Capacity;
                    }
                    if (blueprintPart.PartType.StartsWith("Reactor"))
                    {
                        moveUpdateUnitPart.AvailablePower = Reactor.AvailablePower;
                        moveUpdateUnitPart.Minerals = Reactor.Container.Metal;
                        moveUpdateUnitPart.Capacity = Reactor.Container.Capacity;
                    }
                    if (blueprintPart.PartType.StartsWith("Armor"))
                    {
                        moveUpdateUnitPart.ShieldActive = Armor.ShieldActive;
                        moveUpdateUnitPart.ShieldPower = Armor.ShieldPower;
                    }
                }
                stats.UnitParts.Add(moveUpdateUnitPart);
            }

            // OLD Code
            if (Engine != null)
                stats.EngineLevel = Engine.Level;
            if (Armor != null)
                stats.ArmorLevel = Armor.Level;
            if (Weapon != null)
            {
                stats.WeaponLevel = Weapon.Level;
                stats.WeaponLoaded = Weapon.WeaponLoaded;
            }
            if (Assembler != null)
            {
                stats.ProductionLevel = Assembler.Level;
                stats.CanProduce = Assembler.CanProduce();
            }
            if (Extractor != null)
            {
                stats.ExtractorLevel = Extractor.Level;
            }
            if (Container != null)
            {
                stats.ContainerLevel = Container.Level;
                stats.ContainerFull = (Container.Metal *100) / Container.Capacity;
                if (stats.ContainerFull > 100)
                {
                    // Extract bug..
                    stats.ContainerFull = 100;                    
                }
            }
            if (Reactor != null)
            {
                stats.ReactorLevel = Reactor.Level;
            }
            if (Radar != null)
            {
                stats.RadarLevel = Radar.Level;
            }
            stats.Power = Power;

            return stats;
        }

        public void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if (Assembler != null)
            {
                Assembler.ComputePossibleMoves(possibleMoves, includedPositions, moveFilter);
            }
            if (Extractor != null)
            {
                Extractor.ComputePossibleMoves(possibleMoves, includedPositions, moveFilter);
            }
            if (Weapon != null)
            {
                Weapon.ComputePossibleMoves(possibleMoves, includedPositions, moveFilter);
            }
            if (Engine != null)
            {
                Engine.ComputePossibleMoves(possibleMoves, includedPositions, moveFilter);
            }
        }

        public virtual bool HitBy(Unit otherUnit)
        {
            // No collisions? No shots also if false
            //return false;

            bool dead = false;

            if (this.Armor != null)
            {
                if (Armor.ShieldActive)
                {
                    Armor.ShieldHit();
                }
                else
                {
                    Armor.Level--;
                    if (Armor.Level <= 0)
                    {
                        this.Armor = null;
                    }
                }
            }
            else
            {
                bool damageDone = false;

                while (damageDone == false)
                {
                    int damageType = Game.Random.Next(7);
                    if (damageType == 0)
                    {
                        if (Weapon != null && Weapon.Level > 0)
                        {
                            Weapon.Level--;
                            if (Weapon.Level == 0)
                            {
                                Weapon = null;
                            }
                            damageDone = true;
                        }
                    }
                    else if (damageType == 1)
                    {
                        if (Engine != null && Engine.Level > 0)
                        {
                            Engine.Level--;
                            if (Engine.Level == 0)
                            {
                                Engine = null;
                            }
                            damageDone = true;
                        }
                    }
                    else if (damageType == 2)
                    {
                        if (Assembler != null && Assembler.Level > 0)
                        {
                            Assembler.Level--;
                            if (Assembler.Level == 0)
                            {
                                Assembler = null;
                            }
                            damageDone = true;
                        }
                    }
                    else if (damageType == 3)
                    {
                        if (Extractor != null && Extractor.Level > 0)
                        {
                            Extractor.Level--;
                            if (Extractor.Level == 0)
                            {
                                Extractor = null;
                            }
                            damageDone = true;
                        }
                    }
                    else if (damageType == 4)
                    {
                        if (Container != null && Container.Level > 0)
                        {
                            Container.Level--;
                            if (Container.Level == 0)
                            {
                                Container = null;
                            }
                            damageDone = true;
                        }
                    }
                    else if (damageType == 5)
                    {
                        if (Reactor != null && Reactor.Level > 0)
                        {
                            Reactor.Level--;
                            if (Reactor.Level == 0)
                            {
                                Reactor = null;
                            }
                            damageDone = true;
                        }
                    }
                    else if (damageType == 6)
                    {
                        if (Radar != null && Radar.Level > 0)
                        {
                            Radar.Level--;
                            if (Radar.Level == 0)
                            {
                                Radar = null;
                            }
                            damageDone = true;
                        }
                    }
                }
            }

            if (Armor == null && Weapon == null && Engine == null && Assembler == null && Container == null && Reactor == null && Radar == null)
            {
                dead = true;
            }
            return dead;
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
            return UnitId + " " + Owner.PlayerModel.Name + ": " + pos.X + "," + pos.Y + unitinfo;
        }
    }
}
