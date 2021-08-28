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
            tileObjects.AddRange(ptileObjects);
        }
        public void Add(TileObject tileObject)
        {
            tileObjects.Add(tileObject);
        }
        public void Remove(TileObject tileObject)
        {
            tileObjects.Remove(tileObject);
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

        public void CreateMinerals(int capacity)
        {
            while (capacity-- > 0)
            {
                TileObject tileObject = new TileObject();
                tileObject.Direction = Direction.N;
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
    public enum TileObjectType
    {
        None,
        All,
        Dirt,
        Gras,
        Bush,
        Tree,
        Mineral,

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

        public TileObjectType TileObjectType { get; set; }

        public Direction Direction { get; set; }

        public override string ToString()
        {
            return TileObjectType.ToString() + " " + Direction.ToString();
        }
    }
}
