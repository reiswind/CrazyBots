using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralContainer
{

    public MineralContainer()
    {
        mineralCubes = new List<GameObject>();
        containers = new List<GameObject>();
    }

    private List<GameObject> mineralCubes;
    private List<GameObject> containers;
    private int filled;
    private int max;

    private bool AddMineral(GameObject container, int minNum)
    {
        Transform transform = container.transform.Find("Mineral" + minNum.ToString());
        if (transform == null) return false;

        mineralCubes.Add(transform.gameObject);
        transform.gameObject.SetActive(false);
        return true;
    }


    public void UpdateContent(HexGrid hexGrid, GameObject gameObject, int? minerals, int? capacity)
    {
        if (mineralCubes.Count == 0)
        {
            Transform transformContainer = gameObject.transform.Find("Container1");
            if (transformContainer == null)
            {
                containers.Add(gameObject);
            }
            else
            {
                containers.Add(transformContainer.gameObject);
                UnitBase.SetPlayerColor(hexGrid, 1, transformContainer.gameObject);

                transformContainer = gameObject.transform.Find("Container2");
                if (transformContainer != null)
                {
                    containers.Add(transformContainer.gameObject);
                    UnitBase.SetPlayerColor(hexGrid, 1, transformContainer.gameObject);

                    transformContainer = gameObject.transform.Find("Container3");
                    if (transformContainer != null)
                    {
                        containers.Add(transformContainer.gameObject);
                        UnitBase.SetPlayerColor(hexGrid, 1, transformContainer.gameObject);
                    }
                }
            }
            foreach (GameObject container in containers)
            {
                int minNum = 1;
                while (minNum < 99)
                {
                    if (!AddMineral(container, minNum++))
                        break;
                }
                /*
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
                filled += 12;*/
            }

            filled = 0;
            max = mineralCubes.Count;
        }

        if (!minerals.HasValue)
            return;

        int mins = minerals.Value;
        if (mins == 12)
        {
            int x = 0;
        }
        //if (mins != max)
        {
            int minPercent = mins * 100 / capacity.Value;
            mins = minPercent * max / 100;
        }

        if (mins != filled)
        {
            while (filled > mins)
            {
                filled--;
                if (filled < mineralCubes.Count)
                    mineralCubes[filled].SetActive(false);
            }
            while (filled < mins)
            {
                if (filled < mineralCubes.Count)
                    mineralCubes[filled].SetActive(true);
                filled++;
            }
        }
    }
}

public class Container1 : MonoBehaviour
{
    internal int Level { get; set; }

    private MineralContainer mineralContainer = new MineralContainer();

    public Container1()
    {

    }
    public void UpdateContent(HexGrid hexGrid, int? minerals, int? capacity)
    {
        mineralContainer.UpdateContent(hexGrid, this.gameObject, minerals, capacity);
    }
}

