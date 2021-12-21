
using Engine.Ants;

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

    public class PlayerVisibleInfo
    {
        public Unit Unit { get; set; }
        public int NumberOfCollectables { get; set; }
        public Position2 Pos { get; set; }
        public int LastUpdated { get; set; }

        public override string ToString()
        {
            return Pos.ToString() + " Col: " + NumberOfCollectables;
        }
    }

    public class Player
    {
        public Game Game { get; set; }

        public MapZone StartZone { get; set; }

        public IControl Control { get; set; }
        public PlayerModel PlayerModel { get; set; }
        public List<Move> LastMoves;

        internal List<GameCommand> GameCommands = new List<GameCommand>();
        public Dictionary<Position2, PlayerVisibleInfo> VisiblePositions = new Dictionary<Position2, PlayerVisibleInfo>();
        public Dictionary<Position2, PlayerVisibleInfo> Discoveries = new Dictionary<Position2, PlayerVisibleInfo>();

        public bool IsVisible(Position2 pos)
        {
            PlayerVisibleInfo playerVisibleInfo;
            if (VisiblePositions.TryGetValue(pos, out playerVisibleInfo))
            {
                return true;
            }
            return false;
        }

        internal void CollectVisiblePos(Unit unit)
        {
            if (unit.UnderConstruction)
                return;

            List<Position2> calcPos = new List<Position2>();

            int visibilityRange = 2;
            if (!unit.Blueprint.IsMoveable())
            {
                visibilityRange = 4; // Unit.Reactor.Range;
            }
            Dictionary<Position2, TileWithDistance> tiles = Game.Map.EnumerateTiles(unit.Pos, visibilityRange, true);
            foreach (TileWithDistance tileWithDistance in tiles.Values)
            {
                PlayerVisibleInfo playerVisibleInfo;
                if (VisiblePositions.TryGetValue(tileWithDistance.Pos, out playerVisibleInfo))
                {
                    /*
                    if (playerVisibleInfo.NumberOfCollectables != tileWithDistance.Tile.NumberOfCollectables)
                    {
                        Discoveries.Add(tileWithDistance.Pos, playerVisibleInfo);
                        //UnityEngine.Debug.Log("Update discovery at " + Position.GetString(tileWithDistance.Pos));
                    }*/
                }
                else
                {
                    playerVisibleInfo = new PlayerVisibleInfo();
                    VisiblePositions.Add(tileWithDistance.Pos, playerVisibleInfo);

                    if (!Game.changedGroundPositions.ContainsKey(tileWithDistance.Pos))
                        Game.changedGroundPositions.Add(tileWithDistance.Pos, tileWithDistance.Tile);
                }

                int numberOfCollectables;

                numberOfCollectables = tileWithDistance.Tile.Counter.NumberOfCollectables;
                if (tileWithDistance.Tile.Unit != null)
                {
                    if (tileWithDistance.Tile.Unit.Owner.PlayerModel.Id == 0)
                    {
                        numberOfCollectables += tileWithDistance.Tile.Unit.CountMineral();
                    }
                }

                if ((numberOfCollectables > 0 && playerVisibleInfo.NumberOfCollectables != numberOfCollectables) ||
                    playerVisibleInfo.Unit != tileWithDistance.Tile.Unit)
                {
                    //UnityEngine.Debug.Log("New discovery at " + Position.GetString(tileWithDistance.Pos));
                    Discoveries.Add(tileWithDistance.Pos, playerVisibleInfo);
                }

                playerVisibleInfo.NumberOfCollectables = numberOfCollectables;
                playerVisibleInfo.Unit = tileWithDistance.Tile.Unit;
                playerVisibleInfo.Pos = tileWithDistance.Pos;
                playerVisibleInfo.LastUpdated = Game.MoveNr + 1;
            }
        }

        public Player(Game game, PlayerModel playerModel)
        {
            Game = game;
            PlayerModel = playerModel;
            Control = new ControlAnt(game, playerModel, game.GameModel);
        }


        //public Dictionary<Position2, Tile> ForeignBorderTiles = new Dictionary<Position2, Tile>();
        
        public void UpdateAll(List<Move> returnMoves)
        {
            /*
            Move move;
            if (this.VisiblePositions.Count > 0)
            {
                move = new Move();
                move.MoveType = MoveType.VisibleTiles;
                move.PlayerId = this.PlayerModel.Id;
                move.Positions = new List<Position2>();
                move.Positions.AddRange(this.VisiblePositions);
                returnMoves.Add(move);
            }
            foreach (PlayerUnit playerUnit in Units.Values)
            {
                move = new Move();
                move.MoveType = MoveType.Add;
                move.PlayerId = this.PlayerModel.Id;
                move.Positions = new List<Position2>();
                move.Positions.Add(playerUnit.Unit.Pos);
                move.UnitId = playerUnit.Unit.UnitId;
                move.Stats = playerUnit.Unit.CollectStats();
                returnMoves.Add(move);
            }*/
        }

        internal void ProcessMoves(List<Move> moves)
        {
            // Update postion of all units
            //Dictionary<Position2, Unit> deletedUnits = new Dictionary<Position2, Unit>();
            //List<Position2> removedUnits = new List<Position2>();
            //Dictionary<Position2, Unit> movedAwayUnits = new Dictionary<Position2, Unit>();
            //List<Position2> changedUnits = new List<Position2>();
            //LastMoves = new List<Move>();
            
            //Dictionary<Position2, Unit> addedUnits = new Dictionary<Position2, Unit>();
            //Dictionary<Position2, Unit> movedToUnits = new Dictionary<Position2, Unit>();
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Add)
                {
                    Position2 to = move.Positions[move.Positions.Count - 1];
                    
                    //if (this.PlayerUnits.ContainsKey(move.UnitId))
                    //    throw new Exception();
                    //Unit unit = Game.Map.Units.GetUnitAt(to);
                    //PlayerUnit playerUnit = new PlayerUnit(unit);
                    //PlayerUnits.Add(move.UnitId, playerUnit);*/
                    //if (unit != null)
                    {
                        //addedUnits.Add(to, unit);
                        //changedUnits.Add(to);
                    }
                }

                if (move.MoveType == MoveType.Build)
                {
                    Position2 to = move.Positions[move.Positions.Count - 1];

                    if (move.PlayerId == PlayerModel.Id)
                    {
                        //Unit unit = Game.Map.Units.GetUnitAt(to);
                        //PlayerUnit playerUnit = new PlayerUnit(unit);
                        //PlayerUnits.Add(move.UnitId, playerUnit);
                        //if (unit != null)
                        {
                            //addedUnits.Add(to, unit);
                            //changedUnits.Add(to);
                        }
                    }
                }
                if (move.MoveType == MoveType.Upgrade)
                {
                    //Position2 to = move.Positions[move.Positions.Count - 1];
                    //if (move.PlayerId == PlayerModel.Id)
                    {

                    }
                }
                if (move.MoveType == MoveType.Move)
                {
                    //Position2 to;
                    if (move.Positions.Count == 1)
                    {
                        // Cloud not move this unit
                    }
                    else
                    {
                        //to = move.Positions[move.Positions.Count-1];
                        //Unit unit = Game.Map.Units.GetUnitAt(to);
                        //if (this.PlayerUnits.ContainsKey(move.UnitId))
                        {
                            //PlayerUnit playerUnit = PlayerUnits[move.UnitId];

                            //Position2 from = move.Positions[0];
                            //movedAwayUnits.Add(from, unit);
                            //movedToUnits.Add(to, unit);
                            //changedUnits.Add(to);
                        }
                    }
                }
                if (move.MoveType == MoveType.Delete)
                {
                    //Position2 from = move.Positions[move.Positions.Count - 1];
                    //Unit unit = Game.Map.Units.GetUnitAt(from);
                    //if (this.PlayerUnits.ContainsKey(move.UnitId))
                    {
                        //PlayerUnit playerUnit = this.PlayerUnits[move.UnitId];
                        //deletedUnits.Add(from, unit);
                        //removedUnits.Add(from);
                        //this.PlayerUnits.Remove(move.UnitId);
                    }
                }
            }

#if OLDVISIBLE
            List<Position2> previouslySeen = new List<Position2>();
            List<Position2> newlySeen = new List<Position2>();
            foreach (Position2 pos in movedAwayUnits.Keys)
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
            hiddenTilesMove.Positions = new List<Position2>();

            foreach (Position2 oldPos in previouslySeen)
            {
                // Not seen by another unit
                if (!newlySeen.Contains(oldPos))
                {
                    // Did we see that before?
                    if (this.VisiblePositions.Contains(oldPos))
                    {
                        // Now we do not see it any more
                        VisiblePositions.Remove(oldPos);
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
            newVisibleTilesMove.Positions = new List<Position2>();
            foreach (Position2 newPos in newlySeen)
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
                            Units.Add(unit.UnitId, playerUnit);
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
                                    addMove.Positions = new List<Position2>();
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
                                deleteMove.Positions = new List<Position2>();
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
                                addMove.Position2s = new List<Position2>();
                                addMove.Position2s.Add(move.Position2s[1]);
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
                                deleteMove.Positions = new List<Position2>();
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
                else if (move.MoveType == MoveType.Transport)
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
                                addMove.Positions = new List<Position2>();
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
                        deleteMove.Positions = new List<Position2>();
                        deleteMove.Positions.Add(unit.Pos);
                        deleteMove.UnitId = unit.UnitId;
                        if (Units.ContainsKey(unit.UnitId))
                            Units.Remove(unit.UnitId);
                        LastMoves.Add(deleteMove);
                    }
                }
            }
            foreach (Unit unit in invisibleUnits)
            {
                Units.Remove(unit.UnitId);
            }

            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.UpdateGround)
                {
                    Position2 pos = move.Positions[0];
                    Tile tile = Game.Map.GetTile(pos);

                    if (VisiblePositions.Contains(pos))
                    {
                        /*
                        if (tile.Minerals > 0)
                        {
                            Game.Pheromones.DropPheromones(this, pos, 10, PheromoneType.Mineral, 0.03f, 0.01f);

                            AntCollect antCollect;
                            if (!AntCollects.TryGetValue(tile.ZoneId, out antCollect))
                            {
                                antCollect = new AntCollect();
                                AntCollects.Add(tile.ZoneId, antCollect);
                            }
                            antCollect.Minerals += tile.Minerals;
                        }*/
                        LastMoves.Add(move);
                    }
                    
                }
            }


#if DEBUG
            CheckUnits();
#endif
#endif
        }

#if OLDVISIBLE
        private void CheckUnits()
        {
            //return;

            foreach (PlayerUnit playerUnit in Units.Values)
            {
                if (!VisiblePositions.Contains(playerUnit.Unit.Pos))
                    throw new Exception();
            }
            foreach (Unit unit in Game.Map.Units.List.Values)
            {
                if (VisiblePositions.Contains(unit.Pos))
                {
                    if (!Units.ContainsKey(unit.UnitId))
                    {
                        //throw new Exception();
                    }
                }
                else
                {
                    if (Units.ContainsKey(unit.UnitId))
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
#endif

        public override string ToString()
        {
            return this.PlayerModel.Name;
        }
    }
}
