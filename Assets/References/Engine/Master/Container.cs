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
        public override string Name { get { return "Container"; } }

        public Container(Unit owner, int level) : base(owner, TileObjectType.PartContainer)
        {
            Level = level;
            TileContainer = new TileContainer();
            ResetCapacity();
        }

        public void ResetCapacity()
        {
            if (Level == 1) TileContainer.Capacity = 20;
            if (Level == 2) TileContainer.Capacity = 60;
            if (Level == 3) TileContainer.Capacity = 220;
        }
        /// <summary>
        /// Transport Range
        /// </summary>
        public int Range
        {
            get
            {
                return 12; // Level * 3;
            }
        }
        public override void ComputePossibleMoves(List<Move> possibleMoves, List<ulong> includedulongs, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Transport) == 0)
                return;

            if (TileContainer.TileObjects.Count() == 0)
                return;

            // For now: Only Minerals are transported
            if (TileContainer.Minerals == 0)
                return;

            Dictionary<ulong, TileWithDistance> tiles = Unit.Game.Map.EnumerateTiles(Unit.Pos, Range, false, matcher: tile =>
            {
                if (tile.Pos == Unit.Pos)
                    return true;

                if (!Unit.Owner.VisiblePositions.Contains(tile.Pos))
                    return false;

                return true;
            });


            foreach (TileWithDistance n in tiles.Values)
            {
                // Do not transport to direct neighbors, there should be an extractor
                if (n.Distance <= 1)
                    continue;

                if (n.Unit != null && 
                    n.Unit.Owner.PlayerModel.Id != 0 && 
                    n.Unit.Owner == Unit.Owner &&
                    n.Unit.IsComplete())
                {
                    bool transport = false;

                    // Fill only container buildings
                    if (n.Unit.Engine == null && 
                        n.Unit.Container != null &&
                        n.Unit.Container.TileContainer.Count < n.Unit.Container.TileContainer.Capacity &&
                        n.Unit.Container.TileContainer.Count < TileContainer.Count - 1)
                        transport = true;

                    if (n.Unit.Engine == null && 
                        n.Unit.Assembler != null &&
                        n.Unit.Assembler.TileContainer.Count < n.Unit.Assembler.TileContainer.Capacity)
                        transport = true;

                    if (n.Unit.Engine == null &&
                        n.Unit.Reactor != null &&
                        n.Unit.Reactor.TileContainer.Count < n.Unit.Reactor.TileContainer.Capacity)
                        transport = true;

                    if (n.Unit.Engine == null &&
                        n.Unit.Weapon != null &&
                        n.Unit.Weapon.TileContainer.Count < n.Unit.Weapon.TileContainer.Capacity)
                        transport = true;

                    if (transport)
                    {
                        Move move = new Move();
                        move.MoveType = MoveType.Transport;
                        move.UnitId = Unit.UnitId;
                        move.OtherUnitId = n.Unit.UnitId;
                        move.Positions = new List<ulong>();
                        move.Positions.Add(Unit.Pos);
                        move.Positions.Add(n.Pos);

                        possibleMoves.Add(move);
                    }
                }
            }
        }
    }
}

