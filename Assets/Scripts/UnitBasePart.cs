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

        public float AnimateFrom { get; set; }
        public float AnimateTo { get; set; }

        public void Fire(Move move)
        {
            foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
            {
                // Transit the ingrdient into the weapon. This is the reloaded ammo. (Can be empty)
                UnitBaseTileObject unitBaseTileObject;
                unitBaseTileObject = UnitBase.RemoveTileObject(moveRecipeIngredient);
                if (unitBaseTileObject != null)
                {
                    // Transit ingredient
                    TransitObject transitObject = new TransitObject();
                    transitObject.GameObject = unitBaseTileObject.GameObject;
                    transitObject.TargetPosition = Part.transform.position;
                    transitObject.DestroyAtArrival = true;

                    unitBaseTileObject.GameObject = null;
                    HexGrid.MainGrid.AddTransitTileObject(transitObject);
                }
            }
            if (TileObjectContainer.TileObjects.Count == 0)
            {
                int why = 0;
            }
            else
            {
                TileObject tileObjectAmmo = TileObjectContainer.TileObjects[0].TileObject;
                hitByBullet = HexGrid.MainGrid.Fire(UnitBase, tileObjectAmmo);

                Position2 targetPosition = move.Positions[move.Positions.Count - 1];

                GroundCell weaponTargetCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(targetPosition, out weaponTargetCell))
                {
                    // Determine which direction to rotate towards
                    Vector3 turnWeaponIntoDirection = (weaponTargetCell.transform.position - Part.transform.position).normalized;
                    turnWeaponIntoDirection.y = 0;
                    UnitBase.TurnWeaponIntoDirection = turnWeaponIntoDirection;
                }
            }
        }
        public void FireBullet()
        {
            GameObject gameObject = HexGrid.MainGrid.CreateShell(Part.transform, hitByBullet.Bullet);
            Shell shell = gameObject.GetComponent<Shell>();
            shell.FireingUnit = UnitBase;
            shell.HitByBullet = hitByBullet;

            GroundCell weaponTargetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(hitByBullet.TargetPosition, out weaponTargetCell))
            {
                Vector3 targetPos = weaponTargetCell.transform.position;
                Rigidbody rigidbody = shell.GetComponent<Rigidbody>();
                rigidbody.velocity = calcBallisticVelocityVector(shell.transform.position, targetPos, UnitBase.HasEngine()?30:-10);
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
