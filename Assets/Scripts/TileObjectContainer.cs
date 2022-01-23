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

        public ReadOnlyCollection<UnitBaseTileObject> TileObjects
        {
            get
            {
                return tileObjects.AsReadOnly();
            }
        }

        public void ExplodeExceedingCapacity(Transform parent, List<TileObject> otherTileObjects)
        {

            foreach (UnitBaseTileObject unitBaseTileObject in tileObjects)
            {

                if (unitBaseTileObject.GameObject != null)
                {
                    unitBaseTileObject.GameObject.transform.SetParent(parent);

                    Vector3 vector3 = parent.position;
                    vector3.y -= 0.4f;
                    //vector3.x = UnityEngine.Random.value;
                    //vector3.z = UnityEngine.Random.value;

                    Rigidbody otherRigid = unitBaseTileObject.GameObject.AddComponent<Rigidbody>();
                    if (otherRigid != null)
                    {
                        otherRigid.isKinematic = false;
                        otherRigid.AddExplosionForce(13, vector3, 1, 1);
                        //otherRigid.velocity = vector3;
                        //otherRigid.rotation = UnityEngine.Random.rotation;
                    }
                    HexGrid.Destroy(unitBaseTileObject.GameObject, 5);
                }
                if (unitBaseTileObject.Placeholder != null)
                    emptyCubes.Add(unitBaseTileObject.Placeholder);
            }
            tileObjects.Clear();
        }

        public void Remove(UnitBaseTileObject unitBaseTileObject)
        {
            if (unitBaseTileObject.Placeholder != null)
            {
                emptyCubes.Add(unitBaseTileObject.Placeholder);
                unitBaseTileObject.Placeholder = null;
            }
            if (unitBaseTileObject.GameObject != null)
            {
                HexGrid.Destroy(unitBaseTileObject.GameObject);
                unitBaseTileObject.GameObject = null;
            }
            tileObjects.Remove(unitBaseTileObject);
        }

        public static void HidePlaceholders(GameObject container)
        {
            if (container.name.StartsWith("Mineral") || container.name.StartsWith("Item"))
            {
                container.SetActive(false);
            }
            else
            {
                for (int i = 0; i < container.transform.childCount; i++)
                {
                    GameObject child = container.transform.GetChild(i).gameObject;
                    HidePlaceholders(child);
                }
            }
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
                    if (child.name.StartsWith("Mineral") || child.name.StartsWith("Item"))
                    {
                        AddPlaceholders(child);
                        child.SetActive(false);
                    }
                    else
                    {
                        if (child.activeSelf)
                            AddPlaceholders(child);
                    }
                }
            }

            return true;
        }


        public void UpdateContent(UnitBase unitBase, GameObject gameObject1, List<TileObject> otherTileObjects)
        {
            if (mineralCubes.Count == 0 && gameObject1 != null)
            {
                AddPlaceholders(gameObject1);

                emptyCubes.Clear();
                emptyCubes.AddRange(mineralCubes);
            }

            // Match content
            List<TileObject> unassignedTileObjects = new List<TileObject>();
            unassignedTileObjects.AddRange(otherTileObjects);

            List<UnitBaseTileObject> assignedGameTileObjects = new List<UnitBaseTileObject>();
            assignedGameTileObjects.AddRange(tileObjects);

            foreach (TileObject otherTileObject in unassignedTileObjects)
            {
                foreach (UnitBaseTileObject unitBaseTileObject in assignedGameTileObjects)
                {
                    if (otherTileObject.TileObjectType == unitBaseTileObject.TileObject.TileObjectType)
                    {
                        if (unitBaseTileObject.GameObject == null)
                        {

                        }
                        else
                        {
                            assignedGameTileObjects.Remove(unitBaseTileObject);
                            otherTileObjects.Remove(otherTileObject);
                        }
                        break;
                    }
                }
            }

            if (otherTileObjects.Count > 0)
            {
                unassignedTileObjects.Clear();
                unassignedTileObjects.AddRange(otherTileObjects);

                List<UnitBaseTileObject> unassignedGameTileObjects = new List<UnitBaseTileObject>();
                unassignedGameTileObjects.AddRange(tileObjects);

                // To less
                foreach (TileObject tileObject in unassignedTileObjects)
                {
                    if (emptyCubes.Count == 0)
                        break;

                    UnitBaseTileObject newUnitBaseTileObject = new UnitBaseTileObject();
                    newUnitBaseTileObject.TileObject = tileObject.Copy();
                    newUnitBaseTileObject.CollectionType = CollectionType.Single;
                    tileObjects.Add(newUnitBaseTileObject);
                    otherTileObjects.Remove(tileObject);

                    newUnitBaseTileObject.Placeholder = emptyCubes[0];
                    emptyCubes.Remove(newUnitBaseTileObject.Placeholder);

                    newUnitBaseTileObject.GameObject = HexGrid.MainGrid.CreateTileObject(gameObject1.transform, newUnitBaseTileObject.TileObject);
                    newUnitBaseTileObject.GameObject.transform.position = newUnitBaseTileObject.Placeholder.transform.position;
                }
            }
            // To many 
            foreach (UnitBaseTileObject unitBaseTileObject in assignedGameTileObjects)
            {
                if (unitBaseTileObject.GameObject != null)
                {
                    HexGrid.Destroy(unitBaseTileObject.GameObject);
                    unitBaseTileObject.GameObject = null;
                }
                tileObjects.Remove(unitBaseTileObject);

                if (unitBaseTileObject.Placeholder != null)
                {
                    emptyCubes.Add(unitBaseTileObject.Placeholder);
                    unitBaseTileObject.Placeholder.SetActive(false);
                }
                else
                {
                    int x = 0;
                }
            }
        }
    }
}
