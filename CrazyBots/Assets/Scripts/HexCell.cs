﻿using Engine.Interface;
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
    private List<GameObject> smallRocks;

    //internal int NumberOfSmallTrees;

    private static GameObject markerPrefab;
    private GameObject markerEnergy;
    private GameObject markerToHome;
    private GameObject markerToMineral;
    private GameObject markerToEnemy;

    public HexCell()
    {
        minerals = new List<GameObject>();
        smallTrees = new List<GameObject>();
        smallRocks = new List<GameObject>();
    }

    internal void Update(MapPheromone mapPheromone)
    {
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
            CreateTrees();
            CreateMinerals();
            NextMove = null;
        }
    }

    internal void CreateTrees()
    {
        while (smallTrees.Count < Tile.NumberOfSmallTrees)
        {
            Vector2 randomPos = Random.insideUnitCircle;

            Vector3 unitPos3 = Cell.transform.position;
            unitPos3.x += (randomPos.x * 0.7f);
            unitPos3.z += (randomPos.y * 0.8f);

            int treeIdx =  HexGrid.game.Random.Next(HexGrid.smallTrees.Count);

            GameObject smallTree = HexGrid.Instantiate(HexGrid.smallTrees[treeIdx], Cell.transform, false);
            smallTree.transform.position = unitPos3;

            smallTrees.Add(smallTree);
        }
        while (smallTrees.Count > Tile.NumberOfSmallTrees)
        {
            GameObject smallTree = smallTrees[0];

            HexGrid.Destroy(smallTree);

            smallTrees.Remove(smallTree);
        }

        while (smallRocks.Count < Tile.NumberOfRocks)
        {
            Vector2 randomPos = Random.insideUnitCircle;

            Vector3 unitPos3 = Cell.transform.position;
            unitPos3.x += (randomPos.x * 0.7f);
            unitPos3.z += (randomPos.y * 0.8f);
            unitPos3.y += 0.1f; // 

            int idx = HexGrid.game.Random.Next(HexGrid.smallRocks.Count);

            GameObject smallRock = HexGrid.Instantiate(HexGrid.smallRocks[idx], Cell.transform, false);
            smallRock.transform.position = unitPos3;

            smallRocks.Add(smallRock);
        }
        while (smallRocks.Count > Tile.NumberOfRocks)
        {
            GameObject smallTree = smallRocks[0];

            HexGrid.Destroy(smallTree);

            smallRocks.Remove(smallTree);
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