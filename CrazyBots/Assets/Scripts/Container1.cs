using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : MonoBehaviour
{
    public float AboveGround { get; set; }
    public UnitFrame UnitFrame { get; set; }

    internal int Level { get; set; }

    private List<GameObject> crystals;

    private int filled;

    void Awake()
    {
        crystals = new List<GameObject>();
    }

    void Update()
    {
        if (UnitFrame == null)
            return;

        UnitFrame.Move(this);
    }

    public void UpdateContent(int? percentage)
    {
        if (percentage.HasValue)
        {
            if (percentage != filled)
            {
                int numCrystals = 0;

                if (Level == 1)
                {
                    numCrystals = percentage.Value * 20 / 100;
                    if (numCrystals > 20)
                        numCrystals = 20;
                }
                if (Level == 2)
                {
                    numCrystals = (percentage.Value * 60) / 1000;
                    if (numCrystals > 60)
                        numCrystals = 60;
                }
                if (Level == 3)
                {
                    numCrystals = (percentage.Value * 220) / 1000;
                    if (numCrystals > 220)
                        numCrystals = 220;
                }

                while (crystals.Count > numCrystals)
                {                  
                    GameObject crystal = crystals[crystals.Count-1];
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


                    GameObject crystalResource = Resources.Load<GameObject>("Prefabs/Terrain/Crystal");
                    GameObject crystal = Instantiate(crystalResource, transform, false);

                    crystal.transform.SetPositionAndRotation(unitPos3, Random.rotation);
                    crystals.Add(crystal);
                }

                filled = percentage.Value;
            }
        }
    }

}

