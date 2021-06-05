using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : MonoBehaviour
{
    internal int Level { get; set; }

    private List<GameObject> crystals;
    private static GameObject crystalResource;
    private int filled;

    public Container1()
    {
        crystals = new List<GameObject>();
    }

    public void UpdateContent(int? minerals, int? capacity)
    {
        if (!minerals.HasValue)
            return;

        if (minerals != filled)
        {
            int numCrystals = minerals.Value;

            while (crystals.Count > numCrystals)
            {
                GameObject crystal = crystals[crystals.Count - 1];
                crystals.Remove(crystal);
                Destroy(crystal);
            }
            while (crystals.Count < numCrystals)
            {
                Vector2 randomPos = Random.insideUnitCircle;

                Vector3 unitPos3 = transform.position;
                unitPos3.x += (randomPos.x * 0.01f);
                unitPos3.z += (randomPos.y * 0.01f);
                unitPos3.y += 0.05f;

                if (crystals.Count > 10)
                    unitPos3.y += 0.05f;

                if (Level == 3)
                {
                    unitPos3.y += 0.65f;
                }
                if (crystalResource == null)
                {
                    crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
                }
                GameObject crystal = Instantiate(crystalResource, transform, false);
                crystal.transform.SetPositionAndRotation(unitPos3, Random.rotation);
                crystals.Add(crystal);
            }

            filled = minerals.Value;
        }
    }
}

