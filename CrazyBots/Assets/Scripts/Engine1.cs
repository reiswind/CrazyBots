using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine1 : MonoBehaviour
{
    private float AboveGround { get; set; }
    public UnitFrame UnitFrame { get; set; }

    public string UnitId { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        AboveGround = 0.0f;
    }

    void Update()
    {
        if (UnitFrame == null)
            return;

        UnitFrame.Move(this);
        if (false && UnitFrame.NextMove?.MoveType == MoveType.Add)
        {
            Position FinalDestination = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.Cell.transform.localPosition;
            unitPos3.y += UnitFrame.HexGrid.hexCellHeight + AboveGround;

            transform.position = Vector3.MoveTowards(transform.position, unitPos3, 1);
        }
        else if (UnitFrame.NextMove?.MoveType == MoveType.Move || UnitFrame.NextMove?.MoveType == MoveType.Add)
        {
            Position FinalDestination = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.Cell.transform.localPosition;
            unitPos3.y += UnitFrame.HexGrid.hexCellHeight + AboveGround;

            if (UnitFrame.NextMove.Positions.Count > 0)
            {
                float speed = 1.75f / UnitFrame.HexGrid.GameSpeed;
                float step = speed * Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);
                UpdateDirection(unitPos3);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, unitPos3, 1);
            }
        }
    }

    void UpdateDirection(Vector3 position)
    {
        //float speed = 1.75f;
        float speed = 3.5f / UnitFrame.HexGrid.GameSpeed;

        // Determine which direction to rotate towards
        Vector3 targetDirection = position - transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        newDirection.y = 0;

        // Draw a ray pointing at our target in
        //Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

}
