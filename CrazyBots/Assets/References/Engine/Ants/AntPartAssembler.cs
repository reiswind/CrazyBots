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
            if (Assembler.CanProduce())
            {
                Assemble(control, player, moves);
            }
            return false;
        }

        public void Assemble(ControlAnt control, Player player, List<Move> moves)
        {
            bool addWorker = false;
            bool addAssembler = false;
            bool addFighter = false;

            if (control.NumberOfFighter < 1)
            {
                addFighter = true;
            }
            else if (control.NumberOfWorkers < control.NumberOfReactors)
            {
                addWorker = true;
            }
            else if (control.NumberOfAssembler < 1)
            {
                addAssembler = true;
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
                                if (control.NumberOfFighter < 8)
                                    addFighter = true;
                            }
                        }
                    }
                }
            }


            bool upgrading = false;

            List<Move> possiblemoves = new List<Move>();
            Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);
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
                        double d = Assembler.Unit.Pos.GetDistanceTo(gameCommand.TargetPosition);
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
                            //addFighter = true;
                        }
                        else if (selectedGameCommand.GameCommandType == GameCommandType.Collect)
                        {
                            //addWorker = true;
                        }
                    }
                }

                Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
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
                                //addFighter = false;
                                //addWorker = false;

                                // Build an assembler to move there
                                if ("Assembler" == possibleMove.UnitId)
                                {
                                    possibleMoves.Add(possibleMove);
                                }
                            }
                        }

                        if (addAssembler)
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
                            AntWorker antWorker = new AntWorker(control);
                            antWorker.AntWorkerType = AntWorkerType.Assembler;
                            control.CreatedAnts.Add(move.Positions[1], antWorker);

                            if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Build)
                            {
                                antWorker.GameCommandDuringCreation = selectedGameCommand;
                                player.GameCommands.Remove(selectedGameCommand);
                            }
                        }
                        else if (move.UnitId == "Worker")
                        {
                            AntWorker antWorker = new AntWorker(control);
                            antWorker.AntWorkerType = AntWorkerType.Worker;
                            control.NumberOfWorkers++;
                            control.CreatedAnts.Add(move.Positions[1], antWorker);

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
                            AntWorker antWorker = new AntWorker(control);
                            antWorker.AntWorkerType = AntWorkerType.Fighter;
                            control.NumberOfFighter++;
                            control.CreatedAnts.Add(move.Positions[1], antWorker);

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

                        //unitMoved = true;
                    }
                }
            }
        }
    }
}
