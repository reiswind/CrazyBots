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
        public GameObject Part1 { get; set; }
        public GameObject Part2 { get; set; }
        public GameObject Part3 { get; set; }

        public void Fire(Move move, Weapon1 weapon)
        {
            if (weapon != null)
                weapon.Fire(UnitBase.HexGrid, this.UnitBase, move, TileObjectContainer);
            
        }

        public void UpdateContent(List<TileObject> tileObjects, int? capacity)
        {
            if (UnitBase.gameObject == null)
            {

            }
            else
            {
                try
                {
                    if (Part1.gameObject != null)
                        TileObjectContainer.UpdateContent(UnitBase, Part1, Part2, Part3, tileObjects, capacity);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
