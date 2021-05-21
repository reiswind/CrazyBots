using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Container1 : MonoBehaviour
{
    public float AboveGround { get; set; }
    public UnitFrame UnitFrame { get; set; }

    void Awake()
    {
        //Mesh mesh = Resources.Load<Mesh>("Meshes/Container1");
        //GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void Update()
    {
        UnitFrame?.Move(this);
    }
}

