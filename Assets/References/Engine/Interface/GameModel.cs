using Engine.Master;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public enum GameCommandType
    {
        None,
        Attack,
        Defend,
        Scout,
        Cancel,
        Move,
        Collect,
        Build,
        AddUnits,
        Extract
    }
    internal class GameCommandItem
    {
        internal GameCommandItem(GameCommand gamecommand, BlueprintCommandItem blueprintCommandItem)
        {
            GameCommand = gamecommand;
            BlueprintCommandItem = blueprintCommandItem;
        }
        // Runtime info
        internal string AttachedUnitId { get; set; }
        internal BlueprintCommandItem BlueprintCommandItem { get; private set; }
        internal GameCommand GameCommand { get; private set; }
        internal string FactoryUnitId { get; set; }

        public override string ToString()
        {
            return GameCommand.ToString();
        }
    }
    internal class GameCommand
    {
        public GameCommand(BlueprintCommand blueprintCommand)
        {
            BlueprintCommand = blueprintCommand;
            GameCommandItems = new List<GameCommandItem>();
            GameCommandType = blueprintCommand.GameCommandType;
            foreach (BlueprintCommandItem blueprintCommandItem in blueprintCommand.Units)
            {
                GameCommandItem gameCommandItem = new GameCommandItem(this, blueprintCommandItem);
                GameCommandItems.Add(gameCommandItem);
            }
        }
        public string Status { get; set; }
        public bool CommandComplete { get; set; }
        public bool DeleteWhenFinished { get; set; }
        public bool CommandCanceled { get; set; }
        public int PlayerId { get; set; }
        public int TargetZone { get; set; }
        public int Radius { get; set; }
        public ulong TargetPosition { get; set; }
        public ulong MoveToPosition { get; set; }
        public GameCommandType GameCommandType { get; set; }
        public BlueprintCommand BlueprintCommand { get; private set; }
        public List<GameCommandItem> GameCommandItems { get; private set; }

        internal Dictionary<ulong, TileWithDistance> IncludedPositions { get; set; }

        public override string ToString()
        {
            string s = GameCommandType.ToString() + " at " + Position.GetX(TargetPosition) + "," + Position.GetY(TargetPosition);
            if (CommandCanceled) s += " Canceld";
            if (CommandComplete) s += " Complete";

            foreach (GameCommandItem blueprintCommandItem in GameCommandItems)
            {
                s += blueprintCommandItem.BlueprintCommandItem.BlueprintName;
                s += " ";
                if (blueprintCommandItem.FactoryUnitId != null)
                    s += " Factory" + blueprintCommandItem.FactoryUnitId;
                s += "\r\n";
            }
            return s;
        }
    }

    public enum MoveFilter
    {
        Fire = 0x0001,
        Move = 0x0002,
        Assemble = 0x0004,
        Upgrade = 0x0008,
        Extract = 0x0020,
        Transport = 0x0040,
        All = Fire | Move | Assemble  | Extract
    }

    public enum MoveToOptions
    {
        None
    }

    public interface IGameController
    {
        MapInfo GetDebugMapInfo();
        List<Move> ProcessMove(int playerId, Move myMove, List<MapGameCommand> gameCommands);

        void ComputePossibleMoves(ulong pos, List<Move> possibleMoves, List<ulong> includedulongs, MoveFilter moveFilter);
        //Move MoveTo(ulong From, ulong To, Master.Engine engine);
        List<ulong> FindPath(ulong from, ulong to, Unit unit, bool ignoreIfToIsOccupied = false);
        Dictionary<int, Player> Players { get; }
        int Seed { get; }
        Tile GetTile(ulong p);
        Map Map { get; }
        Blueprints Blueprints { get; }
        void CreateUnits();
        void CollectGroundStats(ulong pos, Move move, List<TileObject> tileObjects);
    }

    [DataContract]
    public class GameModel
    {
        [DataMember]
        public int? Seed { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string MapType { get; set; }
        [DataMember]
        public List<PlayerModel> Players { get; set;  }
        [DataMember]
        public int MapWidth { get; set; }
        [DataMember]
        public int MapHeight { get; set; }
        [DataMember]
        public List<UnitModel> Units { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public IGameController CreateGame(int seed)
        {
            return new Game(this, seed);
        }
        public IGameController CreateGame()
        {
            return new Game(this);
        }
    }

    [DataContract]
    public class UnitModel
    {
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public string Position { get; set; }
        [DataMember]
        public string Parts { get; set; }
        [DataMember]
        public string Blueprint { get; set; }

        [DataMember]
        public bool HoldPosition { get; set; }
        [DataMember]
        public bool HoldFire { get; set; }
        [DataMember]
        public bool FireAtGround { get; set; }
        [DataMember]
        public bool EndlessAmmo { get; set; }
        [DataMember]
        public bool EndlessPower { get; set; }
        [DataMember]
        public bool UnderConstruction { get; set; }
        [DataMember]
        public int ContainerFilled { get; set; }
    }

    [DataContract]
    public class PlayerModel
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Zone { get; set; }
        [DataMember]
        public int ControlLevel { get; set; }
        [DataMember]
        public bool IsHuman { get; set; }

        public override string ToString()
        {
            return Name + " (" + Id + ")";
        }
    }
}
