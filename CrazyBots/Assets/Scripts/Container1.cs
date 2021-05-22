using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : MonoBehaviour
{
    public float AboveGround { get; set; }
    public UnitFrame UnitFrame { get; set; }

    private List<GameObject> crystals;

    private int filled;

    void Awake()
    {
        crystals = new List<GameObject>();
    }

    void Update()
    {
        UnitFrame?.Move(this);
        UpdateContent(UnitFrame?.MoveUpdateStats.ContainerFull);
    }

    public void UpdateContent(int? percentage)
    {
        if (percentage.HasValue)
        {
            if (percentage != filled)
            {
                int numCrystals = percentage.Value * 20 / 100;

                if (numCrystals > 20)
                    numCrystals = 20;

                while (crystals.Count > numCrystals)
                {
                    GameObject crystal = crystals[0];

                    Destroy(crystal);

                    crystals.Remove(crystal);
                    /*
                    GameObject crystal = crystals[crystals.Count-1];
                    crystals.Remove(crystal);
                    
                    Destroy(crystal);*/
                }
                while (crystals.Count < numCrystals)
                {
                    numCrystals++;

                    Vector2 randomPos = Random.insideUnitCircle;

                    Vector3 unitPos3 = transform.position;
                    unitPos3.x += (randomPos.x * 0.01f);
                    unitPos3.z += (randomPos.y * 0.01f);
                    unitPos3.y += 0.05f;

                    if (crystals.Count > 10)
                        unitPos3.y += 0.05f;

                    GameObject crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
                    GameObject crystal = Instantiate(crystalResource, transform, false);

                    crystal.transform.position = unitPos3;
                    crystal.transform.rotation = Random.rotation;
                    crystals.Add(crystal);

                    break;
                }

                filled = percentage.Value;
            }
        }
    }

}

