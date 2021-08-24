using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Interface
{
    public class TileContainer
    {
        public TileContainer()
        {
            TileObjects = new List<TileObject>();
        }

        public List<TileObject> TileObjects { get; set; }

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

        public TileObject RemoveTileObject(TileObjectType tileObjectType)
        {
            foreach (TileObject tileObject in TileObjects)
            {
                if (tileObject.TileObjectType == tileObjectType || tileObjectType == TileObjectType.All)
                {
                    TileObjects.Remove(tileObject);
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
                TileObjects.Add(tileObject);
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
        Mineral
    }

    public class TileObject
    {
        public TileObject()
        {

        }

        public TileObjectType TileObjectType { get; set; }

        public Direction Direction { get; set; }

        public override string ToString()
        {
            return TileObjectType.ToString() + " " + Direction.ToString();
        }
    }
}
