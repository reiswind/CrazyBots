using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public ParticleSystem m_ExplosionParticles;

    internal string TargetUnitId { get; set; }
    public UnitFrame UnitFrame { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, 2.6f);
    }

    private void OnTriggerEnter(Collider other)
    {
        bool targetHit = false;
        if (other.name.StartsWith("Ground"))
        {
            targetHit = true;
            Destroy(this.gameObject);
        }
        else if (other.name.StartsWith(TargetUnitId))
        {
            targetHit = true;
            Destroy(this.gameObject);
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
