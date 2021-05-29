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
                    if (tile.Unit.Container != null && tile.Unit.Container.Metal > 0)
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
                if (t.Unit == null)
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
                }
                else if (t.Pos == Unit.Pos)
                {
                    // Extract from ourselves? Not.
                }
                else
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
                        else
                        {
                            if (t.Unit.Container != null)
                            {
                                if (Unit.Engine == null || Unit.BuilderWaitForMetal)
                                {
                                    if (Unit.Container != null)
                                    {
                                        // TAKEALL
                                        if (true || Unit.BuilderWaitForMetal || Unit.Container.Metal + 4 < t.Unit.Container.Metal)
                                        {
                                            // Extract from other container if this metal is less than 
                                            Move move = new Move();

                                            move.MoveType = MoveType.Extract;

                                            move.UnitId = Unit.UnitId;
                                            move.OtherUnitId = "ContainerLess";
                                            move.Positions = new List<Position>();
                                            move.Positions.Add(Unit.Pos);
                                            move.Positions.Add(t.Pos);

                                            possibleMoves.Add(move);
                                        }
                                        else
                                        {
                                            // Got enough
                                        }
                                    }
                                    else
                                    {
                                        // Extract from other container
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
                                    // A Unit with a container and a engine should collect metal from the ground, not from other containers
                                }
                            }
                        }
                    }
                    else
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

        public bool CanExtract
        {
            get
            {
                
                //if (Unit.Metal > 0)
                {
                    if (Unit.Container == null || Unit.Container.Metal >= Unit.Container.Capacity)
                    {
                        // Unit full Not possible to extract
                        return false;
                    }
                }
                return true;
            }
        }


        public bool ExtractInto(Position from, List<Move> moves, Game game)
        {
            Tile fromTile = Unit.Game.Map.GetTile(from);
            
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

            if (fromTile.Unit != null)
            {
                if (fromTile.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    // friendly unit
                    if (fromTile.Unit.Container != null && fromTile.Unit.Container.Metal > 0)
                    {
                        if (Unit.Container != null)
                        {
                            int totalMetal = 0;

                            if (fromTile.Unit.Engine != null)
                            {
                                // Take all
                                totalMetal = fromTile.Unit.Container.Metal;
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
                                    totalMetal = fromTile.Unit.Container.Metal;
                                }

                            }
                            
                            if (Unit.Container.Metal + totalMetal > Unit.Container.Capacity)
                            {
                                // Remove not more than fits in the container
                                metalRemoved = Unit.Container.Capacity - Unit.Container.Metal;
                                fromTile.Unit.Container.Metal -= metalRemoved;
                            }
                            else
                            {
                                metalRemoved = totalMetal;
                                fromTile.Unit.Container.Metal -= metalRemoved;
                            }

                            /*
                            if (fromTile.Unit.Metal > 0)
                            {
                                metalRemoved += fromTile.Unit.Metal;
                                fromTile.Unit.Metal = 0;
                            }*/

                            if (fromTile.Unit.Container.Metal < 0)
                            {
                                throw new Exception("omg");
                            }

                            Move moveUpdate = new Move();

                            moveUpdate.MoveType = MoveType.UpdateStats;
                            moveUpdate.UnitId = fromTile.Unit.UnitId;
                            moveUpdate.PlayerId = fromTile.Unit.Owner.PlayerModel.Id;
                            moveUpdate.Positions = new List<Position>();
                            moveUpdate.Positions.Add(fromTile.Unit.Pos);
                            moveUpdate.Stats = fromTile.Unit.CollectStats();

                            moves.Add(moveUpdate);
                        }
                        else
                        {
                            metalRemoved = 1;
                            fromTile.Unit.Container.Metal--;
                        }
                    }

                    bool canExtractMore = true;
                    if (Unit.Container == null)
                    {
                        // Unit full Not possible to extract
                        canExtractMore = false;
                    }
                    else if (Unit.Container != null && Unit.Container.Metal + metalRemoved >= Unit.Container.Capacity)
                    {
                        // Unit full Not possible to extract
                        canExtractMore = false;
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
                fromTile.Metal--;

            }
            if (metalRemoved > 0)
            {
                /*
                if (Unit.Metal == 0)
                {
                    Unit.Metal = 1;
                    metalRemoved--;
                }*/
                if (metalRemoved > 0)
                {
                    if (Unit.Container == null)
                    {
                        fromTile.Metal += metalRemoved;
                    }
                    else
                    {
                        Unit.Container.Metal += metalRemoved;
                    }
                }
            }

            return metalRemoved > 0;
        }
    }
}
