using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    private int shot;

    private Vector3 calcBallisticVelocityVector(Vector3 initialPos, Vector3 finalPos, float angle)
    {
        var toPos = initialPos - finalPos;

        var h = toPos.y;

        toPos.y = 0;
        var r = toPos.magnitude;

        var g = -Physics.gravity.y;

        var a = Mathf.Deg2Rad * angle;

        var vI = Mathf.Sqrt(((Mathf.Pow(r, 2f) * g)) / (r * Mathf.Sin(2f * a) + 2f * h * Mathf.Pow(Mathf.Cos(a), 2f)));

        Vector3 velocity = (finalPos - initialPos).normalized * Mathf.Cos(a);
        velocity.y = Mathf.Sin(a);

        return velocity * vI;
    }

    // Update is called once per frame
    void Update()
    {
        UnitFrame?.Move(this);
        if (UnitFrame.NextMove?.MoveType == MoveType.Move)
        {
            if (shot > 0)
            {
                shot--;
                if (shot == 0)
                {
                    // Point weapon into move direction (TOdo;: Does not work)
                    Position FinalDestination = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
                    HexCell targetCell = UnitFrame.HexGrid.GroundCells[FinalDestination];

                    Vector3 unitPos3 = targetCell.Cell.transform.localPosition;
                    unitPos3.y += UnitFrame.HexGrid.hexCellHeight;
                    UpdateDirection(unitPos3);
                }
            }
        }
        if (UnitFrame.NextMove?.MoveType == MoveType.Fire)
        {
            Position FinalDestination = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.Cell.transform.localPosition;
            unitPos3.y += UnitFrame.HexGrid.hexCellHeight;
            UpdateDirection(unitPos3);

            Vector3 launchPosition = transform.position;
            //launchPosition.x += 1.1f;
            //launchPosition.z += 1.1f;
            launchPosition.y += 1f;

            //shot = true;
            Shell shell = UnitFrame.HexGrid.InstantiatePrefab<Shell>("Shell");
            shell.transform.position = launchPosition; // transform.position;
            shell.transform.rotation = transform.rotation;

            shell.TargetUnitId = UnitFrame.NextMove.OtherUnitId;
            shell.UnitFrame = UnitFrame;

            Rigidbody rigidbody = shell.GetComponent<Rigidbody>();

            rigidbody.velocity = calcBallisticVelocityVector(launchPosition, targetCell.Cell.transform.position, 25);
            shot = 10;

            UnitFrame.NextMove = null;
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
        singleStep = 360;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        newDirection.y = 0;
        // Draw a ray pointing at our target in
        //Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
