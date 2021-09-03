using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Weapon1 : MonoBehaviour
    {
        private Vector3 calcBallisticVelocityVector(Vector3 initialPos, Vector3 finalPos, float angle)
        {
            var toPos = initialPos - finalPos;

            var h = toPos.y;

            toPos.y = 0;
            var r = toPos.magnitude;

            //float rpercent = r * 10; // / 100;
            //angle = 70 * rpercent / 100;
            //if (r > 5)
            //    angle = 30;

            var g = -Physics.gravity.y;
            var a = Mathf.Deg2Rad * angle;

            var vI = Mathf.Sqrt(((Mathf.Pow(r, 2f) * g)) / (r * Mathf.Sin(2f * a) + 2f * h * Mathf.Pow(Mathf.Cos(a), 2f)));

            Vector3 velocity = (finalPos - initialPos).normalized * Mathf.Cos(a);
            velocity.y = Mathf.Sin(a);

            return velocity * vI;
        }

        private UnitBaseTileObject GetAmmoTileObject(TileObjectContainer tileObjectContainer)
        {
            if (tileObjectContainer != null && tileObjectContainer.TileObjects.Count > 0)
            {
                UnitBaseTileObject unitBaseTileObject = tileObjectContainer.TileObjects[0];
                if (unitBaseTileObject.GameObject != null)
                {
                    return unitBaseTileObject;
                }
            }
            return null;
        }

        private UnitBaseTileObject ammoTileObject;

        internal void UpdateContent(HexGrid hexGrid, TileObjectContainer tileObjectContainer)
        {
            GameObject weapon = UnitBase.FindChildNyName(this.gameObject, "Weapon");
            if (weapon != null)
            {
                GameObject ammo = UnitBase.FindChildNyName(weapon, "Ammo");
                if (ammo != null)
                {
                    UnitBaseTileObject haveAmmo = GetAmmoTileObject(tileObjectContainer);

                    if (haveAmmo != null && haveAmmo.GameObject != null)
                    {
                        if (ammoTileObject != null)
                        {
                            // happened
                            //throw new System.Exception("double ammo");
                        }
                        ammoTileObject = haveAmmo;
                        ammoTileObject.GameObject.transform.position = ammo.transform.position;
                        ammoTileObject.GameObject.SetActive(true);
                    }
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
        private UnitBase fireingUnit;

        public void Fire(HexGrid hexGrid, UnitBase fireingUnit, Move move, TileObjectContainer tileObjectContainer)
        {
            if (ammoTileObject != null && ammoTileObject.GameObject != null)
            {
                this.fireingUnit = fireingUnit;
                tileObjectContainer.TileObjects.Remove(ammoTileObject);

                Position pos = move.Positions[move.Positions.Count - 1];
                weaponTargetCell = hexGrid.GroundCells[pos];
                this.move = move;
                this.hexGrid = hexGrid;

                // Determine which direction to rotate towards
                turnWeaponIntoDirection = (weaponTargetCell.transform.position - transform.position).normalized;
                turnWeaponIntoDirection.y = 0;
            }
        }

        void UpdateDirection(Transform transform)
        {
            float str; // = Mathf.Min(2f * Time.deltaTime, 1);
            str = 8f * Time.deltaTime;

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

                    /*
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
                    */

                    if (ammoTileObject != null && ammoTileObject.GameObject != null)
                    {
                        GameObject shellprefab = hexGrid.GetUnitResource("Shell");
                        GameObject shellObject = Instantiate(shellprefab);

                        GameObject ammo = ammoTileObject.GameObject;

                        Vector3 launchPos = ammo.transform.position;
                        //launchPos.y += 0.5f;

                        Shell shell = shellObject.GetComponent<Shell>();
                        shell.transform.SetPositionAndRotation(launchPos, ammo.transform.rotation);
                        shell.FireingUnit = fireingUnit;
                        ammo.transform.SetParent(shell.transform, false);

                        //Vector3 launchPos = launchPosition.position;
                        //launchPos.y += 0.5f;
                        //shell.gameObject.hideFlags = HideFlags.HideAndDontSave;
                        //shell.transform.SetPositionAndRotation(launchPos, launchPosition.rotation);

                        //shell.TargetUnitId = move.OtherUnitId;
                        //shell.HexGrid = hexGrid;

                        Vector3 targetPos = weaponTargetCell.transform.position;
                        //targetPos.y += 0.5f;

                        Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
                        rigidbody.velocity = calcBallisticVelocityVector(shell.transform.position, targetPos, angle);
                        //rigidbody.rotation = Random.rotation;

                        //Destroy(shellObject, 2.6f);

                        turnWeaponIntoDirection = Vector3.zero;
                        weaponTargetCell = null;

                        ammoTileObject = null;
                    }
                }
            }
            transform.rotation = newrotation;
        }
    }
}