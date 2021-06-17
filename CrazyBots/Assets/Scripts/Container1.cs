using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : MonoBehaviour
{
    internal int Level { get; set; }

    private List<GameObject> mineralCubes;
    private List<GameObject> containers;
    private int filled;
    private int max;

    public Container1()
    {
        mineralCubes = new List<GameObject>();
        containers = new List<GameObject>();
    }

    public void UpdateContent(HexGrid hexGrid, int? minerals, int? capacity)
    {        
        if (mineralCubes.Count == 0)
        {
            Transform transformContainer = transform.Find("Container1");
            if (transformContainer == null)
            {
                containers.Add(this.gameObject);
            }
            else
            {
                containers.Add(transformContainer.gameObject);
                UnitBase.SetPlayerColor(hexGrid, 1, transformContainer.gameObject);

                transformContainer = transform.Find("Container2");
                if (transformContainer != null)
                {
                    containers.Add(transformContainer.gameObject);
                    UnitBase.SetPlayerColor(hexGrid, 1, transformContainer.gameObject);

                    transformContainer = transform.Find("Container3");
                    if (transformContainer != null)
                    {
                        containers.Add(transformContainer.gameObject);
                        UnitBase.SetPlayerColor(hexGrid, 1, transformContainer.gameObject);
                    }
                }
            }
            foreach (GameObject container in containers)
            {
                mineralCubes.Add(container.transform.Find("Mineral1").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral2").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral3").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral4").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral5").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral6").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral7").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral8").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral9").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral10").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral11").gameObject);
                mineralCubes.Add(container.transform.Find("Mineral12").gameObject);
                filled += 12;
            }
            max = filled;
        }

        if (!minerals.HasValue)
            return;

        int mins = minerals.Value;
        if  (mins != max)
        {
            int minPercent = mins * 100 / capacity.Value;
            mins = minPercent * max / 100;
        }

        if (mins != filled)
        {
            while (filled > mins)
            {
                filled--;
                if (filled < 12)
                    mineralCubes[filled].SetActive(false);
            }
            while (filled < mins)
            {
                if (filled < 12)
                    mineralCubes[filled].SetActive(true);
                filled++;
            }
        }
    }
}

