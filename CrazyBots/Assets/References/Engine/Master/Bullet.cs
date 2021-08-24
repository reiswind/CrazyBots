using Engine.Algorithms;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class Bullet
    {
        public TileObject TileObject { get; set; }
        public Position Target { get; set; }
    }

    public class AbilityBullet : Ability
    {
        public override string Name { get { return "Bullet"; } }

        //public  AbilityBulletModel Model;
        public Move Move;
        private int moved;
        private bool targetReached;

        private List<Position> lineRoute;
        private static int GridSize = 10;

        public AbilityBullet(Unit owner) : base(owner)
        {
            //Model = model;
        }

        public void Init(Move move)
        { 
            Move = move;

            this.lineRoute = new List<Position>();
            Bresehnham.CalcLineTo(
                lineRoute,
                (move.Positions[0].X * GridSize),
                (move.Positions[0].Y * GridSize),
                (move.Positions[move.Positions.Count - 1].X * GridSize),
                (move.Positions[move.Positions.Count - 1].Y * GridSize));
        }

        public static bool CanFireAt(Unit fromUnit, Position target)
        {
            List<Position> lineRoute = new List<Position>();
            Bresehnham.CalcLineTo(
                lineRoute,
                (fromUnit.Pos.X * GridSize),
                (fromUnit.Pos.Y * GridSize),
                (target.X * GridSize),
                (target.Y * GridSize));

            foreach (Position route in lineRoute)
            {
                int x = route.X / GridSize;
                int y = route.Y / GridSize;

                Position p = new Position(x, y);

                /*
                if (fromUnit.Owner.Game.Map.Matrix[x, y] == 0)
                    return false;
                */
                // find units in the area of the bullet
                Unit unit = fromUnit.Owner.Game.Map.Units.GetUnitAt(p);
                if (unit != null &&
                    unit != fromUnit &&
                    unit.Owner == fromUnit.Owner)
                {
                    return false;
                }
            }
            return true;
        }

        public bool RemoveBullet()
        {
            return targetReached;
        }

        internal List<Unit> Advance(Move move)
        {
            List<Unit> unitsThathaveBeenHit = new List<Unit>();
            //List<Position> checkedPos = new List<Position>();

            int lastX = -1;
            int lastY = -1;

            //for (int i = 0; i < Model.Speed * GridSize; i++)
            {
                int nStart = 0; // moved * GridSize;
                moved += 1; // Model.Speed;
                int nEnd = lineRoute.Count; // moved * GridSize;
                if (nEnd >= lineRoute.Count)
                {
                    nEnd = lineRoute.Count;
                    targetReached = true;
                }


                    while (nStart < nEnd)
                    {
                        int x = lineRoute[nStart].X / GridSize;
                        int y = lineRoute[nStart].Y / GridSize;
                        nStart++;
                        if (x == lastX && y == lastY)
                            continue;
                        lastX = x;
                        lastY = y;

                        Position p = new Position(x, y);
                        //checkedPos.Add(p);
                        // find units in the area of the bullet
                        Unit unit = Unit.Owner.Game.Map.Units.GetUnitAt(p);
                        if (unit != null) //.Pos != Move.Position)
                        {
                            if (unit.UnitId == move.UnitId)
                            {
                                // Hit units in direkt way
                                unitsThathaveBeenHit.Add(unit);
                                targetReached = true;
                                break;
                            }
                        }
                    
                    //move.Position = new Position(lastX, lastY);
                    //move.Route[0] = new Position(lastX, lastY);
                }
            }
            return unitsThathaveBeenHit;
        }
    }
}
