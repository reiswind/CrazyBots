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
        //Scout,
        Cancel,
        //Move,
        Collect,
        Build,
        //AddUnits,
        Extract,
        ItemRequest,
        ItemOrder,
        Fire,
        Automate
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
        List<Position2> FindPath(Position2 from, Position2 to, string unitId, bool ignoreIfToIsOccupied = false);
        Dictionary<int, Player> Players { get; }
        int Seed { get; }
        Tile GetTile(Position2 p);
        Map Map { get; }
        Blueprints Blueprints { get; }
        void CreateUnits();
        void CollectGroundStats(Position2 pos, Move move);
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
        public string Direction { get; set; }

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
        public bool MarkForExtraction { get; set; }
        [DataMember]
        public int? ContainedMinerals { get; set; }
        [DataMember]
        public int? ContainedWood { get; set; }
        [DataMember]
        public int? ContainedStones { get; set; }
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
