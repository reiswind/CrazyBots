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
            bool addWorker = false;
            bool addAssembler = false;
            bool addFighter = false;

            if (control.NumberOfWorkers < control.NumberOfReactors)
            {
                addWorker = true;
            }
            else if (control.NumberOfAssembler < 1)
            {
                addAssembler = true;
            }
            else if (control.NumberOfFighter < 10)
            {
                addFighter = true;
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
                return true;
            }

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
                    }
                }
                Blueprint commandBluePrint = null;
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
                                // Build this unit, this is an assembler
                                if (Ant.BuildPositionReached)
                                {
                                    possibleMoves.Add(possibleMove);
                                }
                                else if ("Assembler" == possibleMove.UnitId)
                                {
                                    // hmmmm
                                    //addFighter = false;
                                    //addWorker = false;

                                    // Build an assembler to move there

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
                            Ant ant = new Ant(control);
                            ant.AntWorkerType = AntWorkerType.Assembler;
                            control.CreatedAnts.Add(move.Positions[1], ant);

                            if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Build)
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

                            if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Collect)
                            {
                                ant.GameCommandDuringCreation = selectedGameCommand;
                                selectedGameCommand.AttachedUnits.Add("Assembler-" + this.Ant.PlayerUnit.Unit.UnitId);
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

                            if (selectedGameCommand != null && selectedGameCommand.GameCommandType == GameCommandType.Attack)
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
                        return true;
                        //unitMoved = true;
                    }
                }
            }
            return false;
        }
    }
}
