using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon1 : MonoBehaviour
{

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

    internal void UpdateContent(bool weaponLoaded)
    {
        Transform weapon = transform.Find("Weapon");
        if (weapon == null) weapon = transform;

        Transform ammo = weapon.Find("Ammo");
        if (ammo != null)
        {
            ammo.gameObject.SetActive(weaponLoaded);
        }
    }

    public void Fire(HexGrid hexGrid, Move move)
    {
        Position FinalDestination = move.Positions[move.Positions.Count - 1];
        GroundCell targetCell = hexGrid.GroundCells[FinalDestination];

        float angle = 25;
        Transform launchPosition;
        Transform weaponPosition = transform.Find("Weapon");
        if (weaponPosition != null)
        {
            angle = 2;
            UpdateDirection(weaponPosition.transform, targetCell.transform.position, 0.04f);
            launchPosition = weaponPosition.transform.Find("Ammo");
        }
        else
        {
            UpdateDirection(transform, targetCell.transform.position, 3.4f);
            launchPosition = transform.Find("Ammo");
        }

        //launchPosition = transform.Find("Ammo");
        if (launchPosition == null)
        {
            Debug.LogError("Missing Ammo");
            return;
        }
        launchPosition.gameObject.SetActive(false);

        /*
        Vector3 unitPos3 = targetCell.Cell.transform.localPosition;
        unitPos3.y += UnitFrame.HexGrid.hexCellHeight;
        UpdateDirection(unitPos3);
        Vector3 launchPosition = transform.position;*/

        //launchPosition.x += 1.1f;
        //launchPosition.z += 1.1f;
        //launchPosition.y += 1f;

        //shot = true;
        Shell shell = hexGrid.InstantiatePrefab<Shell>("Shell");
        shell.gameObject.hideFlags = HideFlags.HideAndDontSave;
        //shell.transform.position = launchPosition; // transform.position;
        shell.transform.SetPositionAndRotation(launchPosition.position, launchPosition.rotation);

        shell.TargetUnitId = move.OtherUnitId;
        
        Rigidbody rigidbody = shell.GetComponent<Rigidbody>();

        rigidbody.velocity = calcBallisticVelocityVector(launchPosition.position, targetCell.transform.position, angle);


    }
    

    static void UpdateDirection(Transform transform, Vector3 position, float speed)
    {
        //float speed = 1.75f;
        //float speed = 3.5f / UnitFrame.HexGrid.GameSpeed;

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
