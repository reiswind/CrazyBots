using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Weapon : Ability
    {
        public override string Name { get { return "Weapon"; } }

        // Testmode
        public bool FireAtGround { get; set; }
        public bool EndlessAmmo { get; set; }
        public bool HoldFire { get; set; }

        public Weapon(Unit owner, int level, int range) : base(owner, TileObjectType.PartWeapon)
        {
            TileContainer = new TileContainer();
            this.range = range;
            if (level == 1)
                TileContainer.Capacity = 1;
            else if (level == 2)
                TileContainer.Capacity = 3;
            else if (level == 3)
                TileContainer.Capacity = 6;
            TileContainer.AcceptedTileObjectTypes = TileObjectType.Ammo;

            Level = level;
        }

        public bool WeaponLoaded
        {
            get
            {
                if (Unit.Power == 0)
                    return false;
                if (TileContainer != null && TileContainer.Count > 0)
                    return true;
                if (Unit.Container != null && Unit.Container.TileContainer.Count > 0)
                    return true;
                return false;
            }
        }
        private int range;
        public int Range
        {
            get
            {
                return range;
            }
        }
        /*
        public int Range
        {
            get
            {
                if (Level == 1) return 2;
                if (Level == 2) return 3;
                if (Level == 3) return 4;
                return Level * 3;
            }
        }*/

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Fire) == 0)
                return;

            if (!WeaponLoaded || HoldFire)
                return;

            Dictionary<Position2, TileWithDistance> tiles = Unit.Game.Map.EnumerateTiles(Unit.Pos, Range , false, matcher: tile =>
            {
                if (tile.Pos == Unit.Pos)
                    return true;

                // Cannot shoot at thins that are not visible
                if (!Unit.Owner.VisiblePositions.ContainsKey(tile.Pos))
                    return false;

                return true;
            });


            foreach (TileWithDistance n in tiles.Values)
            {
                // Cannot fire on ground
                if (n.Unit == null)
                {
                    if (n.Tile.Count > 0)
                    {
                        foreach (TileObject tileObject in n.Tile.TileObjects)
                        {
                            if (tileObject.Direction == Direction.C && tileObject.TileObjectType != TileObjectType.Mineral)
                                continue;

                            if (FireAtGround)
                            {
                                /* No longer fire at destrucables. They are extracted now */

                                Move move = new Move();
                                move.MoveType = MoveType.Fire;
                                move.UnitId = Unit.UnitId;
                                move.OtherUnitId = tileObject.TileObjectType.ToString();
                                move.Positions = new List<Position2>();
                                move.Positions.Add(Unit.Pos);
                                move.Positions.Add(n.Tile.Pos);

                                //possibleMoves.Add(move);
                            }
                        }
                    }

                    if (FireAtGround)
                    {
                        /* No longer fire at ground. They are extracted now */

                        Move move = new Move();
                        move.MoveType = MoveType.Fire;
                        move.UnitId = Unit.UnitId;
                        //move.OtherUnitId = tileObject.TileObjectType.ToString();
                        move.Positions = new List<Position2>();
                        move.Positions.Add(Unit.Pos);
                        move.Positions.Add(n.Tile.Pos);

                        possibleMoves.Add(move);
                    }
                    else
                    {
                        continue;
                    }
                    
                }
                else
                {
                    // Cannot fire on ourselves
                    if (n.Unit.Owner.PlayerModel.Id != 0 && n.Unit.Owner != Unit.Owner)
                    {
                        Move move = new Move();
                        move.MoveType = MoveType.Fire;
                        move.UnitId = Unit.UnitId;
                        move.OtherUnitId = null;
                        move.Positions = new List<Position2>();
                        move.Positions.Add(Unit.Pos);
                        move.Positions.Add(n.Pos);

                        possibleMoves.Add(move);
                    }
                }
            }
        }
    }
}
