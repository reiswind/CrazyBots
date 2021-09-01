using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class TileObjectContainer
    {

        public TileObjectContainer()
        {
            mineralCubes = new List<GameObject>();
            tileObjects = new List<UnitBaseTileObject>();
        }

        private List<GameObject> mineralCubes;
        private List<UnitBaseTileObject> tileObjects;
        private int filled;
        private int max;

        public GameObject RemoveTop()
        {
            GameObject top = mineralCubes[0];
            mineralCubes.Remove(top);
            return top;
        }

        public List<UnitBaseTileObject> TileObjects
        {
            get
            {
                return tileObjects;
            }
        }

        public void Add(UnitBaseTileObject unitBaseTileObject)
        {
            tileObjects.Add(unitBaseTileObject);
        }
        public void Remove(UnitBaseTileObject unitBaseTileObject)
        {
            tileObjects.Remove(unitBaseTileObject);
        }

        private bool AddMinerals(GameObject container)
        {
            if (container.name.StartsWith("Mineral"))
            {
                mineralCubes.Add(container);
                container.SetActive(false);
            }
            else
            {
                for (int i = 0; i < container.transform.childCount; i++)
                {
                    GameObject child = container.transform.GetChild(i).gameObject;
                    AddMinerals(child);
                }
            }
            return true;
        }
        public void ClearContent()
        {
            foreach (GameObject gameObject in mineralCubes)
            {
                gameObject.SetActive(false);
            }
        }

        public bool AttachGameTileObject(UnitBaseTileObject attachBaseTileObject)
        {
            foreach (UnitBaseTileObject unitBaseTileObject in tileObjects)
            {
                if (attachBaseTileObject.TileObject.TileObjectType == unitBaseTileObject.TileObject.TileObjectType &&
                    unitBaseTileObject.GameObject == null)
                {
                    unitBaseTileObject.GameObject = attachBaseTileObject.GameObject;
                    return true;
                }
            }
            return false;
        }

        public void UpdateContent(UnitBase unitBase, GameObject gameObject, List<TileObject> otherTileObjects, int? capacity, List<UnitBaseTileObject> extractedBaseTileObjects)
        {

            if (capacity.HasValue && capacity <= 0)
                return;

            if (mineralCubes.Count == 0)
            {
                AddMinerals(gameObject);

                filled = 0;
                max = mineralCubes.Count;
            }

            // Match content
            List<TileObject> unassignedTileObjects = new List<TileObject>();
            unassignedTileObjects.AddRange(otherTileObjects);

            List<UnitBaseTileObject> assignedGameTileObjects = new List<UnitBaseTileObject>();
            assignedGameTileObjects.AddRange(tileObjects);

            List<UnitBaseTileObject> unassignedGameTileObjects = new List<UnitBaseTileObject>();
            unassignedGameTileObjects.AddRange(tileObjects);

            foreach (TileObject otherTileObject in otherTileObjects)
            {
                foreach (UnitBaseTileObject unitBaseTileObject in unassignedGameTileObjects)
                {
                    if (otherTileObject.TileObjectType == unitBaseTileObject.TileObject.TileObjectType)
                    {
                        unassignedTileObjects.Remove(otherTileObject);
                        assignedGameTileObjects.Remove(unitBaseTileObject);
                        unassignedGameTileObjects.Remove(unitBaseTileObject);
                        break;
                    }
                }
            }
            // To less
            foreach (TileObject tileObject in unassignedTileObjects)
            {
                UnitBaseTileObject newUnitBaseTileObject = new UnitBaseTileObject();
                newUnitBaseTileObject.TileObject = tileObject;
                tileObjects.Add(newUnitBaseTileObject);
            }
            // To many 
            foreach (UnitBaseTileObject unitBaseTileObject in assignedGameTileObjects)
            {
                if (unitBaseTileObject.GameObject != null)
                {
                    HexGrid.Destroy(unitBaseTileObject.GameObject);
                }
                tileObjects.Remove(unitBaseTileObject);
            }
            // Attach
            List<UnitBaseTileObject> unattachedTileObjects = new List<UnitBaseTileObject>();
            unattachedTileObjects.AddRange(extractedBaseTileObjects);

            foreach (UnitBaseTileObject extractedUnitBaseTile in unattachedTileObjects)
            {
                foreach (UnitBaseTileObject tileObject in tileObjects)
                {
                    if (tileObject.GameObject == null && tileObject.TileObject.TileObjectType == extractedUnitBaseTile.TileObject.TileObjectType)
                    {
                        tileObject.GameObject = extractedUnitBaseTile.GameObject;
                        extractedBaseTileObjects.Remove(extractedUnitBaseTile);
                        break;
                    }
                }
            }

            int minerals = tileObjects.Count;
            int mins = minerals;

            int minPercent = mins * 100 / capacity.Value;
            mins = minPercent * max / 100;

            if (minerals > 0 && mins == 0)
                mins = 1;

            if (mins != filled)
            {
                while (filled > mins)
                {
                    filled--;
                    if (filled < mineralCubes.Count)
                        mineralCubes[filled].SetActive(false);
                }
                while (filled < mins)
                {
                    if (filled < mineralCubes.Count)
                        mineralCubes[filled].SetActive(true);
                    filled++;
                }
            }
        }
    }
}
