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
            this.range = range;
            Level = level;

            TileContainer = new TileContainer();
            TileContainer.Capacity = 4;
            TileContainer.AcceptedTileObjectTypes = TileObjectType.Burn;
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
                /*
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
                }*/
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
        public Move BurnIfNeccessary(int minPower, Dictionary<Position2, Unit> changedUnits)
        {
            Move move = null;
            if (AvailablePower < minPower)
            {
                MoveRecipeIngredient moveRecipeIngredient = Unit.FindIngredientToBurn(null);

                if (moveRecipeIngredient != null)
                {
                    AvailablePower += TileObject.GetPowerForTileObjectType(moveRecipeIngredient.TileObjectType);

                    // Animation missing, no move
                    Unit.Changed = true;
                    moveRecipeIngredient.TargetPosition = Unit.Pos;

                    TileObject tileObject = Unit.ConsumeIngredient(moveRecipeIngredient, changedUnits);
                    if (tileObject != null)
                       Unit.Game.Map.AddOpenTileObject(tileObject);

                    move = new Move();
                    move.MoveType = MoveType.Burn;
                    move.UnitId = Unit.UnitId;
                    move.Positions = new List<Position2>();
                    move.Positions.Add(Unit.Pos);
                    move.PlayerId = Unit.Owner.PlayerModel.Id;
                    move.MoveRecipe = new MoveRecipe();
                    move.MoveRecipe.Ingredients.Add(moveRecipeIngredient);

                    // Debug to burn faster
                    //TileContainer.Clear();
                }
            }
            return move;
        }

        public int ConsumePower(int remove)
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
            return removed;
        }


    }
}
