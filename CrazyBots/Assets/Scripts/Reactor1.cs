using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactor1 : MonoBehaviour
{
    private MineralContainer mineralContainer = new MineralContainer();

    public void UpdateContent(HexGrid hexGrid, int? minerals, int? capacity)
    {
        mineralContainer.UpdateContent(hexGrid, this.gameObject, minerals, capacity);
    }
}
