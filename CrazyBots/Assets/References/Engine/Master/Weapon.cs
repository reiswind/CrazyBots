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
        public Container Container { get; set; }
        public int Level { get; set; }
        public Weapon(Unit owner, int level) : base(owner)
        {
            Container = new Container(owner, 1);

            if (level == 1)
                Container.Capacity = 1;
            else if (level == 2)
                Container.Capacity = 3;
            else if (level == 3)
                Container.Capacity = 10;


            Level = level;
        }

        public bool WeaponLoaded
        {
            get
            {
                if (Unit.Power == 0)
                    return false;
                if (Container != null && Container.Mineral > 0)
                    return true;
                if (Unit.Container != null && Unit.Container.Mineral > 0)
                    return true;
                return false;
            }
        }

        public int Range
        {
            get
            {
                return Level * 3;
            }
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Fire) == 0)
                return;

            if (!WeaponLoaded)
                return;

            Dictionary<Position, TileWithDistance> tiles = Unit.Game.Map.EnumerateTiles(Unit.Pos, Range , false, matcher: tile =>
            {
                if (tile.Pos == Unit.Pos)
                    return true;

                if (!Unit.Owner.VisiblePositions.Contains(tile.Pos))
                    return false;

                return true;
            });


            //List<Tile> resultList = new List<Tile>();
            //ListHitableTiles(resultList, includedPositions);
            foreach (TileWithDistance n in tiles.Values)
            {
                // Cannot fire on ground
                if (n.Unit == null)
                {
                    if (n.Tile.NumberOfDestructables > 0)
                    {
                        Move move = new Move();
                        move.MoveType = MoveType.Fire;
                        move.UnitId = Unit.UnitId;
                        move.OtherUnitId = "Destructable";
                        move.Positions = new List<Position>();
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
                        move.OtherUnitId = n.Unit.UnitId;
                        move.Positions = new List<Position>();
                        move.Positions.Add(Unit.Pos);
                        move.Positions.Add(n.Pos);

                        possibleMoves.Add(move);
                    }
                }
            }
        }
    }
}
