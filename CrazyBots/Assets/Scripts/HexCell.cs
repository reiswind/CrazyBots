using Engine.Interface;
using System.Collections.Generic;
using UnityEngine;

public class HexCell
{
    public Tile Tile { get; set; }

    public HexGrid HexGrid { get; set; }
    public Move NextMove { get; set; }
    internal GameObject Cell { get; set; }
    private List<GameObject> minerals;
    private List<GameObject> smallTrees;

    internal int NumberOfSmallTrees;

    private static GameObject markerPrefab;
    private GameObject markerEnergy;
    private GameObject markerToHome;

    public HexCell()
    {
        minerals = new List<GameObject>();
        smallTrees = new List<GameObject>();
    }

    internal void Update(MapPheromone mapPheromone)
    {
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
                meshRenderer.material.color = new Color(0, 0, 0.4f);
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
            CreateMinerals();
            NextMove = null;
        }
    }

    internal void CreateTrees(List<GameObject> smmallTrees)
    {
        while (smallTrees.Count < NumberOfSmallTrees)
        {
            Vector2 randomPos = Random.insideUnitCircle;

            Vector3 unitPos3 = Cell.transform.position;
            unitPos3.x += (randomPos.x * 0.7f);
            unitPos3.z += (randomPos.y * 0.8f);
            unitPos3.y += 0.23f; // 

            int treeIdx =  HexGrid.game.Random.Next(smmallTrees.Count);

            GameObject smallTree = HexGrid.Instantiate(smmallTrees[treeIdx], Cell.transform, false);
            smallTree.transform.position = unitPos3;

            //smallTree.transform.rotation = Random.rotation;

            smallTrees.Add(smallTree);
        }
        while (smallTrees.Count > NumberOfSmallTrees)
        {
            GameObject smallTree = minerals[0];

            HexGrid.Destroy(smallTree);

            smallTrees.Remove(smallTree);
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
            unitPos3.y += 0.23f; // 

            GameObject crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
            GameObject crystal = HexGrid.Instantiate(crystalResource, Cell.transform, false);
            crystal.transform.position = unitPos3;

            crystal.transform.rotation = Random.rotation;

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