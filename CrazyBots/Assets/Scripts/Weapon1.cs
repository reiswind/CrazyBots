using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon1 : MonoBehaviour
{
    private MineralContainer mineralContainer = new MineralContainer();
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

    internal void UpdateContent(HexGrid hexGrid, int? minerals, int? capacity)
    {
        mineralContainer.UpdateContent(hexGrid, this.gameObject, minerals-1, capacity-1);

        Transform weapon = transform.Find("Weapon");
        if (weapon == null) weapon = transform;

        Transform ammo = weapon.Find("Ammo");
        if (ammo != null)
        {
            ammo.gameObject.SetActive(minerals > 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (turnWeaponIntoDirection != Vector3.zero)
        {
            Transform weaponPosition = transform.Find("Weapon");
            if (weaponPosition != null)
            {
                UpdateDirection(weaponPosition.transform);
            }
            else
            {
                UpdateDirection(transform);
            }
        }
    }

    private Vector3 turnWeaponIntoDirection = Vector3.zero;

    public void TurnTo(HexGrid hexGrid, Position pos)
    {
        GroundCell weaponTargetCell;
        weaponTargetCell = hexGrid.GroundCells[pos];

        // Determine which direction to rotate towards
        turnWeaponIntoDirection = (weaponTargetCell.transform.position - transform.position).normalized;
        turnWeaponIntoDirection.y = 0;
    }

    private HexGrid hexGrid;
    private Move move;
    private GroundCell weaponTargetCell;

    public void Fire(HexGrid hexGrid, Move move)
    {
        Position pos = move.Positions[move.Positions.Count - 1];
        weaponTargetCell = hexGrid.GroundCells[pos];
        this.move = move;
        this.hexGrid = hexGrid;

        // Determine which direction to rotate towards
        turnWeaponIntoDirection = (weaponTargetCell.transform.position - transform.position).normalized;
        turnWeaponIntoDirection.y = 0;
    }

    void UpdateDirection(Transform transform)
    {
        float str; // = Mathf.Min(2f * Time.deltaTime, 1);
        str = 4f * Time.deltaTime;

        // Calculate a rotation a step closer to the target and applies rotation to this object
        Quaternion lookRotation = Quaternion.LookRotation(turnWeaponIntoDirection);

        // Rotate the forward vector towards the target direction by one step
        Quaternion newrotation = Quaternion.Slerp(transform.rotation, lookRotation, str);

        float angle = Quaternion.Angle(lookRotation, newrotation);
        if (angle < 5)
        {
            if (weaponTargetCell != null)
            {
                angle = 25;
                Transform launchPosition;
                Transform weaponPosition = transform.Find("Weapon");
                if (weaponPosition != null)
                {
                    angle = 2;
                    launchPosition = weaponPosition.transform.Find("Ammo");
                }
                else
                {
                    launchPosition = transform.Find("Ammo");
                }

                if (launchPosition == null)
                {
                    Debug.LogError("Missing Ammo");
                    return;
                }
                launchPosition.gameObject.SetActive(false);

                GameObject shellprefab = hexGrid.GetUnitResource("Shell");

                GameObject shellObject = Instantiate(shellprefab);
                Shell shell = shellObject.GetComponent<Shell>();

                shell.gameObject.hideFlags = HideFlags.HideAndDontSave;
                shell.transform.SetPositionAndRotation(launchPosition.position, launchPosition.rotation);

                shell.TargetUnitId = move.OtherUnitId;

                Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
                rigidbody.velocity = calcBallisticVelocityVector(launchPosition.position, weaponTargetCell.transform.position, angle);

                Destroy(shellObject, 2.6f);

                turnWeaponIntoDirection = Vector3.zero;
                weaponTargetCell = null;
            }
        }
        transform.rotation = newrotation;
    }
}
