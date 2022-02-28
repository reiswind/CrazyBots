//#define MEASURE_TIMINGS
//#define MEASURE_MINS

using Engine.Algorithms;
using Engine.Ants;
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
        public Recipes Recipes { get; private set; }

        public Recipe RecipeForAnyUnit { get; private set; }

        private void Init(GameModel gameModel, int initSeed)
        {
            Blueprints = new Blueprints();
            Pheromones = new Pheromones();
            Recipes = new Recipes();
            foreach (Recipe recipe in Recipes.RecipeList)
            {
                if (recipe.Results.Count == 1 &&
                    recipe.Results[0].TileObjectType == TileObjectType.Unit)
                {
                    RecipeForAnyUnit = recipe;
                    break;
                }
            }

            seed = initSeed;
            Random = new Random(seed);
            MoveNr = 0;
            GameModel = gameModel;
            Map = new Map(this);
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

            Map.GetTile(new Position2(0, 0));

            // TESTEXTRACT

            for (int i = 0; i < Map.DefaultMinerals; i++)
            {
                TileObject tileObject = new TileObject();
                tileObject.Direction = Direction.C;
                tileObject.TileObjectType = TileObjectType.Mineral;

                Map.DistributeTileObject(tileObject);

                tileObject = new TileObject();
                tileObject.Direction = Direction.C;
                tileObject.TileObjectType = TileObjectType.Stone;

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
            // Clear overflow minerals
            Map.ClearExcessMins();

            initialized = true;
        }

        public void CreateUnits()
        {
            foreach (UnitModel unitModel in GameModel.Units)
            {
                Player player = Players[unitModel.PlayerId];

                Position2 posOnMap = Position2.Null;
                if (player.StartZone != null)
                {
                    Position2 posInModel = Position2.ParsePosition(unitModel.Position);
                    posOnMap = new Position2(player.StartZone.Center.X + posInModel.X, player.StartZone.Center.Y + posInModel.Y);
                    //Position.GetX(player.StartZone.Center) + Position.GetX(posInModel),
                    //Position.GetY(player.StartZone.Center) + Position.GetY(posInModel));

                    Tile t = Map.GetTile(posOnMap);
                    if (t != null)
                    {
                        Unit thisUnit = new Unit(this, unitModel.Blueprint);

                        thisUnit.Power = 20;
                        thisUnit.MaxPower = 20;
                        thisUnit.CreateAllPartsFromBlueprint();
                        thisUnit.Pos = posOnMap;
                        thisUnit.Owner = Players[unitModel.PlayerId];

                        if (Map.Units.GetUnitAt(thisUnit.Pos) == null)
                            Map.Units.Add(thisUnit);

                        InitUnitWithModel(thisUnit, unitModel);

                        t.Owner = unitModel.PlayerId;
                        ResetTile(t);
                        foreach (Tile n in t.Neighbors)
                            ResetTile(n);
                    }
                }
            }
        }

        
        private void InitUnitWithModel(Unit thisUnit, UnitModel unitModel)
        {

            if (unitModel.FireAtGround && thisUnit.Weapon != null)
                thisUnit.Weapon.FireAtGround = true;
            if (unitModel.HoldFire && thisUnit.Weapon != null)
                thisUnit.Weapon.HoldFire = true;
            if (unitModel.EndlessAmmo && thisUnit.Weapon != null)
                thisUnit.Weapon.EndlessAmmo = true;
            if (unitModel.EndlessPower)
                thisUnit.EndlessPower = true;
            if (unitModel.MarkForExtraction)
                thisUnit.ExtractUnit();
            if (unitModel.ContainedMinerals.HasValue ||
                unitModel.ContainedWood.HasValue ||
                unitModel.ContainedStones.HasValue)
            {
                if (thisUnit.Container != null && thisUnit.Container.TileContainer != null)
                    thisUnit.Container.TileContainer.Clear();
                if (thisUnit.Weapon != null && thisUnit.Weapon.TileContainer != null)
                    thisUnit.Weapon.TileContainer.Clear();
                if (thisUnit.Reactor != null && thisUnit.Reactor.TileContainer != null)
                    thisUnit.Reactor.TileContainer.Clear();
                if (thisUnit.Assembler != null && thisUnit.Assembler.TileContainer != null)
                    thisUnit.Assembler.TileContainer.Clear();

                if (unitModel.ContainedMinerals.HasValue)
                    thisUnit.FillWithTileObjects(TileObjectType.Mineral, unitModel.ContainedMinerals.Value);
                if (unitModel.ContainedWood.HasValue)
                    thisUnit.FillWithTileObjects(TileObjectType.Wood, unitModel.ContainedWood.Value);
                if (unitModel.ContainedStones.HasValue)
                    thisUnit.FillWithTileObjects(TileObjectType.Stone, unitModel.ContainedStones.Value);
            }
            if (string.IsNullOrEmpty(unitModel.Direction))
                thisUnit.Direction = Direction.C;
            else if (unitModel.Direction == "SW")
                thisUnit.Direction = Direction.SW;
            else if (unitModel.Direction == "SE")
                thisUnit.Direction = Direction.SE;
            else if (unitModel.Direction == "S")
                thisUnit.Direction = Direction.S;
            else if (unitModel.Direction == "NW")
                thisUnit.Direction = Direction.NW;
            else if (unitModel.Direction == "NE")
                thisUnit.Direction = Direction.NE;
            else if (unitModel.Direction == "N")
                thisUnit.Direction = Direction.N;

            
            if (unitModel.AttackPosition && thisUnit.Engine != null)
            {
                GameCommand gameCommand = new GameCommand();
                gameCommand.GameCommandType = GameCommandType.AttackMove;
                gameCommand.GameCommandState = GameCommandState.TargetPositionReached;
                gameCommand.UnitId = thisUnit.UnitId;
                gameCommand.TargetPosition = thisUnit.Pos;
                gameCommand.Direction = thisUnit.Direction;
             
                thisUnit.SetGameCommand(gameCommand);
            }
        }

        public void StartWithFactory(List<Move> newMoves)
        {
            if (GameModel.Units != null)
            {
                foreach (UnitModel unitModel in GameModel.Units)
                {
                    Player player = Players[unitModel.PlayerId];

                    Position2 posOnMap = Position2.Null;
                    if (player.StartZone != null)
                    {
                        //posOnMap = n ew Position2(player.StartZone.Center.X + unitModel.Position2.X, player.StartZone.Center.Y + unitModel.Position2.Y);
                        Position2 posInModel = Position2.ParsePosition(unitModel.Position);
                        posOnMap = new Position2(player.StartZone.Center.X + posInModel.X, player.StartZone.Center.Y + posInModel.Y);
                        //Position.GetX(player.StartZone.Center) + Position.GetX(posInModel),
                        //Position.GetY(player.StartZone.Center) + Position.GetY(posInModel));

                        Tile t = Map.GetTile(posOnMap);
                        if (t != null)
                        {
                            Unit thisUnit = new Unit(this, unitModel.Blueprint);

                            thisUnit.Power = 20;
                            thisUnit.MaxPower = 20;
                            thisUnit.CreateAllPartsFromBlueprint();
                            thisUnit.Pos = posOnMap;

                            // Turn into direction missing
                            thisUnit.Direction = Direction.C; // CalcDirection(move.Position2s[0], move.Position2s[1]);
                            thisUnit.Owner = Players[unitModel.PlayerId];

                            if (Map.Units.GetUnitAt(thisUnit.Pos) == null)
                                Map.Units.Add(thisUnit);

                            InitUnitWithModel(thisUnit, unitModel);
                            
                            Move move = new Move();
                            move.MoveType = MoveType.Add;
                            move.PlayerId = unitModel.PlayerId;
                            move.UnitId = thisUnit.UnitId;
                            move.Stats = thisUnit.CollectStats();
                            move.Positions = new List<Position2>();
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

            foreach (TileObject tileObject in t.TileObjects)
            {
                if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                {
                    remove.Add(tileObject);
                    if (!changedGroundPositions.ContainsKey(t.Pos))
                        changedGroundPositions.Add(t.Pos, null);
                }
            }
            foreach (TileObject tileObject in remove)
            {
                Map.AddOpenTileObject(tileObject);
                t.Remove(tileObject);
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
        internal Random Random { get; private set; }
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

        public Tile GetTile(Position2 p)
        {
            return Map.GetTile(p);
        }

        public Pheromones Pheromones { get; set; }


        public void ComputePossibleMoves(Position2 pos, List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter)
        {
            Tile t = Map.GetTile(pos);
            if (t != null && t.Unit != null)
            {
                t.Unit.ComputePossibleMoves(possibleMoves, includedPosition2s, moveFilter);
            }
        }

        public List<Position2> FindPath(Position2 from, Position2 to, Unit unit, bool ignoreIfToIsOccupied = false)
        {
            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            return pathFinder.FindPath(unit, from, to, ignoreIfToIsOccupied);
        }
        public List<Position2> FindPath(Position2 from, Position2 to, string unitId, bool ignoreIfToIsOccupied = false)
        {
            Unit unit = Map.Units.FindUnit(unitId);
            if (unit == null) return null;

            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            return pathFinder.FindPath(unit, from, to, ignoreIfToIsOccupied);
        }
        /*
        public Move MoveTo(Position2 from, Position2 to, Engine engine)
        {
            Tile t = Map.GetTile(from);
            if (t == null || t.Unit == null)
                throw new Exception("errr");

            Unit unit = Map.Units.GetUnitAt(from);
            if (unit.Power == 0)
                return null;

            PathFinderFast pathFinder = new PathFinderFast(Map);
            pathFinder.IgnoreVisibility = true;
            List<Position2> route = pathFinder.FindPath(unit, from, to);
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
        }*/

        private bool initialized;
        private List<Move> lastMoves = new List<Move>();
        private List<Move> newMoves = new List<Move>();

        public Dictionary<int, Player> Players { get; set; }

        public int MoveNr { get; private set; }
        private int CollisionCntr;

        public void UpdateUnitPositions(List<Move> newMoves)
        {
            CollisionCntr++;
            if (CollisionCntr == 230)
            {

            }

            List<Unit> addedUnits = new List<Unit>();
            List<Unit> deletedUnits = new List<Unit>();

            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Upgrade || move.MoveType == MoveType.Build)
                {
                    Unit factory = Map.Units.GetUnitAt(move.Positions[0]);
                    if (factory == null || factory.Assembler == null)
                    {
                        move.MoveType = MoveType.Skip;
                    }
                    else
                    {
                        List<TileObject> results = factory.ConsumeIngredients(move.MoveRecipe, changedUnits);
                        if (factory.IsDead())
                        {
                            deletedUnits.Add(factory);
                        }
                        if (results == null || results.Count == 0)
                        {
                            move.MoveType = MoveType.Skip;
                        }
                        else
                        {
                            Unit thisUnit;
                            if (move.MoveType == MoveType.Build)
                            {
                                thisUnit = new Unit(this, move.Stats.BlueprintName);
                                if (move.PlayerId > 0)
                                    thisUnit.Owner = Players[move.PlayerId];
                                else
                                    thisUnit.Owner = NeutralPlayer;

                                move.UnitId = thisUnit.UnitId;

                                Position2 Destination = move.Positions[move.Positions.Count - 1];
                                thisUnit.Pos = Destination;
                                if (move.Positions.Count > 1)
                                    thisUnit.Direction = Position3.CalcDirection(move.Positions[0], move.Positions[1]);

                                move.Stats = thisUnit.CollectStats();

                                if (move.GameCommand != null)
                                {
                                    if (move.GameCommand.AssemblerToBuild)
                                    {
                                        // The new unit will be the factory for the build target
                                        move.GameCommand.AttachedUnit.SetUnitId(thisUnit.UnitId);
                                        move.GameCommand.AttachedUnit.SetStatus("AssemblerToBuildCreated");
                                    }
                                    else if (move.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                                    {
                                        move.GameCommand.TransportUnit.SetUnitId(thisUnit.UnitId);
                                        move.GameCommand.TransportUnit.SetStatus("TransporterCreated");
                                    }
                                    else
                                    {
                                        move.GameCommand.AttachedUnit.SetUnitId(thisUnit.UnitId);
                                        move.GameCommand.AttachedUnit.SetStatus("UnitCreated");
                                    }
                                    thisUnit.SetGameCommand(move.GameCommand);
                                    thisUnit.Changed = true;
                                }
                                addedUnits.Add(thisUnit);
                            }
                            else
                            {
                                thisUnit = Map.Units.GetUnitAt(move.Positions[1]);
                                if (thisUnit == null || thisUnit.UnitId != move.OtherUnitId)
                                {
                                    throw new Exception("errr");
                                }
                            }
                            thisUnit.Upgrade(move, results[0]);

                            move.Stats = thisUnit.CollectStats();
                        }
                    }
                }
                if (move.MoveType == MoveType.Move && move.Positions.Count == 1)
                {
                    // Unit turns
                    Unit thisUnit;
                    thisUnit = Map.Units.GetUnitAt(move.Positions[0]);
                    move.Stats = thisUnit.CollectStats();
                }
                else if (move.MoveType == MoveType.Move)
                {
                    Position2 Destination;
                    Position2 From;

                    From = move.Positions[0];
                    Destination = move.Positions[move.Positions.Count - 1];

                    Unit thisUnit;
                    thisUnit = Map.Units.GetUnitAt(From);

                    move.Stats = thisUnit.CollectStats();

                    // Remove moving unit from map
                    if (thisUnit.Engine != null && move.Positions.Count > 1)
                        thisUnit.Direction = Position3.CalcDirection(move.Positions[0], move.Positions[1]);

                    move.Stats.Direction = thisUnit.Direction;
                    Map.Units.Remove(From);

                    // Update new pos
                    thisUnit.Pos = Destination;
                    addedUnits.Add(thisUnit);

                    Tile t = Map.GetTile(Destination);
                    if (t.RemoveBio(false))
                    {
                        if (!changedGroundPositions.ContainsKey(Destination))
                            changedGroundPositions.Add(Destination, null);
                    }
                }
            }

            foreach (Unit deletedUnit in deletedUnits)
            {
                if (deletedUnit.CurrentGameCommand != null)
                {
                    deletedUnit.CurrentGameCommand.CommandComplete = true;
                    deletedUnit.OnDestroyed();
                }
                
                // Unit has died!
                Move deleteMove = new Move();
                deleteMove.PlayerId = deletedUnit.Owner.PlayerModel.Id;
                deleteMove.MoveType = MoveType.Delete;
                deleteMove.Positions = new List<Position2>();
                deleteMove.Positions.Add(deletedUnit.Pos);
                deleteMove.UnitId = deletedUnit.UnitId;
                newMoves.Add(deleteMove);

                Map.Units.Remove(deletedUnit.UnitId);
                Map.Units.Remove(deletedUnit.Pos);                
            }

            foreach (Unit addedUnit in addedUnits)
            {
                if (addedUnit.Pos != Position2.Null)
                {
                    if (Map.Units.GetUnitAt(addedUnit.Pos) == null)
                        Map.Units.Add(addedUnit);
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
                    move.MoveType == MoveType.Burn ||
                    move.MoveType == MoveType.UpdateGround ||
                    move.MoveType == MoveType.Skip ||
                    move.MoveType == MoveType.UpdateStats ||
                    move.MoveType == MoveType.Command ||
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
#if MEASURE_MINS
                    MapInfo mapInfoPrev = new MapInfo();
                    mapInfoPrev.ComputeMapInfo(this, null);
#endif
                    /*
                    if (move.Stats != null && move.MoveRecipe != null)
                    {
                        // Insert the previously removed tileobjects into the unit
                        Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                        if (unit != null && unit.Extractor != null)
                        {
                            unit.AddIngredients(move.MoveRecipe, changedUnits);

                            // Insert an update move, so the client knows that tileobjects have been added
                            Move moveUpdate = new Move();
                            moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                            moveUpdate.MoveType = MoveType.UpdateStats;
                            moveUpdate.UnitId = unit.UnitId;
                            moveUpdate.Positions = new List<Position2>();
                            moveUpdate.Positions.Add(unit.Pos);
                            moveUpdate.Stats = unit.CollectStats();
                            nextMoves.Add(moveUpdate);
                        }
                    }*/
                    /*
                    if (move.Stats != null &&
                        move.Stats.MoveUpdateGroundStat != null &&
                        move.Stats.MoveUpdateGroundStat.TileObjects != null &&
                        move.Stats.MoveUpdateGroundStat.TileObjects.Count > 0)
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
                            }
                            else
                            {
                                tileObjects.Add(tileObject);
                            }
                        }
                        // Insert the previously removed tileobjects into the unit
                        Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                        if (unit != null && unit.Extractor != null)
                        {
                            unit.AddTileObjects(tileObjects);

                            // Insert an update move, so the client knows that tileobjects have been added
                            Move moveUpdate = new Move();
                            moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                            moveUpdate.MoveType = MoveType.UpdateStats;
                            moveUpdate.UnitId = unit.UnitId;
                            moveUpdate.Positions = new List<Position2>();
                            moveUpdate.Positions.Add(unit.Pos);
                            moveUpdate.Stats = unit.CollectStats();
                            nextMoves.Add(moveUpdate);
                        }
                        if (tileObjects.Count > 0)
                        {
                            Position2 from = move.Positions[move.Positions.Count - 1];
                            Tile fromTile = Map.GetTile(from);

                            foreach (TileObject tileObject in tileObjects)
                            {
                                if (tileObject.TileObjectType == TileObjectType.Mineral)
                                {
                                    // Drop Minerals on the floor, distribute anything else on the map
                                    // (No Trees in Buildings)
                                    fromTile.Add(tileObject);

                                    if (!changedGroundPositions.ContainsKey(fromTile.Pos))
                                        changedGroundPositions.Add(fromTile.Pos, null);
                                }
                                else
                                {
                                    Map.AddOpenTileObject(tileObject);
                                }
                            }
                            Move updateGroundMove = new Move();
                            updateGroundMove.MoveType = MoveType.UpdateGround;
                            updateGroundMove.Positions = new List<Position2>();
                            updateGroundMove.Positions.Add(from);
                            CollectGroundStats(from, updateGroundMove);
                            nextMoves.Add(updateGroundMove);
                        }
                    }*/
                    finishedMoves.Add(move);

#if MEASURE_MINS
                    MapInfo mapInfoNow = new MapInfo();
                    mapInfoNow.ComputeMapInfo(this, null);
                    if (mapInfoNow.TotalMetal != mapInfoPrev.TotalMetal)
                    {
                        throw new Exception();
                    }
#endif
                }
                else if (move.MoveType == MoveType.Transport)
                {
#if MEASURE_MINS
                    MapInfo mapInfoPrev = new MapInfo();
                    mapInfoPrev.ComputeMapInfo(this, null);
                    int countMin = mapInfoPrev.TotalMetal;
                    if (move.MoveType != MoveType.Skip)
                    {
                        foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
                        {
                            if (tileObject.TileObjectType == TileObjectType.Mineral ||
                                TileObject.CanConvertTileObjectIntoMineral(tileObject.TileObjectType))
                                countMin++;
                        }
                    }
#endif
                    if (move.Stats != null)
                    {
                        Position2 transportTargetPos = move.Positions[move.Positions.Count - 1];
                        Unit unit = Map.Units.GetUnitAt(transportTargetPos);

                        if (unit == null)
                        {
                            // Target died, transport to ground
                            Tile unitTile = Map.GetTile(transportTargetPos);

                            if (!changedGroundPositions.ContainsKey(transportTargetPos))
                                changedGroundPositions.Add(transportTargetPos, null);
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
                            moveUpdate.Positions = new List<Position2>();
                            moveUpdate.Positions.Add(unit.Pos);
                            moveUpdate.Stats = unit.CollectStats();
                            nextMoves.Add(moveUpdate);
                        }
                    }
#if MEASURE_MINS
                    MapInfo mapInfoNow = new MapInfo();
                    mapInfoNow.ComputeMapInfo(this, null);
                    if (mapInfoNow.TotalMetal != countMin)
                    {
                        throw new Exception();
                    }
#endif
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

        }

        private List<Unit> stunnedUnits = new List<Unit>();

        internal void StunUnit(Unit unit)
        {
            if (unit.Stunned == 0)
            {
                unit.Stunned += 2; // One is removed at start of next round
                stunnedUnits.Add(unit);
            }
            else
            {
                unit.Stunned++;
            }
        }

        internal void BulletImpact(Tile targetTile)
        {
            foreach (Tile neighbor in targetTile.Neighbors)
            {
                if (neighbor.Unit != null)
                {
                    StunUnit(neighbor.Unit);
                }
            }
        }



        private bool ProcessNewFireMove(Move move)
        {
            bool wasSuccessful = false;

            Unit fireingUnit = Map.Units.GetUnitAt(move.Positions[0]);
            if (fireingUnit != null && fireingUnit.Weapon != null && fireingUnit.Weapon.WeaponLoaded)
            {
                MoveRecipeIngredient moveRecipeIngredient;
                if (fireingUnit.Weapon.EndlessAmmo)
                {
                    moveRecipeIngredient = new MoveRecipeIngredient();
                    moveRecipeIngredient.TileObjectType = TileObjectType.Mineral;
                }
                else
                {
                    moveRecipeIngredient = fireingUnit.FindRefillAmmo();
                }
                move.Stats = fireingUnit.CollectStats();
                move.MoveRecipe = new MoveRecipe();

                // Ingredient is the reloaded ammo
                if (moveRecipeIngredient != null)
                {
                    if (!changedUnits.ContainsKey(moveRecipeIngredient.SourcePosition))
                        changedUnits.Add(moveRecipeIngredient.SourcePosition, Map.Units.GetUnitAt(moveRecipeIngredient.SourcePosition));

                    move.MoveRecipe.Ingredients.Add(moveRecipeIngredient);
                }
                // Result is the ammo that was used to fire
                TileObject shellTileObject = fireingUnit.FindAmmoTileObject(fireingUnit.Weapon.TileContainer);
                if (shellTileObject == null)
                {
                    throw new Exception("No Ammo");
                }
                //TileObject tileObject = fireingUnit.Weapon.TileContainer.TileObjects[0];
                fireingUnit.Weapon.TileContainer.Remove(shellTileObject);
                move.MoveRecipe.Result = shellTileObject.TileObjectType;

                if (!changedUnits.ContainsKey(fireingUnit.Pos))
                    changedUnits.Add(fireingUnit.Pos, fireingUnit);

                // Must be before the hit moves
                lastMoves.Add(move);

                HitByBullet(move, fireingUnit, lastMoves, shellTileObject);

                if (fireingUnit.CurrentGameCommand != null &&
                    fireingUnit.CurrentGameCommand.GameCommandType == GameCommandType.Fire &&
                    fireingUnit.CurrentGameCommand.FollowUpUnitCommand == FollowUpUnitCommand.DeleteCommand)
                {
                    fireingUnit.CurrentGameCommand.CommandComplete = true;
                }

                wasSuccessful = true;
            }
            return wasSuccessful;
        }

        internal void HitByBullet(Move move, Unit fireingUnit, List<Move> nextMoves, TileObject shellTileObject)
        {
            Position2 pos = move.Positions[move.Positions.Count - 1];
            Tile targetTile = Map.GetTile(pos);

            foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
            {
                fireingUnit.ConsumeIngredient(moveRecipeIngredient, changedUnits);
            }
            //TileObject tileObject = fireingUnit.Weapon.TileContainer.TileObjects[0];
            //fireingUnit.Weapon.TileContainer.Remove(tileObject);

            foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
            {
                TileObject reloadedAmmo = new TileObject();
                reloadedAmmo.TileObjectType = moveRecipeIngredient.TileObjectType;
                reloadedAmmo.Direction = Direction.C;
                fireingUnit.Weapon.TileContainer.Add(reloadedAmmo);
            }

            targetTile.HitByBullet(shellTileObject);

            if (!changedGroundPositions.ContainsKey(pos))
                changedGroundPositions.Add(pos, null);

            Unit targetUnit = targetTile.Unit;
            if (targetUnit != null)
            {
                StunUnit(targetUnit);

                Ability hitPart = targetUnit.HitBy(false);
                if (hitPart == null || hitPart is Shield)
                {
                    // Shield was hit
                    Move hitmove = new Move();
                    hitmove.MoveType = MoveType.Hit;
                    hitmove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                    hitmove.Positions = move.Positions;
                    hitmove.UnitId = targetUnit.UnitId;
                    hitmove.OtherUnitId = "Shield";

                    hitmove.Stats = new MoveUpdateStats();
                    hitmove.Stats.MoveUpdateGroundStat = move.Stats.MoveUpdateGroundStat;

                    nextMoves.Add(hitmove);

                    if (!changedUnits.ContainsKey(pos))
                        changedUnits.Add(pos, targetUnit);
                }
                else
                {

                    if (hitPart.TileContainer != null)
                    {
                        //foreach (TileObject unitTileObject in hitPart.TileContainer.TileObjects)
                        while (hitPart.TileContainer.Count > hitPart.TileContainer.Capacity)
                        {
                            TileObject toDrop = hitPart.TileContainer.RemoveTileObject(TileObjectType.All);

                            // Anything but minerals are distributed
                            if (toDrop.TileObjectType != TileObjectType.Mineral)
                            {
                                Map.AddOpenTileObject(toDrop);
                            }
                            else
                            {
                                targetTile.Add(toDrop);

                                if (!changedGroundPositions.ContainsKey(targetTile.Pos))
                                    changedGroundPositions.Add(targetTile.Pos, null);
                            }
                        }
                    }

                    TileObject hitPartTileObject = hitPart.PartTileObjects[0];
                    hitPart.PartTileObjects.Remove(hitPartTileObject);

                    // Part turns into mineral on ground
                    hitPartTileObject.TileObjectType = TileObjectType.Mineral;
                    hitPartTileObject.Direction = Direction.C;
                    targetTile.Add(hitPartTileObject);

                    if (!changedGroundPositions.ContainsKey(targetTile.Pos))
                        changedGroundPositions.Add(targetTile.Pos, null);

                    // Unit was hit
                    Move hitmove = new Move();
                    hitmove.MoveType = MoveType.Hit;
                    hitmove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                    hitmove.Positions = move.Positions;
                    hitmove.UnitId = targetUnit.UnitId;
                    hitmove.OtherUnitId = hitPart.PartType.ToString();
                    hitmove.Stats = new MoveUpdateStats();
                    hitmove.Stats = targetUnit.CollectStats();

                    nextMoves.Add(hitmove);

                    if (targetUnit.IsDead())
                    {
                        targetUnit.OnDestroyed();

                        if (hitPart.PartTileObjects.Count > 0)
                            throw new Exception();

                        // Unit has died!
                        Move deleteMove = new Move();
                        deleteMove.PlayerId = targetUnit.Owner.PlayerModel.Id;
                        deleteMove.MoveType = MoveType.Delete;
                        deleteMove.Positions = new List<Position2>();
                        deleteMove.Positions.Add(targetUnit.Pos);
                        deleteMove.UnitId = targetUnit.UnitId;
                        nextMoves.Add(deleteMove);

                        Map.Units.Remove(targetUnit.UnitId);
                        Map.Units.Remove(targetUnit.Pos);
                    }
                }
            }
            else
            {
                BulletImpact(targetTile);

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
                        if (move.MoveType == MoveType.Command || move.MoveType == MoveType.Transport)
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
                                if (move.UnitId == null)
                                {
                                    Unit unit = Map.Units.GetUnitAt(move.Positions[1]);
                                    if (unit.Extractor == null)
                                        throw new Exception("Cheater");
                                }
                                else
                                {
                                    Unit unit = Map.Units.GetUnitAt(move.Positions[0]);
                                    if (unit.Extractor == null)
                                        throw new Exception("Cheater");
                                }
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
                /*
                foreach (PlayerUnit playerUnit in player.PlayerUnits.Values)
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
                        throw new Exception("player has different unit");
                        //can happen if unit to be upgraded has moved away
                    }
                    if (unit.Owner.PlayerModel.Id != mapUnit.Owner.PlayerModel.Id)
                    {
                        throw new Exception("wrong player");
                    }
                }*/
            }
            foreach (Unit unit in Map.Units.List.Values)
            {

            }
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Add)
                {
                    int cnt = move.Positions.Count;
                    Position2 p = move.Positions[cnt - 1];
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
                    Position2 p = move.Positions[cnt - 1];
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
                    Position2 p = move.Positions[0];
                    if (move.Positions.Count > 1)
                    {
                        p = move.Positions[move.Positions.Count - 1];
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
                    Position2 p = move.Positions[cnt - 1];
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
                if (move.MoveType == MoveType.Move)
                {
                    lastMoves.Add(move);
                }
                if (move.MoveType == MoveType.Burn)
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
#if MEASURE_MINS
                    MapInfo mapInfoPrev = new MapInfo();
                    mapInfoPrev.ComputeMapInfo(this, null);

#endif
                    Unit unit;
                    if (move.UnitId == null)
                    {
                        // Extract from unit to ground
                        Position2 targetPos = move.Positions[0];
                        Tile targetTile = Map.GetTile(targetPos);

                        unit = Map.Units.GetUnitAt(move.Positions[1]);
                        if (unit.Extractor.UnloadInto(unit, move, targetTile, changedUnits))
                        {
                            if (!changedGroundPositions.ContainsKey(targetPos))
                                changedGroundPositions.Add(targetPos, null);
                            lastMoves.Add(move);
                        }
                    }
                    else
                    {
                        unit = Map.Units.GetUnitAt(move.Positions[0]);

                        if (unit != null && unit.Extractor != null)
                        {
                            bool extracted = false;

                            Position2 fromPos = move.Positions[move.Positions.Count - 1];
                            Tile fromTile = Map.GetTile(fromPos);

                            Unit otherUnit = null;
                            if (move.OtherUnitId != null &&
                                move.OtherUnitId.StartsWith("unit"))
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

                                //tileObject = move.Stats.MoveUpdateGroundStat.TileObjects[0];
                            }
                            if (move.MoveType != MoveType.Skip)
                            {
                                extracted = unit.Extractor.ExtractInto(unit, move, fromTile, otherUnit, changedUnits);

                                if (extracted)
                                {
                                    if (!changedGroundPositions.ContainsKey(fromPos))
                                        changedGroundPositions.Add(fromPos, null);

                                    if (otherUnit != null && !changedUnits.ContainsKey(otherUnit.Pos))
                                        changedUnits.Add(otherUnit.Pos, otherUnit);

                                    lastMoves.Add(move);

                                    if (otherUnit != null && otherUnit.IsDead())
                                    {
                                        otherUnit.OnDestroyed();

                                        // Unit has died!
                                        Move deleteMove = new Move();
                                        deleteMove.PlayerId = otherUnit.Owner.PlayerModel.Id;
                                        deleteMove.MoveType = MoveType.Delete;
                                        deleteMove.Positions = new List<Position2>();
                                        deleteMove.Positions.Add(otherUnit.Pos);
                                        deleteMove.UnitId = otherUnit.UnitId;
                                        lastMoves.Add(deleteMove);

                                        Map.Units.Remove(otherUnit.UnitId);
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
#if MEASURE_MINS
                    MapInfo mapInfoNow = new MapInfo();
                    mapInfoNow.ComputeMapInfo(this, null);
                    
                    if (mapInfoNow.TotalMetal != mapInfoPrev.TotalMetal)
                    {
                        throw new Exception();
                    }
#endif
                }
                else if (move.MoveType == MoveType.Fire)
                {
#if MEASURE_MINS
                    MapInfo mapInfoPrev = new MapInfo();
                    mapInfoPrev.ComputeMapInfo(this, null);
#endif

                    if (!ProcessNewFireMove(move))
                        move.MoveType = MoveType.Skip;

#if MEASURE_MINS
                    MapInfo mapInfoNow = new MapInfo();
                    mapInfoNow.ComputeMapInfo(this, null);
                    if (mapInfoNow.TotalMetal != mapInfoPrev.TotalMetal)
                    {
                        throw new Exception();
                    }
#endif
                }
                else if (move.MoveType == MoveType.Transport)
                {
#if MEASURE_MINS
                    MapInfo mapInfoPrev = new MapInfo();
                    mapInfoPrev.ComputeMapInfo(this, null);

#endif
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
#if MEASURE_MINS
                    MapInfo mapInfoNow = new MapInfo();
                    mapInfoNow.ComputeMapInfo(this, null);

                    if (mapInfoNow.TotalMetal != mapInfoPrev.TotalMetal)
                    {
                        throw new Exception();
                    }
#endif
                }
            }
            // Move all units to their new location
            foreach (Move move in newMoves)
            {
                if (move.MoveType == MoveType.Delete)
                {
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

        private int maxPowerPerTurn = 80;

        private void ConsumePower(Player player, List<Move> newMoves)
        {
            List<Unit> reactors = new List<Unit>();
            int totalNumberOfUnits = 0;

            // Collect reactors and consume power for every unit
            foreach (Unit unit in Map.Units.List.Values)
            {
                // Only own units
                if (unit.Owner.PlayerModel.Id != player.PlayerModel.Id)
                    continue;

                //unit.ClearReservations();
                if (unit.Changed)
                {
                    unit.Changed = false;
                    if (!changedUnits.ContainsKey(unit.Pos))
                        changedUnits.Add(unit.Pos, unit);
                }
                if (unit.UnderConstruction)
                    continue;

                if (unit.Reactor != null && unit.Engine == null)
                {
                    if (unit.Reactor.AvailablePower < maxPowerPerTurn)
                    {
                        Move move = unit.Reactor.BurnIfNeccessary(maxPowerPerTurn, changedUnits);
                        if (move != null)
                        {
                            lastMoves.Add(move);
                        }
                    }
                    reactors.Add(unit);
                    if (!changedUnits.ContainsKey(unit.Pos))
                        changedUnits.Add(unit.Pos, unit);
                }
                unit.PrevPower = unit.Power;
                if (unit.Power > 0)
                {
                    if (unit.EndlessPower)
                    {
                        if (unit.Armor != null)
                        {
                            unit.Armor.LoadShield();
                        }
                    }
                    else
                    {
                        unit.Power--;
                    }
                    totalNumberOfUnits++;
                }
                else
                {
                    if (unit.Owner != NeutralPlayer)
                    {
                        // Unpowered unit
                        unit.Owner = NeutralPlayer;
                        unit.ResetGameCommand();

                        if (!changedUnits.ContainsKey(unit.Pos))
                            changedUnits.Add(unit.Pos, unit);
                    }
                }
            }

            // Collect the total out power of all reactors
            int totalAvailablePower = 0;
            //int totalStoredPower = 0;
            foreach (Unit reactor in reactors)
            {
                // Max output of one reactor
                if (reactor.Reactor.AvailablePower > maxPowerPerTurn)
                    totalAvailablePower += maxPowerPerTurn;
                else
                    totalAvailablePower += reactor.Reactor.AvailablePower;

                //totalStoredPower += reactor.Reactor.StoredPower;
            }

            int availablePower = totalAvailablePower;
            int totalPowerRemoved = 0;
            int totalPowerMissing = 0;

            if (mapInfo.PlayerInfo.ContainsKey(player.PlayerModel.Id) && totalNumberOfUnits > 0)
            {
                // Recharge units
                bool allUnitsCharged = false;
                while (availablePower > 0 && !allUnitsCharged)
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

                        if (unit.Power < unit.MaxPower)
                        {
                            int chargedPower = maxPowerPerUnit;

                            if (unit.Power + maxPowerPerUnit > unit.MaxPower)
                            {
                                chargedPower = unit.MaxPower - unit.Power;
                            }
                            if (chargedPower > availablePower)
                            {
                                totalPowerMissing += chargedPower - availablePower;
                                chargedPower = availablePower;
                            }

                            unit.Power += chargedPower;
                            if (unit.Power < unit.MaxPower)
                            {
                                allUnitsCharged = false;
                            }
                            availablePower -= chargedPower;
                            totalPowerRemoved += chargedPower;
                        }
                        if (unit.Armor != null)
                        {
                            int chargedPower = unit.Armor.LoadShield();
                            if (chargedPower > availablePower)
                            {
                                totalPowerMissing += chargedPower - availablePower;
                                chargedPower = availablePower;
                            }
                            availablePower -= chargedPower;
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

                if (unit.Power < (unit.MaxPower / 2) && unit.Armor != null && unit.Armor.ShieldPower > 0)
                {
                    unit.Power++;
                    unit.Armor.ShieldPower--;
                    unit.Armor.ShieldActive = false;

                    if (!changedUnits.ContainsKey(unit.Pos))
                        changedUnits.Add(unit.Pos, unit);
                }
                if (unit.PrevPower != unit.Power)
                {
                    if (!changedUnits.ContainsKey(unit.Pos))
                        changedUnits.Add(unit.Pos, unit);
                }
            }

            if (totalPowerRemoved > totalAvailablePower)
            {
                throw new Exception("Can it be true?");
            }
            MapPlayerInfo mapPlayerInfo;
            if (mapInfo.PlayerInfo.TryGetValue(player.PlayerModel.Id, out mapPlayerInfo))
            {
                if (totalPowerRemoved == 0)
                    mapPlayerInfo.PowerOutInTurns = 9999;
                else
                    mapPlayerInfo.PowerOutInTurns = totalAvailablePower / (totalPowerRemoved + totalPowerMissing);
                if (totalAvailablePower > 0)
                {
                    mapPlayerInfo.TotalPower = ((totalPowerRemoved + totalPowerMissing) * 100) / totalAvailablePower;
                }
                else
                {
                    mapPlayerInfo.TotalPower = 100;
                }
            }
            int att = 100;
            while (totalPowerRemoved > 0 && att-- > 0)
            {
                // Consume the charged power from the reactors
                int removePowerFromEachReactor = (totalPowerRemoved / reactors.Count) + 1;
                if (removePowerFromEachReactor > maxPowerPerTurn)
                    removePowerFromEachReactor = maxPowerPerTurn;

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

            List<Move> acceptedMoves = new List<Move>();
            Dictionary<Position2, Move> moveToTargets = new Dictionary<Position2, Move>();

            // Remove moves that go to the same destination
            /*
            foreach (Move move in newMoves)
            {
                // Any position that is currently being upgraded or a unit turns from ghost into real
                // cannot be stepped on
                if (move.MoveType == MoveType.Upgrade) // || move.MoveType == MoveType.Build)
                {
                    Position2 destination = move.Positions[move.Positions.Count - 1];
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
            }*/

            int loopCounter = 0;

            bool somethingChanged = true;
            while (somethingChanged)
            {
                loopCounter++;
                somethingChanged = false;

                foreach (Move move in newMoves)
                {
                    if (move.MoveType == MoveType.Move && move.Positions.Count == 1)
                    {
                        // Unit turned
                        acceptedMoves.Add(move);
                    }
                    else if (move.MoveType == MoveType.Move || move.MoveType == MoveType.Add || move.MoveType == MoveType.Build)
                    {
                        Position2 destination = move.Positions[move.Positions.Count - 1];
                        if (moveToTargets.ContainsKey(destination))
                        {
                            if (move.MoveType == MoveType.Build && move.UnitId != null)
                            {
                                Unit unit = Map.Units.FindUnit(move.UnitId);
                                if (unit != null)
                                    unit.ResetGameCommand();
                            }
                            // (Hit) Could do nasty things, but for now, the unit does not move
                            newMoves.Remove(move);

                            moveToTargets.Clear();
                            acceptedMoves.Clear();
                            somethingChanged = true;
                            break;
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
            }
            if (loopCounter > 10)
            {
                //UnityEngine.Debug.Log("Loops " + loopCounter);
            }
            if (moveToTargets.Count > 100)
            {
                //UnityEngine.Debug.Log("moveToTargets " + moveToTargets.Count);
            }
            List<Move> revokedMoves = new List<Move>();
            List<Move> unblockedMoves = new List<Move>();

            foreach (Move move in moveToTargets.Values)
            {
                Position2 from = move.Positions[0];
                Position2 destination = move.Positions[move.Positions.Count - 1];
                Tile t = Map.GetTile(destination);
                if (t == null)
                {
                    // Moved outside?
                    if (move.MoveType == MoveType.Build)
                    {
                        Map.Units.Remove(move.UnitId);
                    }
                    revokedMoves.Add(move);
                }
                else if (!t.CanMoveTo(from))
                {
                    // Move to invalid pos
                    if (move.MoveType == MoveType.Build && move.UnitId != null)
                    {
                        Map.Units.Remove(move.UnitId);
                    }
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
                                unblockedMoves.Add(move);
                                acceptedMoves.Add(move);
                                unitBlocked = false;
                                break;
                            }
                        }
                    }
                    if (unitBlocked == true)
                    {
                        RevokeUnblockedMoves(unblockedMoves, move.Positions[0], revokedMoves, acceptedMoves);
                        if (move.MoveType == MoveType.Build)
                        {
                            Map.Units.Remove(move.UnitId);
                        }
                        revokedMoves.Add(move);
                        // (Hit) Could do nasty things, but for now, the unit does not move
                        if (move.MoveType == MoveType.Build)
                        {
                            Unit unit = Map.Units.FindUnit(move.UnitId);
                            if (unit != null)
                                unit.ResetGameCommand();
                        }
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

        private void RevokeUnblockedMoves(List<Move> unblockedMoves, Position2 blockedPosition, List<Move> revokedMoves, List<Move> acceptedMoves)
        {
            List<Position2> moreBlockedPositions = new List<Position2>();
            foreach (Move unblockedMove in unblockedMoves)
            {
                if (unblockedMove.Positions[1] == blockedPosition)
                {
                    // Revoke this move too
                    revokedMoves.Add(unblockedMove);
                    acceptedMoves.Remove(unblockedMove);
                    moreBlockedPositions.Add(unblockedMove.Positions[0]);
                }
            }
            foreach (Position2 position2 in moreBlockedPositions)
            {
                RevokeUnblockedMoves(unblockedMoves, position2, revokedMoves, acceptedMoves);
            }
        }

        internal Dictionary<Position2, Tile> changedGroundPositions = new Dictionary<Position2, Tile>();
        internal Dictionary<Position2, Unit> changedUnits = new Dictionary<Position2, Unit>();
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
                            tile.Add(tileObject);
                        }
                        if (!changedGroundPositions.ContainsKey(tile.Pos))
                            changedGroundPositions.Add(tile.Pos, null);
                    }
                }
            }
            bool freespace = true;
            while (freespace && attempts-- > 0)
            {
                bool oneSpace = false;

                foreach (MapZone mapZone in Map.Zones.Values)
                {
                    Position2 pos = mapZone.CreateTerrainTile(Map);
                    if (pos != Position2.Null)
                    {
                        oneSpace = true;
                        if (!changedGroundPositions.ContainsKey(pos))
                            changedGroundPositions.Add(pos, null);
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
            foreach (Position2 pos in changedGroundPositions.Keys)
            {
                Move hitmove = new Move();
                hitmove.MoveType = MoveType.UpdateGround;
                hitmove.Positions = new List<Position2>();
                hitmove.Positions.Add(pos);
                CollectGroundStats(pos, hitmove);
                moves.Add(hitmove);
            }
            changedGroundPositions.Clear();
        }
        /*
        public void CollectGroundStats(Position2 pos, Move move, List<TileObject> tileObjects)
        {
            CollectGroundStats(pos, move);
            if (tileObjects != null)
            {
                move.Stats.MoveUpdateGroundStat.TileObjects.Clear();
                foreach (TileObject tileObject in tileObjects)
                {
                    TileObject newTileObject = tileObject.Copy();
                    move.Stats.MoveUpdateGroundStat.TileObjects.Add(newTileObject);
                }
            }
        }
        */
        public void CollectGroundStats(Position2 pos, Move move)
        {
            if (move.Stats == null)
                move.Stats = new MoveUpdateStats();

            if (move.Stats.MoveUpdateGroundStat == null)
                move.Stats.MoveUpdateGroundStat = new MoveUpdateGroundStat();

            MoveUpdateGroundStat moveUpdateGroundStat = move.Stats.MoveUpdateGroundStat;

            Tile t = GetTile(pos);
            moveUpdateGroundStat.Owner = t.Owner;
            SetVisibilityMask(pos, move.Stats);
            moveUpdateGroundStat.IsBorder = t.IsBorder;
            moveUpdateGroundStat.IsUnderwater = t.IsUnderwater;
            moveUpdateGroundStat.TileObjects = new List<TileObject>();
            moveUpdateGroundStat.TileObjects.AddRange(t.TileObjects);
            moveUpdateGroundStat.Height = (float)t.Height;
            moveUpdateGroundStat.IsOpenTile = t.IsOpenTile;
            moveUpdateGroundStat.ZoneId = t.ZoneId;
        }

        public void SetVisibilityMask(Position2 pos, MoveUpdateStats stats)
        {
            Player player;
            if (Players.TryGetValue(1, out player))
            {
                if (player.IsVisible(pos))
                    stats.VisibilityMask |= 1;
            }
            if (Players.TryGetValue(2, out player))
            {
                if (player.IsVisible(pos))
                    stats.VisibilityMask |= 2;
            }
            if (Players.TryGetValue(3, out player))
            {
                if (player.IsVisible(pos))
                    stats.VisibilityMask |= 4;
            }
            if (Players.TryGetValue(4, out player))
            {
                if (player.IsVisible(pos))
                    stats.VisibilityMask |= 8;
            }
        }

        private int minsAfterStart;

        public List<Move> ProcessMove(int playerId, Move myMove, List<MapGameCommand> mapGameCommands)
        {
            List<Move> returnMoves = new List<Move>();
            if (myMove != null && myMove.MoveType == MoveType.UpdateAll)
            {
                UpdateAll(playerId, returnMoves);
                return returnMoves;
            }
#if MEASURE_MINS
            MapInfo mapInfoProcessFirstMoves = new MapInfo();
            mapInfoProcessFirstMoves.ComputeMapInfo(this, lastMoves);
            if (minsAfterStart != 0 &&
                mapInfoProcessFirstMoves.TotalMetal != minsAfterStart)
            {
            }
#endif


            changedUnits.Clear();
            changedGroundPositions.Clear();

#if MEASURE_TIMINGS
                DateTime start;
                double timetaken;
                
                start = DateTime.Now;
#endif

#if MEASURE_TIMINGS
                GC.Collect();
                timetaken = (DateTime.Now - start).TotalMilliseconds;
                if (timetaken > 10)
                {
                    UnityEngine.Debug.Log("GC.Collect(); (" + MoveNr + "): " + timetaken);
                    start = DateTime.Now;
                }
#endif

            PathFinderFast.CalculatedPaths = 0;

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
                //CreateTileObjects(1);
                //Map.GrowBio(changedGroundPositions);
#if DEBUG
                Validate(lastMoves);
#endif

#if MEASURE_MINS
                MapInfo mapInfoProcessLastMoves = new MapInfo();
                mapInfoProcessLastMoves.ComputeMapInfo(this, lastMoves);
#endif
                if (lastMoves.Count > 0)
                {
                    // Remove moves that have been processed
                    ProcessLastMoves();

                    // Follow up moves (units have been hit i.e. and must be removed. This is a result of the
                    // last move. Update the players unit list (delete moves are called double in this case)
                    foreach (Player player in Players.Values)
                    {
                        //player.ProcessMoves(lastMoves);
                        if (player.Control != null)
                            player.Control.ProcessMoves(player, lastMoves);
                        player.Discoveries.Clear();
                    }
                    newMoves.AddRange(lastMoves);
                    lastMoves.Clear();

                    // Only once for all
                    foreach (Unit unit in Map.Units.List.Values)
                    {
                        if (unit.Owner.PlayerModel.Id != 0)
                        {
                            Player player = Players[unit.Owner.PlayerModel.Id];
                            player.CollectVisiblePos(unit);
                        }
                    }
                    foreach (Player player in Players.Values)
                    {
                        List<Position2> hidePositions = new List<Position2>();
                        foreach (PlayerVisibleInfo playerVisibleInfo in player.VisiblePositions.Values)
                        {
                            if (playerVisibleInfo.LastUpdated < MoveNr)
                            {
                                hidePositions.Add(playerVisibleInfo.Pos);
                            }
                        }
                        foreach (Position2 pos in hidePositions)
                        {
                            player.VisiblePositions.Remove(pos);
                            if (!changedGroundPositions.ContainsKey(pos))
                                changedGroundPositions.Add(pos, null);
                        }
                    }
                }
#if MEASURE_MINS
                MapInfo mapInfoProcessLastMoves1 = new MapInfo();
                mapInfoProcessLastMoves1.ComputeMapInfo(this, null);
                if (mapInfoProcessLastMoves1.TotalMetal != mapInfoProcessLastMoves.TotalMetal)
                {
                    throw new Exception();
                }
#endif
            }

            List<Unit> allStunnedUnits = new List<Unit>();
            allStunnedUnits.AddRange(stunnedUnits);

            foreach (Unit unit in allStunnedUnits)
            {
                unit.Stunned--;
                if (unit.Stunned == 0)
                {
                    stunnedUnits.Remove(unit);
                }
            }

            if (mapGameCommands != null)
            {
                foreach (MapGameCommand mapGameCommand in mapGameCommands)
                {
                    GameCommand gameCommand = mapGameCommand.Copy();
                    gameCommand.IsHuman = true;
                    if (gameCommand.Radius > 0)
                    {
                        if (gameCommand.Radius == 1)
                        {
                            TileWithDistance tileWithDistance = new TileWithDistance(Map.GetTile(mapGameCommand.TargetPosition), 0);
                            gameCommand.IncludedPositions = new Dictionary<Position2, TileWithDistance>();
                            gameCommand.IncludedPositions.Add(mapGameCommand.TargetPosition, tileWithDistance);
                        }
                        else
                        {
                            gameCommand.IncludedPositions = Map.EnumerateTiles(gameCommand.TargetPosition, gameCommand.Radius-1, true);
                        }
                    }

                    Player player;

                    if (Players.TryGetValue(mapGameCommand.PlayerId, out player))
                    {
                        player.AddGamecommand(gameCommand);
                        //player.GameCommands.Add(gameCommand);
                    }

                    MapGameCommand currentGameCommand = mapGameCommand;
                    while (currentGameCommand.NextGameCommand != null)
                    {
                        gameCommand.NextGameCommand = currentGameCommand.NextGameCommand.Copy();
                        gameCommand = gameCommand.NextGameCommand;
                        currentGameCommand = currentGameCommand.NextGameCommand;
                    }
                }
            }
            Pheromones.Evaporate();

#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
                UnityEngine.Debug.Log("Prepare move Time " + timetaken);
#endif

            if (!first && lastMoves.Count == 0)
            {
#if MEASURE_MINS
                MapInfo mapInfoProcessCollectMoves = new MapInfo();
                mapInfoProcessCollectMoves.ComputeMapInfo(this, null);
#endif

                // New move
                bool allPlayersMoved = CollectNewMoves(myMove);

#if MEASURE_MINS
                MapInfo mapInfoProcessCollectMoves1 = new MapInfo();
                mapInfoProcessCollectMoves1.ComputeMapInfo(this, null);
                if (MoveNr > 0 && mapInfoProcessCollectMoves1.TotalMetal != mapInfoProcessCollectMoves.TotalMetal)
                {
                    throw new Exception();
                }
#endif

                if (!allPlayersMoved)
                {
                    return lastMoves;
                }
#if MEASURE_TIMINGS
                timetaken = (DateTime.Now - start).TotalMilliseconds;
                if (timetaken > 10)
                {
                    UnityEngine.Debug.Log("CollectNewMoves " + timetaken);
                    start = DateTime.Now;
                }
#endif
            }

            if (first)
            {
                lastMoves.AddRange(newMoves);
                newMoves.Clear();
            }
            else
            {
#if MEASURE_MINS
                MapInfo mapInfoProcessHandleMoves = new MapInfo();
                mapInfoProcessHandleMoves.ComputeMapInfo(this, null);
#endif

                LogMoves("Process new Moves " + Seed, MoveNr, newMoves);

#if MEASURE_TIMINGS
                List<Move> beforeHandle = new List<Move>();
                beforeHandle.AddRange(newMoves);
#endif
                // Check collisions and change moves if units collide or get destroyed
                HandleCollisions(newMoves);

#if MEASURE_TIMINGS
                timetaken = (DateTime.Now - start).TotalMilliseconds;
                if (timetaken > 10)
                {
                    UnityEngine.Debug.Log("HandleCollisions (" + MoveNr + "): " + timetaken);
                    start = DateTime.Now;
                }
#endif

                LogMoves("New Moves after HandleCollisions", MoveNr, newMoves);

                UpdateUnitPositions(newMoves);

#if MEASURE_TIMINGS
                timetaken = (DateTime.Now - start).TotalMilliseconds;
                if (timetaken > 10)
                {
                    UnityEngine.Debug.Log("UpdateUnitPositions " + timetaken);
                    start = DateTime.Now;
                }
#endif
#if MEASURE_MINS
                MapInfo mapInfoProcessHandleMoves1 = new MapInfo();
                mapInfoProcessHandleMoves1.ComputeMapInfo(this, null);
                if (mapInfoProcessHandleMoves1.TotalMetal != mapInfoProcessHandleMoves.TotalMetal)
                {
                    throw new Exception();
                }
#endif


#if MEASURE_MINS
                MapInfo mapInfoProcessNewMoves = new MapInfo();
                mapInfoProcessNewMoves.ComputeMapInfo(this, null);
#endif

                ProcessNewMoves();
#if MEASURE_MINS
                MapInfo mapInfoProcessNewMoves1 = new MapInfo();
                mapInfoProcessNewMoves1.ComputeMapInfo(this, lastMoves);
                if (MoveNr > 0 && mapInfoProcessNewMoves1.TotalMetal != mapInfoProcessNewMoves.TotalMetal)
                {
                    throw new Exception();
                }
#endif
#if MEASURE_TIMINGS
                timetaken = (DateTime.Now - start).TotalMilliseconds;
                if (timetaken > 10)
                {
                    UnityEngine.Debug.Log("ProcessNewMoves " + timetaken);
                    start = DateTime.Now;
                }
#endif
            }
            mapInfo = new MapInfo();
            mapInfo.ComputeMapInfo(this, lastMoves);
            if (minsAfterStart == 0)
                minsAfterStart = mapInfo.TotalMetal;
            else
            {
                if (minsAfterStart != mapInfo.TotalMetal)
                {
                }
            }

            foreach (Player player in Players.Values)
            {
                ConsumePower(player, lastMoves);
            }
#if MEASURE_MINS
            MapInfo mapInfoProcessExitMoves = new MapInfo();
            mapInfoProcessExitMoves.ComputeMapInfo(this, lastMoves);
            if (minsAfterStart != 0 &&
                mapInfoProcessExitMoves.TotalMetal != minsAfterStart)
            {
            }
#endif
            ProcessBorders();

            foreach (Unit unit in changedUnits.Values)
            {
                Move moveUpdate = new Move();
                moveUpdate.PlayerId = unit.Owner.PlayerModel.Id;
                moveUpdate.MoveType = MoveType.UpdateStats;
                moveUpdate.UnitId = unit.UnitId;
                moveUpdate.Positions = new List<Position2>();
                moveUpdate.Positions.Add(unit.Pos);
                moveUpdate.Stats = unit.CollectStats();
                lastMoves.Add(moveUpdate);
            }

            foreach (Player player in Players.Values)
            {
                if (player.PlayerModel.Id == playerId)
                {
                    //player.ProcessMoves(lastMoves);
                    returnMoves = lastMoves;
                }
                else
                {
                    //player.ProcessMoves(lastMoves);

                    if (player.Control != null)
                        player.Control.ProcessMoves(player, lastMoves);
                }
                foreach (GameCommand gameCommand in player.CompletedCommands)
                {
                    Move moveCommand = new Move();
                    moveCommand.PlayerId = player.PlayerModel.Id;
                    moveCommand.MoveType = MoveType.Command;

                    MapGameCommand mapGameCommand = new MapGameCommand();

                    mapGameCommand.CommandId = gameCommand.CommandId;
                    mapGameCommand.ClientId = gameCommand.ClientId;
                    mapGameCommand.CommandCanceled = gameCommand.CommandCanceled;
                    mapGameCommand.CommandComplete = gameCommand.CommandComplete;
                    mapGameCommand.GameCommandType = gameCommand.GameCommandType;

                    moveCommand.Command = mapGameCommand;

                    lastMoves.Add(moveCommand);

                }
                foreach (GameCommand gameCommand in player.GameCommands)
                {
                    Move moveCommand = new Move();
                    moveCommand.PlayerId = player.PlayerModel.Id;
                    moveCommand.MoveType = MoveType.Command;
                    
                    MapGameCommand mapGameCommand = new MapGameCommand();

                    mapGameCommand.CommandId = gameCommand.CommandId;
                    mapGameCommand.ClientId = gameCommand.ClientId;
                    mapGameCommand.CommandCanceled = gameCommand.CommandCanceled;
                    mapGameCommand.CommandComplete = gameCommand.CommandComplete;
                    mapGameCommand.GameCommandType = gameCommand.GameCommandType;
                    mapGameCommand.GameCommandState = gameCommand.GameCommandState;
                    mapGameCommand.PlayerId = gameCommand.PlayerId;
                    mapGameCommand.TargetPosition = gameCommand.TargetPosition;
                    mapGameCommand.Radius = gameCommand.Radius;
                    mapGameCommand.Direction = gameCommand.Direction;
                    mapGameCommand.Layout = gameCommand.Layout;

                    mapGameCommand.BlueprintName = gameCommand.BlueprintName;
                    mapGameCommand.Direction = gameCommand.Direction;
                    //mapGameCommand.Position3 = gameCommandItem.Position3;

                    mapGameCommand.AttachedUnit = new MapGameCommandItemUnit();
                    mapGameCommand.AttachedUnit.UnitId = gameCommand.AttachedUnit.UnitId;
                    mapGameCommand.AttachedUnit.Status = gameCommand.AttachedUnit.Status;
                    mapGameCommand.AttachedUnit.Alert = gameCommand.AttachedUnit.Alert;

                    mapGameCommand.FactoryUnit = new MapGameCommandItemUnit();
                    mapGameCommand.FactoryUnit.UnitId = gameCommand.FactoryUnit.UnitId;
                    mapGameCommand.FactoryUnit.Status = gameCommand.FactoryUnit.Status;
                    mapGameCommand.FactoryUnit.Alert = gameCommand.FactoryUnit.Alert;

                    mapGameCommand.TransportUnit = new MapGameCommandItemUnit();
                    mapGameCommand.TransportUnit.UnitId = gameCommand.TransportUnit.UnitId;
                    mapGameCommand.TransportUnit.Status = gameCommand.TransportUnit.Status;
                    mapGameCommand.TransportUnit.Alert = gameCommand.TransportUnit.Alert;

                    mapGameCommand.TargetUnit = new MapGameCommandItemUnit();
                    mapGameCommand.TargetUnit.UnitId = gameCommand.TargetUnit.UnitId;
                    mapGameCommand.TargetUnit.Status = gameCommand.TargetUnit.Status;
                    mapGameCommand.TargetUnit.Alert = gameCommand.TargetUnit.Alert;

                    moveCommand.Command = mapGameCommand;

                    lastMoves.Add(moveCommand);
                }
            }
            // Place tile objects (For Debug) and grow here, after all moves. So units cannot pick up items that grew in this move without notifing the client
            CreateTileObjects(1);
            Map.GrowBio(changedGroundPositions);

            // Add changed ground info
            AddChangedGroundInfoMoves(lastMoves);

            //CreateAreas();
            if (playerId == 0)
            {
                returnMoves = lastMoves;
#if DEBUG
                Validate(returnMoves);
#endif
            }


#if MEASURE_TIMINGS
            timetaken = (DateTime.Now - start).TotalMilliseconds;
            if (timetaken > 10)
                UnityEngine.Debug.Log("Complete move " + timetaken);
#endif



            if (lastMoves.Count >= 0)
            {
                //OutgoingMoves.Add(MoveNr, lastMoves);
                MoveNr++;
            }
            return returnMoves;
        }

        private MapInfo mapInfo;

        public MapInfo GetDebugMapInfo()
        {
            return mapInfo;
        }

        private List<Position2> updatedPositions = new List<Position2>();
        private void ProcessBorders()
        {
            List<Position2> newUpdatedPosition2s = new List<Position2>();

            foreach (Position2 pos in mapInfo.Pheromones.Keys)
            {
                newUpdatedPosition2s.Add(pos);
                updatedPositions.Remove(pos);

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
                    if (t.Owner != highestPlayerId)
                    {
                        if (!changedGroundPositions.ContainsKey(pos))
                            changedGroundPositions.Add(pos, null);
                        t.Owner = highestPlayerId;
                    }
                }
                else
                {
                    if (t.Owner != 0)
                    {
                        if (!changedGroundPositions.ContainsKey(pos))
                            changedGroundPositions.Add(pos, null);
                        t.Owner = 0;
                    }
                }
            }
            foreach (Position2 pos in updatedPositions)
            {
                Tile t = Map.GetTile(pos);
                if (t.Owner != 0 || t.IsBorder)
                {
                    if (!changedGroundPositions.ContainsKey(pos))
                        changedGroundPositions.Add(pos, null);
                    t.Owner = 0;
                    t.IsBorder = false;
                }
            }
            updatedPositions = newUpdatedPosition2s;

            foreach (Position2 pos in newUpdatedPosition2s)
            {
                Tile t = Map.GetTile(pos);
                if (t.Owner != 0)
                {
                    bool isBorder = false;
                    foreach (Tile n in t.Neighbors)
                    {
                        if (n.Owner != t.Owner && !n.IsUnderwater)
                        {
                            isBorder = true;
                            break;
                        }
                    }
                    if (t.IsBorder != isBorder)
                    {
                        t.IsBorder = isBorder;
                        if (!changedGroundPositions.ContainsKey(pos))
                            changedGroundPositions.Add(pos, null);
                    }
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
                    area.Units.Add(unit);
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
                    //if (!VisiblePosition2s.Contains(n.Pos))
                    //    continue;

                    //if (!n.CanMoveTo())
                    //    continue;

                    TileWithDistance neighborsTile = new TileWithDistance(Map.GetTile(n.Pos), tile.Distance + 1);
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
