using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Shell : MonoBehaviour
    {
        public ParticleSystem m_ExplosionParticles;

        public UnitBase FireingUnit { get; set; }
        internal string TargetUnitId { get; set; }
        internal HexGrid HexGrid { get; set; }

        private UnitBase GetUnitFrameFromCollider(Collider other)
        {
            UnitBase unitBase = other.GetComponent<UnitBase>();
            if (unitBase != null) return unitBase;

            Transform transform = other.transform;

            while (transform.parent != null)
            {
                unitBase = transform.parent.GetComponent<UnitBase>();
                if (unitBase != null) return unitBase;
                if (transform.parent == null)
                    break;
                transform = transform.parent;
            }
            return null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);

            Collider other = collision.collider;
            if (other == null || string.IsNullOrEmpty(other.name))
                return;

            Rigidbody otherRigid = other.GetComponent<Rigidbody>();
            if (otherRigid != null)
            {
                Vector3 velo = otherRigid.velocity;
            }


            UnitBase hitUnit = GetUnitFrameFromCollider(other);
            if (hitUnit == null)
            {
                // Play some hit ground animation
            }
            else
            {
                hitUnit.Impact();
            }
            /*


            bool targetHit = false;
            if (other.name.StartsWith("Ground"))
            {
                targetHit = true;

                if (TargetUnitId != "Dirt" || TargetUnitId != "Destructable")
                {
                    //if (HexGrid.BaseUnits.ContainsKey(TargetUnitId))
                    //    hitUnit = HexGrid.BaseUnits[TargetUnitId];
                }
            }
            else if (hitUnit != null && hitUnit.UnitId == TargetUnitId)
            {
                targetHit = true;
            }
            else
            {
                targetHit = true;
            }
            if (targetHit)
            {

                m_ExplosionParticles.transform.parent = null;

                // Play the particle system.
                m_ExplosionParticles.Play();
                Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
            }*/
        }

        private void OnTriggerEnter(Collider other)
        {
            UnitBase hitUnit = GetUnitFrameFromCollider(other);
            if (hitUnit == FireingUnit)
            {
                return;
            }
            if (hitUnit == null)
            {
                // Play some hit ground animation
            }
            else
            {
                hitUnit.Impact();
            }

            m_ExplosionParticles.transform.parent = null;

            // Play the particle system.
            m_ExplosionParticles.Play();
            Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
        }

        // Update is called once per frame
        void Update()
        {
            //transform.rotation = Random.rotation;
            transform.Rotate(Vector3.right); // * Time.deltaTime);
            transform.Rotate(Vector3.up); // * Time.deltaTime);
        }
    }
}