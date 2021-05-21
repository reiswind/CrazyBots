using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactor1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UnitFrame?.Move(this);
    }
}
