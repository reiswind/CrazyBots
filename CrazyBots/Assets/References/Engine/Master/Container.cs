using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Container : Ability
    {
        public int Level { get; set; }
        public int Mineral { get; set; }
        public int Dirt { get; set; }

        private int extraCapacity;

        public bool IsFreeSpace
        {
            get
            {
                return Mineral + Dirt < Capacity;
            }
        }
        public int Capacity
        {
            get
            {
                //return 30;

                if (extraCapacity != 0) return extraCapacity;
                if (Level == 1) return 20;
                if (Level == 2) return 60;
                if (Level == 3) return 220;
                return Level * 20;
            }
            set
            {
                extraCapacity = value;
            }
        }

        public Container(Unit owner, int level) : base(owner)
        {
            Level = level;
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
            if ((moveFilter & MoveFilter.Transport) == 0)
                return;

            if (Mineral == 0)
                return;

            Dictionary<Position, TileWithDistance> tiles = Unit.Game.Map.EnumerateTiles(Unit.Pos, Range, false, matcher: tile =>
            {
                if (tile.Pos == Unit.Pos)
                    return true;

                if (!Unit.Owner.VisiblePositions.Contains(tile.Pos))
                    return false;

                return true;
            });


            foreach (TileWithDistance n in tiles.Values)
            {
                if (n.Unit != null && 
                    n.Unit.Owner.PlayerModel.Id != 0 && 
                    n.Unit.Owner == Unit.Owner &&
                    n.Unit.IsComplete())
                {
                    bool transport = false;

                    // Fill only container buildings
                    if (n.Unit.Engine == null && 
                        n.Unit.Container != null &&
                        n.Unit.Container.Mineral < n.Unit.Container.Capacity &&
                        n.Unit.Container.Mineral < Mineral-1)
                        transport = true;

                    if (n.Unit.Engine == null && 
                        n.Unit.Assembler != null &&
                        n.Unit.Assembler.Container.Mineral < n.Unit.Assembler.Container.Capacity)
                        transport = true;

                    if (n.Unit.Engine == null &&
                        n.Unit.Reactor != null &&
                        n.Unit.Reactor.Container.Mineral < n.Unit.Reactor.Container.Capacity)
                        transport = true;

                    if (n.Unit.Engine == null &&
                        n.Unit.Weapon != null &&
                        n.Unit.Weapon.Container.Mineral < n.Unit.Weapon.Container.Capacity)
                        transport = true;

                    if (transport)
                    {
                        Move move = new Move();
                        move.MoveType = MoveType.Transport;
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

