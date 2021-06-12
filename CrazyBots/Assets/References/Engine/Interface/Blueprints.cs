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

        public void Initialize()
        {
            Blueprint blueprint;

            // Outpost
            blueprint = new Blueprint();
            blueprint.Name = "Outpost";
            blueprint.Parts.Add(new BlueprintPart("ExtractorGround1"));
            blueprint.Parts.Add(new BlueprintPart("Assembler1"));
            blueprint.Parts.Add(new BlueprintPart("Container1", true));
            blueprint.Parts.Add(new BlueprintPart("Reactor1"));
            Items.Add(blueprint);

            // Worker to collect Minerals
            blueprint = new Blueprint();
            blueprint.Name = "Worker";
            blueprint.Parts.Add(new BlueprintPart("Engine1"));
            blueprint.Parts.Add(new BlueprintPart("Extractor1"));
            blueprint.Parts.Add(new BlueprintPart("Container1"));
            blueprint.Parts.Add(new BlueprintPart("Armor1"));
            Items.Add(blueprint);

            // Fighter
            blueprint = new Blueprint();
            blueprint.Name = "Fighter";
            blueprint.Parts.Add(new BlueprintPart("Engine1"));
            blueprint.Parts.Add(new BlueprintPart("Weapon1"));
            blueprint.Parts.Add(new BlueprintPart("Extractor1"));
            blueprint.Parts.Add(new BlueprintPart("Armor1"));
            Items.Add(blueprint);

            // Assembler (moving)
            blueprint = new Blueprint();
            blueprint.Name = "Assembler";
            blueprint.Parts.Add(new BlueprintPart("Engine1"));
            blueprint.Parts.Add(new BlueprintPart("Assembler1"));
            blueprint.Parts.Add(new BlueprintPart("Extractor1"));
            blueprint.Parts.Add(new BlueprintPart("Armor1"));
            Items.Add(blueprint);

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

    public class Blueprint
    {
        public Blueprint()
        {
            Parts = new List<BlueprintPart>();
        }
        public string Name { get; set; }
        public string Layout { get; set; }

        public List<BlueprintPart> Parts { get; private set; }
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
            if (Name.Contains("Extractor"))
                PartType = "Extractor";
            if (Name.Contains("Assembler"))
                PartType = "Assembler";
            if (Name.Contains("Container"))
                PartType = "Container";
            if (Name.Contains("Armor"))
                PartType = "Armor";
            if (Name.Contains("Engine"))
                PartType = "Engine";
            if (Name.Contains("Weapon"))
                PartType = "Weapon";
            if (Name.Contains("Reactor"))
                PartType = "Reactor";
        }

        public BlueprintPart(string name, bool filled)
        {
            Name = name;
            Filled = filled;
            DetactPartType();
        }

        public string PartType { get; set; }
        public string Name { get; set; }
        public bool Filled { get; set; }
    }
}
