using Engine.Interface;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    public Tile Tile { get; set; }

    //public HexGrid HexGrid { get; set; }
    public Move NextMove { get; set; }

    private List<GameObject> crystals;

    void Start()
    {
        if (Tile.Metal > 0)
        {
            if (crystals == null)
            {
                crystals = new List<GameObject>();

                int metalRemaining = Tile.Metal;
                while (metalRemaining > 0)
                {
                    Vector2 randomPos = Random.insideUnitCircle;

                    Vector3 unitPos3 = transform.position;
                    unitPos3.x += (randomPos.x * 0.7f);
                    unitPos3.z += (randomPos.y * 0.8f);
                    unitPos3.y += 0.23f; // 

                    GameObject crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
                    GameObject crystal = Instantiate(crystalResource, transform, false);

                    //crystal.transform.SetParent(transform, false);
                    crystal.transform.position = unitPos3;

                    crystal.transform.rotation = Random.rotation;

                    metalRemaining--;
                    crystals.Add(crystal);
                }
            }
        }
    }

    void Update()
    {
        if (NextMove != null)
        {
            if (crystals != null)
            {
                while (crystals.Count > Tile.Metal)
                {
                    GameObject crystal = crystals[0];

                    Destroy(crystal);

                    crystals.Remove(crystal);
                }

            }        
            NextMove = null;
        }

    }
}