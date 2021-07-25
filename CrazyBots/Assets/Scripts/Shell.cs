using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public ParticleSystem m_ExplosionParticles;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || string.IsNullOrEmpty(other.name))
            return;

        Rigidbody otherRigid = other.GetComponent<Rigidbody>();
        if (otherRigid != null)
        {
            Vector3 velo = otherRigid.velocity;
        }


        UnitBase hitUnit = GetUnitFrameFromCollider(other);
    

        bool targetHit = false;
        if (other.name.StartsWith("Ground"))
        {
            targetHit = true;
            Destroy(gameObject);

            if (TargetUnitId != "Dirt" || TargetUnitId != "Destructable")
            {
                if (HexGrid.BaseUnits.ContainsKey(TargetUnitId))
                    hitUnit = HexGrid.BaseUnits[TargetUnitId];
            }
        }
        else if (hitUnit != null && hitUnit.UnitId == TargetUnitId)
        {
            targetHit = true;
            Destroy(gameObject);
        }
        else
        {
        }
        if (targetHit)
        {
            if (hitUnit != null)
            {
                hitUnit.HitByShell();
            }
            m_ExplosionParticles.transform.parent = null;

            // Play the particle system.
            m_ExplosionParticles.Play();
            Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);

            /* overkill
            ParticleSystem particleTarget = UnitFrame.HexGrid.MakeParticleSource("TankExplosion");
            particleTarget.transform.position = transform.position;

            Vector3 unitPos3 = particleTarget.transform.position;
            unitPos3.y += 1.1f;
            particleTarget.transform.position = unitPos3;

            HexGrid.Destroy(particleTarget, 2.5f);

            particleTarget.Play();
            */

        }
    }

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = Random.rotation;
        transform.Rotate(Vector3.right); // * Time.deltaTime);
        transform.Rotate(Vector3.up); // * Time.deltaTime);
    }
}
