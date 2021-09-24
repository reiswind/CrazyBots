using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            emptyCubes = new List<GameObject>();
            tileObjects = new List<UnitBaseTileObject>();
        }

        private List<GameObject> mineralCubes;
        private List<GameObject> emptyCubes;
        private List<UnitBaseTileObject> tileObjects;
        private int filled;
        private int max;

        public ReadOnlyCollection<UnitBaseTileObject> TileObjects
        {
            get
            {
                return tileObjects.AsReadOnly();
            }
        }
        
        public void Remove(UnitBaseTileObject unitBaseTileObject)
        {
            if (unitBaseTileObject.Placeholder != null)
            {
                emptyCubes.Add(unitBaseTileObject.Placeholder);
                unitBaseTileObject.Placeholder = null;
            }
            tileObjects.Remove(unitBaseTileObject);
        }

        private bool AddPlaceholders(GameObject container)
        {
            if (container.name.StartsWith("Mineral") || container.name.StartsWith("Item"))
            {
                mineralCubes.Add(container);
                container.SetActive(false);
            }
            else
            {
                for (int i = 0; i < container.transform.childCount; i++)
                {
                    GameObject child = container.transform.GetChild(i).gameObject;
                    AddPlaceholders(child);
                }
            }
            emptyCubes.Clear();
            emptyCubes.AddRange(mineralCubes);
            return true;
        }

        public void UpdateContent(UnitBase unitBase, GameObject gameObject, List<TileObject> otherTileObjects, int? capacity)
        {
            if (capacity.HasValue && capacity <= 0)
                return;

            if (mineralCubes.Count == 0)
            {
                AddPlaceholders(gameObject);

                filled = 0;
                max = mineralCubes.Count;
            }

            bool oneItemPerCube = capacity == mineralCubes.Count;
            if (oneItemPerCube && capacity == 24)
            {
                int x=0;
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

                if (oneItemPerCube)
                {
                    if (emptyCubes.Count == 0)
                    {
                        int x = 0;
                    }
                    newUnitBaseTileObject.Placeholder = emptyCubes[0];
                    emptyCubes.Remove(newUnitBaseTileObject.Placeholder);
                    

                    newUnitBaseTileObject.GameObject = unitBase.HexGrid.CreateTileObject(gameObject.transform, tileObject);
                    newUnitBaseTileObject.GameObject.transform.position = newUnitBaseTileObject.Placeholder.transform.position;
                    
                }
            }
            // To many 
            foreach (UnitBaseTileObject unitBaseTileObject in assignedGameTileObjects)
            {
                if (unitBaseTileObject.GameObject != null)
                {
                    HexGrid.Destroy(unitBaseTileObject.GameObject);
                }
                tileObjects.Remove(unitBaseTileObject);

                if (oneItemPerCube)
                {
                    if (unitBaseTileObject.Placeholder != null)
                    {
                        emptyCubes.Add(unitBaseTileObject.Placeholder);
                        unitBaseTileObject.Placeholder.SetActive(false);
                    }
                }
            }
            
            if (oneItemPerCube)
            {
                // Attach
                /*
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
                }*/
            }
            else
            {
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
}
