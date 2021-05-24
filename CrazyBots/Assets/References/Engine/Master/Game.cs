﻿using Engine.Algorithms;
using Engine.Control;
using Engine.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    [DataContract]
    internal class UnitCollision
    {
        [DataMember]
        public Unit Unit1 { get; set; }
        [DataMember]
        public Move Move1 { get; set; }
        [DataMember]
        public Unit Unit2 { get; set; }
        [DataMember]
        public Move Move2 { get; set; }
    }

    public class Game : IGameController
    {
        private static string logFile = null; // = @"c:\Temp\moves.json";
        public GameModel GameModel { get; private set; }

        public Map Map { get; private set; }

        
        private void StartWithFactory(List<Move> newMoves)
        {

            foreach (Player player in Players.Values)
            {
                //Move move;

                // Factory
                string newUnitId = GetNextUnitId("unit");

                if (player.PlayerModel.ControlLevel == 1)
                {
                    /*
                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = newUnitId + ":StartColony";
                    move.Positions = new List<Position>();
                    move.Positions.Add(player.PlayerModel.StartPosition);
                    newMoves.Add(move);*/
                }
                else
                {
                    /*
                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = newUnitId + ":StartFactory";
                    move.Positions = new List<Position>();
                    move.Positions.Add(player.PlayerModel.StartPosition);
                    newMoves.Add(move);

                    Assemble assemble = new Assemble();
                    assemble.Center = player.PlayerModel.StartPosition;
                    assemble.PlayerId = player.PlayerModel.Id;
                    assemble.Map = Map;
                    assemble.AssignUnit(newUnitId);
                    player.Commands.Add(assemble);
                    */
                    // Human player must set commands by himself

                    if (player.PlayerModel.ControlLevel != 0)
                    {
                        /*
                        Collect collect = new Collect();
                        collect.Center = player.PlayerModel.StartPosition;
                        collect.PlayerId = player.PlayerModel.Id;
                        collect.Range = 5;
                        collect.Map = Map;

                        CommandSource commandSource = new CommandSource();
                        commandSource.Child = collect;
                        commandSource.Parent = assemble;
                        collect.CommandSources.Add(commandSource);

                        player.Commands.Add(collect);
                        */
                    }
                }
                //break;
            }
            foreach (UnitModel unitModel in GameModel.Units)
            {
                Move move = new Move();
                move.MoveType = MoveType.Add;
                move.PlayerId = unitModel.PlayerId;
                move.UnitId = unitModel.Parts; // newUnitId + ":StartColony";
                move.Positions = new List<Position>();
                move.Positions.Add(unitModel.Position);
                newMoves.Add(move);

                // Turn into direction missing
            }
        }

        private int unitCntr;
        public string GetNextUnitId(string unitModelName)
        {
            string unitId = unitModelName + unitCntr.ToString();
            unitCntr++;
            return unitId;
        }

        private int seed;
        public Random Random { get; private set; }
        public int Seed
        {
            get
            {
                return seed;
            }
        }

        public Game(GameModel gameModel)
        {
            int tcks = (int)DateTime.Now.Ticks;
            //tcks = -867792232;
            //tcks = -2073632798;
            //tcks = 506882786; // grass water
            //tcks = 1588186970; // hill wood
            //tcks = -2036036062;
            //846181346
            //tcks = 944096234;
            //tcks = 1991245194;
            Init(gameModel, tcks);
        }

        public Game(GameModel gameModel, int seed)
        {
            Init(gameModel, seed);
        }

        public Tile GetTile(Position p)
        {
            return Map.GetTile(p);
        }

        private void Init(GameModel gameModel, int initSeed)
        {
            Ants.Pheromones.Clear();
            seed = initSeed;
            Random = new Random(seed);
            MoveNr = 0;
            GameModel = gameModel;
            Map = new Map(this, initSeed);
            Players = new Dictionary<int, Player>();

            if (!string.IsNullOrEmpty(logFile))
                File.Delete(logFile);
            if (gameModel?.Players != null)
            {
                foreach (PlayerModel playerModel in gameModel.Players)
                {
                    Player p = new Player(this, playerModel);
                    Players.Add(playerModel.Id, p);
                }
            }
        }

        public void ComputePossibleMoves(Position pos, List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter)
        {
            Tile t = Map.GetTile(pos);
            if (t != null && t.Unit != null)
            {
                t.Unit.ComputePossibleMoves(possibleMoves, includedPositions, moveFilter);
            }
        }

        public List<Position> FindPath(Position from, Position to, Unit unit)
        {
            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            return pathFinder.FindPath(unit, from, to);
        }

        public Move MoveTo(Position from, Position to, Engine engine)
        {
            Tile t = Map.GetTile(from);
            if (t == null || t.Unit == null)
                throw new Exception("errr");

            Unit unit = Map.Units.GetUnitAt(from);
            if (unit.Power == 0)
                return null;

            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            List<Position> route = pathFinder.FindPath(unit, from, to);
            if (route == null)
                return null;

            if (engine != null)
            {
                while (route.Count-1 > engine.Range)
                {
                    route.RemoveAt(route.Count-1);
                }
            }
            if (route.Count > 1)
            {
                if (Map.GetTile(route[route.Count-1]).Unit != null)
                    route.RemoveAt(route.Count - 1);
            }
            if (route.Count < 2)
                return null;

            Move move = new Move();
            move.PlayerId = t.Unit.Owner.PlayerModel.Id;
            move.MoveType = MoveType.Move;
            move.Positions = route;
            move.UnitId = t.Unit.UnitId;

            return move;
        }

        private bool initialized;
        private List<Move> lastMoves = new List<Move>();
        private List<Move> newMoves = new List<Move>();

        public Dictionary<int,Player> Players { get; set; }

        private int MoveNr;
        private int CollisionCntr;

        private Direction CalcDirection(Position fromGround, Position destinationGround)
        {
            //int angle = 0;
            Direction direction = Direction.C;

            // move right
            if (fromGround.X < destinationGround.X)
            {
                if (fromGround.X % 2 != 0)
                {
                    if (fromGround.Y < destinationGround.Y)
                    {
                        // right down OK,
                        //angle = 40;
                        direction = Direction.SE;
                    }
                    if (fromGround.Y == destinationGround.Y)
                    {
                        // right up OK
                        //angle = -40;
                        direction = Direction.NE;
                    }
                    if (fromGround.Y > destinationGround.Y)
                    {
                        // never OK
                        //angle = 0;
                        direction = Direction.C;
                    }
                }
                else
                {
                    if (fromGround.Y < destinationGround.Y)
                    {
                        // never OK
                        //angle = 0;
                        direction = Direction.C;
                    }
                    if (fromGround.Y == destinationGround.Y)
                    {
                        // right down OK
                        //angle = 40;
                        direction = Direction.SE;
                    }
                    if (fromGround.Y > destinationGround.Y)
                    {
                        // right up OK
                        //angle = -40;
                        direction = Direction.NE;
                    }

                }

            }
            // move left
            if (fromGround.X > destinationGround.X)
            {
                if (fromGround.X % 2 != 0)
                {
                    if (fromGround.Y < destinationGround.Y)
                    {
                        // left down OK
                        //angle = 140;
                        direction = Direction.SW;
                    }
                    if (fromGround.Y == destinationGround.Y)
                    {
                        // left up OK
                        //angle = 215;
                        direction = Direction.NW;
                    }
                    if (fromGround.Y > destinationGround.Y)
                    {
                        // never
                        //angle = 0;
                        direction = Direction.C;
                    }
                }
                else
                {
                    if (fromGround.Y < destinationGround.Y)
                    {
                        // never
                        //angle = 0;
                        direction = Direction.NE;
                    }
                    if (fromGround.Y == destinationGround.Y)
                    {
                        // left down OK
                        //angle = 140;
                        direction = Direction.SW;
                    }
                    if (fromGround.Y > destinationGround.Y)
                    {
                        // left up OK
                        //angle = 215;
                        direction = Direction.NW;
                    }

                }
            }


            if (fromGround.X == destinationGround.X)
            {
                if (fromGround.Y < destinationGround.Y)
                {
                    // OK
                    //angle = 90;
                    direction = Direction.S;
                }
                if (fromGround.Y > destinationGround.Y)
                {
                    // OK
                    //angle = -90;
                    direction = Direction.N;
                }
            }
            return direction;
        }

        public void UpdateUnitPositions(List<Move> newMoves)
        {
            CollisionCntr++;
            if (CollisionCntr == 446)
            {

            }

            List<Unit> addedUnits = new List<Unit>();

            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Move || move.MoveType == MoveType.Add)
                {
                    Position Destination;
                    Position From;

                    From = move.Positions[0];
                    Destination = move.Positions[move.Positions.Count - 1];

                    Unit thisUnit;

                    thisUnit = Map.Units.GetUnitAt(From);
                    if (move.MoveType == MoveType.Move && move.Stats == null)
                        move.Stats = thisUnit.CollectStats();

                    if (move.MoveType == MoveType.Add)
                    {
                        // New units will be added here
                        int? containerNetal = null;
                        bool markForExtraction = false;

                        if (move.UnitId.Contains(":Remove"))
                        {
                            // 
                            if (move.UnitId.EndsWith("Reactor"))
                                thisUnit.Reactor = null;
                            else if (move.UnitId.EndsWith("Engine"))
                            {
                                markForExtraction = true;
                                thisUnit.Engine = null;

                                UpdateGroundPlates(null, thisUnit);
                            }
                            else if (move.UnitId.EndsWith("Container"))
                            {
                                containerNetal = thisUnit.Container.Metal;
                                thisUnit.Container = null;
                            }
                            else
                                throw new Exception();
                            move.UnitId = move.UnitId.Replace("Remove", "");
                        }
                        thisUnit = new Unit(this, move.UnitId);
                        if (thisUnit.Container != null && containerNetal.HasValue)
                            thisUnit.Container.Metal = containerNetal.Value;

                        thisUnit.Power = 100;
                        thisUnit.ExtractMe = markForExtraction;

                        move.UnitId = thisUnit.UnitId;
                        move.Stats = thisUnit.CollectStats();
                        thisUnit.Pos = Destination;
                        if (move.Positions.Count > 1)
                            thisUnit.Direction = CalcDirection(move.Positions[0], move.Positions[1]);
                        if (move.PlayerId > 0)
                            thisUnit.Owner = Players[move.PlayerId];
                    }
                    else
                    {
                        // Remove moving unit from map
                        if (thisUnit != null && move.Positions.Count > 1)
                            thisUnit.Direction = CalcDirection(move.Positions[0], move.Positions[1]);

                        Map.Units.Remove(From);
                    }

                    if (thisUnit == null)
                        throw new Exception("wrong");

                    // Update new pos
                    thisUnit.Pos = Destination;
                    addedUnits.Add(thisUnit);

                    // is there a unit on the map?
                    Unit otherUnit = Map.Units.GetUnitAt(Destination);
                    if (otherUnit != null)
                    {
                        throw new Exception("Handle collisions failed");
                    }
                }
            }
            
            foreach (Unit addedUnit in addedUnits)
            {
                if (addedUnit.Pos != null &&
                    Map.Units.GetUnitAt(addedUnit.Pos) == null)
                {
                    Map.Units.Add(addedUnit);
                }
            }
        }

        private void Szenario1(List<Move> newMoves)
        {
            int playerNum = 0;
            foreach (Player player in Players.Values)
            {
                if (playerNum == 0)
                {
                    Move move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    //move.UnitId = "1;1;1";
                    move.UnitId = "1;1;1;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, 0));

                    newMoves.Add(move);

                    playerNum++;
                }
                else if (playerNum == 1)
                {
                    Move move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;0;0;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(2, 2));

                    newMoves.Add(move);

                    playerNum++;
                }
            }
        }

        private void SzenarioShowUnits(List<Move> newMoves)
        {
            Move move;

            int playerNum = 0;
            foreach (Player player in Players.Values)
            {
                if (playerNum == 0)
                {
                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;0;0";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, 0));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;0;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, 1));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;1;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, 2));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;1;1;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, 3));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;0;1;1;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, 4));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "0;1;0";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(1, 0));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "0;0;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(2, 0));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "0;0;0;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(3, 0));
                    newMoves.Add(move);

                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "0;0;0;0;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(4, 0));
                    newMoves.Add(move);

                    playerNum++;
                }
                else if (playerNum == 1)
                {
                    move = new Move();
                    move.MoveType = MoveType.Add;
                    move.PlayerId = player.PlayerModel.Id;
                    move.UnitId = "1;0;1;1;1";
                    move.Positions = new List<Position>();
                    move.Positions.Add(new Position(0, -4));
                    newMoves.Add(move);
                }
            }
        }

        private void Initialize(List<Move> newMoves)
        {
            //Szenario1(newMoves);
            //SzenarioShowUnits(newMoves);
            StartWithFactory(newMoves);

            initialized = true;
        }

        private void ProcessLastMoves()
        {
            List<Move> nextMoves = new List<Move>();

            List<Move> finishedMoves = new List<Move>();
            foreach (Move move in lastMoves)
            {
                if (move.MoveType == MoveType.Add ||
                    move.MoveType == MoveType.Assemble ||
                    move.MoveType == MoveType.Upgrade ||
                    move.MoveType == MoveType.Hit ||
                    move.MoveType == MoveType.UpdateGround ||
                    move.MoveType == MoveType.Skip ||
                    move.MoveType == MoveType.UpdateStats ||
                    move.MoveType == MoveType.VisibleTiles ||
                    move.MoveType == MoveType.HiddenTiles)
                {
                    finishedMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Delete)
                {
                    finishedMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Move)
                {
                    finishedMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Extract)
                {
                    Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                    if (unit != null && unit.Extractor != null)
                    {
                        bool extracted = false;

                        Position fromPos = move.Positions[move.Positions.Count - 1];
                        extracted = unit.Extractor.ExtractInto(fromPos, nextMoves, this);
                        if (extracted)
                        {
                            Move hitmove = new Move();
                            hitmove.MoveType = MoveType.UpdateGround;
                            hitmove.Positions = new List<Position>();
                            hitmove.Positions.Add(fromPos);
                            nextMoves.Add(hitmove);

                            Move moveUpdate = new Move();

                            moveUpdate.MoveType = MoveType.UpdateStats;
                            moveUpdate.UnitId = unit.UnitId;
                            moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                            moveUpdate.Positions = new List<Position>();
                            moveUpdate.Positions.Add(unit.Pos);
                            moveUpdate.Stats = unit.CollectStats();

                            nextMoves.Add(moveUpdate);
                        }
                        else
                        {
                            // cloud not extract, ignore move
                            //move.MoveType = MoveType.None;
                        }
                    }
                    else
                    {
                        // move failed
                        //move.MoveType = MoveType.None;
                    }
                    finishedMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Fire)
                {
                    Unit targetUnit = Map.Units.GetUnitAt(move.Positions[1]);
                    if (targetUnit != null)
                    {
                        int totalMetalInUnitBeforeHit = targetUnit.CountMetal();

                        // Todo: Drop all the metal if container level goes down

                        if (targetUnit.HitBy(null))
                        {
                            // Unit has died!
                            Move deleteMove = new Move();
                            deleteMove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                            deleteMove.MoveType = MoveType.Delete;
                            deleteMove.Positions = new List<Position>();
                            deleteMove.Positions.Add(targetUnit.Pos);
                            deleteMove.UnitId = targetUnit.UnitId;
                            nextMoves.Add(deleteMove);
                            
                            Map.Units.Remove(targetUnit.Pos);

                            Tile unitTile = GetTile(targetUnit.Pos);

                            int totalMetalAfterUnit = targetUnit.CountMetal();
                            int releasedMetal = totalMetalInUnitBeforeHit - totalMetalAfterUnit;

                            // Bullet + demaged Part + collected metal
                            unitTile.Metal += 2 + releasedMetal;
                        }
                        else
                        {
                            // Unit was hit
                            Move hitmove = new Move();
                            hitmove.MoveType = MoveType.Hit;
                            hitmove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                            hitmove.Positions = new List<Position>();
                            hitmove.Positions.Add(targetUnit.Pos);
                            hitmove.UnitId = targetUnit.UnitId;
                            hitmove.Stats = targetUnit.CollectStats();
                            nextMoves.Add(hitmove);

                            int totalMetalAfterUnit = targetUnit.CountMetal();
                            int releasedMetal = totalMetalInUnitBeforeHit - totalMetalAfterUnit;

                            Tile unitTile = GetTile(targetUnit.Pos);
                            // Bullet + demage Part
                            unitTile.Metal += 2 + releasedMetal;
                        }
                    }
                    else
                    {
                        Move hitmove = new Move();
                        hitmove.MoveType = MoveType.UpdateGround;
                        hitmove.Positions = new List<Position>();
                        hitmove.Positions.Add(move.Positions[1]);
                        nextMoves.Add(hitmove);

                        // Fired on ground (+Bullet)
                        Map.GetTile(move.Positions[1]).Metal++;
                    }
                    finishedMoves.Add(move);
                }
                else
                {
                    throw new Exception("missing");
                }
            }
            foreach (Move move in finishedMoves)
            {
                lastMoves.Remove(move);
            }
            foreach (Move move in nextMoves)
            {
                lastMoves.Add(move);
            }
        }
        private void LogMoves(string header, int moveNr, List<Move> moves)
        {
            if (string.IsNullOrEmpty(logFile))
                return;
            using (StreamWriter sr = new StreamWriter(logFile, true))
            {
                sr.WriteLine("");
                sr.WriteLine(header);
                Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

                Newtonsoft.Json.JsonTextWriter writer = new JsonTextWriter(sr);
                writer.Formatting = Formatting.Indented;

                json.Serialize(writer, moves);
            }
        }
        private Move CheckIfPlayerCanMove(int playerId)
        {
            foreach (Unit unit in Map.Units.List.Values)
            {
                if (unit.Owner.PlayerModel.Id == playerId)
                {
                    List<Move> moves = new List<Move>();
                    unit.ComputePossibleMoves(moves, null, MoveFilter.All);
                    if (moves.Count > 0)
                        return null;
                }
            }
            Move move = new Move();
            move.MoveType = MoveType.Skip;
            return move;
        }

        private void AutoFire()
        {
            foreach (Unit unit in Map.Units.List.Values)
            {
                if (unit.Weapon != null)
                {
                    bool found = false;
                    foreach (Move move in lastMoves)
                    {
                        if (move.UnitId == unit.UnitId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        List<Move> possibleMoves = new List<Move>();
                        unit.Weapon.ComputePossibleMoves(possibleMoves, null, MoveFilter.Fire);
                        if (possibleMoves.Count > 0)
                        {
                            lastMoves.Add(possibleMoves.ElementAt(0));
                        }
                    }
                }
            }
        }

        private bool CollectNewMoves(Move myMove)
        {
            bool allPlayersMoved = true;

            foreach (Player player in Players.Values)
            {
                List<Move> moves = player.Control.Turn(player);
                if (moves != null)
                {
                    Dictionary<string, Move> unitsThatMoved = new Dictionary<string, Move>();

                    foreach (Move move in moves)
                    {
                        if (move.MoveType == MoveType.Add)
                        {
                            if (unitsThatMoved.ContainsKey(move.OtherUnitId))
                                throw new Exception("Cheater");
                            unitsThatMoved.Add(move.OtherUnitId, move);
                        }
                        else
                        {
                            if (move.MoveType == MoveType.Move)
                            {
                                Unit unit = Map.Units.GetUnitAt(move.Positions[0]);

                                if (unit.Engine == null)
                                    throw new Exception("Cheater");

                                if (unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                                    throw new Exception("Cheater");
                            }
                            else if (move.MoveType == MoveType.Fire)
                            {
                                Unit unit = Map.Units.GetUnitAt(move.Positions[0]);

                                if (unit.Weapon == null)
                                    throw new Exception("Cheater");
                            }
                            else if (move.MoveType == MoveType.UpdateStats)
                            {
                                if (unitsThatMoved.ContainsKey(move.UnitId))
                                    throw new Exception("Cheater");
                                unitsThatMoved.Add(move.UnitId, move);
                            }
                            else if (move.MoveType == MoveType.Extract)
                            {
                                Unit unit = Map.Units.GetUnitAt(move.Positions[0]);

                                if (unit.Extractor == null)
                                    throw new Exception("Cheater");
                            }
                            if (move.UnitId.StartsWith("unit"))
                            {
                                if (unitsThatMoved.ContainsKey(move.UnitId))
                                    throw new Exception("Cheater");
                                unitsThatMoved.Add(move.UnitId, move);
                            }
                        }
                        move.PlayerId = player.PlayerModel.Id;
                        newMoves.Add(move);
                    }
                }
            }
            return allPlayersMoved;
        }

        private void Validate(List<Move> moves)
        {
            foreach (Player player in Players.Values)
            {
                foreach (PlayerUnit playerUnit in player.Units.Values)
                {
                    Unit unit = playerUnit.Unit;
                    Tile tile = Map.GetTile(unit.Pos);
                    Unit mapUnit = Map.Units.GetUnitAt(unit.Pos);
                    if (mapUnit == null)
                    {
                        throw new Exception("player has unit that does not exists");
                    }
                    if (tile.Unit != mapUnit)
                    {
                        throw new Exception("player has unit that does not exists1");
                    }
                    if (unit.UnitId != mapUnit.UnitId)
                    {
                        throw new Exception("wrong unit moved");
                    }
                    if (unit.Owner.PlayerModel.Id != mapUnit.Owner.PlayerModel.Id)
                    {
                        throw new Exception("wrong player");
                    }
                }
            }
            foreach (Unit unit in Map.Units.List.Values)
            {

            }
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Add)
                {
                    int cnt = move.Positions.Count;
                    Position p = move.Positions[cnt-1];
                    Unit unit = Map.Units.GetUnitAt(p);
                    if (unit == null)
                    {
                        throw new Exception("unit not on map");
                    }
                    if (move.PlayerId != 0 && unit.Owner.PlayerModel.Id != move.PlayerId)
                    {
                        throw new Exception("wrong player");
                    }
                }
                if (move.MoveType == MoveType.Move)
                {
                    Position p = move.Positions[0];
                    if (move.Positions.Count > 1)
                    {
                        p = move.Positions[move.Positions.Count-1];
                        Unit unit = Map.Units.GetUnitAt(p);
                        if (unit == null)
                        {
                            throw new Exception("unit not moved there");
                        }
                        if (unit.UnitId != move.UnitId)
                        {
                            throw new Exception("wrong unit moved");
                        }
                        if (unit.Owner.PlayerModel.Id != move.PlayerId)
                        {
                            throw new Exception("wrong player");
                        }
                    }
                }

                if (move.MoveType == MoveType.Delete)
                {
                    int cnt = move.Positions.Count;
                    Position p = move.Positions[cnt - 1];
                    Unit unit = Map.Units.GetUnitAt(p);
                    if (unit != null)
                    {
                        if (unit.UnitId == move.UnitId)
                            throw new Exception("unit not deleted");
                    }
                }
            }
        }

        public void UpdateGroundPlates(List<Move> moves, Unit unit, bool remove = false)
        {
            if (unit.Engine == null)
            {
                int range = 0;

                if (unit.Assembler != null && unit.Assembler.Level == 1) range = 1;
                if (unit.Assembler != null && unit.Assembler.Level == 2) range = 1;
                if (unit.Assembler != null && unit.Assembler.Level == 3) range = 1;

                //if (unit.Reactor != null && unit.Reactor.Level == 2) range = 2;
                //if (unit.Reactor != null && unit.Reactor.Level == 2) range = 2;
                //if (unit.Reactor != null && unit.Reactor.Level == 3) range = 3;

                //if (unit.Container != null && unit.Container.Level == 2) range = 2;
                //if (unit.Container != null && unit.Container.Level == 2) range = 2;
                //if (unit.Container != null && unit.Container.Level == 3) range = 3;

                //if (unit.Radar != null && unit.Radar.Level == 2) range = 2;
                //if (unit.Radar != null && unit.Radar.Level == 2) range = 2;
                //if (unit.Radar != null && unit.Radar.Level == 3) range = 3;

                

                Dictionary<Position, TileWithDistance> tiles = Map.EnumerateTiles(unit.Pos, range);
                if (tiles != null)
                {
                    foreach (TileWithDistance n in tiles.Values)
                    {
                        if (remove)
                            n.Tile.Plates--;
                        else
                            n.Tile.Plates++;

                        if (moves != null)
                        {
                            Move hitmove = new Move();
                            hitmove.MoveType = MoveType.UpdateGround;
                            hitmove.Positions = new List<Position>();
                            hitmove.Positions.Add(n.Pos);
                            moves.Add(hitmove);
                        }
                    }
                }
            }
        }

        private void ProcessNewMoves()
        {
            lastMoves.Clear();
            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Add)
                {
                    Unit factory = Map.Units.GetUnitAt(move.Positions[0]);
                    Unit newUnit = null;
                    if (move.Positions.Count > 1)
                        newUnit = Map.Units.GetUnitAt(move.Positions[1]);

                    if (factory != null && newUnit != null)
                    {
                        factory.Assembler.ConsumeMetalForUnit(newUnit);
 
                        Move moveUpdate = new Move();
                        moveUpdate.PlayerId = factory.Owner.PlayerModel.Id;
                        moveUpdate.MoveType = MoveType.UpdateStats;
                        moveUpdate.UnitId = factory.UnitId;
                        moveUpdate.Positions = new List<Position>();
                        moveUpdate.Positions.Add(factory.Pos);
                        moveUpdate.Stats = factory.CollectStats();
                        lastMoves.Add(moveUpdate);
                    }
                    if (newUnit != null)
                    {
                        UpdateGroundPlates(lastMoves, newUnit);
                    }
                    else
                    {
                        // Startunit
                        UpdateGroundPlates(lastMoves, factory);
                    }
                }
                else if (move.MoveType == MoveType.Upgrade)
                {
                    Unit factory = Map.Units.GetUnitAt(move.Positions[0]);
                    Unit newUnit = Map.Units.GetUnitAt(move.Positions[1]);

                    if (factory != null && newUnit != null)
                    {
                        factory.Assembler.ConsumeMetalForUnit(newUnit);

                        Move moveUpdate = new Move();
                        moveUpdate.PlayerId = factory.Owner.PlayerModel.Id;
                        moveUpdate.MoveType = MoveType.UpdateStats;
                        moveUpdate.UnitId = factory.UnitId;
                        moveUpdate.Positions = new List<Position>();
                        moveUpdate.Positions.Add(factory.Pos);
                        moveUpdate.Stats = factory.CollectStats();
                        lastMoves.Add(moveUpdate);
                        
                    }
                    if (newUnit != null)
                    {
                        UpdateGroundPlates(lastMoves, newUnit, remove: true);
                        newUnit.Upgrade(move.UnitId);

                        if (move.UnitId == "Container")
                        {
                            if (newUnit.Container.Metal == 0)
                            {
                                int metal = 0;
                                /*
                                if (factory.Container != null)
                                {
                                    if (factory.Container.Metal > 20)
                                        metal = 20;
                                }*/
                                newUnit.Container.Metal += metal;
                            }
                        }

                        Move moveUpdate = new Move();
                        moveUpdate.PlayerId = newUnit.Owner.PlayerModel.Id;
                        moveUpdate.MoveType = MoveType.UpdateStats;
                        moveUpdate.UnitId = newUnit.UnitId;
                        moveUpdate.Positions = new List<Position>();
                        moveUpdate.Positions.Add(newUnit.Pos);
                        moveUpdate.Stats = newUnit.CollectStats();
                        lastMoves.Add(moveUpdate);

                        UpdateGroundPlates(lastMoves, newUnit);
                    }
                }
                else if (move.MoveType == MoveType.Extract)
                {
                    /*
                    Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                    if (unit != null && unit.Extractor != null)
                    {
                        bool extracted = false;

                        Position fromPos = move.Positions[move.Positions.Count-1];
                        extracted = unit.Extractor.ExtractInto(fromPos, lastMoves);

                        if (extracted)
                        {
                            Move moveUpdate = new Move();

                            moveUpdate.MoveType = MoveType.UpdateStats;
                            moveUpdate.UnitId = unit.UnitId;
                            moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                            moveUpdate.Positions = new List<Position>();
                            moveUpdate.Positions.Add(unit.Pos);
                            moveUpdate.Stats = unit.CollectStats();

                            lastMoves.Add(moveUpdate);
                            
                        }
                        else
                        {
                            // cloud not extract, ignore move
                            move.MoveType = MoveType.None;
                        }
                    }
                    else
                    {
                        // move failed
                        move.MoveType = MoveType.None;
                    }*/
                }
                else if (move.MoveType == MoveType.Fire)
                {
                    Unit fireingUnit = Map.Units.GetUnitAt(move.Positions[0]);
                    if (fireingUnit != null && fireingUnit.Weapon != null)
                    {
                        if (fireingUnit.Container != null && fireingUnit.Container.Metal > 0)
                            fireingUnit.Container.Metal--;
                        else if (fireingUnit.Metal > 0)
                            fireingUnit.Metal--;
                        else
                            throw new Exception();
                        move.Stats = fireingUnit.CollectStats();

                        Move moveUpdate = new Move();

                        moveUpdate.MoveType = MoveType.UpdateStats;
                        moveUpdate.UnitId = fireingUnit.UnitId;
                        moveUpdate.PlayerId = fireingUnit.Owner.PlayerModel.Id;
                        moveUpdate.Positions = new List<Position>();
                        moveUpdate.Positions.Add(fireingUnit.Pos);
                        moveUpdate.Stats = fireingUnit.CollectStats();

                        lastMoves.Add(moveUpdate);
                        
                    }

                }
                else if (move.MoveType == MoveType.Move)
                {

                }
                if (move.MoveType != MoveType.None &&
                    move.MoveType != MoveType.Skip)
                {
                    lastMoves.Add(move);
                }
            }
            newMoves.Clear();

            foreach (Unit unit in Map.Units.List.Values)
            {
                bool found = false;
                foreach (Move move in lastMoves)
                {
                    if (move.UnitId == unit.UnitId)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    //if (unit.Weapon != null && unit.Stats.FireCooldown > 0)
                    {
                        //unit.Stats.FireCooldown--;
                        //if (unit.Stats.FireCooldown == 0)
                        {
                            /*
                            Move move = new Move();
                            move.MoveType = MoveType.UpdateStats;
                            move.UnitId = unit.UnitId;
                            move.PlayerId = unit.Owner.PlayerModel.Id;

                            move.Positions = new List<Position>();
                            move.Positions.Add(unit.Pos);

                            move.Stats = unit.Stats;
                            if (move.Stats == null)
                                throw new Exception();

                            lastMoves.Add(move);
                            */
                        }
                    }
                    //if (unit.Engine != null && unit.Stats.MoveCooldown > 0)
                    {
                        //unit.Stats.MoveCooldown--;
                        //if (unit.Stats.MoveCooldown == 0)
                        {
                            /*
                            Move move = new Move();
                            move.MoveType = MoveType.UpdateStats;
                            move.UnitId = unit.UnitId;
                            move.PlayerId = unit.Owner.PlayerModel.Id;

                            move.Positions = new List<Position>();
                            move.Positions.Add(unit.Pos);

                            move.Stats = unit.Stats;
                            if (move.Stats == null)
                                throw new Exception();

                            lastMoves.Add(move);
                            */
                        }
                    }
                    if (unit.Assembler != null) // && unit.Stats.ProductionCooldown > 0)
                    {
                        //unit.Stats.ProductionCooldown--;
                        //if (unit.Stats.ProductionCooldown == 0)
                        {
                            Player player = Players[unit.Owner.PlayerModel.Id];
                            if (player.CanProduceMoreUnits())
                            {
                                /*
                                Move move = new Move();
                                move.MoveType = MoveType.UpdateStats;
                                move.PlayerId = unit.Owner.PlayerModel.Id;
                                move.UnitId = unit.UnitId;
                                move.Positions = new List<Position>();
                                move.Positions.Add(unit.Pos);
                                move.Stats = unit.Stats;
                                if (move.Stats == null)
                                    throw new Exception();
                                lastMoves.Add(move);
                                */
                            }
                        }
                    }
                }
            }
        }

        private void UpdateAll(int playerId, List<Move> returnMoves)
        {
            foreach (Player player in Players.Values)
            {
                if (playerId == 0 || player.PlayerModel.Id == playerId)
                {
                    player.UpdateAll(returnMoves);
                }
            }
        }

        private void ConsumePower(List<Move> newMoves)
        {
            foreach (Unit unit in Map.Units.List.Values)
            {
                if (unit.Power > 0)
                {
                    //unit.Power--;
                    /*
                    Move hitmove = new Move();
                    hitmove.MoveType = MoveType.UpdateStats;
                    hitmove.PlayerId = 0;
                    hitmove.Positions = new List<Position>();
                    hitmove.Positions.Add(unit.Pos);
                    hitmove.UnitId = unit.UnitId;
                    hitmove.Stats = unit.CollectStats();
                    newMoves.Add(hitmove);*/
                }
            }
        }

        private void HandleCollisions(List<Move> newMoves)
        {
            CollisionCntr++;
            if (CollisionCntr == 445)
            {

            }

            bool somethingChanged = true;

            while (somethingChanged)
            {
                somethingChanged = false;
                List<Move> acceptedMoves = new List<Move>();

                // Remove moves that go to the same destination
                Dictionary<Position, Move> moveToTargets = new Dictionary<Position, Move>();
                foreach (Move move in newMoves)
                {
                    if (move.MoveType == MoveType.Move || move.MoveType == MoveType.Add)
                    {
                        Position destination = move.Positions[move.Positions.Count - 1];
                        if (moveToTargets.ContainsKey(destination))
                        {
                            // (Hit) Could do nasty things, but for now, the unit does not move
                            somethingChanged = true;
                        }
                        else
                        {
                            moveToTargets.Add(destination, move);
                        }
                    }
                    else
                    {
                        acceptedMoves.Add(move);
                    }
                }



                foreach (Move move in moveToTargets.Values)
                {
                    Position destination = move.Positions[move.Positions.Count - 1];
                    Tile t = Map.GetTile(destination);
                    if (t == null)
                    {
                        // Moved outside?
                        throw new Exception("bah");
                    }
                    else if (!t.CanMoveTo())
                    {
                        // Move to invalid pos
                        // Happend with bad startup pos
                        //throw new Exception("how dare you");
                    }
                    else if (t.Unit != null)
                    {
                        bool unitBlocked = true;
                        // Move onto another unit? Check if this unit goes away
                        foreach (Move unitMove in moveToTargets.Values)
                        {
                            if ((unitMove.MoveType == MoveType.Move || unitMove.MoveType == MoveType.Add) && t.Unit.UnitId == unitMove.UnitId)
                            {
                                // This unit moves away, so it is ok
                                acceptedMoves.Add(move);
                                unitBlocked = false;
                                break;
                            }
                        }
                        if (unitBlocked == true)
                        {
                            // (Hit) Could do nasty things, but for now, the unit does not move
                            somethingChanged = true;
                        }
                    }
                    else
                    {
                        acceptedMoves.Add(move);
                    }
                }
                newMoves.Clear();
                newMoves.AddRange(acceptedMoves);
            }
        }

        public List<Move> ProcessMove(int playerId, Move myMove)
        {
            List<Move> returnMoves = new List<Move>();
            lock (GameModel)
            {
                if (myMove != null && myMove.MoveType == MoveType.UpdateAll)
                {
                    UpdateAll(playerId, returnMoves);
                    return returnMoves;
                }

                if (MoveNr == 31)
                {

                }
                bool first = false;
                if (!initialized)
                {
                    first = true;
                    Initialize(newMoves);

                    //CheckCollisions(newMoves);
                }
                else
                {
#if DEBUG
                    Validate(lastMoves);
#endif

                    if (lastMoves.Count > 0)
                    {
                        // Remove moves that have been processed
                        ProcessLastMoves();
                        if (lastMoves.Count > 0)
                        {
                            // Follow up moves (units have been hit i.e. and must be removed. This is a result of the
                            // last move. Update the players unit list (delete moves are called double in this case)
                            foreach (Player player in Players.Values)
                            {
                                player.ProcessMoves(lastMoves);
                                if (player.Control != null)
                                    player.Control.ProcessMoves(player, player.LastMoves);
                            }
                            newMoves.AddRange(lastMoves);
                            lastMoves.Clear();
                        }
                    }
                }

                if (!first && lastMoves.Count == 0)
                {
                    // New move
                    bool allPlayersMoved = CollectNewMoves(myMove);
                    if (!allPlayersMoved)
                    {
                        return lastMoves;
                    }
                }
                ConsumePower(newMoves);

                LogMoves("Process new Moves " + Seed, MoveNr, newMoves);

                // Check collisions and change moves if units collide or get destroyed
                HandleCollisions(newMoves);
                
                LogMoves("New Moves after Collisions", MoveNr, newMoves);                

                UpdateUnitPositions(newMoves);
                
                ProcessNewMoves();

                foreach (Player player in Players.Values)
                {
                    if (player.PlayerModel.Id == playerId)
                    {
                        player.ProcessMoves(lastMoves);
                        returnMoves = player.LastMoves;
                    }
                    else
                    {
                        player.ProcessMoves(lastMoves);
                        if (player.Control != null)
                            player.Control.ProcessMoves(player, player.LastMoves);
                    }
                    if (player.NumberOfUnits == 0 && player.Commands.Count > 0)
                    {
                        // This player is dead.
                        player.Commands.Clear();
                    }
                }

                CreateAreas();
                if (playerId == 0)
                {
                    returnMoves = lastMoves;
#if DEBUG
                    Validate(returnMoves);
#endif
                }
            }
            if (lastMoves.Count >= 0)
            {
                //OutgoingMoves.Add(MoveNr, lastMoves);
                MoveNr++;
            }

            // Unsorted. So UpdateUnit comes bevor this unit moves. Better: Remmove the Update Unit because the move does ths update

            return returnMoves;
            /*
            var mySortedList = new SortedList<MoveType, Move>(new MoveTypeComparer());

            // Sort moves
            foreach (Move move in returnMoves)
            {
                mySortedList.Add(move.MoveType, move);
            }
            // Check if a unit moves to a occupied place
            return mySortedList.Values.ToList();
            */
        }

        public List<Area> Areas { get; private set; }

        private void CreateAreas()
        {
            Areas = new List<Area>();
            
            Dictionary<Tile, Area> areaMap = new Dictionary<Tile, Area>();
            List<TileWithDistance> openList = new List<TileWithDistance>();

            foreach (Unit unit in Map.Units.List.Values)
            {
                if (unit.Radar != null)
                {
                    Area area = new Area(Map);

                    area.Range = unit.Radar.Level * 4;

                    Tile tile = Map.GetTile(unit.Pos);
                    if (tile.CanMoveTo())
                        openList.Add(new TileWithDistance(tile, 0));

                    area.PlayerId = tile.Unit.Owner.PlayerModel.Id;
                    area.Units.Add(new PlayerUnit(unit));
                    area.Tiles.Add(tile.Pos, tile);
                    areaMap.Add(tile, area);
                }
            }

            while (openList.Count > 0)
            {
                TileWithDistance tile = openList[0];
                openList.RemoveAt(0);

                Area startArea = null;
                foreach (Area area in areaMap.Values)
                {
                    if (area.Tiles.ContainsKey(tile.Pos))
                    {
                        startArea = area;
                        break;
                    }
                }
                //int distance = 8;
                //if (tile.Unit != null && tile.Unit.Radar != null)
                //    distance = tile.Unit.Radar.Level * 4;

                foreach (Tile n in tile.Neighbors)
                {
                    //if (!VisiblePositions.Contains(n.Pos))
                    //    continue;

                    if (!n.CanMoveTo())
                        continue;

                    TileWithDistance neighborsTile = new TileWithDistance(GetTile(n.Pos), tile.Distance + 1);
                    if (neighborsTile.Distance > startArea.Range)
                        continue;

                    Area otherArea = null;
                    foreach (Area area in areaMap.Values)
                    {
                        if (area.Tiles.ContainsKey(n.Pos))
                        {
                            otherArea = area;
                            break;
                        }
                    }
                    if (otherArea == null)
                    {
                        startArea.Tiles.Add(n.Pos, n);
                        openList.Add(neighborsTile);
                    }
                    else
                    {
                        if (startArea != otherArea &&
                            startArea.PlayerId == otherArea.PlayerId)
                        {
                            startArea.Units.AddRange(otherArea.Units);
                            foreach (Tile t in otherArea.Tiles.Values)
                                startArea.Tiles.Add(t.Pos, t);

                            foreach (Tile tunit in otherArea.Tiles.Values)
                            {
                                if (tunit.Unit != null)
                                {
                                    areaMap[tunit] = startArea;
                                }
                            }
                        }
                    }
                }
            }

            int areaCounter = 0;
            foreach (Area area in areaMap.Values)
            {
                if (!Areas.Contains(area))
                {
                    area.AreaNr = areaCounter++;
                    Areas.Add(area);
                }
            }

            // determine border
            foreach (Area area in Areas)
            {
                foreach (Tile tile in area.Tiles.Values)
                {
                    foreach (Tile n in tile.Neighbors)
                    {
                        if (!area.Tiles.ContainsKey(n.Pos))
                        {
                            Area otherArea = null;
                            foreach (Area area2 in areaMap.Values)
                            {
                                if (area2.Tiles.ContainsKey(n.Pos))
                                {
                                    otherArea = area2;
                                    break;
                                }
                            }
                            if (otherArea == null)
                            {
                                area.ForeignBorderTiles.Add(tile.Pos, tile);
                                
                            }
                            else
                            {
                                area.BorderTiles.Add(tile.Pos, tile);

                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    public class MoveTypeComparer : IComparer<MoveType>
    {
        public int Compare(MoveType x, MoveType y)
        {
            if (x < y)
                return -1;
            else
                return 1;
        }
    }
}