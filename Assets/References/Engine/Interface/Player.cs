
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

    public class PlayerMove 
    {
        public PlayerMove(Move move)
        {
            Move = move;
        }

        public Move Move { get; private set; }
        public string NewUnitId { get; set; }

    }

    public class PlayerUnit
    {
        public Unit Unit { get; set; }
        public PlayerUnit(Unit unit)
        {
            //if (unit == null)
            //    throw new Exception("unit is null");
            Unit = unit;
        }

        public override string ToString()
        {
            return Unit.ToString();
        }
        
        internal void CollectVisiblePos(ulong pos, List<ulong> positions, bool keep)
        {
            List<ulong> calcPos = new List<ulong>();

            int visibilityRange = 4;
            if (Unit.Reactor != null)
            {
                //visibilityRange = 4; // Unit.Reactor.Range;
            }

            CollectVisiblePos(this.Unit.Game, pos, calcPos, visibilityRange);
            positions.AddRange(calcPos);
        }

        internal static void CollectVisiblePos(Game game, ulong pos, List<ulong> positions, int visibilityRange)
        {
            List<TileWithDistance> openList = new List<TileWithDistance>();
            Dictionary<ulong, TileWithDistance> reachedTiles = new Dictionary<ulong, TileWithDistance>();

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
    public class PlayerVisibleInfo
    {
        public int LastUpdated { get; set; }
    }
    public class Player
    {
        public Game Game { get; set; }

        public MapZone StartZone { get; set; }

        public IControl Control { get; set; }
        public PlayerModel PlayerModel { get; set; }
        public List<Move> LastMoves;

        internal List<GameCommand> GameCommands = new List<GameCommand>();

        // Unit that the player knows. Own and enemy
        public Dictionary<string, PlayerUnit> Units = new Dictionary<string, PlayerUnit>();
        //public Dictionary<string, PlayerUnit> UnitsInBuild = new Dictionary<string, PlayerUnit>();
        // ulongs the player sees
        public Dictionary<ulong, PlayerVisibleInfo> VisiblePositions = new Dictionary<ulong, PlayerVisibleInfo>();

        public bool IsVisible(ulong pos)
        {
            PlayerVisibleInfo playerVisibleInfo;
            if (VisiblePositions.TryGetValue(pos, out playerVisibleInfo))
            {
                    return true;
            }
            return false;
        }

        internal void CollectVisiblePos(PlayerUnit playerUnit)
        {
            List<ulong> calcPos = new List<ulong>();

            int visibilityRange = 4;
            if (playerUnit.Unit.Reactor != null)
            {
                visibilityRange = 4; // Unit.Reactor.Range;
            }
            Dictionary<ulong, TileWithDistance> tiles = Game.Map.EnumerateTiles(playerUnit.Unit.Pos, visibilityRange, true);
            foreach (TileWithDistance tileWithDistance in tiles.Values)
            {
                PlayerVisibleInfo playerVisibleInfo;
                if (VisiblePositions.TryGetValue(tileWithDistance.Pos, out playerVisibleInfo))
                {

                }
                else
                {
                    playerVisibleInfo = new PlayerVisibleInfo();
                    VisiblePositions.Add(tileWithDistance.Pos, playerVisibleInfo);

                    if (!Game.changedGroundPositions.ContainsKey(tileWithDistance.Pos))
                        Game.changedGroundPositions.Add(tileWithDistance.Pos, tileWithDistance.Tile);
                }
                playerVisibleInfo.LastUpdated = Game.MoveNr;
            }
        }

        public bool CanProduceMoreUnits()
        {
            return true;
            /*
            if (NumberOfUnits > 10)
                return false;
            if (NumberOfUnits < 5 + this.Visibleulongs.Count / 20)
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
            Control = new ControlAnt(game, playerModel, game.GameModel);
        }


        public Dictionary<ulong, Tile> ForeignBorderTiles = new Dictionary<ulong, Tile>();
        
        public void UpdateAll(List<Move> returnMoves)
        {
            /*
            Move move;
            if (this.VisiblePositions.Count > 0)
            {
                move = new Move();
                move.MoveType = MoveType.VisibleTiles;
                move.PlayerId = this.PlayerModel.Id;
                move.Positions = new List<ulong>();
                move.Positions.AddRange(this.VisiblePositions);
                returnMoves.Add(move);
            }
            foreach (PlayerUnit playerUnit in Units.Values)
            {
                move = new Move();
                move.MoveType = MoveType.Add;
                move.PlayerId = this.PlayerModel.Id;
                move.Positions = new List<ulong>();
                move.Positions.Add(playerUnit.Unit.Pos);
                move.UnitId = playerUnit.Unit.UnitId;
                move.Stats = playerUnit.Unit.CollectStats();
                returnMoves.Add(move);
            }*/
        }



        private int moveNr;

        internal void ProcessMoves(List<Move> moves)
        {
            // Update postion of all units
            Dictionary<ulong, PlayerUnit> deletedUnits = new Dictionary<ulong, PlayerUnit>();
            List<ulong> removedUnits = new List<ulong>();
            Dictionary<ulong, PlayerUnit> movedAwayUnits = new Dictionary<ulong, PlayerUnit>();
            List<ulong> changedUnits = new List<ulong>();
            LastMoves = new List<Move>();

            moveNr++;
            
            
            Dictionary<ulong, PlayerUnit> addedUnits = new Dictionary<ulong, PlayerUnit>();
            Dictionary<ulong, PlayerUnit> movedToUnits = new Dictionary<ulong, PlayerUnit>();
            foreach (Move move in moves)
            {
                if (move.MoveType == MoveType.Add)
                {
                    ulong to = move.Positions[move.Positions.Count - 1];
                    if (this.Units.ContainsKey(move.UnitId))
                        throw new Exception();
                    Unit unit = Game.Map.Units.GetUnitAt(to);
                    PlayerUnit playerUnit = new PlayerUnit(unit);
                    Units.Add(move.UnitId, playerUnit);
                    if (unit != null)
                    {
                        addedUnits.Add(to, playerUnit);
                        changedUnits.Add(to);
                    }

                    //CollectVisiblePos(playerUnit);
                }

                if (move.MoveType == MoveType.Build)
                {
                    ulong to = move.Positions[move.Positions.Count - 1];

                    if (move.PlayerId == PlayerModel.Id)
                    {
                        Unit unit = Game.Map.Units.GetUnitAt(to);
                        PlayerUnit playerUnit = new PlayerUnit(unit);
                        Units.Add(move.UnitId, playerUnit);
                        if (unit != null)
                        {
                            addedUnits.Add(to, playerUnit);
                            changedUnits.Add(to);
                        }
                    }
                }
                if (move.MoveType == MoveType.Upgrade)
                {
                    ulong to = move.Positions[move.Positions.Count - 1];
                    if (move.PlayerId == PlayerModel.Id)
                    {
                        /*
                        if (UnitsInBuild.ContainsKey(move.OtherUnitId))
                        {
                            PlayerUnit playerUnit = UnitsInBuild[move.OtherUnitId];
                            addedUnits.Add(to, playerUnit);
                            Units.Add(playerUnit.Unit.UnitId, playerUnit);
                            changedUnits.Add(to);
                            UnitsInBuild.Remove(move.OtherUnitId);
                        }*/
                    }
                }
                if (move.MoveType == MoveType.Move)
                {
                    ulong to;
                    if (move.Positions.Count == 1)
                    {
                        // Cloud not move this unit
                    }
                    else
                    {
                        to = move.Positions[move.Positions.Count-1];

                        if (this.Units.ContainsKey(move.UnitId))
                        {
                            PlayerUnit playerUnit = Units[move.UnitId];
                            /*
                            Unit unit = Game.Map.Units.GetUnitAt(to);
                            PlayerUnit playerUnit = new PlayerUnit(unit);
                            Units.Add(move.UnitId, playerUnit);*/

                            ulong from = move.Positions[0];
                            movedAwayUnits.Add(from, playerUnit);
                            movedToUnits.Add(to, playerUnit);
                            changedUnits.Add(to);

                            //CollectVisiblePos(playerUnit);
                        }
                    }
                }
                if (move.MoveType == MoveType.Delete)
                {
                    ulong from = move.Positions[move.Positions.Count - 1];
                    if (this.Units.ContainsKey(move.UnitId))
                    {
                        PlayerUnit playerUnit = this.Units[move.UnitId];
                        deletedUnits.Add(from, playerUnit);
                        removedUnits.Add(from);
                        this.Units.Remove(move.UnitId);
                    }
                }
            }
            Dictionary<ulong, PlayerVisibleInfo> previousVisibleTiles = VisiblePositions;

            VisiblePositions = new Dictionary<ulong, PlayerVisibleInfo>();
            foreach (PlayerUnit playerUnit1 in this.Units.Values)
            {
                CollectVisiblePos(playerUnit1);
            }
            foreach (ulong pos in previousVisibleTiles.Keys)
            {
                if (!VisiblePositions.ContainsKey(pos))
                {
                    if (!Game.changedGroundPositions.ContainsKey(pos))
                        Game.changedGroundPositions.Add(pos, null);
                }
            }
#if OLDVISIBLE
            List<ulong> previouslySeen = new List<ulong>();
            List<ulong> newlySeen = new List<ulong>();
            foreach (ulong pos in movedAwayUnits.Keys)
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
            hiddenTilesMove.Positions = new List<ulong>();

            foreach (ulong oldPos in previouslySeen)
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
            newVisibleTilesMove.Positions = new List<ulong>();
            foreach (ulong newPos in newlySeen)
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
                                    addMove.Positions = new List<ulong>();
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
                                deleteMove.Positions = new List<ulong>();
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
                                addMove.ulongs = new List<ulong>();
                                addMove.ulongs.Add(move.ulongs[1]);
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
                                deleteMove.Positions = new List<ulong>();
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
                                addMove.Positions = new List<ulong>();
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
                        deleteMove.Positions = new List<ulong>();
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
                    ulong pos = move.Positions[0];
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
#endif

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
