using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }


    // Update is called once per frame
    void Update()
    {
        UnitFrame?.Move(this);
    }
}
