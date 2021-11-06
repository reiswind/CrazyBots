using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitBasePart
    {
        public TileObjectContainer TileObjectContainer { get; set; }
        public UnitBasePart(UnitBase unitBase)
        {
            this.UnitBase = unitBase;
        }

        public UnitBase UnitBase { get; private set; }

        public string Name { get; set; }
        public TileObjectType PartType { get; set; }
        public int Level { get; set; }
        public int CompleteLevel { get; set; }
        public bool IsUnderConstruction { get; set; }
        public bool Destroyed { get; set; }
        public GameObject Part { get; set; }

        private HitByBullet hitByBullet;

        public void Fire(Move move)
        {
            TileObject anmo = move.Stats.MoveUpdateGroundStat.TileObjects[0];

            hitByBullet = HexGrid.MainGrid.Fire(UnitBase, anmo);

            Position2 pos = move.Positions[move.Positions.Count - 1];

            GroundCell weaponTargetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(pos, out weaponTargetCell))
            {
                // Determine which direction to rotate towards
                Vector3 turnWeaponIntoDirection = (weaponTargetCell.transform.position - Part.transform.position).normalized;
                turnWeaponIntoDirection.y = 0;
                UnitBase.TurnWeaponIntoDirection = turnWeaponIntoDirection;
            }
        }
        public void FireBullet()
        {
            GameObject gameObject = HexGrid.MainGrid.CreateShell(Part.transform, hitByBullet.Bullet);
            Shell shell = gameObject.GetComponent<Shell>();
            //shell.transform.SetPositionAndRotation(launchPos, ammo.transform.rotation);
            shell.FireingUnit = UnitBase;
            shell.HitByBullet = hitByBullet;

            GroundCell weaponTargetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(hitByBullet.TargetPosition, out weaponTargetCell))
            {
                Vector3 targetPos = weaponTargetCell.transform.position;
                Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
                rigidbody.velocity = calcBallisticVelocityVector(shell.transform.position, targetPos, 45);
            }
        }
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
        public void UpdateContent(List<TileObject> tileObjects, int? capacity)
        {
            if (tileObjects == null || TileObjectContainer == null)
                return;

            if (UnitBase.gameObject == null)
            {

            }
            else
            {
                try
                {
                    if (Part.gameObject != null)
                        TileObjectContainer.UpdateContent(UnitBase, Part, tileObjects, capacity);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
