using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCell : MonoBehaviour
{
    public Tile Tile { get; set; }

    public HexGrid HexGrid { get; set; }
    public Move NextMove { get; set; }
    public bool ShowPheromones { get; set; }

    internal List<UnitCommand> UnitCommands { get; private set; }

    private List<GameObject> minerals;
    private GameObject mineralObstacle;
    private List<GameObject> destructables;
    private List<GameObject> obstacles;

    private static GameObject markerPrefab;
    private GameObject markerEnergy;
    private GameObject markerToHome;
    private GameObject markerToMineral;
    private GameObject markerToEnemy;

    public GroundCell()
    {
        minerals = new List<GameObject>();
        destructables = new List<GameObject>();
        obstacles = new List<GameObject>();

        UnitCommands = new List<UnitCommand>();
        ShowPheromones = true;
    }

    internal void UpdatePheromones (MapPheromone mapPheromone)
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
                if (markerPrefab == null)
                    markerPrefab = Resources.Load<GameObject>("Prefabs/Terrain/Marker");
                markerEnergy = HexGrid.Instantiate(markerPrefab, transform, false);
                markerEnergy.name = name + "-Energy";

                markerToHome = HexGrid.Instantiate(markerPrefab, transform, false);
                markerToHome.name = name + "-Home";
                MeshRenderer meshRenderer = markerToHome.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0, 0.6f);

                markerToMineral = HexGrid.Instantiate(markerPrefab, transform, false);
                markerToMineral.name = name + "-Mineral";
                meshRenderer = markerToMineral.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0.4f, 0);

                markerToEnemy = HexGrid.Instantiate(markerPrefab, transform, false);
                markerToEnemy.name = name + "-Mineral";
                meshRenderer = markerToEnemy.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0.4f, 0, 0);
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
            }
            */
            
            if (mapPheromone.IntensityToWork > 0)
            {
                Vector3 position = transform.position;
                position.y += 0.054f + (0.2f * mapPheromone.IntensityToWork);
                position.x += 0.3f;
                markerToEnemy.transform.position = position;
            }
            else
            {
                Vector3 position = transform.position;
                position.y -= 1;
                position.x += 0.3f;
                markerToEnemy.transform.position = position;
            }
            
            
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
            //highestEnergy = 0;
            if (highestEnergy > 0)
            {
                Vector3 position = transform.position;
                position.y += 0.054f + (0.2f * highestEnergy);
                markerEnergy.transform.position = position;
                UnitBase.SetPlayerColor(highestPlayerId, markerEnergy);
            }
            else
            {
                Vector3 position = transform.position;
                position.y -= 1;
                markerEnergy.transform.position = position;
            }
            
        }
    }

    internal void UpdateGround()
    {
        if (NextMove != null)
        {
            if (NextMove.Stats != null && NextMove.Stats.MoveUpdateGroundStat != null)
            {
                MoveUpdateGroundStat stat = NextMove.Stats.MoveUpdateGroundStat;
                if (Tile.Metal != stat.Minerals)
                {
                    // Currently not in sync. later.
                    int x =0;
                }
                Tile.Metal = stat.Minerals;

                Tile.NumberOfDestructables = stat.NumberOfDestructables;
                Tile.NumberOfObstacles = stat.NumberOfObstacles;
                
            }
            CreateObstacles();
            CreateDestructables();
            CreateMinerals();
            NextMove = null;
        }
    }

    internal void CreateDestructables()
    {
        while (destructables.Count < Tile.NumberOfDestructables)
        {
            Vector2 randomPos = Random.insideUnitCircle;

            Vector3 unitPos3 = transform.position;
            unitPos3.x += (randomPos.x * 0.7f);
            unitPos3.z += (randomPos.y * 0.8f);

            GameObject destructable;
            if (Tile.IsDarkSand() || Tile.IsSand())
            {
                int treeIdx = HexGrid.game.Random.Next(HexGrid.smallRocks.Count);
                destructable = HexGrid.Instantiate(HexGrid.smallRocks[treeIdx], transform, false);
            }
            else
            {
                int treeIdx = HexGrid.game.Random.Next(HexGrid.smallTrees.Count);
                destructable = HexGrid.Instantiate(HexGrid.smallTrees[treeIdx], transform, false);
            }
            destructable.transform.Rotate(Vector3.up, Random.Range(0, 360));
            destructable.transform.position = unitPos3;

            destructables.Add(destructable);
        }
        while (destructables.Count > Tile.NumberOfDestructables)
        {
            GameObject destructable = destructables[0];
            HexGrid.Destroy(destructable);
            destructables.Remove(destructable);
        }
    }

    internal void CreateObstacles()
    {
        while (obstacles.Count < Tile.NumberOfObstacles)
        {
            Vector3 unitPos3 = transform.position;

            GameObject obstacle;

            int treeIdx = HexGrid.game.Random.Next(HexGrid.obstacles.Count);
            obstacle = HexGrid.Instantiate(HexGrid.obstacles[treeIdx], transform, false);

            obstacle.transform.Rotate(Vector3.up, Random.Range(0, 360));
            obstacle.transform.position = unitPos3;

            obstacles.Add(obstacle);
        }
        while (obstacles.Count > Tile.NumberOfObstacles)
        {
            GameObject obstacle = obstacles[0];
            HexGrid.Destroy(obstacle);
            obstacles.Remove(obstacle);
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

    internal void CreateMinerals()
    {
        if (Tile.Metal >= 20)
        {
            if (mineralObstacle == null)
            {
                Material crystalMaterial = Resources.Load<Material>("Materials/CrystalMat");

                int treeIdx = HexGrid.game.Random.Next(HexGrid.obstacles.Count);
                mineralObstacle = HexGrid.Instantiate(HexGrid.obstacles[treeIdx], transform, false);

                MeshRenderer meshRenderer = mineralObstacle.GetComponent<MeshRenderer>();
                meshRenderer.material = crystalMaterial;
                mineralObstacle.transform.Rotate(Vector3.up, Random.Range(0, 360));

                mineralObstacle.transform.position = transform.position;
            }

            while (minerals.Count > 0)
            {
                GameObject crystal = minerals[0];
                HexGrid.Destroy(crystal);
                minerals.Remove(crystal);
            }
        }
        else
        {
            if (Tile.Metal < 20 && mineralObstacle != null)
            {
                HexGrid.Destroy(mineralObstacle);
                mineralObstacle = null;
            }

            while (minerals.Count < Tile.Metal)
            {
                Vector2 randomPos = Random.insideUnitCircle;

                Vector3 unitPos3 = transform.position;
                unitPos3.x += (randomPos.x * 0.7f);
                unitPos3.z += (randomPos.y * 0.8f);
                unitPos3.y += 0.13f; // 

                GameObject crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
                GameObject crystal = HexGrid.Instantiate(crystalResource, transform, false);

                crystal.transform.SetPositionAndRotation(unitPos3, Random.rotation);

                minerals.Add(crystal);
            }
            while (minerals.Count > Tile.Metal)
            {
                GameObject crystal = minerals[0];

                HexGrid.Destroy(crystal);

                minerals.Remove(crystal);
            }
        }
    }
}
