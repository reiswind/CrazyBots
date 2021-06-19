﻿using Engine.Control;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Engine.Ants
{
    internal enum AntWorkerType
    {
        None,
        Worker,
        Fighter,
        Assembler
    }
    internal class AntWorker : Ant
    {
        public AntWorkerType AntWorkerType { get; set; }
        public bool ReturnHome { get; set; }
        public bool NothingFound { get; set; }
        public bool GotLostNoWayHome { get; set; }

        public AntWorker(ControlAnt control) : base(control)
        {

        }

        private Direction TurnLeft(Direction direction)
        {
            if (direction == Direction.N) return Direction.NW;
            if (direction == Direction.NW) return Direction.SW;
            if (direction == Direction.SW) return Direction.S;
            if (direction == Direction.S) return Direction.SE;
            if (direction == Direction.SE) return Direction.NE;
            if (direction == Direction.NE) return Direction.N;
            return Direction.C;
        }
        private Direction TurnRight(Direction direction)
        {
            if (direction == Direction.N) return Direction.NE;
            if (direction == Direction.NE) return Direction.SE;
            if (direction == Direction.SE) return Direction.S;
            if (direction == Direction.S) return Direction.SW;
            if (direction == Direction.SW) return Direction.NW;
            if (direction == Direction.NW) return Direction.N;
            return Direction.C;
        }

        private Direction TurnToPos(Position pos)
        {
            Position curPos = PlayerUnit.Unit.Pos;

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
        }

        private Direction TurnAround(Direction direction)
        {
            if (direction == Direction.N) return Direction.S;
            if (direction == Direction.NE) return Direction.SW;
            if (direction == Direction.SE) return Direction.NW;
            if (direction == Direction.S) return Direction.N;
            if (direction == Direction.SW) return Direction.NE;
            if (direction == Direction.NW) return Direction.SE;
            return Direction.C;
        }

        private Tile GetNextPosition(Player player, Position pos, Direction direction)
        {
            Position next = null;
            if (direction == Direction.N)
            {
                if (pos.Y > 0)
                    next = new Position(pos.X, pos.Y - 1);
            }
            else if (direction == Direction.S)
            {
                if (pos.Y < 100)
                    next = new Position(pos.X, pos.Y + 1);
            }
            else if (direction == Direction.NE)
            {
                if (pos.X % 2 != 0)
                {
                    if (pos.X < 100)
                        next = new Position(pos.X + 1, pos.Y);
                }
                else
                {
                    if (pos.X < 100 && pos.Y > 0)
                        next = new Position(pos.X + 1, pos.Y - 1);
                }
            }
            else if (direction == Direction.SE)
            {
                if (pos.X % 2 == 0)
                {
                    if (pos.X < 100)
                        next = new Position(pos.X + 1, pos.Y);
                }
                else
                {
                    if (pos.X < 100 && pos.Y < 100)
                        next = new Position(pos.X + 1, pos.Y + 1);
                }
            }
            else if (direction == Direction.NW)
            {
                if (pos.X % 2 != 0)
                {
                    if (pos.X > 0)
                        next = new Position(pos.X - 1, pos.Y);
                }
                else
                {
                    if (pos.X > 0 && pos.Y > 0)
                        next = new Position(pos.X - 1, pos.Y - 1);
                }
            }
            else if (direction == Direction.SW)
            {
                if (pos.X % 2 == 0)
                {
                    if (pos.X > 0)
                        next = new Position(pos.X - 1, pos.Y);
                }
                else
                {
                    if (pos.X > 0 && pos.Y < 100)
                        next = new Position(pos.X - 1, pos.Y + 1);
                }
            }

            Tile t = null;
            if (next != null)
            {
                t = player.Game.Map.GetTile(next);
                if (t != null && !t.CanMoveTo())
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

        public bool GoHome(Player player, List<Move> moves, bool dropPheromone)
        {
            List<Move> possibleMove = new List<Move>();

            int maxSearchHome = 3;

            Unit cntrlUnit = PlayerUnit.Unit;
            foreach (Ant ant in Control.Ants.Values)
            {
                AntFactory antFactory = ant as AntFactory;
                AntContainer antContainer = ant as AntContainer;
                if (antFactory != null || antContainer != null)
                {
                    if (ant.PlayerUnit.Unit.Container != null &&
                        ant.PlayerUnit.Unit.Container.Metal < ant.PlayerUnit.Unit.Container.Capacity)
                    {
                        Move move = player.Game.MoveTo(cntrlUnit.Pos, ant.PlayerUnit.Unit.Pos, cntrlUnit.Engine);
                        if (move != null)
                        {
                            possibleMove.Add(move);
                        }
                    }
                    if (maxSearchHome-- == 0)
                        break;
                }
            }

            foreach (Move move in possibleMove)
            {
                Tile t = player.Game.Map.GetTile(move.Positions[1]);
                if (CheckHandover(t.Pos, moves))
                    return true;

                if (!Control.IsOccupied(player, moves, t.Pos))
                {
                    moves.Add(move);
                    if (dropPheromone && !NothingFound)
                        DropPheromone(player, PheromoneType.Mineral);
                    return true;
                }
            }
            
            return false;
        }


        public void AddDestination(List<AntDestination> possibleTiles, Player player, Tile t, float phem_d, bool onlyIfMovable, PheromoneType pheromoneType)
        {
            if (t != null)
            {
                if (onlyIfMovable && !t.CanMoveTo())
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
                    if (pheromoneType == PheromoneType.AwayFromEnergy)
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

        private bool CheckHandover(Position pos, List<Move> moves)
        {
            return false;
            /*
            if (ReturnHome && PlayerUnit.Unit.Container.Metal >= PlayerUnit.Unit.Container.Capacity)
            {
                // Check Ant at target position
                foreach (Ant ant in Control.Ants.Values)
                {
                    if (ant.PlayerUnit.Unit.Pos == pos)
                    {
                        AntWorker antWorker = ant as AntWorker;
                        if (antWorker != null)
                        {
                            if (antWorker.PlayerUnit.Unit.Container != null &&
                                PlayerUnit.Unit.Container != null &&
                                antWorker.PlayerUnit.Unit.Container.Metal < PlayerUnit.Unit.Container.Metal)
                            {
                                bool otherUnitMoved = false;
                                foreach (Move plannedMove in moves)
                                {
                                    if (plannedMove.MoveType == MoveType.Move && plannedMove.UnitId == antWorker.PlayerUnit.Unit.UnitId)
                                    {
                                        otherUnitMoved = true;
                                    }
                                }
                                if (!otherUnitMoved)
                                {
                                    // Handover the food
                                    antWorker.ReturnHome = true;
                                    antWorker.NothingFound = false;
                                    antWorker.PlayerUnit.Unit.Container.Metal += PlayerUnit.Unit.Container.Metal;
                                    antWorker.PlayerUnit.Unit.Direction = TurnAround(antWorker.PlayerUnit.Unit.Direction);
                                    antWorker.FoodIntensity = FoodIntensity;

                                    ReturnHome = false;
                                    NothingFound = false;
                                    PlayerUnit.Unit.Container.Metal = 0;
                                    PlayerUnit.Unit.Direction = TurnAround(PlayerUnit.Unit.Direction);
                                    FoodIntensity = 0;

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
            */
        }

        private int SmellFoodCooldown;

        private bool SmellFood(Player player)
        {
            if (SmellFoodCooldown > 0)
            {
                SmellFoodCooldown--;
                return false;
            }

            

            List<AntDestination> possibleTiles = new List<AntDestination>();

            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(PlayerUnit.Unit.Pos, 3, false, matcher: tile =>
            {
                Pheromone pheroHere = player.Game.Pheromones.FindAt(tile.Pos);
                if (tile.Metal > 0)
                {
                    AddDestination(possibleTiles, player, tile.Tile, 0.6f, false, PheromoneType.None);
                    return true;
                }
                else if (pheroHere != null)
                {
                    AddDestination(possibleTiles, player, tile.Tile, 0.2f, false, PheromoneType.Mineral);
                    return true;
                }
                return false;
            });

            AntDestination bestMetal = null;
            AntDestination bestPheromone = null;

            foreach (AntDestination antDestination in possibleTiles)
            {
                if (antDestination.Tile.Metal > 0 && (bestMetal == null || bestMetal.Tile.Metal > antDestination.Tile.Metal))
                {
                    bestMetal = antDestination;
                }
                if (antDestination.Pheromone != null && 
                    (bestPheromone == null || 
                    antDestination.Pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Mineral) > bestPheromone.Pheromone.GetIntensityF(player.PlayerModel.Id, PheromoneType.Mineral)))
                {
                    bestPheromone = antDestination;
                }
            }
            // Turn ant to food
            if (bestMetal != null)
            {
                Direction d = TurnToPos(bestMetal.Tile.Pos);
                if (d != Direction.C && d != PlayerUnit.Unit.Direction)
                {
                    SmellFoodCooldown = 1;
                    PlayerUnit.Unit.Direction = d;
                    return true;
                }
            }
            else if (bestPheromone != null)
            {
                
                Direction d = TurnToPos(bestPheromone.Tile.Pos);
                if (d != Direction.C && d != PlayerUnit.Unit.Direction)
                {
                    SmellFoodCooldown = 1;
                    PlayerUnit.Unit.Direction = d;
                    return true;
                }
            }

            /* Move there leads to locks
            foreach (AntDestination antDestination in possibleTiles)
            {
                Unit cntrlUnit = PlayerUnit.Unit;

                Move move = player.Game.MoveTo(cntrlUnit.Pos, antDestination.Tile.Pos, cntrlUnit.Engine);
                if (move != null)
                {
                    if (!Control.IsOccupied(player, moves, move.Positions[1]))
                    {
                        moves.Add(move);
                        DropPheromone(player, PheromoneType.ToHome);
                        return true;
                    }
                }
            }
            */
            SmellFoodCooldown = 0;
            return false;
        }

        private bool IsCloseToFactory(Player player, int range)
        {
            Dictionary<Position, TileWithDistance> tiles = player.Game.Map.EnumerateTiles(PlayerUnit.Unit.Pos, range, false, matcher: tile =>
            {
                if (tile.Unit != null && tile.Unit.Assembler != null)
                    return true;
                return false;
            });
            if (tiles.Count > 0)
                return true;

            return false;
        }

        private AntDestination FindBest(Player player, List<AntDestination> possibleTiles) //, PheromoneType pheromoneType)
        {
            AntDestination moveToTile = null;

            float a, b;
            a = 0.5f; b = 0.8f;

            //if (pheromoneType == PheromoneType.ToHome) { a = 0.5f; b = 1.0f; }
            //else if (pheromoneType == PheromoneType.ToFood) { a = 0.5f; b = 0.8f; }
            //else { a = 0.5f; b = 0.8f; }

            float best_str = 0.01f;     // Best pheromone strength

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


        public bool MoveUnit(Player player, List<Move> moves)
        {
            if (MoveAttempts > 0)
            {

            }
            Unit cntrlUnit = PlayerUnit.Unit;

            // Follow trail if possible.
            Position moveToPosition = null;
            if (FollowThisRoute != null)
            {
                if (FollowThisRoute.Count == 0)
                {
                    FollowThisRoute = null;
                }
                else
                {
                    moveToPosition = FollowThisRoute[0];
                    if (Control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = null;
                        FollowThisRoute = null;
                    }
                    else
                    {
                        FollowThisRoute.RemoveAt(0);
                        if (FollowThisRoute.Count == 0)
                            FollowThisRoute = null;
                    }
                }
            }

            if (moveToPosition == null)
            {
                List<Tile> tiles = MakeForwardTilesList(player, cntrlUnit);
                PheromoneType pheromoneType = PheromoneType.AwayFromEnergy;

                // Minerals needed?
                if (cntrlUnit.Weapon != null && !cntrlUnit.Weapon.WeaponLoaded)
                {
                    pheromoneType = PheromoneType.Mineral;
                }
                else if (AntWorkerType == AntWorkerType.Worker)
                {
                    if (cntrlUnit.Container != null && cntrlUnit.Container.Metal < cntrlUnit.Container.Capacity)
                    {
                        // Fill up with food!
                        pheromoneType = PheromoneType.Mineral;
                    }
                    else
                    {
                        // Look for a target to unload
                        pheromoneType = PheromoneType.Container;
                    }
                }
                else if (AntWorkerType == AntWorkerType.Fighter)
                {
                    pheromoneType = PheromoneType.Enemy;
                }
                else if (AntWorkerType == AntWorkerType.Assembler)
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

                List<AntDestination> possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Mineral)
                {
                    moveToPosition = Control.FindMineral(player, this);
                    if (moveToPosition != null && Control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = null;
                        FollowThisRoute = null;
                    }
                }
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Work)
                {
                    moveToPosition = Control.FindWork(player, this);
                    if (moveToPosition != null && Control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = null;
                        FollowThisRoute = null;
                    }
                }
                if (AntWorkerType == AntWorkerType.Worker && possibleTiles.Count == 0 && pheromoneType == PheromoneType.Container)
                {
                    moveToPosition = Control.FindContainer(player, this);
                    if (moveToPosition != null && Control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = null;
                        FollowThisRoute = null;
                    }
                }
                if (possibleTiles.Count == 0 && pheromoneType == PheromoneType.Enemy)
                {
                    moveToPosition = Control.FindEnemy(player, this);
                    if (moveToPosition != null && Control.IsOccupied(player, moves, moveToPosition))
                    {
                        moveToPosition = null;
                        FollowThisRoute = null;
                    }
                }
                if (moveToPosition == null)
                {
                    if (AntWorkerType == AntWorkerType.Fighter && possibleTiles.Count == 0)
                    {
                        // Fighter may try to move to border until food is found
                        pheromoneType = PheromoneType.AwayFromEnergy;
                        possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                    }
                    else if (AntWorkerType == AntWorkerType.Assembler && possibleTiles.Count == 0)
                    {
                        // Assembler hangs around at home
                        pheromoneType = PheromoneType.Energy;
                        possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                    }
                    else if (AntWorkerType == AntWorkerType.Worker && possibleTiles.Count == 0)
                    {
                        // Worker hangs around at home
                        pheromoneType = PheromoneType.Energy;
                        possibleTiles = ComputePossibleTiles(player, tiles, pheromoneType);
                    }

                    AntDestination moveToTile = null;
                    while (possibleTiles.Count > 0 && moveToTile == null)
                    {
                        moveToTile = FindBest(player, possibleTiles);
                        if (moveToTile == null)
                            break;

                        if (Control.IsOccupied(player, moves, moveToTile.Tile.Pos))
                        {
                            possibleTiles.Remove(moveToTile);
                            moveToTile = null;
                        }
                    }

                    if (moveToTile == null)
                    {
                        if (pheromoneType == PheromoneType.AwayFromEnergy)
                        {
                            // out of reactor range
                            moveToPosition = Control.FindReactor(player, this);
                            if (moveToPosition != null && Control.IsOccupied(player, moves, moveToPosition))
                            {
                                moveToPosition = null;
                                FollowThisRoute = null;
                            }
                            else
                            {
                                // Do not follow the route, cause first moveToPosition would be skippd. Otherwise, 
                                if (moveToPosition != null && FollowThisRoute != null && FollowThisRoute.Count > 0)
                                {
                                    FollowThisRoute.Insert(0, moveToPosition);
                                }
                                cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                                return true;
                            }
                        }
                        // cannot reach anything cause to crowded?
                        //if (pheromoneType == PheromoneType.ToFood)
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
                                    cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                                    return true;
                                }
                            }
                            while (possibleTiles.Count > 0 && moveToTile == null)
                            {
                                moveToTile = FindBest(player, possibleTiles);
                                if (moveToTile == null)
                                    break;

                                if (Control.IsOccupied(player, moves, moveToTile.Tile.Pos))
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
            if (moveToPosition != null && Control.IsOccupied(player, moves, moveToPosition))
            {
                moveToPosition = null;
            }

            Move move = null;
            if (moveToPosition != null)
            {
                bool myPosFound = false;
                Tile t = player.Game.Map.GetTile(moveToPosition);
                foreach (Tile n  in t.Neighbors)
                { 
                    if (n.Pos == cntrlUnit.Pos)
                    {
                        myPosFound = true;
                        break;
                    }
                }
                if (myPosFound == false)
                {
                    int x = 0;
                }

                move = new Move();
                move.MoveType = MoveType.Move;
                move.UnitId = cntrlUnit.UnitId;
                move.PlayerId = player.PlayerModel.Id;
                move.Positions = new List<Position>();
                move.Positions.Add(cntrlUnit.Pos);
                move.Positions.Add(moveToPosition);
                moves.Add(move);

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

        /*
         
         // Home Trail
        float Settings::HOME_TRAIL_DEPOSIT_RATE = 0.5f;
        float Settings::HOME_TRAIL_FORGET_RATE = 0.01f;
        float Settings::HOME_ALPHA = 0.5f;
        float Settings::HOME_BETA = 0.8f;

        // Food Trail
        float Settings::FOOD_TRAIL_DEPOSIT_RATE = 0.2f;
        float Settings::FOOD_TRAIL_FORGET_RATE = 0.05f;
        float Settings::FOOD_ALPHA = 0.5f;
        float Settings::FOOD_BETA = 1.0f;

           void PheromoneAnt::DepositPheromone()
            {
	            float foodStorage = this->GetFoodStorage();
	            float energy = this->GetEnergy();

	            // If ant is carrying food it leaves food trails, otherwise leaves home trail
	            if (foodStorage > 0)
	            {
		            // Leave food pheromobe trail
		            m_grid->DepositPheromone(m_position, PheromoneType::food, (GetFoodStorage() / Settings::MAX_FOOD_STORAGE) * Settings::FOOD_TRAIL_DEPOSIT_RATE);
	            }
	            else
	            {
		            // Leave home pheromone trail
		            m_grid->DepositPheromone(m_position, PheromoneType::home, (GetEnergy() / Settings::MAX_ENERGY) * Settings::HOME_TRAIL_DEPOSIT_RATE);
	            }
            }*/

        private void DropPheromone(Player player, PheromoneType pheromoneType)
        {
            // For now
            //if (player.PlayerModel.Id == 9999)
            {
                if (pheromoneType == PheromoneType.Container)
                {
                    Energy -= 3;
                    if (Energy < 0)
                    {
                        Energy = 0;
                        return;
                    }
                }
                else
                {
                    if (GotLostNoWayHome)
                    {
                        // Do not drop phromone to food if not on the way home
                        return;
                    }
                    FoodIntensity -= 5;
                    if (FoodIntensity < 0)
                    {
                        FoodIntensity = 0;
                        //return;
                    }
                }
                Unit cntrlUnit = PlayerUnit.Unit;
                /*
                if (pheromoneType == PheromoneType.Food && StepsFromHome >= Pheromone.MaxStepsFromHome)
                    return;

                if (pheromoneType == PheromoneType.Home && StepsFromFood >= Pheromone.MaxStepsFromFood)
                    return;
                */
                /*
                Pheromone pheromone = Pheromones.FindAt(cntrlUnit.Pos);
                if (pheromone == null)
                {
                    pheromone = new Pheromone();
                    pheromone.Pos = cntrlUnit.Pos;
                    Pheromones.Add(pheromone);
                }*/
                float intensity;
                switch (pheromoneType)
                {
                    case PheromoneType.Container:
                        intensity = (Energy / MaxEnergy) * HomeTrailDepositRate;
                        //pheromone.Deposit(intensity, pheromoneType, false);
                        break;

                    case PheromoneType.Mineral:
                        intensity = 0.5f; // (FoodIntensity / MaxFoodIntensity) * FoodTrailDepositRate;
                        player.Game.Pheromones.Deposit(player, cntrlUnit.Pos, pheromoneType, intensity, false);
                        break;
                }
            }
        }

        public override bool Move(Player player, List<Move> moves)
        {
            Unit cntrlUnit = PlayerUnit.Unit;
            bool unitMoved = false;

            /*
            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Extract)
                {
                    if (intendedMove.Positions[intendedMove.Positions.Count - 1] == cntrlUnit.Pos)
                    {
                        // Unit should not move until empty
                        return true;
                    }
                }
            }*/

            /*
            if (!ReturnHome && Energy == 0)
            {
                NothingFound = true;
                ReturnHome = true;

                cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
            }
            */

            // Check if reached
            /*
            if (ReturnHome)
            {
                if (IsCloseToFactory(player, 3))
                {
                    if (NothingFound)
                    {
                        // Returned home without anything, start search again
                        GotLostNoWayHome = false;
                        Energy = MaxEnergy;
                        FoodIntensity = 0;
                        NothingFound = false;
                        ReturnHome = false;
                    }
                    else
                    {
                        if (cntrlUnit.Container != null)
                        {
                            if (cntrlUnit.Container.Metal == 0)
                            {
                                // Unit has been emptied, start search again (in opposite direction)
                                cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                                FoodIntensity = 0;
                                NothingFound = false;
                                GotLostNoWayHome = false;
                                Energy = MaxEnergy;
                                ReturnHome = false;
                            }
                            else
                            {
                                /*
                                if (IsCloseToFactory(player, 1))
                                {
                                    // Wait for clear
                                }
                                else
                                {
                                    // Go strait home, ignore pheromones
                                    //unitMoved = GoHome(player, moves, true);
                                    //if (unitMoved)
                                    //    return unitMoved;
                                } * /
                            }
                        }
                    }
                }
            }*/

            /*
            if (ReturnHome)
            {
                //GoHome(player, moves);
                //return;
            }
            else
            {
                if (cntrlUnit.Container != null && cntrlUnit.Container.Metal >= cntrlUnit.Container.Capacity)
                {
                    // Ant full, return home
                    FoodIntensity = MaxFoodIntensity;
                    Energy = MaxEnergy;
                    ReturnHome = true;
                    cntrlUnit.Direction = TurnAround(cntrlUnit.Direction);
                }
            }
            */

            if (cntrlUnit.Weapon != null)
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Weapon.ComputePossibleMoves(possiblemoves, null, MoveFilter.Fire);
                if (possiblemoves.Count > 0)
                {
                    int idx = player.Game.Random.Next(possiblemoves.Count);
                    if (cntrlUnit.Engine == null)
                    {
                        // Do not fire at trees
                        while (possiblemoves.Count > 0 && possiblemoves[idx].OtherUnitId == "Destructable")
                        {
                            possiblemoves.RemoveAt(idx);
                            idx = player.Game.Random.Next(possiblemoves.Count);
                        }
                    }
                    if (possiblemoves.Count > 0)
                    {
                        moves.Add(possiblemoves[idx]);
                        unitMoved = true;
                        return unitMoved;
                    }
                }
            }
            if (AntWorkerType == AntWorkerType.Assembler && cntrlUnit.Assembler != null && cntrlUnit.Assembler.CanProduce())
            {
                List<Move> possiblemoves = new List<Move>();
                cntrlUnit.Assembler.ComputePossibleMoves(possiblemoves, null, MoveFilter.Upgrade);
                if (possiblemoves.Count > 0)
                {
                    int idx = player.Game.Random.Next(possiblemoves.Count);
                    Move move = possiblemoves[idx];
                    moves.Add(move);

                    unitMoved = true;
                    return unitMoved;
                }
            }

            if (AntWorkerType == AntWorkerType.Fighter && cntrlUnit.Weapon != null && cntrlUnit.Weapon.WeaponLoaded)
            {
                // Fight, do not extract if can fire
            }
            else
            {
                if (cntrlUnit.Extractor != null && cntrlUnit.Extractor.CanExtract)
                {
                    List<Move> possiblemoves = new List<Move>();
                    cntrlUnit.Extractor.ComputePossibleMoves(possiblemoves, null, MoveFilter.Extract);
                    if (possiblemoves.Count > 0)
                    {
                        int idx = player.Game.Random.Next(possiblemoves.Count);
                        Move move = possiblemoves[idx];
                        moves.Add(move);

                        Control.MineralsFound(player, move.Positions[1], false);

                        unitMoved = true;
                        return unitMoved;
                    }
                }
            }
            if (cntrlUnit.Engine != null && cntrlUnit.UnderConstruction == false)
            {
                if (MoveUnit(player, moves))
                    unitMoved = true;
                
            }
            return unitMoved;
        }
    }

}
