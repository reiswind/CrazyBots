using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactor1 : MonoBehaviour
{
    private MineralContainer mineralContainer = new MineralContainer();

    public void UpdateContent(HexGrid hexGrid, List<TileObject> tileObjects, int? capacity)
    {
        mineralContainer.UpdateContent(hexGrid, this.gameObject, tileObjects, capacity);
    }
}
