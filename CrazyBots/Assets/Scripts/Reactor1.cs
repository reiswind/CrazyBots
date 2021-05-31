using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactor1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    // Update is called once per frame
    void Update()
    {
        if (UnitFrame == null)
            return;

        UnitFrame.Move(this);
    }
}
