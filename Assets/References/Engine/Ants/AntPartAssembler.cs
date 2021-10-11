using Engine.Control;
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
                if (Ant.BuildPositionReached)
                {
                    //moved = BuildUnit(control, player, moves);
                }
                
                {
                    moved = Assemble(control, player, moves);
                }
            }
            return moved;
        }

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
        }

        public bool Assemble(ControlAnt control, Player player, List<Move> moves)
        {
            /*
            bool addWorker = false;
            bool addAssembler = false;
            bool addFighter = false;

            if (control.NumberOfWorkers < control.NumberOfReactors)
            {
                //addWorker = true;
            }
            else if (control.NumberOfAssembler < 1)
            {
                //addAssembler = true;
            }
            else if (control.NumberOfFighter < 10)
            {
                //addFighter = true;
            }
            else
            {
                foreach (AntNetworkConnect antNetworkConnect in AntNetworkNode.Connections)
                {
                    if (antNetworkConnect.AntNetworkDemands != null)
                    {
                        foreach (AntNetworkDemand antNetworkDemand in antNetworkConnect.AntNetworkDemands)
                        {
                            if (antNetworkDemand.Demand == AntNetworkDemandType.Storage &&
                                antNetworkDemand.Urgency == 1)
                            {
                                // Create more storage by building new container OR build something
                                //if (control.NumberOfFighter < 8)
                                //    addFighter = true;
                            }
                        }
                    }
                }
            }
            */

            bool upgrading = false;

            List<Position> includedPositions = null;
            if (Assembler.Unit.CurrentGameCommand != null)
            {
                // Just update... does not work
                //includedPositions = new List<Position>();
                //includedPositions.Add(Assembler.Unit.CurrentGameCommand.TargetPosition);
            }
            List<Move> possiblemoves = new List<Move>();
            Assembler.ComputePossibleMoves(possiblemoves, includedPositions, MoveFilter.Upgrade);
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
            includedPositions = null;

            if (!upgrading)
            {
                GameCommand selectedGameCommand = null;

                if (Assembler.Unit.CurrentGameCommand != null &&
                    Assembler.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                {
                    selectedGameCommand = Assembler.Unit.CurrentGameCommand;
                }
                else
                {
                    // Assigning orders should be in control
                    /*
                    if (player.GameCommands.Count > 0)
                    {
                        double bestDistance = 0;
                        GameCommand bestGameCommand = null;

                        foreach (GameCommand gameCommand in player.GameCommands)
                        {
                            if (gameCommand.AttachedUnits.Count > 0)
                                continue;

                            double d = Assembler.Unit.Pos.GetDistanceTo(gameCommand.TargetPosition);
                            if (bestGameCommand == null || d < bestDistance)
                            {
                                bestDistance = d;
                                selectedGameCommand = gameCommand;
                            }
                        }
                    }*/
                }

                

                bool computePossibleMoves = true;
                GameCommand passGameCommandToNewUnit = null;
                GameCommand finishCommandWhenCompleted = null;
                if (selectedGameCommand == null)
                {
                    return false;
                }
                else
                {
                    if (selectedGameCommand.GameCommandType == GameCommandType.Build)
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
                                includedPositions = new List<Position>();
                                includedPositions.Add(selectedGameCommand.TargetPosition);

                                finishCommandWhenCompleted = selectedGameCommand;
                            }
                        }
                        else
                        {
                            // Structure: Build unit or an assembler that moves there 
                            Blueprint commandBluePrint;
                            if (selectedGameCommand.UnitId == null)
                            {
                                commandBluePrint = player.Game.Blueprints.FindBlueprint(selectedGameCommand.BlueprintCommand.Units[0].BlueprintName);
                            }
                            else
                            {
                                commandBluePrint = player.Game.Blueprints.FindBlueprint(selectedGameCommand.UnitId);
                            }

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
                                if (tile.IsNeighbor(selectedGameCommand.TargetPosition))
                                {
                                    // No need to build an assembler. Just build the unit at this position
                                    includedPositions = new List<Position>();
                                    includedPositions.Add(selectedGameCommand.TargetPosition);
                                }
                               
                                // Build this unit, it will move to the target COMMAND-STEP3
                                passGameCommandToNewUnit = selectedGameCommand.AttachToThisOnCompletion;
                                finishCommandWhenCompleted = selectedGameCommand;
                            }
                            else
                            {
                                // Check if TargetPosition is neighbor!
                                Tile tile = player.Game.Map.GetTile(Ant.PlayerUnit.Unit.Pos);
                                if (tile.IsNeighbor(selectedGameCommand.TargetPosition))
                                {
                                    // No need to build an assembler. Just build the unit
                                    includedPositions = new List<Position>();
                                    includedPositions.Add(selectedGameCommand.TargetPosition);
                                    finishCommandWhenCompleted = selectedGameCommand;
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
                                            blueprintCommandItem.Count = 1;
                                            blueprintCommand.Units.Add(blueprintCommandItem);
                                            break;
                                        }
                                    }

                                    GameCommand newCommand = new GameCommand();

                                    newCommand.GameCommandType = GameCommandType.Build;
                                    newCommand.TargetPosition = selectedGameCommand.TargetPosition;
                                    newCommand.BlueprintCommand = blueprintCommand;
                                    newCommand.PlayerId = player.PlayerModel.Id;
                                    newCommand.AttachToThisOnCompletion = selectedGameCommand;

                                    computePossibleMoves = false;

                                    // Hack to create build assembler moves
                                    Assembler.Unit.SetGameCommand(newCommand);
                                    Assembler.ComputePossibleMoves(possiblemoves, includedPositions, MoveFilter.Assemble);

                                    Assembler.Unit.SetGameCommand(selectedGameCommand);
                                    passGameCommandToNewUnit = newCommand;
                                }
                            }
                        }
                    }
                }

                if (computePossibleMoves)
                    Assembler.ComputePossibleMoves(possiblemoves, includedPositions, MoveFilter.Assemble);

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
                        moves.Add(move);

                        Ant ant = new Ant(control);
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
                        control.CreatedAnts.Add(move.Positions[1], ant);

                        ant.FinishCommandWhenCompleted = finishCommandWhenCompleted;
                        if (passGameCommandToNewUnit != null)
                        {
                            // Pass the command to the created unit. The command is attached to the factory until the unit is created COMMAND-STEP4 
                            ant.GameCommandDuringCreation = passGameCommandToNewUnit;
                            passGameCommandToNewUnit.AttachedUnits.Clear();
                            passGameCommandToNewUnit.AttachedUnits.Add("Assembler-" + this.Ant.PlayerUnit.Unit.UnitId);
                        }
                        Assembler.Unit.ResetGameCommand();

#if OLDDD
                        if (move.UnitId == "Assembler")
                        {
                            Ant ant = new Ant(control);
                            ant.AntWorkerType = AntWorkerType.Assembler;
                            control.CreatedAnts.Add(move.Positions[1], ant);

                            if (selectedGameCommand != null) // && selectedGameCommand.GameCommandType == GameCommandType.Build)
                            {
                                ant.GameCommandDuringCreation = selectedGameCommand;
                                selectedGameCommand.AttachedUnits.Add("Assembler-" + this.Ant.PlayerUnit.Unit.UnitId);
                                //player.GameCommands.Remove(selectedGameCommand);
                            }
                        }
                        else if (move.UnitId == "Worker")
                        {
                            Ant ant = new Ant(control);
                            ant.AntWorkerType = AntWorkerType.Worker;
                            control.NumberOfWorkers++;
                            control.CreatedAnts.Add(move.Positions[1], ant);

                            // This is the BUILD Command.
                            if (selectedGameCommand != null) // && selectedGameCommand.GameCommandType == GameCommandType.Collect)
                            {
                                ant.GameCommandDuringCreation = selectedGameCommand;
                                selectedGameCommand.AttachedUnits.Add("Assembler-" + this.Ant.PlayerUnit.Unit.UnitId);
                                Assembler.Unit.CurrentGameCommand = null;
                                //player.GameCommands.Remove(selectedGameCommand);
                            }
                            /*
                            if (cntrlUnit.GameCommands != null && cntrlUnit.GameCommands.Count > 0)
                            {
                                GameCommand gameCommand = cntrlUnit.GameCommands[0];
                                if (gameCommand.GameCommandType == GameCommandType.Minerals)
                                {
                                    GameCommand attackMove = new GameCommand();
                                    attackMove.GameCommandType = GameCommandType.Move;
                                    attackMove.TargetPosition = gameCommand.TargetPosition;

                                    antWorker.CurrentGameCommand = attackMove;
                                }
                            }*/
                        }
                        else if (move.UnitId == "Fighter" || move.UnitId.StartsWith("Bomber"))
                        {
                            Ant ant = new Ant(control);
                            ant.AntWorkerType = AntWorkerType.Fighter;
                            control.NumberOfFighter++;
                            control.CreatedAnts.Add(move.Positions[1], ant);

                            if (selectedGameCommand != null) // && selectedGameCommand.GameCommandType == GameCommandType.Attack)
                            {
                                ant.GameCommandDuringCreation = selectedGameCommand;
                                selectedGameCommand.AttachedUnits.Add("Assembler-" + this.Ant.PlayerUnit.Unit.UnitId);
                                //player.GameCommands.Remove(selectedGameCommand);
                            }
                            /*
                            if (cntrlUnit.GameCommands != null && cntrlUnit.GameCommands.Count > 0)
                            {
                                GameCommand gameCommand = cntrlUnit.GameCommands[0];
                                if (gameCommand.GameCommandType == GameCommandType.Attack)
                                {
                                    GameCommand attackMove = new GameCommand();
                                    attackMove.GameCommandType = GameCommandType.AttackMove;
                                    attackMove.TargetPosition = gameCommand.TargetPosition;

                                    antWorker.CurrentGameCommand = attackMove;
                                }
                            }*/
                        }
                        else
                        {
                            /*
                            AntWorker antWorker = new AntWorker(Control);
                            antWorker.AntWorkerType = AntWorkerType.None;
                            Control.CreatedAnts.Add(move.Positions[1], antWorker);

                            if (selectedGameCommand != null)
                            {
                                antWorker.GameCommandDuringCreation = selectedGameCommand;
                                player.GameCommands.Remove(selectedGameCommand);
                            }*/
                        }
#endif
                        return true;
                        //unitMoved = true;
                    }
                }
            }
            return false;
        }
    }
}
