
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
        //private int demandWorker = 2;

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
            bool moved = false;
            if (Assembler.CanProduce())
            {
                moved = Assemble(control, player, moves);
            }
            return moved;
        }
        /*
        public bool BuildUnit(ControlAnt control, Player player, List<Move> moves)
        {
            Unit cntrlUnit = Assembler.Unit;

            // Assembler reached target, build order
            List<Move> possiblemoves = new List<Move>();
            cntrlUnit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
            if (possiblemoves.Count > 0)
            {
                foreach (Move move1 in possiblemoves)
                {
                    if (cntrlUnit.CurrentGameCommand != null &&
                        cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Build &&
                        move1.Positions[1] == cntrlUnit.CurrentGameCommand.TargetPosition &&
                        move1.UnitId == cntrlUnit.CurrentGameCommand.UnitId)
                    {
                        moves.Add(move1);

                        Ant.BuildPositionReached = false;
                        Ant.FollowThisRoute = null;
                        return true;
                    }
                }
            }
            // Reached position, tryin to build but cant.
            Ant.StuckCounter++;
            if (Ant.StuckCounter > 10)
            {
                Ant.AbandonUnit(player);
            }
            return false;
        }*/

        public bool Assemble(ControlAnt control, Player player, List<Move> moves)
        {
            bool upgrading = false;

            List<Position2> includePositions = null;
            if (Assembler.Unit.CurrentGameCommand != null && Ant.AntPartEngine != null)
            {
                // If engine, move to target and upgrade then. Do not upgrade anything else.
                includePositions = new List<Position2>();
                includePositions.Add(Assembler.Unit.CurrentGameCommand.GameCommand.TargetPosition);
            }
            List<Move> possiblemoves = new List<Move>();
            Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Upgrade);
            while (possiblemoves.Count > 0)
            {
                int idx = player.Game.Random.Next(possiblemoves.Count);
                Move move = possiblemoves[idx];

                if (control.IsUpgrading(player, moves, move))
                {
                    possiblemoves.RemoveAt(idx);
                    continue;
                }
                upgrading = true;
                moves.Add(move);
                return true;
            }
            includePositions = null;

            if (!upgrading)
            {

                GameCommandItem selectedGameCommand = Ant.PlayerUnit.Unit.CurrentGameCommand;
                GameCommandItem passGameCommandToNewUnit = null;
                /*
                if (Assembler.Unit.CurrentGameCommand != null &&
                    Assembler.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (Assembler.Unit.CurrentGameCommand.CommandComplete)
                    {
                        //int whereisit = 0;
                    }
                    else if (Assembler.Unit.CurrentGameCommand.CommandCanceled)
                    {
                        //int whereisit = 0;
                    }
                    else if (Assembler.Unit.CurrentGameCommand.WaitingForUnit)
                    {
                        //int whereisit = 0;
                    }
                    else
                    {
                        selectedGameCommand = Assembler.Unit.CurrentGameCommand;
                    }
                }*/

                bool computePossibleMoves = true;
                bool assemblerUsedToBuild = false;

                if (selectedGameCommand == null)
                {
                    return false;
                }
                else
                {
                    //if (selectedGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
                    {
                        // Assembler should move to the target
                        if (Ant.AntPartEngine != null)
                        {
                            if (!Ant.BuildPositionReached)
                            {
                                return false;
                            }
                            else
                            {
                                includePositions = new List<Position2>();
                                includePositions.Add(selectedGameCommand.GameCommand.TargetPosition);

                                //finishCommandWhenCompleted = selectedGameCommand;
                                passGameCommandToNewUnit = selectedGameCommand;
                            }
                        }
                        else
                        {
                            // Structure: Build unit or an assembler that moves there 
                            Blueprint commandBluePrint;
                            commandBluePrint = player.Game.Blueprints.FindBlueprint(selectedGameCommand.BlueprintCommandItem.BlueprintName);

                            bool engineFound = false;
                            foreach (BlueprintPart blueprintPart in commandBluePrint.Parts)
                            {
                                if (blueprintPart.PartType == TileObjectType.PartEngine)
                                {
                                    engineFound = true;
                                    break;
                                }
                            }
                            if (engineFound)
                            {
                                Tile tile = player.Game.Map.GetTile(Ant.PlayerUnit.Unit.Pos);
                                if (tile.IsNeighbor(selectedGameCommand.GameCommand.TargetPosition))
                                {
                                    // No need to build an assembler. Just build the unit at this position
                                    includePositions = new List<Position2>();
                                    includePositions.Add(selectedGameCommand.GameCommand.TargetPosition);
                                }

                                // Build this unit, it will move to the target COMMAND-STEP3
                                passGameCommandToNewUnit = selectedGameCommand; //.AttachToThisOnCompletion;
                                //finishCommandWhenCompleted = selectedGameCommand;
                            }
                            else
                            {
                                // Check if TargetPosition is neighbor!
                                Tile tile = player.Game.Map.GetTile(Ant.PlayerUnit.Unit.Pos);
                                if (tile.IsNeighbor(selectedGameCommand.GameCommand.TargetPosition))
                                {
                                    // No need to build an assembler. Just build the unit
                                    includePositions = new List<Position2>();
                                    includePositions.Add(selectedGameCommand.GameCommand.TargetPosition);
                                    //finishCommandWhenCompleted = selectedGameCommand;

                                    passGameCommandToNewUnit = selectedGameCommand;
                                }
                                else
                                {

                                    // If not neighbor, need to build an assembler to move there BUILD-STEP3
                                    BlueprintCommand blueprintCommand = new BlueprintCommand();
                                    blueprintCommand.GameCommandType = GameCommandType.Build;
                                    blueprintCommand.Name = "BuildUnitForAssemble";

                                    bool assembler;
                                    bool engine;
                                    foreach (Blueprint blueprint in player.Game.Blueprints.Items)
                                    {
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
                                        }
                                        if (assembler && engine)
                                        {
                                            BlueprintCommandItem blueprintCommandItem = new BlueprintCommandItem();
                                            blueprintCommandItem.BlueprintName = blueprint.Name;
                                            blueprintCommand.Units.Add(blueprintCommandItem);
                                            break;
                                        }
                                    }
                                    
                                    GameCommand newCommand = new GameCommand(blueprintCommand);

                                    newCommand.GameCommandType = GameCommandType.Build;
                                    newCommand.TargetPosition = selectedGameCommand.GameCommand.TargetPosition;
                                    
                                    newCommand.PlayerId = player.PlayerModel.Id;
                                    newCommand.DeleteWhenFinished = true;
                                    GameCommandItem gameCommandItem = newCommand.GameCommandItems[0];
                                    //gameCommandItem.FactoryUnitId = Ant.PlayerUnit.Unit.UnitId;
                                    //gameCommandItem.AttachToThisOnCompletion = selectedGameCommand.GameCommand;

                                    computePossibleMoves = false;
                                    
                                    // Hack to create build assembler moves
                                    
                                    Assembler.Unit.SetTempGameCommand(gameCommandItem);
                                    Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Assemble);

                                    Assembler.Unit.SetTempGameCommand(selectedGameCommand);
                                    passGameCommandToNewUnit = selectedGameCommand;
                                    assemblerUsedToBuild = true;
                                }
                            }
                        }
                    }
                }

                if (computePossibleMoves)
                    Assembler.ComputePossibleMoves(possiblemoves, includePositions, MoveFilter.Assemble);

                if (possiblemoves.Count > 0)
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

                        Unit createdUnit = new Unit(player.Game, move.Stats.BlueprintName);
                        player.Game.Map.Units.Add(createdUnit);

                        // Too early, unit just started to build
                        //Ant.PlayerUnit.Unit.ClearGameCommand();

                        // Pass the command
                        if (passGameCommandToNewUnit != null)
                        {
                            // If its an assembler, this is not the attached unit for the command
                            if (!assemblerUsedToBuild)
                                passGameCommandToNewUnit.AttachedUnitId = createdUnit.UnitId;
                            //passGameCommandToNewUnit.FactoryUnitId = createdUnit.UnitId // Remains until unit is complete!;
                            createdUnit.SetGameCommand(passGameCommandToNewUnit);
                        }
                        else
                        {
                            selectedGameCommand.AttachedUnitId = createdUnit.UnitId;
                        }
                        move.UnitId = createdUnit.UnitId;
                        moves.Add(move);

                        //Ant ant = new Ant(control, new PlayerUnit(createdUnit));
                        /*
                        if (move.UnitId == "Assembler")
                        {
                            ant.AntWorkerType = AntWorkerType.Assembler;
                            control.NumberOfAssembler++;
                        }
                        else if (move.UnitId == "Worker")
                        {
                            ant.AntWorkerType = AntWorkerType.Worker;
                            control.NumberOfWorkers++;
                        }
                        else if (move.UnitId.StartsWith("Fighter") || move.UnitId.StartsWith("Bomber"))
                        {
                            ant.AntWorkerType = AntWorkerType.Fighter;
                            control.NumberOfFighter++;
                        }
                        */
                        
                        //createdUnit.FinishCommandWhenCompleted = finishCommandWhenCompleted;
                        //if (passGameCommandToNewUnit != null)
                        {
                            // Pass the command to the created unit. The command is attached to the factory until the unit is created COMMAND-STEP4 
                            //createdUnit.SetGameCommand(passGameCommandToNewUnit);
                            /*
                            ant.GameCommandDuringCreation = passGameCommandToNewUnit;
                            passGameCommandToNewUnit.AttachedUnits.Clear();
                            passGameCommandToNewUnit.AttachedUnits.Add("Assembler-" + this.Ant.PlayerUnit.Unit.UnitId);
                            */
                            if (selectedGameCommand != null)
                            {
                                //selectedGameCommand.WaitingForUnit = true;

                                // It has been build. Upgrading will happen, because unit is nearby
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
