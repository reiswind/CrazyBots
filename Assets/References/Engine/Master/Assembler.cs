
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
        }

        /// <summary>
        /// Will only create a unitid and reserve the postion. 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="productCode"></param>
        /// <returns></returns>
        private Move CreateAssembleMove(Position2 pos, Blueprint blueprint, MoveRecipeIngredient moveRecipeIngredient)
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
            move.Stats.BlueprintName = blueprint.Name;

            move.MoveRecipe = new MoveRecipe();
            move.MoveRecipe.Ingredients = new List<MoveRecipeIngredient>();
            move.MoveRecipe.Ingredients.Add(moveRecipeIngredient);
            move.MoveRecipe.Result = blueprint.Parts[0].PartType;

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

        

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            MoveRecipeIngredient moveRecipeIngredient = Unit.FindIngredient(TileObjectType.Mineral, true, null);
            if (moveRecipeIngredient != null)
            {
                ComputePossibleMoves(possibleMoves, includedPosition2s, moveFilter, moveRecipeIngredient);
            }
        }
        public void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter, MoveRecipeIngredient moveRecipeIngredient)
        {
            if ((moveFilter & MoveFilter.Assemble) == 0 && (moveFilter & MoveFilter.Upgrade) == 0)
                return;

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
                                    possibleMoves.Add(CreateAssembleMove(neighbor.Pos, blueprint, moveRecipeIngredient));
                                }
                            }
                            else
                            {
                                Blueprint blueprint = Unit.Owner.Game.Blueprints.FindBlueprint(Unit.CurrentGameCommand.BlueprintName);
                                possibleMoves.Add(CreateAssembleMove(neighbor.Pos, blueprint, moveRecipeIngredient));
                            }
                        }
                    }
                }
                else
                {
                    if (neighbor.Unit.CurrentGameCommand != null)
                    {
                        if (neighbor.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                            neighbor.Unit.CurrentGameCommand.AttachedUnit.UnitId == neighbor.Unit.UnitId)
                        {
                            // The unit to upgrade is upgraded by a command. If this unit is not the factory,
                            // do not upgrade the unit
                            if (neighbor.Unit.CurrentGameCommand.FactoryUnit != null &&
                                neighbor.Unit.CurrentGameCommand.FactoryUnit.UnitId != Unit.UnitId)
                            {
                                // Do not upgrade. This will lead to double upgrade, cause the command factory will try to
                                // upgrade too
                                continue;
                            }
                        }

                    }

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
