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
            Container.Capacity = 1;
            Level = level;
        }

        public bool WeaponLoaded
        {
            get
            {
                if (Container != null && Container.Metal > 0)
                    return true;
                if (Unit.Container != null && Unit.Container.Metal > 0)
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

        private void ListHitableTiles(List<Tile> resultList, List<Position> includedPositions)
        {
            List<TileWithDistance> openList = new List<TileWithDistance>();
            Dictionary<Position, TileWithDistance> reachedTiles = new Dictionary<Position, TileWithDistance>();

            TileWithDistance startTile = new TileWithDistance(Unit.Game.Map.GetTile(Unit.Pos), 0);

            openList.Add(startTile);
            reachedTiles.Add(startTile.Pos, startTile);

            while (openList.Count > 0)
            {
                TileWithDistance tile = openList[0];
                openList.RemoveAt(0);

                if (tile.Distance > Range)
                    continue;

                foreach (Tile n in tile.Neighbors)
                {
                    if (n.Pos == Unit.Pos)
                        continue;

                    if (!Unit.Owner.VisiblePositions.Contains(n.Pos))
                        continue;

                    if (includedPositions != null)
                    {
                        if (!includedPositions.Contains(n.Pos))
                            continue;
                    }
                    if (!reachedTiles.ContainsKey(n.Pos))
                    {
                        TileWithDistance neighborsTile = new TileWithDistance(Unit.Game.Map.GetTile(n.Pos), tile.Distance + 1);
                        reachedTiles.Add(neighborsTile.Pos, neighborsTile);

                        if (neighborsTile.Distance <= Range)
                        {
                            openList.Add(neighborsTile);
                            resultList.Add(n);
                        }
                    }
                }
            }
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Fire) == 0)
                return;

            if (!WeaponLoaded)
                return;

            List<Tile> resultList = new List<Tile>();
            ListHitableTiles(resultList, includedPositions);
            foreach (Tile n in resultList)
            {
                // Cannot fire on ground
                if (n.Unit == null)
                {
                    if (n.NumberOfSmallTrees > 0 || n.NumberOfRocks > 0)
                    {
                        Move move = new Move();
                        move.MoveType = MoveType.Fire;
                        move.UnitId = Unit.UnitId;
                        move.OtherUnitId = "Tree";
                        move.Positions = new List<Position>();
                        move.Positions.Add(Unit.Pos);
                        move.Positions.Add(n.Pos);

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
                    if (n.Unit.Owner != Unit.Owner)
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
