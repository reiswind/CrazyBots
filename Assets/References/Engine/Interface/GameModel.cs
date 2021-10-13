﻿using Engine.Master;
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
        //Move,
        //AttackMove,
        Attack,
        Defend,
        Scout,
        Cancel,
        Collect,
        Build,
        Extract,
        Pipeline
    }

    public class GameCommand
    {
        public GameCommand()
        {
            AttachedUnits = new List<string>();        
        }
        public bool CommandComplete { get; set; }
        public bool WaitingForUnit { get; set; }
        public int PlayerId { get; set; }
        public int TargetZone { get; set; }
        public string UnitId { get; set; } // Which unit to build, extract...
        public Position TargetPosition { get; set; }
        public GameCommandType GameCommandType { get; set; }
        public BlueprintCommand BlueprintCommand { get; set; }
        public List<string> AttachedUnits { get; private set; }

        internal GameCommand AttachToThisOnCompletion { get; set; }
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
        List<Move> ProcessMove(int playerId, Move myMove, List<GameCommand> gameCommands);

        void ComputePossibleMoves(Position pos, List<Move> possibleMoves, List<Position> includedPositions, MoveFilter moveFilter);
        Move MoveTo(Position From, Position To, Master.Engine engine);
        List<Position> FindPath(Position from, Position to, Unit unit);
        Dictionary<int, Player> Players { get; }
        int Seed { get; }
        Random Random { get; }
        Tile GetTile(Position p);
        Map Map { get; }
        Blueprints Blueprints { get; }
        List<Area> Areas { get; }

        void CreateUnits();
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
        public Position Position { get; set; }
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
