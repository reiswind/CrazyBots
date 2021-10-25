﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class Blueprints
    {
        public Blueprints()
        {
            Items = new List<Blueprint>();
            Initialize();
        }
        public List<Blueprint> Items { get; private set; }
        public List<BlueprintCommand> Commands { get; private set; }

        public void Initialize()
        {
            Blueprint blueprint;

            // Outpost
            blueprint = new Blueprint();
            blueprint.Name = "Outpost";
            blueprint.Layout = "S-Outpost";
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartAssembler", TileObjectType.PartAssembler, 1, 4));
            blueprint.Parts.Add(new BlueprintPart("S-PartContainer", 24));
            blueprint.Parts.Add(new BlueprintPart("S-PartReactor"));
            Items.Add(blueprint);

            // Container
            blueprint = new Blueprint();
            blueprint.Name = "Container";
            blueprint.Layout = "S-Container";
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartContainer", TileObjectType.PartContainer, 3, 72));
            Items.Add(blueprint);

            // Turret
            blueprint = new Blueprint();
            blueprint.Name = "Turret";
            blueprint.Layout = "S-Turret";
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartWeapon", TileObjectType.PartWeapon, 3, 6));
            Items.Add(blueprint);

            // Reactor
            blueprint = new Blueprint();
            blueprint.Name = "Reactor";
            blueprint.Layout = "Ground";
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartReactor", TileObjectType.PartReactor, 3, 6));
            Items.Add(blueprint);

            // Factory
            blueprint = new Blueprint();
            blueprint.Name = "Factory";
            blueprint.Layout = "Ground";
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartAssembler", TileObjectType.PartAssembler, 3, 6));
            Items.Add(blueprint);

            // Worker to collect Minerals
            blueprint = new Blueprint();
            blueprint.Name = "Worker";
            blueprint.Layout = "U-Worker";
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartContainer", 12));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            Items.Add(blueprint);

            // Fighter
            blueprint = new Blueprint();
            blueprint.Name = "Fighter";
            blueprint.Layout = "U-Fighter";
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartWeapon"));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            Items.Add(blueprint);

            // Bomber
            blueprint = new Blueprint();
            blueprint.Name = "Bomber";
            blueprint.Layout = "U-Bomber";
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartWeapon", TileObjectType.PartWeapon, 2, 3));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            Items.Add(blueprint);

            // Assembler (moving)
            blueprint = new Blueprint();
            blueprint.Name = "Assembler";
            blueprint.Layout = "U-Assembler";
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartAssembler"));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            Items.Add(blueprint);


            Commands = new List<BlueprintCommand>();

            // Commands
            BlueprintCommand blueprintCommand;
            BlueprintCommandItem blueprintCommandItem;

            // Collect resources
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Collect";
            blueprintCommand.Layout = "UICollect";
            blueprintCommand.GameCommandType = GameCommandType.Collect;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Worker";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Attack
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Attack";
            blueprintCommand.Layout = "UIAttack";
            blueprintCommand.GameCommandType = GameCommandType.Attack;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Fighter";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Defend
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Defend";
            blueprintCommand.Layout = "UIAttack";
            blueprintCommand.GameCommandType = GameCommandType.Defend;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Bomber";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);


            // Build outpost
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Outpost";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Outpost";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Container
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Container";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Container";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Container
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Turret";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Turret";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            /*
            // Build Fighter
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Fighter";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Fighter";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Bomber
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Bomber";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Bomber";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Worker
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Worker";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Worker";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);
            */
        }

        public Blueprint FindBlueprint(string name)
        {
            foreach (Blueprint blueprint in Items)
            {
                if (blueprint.Name == name)
                    return blueprint;
            }
            return null;
        }
    }

    public class BlueprintCommand
    {
        public BlueprintCommand()
        {
            Units = new List<BlueprintCommandItem>();
        }
        public string Name { get; set; }
        public string Layout { get; set; }

        public GameCommandType GameCommandType { get; set; }

        public List<BlueprintCommandItem> Units { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class BlueprintCommandItem
    {
        public Direction Direction { get; set; }
        public string BlueprintName { get; set; }
    }

    public class Blueprint
    {
        public Blueprint()
        {
            Parts = new List<BlueprintPart>();
        }
        public string Name { get; set; }
        public string Layout { get; set; }

        public List<BlueprintPart> Parts { get; private set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class BlueprintPart
    {
        public BlueprintPart()
        {
        }
        public BlueprintPart(string name)
        {
            Name = name;
            DetactPartType();
        }
        private void DetactPartType()
        {
            if (Name.Contains("Extractor") || Name.Contains("Foundation"))
                PartType = TileObjectType.PartExtractor;
            if (Name.Contains("Assembler"))
                PartType = TileObjectType.PartAssembler;
            if (Name.Contains("Container"))
                PartType = TileObjectType.PartContainer;
            if (Name.Contains("Armor"))
                PartType = TileObjectType.PartArmor;
            if (Name.Contains("Engine"))
                PartType = TileObjectType.PartEngine;
            if (Name.Contains("Weapon"))
                PartType = TileObjectType.PartWeapon;
            if (Name.Contains("Reactor"))
                PartType = TileObjectType.PartReactor;
            if (Name.Contains("Radar"))
                PartType = TileObjectType.PartRadar;
            Level = 1;
        }

        public BlueprintPart(string name, int capacity)
        {
            Name = name;
            Capacity = capacity;
            DetactPartType();
        }

        public BlueprintPart(string name, TileObjectType partType, int level, int capacity)
        {
            Name = name;
            Capacity = capacity;
            PartType = partType;
            Level = level;
        }

        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public int? Capacity { get; set; }
    }
}
