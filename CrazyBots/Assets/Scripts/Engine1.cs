using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AboveGround = 4.3f;
    }

    public int X;
    public int Z;
    public float AboveGround { get; set; }

    public Move NextMove { get; set; }
    public HexGrid HexGrid { get; set; }
    public Position FinalDestination { get; set; }
    void Update()
    {
        if (NextMove != null)
        {
            FinalDestination = NextMove.Positions[1];
            HexCell targetCell = HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y = 3; //-= AboveGround;
            //transform.localPosition = unitPos3;

            float speed = 1.75f;
            float step = speed * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position, targetCell.transform.localPosition, step);
            transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);

            //transform.m
            transform.LookAt(unitPos3);

            //NextMove = null;
        }
    }

    public void JumpToTarget(Position pos)
    {
        //if (FinalDestination != null)
        {
            // Did not reach target in time. Jump to it.
            HexCell targetCell = HexGrid.GroundCells[pos];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y = 3; //-= AboveGround;
            transform.position = unitPos3;
            //FinalDestination = null;
        }
    }

}
