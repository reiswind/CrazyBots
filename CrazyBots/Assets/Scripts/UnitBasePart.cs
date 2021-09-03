using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitBasePart
    {
        public TileObjectContainer TileObjectContainer { get; set; }
        public UnitBasePart(UnitBase unitBase)
        {
            this.UnitBase = unitBase;
        }

        public UnitBase UnitBase { get; private set; }

        public string Name { get; set; }
        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public int CompleteLevel { get; set; }
        public bool IsUnderConstruction { get; set; }
        public bool Destroyed { get; set; }
        public GameObject Part { get; set; }
        //public List<UnitBaseTileObject> TileObjects { get; set; }

        public void Fire(Move move, Weapon1 weapon)
        {
            if (weapon != null)
                weapon.Fire(UnitBase.HexGrid, this.UnitBase, move, TileObjectContainer);
            
        }

        public void UpdateContent(List<TileObject> tileObjects, int? capacity)
        {
            TileObjectContainer.UpdateContent(UnitBase, Part, tileObjects, capacity, UnitBase.extractedBaseTileObjects);
        }
    }
}
