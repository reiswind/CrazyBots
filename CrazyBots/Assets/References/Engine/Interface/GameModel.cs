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
        Move,
        AttackMove,
        Attack,
        Minerals
    }

    public class GameCommand
    {
        public string UnitId { get; set; }
        //public Position UnitPosition { get; set; }
        public Position TargetPosition { get; set; }
        public GameCommandType GameCommandType { get; set; }
        public bool Append { get; set; }
    }

    public enum MoveFilter
    {
        Fire = 0x0001,
        Move = 0x0002,
        Assemble = 0x0004,
        Upgrade = 0x0008,
        Extract = 0x0020,
        All = Fire | Move | Assemble  | Extract
    }

    public enum MoveToOptions
    {
        None
    }


    public interface IGameController
    {
        MapInfo GetDebugMapInfo();
        List<Move> ProcessMove(int playerId, Move myMove, List<GameCommand> gameCommands);

        void ComputePossibleMoves(Position pos, List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter);
        Move MoveTo(Position From, Position To, Master.Engine engine);
        List<Position> FindPath(Position from, Position to, Unit unit);
        Dictionary<int, Player> Players { get; }
        int Seed { get; }
        Random Random { get; }
        Tile GetTile(Position p);
        Map Map { get; }
        List<Area> Areas { get; }
    }

    [DataContract]
    public class GameModel
    {
        [DataMember]
        public int? Seed { get; set; }
        [DataMember]
        public string Name { get; set; }
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
        public void Add(GameModel gamePart)
        {
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
        public Position Position { get; set; }
        [DataMember]
        public string Parts { get; set; }
        [DataMember]
        public string Blueprint { get; set; }
    }

    [DataContract]
    public class PlayerModel
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int ControlLevel { get; set; }

        public override string ToString()
        {
            return Name + " (" + Id + ")";
        }
    }
}
