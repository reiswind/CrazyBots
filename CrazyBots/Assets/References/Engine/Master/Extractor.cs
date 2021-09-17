﻿using Engine.Interface;
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
        public override string Name { get { return "Extractor"; } }

        public Extractor(Unit owner, int level) : base(owner, TileObjectType.PartExtractor)
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

        public Dictionary<Position, TileWithDistance> CollectExtractableTiles()
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
                    if (tile.Unit.Container != null && tile.Unit.Container.TileContainer.Count > 0)
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
                if (tile.Tile.TileContainer != null)
                {
                    foreach (TileObject tileObject in tile.Tile.TileContainer.TileObjects)
                    {
                        if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                            return true;
                    }
                }
                return false;

            });
        }

        /*
        public int CountAvailableMetal()
        {
            int metal = 0;
            Dictionary<Position, TileWithDistance> resultList = CollectTilesWithMetal();
            foreach (TileWithDistance n in resultList.Values)
            {
                metal += n.Minerals;
            }
            return metal;
        }*/


        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Extract) == 0)
                return;
            /* TODOMIN
            if (CanExtractDirt)
            {
                Tile highest = null;
                Tile t = Unit.Game.Map.GetTile(Unit.Pos);
                foreach (Tile n in t.Neighbors)
                {
                    bool possible = false;
                    if (n.Unit == null &&
                        !n.IsUnderwater &&
                        n.Height >= 0.2f &&
                        n.Height -0.1f > t.Height)
                    {
                        possible = true;
                    }
                    if (!possible)
                    {
                        if (n.TileObjects.Count > 0)
                            possible = true;
                    }

                    if (possible)
                    {
                        if (highest == null)
                            highest = n;
                        else
                        {
                            if (n.Height > highest.Height)
                                highest = n;
                        }
                    }
                }
                if (highest != null)
                {
                    Move move = new Move();

                    move.MoveType = MoveType.Extract;

                    move.UnitId = Unit.UnitId;
                    move.OtherUnitId = "Dirt";
                    move.Positions = new List<Position>();
                    move.Positions.Add(Unit.Pos);
                    move.Positions.Add(highest.Pos);

                    possibleMoves.Add(move);

                }
                // Dirt is good enough
                if (possibleMoves.Count > 0)
                {
                    return;
                }
            }

            if (!CanExtractMinerals)
                // Unit full Not possible to extract
                return;
            */
            if (Unit.ExtractMe)
                return;

            Dictionary<Position, TileWithDistance> resultList = CollectExtractableTiles();

            foreach (TileWithDistance t in resultList.Values)
            {
                foreach (TileObject tileObject in t.Tile.TileContainer.TileObjects)
                {
                    // Everything but gras
                    if (tileObject.Direction == Direction.C && tileObject.TileObjectType != TileObjectType.Mineral)
                        continue;

                    if (Unit.IsSpaceForTileObject(tileObject))
                    {
                        Move move = new Move();

                        move.MoveType = MoveType.Extract;

                        move.UnitId = Unit.UnitId;
                        move.OtherUnitId = tileObject.TileObjectType.ToString();
                        move.Positions = new List<Position>();
                        move.Positions.Add(Unit.Pos);
                        move.Positions.Add(t.Pos);

                        possibleMoves.Add(move);
                    }
                }

                if (t.Pos == Unit.Pos)
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
                            // Extract everything
                            Move move = new Move();

                            move.MoveType = MoveType.Extract;

                            move.UnitId = Unit.UnitId;
                            move.OtherUnitId = t.Unit.UnitId;
                            move.Positions = new List<Position>();
                            move.Positions.Add(Unit.Pos);
                            move.Positions.Add(t.Pos);

                            possibleMoves.Add(move);
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
                                    move.OtherUnitId = t.Unit.UnitId;
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
                                    move.OtherUnitId = t.Unit.UnitId;
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
                        // Cannot extract if enemy shield is up
                        if (t.Unit.Armor == null || !t.Unit.Armor.ShieldActive)
                        {
                            // Extract from enemy? Always an option
                            Move move = new Move();

                            move.MoveType = MoveType.Extract;

                            move.UnitId = Unit.UnitId;
                            move.OtherUnitId = t.Unit.UnitId;
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
                return CanExtractTileObject || CanExtractDirt;
            }
        }

        public bool CanExtractDirt
        {
            get
            {
                if (Unit.Power == 0)
                    return false;

                if (Unit.Weapon != null)
                {
                    // Dirt
                    if (Unit.Weapon.TileContainer != null && Unit.Weapon.TileContainer.Count < Unit.Weapon.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CanExtractTileObject
        {
            get
            {
                if (Unit.Power == 0)
                    return false;

                if (Unit.Assembler != null)
                {
                    if (Unit.Assembler.TileContainer != null && Unit.Assembler.TileContainer.Count < Unit.Assembler.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Reactor != null)
                {
                    if (Unit.Reactor.TileContainer != null && Unit.Reactor.TileContainer.Count < Unit.Reactor.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Weapon != null)
                {
                    if (Unit.Weapon.TileContainer != null && Unit.Weapon.TileContainer.Count < Unit.Weapon.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Container != null && Unit.Container.TileContainer.Count < Unit.Container.TileContainer.Capacity)
                {
                    return true;
                }                
                return false;
            }
        }

        public void ExtractFromUnit(Move move, Unit otherUnit, List<TileObject> removeTileObjects)
        {
            Ability hitPart = otherUnit.HitBy();
            if (hitPart is Shield)
            {
                move.MoveType = MoveType.Skip;
                return;
            }

            if (hitPart.Level == 0 && hitPart.TileContainer != null)
            {
                if (hitPart.TileContainer.TileObjects.Count > 0)
                {
                    removeTileObjects.AddRange(hitPart.TileContainer.TileObjects);
                    hitPart.TileContainer.Clear();
                }
            }

            TileObject removedTileObject = hitPart.PartTileObjects[0];
            hitPart.PartTileObjects.Remove(removedTileObject);
            removeTileObjects.Add(removedTileObject);

            move.Stats = new MoveUpdateStats();
            Unit.Game.Map.CollectGroundStats(otherUnit.Pos, move, removeTileObjects);
            /*
            move.Stats.MoveUpdateGroundStat = new MoveUpdateGroundStat();
            move.Stats.MoveUpdateGroundStat.TileObjects = new List<TileObject>();
            move.Stats.MoveUpdateGroundStat.TileObjects.Add(removedTileObject);
            */
            if (otherUnit.IsDead())
            {
                if (hitPart.PartTileObjects.Count > 0)
                    throw new Exception();
            }
        }

        public bool ExtractInto(Unit unit, Move move, Tile fromTile, Game game, Unit otherUnit, TileObjectType tileObjectType)
        {
            List<TileObject> removeTileObjects = new List<TileObject>();
            //MoveUpdateGroundStat moveUpdateGroundStat;
            //moveUpdateGroundStat = new MoveUpdateGroundStat();

            if (otherUnit != null)
            {
                if (otherUnit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    int capacity = Unit.CountCapacity();
                    int minsInContainer = Unit.CountTileObjectsInContainer();

                    capacity -= minsInContainer;

                    // friendly unit
                    while (capacity-- > 0)
                    {
                        if (!otherUnit.RemoveTileObjects(removeTileObjects, 1, TileObjectType.All, unit))
                        {
                            break;
                        }
                    }

                    if (otherUnit.ExtractMe && !otherUnit.IsDead() && capacity > 0)
                    {
                        ExtractFromUnit(move, otherUnit, removeTileObjects);
                    }
                }
                else
                {
                    // enemy unit
                    if (!otherUnit.IsDead())
                    {
                        ExtractFromUnit(move, otherUnit, removeTileObjects);
                    }
                }
            }
            else
            {
                TileObject removedTileObject;

                // Extract from tile (Only 1)
                if (tileObjectType == TileObjectType.Dirt)
                {
                    fromTile.Height -= 0.1f;

                    removedTileObject = new TileObject();
                    removedTileObject.TileObjectType = TileObjectType.Dirt;
                    removedTileObject.Direction = Direction.C;
                }
                else
                {
                    removedTileObject = fromTile.TileContainer.RemoveTileObject(tileObjectType);
                }
                if (removedTileObject != null)
                {
                    removeTileObjects.Add(removedTileObject);

                    if (removedTileObject.TileObjectType == TileObjectType.Bush ||
                        removedTileObject.TileObjectType == TileObjectType.Tree)
                    {
                        bool containsOtherObstacles = false;
                        foreach (TileObject tileObject in fromTile.TileContainer.TileObjects)
                        {
                            if (tileObject.TileObjectType == TileObjectType.Bush ||
                                tileObject.TileObjectType == TileObjectType.Tree)
                            {
                                containsOtherObstacles = true;
                            }
                        }
                        if (containsOtherObstacles == false)
                        {
                            Unit.Game.Map.AddOpenTile(fromTile);
                        }
                    }
                }
            }

            // The removed tileobjects will be in the move until the next move
            move.Stats = unit.CollectStats();
            Unit.Game.Map.CollectGroundStats(unit.Pos, move, removeTileObjects);

            bool didRemove = removeTileObjects.Count > 0;

            return didRemove;
        }
    }
}
