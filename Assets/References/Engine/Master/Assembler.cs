
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Assembler : Ability
    {
        public override string Name { get { return "Assembler"; } }

        public List<string> BuildQueue { get; set; }

        public bool CanProduce()
        {
            if (Unit.Power == 0)
                return false;
            if (TileContainer != null && TileContainer.Minerals > 0)
                return true;
            if (Unit.Container != null && Unit.Container.TileContainer.Minerals> 0)
                return true;
            return false;
        }

        public Assembler(Unit owner, int level) : base(owner, TileObjectType.PartAssembler)
        {
            Level = level;
            TileContainer = new TileContainer();
            TileContainer.Capacity = 4;
            TileContainer.AcceptedTileObjectTypes = TileObjectType.Mineral;
        }

        /// <summary>
        /// Will only create a unitid and reserve the postion. 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="productCode"></param>
        /// <returns></returns>
        private Move CreateAssembleMove(Position2 pos, string productCode)
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Build;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.OtherUnitId = Unit.UnitId;

            move.Stats = new MoveUpdateStats();
            move.Stats.BlueprintName = productCode;

            return move;
        }

        private Move CreateUpgradeMove(Position2 pos, Unit assemblerUnit, Unit upgradedUnit, MoveRecipeIngredient moveRecipeIngredient, BlueprintPart blueprintPart) 
        {
            Move move;

            // possible production move
            move = new Move();
            move.MoveType = MoveType.Upgrade;
            move.Positions = new List<Position2>();
            move.Positions.Add(Unit.Pos);
            move.Positions.Add(pos);
            move.PlayerId = Unit.Owner.PlayerModel.Id;
            move.UnitId = assemblerUnit.UnitId;
            move.OtherUnitId = upgradedUnit.UnitId;

            move.MoveRecipe = new MoveRecipe();
            move.MoveRecipe.Ingredients = new List<MoveRecipeIngredient>();
            move.MoveRecipe.Ingredients.Add(moveRecipeIngredient);
            move.MoveRecipe.Result = blueprintPart.PartType;

            return move;
        }

        public List<TileObject> ConsumeIngredients(MoveRecipe moveRecipe)
        {
            List<MoveRecipeIngredient> realIngredients = new List<MoveRecipeIngredient>();

            bool missingIngredient = false;
            foreach (MoveRecipeIngredient moveRecipeIngredient in moveRecipe.Ingredients)
            {
                MoveRecipeIngredient realIngredient = Unit.FindIngredient(moveRecipeIngredient.TileObjectType, true);
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
            foreach (MoveRecipeIngredient realIngredient in realIngredients )
            {
                Unit.ConsumeIngredient(realIngredient);
                moveRecipe.Ingredients.Add(realIngredient);
            }

            List<TileObject> results = new List<TileObject>();

            TileObject tileObject = new TileObject();
            tileObject.TileObjectType = moveRecipe.Result;
            tileObject.Direction = Direction.C;
            results.Add(tileObject);

            return results;
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Assemble) == 0 && (moveFilter & MoveFilter.Upgrade) == 0)
                return;

            MoveRecipeIngredient moveRecipeIngredient = Unit.FindIngredient(TileObjectType.Mineral, true);

            Dictionary<Position2, TileWithDistance> neighbors = Unit.Game.Map.EnumerateTiles(Unit.Pos, 1, false);
            
            foreach (TileWithDistance neighbor in neighbors.Values)
            {
                if (neighbor.Unit == null)
                {
                    if (!neighbor.Tile.CanBuild())
                        continue;
                }

                if (includedPosition2s != null)
                {
                    if (!includedPosition2s.Contains(neighbor.Pos))
                        continue;
                }

                if (neighbor.Unit == null)
                {
                    if ((moveFilter & MoveFilter.Assemble) > 0)
                    {
                        if (Level > 0)
                        {
                            if (Unit.CurrentGameCommand == null)
                            {
                                //Can build everything
                                foreach (Blueprint blueprint in Unit.Owner.Game.Blueprints.Items)
                                {
                                    possibleMoves.Add(CreateAssembleMove(neighbor.Pos, blueprint.Name));
                                }
                            }
                            else
                            {
                                possibleMoves.Add(CreateAssembleMove(neighbor.Pos, Unit.CurrentGameCommand.BlueprintName));
                            }
                        }
                    }
                }
                else
                {
                    if (moveRecipeIngredient != null &&
                        neighbor.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                    {
                        if (Level > 0 && !neighbor.Unit.IsComplete() && !neighbor.Unit.ExtractMe)
                        {
                            if ((moveFilter & MoveFilter.Upgrade) > 0)
                            {
                                foreach (BlueprintPart blueprintPart in neighbor.Unit.Blueprint.Parts)
                                {
                                    if (!neighbor.Unit.IsInstalled(blueprintPart, blueprintPart.Level))
                                    {
                                        possibleMoves.Add(CreateUpgradeMove(neighbor.Pos, Unit, neighbor.Unit, moveRecipeIngredient, blueprintPart));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Enemy nearby? OMG
                    }
                }
            }
        }
    }
}
