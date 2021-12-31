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
        /*
        private List<TileObjectType> reservedTileObjectTypes = new List<TileObjectType>();


        public void ClearReservations()
        {
            reservedTileObjectTypes.Clear();
        }

        public void ReserveIngredient(TileObjectType tileObjectType)
        {
            reservedTileObjectTypes.Add(tileObjectType);
        }
        public void ReleaseReservedIngredient(TileObjectType tileObjectType)
        {
            reservedTileObjectTypes.Remove(tileObjectType);
        }
        */
        public TileObject GetMatchingTileObject(TileObjectType tileObjectType, List<TileObject> excludeIngredients)
        {
            //List<TileObjectType> reserved = new List<TileObjectType>();
            //reserved.AddRange(reservedTileObjectTypes);

            foreach (TileObject tileObject in tileObjects)
            {
                if (excludeIngredients != null && excludeIngredients.Contains(tileObject))
                    continue;

                if (tileObjectType == TileObjectType.Ammo)
                {
                    if (TileObject.GetDeliveryScoreForAmmoType(tileObject.TileObjectType) > 0)
                    {
                        /*
                        if (reserved.Contains(tileObject.TileObjectType))
                        {
                            reserved.Remove(tileObject.TileObjectType);
                            continue;
                        }*/
                    }
                    return tileObject;
                }

                // Burn everything
                if (tileObjectType == TileObjectType.Burn)
                {
                    if (TileObject.GetPowerForTileObjectType(tileObject.TileObjectType) > 0)
                    {
                        /*
                        if (reserved.Contains(tileObject.TileObjectType))
                        {
                            reserved.Remove(tileObject.TileObjectType);
                            continue;
                        }*/
                    }
                    return tileObject;
                }

                if (tileObject.TileObjectType == tileObjectType || tileObjectType == TileObjectType.All)
                {
                    /*
                    if (reserved.Contains(tileObject.TileObjectType))
                    {
                        reserved.Remove(tileObject.TileObjectType);
                        continue;
                    }*/
                    return tileObject;
                }
            }
            return null;
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

        public bool Accepts(MoveRecipeIngredient realIndigrient)
        {
            return Accepts(realIndigrient.TileObjectType);
        }
        public bool Accepts(TileObject tileObject)
        {
            return Accepts(tileObject.TileObjectType);
        }

        public bool Accepts(TileObjectType tileObjectType)
        {
            if (AcceptedTileObjectTypes == TileObjectType.All)
                return true;

            if (AcceptedTileObjectTypes == TileObjectType.Burn &&
                TileObject.GetDeliveryScoreForBurnType(tileObjectType) > 0)
                return true;

            if (AcceptedTileObjectTypes == TileObjectType.Ammo &&
                TileObject.GetDeliveryScoreForAmmoType(tileObjectType) > 0)
                return true;

            if (AcceptedTileObjectTypes == tileObjectType)
                return true;

            return false;
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



    public enum TileObjectKind
    {
        None,
        LeaveTree,

        LightGras,
        DarkGras

    }

    public enum TileObjectType
    {
        // Groups
        None,
        All,
        Burn,
        Ammo,
        Unit,
        Ground, // Source

        // Collectable
        Mineral,
        Dirt,
        Stone,
        Wood,
        Coal,

        // Environment
        Gras,
        TreeTrunk,
        Water,
        Sand,
        Bush,
        Tree,

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
    public class TileCounter
    {
        public int Gras;
        public int Bush;
        public int Wood;
        public int Tree;
        public int Sand;
        public int Mineral;
        public int Stone;
        public int TreeTrunk;
        public int Water;
        public int None;
        public int NumberOfCollectables;

        public void Update(ReadOnlyCollection<TileObject> tileObjects)
        {
            Clear();
            Add(tileObjects);
        }

        public void Clear()
        {
            Gras = 0;
            Bush = 0;
            Wood = 0;
            Tree = 0;
            Sand = 0;
            Mineral = 0;
            Stone = 0;
            TreeTrunk = 0;
            Water = 0;
            NumberOfCollectables = 0;
            None = 7;
        }

        public void Add(TileCounter t)
        {
            Gras += t.Gras;
            Bush += t.Bush;
            Wood += t.Wood;
            Tree += t.Tree;
            Sand += t.Sand;
            Mineral += t.Mineral;
            Stone += t.Stone;
            TreeTrunk += t.TreeTrunk;
            Water += t.Water;
            NumberOfCollectables += t.NumberOfCollectables;
            None += t.None;
        }
        public void Add(ReadOnlyCollection<TileObject> tileObjects)
        {
            if (tileObjects == null)
                return;

            foreach (TileObject tileObject in tileObjects)
            {
                if (tileObject.TileObjectType == TileObjectType.Mineral)
                {
                    Mineral++;
                }
                else if (tileObject.TileObjectType == TileObjectType.Gras)
                {
                    Gras++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.TreeTrunk)
                {
                    TreeTrunk++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.Wood)
                {
                    Wood++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.Bush)
                {
                    Bush++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.Tree)
                {
                    Tree++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.Sand)
                {
                    Sand++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.Stone)
                {
                    Stone++;
                    None--;
                }
                else if (tileObject.TileObjectType == TileObjectType.Water)
                {
                    Water++;
                    None--;
                }
                if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                    NumberOfCollectables++;
                // Sure?
                int convertable;
                convertable = TileObject.GetWoodForObjectType(tileObject.TileObjectType);
                Wood += convertable;
                NumberOfCollectables += convertable;
            }
        }
    }

    public class TileObject
    {
        public TileObject()
        {
            Direction = Direction.C;
        }

        public TileObject Copy()
        {
            TileObject copy = new TileObject();
            copy.Direction = Direction;
            copy.TileObjectType = TileObjectType;
            copy.TileObjectKind = TileObjectKind;
            return copy;
        }

        public static int GetWoodForObjectType(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.Tree) return 1;
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
            if (tileObjectType == TileObjectType.Wood) return true;
            if (tileObjectType == TileObjectType.Stone) return true;

            return false;
        }
        public static bool IsAmmo(TileObjectType tileObjectType)
        {
            if (tileObjectType == TileObjectType.Mineral) return true;
            if (tileObjectType == TileObjectType.Wood) return true;
            if (tileObjectType == TileObjectType.Stone) return true;

            return false;
        }
        public static bool IsTileObjectTypeObstacle(TileObjectType tileObjectType)
        {

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
            if (tileObjectType == TileObjectType.Wood) return 150;
            if (tileObjectType == TileObjectType.Stone) return 15;

            return 0;
        }
        public static int GetDeliveryScoreForBurnType(TileObjectType presentType)
        {
            if (presentType == TileObjectType.Wood)
                return 10;
            if (presentType == TileObjectType.Mineral)
                return 1;
            return 0;
        }
        public static int GetDeliveryScoreForAmmoType(TileObjectType presentType)
        {
            if (presentType == TileObjectType.Wood)
                return 20;
            if (presentType == TileObjectType.Stone)
                return 30;
            if (presentType == TileObjectType.Mineral)
                return 10;
            return 0;
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

        public static bool CanMoveTo (TileCounter tileCounter)
        {
            bool canMove = true;

            if (tileCounter.Mineral >= Position2.BlockPathItemCount)
                canMove = false;
            if (tileCounter.Stone >= Position2.BlockPathItemCount)
                canMove = false;
            if (tileCounter.Tree > 0)
                canMove = false;
            if (tileCounter.Bush > 0)
                canMove = false;
            if (tileCounter.Water > 0)
                canMove = false;

            return canMove;
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
