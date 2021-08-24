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
        public override string Name { get { return "Extractor"; } }

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
                    if (tile.Unit.Container != null && tile.Unit.Container.TileContainer.Minerals > 0)
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

                if (tile.Minerals > 0)
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
                metal += n.Minerals;
            }
            return metal;
        }


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

            Dictionary<Position, TileWithDistance> resultList = CollectTilesWithMetal();

            foreach (TileWithDistance t in resultList.Values)
            {
                foreach (TileObject tileObject in t.Tile.TileContainer.TileObjects)
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
                            if (Unit.Engine == null)
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
                            else
                            {
                                //if (t.Unit.Engine == null)
                                {
                                    // Extract garbage units that cannot move. Do not extract from containers returning home
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

                                    //possibleMoves.Add(move);
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
                return CanExtractMinerals || CanExtractDirt;
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
                    if (Unit.Weapon.TileContainer != null && Unit.Weapon.TileContainer.Loaded < Unit.Weapon.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CanExtractMinerals
        {
            get
            {
                if (Unit.Power == 0)
                    return false;

                if (Unit.Assembler != null)
                {
                    if (Unit.Assembler.TileContainer != null && Unit.Assembler.TileContainer.Minerals < Unit.Assembler.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Reactor != null)
                {
                    if (Unit.Reactor.TileContainer != null && Unit.Reactor.TileContainer.Minerals < Unit.Reactor.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Weapon != null)
                {
                    if (Unit.Weapon.TileContainer != null && Unit.Weapon.TileContainer.Minerals < Unit.Weapon.TileContainer.Capacity)
                    {
                        return true;
                    }
                }
                if (Unit.Container != null && Unit.Container.TileContainer.Minerals < Unit.Container.TileContainer.Capacity)
                {
                    return true;
                }                
                return false;
            }
        }
        public bool ExtractInto(Position from, List<Bullet> hitByBullet, Game game, string otherUnitId)
        {
            Tile fromTile = Unit.Game.Map.GetTile(from);

            List<TileObject> removeTileObjects = new List<TileObject>();

            if (otherUnitId.StartsWith("unit"))
            {
                Unit otherUnit = fromTile.Unit;
                if (otherUnit == null || otherUnit.UnitId != otherUnitId)
                {
                    // Extract from unit, but no longer there or not from this unit
                    return false;
                }
                if (otherUnit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
                    int capacity = Unit.CountCapacity();
                    // friendly unit
                    while (capacity-- > 0)
                    {
                        if (!otherUnit.RemoveTileObjects(removeTileObjects, 1, TileObjectType.All))
                        {
                            break;
                        }
                    }

                    if (otherUnit.ExtractMe && Unit.CanFill())
                    {
                        Bullet bullet = new Bullet();
                        bullet.Target = fromTile.Pos;
                        //bullet.BulletType = "Extract";
                        hitByBullet.Add(bullet);
                    }
                }
                else
                {
                    // enemy unit
                    Bullet bullet = new Bullet();
                    bullet.Target = fromTile.Pos;
                    //bullet.BulletType = "Extract";
                    hitByBullet.Add(bullet);
                }
            }
            else
            {
                TileObject removedTileObject;

                // Extract from tile
                TileObjectType tileObjectType = Tile.GetObjectType(otherUnitId);
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
                    if (removedTileObject == null)
                    {
                        int x = 0;
                        
                    }
                }
                if (removedTileObject != null)
                    removeTileObjects.Add(removedTileObject);
            }
            /*
            if (!CanExtractMinerals)
                return false;
            int metalRemoved = 0;

            if (otherUnitId != "Mineral" && fromTile.Unit != null)
            {
                // friendly unit
                if (fromTile.Unit.Owner.PlayerModel.Id == Unit.Owner.PlayerModel.Id)
                {
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

                        if (!game.changedUnits.ContainsKey(fromTile.Unit.Pos))
                            game.changedUnits.Add(fromTile.Unit.Pos, fromTile.Unit);
                        
                    }

                    if (fromTile.Unit.ExtractMe && canExtractMore)
                    {
                        Bullet bullet = new Bullet();
                        bullet.Target = fromTile.Pos;
                        bullet.BulletType = "Extract";
                        hitByBullet.Add(bullet);
                    }
                }
                else
                {
                    Bullet bullet = new Bullet();
                    bullet.Target = fromTile.Pos;
                    bullet.BulletType = "Extract";
                    hitByBullet.Add(bullet);
                }
            }

            if (metalRemoved == 0 && fromTile.Metal > 0)
            {
                // Extract from ground
                metalRemoved = 1;
                fromTile.AddMinerals(-1);
            }
            */
            bool didRemove = removeTileObjects.Count > 0;

            Unit.AddTileObjects(removeTileObjects);
            
            if (removeTileObjects.Count > 0)
            {
                fromTile.TileContainer.TileObjects.AddRange(removeTileObjects);
            }

            return didRemove;
        }
        
    }
}
