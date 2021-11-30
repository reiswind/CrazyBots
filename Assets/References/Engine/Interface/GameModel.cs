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
        Extract,
        ItemRequest
    }
    internal class GameCommandItemUnit
    {
        public GameCommandItemUnit()
        {
            Status = "Created";
        }

        public int StuckCounter { get; set; }
        internal void SetStatus(string text, bool alert = false)
        {
            if (Status != text)
                StuckCounter = 0;
            Status = text;
            Alert = alert;
        }
        internal void ResetStatus()
        {
            StuckCounter = 0;
            Status = null;
            Alert = false;
        }

        public string UnitId { get; set; }
        public string Status { get; private set; }
        public bool Alert { get; private set; }

        public override string ToString()
        {
            if (UnitId == null) return "Nothing";
            if (!Alert)
                return UnitId + " " + Status;
            return UnitId + " " + Status + " ALERT";
        }
    }

    internal class GameCommandItem
    {
        internal GameCommandItem(GameCommand gamecommand)
        {
            AttachedUnit = new GameCommandItemUnit();
            FactoryUnit = new GameCommandItemUnit();
            TargetUnit = new GameCommandItemUnit();
            TransportUnit = new GameCommandItemUnit();

            GameCommand = gamecommand;
        }
        internal GameCommandItem(GameCommand gamecommand, BlueprintCommandItem blueprintCommandItem)
        {
            AttachedUnit = new GameCommandItemUnit();
            FactoryUnit = new GameCommandItemUnit();
            TargetUnit = new GameCommandItemUnit();
            TransportUnit = new GameCommandItemUnit();

            BlueprintName = blueprintCommandItem.BlueprintName;
            Position3 = blueprintCommandItem.Position3;
            Direction = blueprintCommandItem.Direction;
            GameCommand = gamecommand;
        }
        // Runtime info
        internal GameCommand GameCommand { get; private set; }

        internal Direction Direction { get; set; }
        internal Position3 Position3 { get; set; }

        public Position3 RotatedPosition3 { get; set; }
        public Direction RotatedDirection { get; set; }
        internal string BlueprintName { get; set; }

        internal GameCommandItemUnit AttachedUnit { get; private set; }
        internal GameCommandItemUnit TransportUnit { get; private set; }
        internal GameCommandItemUnit TargetUnit { get; private set; }
        internal GameCommandItemUnit FactoryUnit { get; private set; }

        public bool DeleteWhenDestroyed { get; set; }
        public bool FollowPheromones { get; set; }



        public override string ToString()
        {
            return GameCommand.ToString();
        }
    }
    internal class GameCommand
    {
        public GameCommand()
        {
            GameCommandItems = new List<GameCommandItem>();
        }
        public GameCommand(BlueprintCommand blueprintCommand)
        {
            Layout = blueprintCommand.Layout;
            BlueprintName = blueprintCommand.Name;
            GameCommandItems = new List<GameCommandItem>();
            GameCommandType = blueprintCommand.GameCommandType;
            foreach (BlueprintCommandItem blueprintCommandItem in blueprintCommand.Units)
            {
                GameCommandItem gameCommandItem = new GameCommandItem(this, blueprintCommandItem);
                GameCommandItems.Add(gameCommandItem);
            }
        }
        public string Layout { get; set; }
        public string BlueprintName { get; set; }
        
        public bool CommandComplete { get; set; }
        public bool DeleteWhenFinished { get; set; }
        public bool CommandCanceled { get; set; }
        public int PlayerId { get; set; }
        public int TargetZone { get; set; }
        public int Radius { get; set; }
        public Position2 TargetPosition { get; set; }
        public Position2 MoveToPosition { get; set; }
        public GameCommandType GameCommandType { get; set; }
        public List<GameCommandItem> GameCommandItems { get; private set; }
        public List<RecipeIngredient> RequestedItems { get; set; }
        internal Dictionary<Position2, TileWithDistance> IncludedPositions { get; set; }

        public override string ToString()
        {
            string s = GameCommandType.ToString() + " at " + TargetPosition.ToString();
            if (CommandCanceled) s += " Canceled";
            if (CommandComplete) s += " Complete";

            foreach (GameCommandItem blueprintCommandItem in GameCommandItems)
            {
                s += blueprintCommandItem.BlueprintName;
                s += " ";
                if (blueprintCommandItem.FactoryUnit.UnitId != null)
                    s += " Factory" + blueprintCommandItem.FactoryUnit.UnitId;
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

        void ComputePossibleMoves(Position2 pos, List<Move> possibleMoves, List<Position2> includedPosition2s, MoveFilter moveFilter);
        //Move MoveTo(Position2 From, Position2 To, Master.Engine engine);
        List<Position2> FindPath(Position2 from, Position2 to, Unit unit, bool ignoreIfToIsOccupied = false);
        Dictionary<int, Player> Players { get; }
        int Seed { get; }
        Tile GetTile(Position2 p);
        Map Map { get; }
        Blueprints Blueprints { get; }
        void CreateUnits();
        void CollectGroundStats(Position2 pos, Move move, List<TileObject> tileObjects);
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
        public int? ContainerFilled { get; set; }
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
