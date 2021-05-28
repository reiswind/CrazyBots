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
    internal MeshRenderer meshRenderer;
    internal Material pheromaterial;

    public HexCell()
    {
        minerals = new List<GameObject>();
        smallTrees = new List<GameObject>();
    }

    internal void Update(MapPheromone mapPheromone)
    {
        
        //pheromaterial.color = Random.ColorHSV();
        /*
        for (int i = 0; i < meshRenderer.materials.Length; i++)
        {
            Material material = meshRenderer.materials[i];
            if (material.name.StartsWith("Player"))
            {
                material.color = Color.red;

            }
            else
            {
                //newMaterials[i] = material;
            }
        }
        */
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