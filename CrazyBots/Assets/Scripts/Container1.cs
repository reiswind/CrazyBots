using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : MonoBehaviour
{

    internal UnitFrame UnitFrame { get; set; }

    void Update()
    {
        UnitFrame.UpdateMove(this, 0);
    }
}

