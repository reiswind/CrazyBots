using Engine.Algorithms;
using Engine.Ants;
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
    public class Game : IGameController
    {
        private static string logFile = null; // = @"c:\Temp\moves.json";
        public GameModel GameModel { get; private set; }
        public Blueprints Blueprints { get; set; }
        public Map Map { get; private set; }

        private void Init(GameModel gameModel, int initSeed)
        {
            Blueprints = new Blueprints();
            Pheromones = new Pheromones();

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
                //int zoneCntr = 1;
                foreach (PlayerModel playerModel in gameModel.Players)
                {
                    Player p = new Player(this, playerModel);
                    
                    Players.Add(playerModel.Id, p);
                }
            }

            if (gameModel.MapType == "2")
            {
                Map.CreateFlat();
            }
            else
            {
                Map.CreateTerrain(this);
            }
            //CreateUnits();

            Map.GetTile(Position.CreatePosition(0, 0));

            // TESTEXTRACT

            for (int i = 0; i < Map.DefaultMinerals; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.Direction = Direction.C;
                tileObject.TileObjectType = TileObjectType.Mineral;

                Map.DistributeTileObject(tileObject);
            }
        }

        private void Initialize(List<Move> newMoves)
        {
            PlayerModel playerModel = new PlayerModel();
            playerModel.Id = 0;
            playerModel.Name = "Neutral";
            NeutralPlayer = new Player(this, playerModel);

            StartWithFactory(newMoves);

            initialized = true;
        }

        public void CreateUnits()
        {
            foreach (UnitModel unitModel in GameModel.Units)
            {
                Player player = Players[unitModel.PlayerId];

                ulong posOnMap = Position.Null;
                if (player.StartZone != null)
                {
                    ulong posInModel = Position.ParsePosition(unitModel.Position);
                    posOnMap = Position.CreatePosition(
                        Position.GetX(player.StartZone.Center) + Position.GetX(posInModel),
                        Position.GetY(player.StartZone.Center) + Position.GetY(posInModel));

                    Tile t = Map.GetTile(posOnMap);
                    if (t != null)
                    {
                        Unit thisUnit = new Unit(this, unitModel.Blueprint);

                        thisUnit.Power = 20;
                        thisUnit.MaxPower = 20;
                        thisUnit.CreateAllPartsFromBlueprint();
                        thisUnit.Pos = posOnMap;

                        if (unitModel.ContainerFilled == 0)
                        {
                            if (thisUnit.Container != null)
                                thisUnit.Container.TileContainer.Clear();
                            if (thisUnit.Weapon != null)
                                thisUnit.Weapon.TileContainer.Clear();
                            if (thisUnit.Reactor != null)
                                thisUnit.Reactor.TileContainer.Clear();
                            if (thisUnit.Assembler != null)
                                thisUnit.Assembler.TileContainer.Clear();
                        }
                        // Turn into direction missing
                        thisUnit.Direction = Direction.C; // CalcDirection(move.ulongs[0], move.ulongs[1]);
                        thisUnit.Owner = Players[unitModel.PlayerId];

                        if (Map.Units.GetUnitAt(thisUnit.Pos) == null)
                            Map.Units.Add(thisUnit);

                        if (unitModel.HoldPosition && thisUnit.Engine != null)
                            thisUnit.Engine.HoldPosition = true;

                        if (unitModel.FireAtGround && thisUnit.Weapon != null)
                            thisUnit.Weapon.FireAtGround = true;
                        if (unitModel.HoldFire && thisUnit.Weapon != null)
                            thisUnit.Weapon.HoldFire = true;
                        if (unitModel.EndlessAmmo && thisUnit.Weapon != null)
                            thisUnit.Weapon.EndlessAmmo = true;
                        if (unitModel.EndlessPower)
                            thisUnit.EndlessPower = true;
                        if (unitModel.UnderConstruction)
                            thisUnit.UnderConstruction = true;



                        t.Owner = unitModel.PlayerId;
                        ResetTile(t);
                        foreach (Tile n in t.Neighbors)
                            ResetTile(n);
                    }
                }
            }
        }


        public void StartWithFactory(List<Move> newMoves)
        {
            if (GameModel.Units != null)
            {
                foreach (UnitModel unitModel in GameModel.Units)
                {
                    Player player = Players[unitModel.PlayerId];

                    ulong posOnMap = Position.Null;
                    if (player.StartZone != null)
                    {
                        //posOnMap = n ew ulong(player.StartZone.Center.X + unitModel.ulong.X, player.StartZone.Center.Y + unitModel.ulong.Y);
                        ulong posInModel = Position.ParsePosition(unitModel.Position);
                        posOnMap = Position.CreatePosition(
                            Position.GetX(player.StartZone.Center) + Position.GetX(posInModel),
                            Position.GetY(player.StartZone.Center) + Position.GetY(posInModel));

                        Tile t = Map.GetTile(posOnMap);
                        if (t != null)
                        {
                            Unit thisUnit = new Unit(this, unitModel.Blueprint);

                            thisUnit.Power = 20;
                            thisUnit.MaxPower = 20;
                            thisUnit.CreateAllPartsFromBlueprint();
                            thisUnit.Pos = posOnMap;

                            // Turn into direction missing
                            thisUnit.Direction = Direction.C; // CalcDirection(move.ulongs[0], move.ulongs[1]);
                            thisUnit.Owner = Players[unitModel.PlayerId];

                            if (Map.Units.GetUnitAt(thisUnit.Pos) == null)
                                Map.Units.Add(thisUnit);

                            if (unitModel.HoldPosition && thisUnit.Engine != null)
                                thisUnit.Engine.HoldPosition = true;

                            if (unitModel.FireAtGround && thisUnit.Weapon != null)
                                thisUnit.Weapon.FireAtGround = true;
                            if (unitModel.HoldFire && thisUnit.Weapon != null)
                                thisUnit.Weapon.HoldFire = true;
                            if (unitModel.EndlessAmmo && thisUnit.Weapon != null)
                                thisUnit.Weapon.EndlessAmmo = true;
                            if (unitModel.EndlessPower)
                                thisUnit.EndlessPower = true;

                            Move move = new Move();
                            move.MoveType = MoveType.Add;
                            move.PlayerId = unitModel.PlayerId;
                            move.UnitId = thisUnit.UnitId;
                            move.OtherUnitId = unitModel.Blueprint;
                            move.Stats = thisUnit.CollectStats();
                            move.Positions = new List<ulong>();
                            move.Positions.Add(posOnMap);
                            newMoves.Add(move);

                            t.Owner = unitModel.PlayerId;
                            ResetTile(t);
                            foreach (Tile n in t.Neighbors)
                                ResetTile(n);
                        }
                    }
                }
            }
        }

        private void ResetTile(Tile t)
        {
            List<TileObject> remove = new List<TileObject>();

            foreach (TileObject tileObject in t.TileContainer.TileObjects)
            {
                if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                {
                    remove.Add(tileObject);
                    if (!changedGroundulongs.ContainsKey(t.Pos))
                        changedGroundulongs.Add(t.Pos, null);
                }
            }
            foreach (TileObject tileObject in remove)
            {
                Map.AddOpenTileObject(tileObject);
                t.TileContainer.Remove(tileObject);
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
            int seed;
            if (gameModel.Seed.HasValue)
                seed = gameModel.Seed.Value;
            else
                seed = (int)DateTime.Now.Ticks;
            Init(gameModel, seed);
        }

        public Game(GameModel gameModel, int seed)
        {
            Init(gameModel, seed);
        }

        public Tile GetTile(ulong p)
        {
            return Map.GetTile(p);
        }

        public Pheromones Pheromones { get; set; }

        
        public void ComputePossibleMoves(ulong pos, List<Move> possibleMoves, List<ulong> includedulongs, MoveFilter moveFilter)
        {
            Tile t = Map.GetTile(pos);
            if (t != null && t.Unit != null)
            {
                t.Unit.ComputePossibleMoves(possibleMoves, includedulongs, moveFilter);
            }
        }

        public List<ulong> FindPath(ulong from, ulong to, Unit unit)
        {
            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            return pathFinder.FindPath(unit, from, to);
        }

        public Move MoveTo(ulong from, ulong to, Engine engine)
        {
            Tile t = Map.GetTile(from);
            if (t == null || t.Unit == null)
                throw new Exception("errr");

            Unit unit = Map.Units.GetUnitAt(from);
            if (unit.Power == 0)
                return null;

            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            List<ulong> route = pathFinder.FindPath(unit, from, to);
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

        public void UpdateUnitPositions(List<Move> newMoves)
        {
            CollisionCntr++;
            if (CollisionCntr == 446)
            {

            }

            List<Unit> addedUnits = new List<Unit>();

            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Upgrade)
                {
                    Unit factory = Map.Units.GetUnitAt(move.Positions[0]);
                    if (factory == null || factory.Assembler == null)
                    {
                        move.MoveType = MoveType.Skip;
                    }
                    else
                    {
                        Unit newUnit = Map.Units.GetUnitAt(move.Positions[1]);
                        if (newUnit != null && newUnit.UnitId != move.OtherUnitId)
                        {
                            // The unit to upgrade is no longer there
                            newUnit = null;
                        }
                        if (newUnit == null)
                        {
                            Player player = Players[move.PlayerId];

                            if (player.UnitsInBuild.ContainsKey(move.OtherUnitId)) //move.ulongs[1]))
                            {
                                // From under construction to real unit
                                PlayerUnit playerUnit = player.UnitsInBuild[move.OtherUnitId];
                                newUnit = playerUnit.Unit;
                                playerUnit.Unit.IsGhost = false;
                                addedUnits.Add(playerUnit.Unit);
                            }
                        }
                        if (newUnit != null)
                        {
                            TileObject tileObject = factory.Assembler.ConsumeMineralForUnit();
                            if (tileObject != null)
                            {
                                newUnit.Upgrade(move, tileObject);

                                if (!changedUnits.ContainsKey(newUnit.Pos))
                                    changedUnits.Add(newUnit.Pos, newUnit);
                            }
                            else
                            {
                                move.MoveType = MoveType.Skip;
                            }
                        }
                    }
                }
                if (move.MoveType == MoveType.Move || move.MoveType == MoveType.Add || move.MoveType == MoveType.Build)
                {
                    ulong Destination;
                    ulong From;

                    From = move.Positions[0];
                    Destination = move.Positions[move.Positions.Count - 1];

                    Unit thisUnit;

                    thisUnit = Map.Units.GetUnitAt(From);
                    if (move.MoveType == MoveType.Move && move.Stats == null)
                        move.Stats = thisUnit.CollectStats();

                    if (move.MoveType == MoveType.Add || move.MoveType == MoveType.Build)
                    {
                        if (move.MoveType == MoveType.Build)
                        {
                            Unit factory = Map.Units.GetUnitAt(move.Positions[0]);
                            if (factory == null || factory.Assembler == null)
                            {
                                move.MoveType = MoveType.Skip;
                                continue;
                            }
                        }

                        // New units will be added here
                        //int? containerNetal = null;

                        /*
                        if (move.UnitId.Contains(":Remove"))
                        {
                            bool markForExtraction = false;
                            // 
                            if (move.UnitId.EndsWith("Reactor"))
                                thisUnit.Reactor = null;
                            else if (move.UnitId.EndsWith("Engine"))
                            {
                                markForExtraction = true;
                                thisUnit.Engine = null;
                            }
                            else if (move.UnitId.EndsWith("Container"))
                            {
                                containerNetal = thisUnit.Container.Mineral;
                                thisUnit.Container = null;
                            }
                            else
                                throw new Exception();
                            move.UnitId = move.UnitId.Replace("Remove", "");
                            thisUnit.ExtractMe = markForExtraction;
                        }*/
                        thisUnit = new Unit(this, move.UnitId);

                        //if (thisUnit.Container != null && containerNetal.HasValue)
                        ///    thisUnit.Container.Mineral = containerNetal.Value;

                        thisUnit.Power = 20;
                        thisUnit.MaxPower = 20;
                        if (move.MoveType == MoveType.Add)
                        {
                            thisUnit.CreateAllPartsFromBlueprint();
                        }

                        if (move.MoveType == MoveType.Build)
                        {
                            thisUnit.IsGhost = true;
                            thisUnit.UnderConstruction = true;
                        }
                        move.UnitId = thisUnit.UnitId;
                        thisUnit.Pos = Destination;
                        if (thisUnit.Engine != null && move.Positions.Count > 1)
                            thisUnit.Direction = CubePosition.CalcDirection(move.Positions[0], move.Positions[1]);
                        else
                            thisUnit.Direction = Direction.C;
                        move.Stats = thisUnit.CollectStats();

                        if (move.PlayerId > 0)
                            thisUnit.Owner = Players[move.PlayerId];
                    }
                    else
                    {
                        // Remove moving unit from map
                        if (thisUnit.Engine != null && move.Positions.Count > 1)
                            thisUnit.Direction = CubePosition.CalcDirection(move.Positions[0], move.Positions[1]);
                        else
                            thisUnit.Direction = Direction.C;
                        move.Stats.Direction =(int)thisUnit.Direction;
                        Map.Units.Remove(From);
                    }

                    if (thisUnit == null)
                        throw new Exception("wrong");

                    // Update new pos
                    thisUnit.Pos = Destination;
                    addedUnits.Add(thisUnit);

                    Tile t = Map.GetTile(Destination);
                    if (t.RemoveBio())
                    {
                        if (!changedGroundulongs.ContainsKey(Destination))
                            changedGroundulongs.Add(Destination, null);
                    }

                    // is there a unit on the map?
                    /* Could be, but may move later so no error
                    Unit otherUnit = Map.Units.GetUnitAt(Destination);
                    if (otherUnit != null)
                    {
                        throw new Exception("Handle collisions failed");
                    }*/
                }
            }
            
            foreach (Unit addedUnit in addedUnits)
            {
                if (addedUnit.Pos != Position.Null)
                {
                    if (addedUnit.IsGhost)
                    {
                        if (addedUnit.Owner.UnitsInBuild.ContainsKey(addedUnit.UnitId))
                        {
                            // Duplicate build order
                            addedUnit.Owner.UnitsInBuild.Remove(addedUnit.UnitId);
                        }
                        PlayerUnit playerUnit = new PlayerUnit(addedUnit);
                        addedUnit.Owner.UnitsInBuild.Add(addedUnit.UnitId, playerUnit);
                    }
                    else
                    {
                        if (Map.Units.GetUnitAt(addedUnit.Pos) == null)
                            Map.Units.Add(addedUnit);
                    }
                }
            }
        }

        private void ProcessLastMoves()
        {
            List<Move> nextMoves = new List<Move>();

            List<Move> finishedMoves = new List<Move>();
            foreach (Move move in lastMoves)
            {
                if (move.MoveType == MoveType.Add ||
                    move.MoveType == MoveType.Build ||
                    move.MoveType == MoveType.Assemble ||
                    move.MoveType == MoveType.Upgrade ||
                    move.MoveType == MoveType.Hit ||
                    move.MoveType == MoveType.UpdateGround ||
                    move.MoveType == MoveType.Skip ||
                    move.MoveType == MoveType.UpdateStats ||
                    move.MoveType == MoveType.CommandComplete ||
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
                    if (move.Stats != null &&
                        move.Stats.MoveUpdateGroundStat != null &&
                        move.Stats.MoveUpdateGroundStat.TileObjects != null)
                    {
                        // Insert the previously removed tileobjects into the units
                        Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                        if (unit != null && unit.Extractor != null)
                        {
                            List<TileObject> tileObjects = new List<TileObject>();
                            foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
                            {
                                if (TileObject.CanConvertTileObjectIntoMineral(tileObject.TileObjectType))
                                {
                                    TileObject newTileObject = new TileObject();
                                    newTileObject.TileObjectType = TileObjectType.Mineral;
                                    newTileObject.Direction = tileObject.Direction;
                                    tileObjects.Add(newTileObject);

                                    //tileObject.TileObjectType = TileObjectType.Mineral;
                                    //tileObject.Direction = Direction.C;
                                    //tileObjects.Add(tileObject);
                                }
                                else
                                {
                                    tileObjects.Add(tileObject);
                                }
                            }
                            unit.AddTileObjects(tileObjects);

                            if (tileObjects.Count > 0)
                            {
                                ulong from = move.Positions[move.Positions.Count - 1];
                                Tile fromTile = Map.GetTile(from);

                                foreach (TileObject tileObject in tileObjects)
                                {
                                    // Drop Minerals on the floor, distribute anything else on the map
                                    // (No Trees in Buildings)
                                    if (tileObject.TileObjectType == TileObjectType.Mineral)
                                        fromTile.TileContainer.Add(tileObject);
                                    else
                                        Map.AddOpenTileObject(tileObject);
                                }
                                Move updateGroundMove = new Move();
                                updateGroundMove.MoveType = MoveType.UpdateGround;
                                updateGroundMove.Positions = new List<ulong>();
                                updateGroundMove.Positions.Add(from);
                                Map.CollectGroundStats(from, updateGroundMove);
                                nextMoves.Add(updateGroundMove);
                            }

                            // Insert an update move, so the client knows that tileobjects have been added
                            Move moveUpdate = new Move();
                            moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                            moveUpdate.MoveType = MoveType.UpdateStats;
                            moveUpdate.UnitId = unit.UnitId;
                            moveUpdate.Positions = new List<ulong>();
                            moveUpdate.Positions.Add(unit.Pos);
                            moveUpdate.Stats = unit.CollectStats();
                            nextMoves.Add(moveUpdate);
                        }
                    }
                    finishedMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Transport)
                {
                    ulong transportTargetPos = move.Positions[move.Positions.Count - 1];
                    Unit unit = Map.Units.GetUnitAt(transportTargetPos);

                    if (unit == null)
                    {
                        // Target died, transport to ground
                        Tile unitTile = GetTile(transportTargetPos);

                        if (!changedGroundulongs.ContainsKey(transportTargetPos))
                            changedGroundulongs.Add(transportTargetPos, null);
                    }
                    else
                    {
                        // Add transported items
                        unit.AddTileObjects(move.Stats.MoveUpdateGroundStat.TileObjects);

                        // Insert an update move, so the client knows that tileobjects have been added
                        Move moveUpdate = new Move();
                        moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                        moveUpdate.MoveType = MoveType.UpdateStats;
                        moveUpdate.UnitId = unit.UnitId;
                        moveUpdate.Positions = new List<ulong>();
                        moveUpdate.Positions.Add(unit.Pos);
                        moveUpdate.Stats = unit.CollectStats();
                        nextMoves.Add(moveUpdate);
                    }
                    finishedMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Fire)
                {
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
            // Compute the damage of the fire shots in the last round
            //foreach (Bullet bullet in hitByBullet)
            {
               // HitByBullet(bullet, lastMoves);

                /* Hit area
                Tile targetTile = GetTile(bullet.Target);
                foreach (Tile n in targetTile.Neighbors)
                {
                    bullet.Target = n.Pos;
                    HitByBullet(bullet, lastMoves);
                }*/
            }
        }

        internal void HitByBullet(Move move, List<Move> nextMoves)
        {
            ulong pos = move.Positions[move.Positions.Count-1];
            Tile targetTile = GetTile(pos);

            TileObject tileObject = move.Stats.MoveUpdateGroundStat.TileObjects[0];
            targetTile.HitByBullet(tileObject);

            if (!changedGroundulongs.ContainsKey(pos))
                changedGroundulongs.Add(pos, null);

            Unit targetUnit = targetTile.Unit;
            if (targetUnit != null)
            {
                Ability hitPart = targetUnit.HitBy();
                if (hitPart is Shield)
                {
                    if (!changedUnits.ContainsKey(pos))
                        changedUnits.Add(pos, targetUnit);
                }
                else
                {
                    // Unit was hit
                    Move hitmove = new Move();
                    hitmove.MoveType = MoveType.Hit;
                    hitmove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                    hitmove.Positions = move.Positions;
                    hitmove.UnitId = targetUnit.UnitId;
                    hitmove.OtherUnitId = hitPart.PartType.ToString();

                    hitmove.Stats = new MoveUpdateStats();
                    hitmove.Stats.MoveUpdateGroundStat = move.Stats.MoveUpdateGroundStat;

                    nextMoves.Add(hitmove);

                    if (hitPart.TileContainer != null)
                    {
                        //foreach (TileObject unitTileObject in hitPart.TileContainer.TileObjects)
                        while (hitPart.TileContainer.Count > hitPart.TileContainer.Capacity)
                        {
                            TileObject toDrop = hitPart.TileContainer.RemoveTileObject(TileObjectType.All);

                            // Anything but minerals are distributed
                            if (toDrop.TileObjectType != TileObjectType.Mineral)
                            {
                                Map.AddOpenTileObject(tileObject);
                            }
                            else
                            {
                                targetTile.TileContainer.Add(tileObject);
                            }
                        }
                    }
                    
                    TileObject hitPartTileObject = hitPart.PartTileObjects[0];
                    hitPart.PartTileObjects.Remove(hitPartTileObject);

                    // Part turns into mineral on ground
                    hitPartTileObject.TileObjectType = TileObjectType.Mineral;
                    hitPartTileObject.Direction = Direction.C;
                    targetTile.TileContainer.Add(hitPartTileObject);

                    if (targetUnit.IsDead())
                    {
                        if (hitPart.PartTileObjects.Count > 0)
                            throw new Exception();

                        // Unit has died!
                        Move deleteMove = new Move();
                        deleteMove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                        deleteMove.MoveType = MoveType.Delete;
                        deleteMove.Positions = new List<ulong>();
                        deleteMove.Positions.Add(targetUnit.Pos);
                        deleteMove.UnitId = targetUnit.UnitId;
                        nextMoves.Add(deleteMove);

                        Map.Units.Remove(targetUnit.Pos);
                    }
                }
            }
            else
            {
                // Ground was hit
                Move hitmove = new Move();
                hitmove.MoveType = MoveType.Hit;
                hitmove.Positions = move.Positions;

                hitmove.Stats = new MoveUpdateStats();                
                hitmove.Stats.MoveUpdateGroundStat = move.Stats.MoveUpdateGroundStat;

                nextMoves.Add(hitmove);
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
                        if (move.MoveType == MoveType.CommandComplete || move.MoveType == MoveType.Transport)
                        {
                        }
                        else if (move.MoveType == MoveType.Build || move.MoveType == MoveType.Add)
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
                            else if (move.MoveType == MoveType.Transport)
                            {
                                Unit unit = Map.Units.GetUnitAt(move.Positions[0]);

                                if (unit.Container == null)
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
                            else if (move.MoveType != MoveType.Upgrade)
                            {
                                if (move.UnitId != null &&
                                    move.UnitId.StartsWith("unit"))
                                {
                                    if (unitsThatMoved.ContainsKey(move.UnitId))
                                        throw new Exception("Cheater");
                                    unitsThatMoved.Add(move.UnitId, move);
                                }
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
                    if (!unit.IsGhost)
                    {
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
                            throw new Exception("player has different unit");
                            //can happen if unit to be upgraded has moved away
                        }
                        if (unit.Owner.PlayerModel.Id != mapUnit.Owner.PlayerModel.Id)
                        {
                            throw new Exception("wrong player");
                        }
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
                    ulong p = move.Positions[cnt-1];
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
                if (move.MoveType == MoveType.Build)
                {
                    int cnt = move.Positions.Count;
                    ulong p = move.Positions[cnt - 1];
                    /*
                    PlayerUnit playerUnit = player.UnitsInBuild[p];
                    if (playerUnit == null)
                    {
                        throw new Exception("unit not on map");
                    }
                    if (move.PlayerId != 0 && playerUnit.Unit.Owner.PlayerModel.Id != move.PlayerId)
                    {
                        throw new Exception("wrong player");
                    }*/
                }
                if (move.MoveType == MoveType.Move)
                {
                    ulong p = move.Positions[0];
                    if (move.Positions.Count > 1)
                    {
                        p = move.Positions[move.Positions.Count-1];
                        Unit unit = Map.Units.GetUnitAt(p);
                        if (unit == null)
                        {
                            /// must not, can be killed after moving there
                            //throw new Exception("unit not moved there");
                        }
                        else
                        {
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
                }

                if (move.MoveType == MoveType.Delete)
                {
                    int cnt = move.Positions.Count;
                    ulong p = move.Positions[cnt - 1];
                    Unit unit = Map.Units.GetUnitAt(p);
                    if (unit != null)
                    {
                        if (unit.UnitId == move.UnitId)
                            throw new Exception("unit not deleted");
                    }
                }
            }
        }

        /// <summary>
        /// All units have been put at their final destination after the moves.
        /// </summary>

        private void ProcessNewMoves()
        {
            lastMoves.Clear();

            // Move all units to their new location
            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.UpdateGround)
                {
                    lastMoves.Add(move);
                }
                if (move.MoveType == MoveType.UpdateStats)
                {
                    lastMoves.Add(move);
                }
                if (move.MoveType == MoveType.Hit)
                {
                    lastMoves.Add(move);
                }
                if (move.MoveType == MoveType.Delete)
                {
                    lastMoves.Add(move);
                }
                if (move.MoveType == MoveType.Move)
                {
                    lastMoves.Add(move);
                }
            }

            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Build || move.MoveType == MoveType.Add)
                {
                    lastMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Upgrade)
                {
                    lastMoves.Add(move);
                }
                
            }

            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Extract)
                {
                    Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                    if (unit != null && unit.Extractor != null)
                    {
                        bool extracted = false;

                        ulong fromPos = move.Positions[move.Positions.Count - 1];
                        Tile fromTile = Map.GetTile(fromPos);

                        Unit otherUnit = null;
                        TileObject tileObject = null;
                        if (move.OtherUnitId.StartsWith("unit"))
                        {
                            otherUnit = fromTile.Unit;
                            if (otherUnit == null || otherUnit.UnitId != move.OtherUnitId)
                            {
                                // Extract from unit, but no longer there or not from this unit
                                move.MoveType = MoveType.Skip;
                            }
                            
                        }
                        else
                        {

                            tileObject = move.Stats.MoveUpdateGroundStat.TileObjects[0];
                        }
                        if (move.MoveType != MoveType.Skip)
                        {
                            extracted = unit.Extractor.ExtractInto(unit, move, fromTile, this, otherUnit, tileObject);

                            if (extracted)
                            {
                                if (!changedGroundulongs.ContainsKey(fromPos))
                                    changedGroundulongs.Add(fromPos, null);

                                if (otherUnit != null && !changedUnits.ContainsKey(otherUnit.Pos))
                                    changedUnits.Add(otherUnit.Pos, otherUnit);

                                lastMoves.Add(move);

                                if (otherUnit != null && otherUnit.IsDead())
                                {
                                    // Unit has died!
                                    Move deleteMove = new Move();
                                    deleteMove.PlayerId = otherUnit.Owner.PlayerModel.Id;
                                    deleteMove.MoveType = MoveType.Delete;
                                    deleteMove.Positions = new List<ulong>();
                                    deleteMove.Positions.Add(otherUnit.Pos);
                                    deleteMove.UnitId = otherUnit.UnitId;
                                    lastMoves.Add(deleteMove);

                                    Map.Units.Remove(otherUnit.Pos);
                                }

                            }
                            else
                            {
                                // cloud not extract, ignore move
                                move.MoveType = MoveType.Skip;
                            }
                        }
                    }
                    else
                    {
                        // move failed, no unit or no extractor
                        move.MoveType = MoveType.Skip;
                    }

                }
                else if (move.MoveType == MoveType.Fire)
                {
                    Unit fireingUnit = Map.Units.GetUnitAt(move.Positions[0]);
                    if (fireingUnit != null && fireingUnit.Weapon != null)
                    {
                        List<TileObject> removedTileObjects = new List<TileObject>();

                        if (fireingUnit.Weapon.EndlessAmmo)
                        {
                            TileObject tileObject = new TileObject();
                            tileObject.TileObjectType = TileObjectType.Mineral;
                            removedTileObjects.Add(tileObject);
                        }
                        else
                        {
                            fireingUnit.RemoveTileObjects(removedTileObjects, 1, TileObjectType.All, null);
                        }
                        if (removedTileObjects.Count > 0)
                        {
                            move.Stats = fireingUnit.CollectStats();
                            move.Stats.MoveUpdateGroundStat = new MoveUpdateGroundStat();
                            move.Stats.MoveUpdateGroundStat.TileObjects = new List<TileObject>();
                            move.Stats.MoveUpdateGroundStat.TileObjects.AddRange (removedTileObjects);
                            
                            if (!changedUnits.ContainsKey(fireingUnit.Pos))
                                changedUnits.Add(fireingUnit.Pos, fireingUnit);

                            lastMoves.Add(move);

                            HitByBullet(move, lastMoves);
                        }
                    }
                }
                else if (move.MoveType == MoveType.Transport)
                {
                    Unit sendingUnit = Map.Units.GetUnitAt(move.Positions[0]);
                    if (sendingUnit != null && sendingUnit.Container != null)
                    {
                        TileObject tileObject = sendingUnit.Container.TileContainer.RemoveTileObject(TileObjectType.Mineral);
                        if (tileObject != null)
                        {
                            //move.Stats = sendingUnit.CollectStats();
                            move.Stats = new MoveUpdateStats();
                            move.Stats.MoveUpdateGroundStat = new MoveUpdateGroundStat();
                            move.Stats.MoveUpdateGroundStat.TileObjects = new List<TileObject>();
                            move.Stats.MoveUpdateGroundStat.TileObjects.Add(tileObject);
                        }
                        else
                        {
                            move.MoveType = MoveType.Skip;
                        }

                        if (!changedUnits.ContainsKey(sendingUnit.Pos))
                            changedUnits.Add(sendingUnit.Pos, sendingUnit);
                    }
                    if (move.MoveType != MoveType.Skip)
                        lastMoves.Add(move);
                }
            }
            newMoves.Clear();
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

        private void ConsumePower(Player player, List<Move> newMoves)
        {
            List<Unit> reactors = new List<Unit>();
            int totalNumberOfUnits = 0;

            // Collect reactors and consume power for every unit
            foreach (Unit unit in Map.Units.List.Values)
            {
                if (unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                    continue;

                // Only own units
                if (unit.Reactor != null && unit.Engine == null)
                {
                    reactors.Add(unit);
                }
                
                if (unit.Power > 0)
                {
                    if (!unit.EndlessPower)
                        unit.Power--;
                    totalNumberOfUnits++;

                    if (!changedUnits.ContainsKey(unit.Pos))
                        changedUnits.Add(unit.Pos, unit);
                }
                else
                {
                    if (unit.Owner != NeutralPlayer)
                    {
                        // Unpowered unit
                        unit.Owner = NeutralPlayer;

                        if (!changedUnits.ContainsKey(unit.Pos))
                            changedUnits.Add(unit.Pos, unit);
                    }
                }
            }

            // Collect the total out power of all reactors
            int totalAvailablePower = 0;
            int totalStoredPower = 0;
            foreach (Unit reactor in reactors)
            {
                totalAvailablePower += reactor.Reactor.AvailablePower;
                totalStoredPower += reactor.Reactor.StoredPower;
            }

            int totalPowerRemoved = 0;

            if (mapInfo.PlayerInfo.ContainsKey(player.PlayerModel.Id) && totalNumberOfUnits > 0) 
            {
                MapPlayerInfo mapPlayerInfo = mapInfo.PlayerInfo[player.PlayerModel.Id];
                mapPlayerInfo.TotalPower = totalStoredPower + totalAvailablePower;

                // Recharge units
                bool allUnitsCharged = false;
                while (totalAvailablePower > 0 && !allUnitsCharged)
                {
                    allUnitsCharged = true;
                    int maxPowerPerUnit = (totalAvailablePower / totalNumberOfUnits) + 1;

                    foreach (Unit unit in Map.Units.List.Values)
                    {
                        if (unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                            continue;

                        if (!mapInfo.Pheromones.ContainsKey(unit.Pos))
                            continue;

                        bool canCharge = false;
                        MapPheromone mapPheromone = mapInfo.Pheromones[unit.Pos];
                        foreach (MapPheromoneItem pheromoneItem in mapPheromone.PheromoneItems)
                        {
                            if (pheromoneItem.PlayerId == player.PlayerModel.Id &&
                                pheromoneItem.PheromoneType == PheromoneType.Energy)
                            {
                                canCharge = true;
                                break;
                            }

                        }
                        if (!canCharge)
                            continue;

                        if (unit.Armor != null)
                        {
                            int chargedPower = unit.Armor.LoadShield(totalAvailablePower);
                            totalAvailablePower -= chargedPower;
                            totalPowerRemoved += chargedPower;
                        }

                        // Only own units
                        if (unit.Power < unit.MaxPower)
                        {
                            int chargedPower = maxPowerPerUnit;

                            if (unit.Power + maxPowerPerUnit > unit.MaxPower)
                            {
                                chargedPower = unit.MaxPower - unit.Power;
                            }
                            unit.Power += chargedPower;
                            if (unit.Power < unit.MaxPower)
                            {
                                allUnitsCharged = false;
                            }
                            totalAvailablePower -= chargedPower;
                            totalPowerRemoved += chargedPower;
                        }
                    }
                }
            }

            // In low power, take the power from the shield
            foreach (Unit unit in Map.Units.List.Values)
            {
                if (unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                    continue;

                if (unit.Power < (unit.MaxPower/2) && unit.Armor != null && unit.Armor.ShieldPower > 0)
                {
                    unit.Power++;
                    unit.Armor.ShieldPower--;
                    unit.Armor.ShieldActive = false;
                }
            }

            int att = 100;

            while (totalPowerRemoved > 0 && att-- > 0)
            {
                // Consume the charged power from the reactors
                int removePowerFromEachReactor = (totalPowerRemoved / reactors.Count) + 1;
                foreach (Unit reactor in reactors)
                {
                    int powerConsumed = reactor.Reactor.ConsumePower(removePowerFromEachReactor);
                    totalPowerRemoved -= powerConsumed;
                }
            }
            
        }

        private void HandleCollisions(List<Move> newMoves)
        {
            CollisionCntr++;
            if (CollisionCntr == 127)
            {

            }

            bool somethingChanged = true;
            List<Move> acceptedMoves = new List<Move>();
            Dictionary<ulong, Move> moveToTargets = new Dictionary<ulong, Move>();

            while (somethingChanged)
            {
                somethingChanged = false;
                

                // Remove moves that go to the same destination
                
                foreach (Move move in newMoves)
                {
                    // Any position that is currently being upgraded or a unit turns from ghost into real
                    // cannot be stepped on
                    if (move.MoveType == MoveType.Upgrade || move.MoveType == MoveType.Build)
                    {
                        ulong destination = move.Positions[move.Positions.Count - 1];
                        if (moveToTargets.ContainsKey(destination))
                        {
                            // Cannot execute move
                            Tile t = Map.GetTile(destination);
                            if (t.Unit != null)
                            {
                                // Double upgrade, its ok
                            }
                            else
                            {
                                // Double upgrade, its ok
                            }
                        }
                        else
                        {
                            moveToTargets.Add(destination, move);
                        }
                    }
                }

                foreach (Move move in newMoves)
                {
                    if (move.MoveType == MoveType.Move || move.MoveType == MoveType.Add) // || move.MoveType == MoveType.Build)
                    {
                        ulong destination = move.Positions[move.Positions.Count - 1];
                        if (moveToTargets.ContainsKey(destination))
                        {
                            // (Hit) Could do nasty things, but for now, the unit does not move
                            newMoves.Remove(move);
                            somethingChanged = true;

                            moveToTargets.Clear();
                            acceptedMoves.Clear();
                            break;
                        }
                        else
                        {
                            moveToTargets.Add(destination, move);
                        }
                    }
                    else if (move.MoveType == MoveType.Upgrade)
                    {

                    }
                    else
                    {
                        acceptedMoves.Add(move);
                    }
                }
            }
            List<Move> revokedMoves = new List<Move>();
            
            foreach (Move move in moveToTargets.Values)
            {
                if (/*move.MoveType == MoveType.Upgrade ||*/ move.MoveType == MoveType.Build)
                    continue;

                ulong from = move.Positions[0];
                ulong destination = move.Positions[move.Positions.Count - 1];
                Tile t = Map.GetTile(destination);
                if (t == null)
                {
                    // Moved outside?
                    revokedMoves.Add(move);
                }
                else if (!t.CanMoveTo(from))
                {
                    // Move to invalid pos
                    // Happend with bad startup pos
                    //throw new Exception("how dare you");
                    //moveToTargets.Remove(move.Positions[move.Positions.Count - 1]);
                    //somethingChanged = true;
                    //break;
                    revokedMoves.Add(move);
                }
                else if (t.Unit != null)
                {
                    bool unitBlocked = true;
                    if (move.MoveType == MoveType.Upgrade)
                    {
                        // Upgrde this unit is ok
                        if (t.Unit.UnitId == move.OtherUnitId)
                        {
                            unitBlocked = false;
                            acceptedMoves.Add(move);
                        }
                    }
                    else
                    {
                        // Move onto another unit? Check if this unit goes away
                        foreach (Move unitMove in moveToTargets.Values)
                        {
                            if (revokedMoves.Contains(unitMove))
                                continue;

                            if ((unitMove.MoveType == MoveType.Move || unitMove.MoveType == MoveType.Build || unitMove.MoveType == MoveType.Add) && t.Unit.UnitId == unitMove.UnitId)
                            {
                                // This unit moves away, so it is ok
                                acceptedMoves.Add(move);
                                unitBlocked = false;
                                break;
                            }
                        }
                    }
                    if (unitBlocked == true)
                    {
                        revokedMoves.Add(move);
                        // (Hit) Could do nasty things, but for now, the unit does not move
                        //somethingChanged = true;
                    }
                    else
                    {
                        //
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

        internal Dictionary<ulong, Tile> changedGroundulongs = new Dictionary<ulong, Tile>();
        internal Dictionary<ulong, Unit> changedUnits = new Dictionary<ulong, Unit>();
        private Player NeutralPlayer;

        private void CreateTileObjects(int attempts)
        {
            foreach (MapZone mapZone in Map.Zones.Values)
            {
                if (!mapZone.UnderwaterTilesCreated)
                {
                    mapZone.UnderwaterTilesCreated = true;

                    foreach (Tile tile in mapZone.Tiles.Values)
                    {
                        if (mapZone.IsUnderwater)
                        {
                            tile.IsUnderwater = true;

                            TileObject tileObject = new TileObject();
                            tileObject.TileObjectType = TileObjectType.Water;
                            tileObject.Direction = Direction.C;
                            tile.TileContainer.Add(tileObject);
                        }
                        if (!changedGroundulongs.ContainsKey(tile.Pos))
                            changedGroundulongs.Add(tile.Pos, null);
                    }
                }
            }
            bool freespace = true;
            while (freespace && attempts-- > 0)
            {
                bool oneSpace = false;

                foreach (MapZone mapZone in Map.Zones.Values)
                {
                    ulong pos = mapZone.CreateTerrainTile(Map);
                    if (pos != Position.Null)
                    {
                        oneSpace = true;
                        if (!changedGroundulongs.ContainsKey(pos))
                            changedGroundulongs.Add(pos, null);
                    }
                    else
                    {

                    }
                }
                if (!oneSpace)
                    freespace = false;
            }
        }

        private void AddChangedGroundInfoMoves(List<Move> moves)
        {
            foreach (ulong pos in changedGroundulongs.Keys)
            {
                Move hitmove = new Move();
                hitmove.MoveType = MoveType.UpdateGround;
                hitmove.Positions = new List<ulong>();
                hitmove.Positions.Add(pos);
                Map.CollectGroundStats(pos, hitmove);
                moves.Add(hitmove);
            }
            changedGroundulongs.Clear();
        }

        public List<Move> ProcessMove(int playerId, Move myMove, List<MapGameCommand> gameCommands)
        {
            List<Move> returnMoves = new List<Move>();
            lock (GameModel)
            {
                if (myMove != null && myMove.MoveType == MoveType.UpdateAll)
                {
                    UpdateAll(playerId, returnMoves);
                    return returnMoves;
                }

                if (MoveNr == 367)
                {
                    int x = 0;
                }

                changedUnits.Clear();
                changedGroundulongs.Clear();


                /*
                MapInfo mapInfo1 = new MapInfo();
                mapInfo1.ComputeMapInfo(this, null);

                if (mapInfo != null && mapInfo1.TotalMetal != mapInfo.TotalMetal)
                {
                    int x = 0;
                }
                */
                bool first = false;
                if (!initialized)
                {
                    first = true;

                    CreateTileObjects(999);
                    AddChangedGroundInfoMoves(newMoves);
                    Initialize(newMoves);
                }
                else
                {
                    // Replace mins
                    /*
                    foreach (TileObject tileObject in Map.OpenTileObjects)
                    {
                        if (tileObject.TileObjectType == TileObjectType.Mineral)
                        {
                            Map.DistributeTileObject(tileObject);
                            Map.OpenTileObjects.Remove(tileObject);
                            break;
                        }
                    }*/

                    // Place tile objects (For Debug)
                    CreateTileObjects(1);
#if DEBUG
                    Validate(lastMoves);
#endif

                    if (lastMoves.Count > 0)
                    {
                        //MapInfo mapInfoLast = new MapInfo();
                        //mapInfoLast.ComputeMapInfo(this, lastMoves);

                        // Remove moves that have been processed
                        ProcessLastMoves();

                        /*
                        MapInfo mapInfoLast1 = new MapInfo();
                        mapInfoLast1.ComputeMapInfo(this, lastMoves);

                        if (mapInfoLast.TotalMetal != mapInfoLast1.TotalMetal)
                        {
                            int x = 0;
                        }*/

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

                if (gameCommands != null)
                {
                    foreach (MapGameCommand mapGameCommand in gameCommands)
                    {
                        GameCommand gameCommand = new GameCommand();

                        foreach (string id in mapGameCommand.AttachedUnits)
                            gameCommand.AttachedUnits.Add(id);

                        gameCommand.BlueprintCommand = mapGameCommand.BlueprintCommand.Copy();
                        gameCommand.DeleteWhenFinished = mapGameCommand.DeleteWhenFinished;
                        gameCommand.CommandCanceled = mapGameCommand.CommandCanceled;
                        gameCommand.CommandComplete = mapGameCommand.CommandComplete;
                        gameCommand.GameCommandType = mapGameCommand.GameCommandType;
                        gameCommand.MoveToPosition = mapGameCommand.MoveToPosition;
                        gameCommand.PlayerId = mapGameCommand.PlayerId;
                        gameCommand.TargetPosition = mapGameCommand.TargetPosition;
                        gameCommand.TargetZone = mapGameCommand.TargetZone;
                        gameCommand.WaitingForUnit = mapGameCommand.WaitingForUnit;

                        Player player = Players[mapGameCommand.PlayerId];
                        if (mapGameCommand.CommandComplete)
                        {
                            player.GameCommands.Remove(gameCommand);
                        }
                        else
                        {
                            player.GameCommands.Add(gameCommand);

                            if (mapGameCommand.GameCommandType == GameCommandType.Extract)
                            {
                                Unit unit = Map.Units.FindUnit(mapGameCommand.UnitId);
                                if (unit != null)
                                    unit.ExtractUnit();
                            }
                        }
                    }
                }
                Pheromones.Evaporate();

                if (!first && lastMoves.Count == 0)
                {
                    // New move
                    bool allPlayersMoved = CollectNewMoves(myMove);
                    if (!allPlayersMoved)
                    {
                        return lastMoves;
                    }
                }

                if (first)
                {
                    lastMoves.AddRange(newMoves);
                    newMoves.Clear();
                }
                else
                {
                    //MapInfo mapInfoPrev = new MapInfo();
                    //mapInfoPrev.ComputeMapInfo(this, newMoves);

                    LogMoves("Process new Moves " + Seed, MoveNr, newMoves);

                    // Check collisions and change moves if units collide or get destroyed
                    HandleCollisions(newMoves);

                    LogMoves("New Moves after Collisions", MoveNr, newMoves);

                    UpdateUnitPositions(newMoves);

                    ProcessNewMoves();
                }
                mapInfo = new MapInfo();
                mapInfo.ComputeMapInfo(this, lastMoves);
                
                //if (mapInfoPrev.TotalMetal != mapInfo.TotalMetal)
                {

                }

                foreach (Player player in Players.Values)
                {
                    /*
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
                    }*/
                    ConsumePower(player, lastMoves);
                }


                ProcessBorders();

                foreach (Unit unit in changedUnits.Values)
                {
                    Move moveUpdate = new Move();
                    moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                    moveUpdate.MoveType = MoveType.UpdateStats;
                    moveUpdate.UnitId = unit.UnitId;
                    moveUpdate.Positions = new List<ulong>();
                    moveUpdate.Positions.Add(unit.Pos);
                    moveUpdate.Stats = unit.CollectStats();
                    lastMoves.Add(moveUpdate);
                }

                // Add changed ground info
                AddChangedGroundInfoMoves(lastMoves);

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

                //CreateAreas();
                if (playerId == 0)
                {
                    returnMoves = lastMoves;
#if DEBUG
                    Validate(returnMoves);
#endif
                }


                /*MapInfo mapInfo2 = new MapInfo();
                mapInfo2.ComputeMapInfo(this);

                if (mapInfo1.TotalMetal != mapInfo2.TotalMetal)
                {
                    //int x = 0;
                }*/
            }
            if (lastMoves.Count >= 0)
            {
                //OutgoingMoves.Add(MoveNr, lastMoves);
                MoveNr++;
            }
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
        private MapInfo mapInfo;

        public MapInfo GetDebugMapInfo()
        {
            return mapInfo;
        }

        private List<ulong> updatedulongs = new List<ulong>();
        private void ProcessBorders()
        {
            List<ulong> newUpdatedulongs = new List<ulong>();

            foreach (ulong pos in mapInfo.Pheromones.Keys)
            {
                newUpdatedulongs.Add(pos);
                updatedulongs.Remove(pos);

                MapPheromone mapPheromone = mapInfo.Pheromones[pos];
                float highestEnergy = -1;
                int highestPlayerId = 0;

                foreach (MapPheromoneItem mapPheromoneItem in mapPheromone.PheromoneItems)
                {
                    if (mapPheromoneItem.PheromoneType == PheromoneType.Energy)
                    {
                        if (mapPheromoneItem.Intensity >= highestEnergy)
                        {
                            highestEnergy = mapPheromoneItem.Intensity;
                            highestPlayerId = mapPheromoneItem.PlayerId;
                        }
                    }
                }

                Tile t = Map.GetTile(pos);
                if (highestEnergy > 0)
                {
                    if (t.Owner != highestPlayerId && !changedGroundulongs.ContainsKey(pos))
                    {
                        changedGroundulongs.Add(pos, null);
                        t.Owner = highestPlayerId;
                    }
                }
                else
                {
                    if (t.Owner != 0 && !changedGroundulongs.ContainsKey(pos))
                    {
                        changedGroundulongs.Add(pos, null);
                        t.Owner = 0;
                    }
                }
            }
            foreach (ulong pos in updatedulongs)
            {
                Tile t = Map.GetTile(pos);
                if (t.Owner != 0 || t.IsBorder)
                {
                    if (!changedGroundulongs.ContainsKey(pos))
                        changedGroundulongs.Add(pos, null);
                    t.Owner = 0;
                    t.IsBorder = false;
                }
            }
            updatedulongs = newUpdatedulongs;
            
            foreach (ulong pos in newUpdatedulongs)
            {
                Tile t = Map.GetTile(pos);
                bool isBorder = false;
                foreach (Tile n in t.Neighbors)
                {
                    if (n.Owner != t.Owner)
                    {
                        isBorder = true;
                        break;
                    }
                }
                if (t.IsBorder != isBorder)
                {
                    t.IsBorder = isBorder;
                    if (!changedGroundulongs.ContainsKey(pos))
                        changedGroundulongs.Add(pos, null);
                }
            }
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
                    //if (tile.CanMoveTo())
                    //    openList.Add(new TileWithDistance(tile, 0));

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
                    //if (!Visibleulongs.Contains(n.Pos))
                    //    continue;

                    //if (!n.CanMoveTo())
                    //    continue;

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
