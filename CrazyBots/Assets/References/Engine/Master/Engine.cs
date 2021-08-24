using Engine.Algorithms;
using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Engine : Ability
    {
        public override string Name { get { return "Engine"; } }

        public int Range
        {
            get
            {
                if (Level == 1) return 1;
                if (Level == 2) return 2;
                if (Level == 3) return 3;
                return 0;
            }
        }

        public Engine(Unit owner, int level) : base(owner)
        {
            Level = level;
        }

        public override void ComputePossibleMoves(List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            if ((moveFilter & MoveFilter.Move) == 0)
                return;

            if (Unit.Power == 0)
                return;

            // Never called by controls
            List<Tile> openList = new List<Tile>();
            List<Tile> reachedTiles = new List<Tile>();
            List<Position> reachedPos = new List<Position>();

            Tile startTile = Unit.Game.Map.GetTile(Unit.Pos);
            openList.Add(startTile);
            reachedTiles.Add(startTile);

            while (openList.Count > 0)
            {
                Tile tile = openList[0];
                openList.RemoveAt(0);

                // Distance at all
                double d = tile.Pos.GetDistanceTo(this.Unit.Pos);
                if (d >= Range)
                    continue;

                foreach (Tile n in tile.Neighbors)
                {
                    if (n.Pos == Unit.Pos)
                        continue;
                    if (includedPositions != null)
                    {
                        if (!includedPositions.Contains(n.Pos))
                            continue;
                    }
                    if (!reachedTiles.Contains(n))
                    {
                        reachedTiles.Add(n);

                        double d1 = n.Pos.GetDistanceTo(this.Unit.Pos);
                        if (d1 < Range)
                        {
                            openList.Add(n);

                            Move move = new Move();
                            move.MoveType = MoveType.Move;

                            PathFinderFast pathFinder = new PathFinderFast(Unit.Owner.Game.Map);

                            move.Positions = pathFinder.FindPath(Unit, Unit.Pos, n.Pos);
                            if (move.Positions != null)
                            {
                                move.UnitId = Unit.UnitId;
                                move.PlayerId = Unit.Owner.PlayerModel.Id;

                                while (move.Positions.Count > Range + 1)
                                {
                                    move.Positions.RemoveAt(move.Positions.Count - 1);
                                }

                                Position finalPos = move.Positions[move.Positions.Count - 1];
                                if (!reachedPos.Contains(finalPos))
                                {
                                    reachedPos.Add(finalPos);


                                    // Do not move on other units
                                    if (Unit.Game.Map.GetTile(finalPos).Unit == null)
                                        possibleMoves.Add(move);
                                    else
                                    {
                                        //int x = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}
