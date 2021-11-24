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

        public int Range
        {
            get
            {
                if (Level == 1) return 12;
                if (Level == 2) return 8;
                return 12;
            }
        }
        public void BurnIfNeccessary(Dictionary<Position2, Unit> changedUnits)
        {
            if (AvailablePower == 0)
            {
                MoveRecipeIngredient moveRecipeIngredient = Unit.FindIngredient(TileObjectType.All, true);
                if (moveRecipeIngredient == null)
                {
                    if (TileContainer != null && TileContainer.Count > 0)
                    {
                        moveRecipeIngredient = new MoveRecipeIngredient();
                        moveRecipeIngredient.Position = Unit.Pos;
                        moveRecipeIngredient.Count = 1;
                        moveRecipeIngredient.TileObjectType = TileContainer.TileObjects[0].TileObjectType;
                        moveRecipeIngredient.Source = TileObjectType.PartReactor;
                    }
                }
                if (moveRecipeIngredient != null)
                {
                    AvailablePower = TileObject.GetPowerForTileObjectType(moveRecipeIngredient.TileObjectType);

                    // Animation missing, no move

                    TileObject tileObject = Unit.ConsumeIngredient(moveRecipeIngredient, changedUnits);
                    if (tileObject != null)
                       Unit.Game.Map.AddOpenTileObject(tileObject);

                    //TileContainer.Clear();
                }
                /*
                List<TileObject> tileObjects = new List<TileObject>();
                this.Unit.RemoveTileObjects(tileObjects, 1, TileObjectType.All, null);
                
                if (tileObjects.Count > 0)
                {
                    AvailablePower = TileObject.GetPowerForTileObjectType(tileObjects[0].TileObjectType);


                    Unit.Game.Map.AddOpenTileObject(tileObjects[0]);
                }*/
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

        public Reactor(Unit owner, int level) : base(owner, TileObjectType.PartReactor)
        {
            AvailablePower = 100;

            Level = level;

            TileContainer = new TileContainer();
            TileContainer.Capacity = 4;
        }
    }
}
