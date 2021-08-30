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
        private MineralContainer mineralContainer = new MineralContainer();
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
        public List<UnitBaseTileObject> TileObjects { get; set; }

        public void ClearContainer()
        {
            mineralContainer.ClearContent();
        }

        public void UpdateContent(List<TileObject> tileObjects, int? capacity)
        {
            mineralContainer.UpdateContent(UnitBase.HexGrid, Part, tileObjects, capacity);
        }
    }
}
