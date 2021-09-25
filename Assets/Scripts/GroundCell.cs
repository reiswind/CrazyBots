using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GroundCell : MonoBehaviour
    {
        public Position Pos { get; set; }

        public HexGrid HexGrid { get; set; }

        public MoveUpdateStats Stats { get; set; }

        public bool ShowPheromones { get; set; }

        internal List<UnitCommand> UnitCommands { get; private set; }

        public List<UnitBaseTileObject> GameObjects { get; private set; }

        private GameObject markerEnergy;
        private GameObject markerToHome;
        private GameObject markerToMineral;
        private GameObject markerToEnemy;

        public GroundCell()
        {
            GameObjects = new List<UnitBaseTileObject>();

            UnitCommands = new List<UnitCommand>();
            ShowPheromones = true;
        }

        private void CreateMarker()
        {
            if (markerEnergy == null)
            {
                GameObject markerPrefab = HexGrid.GetTerrainResource("Marker");
                markerEnergy = Instantiate(markerPrefab, transform, false);
                markerEnergy.name = name + "-Energy";

                markerToHome = Instantiate(markerPrefab, transform, false);
                markerToHome.name = name + "-Home";
                MeshRenderer meshRenderer = markerToHome.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0, 0.6f);

                markerToMineral = Instantiate(markerPrefab, transform, false);
                markerToMineral.name = name + "-Mineral";
                meshRenderer = markerToMineral.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0.4f, 0);

                markerToEnemy = Instantiate(markerPrefab, transform, false);
                markerToEnemy.name = name + "-Mineral";
                meshRenderer = markerToEnemy.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0.4f, 0, 0);
            }
        }

        internal void UpdatePheromones(MapPheromone mapPheromone)
        {
            if (!ShowPheromones)
                return;
            if (mapPheromone == null)
            {
                if (markerEnergy != null)
                {
                    markerEnergy.transform.position = transform.position;
                }
                if (markerToHome != null)
                {
                    markerToHome.transform.position = transform.position;
                }
                if (markerToMineral != null)
                {
                    markerToMineral.transform.position = transform.position;
                }
                if (markerToEnemy != null)
                {
                    markerToEnemy.transform.position = transform.position;
                }
            }
            else
            {
                if (markerEnergy == null)
                {
                    CreateMarker();
                }

                /*
                if (mapPheromone.IntensityToWork > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityToWork);
                    position.x += 0.1f;
                    markerToHome.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.1f;
                    markerToHome.transform.position = position;
                }*/

                /*
                if (mapPheromone.IntensityToMineral > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityToMineral);
                    position.x += 0.2f;
                    markerToMineral.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.2f;
                    markerToMineral.transform.position = position;
                }*/

                /*
                if (mapPheromone.IntensityToEnemy > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * mapPheromone.IntensityToEnemy);
                    position.x += 0.3f;
                    markerToEnemy.transform.position = position;
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    position.x += 0.3f;
                    markerToEnemy.transform.position = position;
                }*/

                /*
                float highestEnergy = -1;
                int highestPlayerId = 0;

                foreach (MapPheromoneItem mapPheromoneItem in mapPheromone.PheromoneItems)
                {
                    if (mapPheromoneItem.PheromoneType == Engine.Ants.PheromoneType.Energy)
                    {
                        if (mapPheromoneItem.Intensity >= highestEnergy)
                        {
                            highestEnergy = mapPheromoneItem.Intensity;
                            highestPlayerId = mapPheromoneItem.PlayerId;
                        }
                    }
                }

                if (highestEnergy > 0)
                {
                    Vector3 position = transform.position;
                    position.y += 0.054f + (0.2f * highestEnergy);
                    markerToEnemy.transform.position = position;
                    //UnitBase.SetPlayerColor(HexGrid, highestPlayerId, markerToEnemy);
                }
                else
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    markerToEnemy.transform.position = position;
                }
                */
            }
        }

        private string currentMaterialName;

        internal void SetGroundMaterial()
        {
            /*
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Ammo"))
                    SetPlayerColor(hexGrid, playerId, child);
            }*/

            string materialName;
            if (Stats.MoveUpdateGroundStat.IsUnderwater)
            {
                materialName = "Water";
            }
            else
            {
                materialName = "Rock";

                /*
                if (tileObject.TileObjectType == TileObjectType.Gras)
                {
                    materialName = "Grass";
                }
                else if (tileObject.TileObjectType == TileObjectType.Bush)
                {
                    materialName = "GrassDark";
                }
                else if (tileObject.TileObjectType == TileObjectType.Tree)
                {
                    materialName = "Wood";
                }
                else
                {
                    int x = 0;
                }
                */
                
                if (Stats.MoveUpdateGroundStat.IsHill())
                {
                    materialName = "Hill";
                }
                else if (Stats.MoveUpdateGroundStat.IsRock())
                {
                    materialName = "Rock";
                }
                else if (Stats.MoveUpdateGroundStat.IsSand())
                {
                    materialName = "Sand";
                }
                else if (Stats.MoveUpdateGroundStat.IsDarkSand())
                {
                    materialName = "DarkSand";
                }
                else if (Stats.MoveUpdateGroundStat.IsDarkWood())
                {
                    materialName = "DarkWood";
                }
                else if (Stats.MoveUpdateGroundStat.IsWood())
                {
                    materialName = "Wood";
                }
                else if (Stats.MoveUpdateGroundStat.IsLightWood())
                {
                    materialName = "LightWood";
                }
                else if (Stats.MoveUpdateGroundStat.IsGrassDark())
                {
                    materialName = "GrassDark";
                }
                else if (Stats.MoveUpdateGroundStat.IsGras())
                {
                    materialName = "Grass";
                }
                else
                {
                    materialName = "";
                }

                if (Stats.MoveUpdateGroundStat.IsOpenTile)
                {
                    materialName = "DarkSand";
                }
            }

            if (currentMaterialName == null || currentMaterialName != materialName)
            {
                currentMaterialName = materialName;

                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    Material[] newMaterials = new Material[meshRenderer.materials.Length];
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                    {
                        Material material = meshRenderer.materials[i];
                        if (!material.name.StartsWith("Normal"))
                        {
                            Destroy(material);
                            newMaterials[i] = HexGrid.GetMaterial(materialName);
                        }
                        else
                        {
                            newMaterials[i] = meshRenderer.materials[i];
                            //Destroy(material);
                            //newMaterials[i] = HexGrid.GetMaterial("GroundMat");
                        }
                    }
                    meshRenderer.materials = newMaterials;
                }
            }
        }

        internal void UpdateGround()
        {
            if (Stats.MoveUpdateGroundStat.Owner == 0 || !Stats.MoveUpdateGroundStat.IsBorder || Stats.MoveUpdateGroundStat.IsUnderwater)
            {
                if (markerEnergy != null)
                {
                    Vector3 position = transform.position;
                    position.y -= 1;
                    markerEnergy.transform.position = position;
                }
            }
            else
            {
                if (markerEnergy == null)
                {
                    CreateMarker();
                }
                float highestEnergy = 1;

                Vector3 position = transform.position;
                position.y += 0.054f + (0.2f * highestEnergy);
                markerEnergy.transform.position = position;
                UnitBase.SetPlayerColor(HexGrid, Stats.MoveUpdateGroundStat.Owner, markerEnergy);
            }

            Vector3 vector3 = transform.localPosition;
            vector3.y = Stats.MoveUpdateGroundStat.Height + 0.3f;
            transform.localPosition = vector3;

            CreateDestructables();
        }

        internal void CreateDestructables()
        {
            SetGroundMaterial();

            List<UnitBaseTileObject> allTileObjects = new List<UnitBaseTileObject>();
            allTileObjects.AddRange(GameObjects);

            foreach (TileObject tileObject in Stats.MoveUpdateGroundStat.TileObjects)
            {
                if ((tileObject.Direction == Direction.C && tileObject.TileObjectType != TileObjectType.Mineral) ||
                    tileObject.TileObjectType == TileObjectType.Dirt)
                {

                }
                else
                {
                    bool found = false;
                    foreach (UnitBaseTileObject destructable in allTileObjects)
                    {
                        if (destructable.TileObject.TileObjectType == tileObject.TileObjectType)
                        {
                            found = true;
                            allTileObjects.Remove(destructable);
                            break;
                        }
                    }
                    if (!found)
                    {

                        GameObject destructable;

                        destructable = HexGrid.CreateDestructable(transform, tileObject);
                        destructable.transform.Rotate(Vector3.up, Random.Range(0, 360));
                        destructable.name = tileObject.TileObjectType.ToString();

                        UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
                        unitBaseTileObject.GameObject = destructable;
                        unitBaseTileObject.TileObject = tileObject;

                        GameObjects.Add(unitBaseTileObject);

                    }
                }
            }

            foreach (UnitBaseTileObject destructable in allTileObjects)
            {
                HexGrid.Destroy(destructable.GameObject);
                GameObjects.Remove(destructable);
            }
        }

        private Light selectionLight;

        public bool IsSelected { get; private set; }
        internal void SetSelected(bool selected)
        {
            if (IsSelected != selected)
            {
                IsSelected = selected;

                if (IsSelected)
                {
                    selectionLight = HexGrid.CreateSelectionLight(gameObject);
                }
                else
                {
                    Destroy(selectionLight);
                }
            }
        }

        public bool IsAttack { get; private set; }
        internal void SetAttack(bool selected)
        {
            if (IsAttack != selected)
            {
                IsAttack = selected;

                transform.Find("Attack").gameObject.SetActive(IsAttack);
            }
        }
    }
}