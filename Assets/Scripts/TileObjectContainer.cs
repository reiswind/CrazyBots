﻿using Engine.Interface;
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

        public void ExplodeExceedingCapacity(Transform parent, int capacity)
        {
            if (oneItemPerCube)
            {
                while (tileObjects.Count > capacity)
                {
                    UnitBaseTileObject unitBaseTileObject = tileObjects[tileObjects.Count - 1];
                    tileObjects.Remove(unitBaseTileObject);

                    try
                    {
                        if (unitBaseTileObject.GameObject != null)
                        {
                            unitBaseTileObject.GameObject.transform.SetParent(parent);

                            Vector3 vector3 = new Vector3();
                            vector3.y = 4f;
                            vector3.x = UnityEngine.Random.value;
                            vector3.z = UnityEngine.Random.value;

                            Rigidbody otherRigid = unitBaseTileObject.GameObject.GetComponent<Rigidbody>();
                            if (otherRigid != null)
                            {
                                otherRigid.isKinematic = false;
                                otherRigid.velocity = vector3;
                                otherRigid.rotation = UnityEngine.Random.rotation;
                            }
                            HexGrid.Destroy(unitBaseTileObject.GameObject, 5);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public void Explode(Transform parent)
        {
            ExplodeExceedingCapacity(parent, 0);
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

        private bool oneItemPerCube;

        public void UpdateContent(UnitBase unitBase, GameObject gameObject, List<TileObject> otherTileObjects, int? capacity)
        {
            if (capacity.HasValue && capacity <= 0)
                return;

            if (mineralCubes.Count == 0)
            {
                AddPlaceholders(gameObject);

                filled = 0;
                max = mineralCubes.Count;
                oneItemPerCube = capacity == mineralCubes.Count;
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
                newUnitBaseTileObject.TileObject = tileObject.Copy();
                tileObjects.Add(newUnitBaseTileObject);

                if (oneItemPerCube)
                {
                    newUnitBaseTileObject.Placeholder = emptyCubes[0];
                    emptyCubes.Remove(newUnitBaseTileObject.Placeholder);
                    
                    newUnitBaseTileObject.GameObject = unitBase.HexGrid.CreateTileObject(gameObject.transform, newUnitBaseTileObject.TileObject);
                    newUnitBaseTileObject.GameObject.transform.position = newUnitBaseTileObject.Placeholder.transform.position;
                }
            }
            // To many 
            foreach (UnitBaseTileObject unitBaseTileObject in assignedGameTileObjects)
            {
                if (unitBaseTileObject.GameObject != null)
                {
                    try
                    {
                        HexGrid.Destroy(unitBaseTileObject.GameObject);
                        unitBaseTileObject.GameObject = null;
                    }
                    catch(Exception)
                    {
                        int x = 0;
                    }

                }
                tileObjects.Remove(unitBaseTileObject);

                if (oneItemPerCube)
                {
                    if (unitBaseTileObject.Placeholder != null)
                    {
                        emptyCubes.Add(unitBaseTileObject.Placeholder);
                        try
                        {
                            unitBaseTileObject.Placeholder.SetActive(false);
                        }
                        catch (Exception)
                        {
                            int x = 0;
                        }
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
                        {
                            try 
                            { 
                                mineralCubes[filled].SetActive(false);
                            }
                            catch (Exception)
                            {
                                int x = 0;
                            }
                        }
                    }
                    while (filled < mins)
                    {
                        if (filled < mineralCubes.Count)
                        {
                            try 
                            {
                                mineralCubes[filled].SetActive(true);
                            }
                            catch (Exception)
                            {
                                int x = 0;
                            }
                        }
                        filled++;
                    }
                }
            }
        }
    }
}
