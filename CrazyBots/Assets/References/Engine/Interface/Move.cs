using Newtonsoft.Json;
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
        Transport,
        Hit,
        UpdateStats,
        Delete,
        UpdateGround,
        Assemble,
        Extract,
        CommandComplete,

        VisibleTiles,
        HiddenTiles,
        UpdateAll
    }

    public class MoveUpdateUnitPart
    {
        public string Name { get; set; }
        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public int CompleteLevel { get; set; }
        

        // True if the part exists
        public bool Exists { get; set; }
        public bool? ShieldActive { get; set; }
        public int? ShieldPower { get; set; }
        
        public List<TileObject> TileObjects { get; set; }
        public int? Capacity { get; set; }
        public int? AvailablePower { get; set; }
        public List<string> BildQueue { get; set; }
    }

    public class MoveUpdateGroundStat
    {
        [DataMember]
        public int Owner { get; set; }
        [DataMember]
        public bool IsBorder { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int PlantLevel { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int TerrainTypeIndex { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsUnderwater { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public float Height { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<TileObject> TileObjects { get; set; }
    }

    public class MoveUpdateStatsCommand
    {
        public Position TargetPosition { get; set; }
        public GameCommandType GameCommandType { get; set; }
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

        [DataMember(EmitDefaultValue = false)]
        public MoveUpdateStatsCommand MoveUpdateStatsCommand { get; set; }


        [DataMember]
        public bool MarkedForExtraction { get; set; }

        public int Power { get; set; }
    }

    [DataContract]
    public class Move
    {
        public Move()
        {

        }

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
