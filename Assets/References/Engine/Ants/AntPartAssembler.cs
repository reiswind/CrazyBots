
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartAssembler : AntPart
    {
        public Assembler Assembler { get; private set; }
        public AntPartAssembler(Ant ant, Assembler assembler) : base(ant)
        {
            Assembler = assembler;
        }

        public override string ToString()
        {
            return "AntPartAssembler";
        }

        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            
            bool moved;

            if (Assembler.Unit.CurrentGameCommand != null &&
                Assembler.Unit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Build &&
                Assembler.Unit.CurrentGameCommand.BuildPositionReached &&
                Assembler.Unit.CurrentGameCommand.FactoryUnit.UnitId == Ant.Unit.UnitId)
            {
                // This is the assembler unit
                moved = BuildStructure(control, player, moves);
            }
            else
            {

                moved = Assemble(control, player, moves);
            }
            return moved;
        }

        public bool BuildStructure(ControlAnt control, Player player, List<Move> moves)
        {
            List<Position2> includePositions = new List<Position2>();
            includePositions.Add(Assembler.Unit.CurrentGameCommand.GameCommand.TargetPosition);

            MoveRecipeIngredient moveRecipeIngredient = new MoveRecipeIngredient();
            moveRecipeIngredient.Count = 1;
            moveRecipeIngredient.SourcePosition = Ant.Unit.Pos;

            List<Move> possiblemoves = new List<Move>();
            if (Ant.Unit.IsComplete())
            {
                moveRecipeIngredient.Source = Ant.Unit.Engine.PartType;
                moveRecipeIngredient.TileObjectType = moveRecipeIngredient.Source;

                // Create the foundation
                Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Assemble, moveRecipeIngredient);
                
            }
            else
            {
                if (Ant.Unit.Extractor != null)
                {
                    moveRecipeIngredient.Source = Ant.Unit.Extractor.PartType;
                    moveRecipeIngredient.TileObjectType = moveRecipeIngredient.Source;
                }
                else if (Ant.Unit.Armor != null)
                {
                    moveRecipeIngredient.Source = Ant.Unit.Armor.PartType;
                    moveRecipeIngredient.TileObjectType = moveRecipeIngredient.Source;
                }
                else if (Ant.Unit.Assembler != null)
                {
                    moveRecipeIngredient.Source = Ant.Unit.Assembler.PartType;
                    moveRecipeIngredient.TileObjectType = moveRecipeIngredient.Source;
                }

                // Foundation build, upgrade
                Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Upgrade, moveRecipeIngredient);
            }
            if (possiblemoves.Count != 1)
            {
                // Impossible to build, cancel this
                Assembler.Unit.CurrentGameCommand.GameCommand.CommandCanceled = true;
                Ant.Unit.ResetGameCommand();
            }
            else
            {
                Move move = possiblemoves[0];
                move.GameCommandItem = Assembler.Unit.CurrentGameCommand;
                moves.Add(move);
            }
            return possiblemoves.Count == 1;
        }

        private void RequestIngredientsForUnit(Player player)
        {
            if (Ant.AntPartEngine != null)
            {
                // Only for structures!
                return;
            }

            foreach (GameCommand gameCommand1 in player.GameCommands)
            {
                if (gameCommand1.TargetPosition == Ant.Unit.Pos &&
                    gameCommand1.GameCommandType == GameCommandType.ItemRequest)
                {
                    // Already requested
                    return;
                }
            }
            // Need something to assemble
            GameCommand gameCommand = new GameCommand();
            gameCommand.GameCommandType = GameCommandType.ItemRequest;
            gameCommand.Layout = "UIDelivery";
            gameCommand.TargetPosition = Ant.Unit.Pos;
            gameCommand.DeleteWhenFinished = true;
            gameCommand.PlayerId = player.PlayerModel.Id;

            BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
            blueprintCommandItem.BlueprintName = Ant.Unit.Blueprint.Name;
            blueprintCommandItem.Direction = Direction.C;

            GameCommandItem gameCommandItem = new GameCommandItem(gameCommand, blueprintCommandItem);
            gameCommandItem.TargetUnit.SetUnitId(Ant.Unit.UnitId);
            gameCommandItem.TargetUnit.SetStatus("WaitingForDelivery");

            gameCommand.RequestedItems = new List<RecipeIngredient>();
            foreach (RecipeIngredient recipeIngredient in player.Game.RecipeForAnyUnit.Ingredients)
            {
                gameCommand.RequestedItems.Add(recipeIngredient);
            }

            Ant.Unit.SetGameCommand(gameCommandItem);

            gameCommand.GameCommandItems.Add(gameCommandItem);
            player.GameCommands.Add(gameCommand);
        }

        public bool Assemble(ControlAnt control, Player player, List<Move> moves)
        {
            bool upgrading = false;

            MoveRecipeIngredient moveRecipeIngredient = Ant.Unit.FindIngredient(TileObjectType.Mineral, true, null);
            if (moveRecipeIngredient == null)
            {
                if (Assembler.Unit.CurrentGameCommand != null)
                {
                    if (Assembler.Unit.CurrentGameCommand.FactoryUnit.UnitId == Ant.Unit.UnitId)
                    {
                        Assembler.Unit.CurrentGameCommand.FactoryUnit.StuckCounter++;
                        if (Assembler.Unit.CurrentGameCommand.FactoryUnit.StuckCounter > 20)
                        {
                            Assembler.Unit.ResetGameCommand();
                        }
                    }
                }
                if (Assembler.Unit.CurrentGameCommand == null)
                {
                    // Need something to assemble
                    RequestIngredientsForUnit(player);
                }
                // Cannot build a unit, no mins
                return false;
            }
            List<Position2> includePositions = null;
            if (Assembler.Unit.CurrentGameCommand != null && Ant.AntPartEngine != null)
            {
                // If engine, move to target and upgrade then. Do not upgrade anything else.
                includePositions = new List<Position2>();
                includePositions.Add(Assembler.Unit.CurrentGameCommand.GameCommand.TargetPosition);
            }
            List<Move> possiblemoves = new List<Move>();

            //if (Assembler.Unit.CurrentGameCommand != null)
            // Allow all upgrades, even without command
            Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Upgrade, moveRecipeIngredient);
            while (possiblemoves.Count > 0)
            {
                int idx = player.Game.Random.Next(possiblemoves.Count);
                Move move = possiblemoves[idx];

                if (control.IsUpgrading(player, moves, move))
                {
                    possiblemoves.RemoveAt(idx);
                    continue;
                }
                moves.Add(move);
                return true;
            }
            includePositions = null;

            if (!upgrading)
            {
                GameCommandItem selectedGameCommand = Ant.Unit.CurrentGameCommand;
                GameCommandItem passGameCommandToNewUnit;
                
                bool computePossibleMoves = true;

                if (selectedGameCommand == null)
                {
                    return false;
                }
                else
                {
                    if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.Build &&
                        selectedGameCommand.AttachedUnit.UnitId != null)
                    {
                        // The unit to be build is already attached and under construction, do not build another
                        return false;
                    }
                    if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.Build &&
                        selectedGameCommand.AttachedUnit.UnitId == Ant.Unit.UnitId)
                    {
                        // This is the command, that is attached to this factory, when the factory was build.
                        return false;
                    }
                    if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest &&
                        selectedGameCommand.FactoryUnit.UnitId != Ant.Unit.UnitId)
                    {
                        // This is not the factory to build the transporter

                        // This blocks a factory to do something, while waiting for delivery
                        return false;
                    }

                    if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
                    {
                        // Check if already built but cannot upgrade for a reason
                        Dictionary<Position2, TileWithDistance> neighbors = Assembler.Unit.Game.Map.EnumerateTiles(Assembler.Unit.Pos, 1, false);
                        foreach (TileWithDistance tileWithDistance in neighbors.Values)
                        {
                            if (tileWithDistance.Unit != null && tileWithDistance.Unit.UnitId == selectedGameCommand.AttachedUnit.UnitId)
                            {
                                // Already under construction
                                return false;
                            }
                        }
                    }

                    // Assembler should move to the target
                    if (Ant.AntPartEngine != null)
                    {
                        /*
                        if (!Ant.BuildPositionReached)
                        {
                            return false;
                        }
                        else
                        {*/
                        includePositions = new List<Position2>();
                        includePositions.Add(selectedGameCommand.GameCommand.TargetPosition);

                        passGameCommandToNewUnit = selectedGameCommand;                        
                    }
                    else
                    {
                        // Need something to assemble (careful, this reserves the indigrients for this move)
                        if (!Ant.Unit.AreAllIngredientsAvailable(player.Game.RecipeForAnyUnit.Ingredients))
                        {
                            if (Ant.AntPartEngine == null)
                            {
                                RequestIngredientsForUnit(player);
                            }
                            return false;
                        }
                        //Ant.Unit.ClearReservations();

                        // Structure: Build unit or an assembler that moves ther
                        bool engineFound = false;
                        if (!string.IsNullOrEmpty(selectedGameCommand.BlueprintName))
                        {
                            Blueprint commandBluePrint;
                            commandBluePrint = player.Game.Blueprints.FindBlueprint(selectedGameCommand.BlueprintName);

                            foreach (BlueprintPart blueprintPart in commandBluePrint.Parts)
                            {
                                if (blueprintPart.PartType == TileObjectType.PartEngine)
                                {
                                    engineFound = true;
                                    break;
                                }
                            }
                        }
                        if (engineFound)
                        {
                            Tile tile = player.Game.Map.GetTile(Ant.Unit.Pos);
                            if (tile.IsNeighbor(selectedGameCommand.GameCommand.TargetPosition))
                            {
                                // No need to build an assembler. Just build the unit at this position
                                includePositions = new List<Position2>();
                                includePositions.Add(selectedGameCommand.GameCommand.TargetPosition);
                            }

                            // Build this unit, it will move to the target COMMAND-STEP3
                            passGameCommandToNewUnit = selectedGameCommand;
                        }
                        else
                        {
                            // Check if TargetPosition is neighbor!
                            Tile tile = player.Game.Map.GetTile(Ant.Unit.Pos);
                            if (tile.IsNeighbor(selectedGameCommand.GameCommand.TargetPosition))
                            {
                                // No need to build an assembler. Just build the unit
                                includePositions = new List<Position2>();
                                includePositions.Add(selectedGameCommand.GameCommand.TargetPosition);

                                passGameCommandToNewUnit = selectedGameCommand;
                            }
                            else
                            {
                                // If not neighbor, need to build an assembler to move there BUILD-STEP3
                                // This is just a fake to compute the possible moves. The command will not be excecuted
                                BlueprintCommand blueprintCommand = new BlueprintCommand();
                                blueprintCommand.GameCommandType = GameCommandType.Build;
                                blueprintCommand.Name = "BuildUnitForAssemble";

                                bool assembler;
                                bool engine;
                                bool container;
                                foreach (Blueprint blueprint in player.Game.Blueprints.Items)
                                {
                                    container = false;
                                    assembler = false;
                                    engine = false;

                                    foreach (BlueprintPart blueprintPart in blueprint.Parts)
                                    {
                                        if (blueprintPart.PartType == TileObjectType.PartAssembler)
                                        {
                                            assembler = true;
                                        }
                                        if (blueprintPart.PartType == TileObjectType.PartEngine)
                                        {
                                            engine = true;
                                        }
                                        if (blueprintPart.PartType == TileObjectType.PartContainer)
                                        {
                                            container = true;
                                        }
                                    }
                                    if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
                                    {
                                        if (assembler && engine)
                                        {
                                            BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
                                            blueprintCommandItem.BlueprintName = blueprint.Name;
                                            blueprintCommand.Units.Add(blueprintCommandItem);
                                            break;
                                        }
                                    }
                                    if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                                    {
                                        if (container && engine)
                                        {
                                            BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
                                            blueprintCommandItem.BlueprintName = blueprint.Name;
                                            blueprintCommand.Units.Add(blueprintCommandItem);
                                            break;
                                        }
                                    }
                                }

                                GameCommand newCommand = new GameCommand(blueprintCommand);

                                newCommand.GameCommandType = GameCommandType.Build;
                                newCommand.TargetPosition = selectedGameCommand.GameCommand.TargetPosition;

                                newCommand.PlayerId = player.PlayerModel.Id;
                                newCommand.DeleteWhenFinished = true;
                                GameCommandItem gameCommandItem = newCommand.GameCommandItems[0];

                                computePossibleMoves = false;

                                // Hack to create build assembler moves
                                Assembler.Unit.SetTempGameCommand(gameCommandItem);
                                Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Assemble, moveRecipeIngredient);

                                Assembler.Unit.SetTempGameCommand(selectedGameCommand);

                                selectedGameCommand.AssemblerToBuild = true;

                                passGameCommandToNewUnit = selectedGameCommand;
                            }
                        }
                    }
                }

                if (computePossibleMoves)
                    Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Assemble, moveRecipeIngredient);

                if (possiblemoves.Count == 0)
                {
                    if (selectedGameCommand.FactoryUnit.UnitId == Ant.Unit.UnitId)
                    {
                        if (selectedGameCommand.GameCommand.TargetPosition != Position2.Null)
                        {
                            Tile tile = player.Game.Map.GetTile(selectedGameCommand.GameCommand.TargetPosition);
                            // Must be possible to move the output there (blocking units..)
                            if (!tile.CanMoveTo(Ant.Unit.Pos))
                            {
                                // Cannot build here any more. 
                                if (player.PlayerModel.IsHuman)
                                {
                                    // Should be factory state?
                                    selectedGameCommand.FactoryUnit.SetStatus("CannotBuild", true);
                                }
                                else
                                {
                                    Ant.Unit.ResetGameCommand();
                                    selectedGameCommand.GameCommand.CommandCanceled = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // possiblemoves contains possible output places
                    List<Move> possibleMoves = new List<Move>();

                    foreach (Move possibleMove in possiblemoves)
                    {
                        if (control.IsOccupied(player, moves, possibleMove.Positions[1]))
                        {
                            continue;
                        }
                        possibleMoves.Add(possibleMove);
                    }
                    if (possibleMoves.Count > 0)
                    {
                        int idx = player.Game.Random.Next(possibleMoves.Count);
                        Move move = possibleMoves[idx];

                        move.GameCommandItem = passGameCommandToNewUnit;
                        
                        moves.Add(move);

                        return true;
                    }
                }
            }
            return false;
        }
    }
}
