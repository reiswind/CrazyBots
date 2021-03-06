
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
        private Direction TurnToPos(Position2 pos)
        {
            Position2 curPos = Engine.Unit.Pos;

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

        internal static Position2 GetPositionInDirection(Position2 pos, Direction direction)
        {
            Position2 next = Position2.Null;
            if (direction != Direction.C)
            {
                Position3 cubePosition2 = new Position3(pos);
                Position3 n = cubePosition2.GetNeighbor(direction);
                //if (n != null)
                    next = n.Pos;
            }
            return next;
            
        }


        private Tile GetNextPosition(Player player, Position2 pos, Direction direction)
        {
            Position2 next = GetPositionInDirection(pos, direction);
            
            Tile t = null;
            if (next != Position2.Null)
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
            Position2 moveToPosition = Position2.Null;
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
                        moveToPosition = Position2.Null;
                        Ant.FollowThisRoute = null;
                    }
                    else
                    {
                        bool canMoveTo = false;

                        List<Tile> tiles = MakeForwardTilesList(player, cntrlUnit);
                        foreach (Tile tile in tiles)
                        {
                            if (tile.Pos == moveToPosition)
                            {
                                canMoveTo = true;
                                break;
                            }
                        }
                        if (canMoveTo)
                        {
                            Ant.FollowThisRoute.RemoveAt(0);
                            if (Ant.FollowThisRoute.Count == 0)
                                Ant.FollowThisRoute = null;
                        }
                        else
                        {
                            Direction old = cntrlUnit.Direction;
                            Position3 position3 = new Position3(cntrlUnit.Pos);
                            foreach (Position3 n in position3.Neighbors)
                            {
                                if (n.Pos == moveToPosition)
                                {
                                    cntrlUnit.Direction = n.Direction;
                                    break;
                                }
                            }
                            if (old == cntrlUnit.Direction) // Safety
                                cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);

                            Move turnMove = new Move();
                            turnMove.MoveType = MoveType.Move;
                            turnMove.UnitId = cntrlUnit.UnitId;
                            turnMove.PlayerId = player.PlayerModel.Id;
                            turnMove.Positions = new List<Position2>();
                            turnMove.Positions.Add(cntrlUnit.Pos);
                            moves.Add(turnMove);

                            return true;
                        }
                    }
                }
            }

            if (moveToPosition == Position2.Null)
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
                    // Do not go for enemy if extract od low power
                    if (pheromoneType != PheromoneType.Energy)
                    {
                        pheromoneType = PheromoneType.Enemy;
                    }
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
                        if (cntrlUnit.CurrentGameCommand != null &&
                            cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            if (pheromoneType == PheromoneType.Mineral)
                            {
                                // Move around                                
                            }
                            if (pheromoneType == PheromoneType.Container)
                            {
                                FindPathToReturnToTarget(player, cntrlUnit);
                                return true;
                            }
                        }
                        if (cntrlUnit.CurrentGameCommand != null && 
                            cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
                        {
                            if (pheromoneType == PheromoneType.Mineral)
                            {
                                // Controlled by gamecommand
                                if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.MoveToTargetPosition)
                                {
                                    cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;
                                    cntrlUnit.Changed = true;
                                }
                                return true;
                            }
                            if (pheromoneType == PheromoneType.Container)
                            {
                                cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.ReturnToUnload;
                                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("ReturnToUnload", false);
                                cntrlUnit.Changed = true;
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
                    if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position2.Null;
                        Ant.FollowThisRoute = null;
                    }
                    if (moveToPosition == Position2.Null && cntrlUnit.Container != null && cntrlUnit.Container.TileContainer.Minerals > 0)
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

                        if (moveToPosition == Position2.Null)
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
                    moveToPosition2 = Control.FindWork(player, this);
                    if (moveToPosition2 != null && Control.IsOccupied(player, moves, moveToPosition2))
                    {
                        moveToPosition2 = null;
                        FollowThisRoute = null;
                    }*/
                }
                if (isWorker && possibleTiles.Count == 0 && pheromoneType == PheromoneType.Container)
                {
                    moveToPosition = control.FindContainer(player, Ant);
                    if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position2.Null;
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
                    if (moveToPosition == Position2.Null)
                    {
                        moveToPosition = control.FindEnemy(player, Ant);
                        if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
                        {
                            moveToPosition = Position2.Null;
                            Ant.FollowThisRoute = null;
                        }
                    }
                }
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Energy)
                {
                    moveToPosition = control.FindReactor(player, Ant);
                    if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = Position2.Null;
                        Ant.FollowThisRoute = null;
                    }
                }
                if (moveToPosition == Position2.Null)
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
                        if (cntrlUnit.CurrentGameCommand != null &&
                            cntrlUnit.CurrentGameCommand.GameCommandType != GameCommandType.ItemRequest)
                        {
                            // Worker hangs around at command target
                            moveToPosition = control.FindCommandTarget(player, Ant);
                        }
                        else
                        {

                            if (cntrlUnit.Container == null ||
                                cntrlUnit.Container.TileContainer.Count == 0)
                            {
                                // Scout
                                pheromoneType = PheromoneType.AwayFromEnergy;
                            }
                            else
                            {
                                // Worker hangs around at home
                                pheromoneType = PheromoneType.Energy;
                            }
                            possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                        }
                    }

                    AntDestination moveToTile = null;
                    while (possibleTiles.Count > 0 && moveToTile == null && moveToPosition == Position2.Null)
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

                    if (moveToTile == null && moveToPosition == Position2.Null)
                    {
                        if (pheromoneType == PheromoneType.AwayFromEnergy)
                        {
                            // out of reactor range
                            moveToPosition = control.FindReactor(player, Ant);
                            if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
                            {
                                moveToPosition = Position2.Null;
                                Ant.FollowThisRoute = null;
                            }
                            else
                            {
                                // Do not follow the route, cause first moveToPosition2 would be skippd. Otherwise, 
                                if (moveToPosition != Position2.Null && Ant.FollowThisRoute != null && Ant.FollowThisRoute.Count > 0)
                                {
                                    Ant.FollowThisRoute.Insert(0, moveToPosition);
                                }
                                cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);

                                Move turnMove = new Move();
                                turnMove.MoveType = MoveType.Move;
                                turnMove.UnitId = cntrlUnit.UnitId;
                                turnMove.PlayerId = player.PlayerModel.Id;
                                turnMove.Positions = new List<Position2>();
                                turnMove.Positions.Add(cntrlUnit.Pos);
                                moves.Add(turnMove);

                                return true;
                            }
                        }
                        else if (pheromoneType == PheromoneType.Container)
                        {
                            cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);

                            Move turnMove = new Move();
                            turnMove.MoveType = MoveType.Move;
                            turnMove.UnitId = cntrlUnit.UnitId;
                            turnMove.PlayerId = player.PlayerModel.Id;
                            turnMove.Positions = new List<Position2>();
                            turnMove.Positions.Add(cntrlUnit.Pos);
                            moves.Add(turnMove);

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
                                    if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
                                    {
                                        Ant.FollowThisRoute = null;
                                        cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);

                                        Move turnMove = new Move();
                                        turnMove.MoveType = MoveType.Move;
                                        turnMove.UnitId = cntrlUnit.UnitId;
                                        turnMove.PlayerId = player.PlayerModel.Id;
                                        turnMove.Positions = new List<Position2>();
                                        turnMove.Positions.Add(cntrlUnit.Pos);
                                        moves.Add(turnMove);

                                        return true;
                                    }
                                    else
                                    {
                                        // Do not follow the route, cause first moveToPosition2 would be skippd. Otherwise, 
                                        if (moveToPosition != Position2.Null && Ant.FollowThisRoute != null && Ant.FollowThisRoute.Count > 0)
                                        {
                                            Ant.FollowThisRoute.Insert(0, moveToPosition);
                                        }
                                        cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);

                                        Move turnMove = new Move();
                                        turnMove.MoveType = MoveType.Move;
                                        turnMove.UnitId = cntrlUnit.UnitId;
                                        turnMove.PlayerId = player.PlayerModel.Id;
                                        turnMove.Positions = new List<Position2>();
                                        turnMove.Positions.Add(cntrlUnit.Pos);
                                        moves.Add(turnMove);

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
            if (moveToPosition != Position2.Null && control.IsOccupied(player, moves, moveToPosition))
            {
                moveToPosition = Position2.Null;
            }

            Move move = null;
            if (moveToPosition != Position2.Null)
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
                    move.Positions = new List<Position2>();
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

        private bool FindPathToEnemyOrAmmo(Player player, Unit cntrlUnit, Position2 targetUnitPosition)
        {
            if (cntrlUnit.Weapon == null)
            {
                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("NoWeapon");
                cntrlUnit.Changed = true;
                return false;
            }
            Dictionary<Position2, TileWithDistance> tilesInArea = player.Game.Map.EnumerateTiles(targetUnitPosition, cntrlUnit.Weapon.Range, false, matcher: tile =>
            {
                // Do not cheat!
                if (!player.VisiblePositions.ContainsKey(tile.Pos))
                    return false;

                if (cntrlUnit.Weapon.WeaponLoaded)
                {
                    // Look for enemy
                    if (tile.Unit == null || tile.Unit.Owner.PlayerModel.Id == 0 || tile.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                        return false;
                }
                else
                {
                    
                    if (tile.Unit != null)
                    {
                        // Own unit, no ammo source.
                        if (tile.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                            return false;

                        // Enemy Unit or neutral units are possible sources.
                        if (tile.Unit.Owner.PlayerModel.Id != 0)
                        {
                            // If enemy has shield, cannot extract, no source
                            if (tile.Unit.Armor != null && tile.Unit.Armor.ShieldActive)
                                return false;
                        }
                    }

                    // Look for ammo
                    if (tile.Tile.Counter.NumberOfCollectables == 0)
                        return false;
                }
                return true;
            });

            // Create a list 
            Dictionary<int, List<Tile>> sortedPositions = new Dictionary<int, List<Tile>>();
            if (tilesInArea != null)
            {
                foreach (TileWithDistance tileWithDistance in tilesInArea.Values)
                {
                    /*
                    if (cntrlUnit.Pos == tileWithDistance.Pos)
                        continue;

                    if (tileWithDistance.Tile.Unit == null)
                        continue;
                    if (tileWithDistance.Tile.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id)
                        continue;
                    */
                    int d = Position3.Distance(cntrlUnit.Pos, tileWithDistance.Pos);

                    List<Tile> tiles;
                    if (sortedPositions.ContainsKey(d))
                    {
                        tiles = sortedPositions[d];
                    }
                    else
                    {
                        tiles = new List<Tile>();
                        sortedPositions.Add(d, tiles);
                    }
                    tiles.Add(tileWithDistance.Tile);
                }
            }
            foreach (List<Tile> tiles in sortedPositions.Values)
            {
                foreach (Tile tile in tiles)
                {
                    List<Position2> positions = player.Game.FindPath(cntrlUnit.Pos, tile.Pos, cntrlUnit, true);
                    if (positions != null && positions.Count >= 3)
                    {
                        Ant.FollowThisRoute = new List<Position2>();
                        for (int i = 1; i < positions.Count - 1; i++)
                        {
                            Ant.FollowThisRoute.Add(positions[i]);
                        }
                        if (cntrlUnit.Weapon.WeaponLoaded)
                            cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("Attacking");
                        else
                            cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("CollectAmmo");
                        cntrlUnit.Changed = true;
                        return true;
                    }

                }
            }

            if (cntrlUnit.Weapon.WeaponLoaded)
                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("NoEnemyFound");
            else
                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("NoAmmoFound");
            cntrlUnit.Changed = true;
            return false;
        }
        private void FindPathToReturnToTarget(Player player, Unit cntrlUnit)
        {
            if (cntrlUnit.CurrentGameCommand.TargetUnit.UnitId == null)
                return;

            Unit targetUnit = player.Game.Map.Units.FindUnit(cntrlUnit.CurrentGameCommand.TargetUnit.UnitId);
            if (targetUnit != null)
            {
                // Compute route to target
                List<Position2> positions = player.Game.FindPath(cntrlUnit.Pos, targetUnit.Pos, cntrlUnit);
                if (positions == null)
                {
                    // Must not be exact
                    Tile t = player.Game.Map.GetTile(cntrlUnit.CurrentGameCommand.TargetPosition);
                    foreach (Tile n in t.Neighbors)
                    {
                        positions = player.Game.FindPath(cntrlUnit.Pos, n.Pos, cntrlUnit);
                        if (positions != null)
                            break;
                    }
                }

                if (positions != null)
                {
                    Ant.FollowThisRoute = new List<Position2>();
                    for (int i = 1; i < positions.Count; i++)
                    {
                        Ant.FollowThisRoute.Add(positions[i]);
                    }
                }
            }
        }

        private bool FindPathForCollect(Player player, Unit cntrlUnit)
        {
            if (cntrlUnit.CurrentGameCommand == null)
            {
                cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.None;
                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("NoArea");
                cntrlUnit.Changed = true;
                return false;
            }
            if (cntrlUnit.CurrentGameCommand.IncludedPositions != null)
            {
                // Create a list to closest in this area
                Dictionary<int, List<Tile>> sortedPositions = new Dictionary<int, List<Tile>>();

                foreach (TileWithDistance tileWithDistance in cntrlUnit.CurrentGameCommand.IncludedPositions.Values)
                {
                    if (cntrlUnit.Pos == tileWithDistance.Pos)
                        continue;

                    // Collect all
                    if (tileWithDistance.Tile.Counter.NumberOfCollectables == 0)
                        continue;

                    // Is it in powered zone?
                    Pheromone pheromone = player.Game.Pheromones.FindAt(tileWithDistance.Pos);
                    if (pheromone == null || pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Energy) == 0)
                    {
                        // no power
                        //continue;
                    }

                    int d = Position3.Distance(cntrlUnit.Pos, tileWithDistance.Pos);

                    List<Tile> tiles;
                    if (sortedPositions.ContainsKey(d))
                    {
                        tiles = sortedPositions[d];
                    }
                    else
                    {
                        tiles = new List<Tile>();
                        sortedPositions.Add(d, tiles);
                    }
                    tiles.Add(tileWithDistance.Tile);
                }
                foreach (List<Tile> tiles in sortedPositions.Values)
                {
                    foreach (Tile tile in tiles)
                    {
                        List<Position2> positions = player.Game.FindPath(cntrlUnit.Pos, tile.Pos, cntrlUnit, true);
                        if (positions != null && positions.Count >= 3)
                        {
                            Ant.FollowThisRoute = new List<Position2>();
                            for (int i = 1; i < positions.Count - 1; i++)
                            {
                                Ant.FollowThisRoute.Add(positions[i]);
                            }
                            cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;
                            return true;
                        }
                    }
                }
            }
            if (cntrlUnit.Pos != cntrlUnit.CurrentGameCommand.TargetPosition)
            {
                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("OutOfResourcesReturnHome", true);
                cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;

                // Wait at targetposition if empty. If it contains stuff, it can be anywhere trying to return the items.
                // Careful not to move between container and targetpostion
                if (cntrlUnit.Container == null || cntrlUnit.Container.TileContainer.Count == 0)
                {
                    // If empty, wait at collect position target
                    List<Position2> positions = player.Game.FindPath(cntrlUnit.Pos, cntrlUnit.CurrentGameCommand.TargetPosition, cntrlUnit, true);
                    if (positions != null && positions.Count >= 2)
                    {
                        Ant.FollowThisRoute = new List<Position2>();
                        for (int i = 1; i < positions.Count; i++)
                        {
                            Ant.FollowThisRoute.Add(positions[i]);
                        }
                    }
                }
            }
            else
            {
                cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.None;
                cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("OutOfResources", true);
            }
            //cntrlUnit.CurrentGameCommand.CommandComplete = true;
            cntrlUnit.Changed = true;
            return false;
        }

        public override bool Move(ControlAnt control, Player player, List<Move> moves)
        {
            Unit cntrlUnit = Engine.Unit;
            bool unitMoved = false;
            Position2 calcPathToPosition = Position2.Null;

            if (control.IsBeingExtracted(moves, cntrlUnit.Pos))
            {
                return false;
            }

            if (cntrlUnit.Stunned > 0)
                return false;

            if (cntrlUnit.UnderConstruction)
                return false;

            //if (cntrlUnit.Engine.AttackPosition != Position2.Null)

            if (cntrlUnit.CurrentGameCommand != null &&
                cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.AttackMove)
            {
                if (cntrlUnit.Weapon != null && !cntrlUnit.Weapon.WeaponLoaded)
                {
                    // Let the unit move to pick up ammo
                }
                else
                {
                    // Position reached
                    if (cntrlUnit.CurrentGameCommand.TargetPosition == cntrlUnit.Pos)
                    {
                        if (cntrlUnit.CurrentGameCommand.Direction == cntrlUnit.Direction)
                        {
                            return false;
                        }
                        else
                        {
                            // Turn into direction
                            cntrlUnit.Direction = cntrlUnit.CurrentGameCommand.Direction;

                            Move turnMove = new Move();
                            turnMove.MoveType = MoveType.Move;
                            turnMove.UnitId = cntrlUnit.UnitId;
                            turnMove.PlayerId = player.PlayerModel.Id;
                            turnMove.Positions = new List<Position2>();
                            turnMove.Positions.Add(cntrlUnit.Pos);
                            moves.Add(turnMove);

                            return true;
                        }                        
                    }
                    else
                    {
                        calcPathToPosition = cntrlUnit.CurrentGameCommand.TargetPosition;
                    }
                }
            }

            if (calcPathToPosition == Position2.Null &&
                cntrlUnit.CurrentGameCommand != null && !cntrlUnit.CurrentGameCommand.FollowPheromones)
            {
                if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.ItemRequest)
                {
                    if (Ant.Unit.UnitId == cntrlUnit.CurrentGameCommand.AttachedUnit.UnitId)
                    {
                        calcPathToPosition = cntrlUnit.CurrentGameCommand.TargetPosition;
                    }
                    else if (Ant.Unit.UnitId == cntrlUnit.CurrentGameCommand.FactoryUnit.UnitId)
                    {
                        if (Ant.Unit.AreAllIngredientsAvailable(player.Game.RecipeForAnyUnit.Ingredients))
                        {
                            //Ant.Unit.ClearReservations();
                        }
                        else
                        {
                            cntrlUnit.CurrentGameCommand.FactoryUnit.StuckCounter++;
                            if (cntrlUnit.CurrentGameCommand.FactoryUnit.StuckCounter > 10)
                            {
                                cntrlUnit.CurrentGameCommand.FactoryUnit.SetStatus("CantFindResources", true);
                            }
                            else if (cntrlUnit.CurrentGameCommand.FactoryUnit.StuckCounter > 20)
                            {
                                cntrlUnit.ResetGameCommand();
                            }
                            else
                            {
                                cntrlUnit.CurrentGameCommand.FactoryUnit.SetStatus("NeedResources");
                            }
                            cntrlUnit.Changed = true;

                            FindPathForCollect(player, cntrlUnit);
                        }
                    }
                    else if (Ant.Unit.UnitId == cntrlUnit.CurrentGameCommand.TransportUnit.UnitId)
                    {
                        /*
                        if (cntrlUnit.Extractor != null && cntrlUnit.Extractor.CanExtract)
                        {
                            if (cntrlUnit.CurrentGameCommand.AttachedUnit.UnitId == null)
                            {
                                cntrlUnit.CurrentGameCommand.TransportUnit.StuckCounter++;
                                cntrlUnit.CurrentGameCommand.TransportUnit.SetStatus("NoTargetForPickup", cntrlUnit.CurrentGameCommand.TransportUnit.StuckCounter > 5);
                                cntrlUnit.Changed = true;
                            }
                            else
                            {
                                // Need to pick up the requested items. A possible pickup unit is in AttachedUnitId
                                Unit containerUnit = player.Game.Map.Units.FindUnit(cntrlUnit.CurrentGameCommand.AttachedUnit.UnitId);
                                if (containerUnit == null)
                                {
                                    cntrlUnit.CurrentGameCommand.AttachedUnit.ClearUnitId(); // player.Game.Map.Units);
                                    cntrlUnit.CurrentGameCommand.GameCommand.CommandCanceled = true;
                                    cntrlUnit.CurrentGameCommand.TransportUnit.SetStatus("TargetUnitDestroyed");
                                    cntrlUnit.Changed = true;
                                }
                                else
                                {
                                    cntrlUnit.CurrentGameCommand.TransportUnit.SetStatus("MoveToPickupLocation " + containerUnit.Pos.ToString());
                                    cntrlUnit.Changed = true;
                                    calcPathToPosition = containerUnit.Pos;
                                }
                            }
                        }
                        else
                        {
                            // Transporter is full, move to target
                            Unit targetUnit = null;
                            if (cntrlUnit.CurrentGameCommand.TargetUnit.UnitId != null)
                            {
                                targetUnit = player.Game.Map.Units.FindUnit(cntrlUnit.CurrentGameCommand.TargetUnit.UnitId);
                            }                            
                            if (targetUnit == null)
                            {
                                cntrlUnit.CurrentGameCommand.AttachedUnit.ClearUnitId(); // player.Game.Map.Units);
                                cntrlUnit.CurrentGameCommand.GameCommand.CommandCanceled = true;
                                cntrlUnit.CurrentGameCommand.TransportUnit.SetStatus("TargetUnitDestroyed");
                                cntrlUnit.Changed = true;
                            }
                            else
                            {
                                cntrlUnit.CurrentGameCommand.TransportUnit.SetStatus("MoveToTargetLocation " + targetUnit.Pos.ToString());
                                cntrlUnit.Changed = true;

                                calcPathToPosition = targetUnit.Pos;
                            }
                        }*/
                    }
                }
                else if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
                {
                    if (cntrlUnit.CurrentGameCommand.Radius == 0 && cntrlUnit.CurrentGameCommand.TransportUnit.UnitId != null)
                    {
                        // Pick up from targetlocation
                        cntrlUnit.CurrentGameCommand.TransportUnit.SetStatus("TransportUnitMovingToTarget", false);
                        calcPathToPosition = cntrlUnit.CurrentGameCommand.TargetPosition;
                    }
                    else if (cntrlUnit.Container != null && cntrlUnit.Container.TileContainer.IsFreeSpace)
                    {
                        FindPathForCollect(player, cntrlUnit);
                    }
                    else
                    {
                        cntrlUnit.CurrentGameCommand.AttachedUnit.SetStatus("Full");
                        cntrlUnit.Changed = true;
                    }
                }
                else if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Unload)
                {
                    Position2 targetUnitPosition = cntrlUnit.CurrentGameCommand.TargetPosition;
                    Tile t = player.Game.Map.GetTile(targetUnitPosition);
                    //if ((t.Unit != null && t.Unit != cntrlUnit) || !t.CanMoveTo(cntrlUnit.Pos))
                    {
                        // Move next to target
                        foreach (Tile n in t.Neighbors)
                        {
                            if (n.Pos == cntrlUnit.Pos)
                            {
                                targetUnitPosition = n.Pos;
                                break;
                            }
                        }
                    }
                    // Do not move around
                    if (cntrlUnit.Pos == targetUnitPosition)
                    {
                        // Turn into direction
                        if (cntrlUnit.Direction != cntrlUnit.CurrentGameCommand.Direction)
                        {
                            cntrlUnit.Direction = cntrlUnit.CurrentGameCommand.Direction;

                            Move turnMove = new Move();
                            turnMove.MoveType = MoveType.Move;
                            turnMove.UnitId = cntrlUnit.UnitId;
                            turnMove.PlayerId = player.PlayerModel.Id;
                            turnMove.Positions = new List<Position2>();
                            turnMove.Positions.Add(cntrlUnit.Pos);
                            moves.Add(turnMove);

                            if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.MoveToTargetPosition)
                            {
                                cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;
                                cntrlUnit.Changed = true;
                            }
                            return true;
                        }
                        // Command complete, change command type to attack
                        if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.TargetPositionReached)
                        {
                            cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.TargetPositionReached;
                            cntrlUnit.Changed = true;
                        }
                        return true;
                    }
                    if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.MoveToTargetPosition)
                    {
                        cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;
                        cntrlUnit.Changed = true;
                    }
                    calcPathToPosition = targetUnitPosition;
                }
                else if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.AttackMove)
                {
                    Position2 targetUnitPosition = cntrlUnit.CurrentGameCommand.TargetPosition;

                    if (!FindPathToEnemyOrAmmo(player, cntrlUnit, targetUnitPosition))
                    {
                        if (cntrlUnit.Pos == targetUnitPosition)
                        {
                            // Turn into direction
                            if (cntrlUnit.Direction != cntrlUnit.CurrentGameCommand.Direction)
                            {
                                cntrlUnit.Direction = cntrlUnit.CurrentGameCommand.Direction;

                                Move turnMove = new Move();
                                turnMove.MoveType = MoveType.Move;
                                turnMove.UnitId = cntrlUnit.UnitId;
                                turnMove.PlayerId = player.PlayerModel.Id;
                                turnMove.Positions = new List<Position2>();
                                turnMove.Positions.Add(cntrlUnit.Pos);
                                moves.Add(turnMove);
                                return true;
                            }
                            // Command complete, change command type to attack
                            cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.TargetPositionReached;
                            return true;
                        }
                        calcPathToPosition = targetUnitPosition;
                    }
                }
                else if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Fire)
                {
                    // Do not move around
                    return true;
                }
                else if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.HoldPosition)
                {
                    Position2 targetUnitPosition = cntrlUnit.CurrentGameCommand.TargetPosition;
                    Tile t = player.Game.Map.GetTile(targetUnitPosition);
                    if ((t.Unit != null && t.Unit != cntrlUnit) || !t.CanMoveTo(cntrlUnit.Pos))
                    {
                        // Stay on target or one next to it in case the target is a structure
                        foreach (Tile n in t.Neighbors)
                        {
                            if (n.Pos == cntrlUnit.Pos)
                            {
                                cntrlUnit.CurrentGameCommand.TargetPosition = targetUnitPosition = n.Pos;
                                break;
                            }
                        }
                    }
                    // Do not move around
                    if (cntrlUnit.Pos == targetUnitPosition)
                    {
                        // Turn into direction
                        if (cntrlUnit.Direction != cntrlUnit.CurrentGameCommand.Direction)
                        {
                            cntrlUnit.Direction = cntrlUnit.CurrentGameCommand.Direction;

                            Move turnMove = new Move();
                            turnMove.MoveType = MoveType.Move;
                            turnMove.UnitId = cntrlUnit.UnitId;
                            turnMove.PlayerId = player.PlayerModel.Id;
                            turnMove.Positions = new List<Position2>();
                            turnMove.Positions.Add(cntrlUnit.Pos);
                            moves.Add(turnMove);

                            if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.MoveToTargetPosition)
                            {
                                cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;
                                cntrlUnit.Changed = true;
                            }
                            return true;
                        }
                        // Command complete, change command type to attack
                        if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.TargetPositionReached)
                        {
                            cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.TargetPositionReached;
                            cntrlUnit.Changed = true;
                        }
                        return true;
                    }
                    if (cntrlUnit.CurrentGameCommand.GameCommandState != GameCommandState.MoveToTargetPosition)
                    {
                        cntrlUnit.CurrentGameCommand.GameCommandState = GameCommandState.MoveToTargetPosition;
                        cntrlUnit.Changed = true;
                    }
                    calcPathToPosition = targetUnitPosition;
                }

                else if (cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (cntrlUnit.Assembler != null)
                    {
                        cntrlUnit.CurrentGameCommand.FactoryUnit.SetStatus("AssemblerMovingToTarget", false);
                        cntrlUnit.Changed = true;

                        Tile t = player.Game.Map.GetTile(cntrlUnit.Pos);
                        foreach (Tile n in t.Neighbors)
                        {
                            if (n.Pos == cntrlUnit.CurrentGameCommand.TargetPosition)
                            {
                                cntrlUnit.CurrentGameCommand.BuildPositionReached = true;
                                cntrlUnit.CurrentGameCommand.FactoryUnit.SetStatus("BuildPositionReached " + n.Pos.ToString());
                                cntrlUnit.Changed = true;

                                // Next to build target
                                Ant.FollowThisRoute = null;
                                return false;
                            }
                        }
                        calcPathToPosition = cntrlUnit.CurrentGameCommand.TargetPosition;
                    }
                }
            }
            if (calcPathToPosition != Position2.Null)
            {
                if (Ant.FollowThisRoute == null || Ant.FollowThisRoute.Count == 0)
                {
                    // Compute route to target
                    List<Position2> positions = player.Game.FindPath(cntrlUnit.Pos, calcPathToPosition, cntrlUnit);
                    if (positions == null && cntrlUnit.CurrentGameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        // Must not be exact
                        Tile t = player.Game.Map.GetTile(cntrlUnit.CurrentGameCommand.TargetPosition);
                        foreach (Tile n in t.Neighbors)
                        {
                            positions = player.Game.FindPath(cntrlUnit.Pos, n.Pos, cntrlUnit);
                            if (positions != null)
                                break;
                        }
                    }

                    if (positions != null)
                    {
                        Ant.FollowThisRoute = new List<Position2>();
                        for (int i = 1; i < positions.Count; i++)
                        {
                            Ant.FollowThisRoute.Add(positions[i]);
                        }
                    }
                }
            }
            if (MoveUnit(control, player, moves))
                unitMoved = true;
            
            return unitMoved;
        }
    }
}
