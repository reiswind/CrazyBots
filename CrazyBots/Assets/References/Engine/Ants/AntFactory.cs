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
            AntContainer containerUnit = null;
            //bool everyOutTileOccupied = true;

            bool addContainer = false;
            bool addAssembler = false;
            bool containerFound = false;

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
                            containerFound = true;
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
            if (containerFound)
            {
                // Must be complete empty
                if (containerUnit != null && 
                    containerUnit.PlayerUnit.Unit.IsComplete() &&
                    cntrlUnit.Container != null &&
                    cntrlUnit.Container.Metal == 0 &&
                    cntrlUnit.Metal == 0)
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
                    addContainer = true;
            }

            bool addWorker = false;
            if (Control.NumberOfWorkers < Control.MaxWorker)
                addWorker = true;

            if (cntrlUnit.Assembler != null)
            {
                cntrlUnit.Assembler.AttachedContainer = containerUnit?.PlayerUnit.Unit.Container;

                if (cntrlUnit.Assembler.CanProduce())
                {
                    List<Move> possiblemoves = new List<Move>();
                    PlayerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);
                    if (possiblemoves.Count > 0)
                    {
                        foreach (Move possibleMove in possiblemoves)
                        {
                            PlayerUnit constructedUnit = player.Units[possibleMove.Positions[1]];
                            if (Control.Ants.ContainsKey(constructedUnit.Unit.UnitId))
                            {
                                Ant ant = Control.Ants[constructedUnit.Unit.UnitId];
                                if (ant is AntContainer)
                                {
                                    
                                    if (ant.PlayerUnit.Unit.Container == null ||
                                        ant.PlayerUnit.Unit.Container.Level < 3)
                                    { 
                                        if (possibleMove.UnitId == "Container")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                }
                                else if (ant is AntFactory)
                                {
                                    // Build a factory add Extractor, Container, 
                                    if (ant.PlayerUnit.Unit.Extractor == null)
                                    {
                                        if (possibleMove.UnitId == "Extractor")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                    if (ant.PlayerUnit.Unit.Container == null)
                                    {
                                        if (possibleMove.UnitId == "Container")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                    if (ant.PlayerUnit.Unit.Radar == null)
                                    {
                                        if (possibleMove.UnitId == "Radar")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                }
                                else if (ant is AntWorker)
                                {
                                    if (constructedUnit.Unit.Container == null)
                                    {
                                        if (possibleMove.UnitId == "Container")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                    else if (constructedUnit.Unit.Weapon == null)
                                    {
                                        //if (possibleMove.UnitId == "Armor")
                                        if (possibleMove.UnitId == "Weapon")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                    else if (constructedUnit.Unit.Extractor == null)
                                    {
                                        if (possibleMove.UnitId == "Extractor")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Build a Worker
                                /*
                                if (constructedUnit.Unit.Engine != null)
                                {
                                    if (possibleMove.UnitId == "Extractor")
                                    {
                                        AntWorker antWorker = new AntWorker(Control, constructedUnit);
                                        Control.Ants.Add(constructedUnit.Unit.UnitId, antWorker);
                                        moves.Add(possibleMove);
                                        unitMoved = true;
                                        break;
                                    }
                                }
                                else if (constructedUnit.Unit.Container != null)
                                {
                                    if (possibleMove.UnitId == "Container")
                                    {
                                        AntContainer antAntContainer = new AntContainer(Control, constructedUnit);
                                        Control.Ants.Add(constructedUnit.Unit.UnitId, antAntContainer);
                                        moves.Add(possibleMove);
                                        unitMoved = true;
                                        break;
                                    }
                                }
                                else if (constructedUnit.Unit.Assembler != null)
                                {
                                    if (possibleMove.UnitId == "Assembler")
                                    {
                                        AntFactory antFactory = new AntFactory(Control, constructedUnit);
                                        Control.Ants.Add(constructedUnit.Unit.UnitId, antFactory);
                                        moves.Add(possibleMove);
                                        unitMoved = true;
                                        break;
                                    }
                                }*/
                            }

                        }
                    }
                    else
                    {
                        //if (Control.NumberOfWorkers < Control.MaxWorker)
                        {
                            PlayerUnit.Unit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Assemble);
                            if (possiblemoves.Count > 0)
                            {
                                // possiblemoves contains possible output places
                                List<Move> possibleMoves = new List<Move>();

                                foreach (Move possibleMove in possiblemoves)
                                {
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
                                        if (possibleMove.UnitId == "Engine")
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
                                    else if (addWorker)
                                    {
                                        AntWorker antWorker = new AntWorker(Control);
                                        antWorker.IsWorker = true;
                                        Control.NumberOfWorkers++;
                                        Control.CreatedAnts.Add(move.Positions[1], antWorker);
                                    }

                                    unitMoved = true;
                                }
                            }
                        }
                    }
                }
            }

            if (!unitMoved)
            {
                if (cntrlUnit.Extractor != null && !containerFound)
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
