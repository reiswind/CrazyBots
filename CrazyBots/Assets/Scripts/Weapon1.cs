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

        float rpercent = r * 10; // / 100;
        angle = 70 * rpercent / 100;
        //if (r > 5)
        //    angle = 30;

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

        GameObject weapon = UnitBase.FindChildNyName(this.gameObject, "Weapon");
        if (weapon != null)
        {
            GameObject ammo = UnitBase.FindChildNyName(weapon, "Ammo");
            if (ammo != null)
            {
                ammo.SetActive(minerals > 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (turnWeaponIntoDirection != Vector3.zero)
        {
            GameObject weapon = UnitBase.FindChildNyName(this.gameObject, "Weapon");
            if (weapon != null)
            {
                UpdateDirection(weapon.transform);
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
                angle = 45;

                GameObject weapon = UnitBase.FindChildNyName(this.gameObject, "Weapon");

                Transform launchPosition = null;
                if (weapon != null)
                {
                    GameObject ammo = UnitBase.FindChildNyName(weapon, "Ammo");
                    if (ammo == null)
                    {
                        Debug.LogError("Missing Ammo");
                        return;
                    }

                    //angle = 2;
                    launchPosition = ammo.transform;
                }
                if (weapon == null)
                {
                    Debug.LogError("Missing weapon");
                    return;
                }
                launchPosition.gameObject.SetActive(false);

                GameObject shellprefab = hexGrid.GetUnitResource("Shell");

                GameObject shellObject = Instantiate(shellprefab);
                Shell shell = shellObject.GetComponent<Shell>();

                Vector3 launchPos = launchPosition.position;
                launchPos.y += 0.5f;

                shell.gameObject.hideFlags = HideFlags.HideAndDontSave;
                shell.transform.SetPositionAndRotation(launchPos, launchPosition.rotation);

                shell.TargetUnitId = move.OtherUnitId;
                shell.HexGrid = hexGrid;

                Vector3 targetPos = weaponTargetCell.transform.position;
                targetPos.y += 0.5f;

                Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
                rigidbody.velocity = calcBallisticVelocityVector(launchPos, targetPos, angle);
                rigidbody.rotation = Random.rotation;

                //Destroy(shellObject, 2.6f);

                turnWeaponIntoDirection = Vector3.zero;
                weaponTargetCell = null;
            }
        }
        transform.rotation = newrotation;
    }
}
