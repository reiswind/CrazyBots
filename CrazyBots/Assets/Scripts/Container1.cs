using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralContainer
{

    public MineralContainer()
    {
        mineralCubes = new List<GameObject>();
    }

    private List<GameObject> mineralCubes;
    private int filled;
    private int max;

    private bool AddMinerals(GameObject container)
    {
        if (container.name.StartsWith("Mineral"))
        {
            mineralCubes.Add(container);
            container.SetActive(false);
        }
        else
        {
            for (int i = 0; i < container.transform.childCount; i++)
            {
                GameObject child = container.transform.GetChild(i).gameObject;
                AddMinerals(child);
            }
        }
        return true;
    }

    public void UpdateContent(HexGrid hexGrid, GameObject gameObject, int? minerals, int? capacity)
    {
        if (capacity.HasValue && capacity <= 0)
            return;

        if (minerals.HasValue && minerals < 0)
            minerals = 0;

        if (mineralCubes.Count == 0)
        {
            AddMinerals(gameObject);
            
            filled = 0;
            max = mineralCubes.Count;
        }

        if (!minerals.HasValue)
            return;

        int mins = minerals.Value;
        
        int minPercent = mins * 100 / capacity.Value;
        mins = minPercent * max / 100;

        if (minerals.Value > 0 && mins == 0)
            mins = 1;

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

