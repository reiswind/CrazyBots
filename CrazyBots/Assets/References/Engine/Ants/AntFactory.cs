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

            bool everyOutTileOccupied = true;

            bool addContainer = false;
            bool addAssembler = false;
                bool containerFound = false;
            // Do we have a container?
            foreach (Tile n in player.Game.Map.GetTile(cntrlUnit.Pos).Neighbors)
            {
                if (n.Unit != null && n.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                {
                    if (n.Unit.Container != null && n.Unit.Engine == null)
                    {
                        if (n.Unit.Container.Metal >= n.Unit.Container.Capacity)
                        {
                            addAssembler = true;
                        }
                        containerFound = true;
                    }
                    else
                    {
                        //if (n.Unit != null && n.Unit.Assembler != null && !n.Unit.ExtractMe)
                            everyOutTileOccupied = false;
                    }
                }
                else
                {
                    everyOutTileOccupied = false;
                }
            }
            if (!containerFound)
            {
                //addAssembler = false;
                //if (cntrlUnit.Container != null && cntrlUnit.Container.Metal >= cntrlUnit.Container.Capacity)
                //    addContainer = true;
            }
            
            if (everyOutTileOccupied)
                cntrlUnit.ExtractMe = true;


            /*
            if (cntrlUnit.Container != null && cntrlUnit.Container.Metal >= cntrlUnit.Container.Capacity)
            {
                // Max reach.
                addContainer = true;
            }*/


            if (cntrlUnit.Assembler != null)
            {
                if (cntrlUnit.Assembler.CanProduce)
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
                                    if (ant.PlayerUnit.Unit.Extractor == null)
                                    { 
                                        if (possibleMove.UnitId == "Extractor")
                                        {
                                            moves.Add(possibleMove);
                                            unitMoved = true;
                                            break;
                                        }
                                    }
                                    else if (possibleMove.UnitId == "Container")
                                    {
                                        moves.Add(possibleMove);
                                        unitMoved = true;
                                        break;
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
                                }
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
                                        if (possibleMove.UnitId == "Container")
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
                                    else if (possibleMove.UnitId == "Engine")
                                    {
                                        if (Control.NumberOfWorkers < Control.MaxWorker)
                                            possibleMoves.Add(possibleMove);
                                    }
                                }
                                if (possibleMoves.Count > 0)
                                {
                                    int idx = player.Game.Random.Next(possibleMoves.Count);
                                    moves.Add(possibleMoves[idx]);
                                    unitMoved = true;
                                }
                            }
                        }
                    }
                }
            }
            return unitMoved;
        }
    }

}
