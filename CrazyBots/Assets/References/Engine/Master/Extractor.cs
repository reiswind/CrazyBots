using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Extractor : Ability
    {
        public int Level { get; set; }

        public Extractor(Unit owner, int level) : base(owner)
        {
            Level = level;
        }
        private int MetalCollectionRange
        {
            get
            {
                
                if (Level == 3)
                    return 3;
                if (Level == 2)
                    return 2;
                if (Level == 1)
                    return 1;
                
                return 0;
            }
        }
        public Dictionary<Position, TileWithDistance> CollectExtractionTiles()
        {
            return Unit.Game.Map.EnumerateTiles(Unit.Pos, MetalCollectionRange, false);

        }

        public Dictionary<Position, TileWithDistance> CollectTilesWithMetal()
        {

            return Unit.Game.Map.EnumerateTiles(Unit.Pos, MetalCollectionRange, false, matcher: tile => 
            {
                if (tile.Unit != null)
                {
                    if (tile.Unit.Owner.PlayerModel.Id != Unit.Owner.PlayerModel.Id)
                    {
                        // Extract from enemy- Yeah!
                        return true;
                    }
                    if (tile.Unit.Container != null && tile.Unit.Container.Mineral > 0)
                    {
                        // Extract from a friendly container
                        return true;
                    }
                    if (tile.Unit.ExtractMe)
                    {
                        // Extract from a friendly unit, marked for extraction
                        return true;
                    }
                }

                if (tile.Metal > 0)
                    return true;
                return false;

            });

        }

        public int CountAvailableMetal()
        {
            int metal = 0;
            Dictionary<Position, TileWithDistance> resultList = CollectTilesWithMetal();
            foreach (TileWithDistance n in resultList.Values)
            {
                metal += n.Metal;
            }
            return metal;
        }


        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Extract) == 0)
                return;

            if (!CanExtract)
                // Unit full Not possible to extract
                return;

            if (Unit.ExtractMe)
                return;

            Dictionary<Position, TileWithDistance> resultList = CollectTilesWithMetal();

            foreach (TileWithDistance t in resultList.Values)
            {
                if (t.Metal > 0)
                {
                    Move move = new Move();

                    move.MoveType = MoveType.Extract;

                    move.UnitId = Unit.UnitId;
                    move.OtherUnitId = "Ground";
                    move.Positions = new List<Position>();
                    move.Positions.Add(Unit.Pos);
                    move.Positions.Add(t.Pos);

                    possibleMoves.Add(move);
                }
                else if (t.Pos == Unit.Pos)
                {
                    // Extract from ourselves? Not.
                }
                else if (t.Unit != null)
                {
                    // Extract from tile with unit?
                    if (Unit.Owner.PlayerModel.Id == t.Unit.Owner.PlayerModel.Id)
                    {
                        // Extract from own unit?
                        if (t.Unit.ExtractMe)
                        {
                            if (Unit.Engine == null)
                            {
                                // Extract everything
                                Move move = new Move();

                                move.MoveType = MoveType.Extract;

                                move.UnitId = Unit.UnitId;
                                move.OtherUnitId = "Enemy";
                                move.Positions = new List<Position>();
                                move.Positions.Add(Unit.Pos);
                                move.Positions.Add(t.Pos);

                                possibleMoves.Add(move);
                            }
                            else
                            {
                                //if (t.Unit.Engine == null)
                                {
                                    // Extract garbage units that cannot move. Do not extract from containers returning home
                                    Move move = new Move();

                                    move.MoveType = MoveType.Extract;

                                    move.UnitId = Unit.UnitId;
                                    move.OtherUnitId = "Enemy";
                                    move.Positions = new List<Position>();
                                    move.Positions.Add(Unit.Pos);
                                    move.Positions.Add(t.Pos);

                                    possibleMoves.Add(move);
                                }
                            }
                        }
                        else if (t.Unit.Container != null)
                        {
                            if (Unit.Engine == null)                                
                            {
                                // Container extracting from worker
                                if (t.Unit.Engine != null)
                                {
                                    Move move = new Move();

                                    move.MoveType = MoveType.Extract;

                                    move.UnitId = Unit.UnitId;
                                    move.OtherUnitId = "Container";
                                    move.Positions = new List<Position>();
                                    move.Positions.Add(Unit.Pos);
                                    move.Positions.Add(t.Pos);

                                    possibleMoves.Add(move);
                                }
                            }
                            else
                            {
                                if (Unit.Weapon != null || Unit.Assembler != null)
                                {
                                    // Fighter or assembler extract from container
                                    Move move = new Move();

                                    move.MoveType = MoveType.Extract;

                                    move.UnitId = Unit.UnitId;
                                    move.OtherUnitId = "Container";
                                    move.Positions = new List<Position>();
                                    move.Positions.Add(Unit.Pos);
                                    move.Positions.Add(t.Pos);

                                    possibleMoves.Add(move);
                                }
                            }
                        }                        
                    }
                    else
                    {
                        // Cannot extract if shield is up
                        if (t.Unit.Armor == null || !t.Unit.Armor.ShieldActive)
                        {
                            // Extract from enemy? Always an option
                            Move move = new Move();

                            move.MoveType = MoveType.Extract;

                            move.UnitId = Unit.UnitId;
                            move.OtherUnitId = "Enemy";
                            move.Positions = new List<Position>();
                            move.Positions.Add(Unit.Pos);
                            move.Positions.Add(t.Pos);

                            possibleMoves.Add(move);
                        }
                    }
                }
            }

        }

        public bool CanExtract
        {
            get
            {
                if (Unit.Power == 0)
                    return false;

                if (Unit.Weapon != null)
                {
                    if (Unit.Weapon.Container != null && Unit.Weapon.Container.Mineral < Unit.Weapon.Container.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Assembler != null)
                {
                    if (Unit.Assembler.Container != null && Unit.Assembler.Container.Mineral < Unit.Assembler.Container.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Reactor != null)
                {
                    if (Unit.Reactor.Container != null && Unit.Reactor.Container.Mineral < Unit.Reactor.Container.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Container != null && Unit.Container.Mineral < Unit.Container.Capacity)
                {
                    return true;
                }                
                return false;
            }
        }


        public bool ExtractInto(Position from, List<Move> moves, Game game, bool fromGround)
        {
            Tile fromTile = Unit.Game.Map.GetTile(from);
            
            if (fromGround)
            {
                int x = 0;
            }
            /*
            bool canExtract = false;

            if (Unit.Metal == 0)
            {
                canExtract = true;
            }
            else
            {
                if (Unit.Container != null && Unit.Container.Metal < Unit.Container.Capacity)
                    canExtract = true;
            }
            */
            if (!CanExtract)
                return false;
            int metalRemoved = 0;

            if (fromGround == false && fromTile.Unit != null)
            {
                // friendly unit
                if (fromTile.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    /*
                    Container targetContainer = null;
                    if (Unit.Weapon != null && Unit.Weapon.Container != null && Unit.Weapon.Container.Metal < Unit.Weapon.Container.Capacity)
                    {
                        targetContainer = Unit.Weapon.Container;
                    }
                    if (Unit.Assembler != null && Unit.Assembler.Container != null && Unit.Assembler.Container.Metal < Unit.Assembler.Container.Capacity)
                    {
                        targetContainer = Unit.Assembler.Container;
                    }
                    if (Unit.Reactor != null && Unit.Reactor.Container != null && Unit.Reactor.Container.Metal < Unit.Reactor.Container.Capacity)
                    {
                        targetContainer = Unit.Reactor.Container;
                    }
                    if (Unit.Container != null)
                    {
                        targetContainer = Unit.Container;
                    }*/
                    bool canExtractMore = true;

                    Container sourceContainer = null;
                    if (fromTile.Unit.Container != null && fromTile.Unit.Container.Mineral > 0)
                    {
                        sourceContainer = fromTile.Unit.Container;
                    }
                    if (sourceContainer != null && sourceContainer.Mineral > 0)
                    {
                        int totalMetal = 0;

                        if (fromTile.Unit.Engine != null)
                        {
                            // Take all
                            totalMetal = sourceContainer.Mineral;
                        }
                        else
                        {
                            if (Unit.BuilderWaitForMetal)
                            {
                                Unit.BuilderWaitForMetal = false;
                            }
                            else
                            {
                                // Take all
                                totalMetal = sourceContainer.Mineral;
                            }
                        }

                        if (totalMetal > 0 && Unit.Weapon != null && Unit.Weapon.Container != null && Unit.Weapon.Container.Mineral < Unit.Weapon.Container.Capacity)
                        {
                            int mins = Unit.Weapon.Container.Capacity - Unit.Weapon.Container.Mineral;
                            if (mins > totalMetal)
                            {
                                metalRemoved += totalMetal;
                                totalMetal = 0;
                            }
                            else
                            {
                                metalRemoved += mins;
                                totalMetal -= mins;
                            }
                        }
                        if (totalMetal > 0 && Unit.Assembler != null && Unit.Assembler.Container != null && Unit.Assembler.Container.Mineral < Unit.Assembler.Container.Capacity)
                        {
                            int mins = Unit.Assembler.Container.Capacity - Unit.Assembler.Container.Mineral;
                            if (mins > totalMetal)
                            {
                                metalRemoved += totalMetal;
                                totalMetal = 0;
                            }
                            else
                            {
                                metalRemoved += mins;
                                totalMetal -= mins;
                            }
                        }
                        if (totalMetal > 0 && Unit.Reactor != null && Unit.Reactor.Container != null && Unit.Reactor.Container.Mineral < Unit.Reactor.Container.Capacity)
                        {
                            int mins = Unit.Reactor.Container.Capacity - Unit.Reactor.Container.Mineral;
                            if (mins > totalMetal)
                            {
                                metalRemoved += totalMetal;
                                totalMetal = 0;
                            }
                            else
                            {
                                metalRemoved += mins;
                                totalMetal -= mins;
                            }
                        }
                        if (totalMetal > 0 && Unit.Container != null && Unit.Container.Mineral < Unit.Container.Capacity)
                        {
                            int mins = Unit.Container.Capacity - Unit.Container.Mineral;
                            if (mins > totalMetal)
                            {
                                metalRemoved += totalMetal;
                                totalMetal = 0;
                            }
                            else
                            {
                                metalRemoved += mins;
                                totalMetal -= mins;
                            }
                        }

                        if (totalMetal > 0)
                        {
                            canExtractMore = true;
                            //break;
                        }
                        sourceContainer.Mineral -= metalRemoved;
                        /*
                        if (targetContainer.Metal + totalMetal > targetContainer.Capacity)
                        {
                            // Remove not more than fits in the container
                            metalRemoved = targetContainer.Capacity - targetContainer.Metal;
                            sourceContainer.Metal -= metalRemoved;
                        }
                        else
                        {
                            metalRemoved = totalMetal;
                            sourceContainer.Metal -= metalRemoved;
                        }*/

                        /*
                        if (fromTile.Unit.Metal > 0)
                        {
                            metalRemoved += fromTile.Unit.Metal;
                            fromTile.Unit.Metal = 0;
                        }*/


                        if (!game.changedUnits.ContainsKey(fromTile.Unit.Pos))
                            game.changedUnits.Add(fromTile.Unit.Pos, fromTile.Unit);
                        /*
                        Move moveUpdate = new Move();

                        moveUpdate.MoveType = MoveType.UpdateStats;
                        moveUpdate.UnitId = fromTile.Unit.UnitId;
                        moveUpdate.PlayerId = fromTile.Unit.Owner.PlayerModel.Id;
                        moveUpdate.Positions = new List<Position>();
                        moveUpdate.Positions.Add(fromTile.Unit.Pos);
                        moveUpdate.Stats = fromTile.Unit.CollectStats();

                        moves.Add(moveUpdate);
                        */

                        /*
                        else
                        {
                            metalRemoved = 1;
                            fromTile.Unit.Container.Metal--;
                        }*/
                    }

                    if (fromTile.Unit.ExtractMe && canExtractMore)
                    {
                        Unit targetUnit = fromTile.Unit;

                        /*if (targetUnit.Metal > 0)
                        {
                            // Remove metal first
                            targetUnit.Metal--;
                            metalRemoved += 1;
                        }
                        else*/
                        {
                            // Remove parts
                            int totalMetalInUnitBeforeHit = targetUnit.CountMetal();

                            // Extract own, useless unit
                            if (targetUnit.HitBy(null))
                            {
                                game.UpdateGroundPlates(moves, targetUnit, remove: true);

                                // Unit extracted remove unit
                                Move deleteMove = new Move();
                                deleteMove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                                deleteMove.MoveType = MoveType.Delete;
                                deleteMove.Positions = new List<Position>();
                                deleteMove.Positions.Add(targetUnit.Pos);
                                deleteMove.UnitId = targetUnit.UnitId;
                                moves.Add(deleteMove);

                                game.Map.Units.Remove(targetUnit.Pos);

                                int totalMetalAfterUnit = targetUnit.CountMetal();
                                metalRemoved += totalMetalInUnitBeforeHit - totalMetalAfterUnit;

                                // Bullet + demaged Part + collected metal
                                if (metalRemoved > 1)
                                {
                                    if (Unit.Container != null)
                                    {
                                        //Unit.Container.Metal += releasedMetal;
                                    }
                                    else
                                    {
                                        // TODO: Extracted from a container?
                                        //throw new Exception("TODO");
                                    }
                                }
                            }
                            else
                            {
                                // Unit remains
                                Move hitmove = new Move();
                                hitmove.MoveType = MoveType.Hit;
                                hitmove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                                hitmove.Positions = new List<Position>();
                                hitmove.Positions.Add(targetUnit.Pos);
                                hitmove.UnitId = targetUnit.UnitId;
                                hitmove.Stats = targetUnit.CollectStats();
                                moves.Add(hitmove);

                                int totalMetalAfterUnit = targetUnit.CountMetal();
                                metalRemoved += totalMetalInUnitBeforeHit - totalMetalAfterUnit;

                                // Bullet + demage Part
                                if (metalRemoved > 1)
                                {
                                    if (Unit.Container != null)
                                    {
                                        //Unit.Container.Metal += releasedMetal;
                                    }
                                    else
                                    {
                                        // TODO: Extracted from a container?
                                        //throw new Exception("TODO");
                                        // Killed container
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Enemy unit!
                    Unit targetUnit = fromTile.Unit;

                    /*if (targetUnit.Metal > 0)

                    {
                        // Remove metal first
                        targetUnit.Metal--;
                        metalRemoved += 1;
                    }
                    else*/
                    {
                        int totalMetalInUnitBeforeHit = targetUnit.CountMetal();

                        // Extract own, useless unit
                        if (targetUnit.HitBy(null))
                        {
                            // Unit extracted remove unit
                            Move deleteMove = new Move();
                            deleteMove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                            deleteMove.MoveType = MoveType.Delete;
                            deleteMove.Positions = new List<Position>();
                            deleteMove.Positions.Add(targetUnit.Pos);
                            deleteMove.UnitId = targetUnit.UnitId;
                            moves.Add(deleteMove);

                            game.Map.Units.Remove(targetUnit.Pos);

                            int totalMetalAfterUnit = targetUnit.CountMetal();
                            metalRemoved += totalMetalInUnitBeforeHit - totalMetalAfterUnit;

                            // Bullet + demaged Part + collected metal
                            if (metalRemoved > 1)
                            {
                                if (Unit.Container != null)
                                {
                                    //Unit.Container.Metal += releasedMetal;
                                }
                                else
                                {
                                    // TODO: Extracted from a container?
                                    //throw new Exception("TODO");
                                }
                            }
                        }
                        else
                        {
                            // Unit remains
                            Move hitmove = new Move();
                            hitmove.MoveType = MoveType.Hit;
                            hitmove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                            hitmove.Positions = new List<Position>();
                            hitmove.Positions.Add(targetUnit.Pos);
                            hitmove.UnitId = targetUnit.UnitId;
                            hitmove.Stats = targetUnit.CollectStats();
                            moves.Add(hitmove);

                            int totalMetalAfterUnit = targetUnit.CountMetal();
                            metalRemoved += totalMetalInUnitBeforeHit - totalMetalAfterUnit;

                            // Bullet + demage Part
                            if (metalRemoved > 1)
                            {
                                if (Unit.Container != null)
                                {
                                    //Unit.Container.Metal += releasedMetal;
                                }
                                else
                                {
                                    // TODO: Extracted from a container?
                                    //throw new Exception("TODO");
                                    // Killed container
                                }
                            }
                        }
                    }
                }
            }

            if (metalRemoved == 0 && fromTile.Metal > 0)
            {
                // Extract from ground
                metalRemoved = 1;
                fromTile.AddMinerals(-1);

            }
            bool didRemove = false;
            if (metalRemoved > 0)
            {
                if (Unit.Reactor != null && Unit.Reactor.Container != null)
                {
                    if (Unit.Reactor.Container.Mineral + metalRemoved > Unit.Reactor.Container.Capacity)
                    {
                        metalRemoved -= Unit.Reactor.Container.Capacity - Unit.Reactor.Container.Mineral;
                        Unit.Reactor.Container.Mineral = Unit.Reactor.Container.Capacity;
                    }
                    else
                    {
                        Unit.Reactor.Container.Mineral += metalRemoved;
                        metalRemoved = 0;
                    }
                    Unit.Reactor.BurnIfNeccessary();
                    didRemove = true;
                }
            }
            if (metalRemoved > 0)
            {
                if (Unit.Assembler != null && Unit.Assembler.Container != null)
                {
                    if (Unit.Assembler.Container.Mineral + metalRemoved > Unit.Assembler.Container.Capacity)
                    {
                        metalRemoved -= Unit.Assembler.Container.Capacity - Unit.Assembler.Container.Mineral;
                        Unit.Assembler.Container.Mineral = Unit.Assembler.Container.Capacity;
                    }
                    else
                    {
                        Unit.Assembler.Container.Mineral += metalRemoved;
                        metalRemoved = 0;
                    }
                    didRemove = true;
                }
            }
            if (metalRemoved > 0)
            {
                if (Unit.Weapon != null && Unit.Weapon.Container != null)
                {
                    if (Unit.Weapon.Container.Mineral + metalRemoved > Unit.Weapon.Container.Capacity)
                    {
                        metalRemoved -= Unit.Weapon.Container.Capacity - Unit.Weapon.Container.Mineral;
                        Unit.Weapon.Container.Mineral = Unit.Weapon.Container.Capacity;
                    }
                    else
                    {
                        Unit.Weapon.Container.Mineral += metalRemoved;
                        metalRemoved = 0;
                    }
                    didRemove = true;
                }
            }
            if (metalRemoved > 0)
            {               
                if (Unit.Container != null)
                {
                    if (Unit.Container.Mineral + metalRemoved > Unit.Container.Capacity)
                    {
                        metalRemoved -= Unit.Container.Capacity - Unit.Container.Mineral;
                        Unit.Container.Mineral = Unit.Container.Capacity;
                    }
                    else
                    {
                        Unit.Container.Mineral += metalRemoved;
                        metalRemoved = 0;
                    }
                    didRemove = true;
                }                
            }
            if (metalRemoved > 0)
            {
                //fromTile.Metal += metalRemoved;
                fromTile.AddMinerals(metalRemoved);
                didRemove = true;
            }
            return didRemove;
        }
    }
}
