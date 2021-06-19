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
        public List<string> UserDefinedNextBlueprint { get; set; }

        public AntFactory(ControlAnt control, PlayerUnit playerUnit) : base(control, playerUnit)
        {
            UserDefinedNextBlueprint = new List<string>();
        }

        public override bool Move(Player player, List<Move> moves)
        {
            bool unitMoved = false;

            Unit cntrlUnit = PlayerUnit.Unit;
            AntContainer containerUnit = null;
            //bool everyOutTileOccupied = true;

            bool addContainer = false;
            //bool addAssemblerOnGround = false;
            //bool containerFound = false;

            // Do we have a container?
            foreach (Tile n in player.Game.Map.GetTile(cntrlUnit.Pos).Neighbors)
            {
                if (n.Unit != null && n.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                {
                    if (Control.Ants.ContainsKey(n.Unit.UnitId))
                    {
                        if (Control.Ants[n.Unit.UnitId] is AntContainer)
                        {
                            containerUnit = Control.Ants[n.Unit.UnitId] as AntContainer;
                            //containerFound = true;
                        }
                    }
                    else
                    {
                        //everyOutTileOccupied = false;
                    }
                }
                else
                {
                    //everyOutTileOccupied = false;
                }
            }
            /*
            if (containerFound)
            {
                // Must be complete empty
                if (containerUnit != null && 
                    containerUnit.PlayerUnit.Unit.IsComplete() &&
                    cntrlUnit.Container != null &&
                    cntrlUnit.Container.Metal == 0 
                    /*&& cntrlUnit.Metal == 0* /)
                {
                    // Remove the local container, since the factory is attached to a container.
                    Move move = new Move();
                    move.MoveType = MoveType.Upgrade;
                    move.Positions = new List<Position>();
                    move.Positions.Add(cntrlUnit.Pos);
                    move.UnitId = cntrlUnit.UnitId;
                    move.OtherUnitId = "RemoveContainerAndUpgradeAssembler";
                    moves.Add(move);

                    return true;
                }
            }
            else
            {
                //addAssembler = false;
                if (cntrlUnit.Container != null && cntrlUnit.Container.Metal >= cntrlUnit.Container.Capacity)
                {
                    //if (player.PlayerModel.Id > 1)
                    //    addContainer = true;
                }
            }
            */

            int totalMetalInPercent = (Control.MapPlayerInfo.TotalMetal * 100) / Control.MapPlayerInfo.TotalCapacity;
            int workerInPercent = (Control.NumberOfWorkers * 100) / Control.MapPlayerInfo.TotalUnits;


            bool addWorker = false;
            bool addAssembler = false;
            bool addFighter = false;

            //if (player.PlayerModel.Id != 1)
            {
                if (workerInPercent < 10)
                    addWorker = true;

                //if (addWorker == false && Control.NumberOfAssembler < Control.MaxAssembler)
                //    addAssembler = true;

                if (addWorker == false && totalMetalInPercent > 10)
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
                        PlayerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                        if (possiblemoves.Count > 0)
                        {
                            // possiblemoves contains possible output places
                            List<Move> possibleMoves = new List<Move>();
                            bool breakMoves = false;

                            foreach (Move possibleMove in possiblemoves)
                            {
                                if (Control.IsOccupied(player, moves, possibleMove.Positions[1]))
                                {
                                    continue;
                                }

                                if (UserDefinedNextBlueprint.Count > 0)
                                {
                                    foreach (string bp in UserDefinedNextBlueprint)
                                    {
                                        if (possibleMove.UnitId == bp)
                                        {
                                            UserDefinedNextBlueprint.Remove(bp);
                                            possibleMoves.Clear();
                                            possibleMoves.Add(possibleMove);
                                            breakMoves = true;
                                            break;
                                        }
                                    }
                                }
                                if (breakMoves)
                                    break;

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
                                    if (possibleMove.UnitId.StartsWith("Fighter"))
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

                                if (addContainer)
                                {
                                    AntContainer antContainer = new AntContainer(Control);
                                    Control.CreatedAnts.Add(move.Positions[1], antContainer);
                                }
                                else if (addAssembler)
                                {
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Assembler;
                                    Control.NumberOfAssembler++;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);
                                }
                                else if (addWorker)
                                {
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Worker;
                                    Control.NumberOfWorkers++;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);

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
                                    }
                                }
                                else if (addFighter)
                                {
                                    AntWorker antWorker = new AntWorker(Control);
                                    antWorker.AntWorkerType = AntWorkerType.Fighter;
                                    Control.NumberOfFighter++;
                                    Control.CreatedAnts.Add(move.Positions[1], antWorker);

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
                                    }
                                }

                                unitMoved = true;
                            }
                        }
                    }
                }
            }

            if (!unitMoved)
            {
                if (cntrlUnit.Extractor != null)
                {
                    List<Move> possiblemoves = new List<Move>();
                    cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, null, MoveFilter.Extract);
                    if (possiblemoves.Count > 0)
                    {
                        foreach (Move possibleMove in possiblemoves)
                        {
                            Tile n = player.Game.Map.GetTile(possibleMove.Positions[1]);
                            if (n.Unit != null && n.Unit.Assembler != null && !n.Unit.ExtractMe)
                            {
                                // Do not extract from attached factory
                                //continue;
                            }
                            moves.Add(possibleMove);
                            return true;
                        }
                    }
                }
            }

            return unitMoved;
        }
    }

}
