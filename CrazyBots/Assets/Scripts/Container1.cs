using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : UnitPart
{

    void Awake()
    {
        //Mesh mesh = Resources.Load<Mesh>("Meshes/Container1");
        //GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    internal UnitFrame UnitFrame { get; set; }

    void Update()
    {
        if (UnitFrame.currentBaseFrame == this)
            UnitFrame.UpdateMove(this);
    }
}

