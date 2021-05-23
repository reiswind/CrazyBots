using Engine.Algorithms;
using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Control
{


    

    internal class ControlAnt : IControl
    {
        public PlayerModel PlayerModel { get; set; }
        public GameModel GameModel { get; set; }
        private IGameController GameController;

        public Dictionary<string, Ant> Ants = new Dictionary<string, Ant>();

        public int MaxWorker = 25;
        public int NumberOfWorkers;

        public ControlAnt(IGameController gameController, PlayerModel playerModel, GameModel gameModel)
        {
            GameController = gameController;
            PlayerModel = playerModel;
            GameModel = gameModel;
        }
        
        public void ProcessMoves(Player player, List<Move> moves)
        {

        }
        

        public static bool HasMoved(List<Move> moves, Unit unit)
        {
            foreach (Move intendedMove in moves)
            {
                if (intendedMove.MoveType == MoveType.Move ||
                    intendedMove.MoveType == MoveType.Add)
                {
                    if (intendedMove.UnitId == unit.UnitId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool WillBeOccupied(Player player, List<Move> moves, Position destination)
        {
            bool occupied = false;

            // Check if the
            foreach (PlayerUnit currentUnit in player.Units.Values)
            {
                if (currentUnit.Unit.Pos == destination)
                {
                    if (currentUnit.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                        currentUnit.Unit.Engine != null)
                    {
                        // Our own unit, that has engine may move away
                    }
                    else
                    {
                        // cannot move here or did unit move away?
                        occupied = true;
                    }
                    break;
                }
            }
            if (!occupied)
            {
                foreach (Move intendedMove in moves)
                {
                    if (intendedMove.MoveType == MoveType.Move ||
                        intendedMove.MoveType == MoveType.Add)
                    {
                        if (intendedMove.Positions[intendedMove.Positions.Count - 1] == destination)
                        {
                            occupied = true;
                            break;
                        }
                    }
                }
            }
            return occupied;
        }

        public bool IsOccupied(Player player, List<Move> moves, Position destination)
        {
            bool occupied = false;

            // Check if the
            foreach (PlayerUnit currentUnit in player.Units.Values)
            {
                if (currentUnit.Unit.Pos == destination)
                {
                    if (currentUnit.Unit.Owner.PlayerModel.Id == player.PlayerModel.Id &&
                        currentUnit.Unit.Engine != null)
                    {
                        occupied = true;

                        // Our own unit, that has engine may move away
                        foreach (Move intendedMove in moves)
                        {
                            if ( (intendedMove.MoveType == MoveType.Move || intendedMove.MoveType == MoveType.Add) &&
                                currentUnit.Unit.UnitId == intendedMove.UnitId)
                            {
                                if (intendedMove.Positions[0] == destination)
                                {
                                    // Unit moves away
                                    occupied = false;
                                    break;
                                }
                            }
                        }

                    }
                    else
                    {
                        // cannot move here or did unit move away?
                        occupied = true;
                    }
                    break;
                }
            }
            if (!occupied)
            {
                foreach (Move intendedMove in moves)
                {
                    if (intendedMove.MoveType == MoveType.Move ||
                        intendedMove.MoveType == MoveType.Add)
                    {
                        if (intendedMove.Positions[intendedMove.Positions.Count-1] == destination)
                        {
                            occupied = true;
                            break;
                        }
                    }
                }
            }
            return occupied;
        }

        private static int moveNr;

        public List<Move> Turn(Player player)
        {
            moveNr++;
            if (moveNr > 17)
            {

            }

            Pheromones.Evaporate();

            // Returned moves
            List<Move> moves = new List<Move>();

            // List of all units that can be moved
            List<PlayerUnit> moveableUnits = new List<PlayerUnit>();

            if (player.PlayerModel.ControlLevel != 0 && player.WonThisGame())
            {
                // Clean up?
                //player.Commands.Clear();

                // Add Clean area command
                //player.Commands.Add();
            }

            foreach (Ant ant in Ants.Values)
            {
                ant.MoveAttempts = 0;
                ant.Alive = false;
            }
            foreach (PlayerUnit playerUnit in player.Units.Values)
            {
                Unit cntrlUnit = playerUnit.Unit;
                if (cntrlUnit.Owner.PlayerModel.Id == PlayerModel.Id)
                {
                    playerUnit.PossibleMoves.Clear();
                    moveableUnits.Add(playerUnit);

                    if (Ants.ContainsKey(cntrlUnit.UnitId))
                    {
                        Ant ant = Ants[cntrlUnit.UnitId];
                        ant.Alive = true;
                    }
                    else
                    {
                        if (playerUnit.Unit.Assembler != null)
                        {
                            AntFactory antFactory = new AntFactory(this, playerUnit);
                            antFactory.Alive = true;
                            Ants.Add(cntrlUnit.UnitId, antFactory);

                            Pheromones.DropStaticPheromones(player, cntrlUnit.Pos, 20, PheromoneType.ToHome);
                        }

                    }
                }
            }

            NumberOfWorkers = 0;

            List<Ant> movableAnts = new List<Ant>();
            List<Ant> killedAnts = new List<Ant>();
            List<Ant> unmovedAnts = new List<Ant>();

            foreach (Ant ant in Ants.Values)
            {
                if (!ant.Alive)
                {
                    killedAnts.Add(ant);
                }
                else
                {
                    if (ant.PlayerUnit.Unit.IsComplete())
                    {
                        if (ant is AntWorker)
                            NumberOfWorkers++;

                        movableAnts.Add(ant);
                    }
                    else if (ant.PlayerUnit.Unit.UnderConstruction)
                    {
                        if (ant.PlayerUnit.Unit.Engine != null)
                            NumberOfWorkers++;
                    }
                    else
                    {
                        ant.PlayerUnit.Unit.ExtractMe = true;   
                    }
                }
            }
            unmovedAnts.AddRange(movableAnts);

            foreach (Ant ant in unmovedAnts)
            {
                if (ant is AntFactory)
                {
                    ant.Move(player, moves);
                    movableAnts.Remove(ant);
                }
            }

            unmovedAnts.Clear();
            unmovedAnts.AddRange(movableAnts);
            while (unmovedAnts.Count > 0)
            {
                foreach (Ant ant in unmovedAnts)
                {
                    if (!(ant is AntFactory))
                    {
                        if (ant.Move(player, moves))
                        {
                            ant.StuckCounter = 0;
                            movableAnts.Remove(ant);
                        }
                        else
                        {
                            ant.StuckCounter++;
                            if (ant.MoveAttempts == 0)
                            {
                                ant.MoveAttempts++;
                            }
                            else
                            {
                                movableAnts.Remove(ant);
                            }
                        }
                    }
                }
                unmovedAnts.Clear();
                if (movableAnts.Count > 0)
                {
                    unmovedAnts.AddRange(movableAnts);
                }
            }

            foreach (Ant ant in killedAnts)
            {
                Ants.Remove(ant.PlayerUnit.Unit.UnitId);
            }            
            return moves;
        }
    }
}
