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

    public void UpdateContent(HexGrid hexGrid, GameObject gameObject, List<TileObject> tileObjects, int? capacity)
    {
        if (capacity.HasValue && capacity <= 0) 
            return;

        int minerals = tileObjects.Count;


        if (mineralCubes.Count == 0)
        {
            AddMinerals(gameObject);
            
            filled = 0;
            max = mineralCubes.Count;
        }

        int mins = minerals;
        
        int minPercent = mins * 100 / capacity.Value;
        mins = minPercent * max / 100;

        if (minerals > 0 && mins == 0)
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



    public void Transport(HexGrid hexGrid, Move move)
    {
        Vector3 launchPosition;
        launchPosition = transform.position;
        launchPosition.y += 1;

        GameObject shellprefab = hexGrid.GetUnitResource("Transport");

        GameObject shellObject = Instantiate(shellprefab);
        Transport transport = shellObject.GetComponent<Transport>();

        Position pos = move.Positions[move.Positions.Count - 1];

        transport.HexGrid = hexGrid;

        Vector3 targetPosition;

        targetPosition = hexGrid.GroundCells[pos].transform.position;
        targetPosition.y += 1;

        transport.TargetPosition = targetPosition;

        transport.gameObject.hideFlags = HideFlags.HideAndDontSave;
        //transport.transform.SetPositionAndRotation(launchPosition, transform.rotation);
        //transport.transform.position = launchPosition;

        Vector3 newDirection = Vector3.RotateTowards(launchPosition, targetPosition, 360, 360);
        //transform.rotation = Quaternion.LookRotation(newDirection);

        transport.transform.SetPositionAndRotation(launchPosition, Quaternion.LookRotation(newDirection));


        Destroy(shellObject, 5f);
    }

    public void UpdateContent(HexGrid hexGrid, List<TileObject> tileObjects, int? capacity)
    {
        mineralContainer.UpdateContent(hexGrid, this.gameObject, tileObjects, capacity);
    }
}

