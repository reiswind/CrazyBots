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
                        if (n.NumberOfDestructables > 0)
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
                    move.OtherUnitId = "Mineral";
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
                                move.OtherUnitId = "Friendly";
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
                    if (Unit.Weapon.Container != null && Unit.Weapon.Container.Dirt < Unit.Weapon.Container.Capacity)
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
                if (Unit.Weapon != null)
                {
                    if (Unit.Weapon.Container != null && Unit.Weapon.Container.Mineral < Unit.Weapon.Container.Capacity)
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


        public bool ExtractInto(Position from, List<Move> moves, Game game, string groundType)
        {
            Tile fromTile = Unit.Game.Map.GetTile(from);


            if (groundType == "Dirt" || groundType == "Destructable")
            {
                if (Unit.Weapon == null || Unit.Weapon.Container == null)
                    return false;
                if (!CanExtractDirt)
                    return false;

                if (fromTile.NumberOfDestructables > 0)
                    fromTile.NumberOfDestructables--;
                else
                    fromTile.Height -= 0.1f;
                Unit.Weapon.Container.Dirt++;

                return true;
            }

            if (!CanExtractMinerals)
                return false;
            int metalRemoved = 0;

            if (groundType != "Mineral" && fromTile.Unit != null)
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
                        game.HitByBullet(fromTile.Pos, moves);
                    }
                }
                else
                {
                    game.HitByBullet(fromTile.Pos, moves);
                }
            }

            if (metalRemoved == 0 && fromTile.Metal > 0)
            {
                // Extract from ground
                metalRemoved = 1;
                fromTile.AddMinerals(-1);
            }

            int remainingMinerals = Unit.AddMinerals(metalRemoved);
            bool didRemove = remainingMinerals < metalRemoved;
            if (remainingMinerals > 0)
            {
                //fromTile.Metal += metalRemoved;
                fromTile.AddMinerals(remainingMinerals);
                didRemove = true;
            }
            return didRemove;
        }
    }
}
