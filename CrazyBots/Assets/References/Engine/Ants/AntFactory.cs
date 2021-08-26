using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntFactory : Ant
    {

        public AntFactory(ControlAnt control, PlayerUnit playerUnit) : base(control, playerUnit)
        {
            
        }

        public override bool Move(Player player, List<Move> moves)
        {
            bool unitMoved = false;

            Unit cntrlUnit = PlayerUnit.Unit;
            bool addContainer = false;

            int totalMetalInPercent = 0;
            if (Control.MapPlayerInfo.TotalCapacity > 0)
                totalMetalInPercent = (Control.MapPlayerInfo.TotalMetal * 100) / Control.MapPlayerInfo.TotalCapacity;
            int workerInPercent = (Control.NumberOfWorkers * 100) / Control.MapPlayerInfo.TotalUnits;
            bool addWorker = false;
            bool addAssembler = false;
            bool addFighter = false;

            if (player.PlayerModel.Id != 1 &&
                cntrlUnit.Assembler != null &&
                cntrlUnit.Assembler.BuildQueue == null)
            {
                //if (workerInPercent < 10 || Control.NumberOfWorkers < (Control.NumberOfAssembler * 2))
                if (Control.NumberOfWorkers < 2 || workerInPercent < 10)
                    addWorker = true;

                //if (addWorker == false && Control.NumberOfAssembler < Control.MaxAssembler)
                //    addAssembler = true;

                int powerPerUnit = Control.MapPlayerInfo.TotalPower / Control.MapPlayerInfo.TotalUnits;

                if (addWorker == false && totalMetalInPercent > 10 && powerPerUnit > 80 && Control.NumberOfFighter <= 2)
                    addFighter = true;
            }
            if (cntrlUnit.Assembler != null)
            {
                if (cntrlUnit.Assembler.CanProduce())
                {
                    bool upgrading = false;

                    List<Move> possiblemoves = new List<Move>();
                    PlayerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);
                    while (possiblemoves.Count > 0)
                    {
                        int idx = player.Game.Random.Next(possiblemoves.Count);
                        Move move = possiblemoves[idx];

                        if (Control.IsUpgrading(player, moves, move))
                        {
                            possiblemoves.RemoveAt(idx);
                            continue;
                        }
                        upgrading = true;
                        moves.Add(move);
                        break;
                    }

                    if (!upgrading)
                    {
                        Blueprint commandBluePrint = null;
                        GameCommand selectedGameCommand = null;
                        if (player.GameCommands.Count > 0)
                        {
                            double bestDistance = 0;
                            GameCommand bestGameCommand = null;

                            foreach (GameCommand gameCommand in player.GameCommands)
                            {
                                double d = cntrlUnit.Pos.GetDistanceTo(gameCommand.TargetPosition);
                                if (bestGameCommand == null || d < bestDistance)
                                {
                                    bestDistance = d;
                                    selectedGameCommand = gameCommand;
                                }
                            }
                            if (selectedGameCommand != null)
                            {
                                if (selectedGameCommand.GameCommandType == GameCommandType.Build)
                                {
                                    commandBluePrint = player.Game.Blueprints.FindBlueprint(selectedGameCommand.UnitId);
                                }
                                else if (selectedGameCommand.GameCommandType == GameCommandType.Attack ||
                                         selectedGameCommand.GameCommandType == GameCommandType.Defend ||
                                         selectedGameCommand.GameCommandType == GameCommandType.Scout)
                                {
                                    addFighter = true;
                                }
                                else if (selectedGameCommand.GameCommandType == GameCommandType.Collect)
                                {
                                    addWorker = true;
                                }
                            }
                        }

                        PlayerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                        if (possiblemoves.Count > 0)
                        {
                            // possiblemoves contains possible output places
                            List<Move> possibleMoves = new List<Move>();

                            foreach (Move possibleMove in possiblemoves)
                            {
                                if (Control.IsOccupied(player, moves, possibleMove.Positions[1]))
                                {
                                    continue;
                                }

                                if (commandBluePrint != null)
                                {
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
                                        // Build this unit, it will move to the target
                                        if (commandBluePrint.Name == possibleMove.UnitId)
                                        {
                                            possibleMoves.Add(possibleMove);
                                        }
                                    }
                                    else
                                    {
                                        // hmmmm
                                        addFighter = false;
                                        addWorker = false;

                                        // Build an assembler to move there
                                        if ("Assembler" == possibleMove.UnitId)
                                        {
                                            possibleMoves.Add(possibleMove);
                                        }
                                    }
                                }
                                /*
                                if (PlayerUnit.Unit.Assembler.BuildQueue != null)
                                {
                                    foreach (string bp in PlayerUnit.Unit.Assembler.BuildQueue)
                                    {
                                        if (possibleMove.UnitId == bp)
                                        {
                                            PlayerUnit.Unit.Assembler.BuildQueue.Remove(bp);
                                            if (PlayerUnit.Unit.Assembler.BuildQueue.Count == 0)
                                                PlayerUnit.Unit.Assembler.BuildQueue = null;
                                            possibleMoves.Clear();
                                            possibleMoves.Add(possibleMove);
                                            breakMoves = true;
                                            break;
                                        }
                                    }
                                }
                                if (breakMoves)
                                    break;
                                */
                                if (addContainer)
                                {
                                    if (possibleMove.UnitId == "Extractor")
                                    {
                                        possibleMoves.Add(possibleMove);
                                    }
                                }
                                else if (addAssembler)
                                {
                                    if (possibleMove.UnitId == "Assembler")
                                    {
                                        possibleMoves.Add(possibleMove);
                                    }
                                }
                                else if (addWorker)
                                {
                                    if (possibleMove.UnitId.StartsWith("Worker"))
                                    {
                                        possibleMoves.Add(possibleMove);
                                    }
                                }
                                else if (addFighter)
                                {
                                    if (possibleMove.UnitId.StartsWith("Fighter") || possibleMove.UnitId.StartsWith("Bomber"))
                                    {
                                        possibleMoves.Add(possibleMove);
                                    }
                                }
                            }
                            if (possibleMoves.Count > 0)
                            {
                                int idx = player.Game.Random.Next(possibleMoves.Count);
                                Move move = possibleMoves[idx];
                                moves.Add(move);

                                if (move.UnitId == "Assembler")
                                {
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Assembler;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);

                                    if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Build)
                                    {
                                        antWorker.GameCommandDuringCreation = selectedGameCommand;
                                        player.GameCommands.Remove(selectedGameCommand);
                                    }
                                }
                                /*
                                if (addContainer)
                                {
                                    AntContainer antContainer = new AntContainer(Control);
                                    Control.CreatedAnts.Add(move.Positions[1], antContainer);

                                    if (selectedGameCommand != null)
                                    {
                                        antContainer.GameCommandDuringCreation = selectedGameCommand;
                                        player.GameCommands.Remove(selectedGameCommand);
                                    }
                                }
                                else
                                if (addAssembler)
                                {
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Assembler;
                                    Control.NumberOfAssembler++;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);

                                    if (selectedGameCommand != null)
                                    {
                                        antWorker.GameCommandDuringCreation = selectedGameCommand;
                                        player.GameCommands.Remove(selectedGameCommand);
                                    }
                                }*/
                                else if (move.UnitId == "Worker")
                                {
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Worker;
                                    Control.NumberOfWorkers++;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);

                                    if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Collect)
                                    {
                                        antWorker.GameCommandDuringCreation = selectedGameCommand;
                                        player.GameCommands.Remove(selectedGameCommand);
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
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Fighter;
                                    Control.NumberOfFighter++;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);

                                    if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Attack)
                                    {
                                        antWorker.GameCommandDuringCreation = selectedGameCommand;
                                        player.GameCommands.Remove(selectedGameCommand);
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

                                unitMoved = true;
                            }
                        }
                    }
                }
            }


            return unitMoved;
        }
    }

}
