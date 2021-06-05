using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public ParticleSystem m_ExplosionParticles;

    internal string TargetUnitId { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || string.IsNullOrEmpty(other.name))
            return;

        bool targetHit = false;
        if (other.name.StartsWith("Ground"))
        {
            targetHit = true;
            Destroy(gameObject);
        }
        else if (other.name.StartsWith(TargetUnitId))
        {
            Rigidbody otherRigid = other.GetComponent<Rigidbody>();
            Vector3 velo = otherRigid.velocity;
            velo.y = 1.5f + (Random.value*3);
            otherRigid.velocity = velo;

            targetHit = true;
            Destroy(gameObject);
        }
        else
        {
        }
        if (targetHit)
        {
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
