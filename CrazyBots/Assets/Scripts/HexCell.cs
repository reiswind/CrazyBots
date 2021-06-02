using Engine.Interface;
using System.Collections.Generic;
using UnityEngine;

public class HexCell
{
    public Tile Tile { get; set; }

    public HexGrid HexGrid { get; set; }
    public Move NextMove { get; set; }
    public bool ShowPheromones { get; set; }
    internal GameObject Cell { get; set; }
    private List<GameObject> minerals;
    private List<GameObject> destructables;
    private List<GameObject> obstacles;

    private static GameObject markerPrefab;
    private GameObject markerEnergy;
    private GameObject markerToHome;
    private GameObject markerToMineral;
    private GameObject markerToEnemy;

    public HexCell()
    {
        minerals = new List<GameObject>();
        destructables = new List<GameObject>();
        obstacles = new List<GameObject>();
    }

    internal void Update(MapPheromone mapPheromone)
    {
        if (!ShowPheromones)
            return;
        if (mapPheromone == null)
        {
            if (markerEnergy != null)
            {
                markerEnergy.transform.position = Cell.transform.position;
            }
            if (markerToHome != null)
            { 
                markerToHome.transform.position = Cell.transform.position;
            }
            if (markerToMineral != null)
            {
                markerToMineral.transform.position = Cell.transform.position;
            }
            if (markerToEnemy != null)
            {
                markerToEnemy.transform.position = Cell.transform.position;
            }
        }
        else
        {
            if (markerEnergy == null)
            {
                if (markerPrefab == null)
                    markerPrefab = Resources.Load<GameObject>("Prefabs/Terrain/Marker");
                markerEnergy = HexGrid.Instantiate(markerPrefab, Cell.transform, false);
                markerEnergy.name = Cell.name + "-Energy";

                markerToHome = HexGrid.Instantiate(markerPrefab, Cell.transform, false);
                markerToHome.name = Cell.name + "-Home";
                MeshRenderer meshRenderer = markerToHome.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0, 0.6f);

                markerToMineral = HexGrid.Instantiate(markerPrefab, Cell.transform, false);
                markerToMineral.name = Cell.name + "-Mineral";
                meshRenderer = markerToMineral.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0, 0.4f, 0);

                markerToEnemy = HexGrid.Instantiate(markerPrefab, Cell.transform, false);
                markerToEnemy.name = Cell.name + "-Mineral";
                meshRenderer = markerToEnemy.GetComponent<MeshRenderer>();
                meshRenderer.material.color = new Color(0.4f, 0, 0);
            }

            if (mapPheromone.IntensityToHome > 0)
            {
                Vector3 position = Cell.transform.position;
                position.y += 0.054f + (0.2f * mapPheromone.IntensityToHome);
                position.x += 0.1f;
                markerToHome.transform.position = position;
            }
            else
            {
                Vector3 position = Cell.transform.position;
                position.y -= 1;
                position.x += 0.1f;
                markerToHome.transform.position = position;
            }

            if (mapPheromone.IntensityToMineral > 0)
            {
                Vector3 position = Cell.transform.position;
                position.y += 0.054f + (0.2f * mapPheromone.IntensityToMineral);
                position.x += 0.2f;
                markerToMineral.transform.position = position;
            }
            else
            {
                Vector3 position = Cell.transform.position;
                position.y -= 1;
                position.x += 0.2f;
                markerToMineral.transform.position = position;
            }

            if (mapPheromone.IntensityToEnemy > 0)
            {
                Vector3 position = Cell.transform.position;
                position.y += 0.054f + (0.2f * mapPheromone.IntensityToEnemy);
                position.x += 0.3f;
                markerToEnemy.transform.position = position;
            }
            else
            {
                Vector3 position = Cell.transform.position;
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
                Vector3 position = Cell.transform.position;
                position.y += 0.054f + (0.2f * highestEnergy);
                markerEnergy.transform.position = position;
                UnitFrame.SetPlayerColor(highestPlayerId, markerEnergy);                
            }
            else
            {
                Vector3 position = Cell.transform.position;
                position.y -= 1;
                markerEnergy.transform.position = position;
            }
        }
    }

    internal void UpdateGround()
    {
        if (NextMove != null)
        {
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

            Vector3 unitPos3 = Cell.transform.position;
            unitPos3.x += (randomPos.x * 0.7f);
            unitPos3.z += (randomPos.y * 0.8f);

            GameObject destructable;
            if (Tile.IsDarkSand() || Tile.IsSand())
            {
                int treeIdx = HexGrid.game.Random.Next(HexGrid.smallRocks.Count);
                destructable = HexGrid.Instantiate(HexGrid.smallRocks[treeIdx], Cell.transform, false);
            }
            else
            {
                int treeIdx = HexGrid.game.Random.Next(HexGrid.smallTrees.Count);
                destructable = HexGrid.Instantiate(HexGrid.smallTrees[treeIdx], Cell.transform, false);
            }

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
            Vector3 unitPos3 = Cell.transform.position;

            GameObject obstacle;
             
            int treeIdx = HexGrid.game.Random.Next(HexGrid.obstacles.Count);
            obstacle = HexGrid.Instantiate(HexGrid.obstacles[treeIdx], Cell.transform, false);

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


    internal void CreateMinerals()
    {
        while (minerals.Count < Tile.Metal)
        {
            Vector2 randomPos = Random.insideUnitCircle;

            Vector3 unitPos3 = Cell.transform.position;
            unitPos3.x += (randomPos.x * 0.7f);
            unitPos3.z += (randomPos.y * 0.8f);
            unitPos3.y += 0.13f; // 

            GameObject crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
            GameObject crystal = HexGrid.Instantiate(crystalResource, Cell.transform, false);
            
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