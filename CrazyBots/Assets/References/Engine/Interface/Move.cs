﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public enum MoveType
    {
        None,
        Skip,

        Add,
        Build,
        Upgrade,
        Move,
        Fire,
        Hit,
        UpdateStats,
        Delete,
        UpdateGround,
        Assemble,
        Extract,

        VisibleTiles,
        HiddenTiles,
        UpdateAll
    }

    public class MoveUpdateUnitPart
    {
        public string Name { get; set; }

        // True if the part exists
        public bool Exists { get; set; }
        // Minerals in part
        public int? Minerals { get; set; }
        public int? Capacity { get; set; }
    }
    public class MoveUpdateGroundStat
    {
        [DataMember(EmitDefaultValue = false)]
        public int Minerals { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int NumberOfDestructables { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int NumberOfObstacles { get; set; }
    }
    public class MoveUpdateStats
    {
        public MoveUpdateStats()
        {

        }
        [DataMember(EmitDefaultValue =false)]
        public string BlueprintName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<MoveUpdateUnitPart> UnitParts { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public MoveUpdateGroundStat MoveUpdateGroundStat { get; set; }

        [DataMember]
        public int EngineLevel { get; set; }


        [DataMember]
        public int ArmorLevel { get; set; }


        [DataMember]
        public int WeaponLevel { get; set; }


        [DataMember]
        public bool WeaponLoaded { get; set; }


        [DataMember]
        public int ProductionLevel { get; set; }


        [DataMember]
        public bool CanProduce { get; set; }

        [DataMember]
        public int ExtractorLevel { get; set; }
        [DataMember]
        public int ContainerLevel { get; set; }
        [DataMember]
        public int ContainerFull { get; set; }


        [DataMember]
        public int RadarLevel { get; set; }
        [DataMember]
        public int ReactorLevel { get; set; }

        public int Power { get; set; }

    }

    [DataContract]
    public class Move
    {
        public Move()
        {

        }

        internal int Priority { get; set; }
        /// <summary>
        /// The Player who made that move
        /// </summary>
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public MoveType MoveType { get; set; }
        /// <summary>
        /// The route
        /// Add = moving in route route Dest: [n-1]
        /// Move = move to route From: [0] to Dest: [1] current pos
        /// Delete = Delete Unit at [0]
        /// Hit = Unit hit at [0]
        /// Fire = From at [0]
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<Position> Positions { get; set; }

        /// <summary>
        /// Add=new Unitmodel
        /// Move=what it is
        /// Delete=what it is
        /// Hit=what it is
        /// Fire=unit that fired
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UnitId { get; set; }

        /// <summary>
        /// Add=Factory made it
        /// Move=null
        /// Delete=null
        /// Hit = Hit by what?
        /// Fire=Hit by what?
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string OtherUnitId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public MoveUpdateStats Stats { get; set; }
        /// <summary>
        /// Text of unit
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MoveType.ToString() + " " + UnitId + "(" + PlayerId + ")";
        }
    }
}
