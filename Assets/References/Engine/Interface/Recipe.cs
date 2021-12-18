using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class RecipeIngredient
    {
        public RecipeIngredient(TileObjectType tileObjectType, int count)
        {
            TileObjectType = tileObjectType;
            Count = count;
        }
        public RecipeIngredient(TileObjectType tileObjectType)
        {
            TileObjectType = tileObjectType;
            Count = 1;
        }
        public TileObjectType TileObjectType { get; private set; }
        public int Count { get; private set; }

        public override string ToString()
        {
            return Count + "x " + TileObjectType.ToString();
        }
    }
    public class Recipes
    {
        public List<Recipe> RecipeList = new List<Recipe>();

        public Recipes()
        {
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartArmor)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartAssembler)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartContainer)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartEngine)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartExtractor)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartRadar)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartReactor)));
            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral), new RecipeIngredient(TileObjectType.PartWeapon)));

            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Mineral, 4), new RecipeIngredient(TileObjectType.Unit)));

            RecipeList.Add(new Recipe(new RecipeIngredient(TileObjectType.Wood, 4), new RecipeIngredient(TileObjectType.Coal)));
        }

    }
    public class Recipe
    {
        
        public Recipe(RecipeIngredient ingredients, RecipeIngredient result)
        {
            Ingredients = new List<RecipeIngredient>();
            Ingredients.Add(ingredients);
            Results = new List<RecipeIngredient>();
            Results.Add(result);
        }

        public Recipe()
        {
            Ingredients = new List<RecipeIngredient>();
            Results = new List<RecipeIngredient>();
        }

        public List<RecipeIngredient> Ingredients { get; private set; }
        public List<RecipeIngredient> Results { get; private set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (RecipeIngredient recipeIngredient in Ingredients)
            {
                sb.Append(recipeIngredient.Count);
                sb.Append("x ");
                sb.Append(recipeIngredient.TileObjectType.ToString());
            }
            sb.Append(" = ");
            foreach (RecipeIngredient recipeIngredient in Results)
            {
                sb.Append(recipeIngredient.Count);
                sb.Append("x ");
                sb.Append(recipeIngredient.TileObjectType.ToString());
            }

            return sb.ToString();
        }
    }
}
