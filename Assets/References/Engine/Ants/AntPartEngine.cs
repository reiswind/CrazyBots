
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Ants
{
    internal class AntPartEngine : AntPart
    {


        public Engine.Master.Engine Engine { get; private set; }
        public AntPartEngine(Ant ant, Engine.Master.Engine extractor) : base(ant)
        {
            Engine = extractor;
        }
        public override string ToString()
        {
            return "AntPartEngine";
        }


        internal static Direction TurnLeft(Direction direction)
        {
            if (direction == Direction.N) return Direction.NW;
            if (direction == Direction.NW) return Direction.SW;
            if (direction == Direction.SW) return Direction.S;
            if (direction == Direction.S) return Direction.SE;
            if (direction == Direction.SE) return Direction.NE;
            if (direction == Direction.NE) return Direction.N;
            return Direction.C;
        }
        internal static Direction TurnRight(Direction direction)
        {
            if (direction == Direction.N) return Direction.NE;
            if (direction == Direction.NE) return Direction.SE;
            if (direction == Direction.SE) return Direction.S;
            if (direction == Direction.S) return Direction.SW;
            if (direction == Direction.SW) return Direction.NW;
            if (direction == Direction.NW) return Direction.N;
            return Direction.C;
        }
        /*
        private Direction TurnToPos(ulong pos)
        {
            ulong curPos = Engine.Unit.Pos;

            if (curPos.X == pos.X)
            {
                if (curPos.Y < pos.Y)
                    return Direction.N;
                else
                    return Direction.S;
            }
            else if (curPos.X < pos.X)
            {
                if (curPos.Y > pos.Y)
                    return Direction.NE;
                else
                    return Direction.SE;
            }
            else if (curPos.X > pos.X)
            {
                if (curPos.Y > pos.Y)
                    return Direction.NW;
                else
                    return Direction.SW;
            }
            return Direction.C;
        }*/

        internal static Direction TurnAround(Direction direction)
        {
            if (direction == Direction.N) return Direction.S;
            if (direction == Direction.NE) return Direction.SW;
            if (direction == Direction.SE) return Direction.NW;
            if (direction == Direction.S) return Direction.N;
            if (direction == Direction.SW) return Direction.NE;
            if (direction == Direction.NW) return Direction.SE;
            return Direction.C;
        }

        internal static ulong GetPositionInDirection(ulong pos, Direction direction)
        {
            ulong next = Position.Null;
            if (direction != Direction.C)
            {
                CubePosition cubeulong = new CubePosition(pos);
                CubePosition n = cubeulong.GetNeighbor(direction);
                if (n != null)
                    next = n.Pos;
            }
            return next;
            
        }


        private Tile GetNextPosition(Player player, ulong pos, Direction direction)
        {
            ulong next = GetPositionInDirection(pos, direction);
            
            Tile t = null;
            if (next != Position.Null)
            {
                t = player.Game.Map.GetTile(next);
                if (t != null && !t.CanMoveTo(pos))
                {
                    t = null;
                }
                /*
                else if (ControlAnt.IsOccupied(player, moves, t.Pos))
                {
                    t = null;
                }*/
            }
            return t;
        }

        public void AddDestination(List<AntDestination> possibleTiles, Player player, Tile t, float phem_d, bool onlyIfMovable, PheromoneType pheromoneType)
        {
            if (t != null)
            {
                if (onlyIfMovable && !t.CanMoveTo(Engine.Unit.Pos))
                    return;

                AntDestination antDestination = new AntDestination();
                antDestination.Tile = t;
                antDestination.phem_d = phem_d;
                if (pheromoneType != PheromoneType.None)
                {
                    antDestination.Pheromone = player.Game.Pheromones.FindAt(t.Pos);
                    if (antDestination.Pheromone == null)
                        return;

                    float intensity = antDestination.Pheromone.GetIntensityF(player.PlayerModel.Id, pheromoneType);
                    if (pheromoneType == PheromoneType.AwayFromEnergy || pheromoneType == PheromoneType.AwayFromEnemy)
                    {
                        if (intensity == 1)
                            return;
                    }
                    else
                    {
                        if (intensity == 0)
                            return;
                    }
                    antDestination.Intensity = intensity;
                }
                possibleTiles.Add(antDestination);
            }
        }

        private AntDestination FindBest(Player player, List<AntDestination> possibleTiles) //, PheromoneType pheromoneType)
        {
            AntDestination moveToTile = null;

            float a, b;
            a = 0.5f; b = 0.8f;

            //if (pheromoneType == PheromoneType.ToHome) { a = 0.5f; b = 1.0f; }
            //else if (pheromoneType == PheromoneType.ToFood) { a = 0.5f; b = 0.8f; }
            //else { a = 0.5f; b = 0.8f; }

            float best_str = 0.001f;     // Best pheromone strength

            foreach (AntDestination destination in possibleTiles)
            {
                float phem_d = 0.01f;
                if (destination.Pheromone != null)
                    phem_d = destination.Intensity; //.Pheromone.GetIntensityF(player.PlayerModel.Id, pheromoneType);
                if (phem_d == 0)
                    phem_d = 0.01f;

                float pos_d = destination.phem_d;

                // Formula for overall attractiveness
                // Alpha is pheromone intensity, and Beta is direction factor
                // Should have really wrote it somewhere else than source code
                float trail_str;
                trail_str = (phem_d * a) * (pos_d * b);

                // Compare, is it better than another directions ?
                if (trail_str > best_str)
                {
                    // Add random factor (decision factor) to movements
                    // For example, if ant have choses of two paths of relatively equals pheromone trail strenght
                    // it could follow lesser strenght trail, thus "trying" another path.
                    float a2 = trail_str;
                    float b2 = best_str * 0.2f; // Settings::RANDOM_FACTOR;
                                                //float r = rnd(a2 + b2);

                    int maxrnd = (int)((a2 * 100) + (b2 * 100));
                    double rint = player.Game.Random.Next(maxrnd);
                    double r = rint / 100;

                    if (r < trail_str)
                    {
                        best_str = trail_str;
                        moveToTile = destination;
                    }
                }
            }
            return moveToTile;
        }

        private List<AntDestination> ComputePossibleTiles(Player player, List<Tile> tiles, PheromoneType pheromoneType)
        {
            List<Tile> copyOfTiles = new List<Tile>();
            copyOfTiles.AddRange(tiles);

            List<AntDestination> possibleTiles = new List<AntDestination>();

            while (copyOfTiles.Count > 0)
            {
                int idx = player.Game.Random.Next(copyOfTiles.Count);
                Tile t = copyOfTiles[idx];
                AddDestination(possibleTiles, player, t, 0.6f, true, pheromoneType);
                copyOfTiles.RemoveAt(idx);
            }
            return possibleTiles;
        }

        private List<Tile> MakeForwardTilesList(Player player, Unit cntrlUnit)
        {
            List<Tile> tiles = new List<Tile>();

            Tile tileForward = GetNextPosition(player, cntrlUnit.Pos, cntrlUnit.Direction);
            if (tileForward != null) tiles.Add(tileForward);

            Tile tileLeft = GetNextPosition(player, cntrlUnit.Pos, TurnLeft(cntrlUnit.Direction));
            if (tileLeft != null) tiles.Add(tileLeft);

            Tile tileRight = GetNextPosition(player, cntrlUnit.Pos, TurnRight(cntrlUnit.Direction));
            if (tileRight != null) tiles.Add(tileRight);

            return tiles;
        }


        public bool MoveUnit(ControlAnt control, Player player, List<Move> moves)
        {
            if (Ant.MoveAttempts > 0)
            {

            }
            Unit cntrlUnit = Engine.Unit;

            if (cntrlUnit.Armor != null && cntrlUnit.Armor.ShieldActive == false)
            {
                // Run away?
                //FollowThisRoute = null;
            }

            // Follow trail if possible.
            ulong moveToPosition = Position.Null;
            if (Ant.FollowThisRoute != null)
            {
                if (Ant.FollowThisRoute.Count == 0)
                {
                    Ant.FollowThisRoute = null;
                }
                else
                {
                    moveToPosition = Ant.FollowThisRoute[0];
                    if (control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position.Null;
                        Ant.FollowThisRoute = null;
                    }
                    else
                    {
                        Ant.FollowThisRoute.RemoveAt(0);
                        if (Ant.FollowThisRoute.Count == 0)
                            Ant.FollowThisRoute = null;
                    }
                }
            }

            if (moveToPosition == Position.Null)
            {
                List<Tile> tiles = MakeForwardTilesList(player, cntrlUnit);
                PheromoneType pheromoneType = PheromoneType.AwayFromEnergy;

                if (cntrlUnit.ExtractMe ||
                    cntrlUnit.Power < cntrlUnit.MaxPower - (cntrlUnit.MaxPower / 4))
                {
                    pheromoneType = PheromoneType.Energy;
                }
                bool isWorker = false;

                // Minerals needed?
                if (Ant.AntWorkerType == AntWorkerType.Fighter && cntrlUnit.Weapon != null && !cntrlUnit.Weapon.WeaponLoaded)
                {
                    //pheromoneType = PheromoneType.Mineral;
                    pheromoneType = PheromoneType.Enemy;
                }
                else if (Ant.AntWorkerType == AntWorkerType.Worker)
                {
                    isWorker = true;

                    if (pheromoneType != PheromoneType.Energy && cntrlUnit.Container != null)
                    {
                        if (cntrlUnit.Container.TileContainer.Count == 0)
                        {
                            pheromoneType = PheromoneType.Mineral;
                        }
                        else if (!cntrlUnit.Container.TileContainer.IsFreeSpace)
                        {
                            pheromoneType = PheromoneType.Container;
                        }
                        else
                        {
                            Pheromone pheromone = player.Game.Pheromones.FindAt(cntrlUnit.Pos);
                            if (pheromone == null)
                            {
                                // Find more?
                                pheromoneType = PheromoneType.Mineral;
                            }
                            else
                            {
                                float intensityContainer = pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Container);
                                float intensityMineral = pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Mineral);

                                float loaded = ((float)cntrlUnit.Container.TileContainer.Count / cntrlUnit.Container.TileContainer.Capacity);
                                intensityContainer *= loaded;

                                if (intensityContainer > intensityMineral)
                                {
                                    // More urgent to return stuff
                                    pheromoneType = PheromoneType.Container;
                                }
                                else
                                {
                                    pheromoneType = PheromoneType.Mineral;
                                }
                            }
                        }
                        /*
                        if (cntrlUnit.Container != null && cntrlUnit.Container.IsFreeSpace)
                        {
                            // Fill up with food!
                            pheromoneType = PheromoneType.Mineral;
                        }
                        else
                        {
                            // Look for a target to unload
                            pheromoneType = PheromoneType.Container;
                        }*/
                    }
                }
                else if (Ant.AntWorkerType == AntWorkerType.Fighter)
                {
                    if (pheromoneType != PheromoneType.Energy)
                    {
                        if (cntrlUnit.Armor != null && cntrlUnit.Armor.ShieldActive == false)
                            pheromoneType = PheromoneType.AwayFromEnemy;
                        else
                            pheromoneType = PheromoneType.Enemy;
                    }
                }
                else if (Ant.AntWorkerType == AntWorkerType.Assembler)
                {
                    if (pheromoneType != PheromoneType.Energy)
                    {
                        if (cntrlUnit.Assembler != null && cntrlUnit.Assembler.CanProduce())
                        {
                            pheromoneType = PheromoneType.Work;
                        }
                        else
                        {
                            pheromoneType = PheromoneType.Mineral;
                        }
                    }
                }

                List<AntDestination> possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Mineral)
                {
                    moveToPosition = control.FindMineral(player, Ant);
                    if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position.Null;
                        Ant.FollowThisRoute = null;
                    }
                    if (moveToPosition == Position.Null && cntrlUnit.Container != null && cntrlUnit.Container.TileContainer.Minerals > 0)
                    {
                        // Return the mins
                        pheromoneType = PheromoneType.Container;
                        possibleTiles.Clear();
                    }
                }
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Work)
                {
                    if (cntrlUnit.CurrentGameCommand != null)
                    {
                        moveToPosition = control.FindCommandTarget(player, Ant);

                        if (moveToPosition == Position.Null)
                        {
                            // Cannot reach target
                            Ant.StuckCounter++;
                            if (Ant.StuckCounter > 10)
                                Ant.AbandonUnit(player);
                            return false;
                        }
                    }
                    else
                    {
                        pheromoneType = PheromoneType.Energy;
                    }
                    /*
                    moveToulong = Control.FindWork(player, this);
                    if (moveToulong != null && Control.IsOccupied(player, moves, moveToulong))
                    {
                        moveToulong = null;
                        FollowThisRoute = null;
                    }*/
                }
                if (isWorker && possibleTiles.Count == 0 && pheromoneType == PheromoneType.Container)
                {
                    moveToPosition = control.FindContainer(player, Ant);
                    if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position.Null;
                        Ant.FollowThisRoute = null;
                    }
                }

                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Enemy)
                {
                    //if (PlayerUnit.Unit.CurrentGameCommand != null &&
                    //    PlayerUnit.Unit.CurrentGameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        int movesCount = moves.Count;
                        moveToPosition = control.LevelGround(moves, player, Ant);
                        if (movesCount != moves.Count)
                            return true;
                    }
                    if (moveToPosition == Position.Null)
                    {
                        moveToPosition = control.FindEnemy(player, Ant);
                        if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
                        {
                            moveToPosition = Position.Null;
                            Ant.FollowThisRoute = null;
                        }
                    }
                }
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Energy)
                {
                    moveToPosition = control.FindReactor(player, Ant);
                    if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position.Null;
                        Ant.FollowThisRoute = null;
                    }
                }
                if (moveToPosition == Position.Null)
                {
                    if (Ant.AntWorkerType == AntWorkerType.Fighter && possibleTiles.Count == 0)
                    {
                        // Fighter may try to move to border until food is found
                        pheromoneType = PheromoneType.AwayFromEnergy;
                        possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                    }
                    else if (Ant.AntWorkerType == AntWorkerType.Assembler && possibleTiles.Count == 0)
                    {
                        // Assembler hangs around at home
                        pheromoneType = PheromoneType.Energy;
                        possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                    }
                    else if (isWorker && possibleTiles.Count == 0)
                    {
                        if (cntrlUnit.CurrentGameCommand != null)
                        {
                            // Worker hangs around at command target
                            moveToPosition = control.FindCommandTarget(player, Ant);
                        }
                        else
                        {
                            // Worker hangs around at home
                            pheromoneType = PheromoneType.Energy;
                            possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                        }
                    }

                    AntDestination moveToTile = null;
                    while (possibleTiles.Count > 0 && moveToTile == null && moveToPosition == Position.Null)
                    {
                        moveToTile = FindBest(player, possibleTiles);
                        if (moveToTile == null)
                            break;

                        if (control.IsOccupied(player, moves, moveToTile.Tile.Pos))
                        {
                            possibleTiles.Remove(moveToTile);
                            moveToTile = null;
                        }
                    }

                    if (moveToTile == null && moveToPosition == Position.Null)
                    {
                        if (pheromoneType == PheromoneType.AwayFromEnergy)
                        {
                            // out of reactor range
                            moveToPosition = control.FindReactor(player, Ant);
                            if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
                            {
                                moveToPosition = Position.Null;
                                Ant.FollowThisRoute = null;
                            }
                            else
                            {
                                // Do not follow the route, cause first moveToulong would be skippd. Otherwise, 
                                if (moveToPosition != Position.Null && Ant.FollowThisRoute != null && Ant.FollowThisRoute.Count > 0)
                                {
                                    Ant.FollowThisRoute.Insert(0, moveToPosition);
                                }
                                cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                                return true;
                            }
                        }
                        else if (pheromoneType == PheromoneType.Container)
                        {
                            cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                            return true;
                        }
                        else
                        {
                            pheromoneType = PheromoneType.Container;
                            possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                            if (possibleTiles.Count == 0)
                            {
                                pheromoneType = PheromoneType.AwayFromEnergy;
                                possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                                if (possibleTiles.Count == 0)
                                {
                                    // out of reactor range
                                    moveToPosition = control.FindReactor(player, Ant);
                                    if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
                                    {
                                        Ant.FollowThisRoute = null;
                                        cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                                        return true;
                                    }
                                    else
                                    {
                                        // Do not follow the route, cause first moveToulong would be skippd. Otherwise, 
                                        if (moveToPosition != Position.Null && Ant.FollowThisRoute != null && Ant.FollowThisRoute.Count > 0)
                                        {
                                            Ant.FollowThisRoute.Insert(0, moveToPosition);
                                        }
                                        cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                                        return true;
                                    }
                                }
                            }
                            while (possibleTiles.Count > 0 && moveToTile == null)
                            {
                                moveToTile = FindBest(player, possibleTiles);
                                if (moveToTile == null)
                                    break;

                                if (control.IsOccupied(player, moves, moveToTile.Tile.Pos))
                                {
                                    possibleTiles.Remove(moveToTile);
                                    moveToTile = null;
                                }
                            }
                        }
                    }
                    if (moveToTile != null)
                    {
                        moveToPosition = moveToTile.Tile.Pos;
                    }
                }

                /*
                if (moveToTile != null && StuckCounter > 2)
                {
                    // If it is really occupied
                    if (Control.IsOccupied(player, moves, moveToTile.Tile.Pos))
                    {
                        moveToTile = null;
                    }
                }*/
            }
            if (moveToPosition != Position.Null && control.IsOccupied(player, moves, moveToPosition))
            {
                moveToPosition = Position.Null;
            }

            Move move = null;
            if (moveToPosition != Position.Null)
            {
                bool myPosFound = false;
                Tile t = player.Game.Map.GetTile(moveToPosition);
                foreach (Tile n in t.Neighbors)
                {
                    if (n.Pos == cntrlUnit.Pos)
                    {
                        myPosFound = true;
                        break;
                    }
                }
                if (myPosFound == false)
                {
                    // Should not happen
                    //throw new Exception("not found");
                }
                else
                {
                    move = new Move();
                    move.MoveType = MoveType.Move;
                    move.UnitId = cntrlUnit.UnitId;
                    move.PlayerId = player.PlayerModel.Id;
                    move.Positions = new List<ulong>();
                    move.Positions.Add(cntrlUnit.Pos);
                    move.Positions.Add(moveToPosition);
                    moves.Add(move);
                }
                /*
                if (dropPheromone && !NothingFound)
                {
                    if (pheromoneType == PheromoneType.ToFood)
                        DropPheromone(player, PheromoneType.ToHome);
                    else if (pheromoneType == PheromoneType.ToHome)
                        DropPheromone(player, PheromoneType.ToFood);
                }*/
            }

            return move != null;
        }


        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            Unit cntrlUnit = Engine.Unit;
            bool unitMoved = false;

            if (control.IsBeingExtracted(moves, cntrlUnit.Pos))
            {
                return false;
            }

            if (cntrlUnit.Engine.HoldPosition)
                return false;

            if (cntrlUnit.UnderConstruction)
                return false;

            if (cntrlUnit.CurrentGameCommand != null)
            {
                bool calcPath = true;

                if (cntrlUnit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Collect)
                {
                    if (cntrlUnit.Container != null && cntrlUnit.Container.TileContainer.IsFreeSpace)
                    {
                        // Move to target area
                        int d = CubePosition.Distance(cntrlUnit.CurrentGameCommand.GameCommand.TargetPosition, cntrlUnit.Pos);
                        if (d < player.Game.Map.SectorSize)
                        {
                            calcPath = false;
                            Ant.FollowThisRoute = null;
                        }
                    }
                    else
                    {
                        // Find container
                        calcPath = false;
                        Ant.FollowThisRoute = null;
                    }
                }
                if (cntrlUnit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Attack)
                {
                    if (cntrlUnit.Pos == cntrlUnit.CurrentGameCommand.GameCommand.TargetPosition)
                        return true;
                }
                if (cntrlUnit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (cntrlUnit.Assembler != null && cntrlUnit.Assembler.CanProduce())
                    {
                        Tile t = player.Game.Map.GetTile(cntrlUnit.Pos);
                        foreach (Tile n in t.Neighbors)
                        {
                            if (n.Pos == cntrlUnit.CurrentGameCommand.GameCommand.TargetPosition)
                            {
                                // Next to build target
                                Ant.FollowThisRoute = null;
                                Ant.BuildPositionReached = true;
                                return true;
                            }
                        }
                    }
                }
                
                if (calcPath)
                {
                    if (Ant.FollowThisRoute == null || Ant.FollowThisRoute.Count == 0)
                    {
                        // Compute route to target
                        List<ulong> positions = player.Game.FindPath(cntrlUnit.Pos, cntrlUnit.CurrentGameCommand.GameCommand.TargetPosition, cntrlUnit);
                        if (positions == null && cntrlUnit.CurrentGameCommand.GameCommand.GameCommandType == GameCommandType.Collect)
                        {
                            // Must not be exact
                            Tile t = player.Game.Map.GetTile(cntrlUnit.CurrentGameCommand.GameCommand.TargetPosition);
                            foreach (Tile n in t.Neighbors)
                            {
                                positions = player.Game.FindPath(cntrlUnit.Pos, n.Pos, cntrlUnit);
                                if (positions != null)
                                    break;
                            }
                        }

                        if (positions != null)
                        {
                            Ant.FollowThisRoute = new List<ulong>();
                            for (int i = 1; i < positions.Count; i++)
                            {
                                Ant.FollowThisRoute.Add(positions[i]);
                            }
                        }
                    }
                }
                if (MoveUnit(control, player, moves))
                    unitMoved = true;
            }
            else
            {
                if (MoveUnit(control, player, moves))
                    unitMoved = true;
            }
            
            return unitMoved;
        }
    }
}
