using System;
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
            blueprint.GuiScaling = 25;
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartAssembler", TileObjectType.PartAssembler, 1, 0, 4));
            blueprint.Parts.Add(new BlueprintPart("S-PartContainer", 24, 4));
            blueprint.Parts.Add(new BlueprintPart("S-PartReactor", 0, 6));

            blueprint.BlueprintUnitOrders.StoreAll();
            blueprint.BlueprintUnitOrders.Request(TileObjectType.Mineral, TileObjectState.Accept);
            blueprint.BlueprintUnitOrders.Request(TileObjectType.Wood, TileObjectState.Accept);
            blueprint.BlueprintUnitOrders.Request(TileObjectType.Stone, TileObjectState.Deny);

            Items.Add(blueprint);

            // Container
            blueprint = new Blueprint();
            blueprint.Name = "Container";
            blueprint.Layout = "S-Container";
            blueprint.GuiScaling = 25;
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartContainer", TileObjectType.PartContainer, 3, 72, 4));
            blueprint.BlueprintUnitOrders.StoreAll();

            Items.Add(blueprint);

            // Turret
            blueprint = new Blueprint();
            blueprint.Name = "Turret";
            blueprint.Layout = "S-Turret";
            blueprint.GuiScaling = 25;
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartWeapon", TileObjectType.PartWeapon, 3, 3, 4));
            blueprint.BlueprintUnitOrders.AcceptAmmo();

            Items.Add(blueprint);

            // Reactor
            blueprint = new Blueprint();
            blueprint.Name = "Reactor";
            blueprint.Layout = "S-Reactor";
            blueprint.GuiScaling = 25;
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartReactor", TileObjectType.PartReactor, 3, 0, 8));
            blueprint.BlueprintUnitOrders.StoreAll();
            blueprint.BlueprintUnitOrders.Request(TileObjectType.Mineral, TileObjectState.Accept);
            blueprint.BlueprintUnitOrders.Request(TileObjectType.Wood, TileObjectState.Accept);
            blueprint.BlueprintUnitOrders.Request(TileObjectType.Stone, TileObjectState.Accept);

            Items.Add(blueprint);

            // Factory
            blueprint = new Blueprint();
            blueprint.Name = "Factory";
            blueprint.Layout = "S-Factory";
            blueprint.GuiScaling = 25;
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartAssembler", TileObjectType.PartAssembler, 3, 0, 4));
            Items.Add(blueprint);

            // Radar
            blueprint = new Blueprint();
            blueprint.Name = "Radar";
            blueprint.Layout = "S-Radar";
            blueprint.GuiScaling = 25;
            blueprint.Parts.Add(new BlueprintPart("S-PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("S-PartRadar", TileObjectType.PartRadar, 3, 0, 8));
            Items.Add(blueprint);

            // Worker to collect Minerals
            blueprint = new Blueprint();
            blueprint.Name = "Worker";
            blueprint.Layout = "U-Worker";
            blueprint.GuiScaling = 60;
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartContainer", 12, 0));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            blueprint.BlueprintUnitOrders.StoreAll();

            Items.Add(blueprint);

            // Fighter
            blueprint = new Blueprint();
            blueprint.Name = "Fighter";
            blueprint.Layout = "U-Fighter";
            blueprint.GuiScaling = 60;
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartWeapon", 1, 2));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            blueprint.BlueprintUnitOrders.AcceptAmmo();

            Items.Add(blueprint);

            // Bomber
            blueprint = new Blueprint();
            blueprint.Name = "Bomber";
            blueprint.Layout = "U-Bomber";
            blueprint.GuiScaling = 60;
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartWeapon", TileObjectType.PartWeapon, 2, 3, 3));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.BlueprintUnitOrders.AcceptAmmo();

            Items.Add(blueprint);

            // Assembler (moving)
            blueprint = new Blueprint();
            blueprint.Name = "Assembler";
            blueprint.Layout = "U-Assembler";
            blueprint.GuiScaling = 60;
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartAssembler", TileObjectType.PartAssembler, 1, 0, 0));
            blueprint.Parts.Add(new BlueprintPart("PartExtractor"));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            Items.Add(blueprint);

            // Builder (for structures)
            blueprint = new Blueprint();
            blueprint.Name = "Builder";
            blueprint.Layout = "U-Builder";
            blueprint.GuiScaling = 60;
            blueprint.Parts.Add(new BlueprintPart("PartEngine"));
            blueprint.Parts.Add(new BlueprintPart("PartAssembler", TileObjectType.PartAssembler, 1, 0, 0));
            blueprint.Parts.Add(new BlueprintPart("PartReactor", TileObjectType.PartReactor, 1, 0, 2));
            blueprint.Parts.Add(new BlueprintPart("PartArmor"));
            Items.Add(blueprint);

            Commands = new List<BlueprintCommand>();

            // Commands
            BlueprintCommand blueprintCommand;
            BlueprintCommandItem blueprintCommandItem;

            // Collect resources
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Collect";
            blueprintCommand.Layout = "UINone";
            blueprintCommand.GameCommandType = GameCommandType.Collect;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Worker";
            blueprintCommandItem.Position3 = new Position3();
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);


            // Fighter
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Fighter";
            blueprintCommand.Layout = "UINone";
            blueprintCommand.GameCommandType = GameCommandType.Build;
            blueprintCommand.FollowUpUnitCommand = FollowUpUnitCommand.Attack;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Fighter";
            blueprintCommandItem.Position3 = new Position3();
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);


            // Bomber
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Bomber";
            blueprintCommand.Layout = "UINone";
            blueprintCommand.GameCommandType = GameCommandType.Build;
            blueprintCommand.FollowUpUnitCommand = FollowUpUnitCommand.Attack;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Bomber";
            blueprintCommandItem.Position3 = new Position3();
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Worker
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Worker";
            blueprintCommand.Layout = "UINone";
            blueprintCommand.GameCommandType = GameCommandType.Build;
            blueprintCommand.FollowUpUnitCommand = FollowUpUnitCommand.HoldPosition;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Worker";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Assembler
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Assembler";
            blueprintCommand.Layout = "UINone";
            blueprintCommand.GameCommandType = GameCommandType.Build;
            blueprintCommand.FollowUpUnitCommand = FollowUpUnitCommand.HoldPosition;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Assembler";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Structures

            // Build outpost
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Outpost";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Outpost";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Container
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Container";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Container";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Turret
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Turret";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Turret";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Reactor
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Reactor";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Reactor";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);


            // Build Assembler
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Factory";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Factory";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);

            // Build Radar
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Radar";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem(blueprintCommand);
            blueprintCommandItem.BlueprintName = "Radar";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);


            // Build Fighter
            /*
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Fighter";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Fighter";
            blueprintCommand.Units.Add(blueprintCommandItem);

            Commands.Add(blueprintCommand);
            */

            // Build Bomber
            /*
            blueprintCommand = new BlueprintCommand();

            blueprintCommand.Name = "Bomber";
            blueprintCommand.Layout = "UIBuild";
            blueprintCommand.GameCommandType = GameCommandType.Build;

            blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = "Bomber";
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
        public FollowUpUnitCommand FollowUpUnitCommand { get; set; }

        public List<BlueprintCommandItem> Units { get; private set; }

        /*
        public BlueprintCommand CopySelf()
        {
            BlueprintCommand mapBueprintCommand = new BlueprintCommand();

            mapBueprintCommand.GameCommandType = GameCommandType;
            mapBueprintCommand.Layout = Layout;
            mapBueprintCommand.Name = Name;
            foreach (BlueprintCommandItem mapBlueprintCommandItem in Units)
            {
                BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
                blueprintCommandItem.BlueprintName = mapBlueprintCommandItem.BlueprintName;
                blueprintCommandItem.Direction = mapBlueprintCommandItem.Direction;
                blueprintCommandItem.Position3 = mapBlueprintCommandItem.Position3;
                mapBueprintCommand.Units.Add(blueprintCommandItem);
            }
            return mapBueprintCommand;
        }
        
        public MapBlueprintCommand Copy()
        {
            MapBlueprintCommand mapBueprintCommand = new MapBlueprintCommand();

            mapBueprintCommand.GameCommandType = GameCommandType;
            mapBueprintCommand.Layout = Layout;
            mapBueprintCommand.Name = Name;
            foreach (BlueprintCommandItem mapBlueprintCommandItem in Units)
            {
                MapBlueprintCommandItem blueprintCommandItem = new MapBlueprintCommandItem();
                blueprintCommandItem.BlueprintName = mapBlueprintCommandItem.BlueprintName;
                blueprintCommandItem.Direction = mapBlueprintCommandItem.Direction;
                blueprintCommandItem.Position3 = mapBlueprintCommandItem.Position3;
                blueprintCommandItem.RotatedDirection = mapBlueprintCommandItem.RotatedDirection;
                blueprintCommandItem.RotatedPosition3 = mapBlueprintCommandItem.RotatedPosition3;

                mapBueprintCommand.Units.Add(blueprintCommandItem);
            }
            return mapBueprintCommand;
        }*/

        public override string ToString()
        {
            return Name;
        }
    }

    public class BlueprintCommandItem
    {
        public BlueprintCommandItem(BlueprintCommand blueprintCommand)
        {
            BlueprintCommand = blueprintCommand;
        }
        public BlueprintCommand BlueprintCommand { get; private set; }
        public Position3 Position3 { get; set; }
        public Direction Direction { get; set; }
        public string BlueprintName { get; set; }
    }

    public class Blueprint
    {
        public Blueprint()
        {
            Parts = new List<BlueprintPart>();
            BlueprintUnitOrders = new BlueprintUnitOrders();
        }
        public string Name { get; set; }
        public int GuiScaling { get; set; }
        public string Layout { get; set; }

        public List<BlueprintPart> Parts { get; private set; }
        public BlueprintUnitOrders BlueprintUnitOrders { get; private set; }
        private bool? isMovable;
        public bool IsMoveable()
        {
            if (!isMovable.HasValue)
            {
                isMovable = false;
                foreach (BlueprintPart blueprintPart in Parts)
                {
                    if (blueprintPart.PartType == TileObjectType.PartEngine)
                    {
                        isMovable = true;
                        break;
                    }
                }    
            
            }
            return isMovable.Value;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class BlueprintUnitItemOrder
    {
        public TileObjectType TileObjectType { get; set; }
        public TileObjectState TileObjectState { get; set; }
    }

    public class BlueprintUnitOrders
    {
        public BlueprintUnitOrders()
        {
            BlueprintItemOrders = new List<BlueprintUnitItemOrder>();
        }

        public void Request(TileObjectType tileObjectType, TileObjectState tileObjectState)
        {
            foreach (BlueprintUnitItemOrder unitItemOrder in this.BlueprintItemOrders)
            {
                if (unitItemOrder.TileObjectType == tileObjectType)
                {
                    unitItemOrder.TileObjectState = tileObjectState;
                }
            }
        }

        public void AcceptAmmo()
        {
            BlueprintUnitItemOrder unitItemOrder;

            unitItemOrder = new BlueprintUnitItemOrder();
            unitItemOrder.TileObjectType = TileObjectType.Stone;
            unitItemOrder.TileObjectState = TileObjectState.Accept;
            BlueprintItemOrders.Add(unitItemOrder);

            unitItemOrder = new BlueprintUnitItemOrder();
            unitItemOrder.TileObjectType = TileObjectType.Mineral;
            unitItemOrder.TileObjectState = TileObjectState.Accept;
            BlueprintItemOrders.Add(unitItemOrder);

            unitItemOrder = new BlueprintUnitItemOrder();
            unitItemOrder.TileObjectType = TileObjectType.Wood;
            unitItemOrder.TileObjectState = TileObjectState.Accept;
            BlueprintItemOrders.Add(unitItemOrder);
        }

        public void StoreAll()
        {
            BlueprintUnitItemOrder unitItemOrder;

            unitItemOrder = new BlueprintUnitItemOrder();
            unitItemOrder.TileObjectType = TileObjectType.Mineral;
            unitItemOrder.TileObjectState = TileObjectState.None;
            BlueprintItemOrders.Add(unitItemOrder);

            unitItemOrder = new BlueprintUnitItemOrder();
            unitItemOrder.TileObjectType = TileObjectType.Wood;
            unitItemOrder.TileObjectState = TileObjectState.None;
            BlueprintItemOrders.Add(unitItemOrder);

            unitItemOrder = new BlueprintUnitItemOrder();
            unitItemOrder.TileObjectType = TileObjectType.Stone;
            unitItemOrder.TileObjectState = TileObjectState.None;
            BlueprintItemOrders.Add(unitItemOrder);
        }

        public List<BlueprintUnitItemOrder> BlueprintItemOrders { get; set; }
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

        public BlueprintPart(string name, int capacity, int range)
        {
            Name = name;
            Capacity = capacity;
            Range = range;
            DetactPartType();
        }

        public BlueprintPart(string name, TileObjectType partType, int level, int capacity, int range)
        {
            Name = name;
            Capacity = capacity;
            PartType = partType;
            Range = range;
            Level = level;
        }

        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public int Range { get; set; }
        public string Name { get; set; }
        public int? Capacity { get; set; }

    }
}
