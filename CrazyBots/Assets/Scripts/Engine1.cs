using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        AboveGround = 0.3f;
    }

    public int X;
    public int Z;
    private float AboveGround { get; set; }

    public Move NextMove { get; set; }
    public HexGrid HexGrid { get; set; }
    public Position FinalDestination { get; set; }
    void Update()
    {
        if (NextMove == null)
            return;
        if (NextMove.MoveType == MoveType.Delete)
        {
            
        }
        else if (NextMove.MoveType == MoveType.Move || NextMove.MoveType == MoveType.Add)
        {            
            FinalDestination = NextMove.Positions[NextMove.Positions.Count - 1];
            HexCell targetCell = HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y += AboveGround;

            float speed = 1.75f;
            float step = speed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);

            if (NextMove.MoveType == MoveType.Move)
            {
                // Nah...
                //transform.position = Vector3.RotateTowards(transform.position, unitPos3, step, 1);
                transform.LookAt(unitPos3);
            }
        }
    }

    public void JumpToTarget(Position pos)
    {
        //if (FinalDestination != null)
        {
            // Did not reach target in time. Jump to it.
            HexCell targetCell = HexGrid.GroundCells[pos];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            //unitPos3.y = 3; //-= AboveGround;
            unitPos3.y += AboveGround;
            transform.position = unitPos3;
            //FinalDestination = null;
        }
    }

}
