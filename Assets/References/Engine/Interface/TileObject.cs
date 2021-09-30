using Engine.Master;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class TileContainer
    {
        public TileContainer()
        {
            AcceptedTileObjectTypes = TileObjectType.All;
            tileObjects = new List<TileObject>();
        }

        private List<TileObject> tileObjects;

        public ReadOnlyCollection<TileObject> TileObjects { get { return tileObjects.AsReadOnly(); } }

        public int Minerals
        {
            get
            {
                int count = 0;
                foreach (TileObject tileObject in TileObjects)
                {
                    if (tileObject.TileObjectType == TileObjectType.Mineral)
                        count++;
                }
                return count;
            }
        }

        public void Clear()
        {
            tileObjects.Clear();
        }

        public int Count
        {
            get
            {
                return tileObjects.Count;
            }
        }

        public void AddRange(List<TileObject> ptileObjects)
        {
            foreach (TileObject tileObject in ptileObjects)
                Add(tileObject);
        }
        public void Add(TileObject tileObject)
        {
            if (Capacity != 0 &&
                !TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
            {
                //if (!TileObject.CanConvertTileObjectIntoMineral(removed.TileObjectType))
                {
                    throw new Exception();
                }
            }
            if (!Accepts(tileObject))
            {
                throw new Exception("Wrong tile type");
            }
            tileObjects.Add(tileObject);
        }
        public void Remove(TileObject tileObject)
        {
            tileObjects.Remove(tileObject);
        }

        public bool Accepts(TileObject tileObject)
        {
            if (AcceptedTileObjectTypes != TileObjectType.All)
            {
                if (AcceptedTileObjectTypes != tileObject.TileObjectType)
                    return false;
            }

            return true;
        }

        public TileObject RemoveTileObject(TileObjectType tileObjectType)
        {
            foreach (TileObject tileObject in tileObjects)
            {
                if (tileObject.TileObjectType == tileObjectType || tileObjectType == TileObjectType.All)
                {
                    tileObjects.Remove(tileObject);
                    return tileObject;
                }
            }
            return null;
        }

        public TileObject RemoveTileObjectIfFits(Unit targetUnit)
        {
            foreach (TileObject tileObject in tileObjects)
            {
                if (targetUnit.IsSpaceForTileObject(tileObject))
                {
                    tileObjects.Remove(tileObject);
                    return tileObject;
                }
            }
            return null;
        }

        public void CreateMinerals(int capacity)
        {
            while (capacity-- > 0)
            {
                TileObject tileObject = new TileObject();
                tileObject.Direction = Direction.C;
                tileObject.TileObjectType = TileObjectType.Mineral;
                tileObjects.Add(tileObject);
            }
        }

        public int Loaded
        {
            get
            {
                return TileObjects.Count(); // Mineral + Dirt;
            }
        }

        public bool IsFreeSpace
        {
            get
            {
                return TileObjects.Count() < Capacity;
            }
        }

        public int Capacity { get; set; }
        public TileObjectType AcceptedTileObjectTypes { get; set; }
    }

    public enum Direction
    {
        C,
        N,
        S,
        NE,
        NW,
        SE,
        SW
    }

    public enum TileObjectKind
    {
        None,
        LeaveTree,

        LightGras,
        DarkGras
    }

    public enum TileObjectType
    {
        None,
        All,

        // Environment
        Dirt,
        Gras,
        Bush,
        Tree,
        TreeTrunk,
        Mineral,
        Water,
        Sand,

        // Parts
        PartExtractor, 
        PartAssembler, 
        PartContainer, 
        PartArmor,
        PartEngine,
        PartWeapon,
        PartReactor,
        PartRadar
    }

    public class TileObject
    {
        public TileObject()
        {

        }
        public static int GetBioMass(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.Tree) return 3;
            if (tileObjectType == TileObjectType.Bush) return 1;

            return 0;
        }

        public TileObject(TileObjectType tileObjectType, Direction direction)
        {
            TileObjectType = tileObjectType;
            Direction = direction;
        }
        public static bool IsTileObjectTypeCollectable(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.Mineral) return true;
            if (tileObjectType == TileObjectType.Tree) return true;
            if (tileObjectType == TileObjectType.Bush) return true;

            return false;
        }

        public static bool IsTileObjectTypeGrow(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.Tree) return true;
            if (tileObjectType == TileObjectType.TreeTrunk) return true;
            if (tileObjectType == TileObjectType.Bush) return true;
            if (tileObjectType == TileObjectType.Dirt) return true;
            if (tileObjectType == TileObjectType.Water) return true;
            if (tileObjectType == TileObjectType.Sand) return true;

            return false;
        }

        public static int GetPowerForTileObjectType(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.Mineral) return 100;
            if (tileObjectType == TileObjectType.Tree) return 40;
            if (tileObjectType == TileObjectType.Bush) return 20;

            return 10;
        }

        public static bool CanConvertTileObjectIntoMineral(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.PartArmor) return true;
            if (tileObjectType == TileObjectType.PartAssembler) return true;
            if (tileObjectType == TileObjectType.PartContainer) return true;
            if (tileObjectType == TileObjectType.PartEngine) return true;
            if (tileObjectType == TileObjectType.PartExtractor) return true;
            if (tileObjectType == TileObjectType.PartRadar) return true;
            if (tileObjectType == TileObjectType.PartReactor) return true;
            if (tileObjectType == TileObjectType.PartWeapon) return true;

            return false;
        }

        public static TileObjectType GetTileObjectTypeFromString(string unitCode, out int unitCodeLevel)
        {
            unitCodeLevel = 1;
            if (unitCode.EndsWith("2"))
                unitCodeLevel = 2;
            if (unitCode.EndsWith("3"))
                unitCodeLevel = 3;

            if (unitCode.StartsWith("PartExtractor")) return TileObjectType.PartExtractor;
            if (unitCode.StartsWith("PartAssembler")) return TileObjectType.PartAssembler;
            if (unitCode.StartsWith("PartContainer")) return TileObjectType.PartContainer;
            if (unitCode.StartsWith("PartArmor")) return TileObjectType.PartArmor;

            if (unitCode.StartsWith("PartEngine")) return TileObjectType.PartEngine;
            if (unitCode.StartsWith("PartWeapon")) return TileObjectType.PartWeapon;
            if (unitCode.StartsWith("PartReactor")) return TileObjectType.PartReactor;
            if (unitCode.StartsWith("PartRadar")) return TileObjectType.PartRadar;

            return TileObjectType.None;
        }

        public TileObjectKind TileObjectKind { get; set; }
        public TileObjectType TileObjectType { get; set; }

        public Direction Direction { get; set; }

        public override string ToString()
        {
            return TileObjectType.ToString() + " " + Direction.ToString();
        }
    }
}
