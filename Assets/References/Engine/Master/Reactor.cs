using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Reactor : Ability
    {
        public Reactor(Unit owner, int level, int range) : base(owner, TileObjectType.PartReactor)
        {
            AvailablePower = 100;
            this.range = range;
            Level = level;

            TileContainer = new TileContainer();
            TileContainer.Capacity = 4;
        }

        public override string Name { get { return "Reactor"; } }
        public int AvailablePower { get; set; }
        public int StoredPower
        {
            get
            {
                int storedPower = 0;
                if (Unit.Container != null)
                {
                    foreach (TileObject tileObject in Unit.Container.TileContainer.TileObjects)
                    {
                        storedPower += TileObject.GetPowerForTileObjectType(tileObject.TileObjectType);
                    }
                }
                if (Unit.Assembler != null && Unit.Assembler.TileContainer != null)
                {
                    foreach (TileObject tileObject in Unit.Assembler.TileContainer.TileObjects)
                    {
                        storedPower += TileObject.GetPowerForTileObjectType(tileObject.TileObjectType);
                    }
                }
                if (Unit.Weapon != null && Unit.Weapon.TileContainer != null)
                {
                    foreach (TileObject tileObject in Unit.Weapon.TileContainer.TileObjects)
                    {
                        storedPower += TileObject.GetPowerForTileObjectType(tileObject.TileObjectType);
                    }
                }
                foreach (TileObject tileObject in TileContainer.TileObjects)
                {
                    storedPower += TileObject.GetPowerForTileObjectType(tileObject.TileObjectType);
                }
                return storedPower;
            }
        }
        private int range;
        public int Range
        {
            get
            {
                return range;
            }
        }
        public void BurnIfNeccessary(Dictionary<Position2, Unit> changedUnits)
        {
            if (AvailablePower == 0)
            {
                MoveRecipeIngredient moveRecipeIngredient = Unit.FindIngredientToBurn();

                if (moveRecipeIngredient != null)
                {
                    AvailablePower = TileObject.GetPowerForTileObjectType(moveRecipeIngredient.TileObjectType);

                    // Animation missing, no move
                    Unit.Changed = true;

                    TileObject tileObject = Unit.ConsumeIngredient(moveRecipeIngredient, changedUnits);
                    if (tileObject != null)
                       Unit.Game.Map.AddOpenTileObject(tileObject);

                    //TileContainer.Clear();
                }
            }
        }

        public int ConsumePower(int remove, Dictionary<Position2, Unit> changedUnits)
        {
            //remove *= 20;

            int removed;
            if (remove > AvailablePower)
            {
                removed = AvailablePower;
                AvailablePower = 0;                
            }    
            else
            {
                AvailablePower -= remove;
                removed = remove;
            }

            if (AvailablePower == 0)
            {
                BurnIfNeccessary(changedUnits);
            }

            return removed;
        }


    }
}
