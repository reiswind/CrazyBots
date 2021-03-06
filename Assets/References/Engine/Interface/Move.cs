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
        /// <summary>
        /// Place unit at start of game
        /// </summary>
        Add,
        /// <summary>
        /// Create units in game
        /// </summary>
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
        Command,
        Burn,
        VisibleTiles,
        HiddenTiles,
        UpdateAll
    }

    public class MoveUpdateUnitPart
    {
        public MoveUpdateUnitPart()
        {

        }
        public string Name { get; set; }
        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public int Range { get; set; }
        public int CompleteLevel { get; set; }
        

        // True if the part exists
        public bool Exists { get; set; }
        public bool? ShieldActive { get; set; }
        public int? ShieldPower { get; set; }
        
        public List<TileObject> TileObjects { get; set; }
        public int? Capacity { get; set; }
        public int? AvailablePower { get; set; }
    }

    public class MoveUpdateGroundStat
    {
        public MoveUpdateGroundStat()
        {

        }

        [DataMember]
        public int Owner { get; set; }

        [DataMember]
        public bool IsBorder { get; set; }
        /*
        [DataMember(EmitDefaultValue = false)]
        public int PlantLevel { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int TerrainTypeIndex { get; set; }
        */
        [DataMember(EmitDefaultValue = false)]
        public bool IsUnderwater { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float Height { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<TileObject> TileObjects { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsOpenTile { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int ZoneId { get; set; }

        private int Count(TileObjectType tileObjectType)
        {
            int count = 0;
            if (TileObjects != null)
            {
                foreach (TileObject tileObject in TileObjects)
                {
                    if (tileObject.TileObjectType == tileObjectType)
                        count++;
                }
            }
            return count;
        }

        public bool IsHill()
        {
            return false;
            //return TerrainTypeIndex >= 4;
            //return Height > 0.8 && Height <= 0.9;
        }
        public bool IsRock()
        {
            return false;
            //return TerrainTypeIndex >= 4;
            //return Height > 0.7 && Height <= 0.8;
        }

        public bool IsDarkWood()
        {
            if (Count(TileObjectType.Tree) >= 6)
                return true;
            return false;
            //return PlantLevel <= 4 && TerrainTypeIndex == 3;
            //return Height > 0.6 && Height <= 0.7;
        }

        public bool IsWood()
        {
            if (Count(TileObjectType.Tree) >= 4)
                    return true;
            return false;
            //return PlantLevel == 2 && TerrainTypeIndex == 3;
            //return Height > 0.5 && Height <= 0.6;
        }

        public bool IsLightWood()
        {
            if (Count(TileObjectType.Tree) >= 2)
                return true;
            return false;
            //if (Count(TileObjectType.Bush) >= 2 || Count(TileObjectType.Tree) == 1)
            //    return true;
            //return PlantLevel <= 1 && TerrainTypeIndex == 3;
            //return Height > 0.4 && Height <= 0.5;
        }
        public bool IsGrassDark()
        {
            return false;
            //if (Count(TileObjectType.Bush) == 1 || Count(TileObjectType.Bush) == 2)
            //    return true;
            //return PlantLevel > 1 && TerrainTypeIndex == 1;
            //return Height > 0.3 && Height <= 0.4;
        }
        public bool IsGras()
        {
            if (Count(TileObjectType.Gras) >= 6)
                return true;
            return false;
            //return PlantLevel <= 1 && TerrainTypeIndex == 1;
            //return Height > 0.20 && Height <= 0.3;
        }
        public bool IsDarkSand()
        {
            if (Count(TileObjectType.Stone) > 0)
                return true;
            return false;
            //return TerrainTypeIndex == 0;
            //return PlantLevel > 1 && TerrainTypeIndex == 0;
            //return Height > 0.1 && Height <= 0.20;
        }
        public bool IsSand()
        {
            if (Count(TileObjectType.Sand) > 0)
                return true;
            return false;
            //return TerrainTypeIndex == 1 && PlantLevel == 3;
        }
        public bool IsDirt()
        {
            return true;
            //return PlantLevel == 0 && TerrainTypeIndex == 0;
        }
    }
    [DataContract]
    public class MoveRecipeIngredient
    {
        [DataMember(EmitDefaultValue = false)]
        public TileObjectType Source { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Position2 SourcePosition { get; set; }
        public string SourceUnitId { get; set; }


        [DataMember(EmitDefaultValue = false)]
        public TileObjectType Target { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Position2 TargetPosition { get; set; }
       

        [DataMember(EmitDefaultValue = false)]
        public TileObjectType TileObjectType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public TileObjectKind TileObjectKind { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Count { get; set; }



        public override string ToString()
        {
            return Count.ToString() + "x " +  TileObjectType.ToString() + " at " + SourcePosition.ToString();
        }
    }
    [DataContract]
    public class MoveRecipe
    {
        public MoveRecipe()
        {
            Ingredients = new List<MoveRecipeIngredient>();
        }
        [DataMember(EmitDefaultValue = false)]
        public List<MoveRecipeIngredient> Ingredients { get;  set; }

        [DataMember(EmitDefaultValue = false)]
        public TileObjectType Result { get; set; }
    }
    [DataContract]
    public class MoveUpdateStatsCommand
    {
        [DataMember(EmitDefaultValue = false)]
        public Position2 TargetPosition { get; set; }
        [DataMember(EmitDefaultValue = false)] 
        public GameCommandType GameCommandType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public GameCommandState GameCommandState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public MapGameCommandItemUnit AttachedUnit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public MapGameCommandItemUnit FactoryUnit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public MapGameCommandItemUnit TargetUnit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public MapGameCommandItemUnit TransportUnit { get; set; }
    }
    [DataContract]
    public class MoveUnitItemOrder
    {
        [DataMember]
        public TileObjectType TileObjectType { get; set; }
        [DataMember]
        public TileObjectState TileObjectState { get; set; }
    }

    [DataContract]
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
        public List<MoveUnitItemOrder> MoveUnitItemOrders { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public MoveUpdateGroundStat MoveUpdateGroundStat { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public MoveUpdateStatsCommand MoveUpdateStatsCommand { get; set; }

        [DataMember]
        public bool MarkedForExtraction { get; set; }
        [DataMember]
        public int VisibilityMask { get; set; }


        [DataMember]
        public int Power { get; set; }

        [DataMember]
        public int Stunned { get; set; }

        [DataMember]
        public Direction Direction { get; set; }
        [DataMember]
        public bool Automatic { get; set; }
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
        //[JsonConverter(typeof(Position2Converter))]
        public List<Position2> Positions { get; set; }
        /// <summary>
        /// Only for Upgrade moves
        /// </summary>
        [DataMember(EmitDefaultValue = false)] 
        public MoveRecipe MoveRecipe { get; set; }        
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
        [DataMember(EmitDefaultValue = false)]

        public MapGameCommand Command { get; set; }

        /// <summary>
        /// Transfer attached command
        /// </summary>
        internal Master.GameCommand GameCommand { get; set; }
        /// <summary>
        /// Text of unit
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(MoveType.ToString());
            sb.Append(" ");
            sb.Append(UnitId);
            sb.Append(" (");
            sb.Append(PlayerId);
            sb.Append(") ");

            if (Positions != null)
            {                
                if (Positions.Count >= 1)
                    sb.Append (Positions[0].ToString());
                if (Positions.Count >= 2)
                    sb.Append (" to " + Positions[1].ToString());
            }
            if (OtherUnitId != null)
            {
                sb.Append(" ");
                sb.Append(OtherUnitId);
            }
            return sb.ToString();
        }
    }
}
