
using Engine.Ants;
using Engine.Control;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engine.Interface
{
    internal class Players
    {
        public List<Player> players = new List<Player>();


        public Players()
        {
        }

        public List<Player> List
        {
            get
            {
                return players;
            }
        }
    }

    public class PlayerMove 
    {
        public PlayerMove(Move move)
        {
            Move = move;
        }

        public Command Command { get; set; }

        public Move Move { get; private set; }
        public string NewUnitId { get; set; }

    }

    public class PlayerUnit
    {
        public int ExplorationValue;
        public Unit Unit { get; set; }
        public PlayerUnit(Unit unit)
        {
            if (unit == null)
                throw new Exception("unit is null");
            Unit = unit;
        }

        public List<PlayerMove> PossibleMoves = new List<PlayerMove>();

        public List<Position> VisiblePositions;
        public override string ToString()
        {
            return Unit.ToString() + " [" + ExplorationValue + "]";
        }

        static public int ComputeExplorationValue(Player player, Position pos, int visibilityRange)
        {
            List<Position> positions = new List<Position>();
            CollectVisiblePos(player.Game, pos, positions, visibilityRange);

            return ComputeExplorationValue(null, player, positions);
        }

        public void ComputeExplorationValue()
        {
            this.ExplorationValue = ComputeExplorationValue(this, Unit.Owner, VisiblePositions);
        }

        private static int ComputeExplorationValue(PlayerUnit notThis, Player player, List<Position> positions)
        {
            List<Position> seenPos = new List<Position>();
            seenPos.AddRange(positions);
            foreach (PlayerUnit playerUnit in player.Units.Values)
            {
                if (playerUnit == notThis)
                    continue;

                // Remove all pos that are seen by the other units
                if (playerUnit.VisiblePositions != null)
                {
                    foreach (Position p in playerUnit.VisiblePositions)
                    {
                        seenPos.Remove(p);
                    }
                }
                if (seenPos.Count == 0)
                    return 0;
            }
            return seenPos.Count;
        }
        internal void CollectVisiblePos(Position pos, List<Position> positions, bool keep)
        {
            List<Position> calcPos = new List<Position>();

            int visibilityRange = 2;
            if (Unit.Reactor != null)
            {
                visibilityRange = 15;
            }

            CollectVisiblePos(this.Unit.Game, pos, calcPos, visibilityRange);
            positions.AddRange(calcPos);
            if (keep)
            {
                this.ExplorationValue = 0;
                this.VisiblePositions = calcPos;
            }
        }

        internal static void CollectVisiblePos(Game game, Position pos, List<Position> positions, int visibilityRange)
        {
            List<TileWithDistance> openList = new List<TileWithDistance>();
            Dictionary<Position, TileWithDistance> reachedTiles = new Dictionary<Position, TileWithDistance>();

            if (!positions.Contains(pos))
            {
                positions.Add(pos);
            }

            Tile startTile = game.Map.GetTile(pos);
            if (startTile == null) return;

            TileWithDistance start = new TileWithDistance(startTile, 0);

            openList.Add(start);

            while (openList.Count > 0)
            {
                TileWithDistance tile = openList[0];
                openList.RemoveAt(0);

                if (tile.Distance > visibilityRange)
                    continue;
                if (!positions.Contains(tile.Pos))
                {
                    positions.Add(tile.Pos);
                }
                foreach (Tile n in tile.Neighbors)
                {
                    if (!reachedTiles.ContainsKey(n.Pos))
                    {
                        TileWithDistance neighborsTile = new TileWithDistance(game.Map.GetTile(n.Pos), tile.Distance + 1);
                        reachedTiles.Add(neighborsTile.Pos, neighborsTile);

                        if (neighborsTile.Distance <= visibilityRange)
                        {
                            openList.Add(neighborsTile);
                        }
                    }
                }
            }
        }
    }

    public class Player
    {
        public Game Game { get; set; }
        public IControl Control { get; set; }
        public PlayerModel PlayerModel { get; set; }
        public List<Move> LastMoves;

        public List<Command> Commands = new List<Command>();

        //public Pheromones Pheromones { get; set; }

        // Unit that the player knows. Own and enemy
        public Dictionary<Position, PlayerUnit> Units = new Dictionary<Position, PlayerUnit>();
        public Dictionary<Position, PlayerUnit> UnitsInBuild = new Dictionary<Position, PlayerUnit>();
        // Positions the player sees
        public List<Position> VisiblePositions = new List<Position>();

        public bool CanProduceMoreUnits()
        {
            return true;
            /*
            if (NumberOfUnits > 10)
                return false;
            if (NumberOfUnits < 5 + this.VisiblePositions.Count / 20)
                return true;
            return false;
            */
        }
        public bool WonThisGame()
        {
            foreach (Unit unit in Game.Map.Units.List.Values)
            {
                if (unit.Owner.PlayerModel.Id != 0 &&
                    unit.Owner.PlayerModel.Id != this.PlayerModel.Id)
                    return false;
            }
            return true;
        }

        public bool IsAttached(string unitId, Command anyOtherThanThis)
        {
            foreach (Command command in Commands)
            {
                if (command == anyOtherThanThis)
                    continue;

                if (command.AssignedUnits.Contains(unitId))
                    return true;
            }
            return false;
        }

        public void DeleteCommand(string commandId)
        {
            // Remove all references
            foreach (Command command in Commands)
            {
                foreach (CommandSource commandSource in command.CommandSources)
                {
                    if (commandSource.Child.CommandId == commandId)
                    {
                        command.CommandSources.Remove(commandSource);
                        break;
                    }
                }
                foreach (CommandSource commandSource in command.CommandSources)
                {
                    if (commandSource.Parent.CommandId == commandId)
                    {
                        command.CommandSources.Remove(commandSource);
                        break;
                    }
                }
            }
            // Remove the command

            foreach (Command command in Commands)
            {
                if (command.CommandId == commandId)
                {
                    Commands.Remove(command);
                    break;
                }
            }
        }

        public int NumberOfUnits
        {
            get
            {
                int numberOfUnits = 0;
                foreach (PlayerUnit playerUnit in Units.Values)
                {
                    if (playerUnit.Unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                        numberOfUnits++;
                }
                return numberOfUnits;
            }
        }

        public Player(Game game, PlayerModel playerModel)
        {
            Game = game;
            PlayerModel = playerModel;

            if (playerModel.ControlLevel == 1)
            {
                Control = new ControlAnt(game, playerModel, game.GameModel);
            }
            else
            {
                Control = new ControlLevelI(game, playerModel, game.GameModel);
            }
            /*
            else if (playerModel.ControlLevel == 2)
                Control = new ControlLevelII(game, playerModel, game.GameModel); */
            //else if (playerModel.ControlLevel == 3)
            //    Control = new ControlLevelIII(game, playerModel, game.GameModel);
            /*else if (playerModel.ControlLevel == 4)
                Control = new ControlLevel4(game, playerModel, game.GameModel);
            else if (playerModel.ControlLevel == 5)
                Control = new ControlLevel5(game, playerModel, game.GameModel);*/
        }

        //public List<Area> Areas { get; set; }

        public Dictionary<Position, Tile> ForeignBorderTiles = new Dictionary<Position, Tile>();
        /*
        private void CreateAreas()
        {
            Dictionary<Tile, Area> areaMap = new Dictionary<Tile, Area>();
            List<Tile> openList = new List<Tile>();

            foreach (PlayerUnit playerUnit in Units.Values)
            {
                Tile tile;
                tile = Game.Map.GetTile(playerUnit.Unit.Pos);

                if (tile.CanMoveTo())
                    openList.Add(tile);

                Area area = new Area(Game.Map);
                area.PlayerId = tile.Unit.Owner.PlayerModel.Id;
                area.Units.Add(playerUnit);                
                area.Tiles.Add(tile.Pos, tile);
                areaMap.Add(tile, area);
            }
            while (openList.Count > 0)
            {
                Tile tile = openList[0];
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

                foreach (Tile n in tile.Neighbors)
                {
                    if (!VisiblePositions.Contains(n.Pos))
                        continue;

                    if (!tile.CanMoveTo())
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
                        openList.Add(n);
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
            Areas = new List<Area>();

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
                                area.ForeignBorderTiles.Add(tile.Pos, tile);
                                area.BorderTiles.Add(tile.Pos, tile);
                            }
                            break;
                        }
                    }
                }
            }
        }*/

        public void UpdateAll(List<Move> returnMoves)
        {
            Move move;
            if (this.VisiblePositions.Count > 0)
            {
                move = new Move();
                move.MoveType = MoveType.VisibleTiles;
                move.PlayerId = this.PlayerModel.Id;
                move.Positions = new List<Position>();
                move.Positions.AddRange(this.VisiblePositions);
                returnMoves.Add(move);
            }
            foreach (PlayerUnit playerUnit in Units.Values)
            {
                move = new Move();
                move.MoveType = MoveType.Add;
                move.PlayerId = this.PlayerModel.Id;
                move.Positions = new List<Position>();
                move.Positions.Add(playerUnit.Unit.Pos);
                move.UnitId = playerUnit.Unit.UnitId;
                move.Stats = playerUnit.Unit.CollectStats();
                returnMoves.Add(move);
            }
        }

        private int moveNr;

        internal void ProcessMoves(List<Move> moves)
        {
            // Update postion of all units
            Dictionary<Position, PlayerUnit> deletedUnits = new Dictionary<Position, PlayerUnit>();
            List<Position> removedUnits = new List<Position>();
            Dictionary<Position, PlayerUnit> movedAwayUnits = new Dictionary<Position, PlayerUnit>();
            List<Position> changedUnits = new List<Position>();
            LastMoves = new List<Move>();

            moveNr++;
            
            // Remove all units first
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Move)
                {
                    if (move.Positions.Count > 1)
                    {
                        Position from = move.Positions[0];
                        if (this.Units.ContainsKey(from))
                        {
                            PlayerUnit unit = this.Units[from];
                            movedAwayUnits.Add(from, unit);
                            this.Units.Remove(from);
                        }
                    }
                }
                if (move.MoveType == MoveType.Delete)
                {
                    Position from = move.Positions[move.Positions.Count-1];
                    if (this.Units.ContainsKey(from))
                    {
                        PlayerUnit playerUnit = this.Units[from];
                        deletedUnits.Add(from, playerUnit);
                        removedUnits.Add(from);
                        this.Units.Remove(from);
                    }
                }
            }
            Dictionary<Position, PlayerUnit> addedUnits = new Dictionary<Position, PlayerUnit>();
            Dictionary<Position, PlayerUnit> movedToUnits = new Dictionary<Position, PlayerUnit>();
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Add)
                {
                    Position to = move.Positions[move.Positions.Count - 1];
                    if (this.Units.ContainsKey(to))
                        throw new Exception();
                    Unit unit = Game.Map.Units.GetUnitAt(to);
                    PlayerUnit playerUnit = new PlayerUnit(unit);
                    addedUnits.Add(to, playerUnit);
                    Units.Add(to, playerUnit);
                    changedUnits.Add(to);
                }
                if (move.MoveType == MoveType.Build)
                {
                    Position to = move.Positions[move.Positions.Count - 1];
                    // ignore, its a ghost
                    //if (this.Units.ContainsKey(to))
                    //    throw new Exception();
                    if (move.PlayerId == PlayerModel.Id && !UnitsInBuild.ContainsKey(to))
                        throw new Exception();
                }
                if (move.MoveType == MoveType.Upgrade)
                {
                    Position to = move.Positions[move.Positions.Count - 1];
                    if (!this.Units.ContainsKey(to))
                    {
                        Unit unit = Game.Map.Units.GetUnitAt(to);
                        PlayerUnit playerUnit = new PlayerUnit(unit);
                        addedUnits.Add(to, playerUnit);
                        Units.Add(to, playerUnit);
                        changedUnits.Add(to);
                    }
                }
                if (move.MoveType == MoveType.Move)
                {
                    Position to;
                    if (move.Positions.Count == 1)
                    {
                        // Cloud not move this unit
                    }
                    else
                    {
                        to = move.Positions[move.Positions.Count-1];

                        if (this.Units.ContainsKey(to))
                            throw new Exception();
                        Unit unit = Game.Map.Units.GetUnitAt(to);
                        PlayerUnit playerUnit = new PlayerUnit(unit);
                        movedToUnits.Add(to, playerUnit);
                        Units.Add(to, playerUnit);
                        changedUnits.Add(to);
                    }
                }
            }

            List<Position> previouslySeen = new List<Position>();
            List<Position> newlySeen = new List<Position>();
            foreach (Position pos in movedAwayUnits.Keys)
            {
                PlayerUnit playerUnit = movedAwayUnits[pos];
                if (playerUnit.Unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    playerUnit.CollectVisiblePos(pos, previouslySeen, false);
            }
            foreach (PlayerUnit playerUnit in deletedUnits.Values)
            {
                if (playerUnit.Unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    playerUnit.CollectVisiblePos(playerUnit.Unit.Pos, previouslySeen, false);
            }
            foreach (PlayerUnit playerUnit in addedUnits.Values)
            {
                if (playerUnit.Unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    playerUnit.CollectVisiblePos(playerUnit.Unit.Pos, newlySeen, true);
            }
            foreach (PlayerUnit playerUnit in movedToUnits.Values)
            {
                if (playerUnit.Unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    playerUnit.CollectVisiblePos(playerUnit.Unit.Pos, newlySeen, true);
            }

            if (previouslySeen.Count > 0) // || newlySeen.Count > 0)
            {
                foreach (PlayerUnit playerUnit in Units.Values)
                {
                    Unit unit = playerUnit.Unit;
                    if (!changedUnits.Contains(unit.Pos) && unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    {
                        // Add the tiles that are visible by not moved/deleted units
                        // Must collect? or reuse playerUnit.Visible
                        playerUnit.CollectVisiblePos( unit.Pos, newlySeen, false);
                    }
                }
            }

            Move hiddenTilesMove = new Move();
            hiddenTilesMove.PlayerId = this.PlayerModel.Id;
            hiddenTilesMove.MoveType = MoveType.HiddenTiles;
            hiddenTilesMove.Positions = new List<Position>();

            foreach (Position oldPos in previouslySeen)
            {
                // Not seen by another unit
                if (!newlySeen.Contains(oldPos))
                {
                    // Did we see that before?
                    if (this.VisiblePositions.Contains(oldPos))
                    {
                        // Now we do not see it any more
                        this.VisiblePositions.Remove(oldPos);
                        hiddenTilesMove.Positions.Add(oldPos);
                        
                        Unit unit = Game.Map.Units.GetUnitAt(oldPos);
                        if (unit == null)
                        {
                            if (deletedUnits.ContainsKey(oldPos))
                                unit = deletedUnits[oldPos].Unit;
                        }
                        if (unit != null && unit.Owner.PlayerModel.Id != this.PlayerModel.Id)
                        {
                            if (!deletedUnits.ContainsKey(oldPos))
                            {
                                deletedUnits.Add(oldPos, new PlayerUnit(unit));
                            }
                        }
                    }
                }
            }
            if (hiddenTilesMove.Positions.Count > 0)
            {
                LastMoves.Add(hiddenTilesMove);
            }
            Move newVisibleTilesMove = new Move();
            newVisibleTilesMove.PlayerId = this.PlayerModel.Id;
            newVisibleTilesMove.MoveType = MoveType.VisibleTiles;
            newVisibleTilesMove.Positions = new List<Position>();
            foreach (Position newPos in newlySeen)
            {
                if (!this.VisiblePositions.Contains(newPos))
                {
                    newVisibleTilesMove.Positions.Add(newPos);
                    this.VisiblePositions.Add(newPos);

                    Unit unit = Game.Map.Units.GetUnitAt(newPos);
                    if (unit != null && unit.Owner.PlayerModel.Id != this.PlayerModel.Id)
                    {
                        if (!addedUnits.ContainsKey(newPos) &&
                            !movedToUnits.ContainsKey(newPos) &&
                            !deletedUnits.ContainsKey(newPos))
                        {
                            PlayerUnit playerUnit = new PlayerUnit(unit);
                            Units.Add(newPos, playerUnit);
                            addedUnits.Add(newPos, playerUnit);
                        }
                    }
                }
            }
            if (newVisibleTilesMove.Positions.Count > 0)
            {
                LastMoves.Add(newVisibleTilesMove);
            }

            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Add || move.MoveType == MoveType.Build)
                {
                    if (this.VisiblePositions.Contains(move.Positions[move.Positions.Count - 1]))
                        LastMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Move)
                {
                    if (!newVisibleTilesMove.Positions.Contains(move.Positions[0]) &&
                        this.VisiblePositions.Contains(move.Positions[0]))
                    {
                        // If count is 1 unit could not be moved
                        if (move.Positions.Count > 1)
                        {
                            if (this.VisiblePositions.Contains(move.Positions[1]))
                            {
                                if (movedAwayUnits.ContainsKey(move.Positions[0]))
                                {
                                    // Move from seen to seen
                                    LastMoves.Add(move);
                                }
                                else
                                {
                                    // Move from unseen to seen
                                    Move addMove = new Move();
                                    addMove.PlayerId = move.PlayerId;
                                    addMove.MoveType = MoveType.Add;
                                    addMove.Positions = new List<Position>();
                                    addMove.Positions.Add(move.Positions[1]);
                                    addMove.UnitId = move.UnitId;
                                    LastMoves.Add(addMove);
                                }
                            }
                            else
                            {
                                // Move from seen to unseen
                                Move deleteMove = new Move();
                                deleteMove.PlayerId = move.PlayerId;
                                deleteMove.MoveType = MoveType.Delete;
                                deleteMove.Positions = new List<Position>();
                                deleteMove.Positions.Add(move.Positions[0]);
                                deleteMove.UnitId = move.UnitId;
                                LastMoves.Add(deleteMove);
                            }
                        }
                    }
                    else if (move.Positions.Count > 1)
                    {
                        if (this.VisiblePositions.Contains(move.Positions[1]))
                        {
                            if (movedAwayUnits.ContainsKey(move.Positions[0]))
                            {
                                // Move from seen to seen
                                LastMoves.Add(move);
                            }
                            else
                            {
                                // Move from unseen to seen
                                /*
                                Move addMove = new Move();
                                addMove.PlayerId = move.PlayerId;
                                addMove.MoveType = MoveType.Add;
                                addMove.Positions = new List<Position>();
                                addMove.Positions.Add(move.Positions[1]);
                                addMove.UnitId = move.UnitId;
                                LastMoves.Add(addMove);
                                */
                            }
                        }
                        else
                        {
                            if (movedAwayUnits.ContainsKey(move.Positions[0]))
                            {
                                // Was seen but moved away
                                Move deleteMove = new Move();
                                deleteMove.PlayerId = move.PlayerId;
                                deleteMove.MoveType = MoveType.Delete;
                                deleteMove.Positions = new List<Position>();
                                deleteMove.Positions.Add(move.Positions[0]);
                                deleteMove.UnitId = move.UnitId;
                                LastMoves.Add(deleteMove);
                            }
                            else
                            {
                                // Move from unseen to unseen
                            }
                        }
                    }
                }
                else if (move.MoveType == MoveType.Delete)
                {
                    if (removedUnits.Contains(move.Positions[0]))
                        LastMoves.Add(move);
                }

                // Remove moves, the player cannot see
                else if (move.MoveType == MoveType.Hit || move.MoveType == MoveType.UpdateStats)
                {
                    if (this.VisiblePositions.Contains(move.Positions[0]))
                        LastMoves.Add(move);
                }
                else if (move.MoveType == MoveType.Fire)
                {
                    if (this.VisiblePositions.Contains(move.Positions[0]))
                        LastMoves.Add(move);
                }
            }

            List<Unit> invisibleUnits = new List<Unit>();

            // Check if all enemy units are visible
            foreach (PlayerUnit playerUnit in Units.Values)
            {
                Unit unit = playerUnit.Unit;
                if (unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    continue;

                if (VisiblePositions.Contains(unit.Pos))
                {
                    // enemy unit is visible
                    if (addedUnits.ContainsKey(unit.Pos) || movedToUnits.ContainsKey(unit.Pos))
                    {
                        // The unit showed up
                        // Enemy Unit appears
                        if (movedAwayUnits.Values.Contains(playerUnit))
                        {

                        }
                        else
                        {
                            bool moveExists = false;
                            foreach (Move curMove in LastMoves)
                            {
                                if ((curMove.MoveType == MoveType.Add || curMove.MoveType == MoveType.Build || curMove.MoveType == MoveType.Move) &&
                                    curMove.UnitId == unit.UnitId)
                                {
                                    moveExists = true;
                                    break;
                                }
                            }
                            if (!moveExists)
                            {
                                Move addMove = new Move();
                                addMove.PlayerId = unit.Owner.PlayerModel.Id;
                                addMove.MoveType = MoveType.Add;
                                addMove.Positions = new List<Position>();
                                addMove.Positions.Add(unit.Pos);
                                addMove.UnitId = unit.UnitId;
                                addMove.Stats = unit.CollectStats();
                                LastMoves.Add(addMove);
                            }
                        }
                    }
                }
                else
                {
                    // enemy unit is invisible
                    invisibleUnits.Add(unit);
                }
            }
            foreach (PlayerUnit playerUnit in deletedUnits.Values)
            {
                Unit unit = playerUnit.Unit;
                if (unit.Owner.PlayerModel.Id == this.PlayerModel.Id)
                    continue;

                bool moveExists = false;
                foreach (Move curMove in LastMoves)
                {
                    if (curMove.MoveType == MoveType.Delete &&
                        curMove.UnitId == unit.UnitId)
                    {
                        moveExists = true;
                        break;
                    }
                }
                if (!moveExists)
                {
                    if (movedToUnits.ContainsKey(unit.Pos) && !movedAwayUnits.ContainsKey(unit.Pos))
                    {
                        // This unit hasn't been seen before, so no delete move is needed
                    }
                    else if (addedUnits.ContainsKey(unit.Pos))
                    {
                        // Just added, no need to delete
                    }
                    else
                    {
                        Move deleteMove = new Move();
                        deleteMove.PlayerId = unit.Owner.PlayerModel.Id;
                        deleteMove.MoveType = MoveType.Delete;
                        deleteMove.Positions = new List<Position>();
                        deleteMove.Positions.Add(unit.Pos);
                        deleteMove.UnitId = unit.UnitId;
                        if (Units.ContainsKey(unit.Pos))
                            Units.Remove(unit.Pos);
                        LastMoves.Add(deleteMove);
                    }
                }
            }
            foreach (Unit unit in invisibleUnits)
            {
                Units.Remove(unit.Pos);
            }

            //CreateAreas();

#if DEBUG
            CheckUnits();
#endif
        }

        private void CheckUnits()
        {
            foreach (PlayerUnit playerUnit in Units.Values)
            {
                if (!VisiblePositions.Contains(playerUnit.Unit.Pos))
                    throw new Exception();
            }
            foreach (Unit unit in Game.Map.Units.List.Values)
            {
                if (VisiblePositions.Contains(unit.Pos))
                {
                    if (!Units.ContainsKey(unit.Pos))
                    {
                        //throw new Exception();
                    }
                }
                else
                {
                    if (Units.ContainsKey(unit.Pos))
                        throw new Exception();
                }
            }
        }
        public int Count(Type unitType)
        {
            int cnt = 0;
            foreach (Unit unit in Game.Map.Units.List.Values)
            {
                if (unit.Owner == this)
                {
                    if (unit.GetType() == unitType)
                        cnt++;
                }
            }
            return cnt;
        }

        public override string ToString()
        {
            return this.PlayerModel.Name;
        }
    }
}
