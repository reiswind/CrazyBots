using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    private ParticleSystem particleSource;

    // Update is called once per frame
    void Update()
    {
        UnitFrame.Move(this);

        if (UnitFrame.NextMove?.MoveType == MoveType.Upgrade)
        {
            if (particleSource == null)
            {
                Position from = UnitFrame.NextMove.Positions[0];
                HexCell sourceCell = UnitFrame.HexGrid.GroundCells[from];

                particleSource = UnitFrame.HexGrid.MakeParticleSource("ExtractSource");
                particleSource.transform.SetParent(sourceCell.Cell.transform, false);
            }

            Position to = UnitFrame.NextMove.Positions[1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[to];

            ParticleSystemForceField particleTarget = UnitFrame.HexGrid.MakeParticleTarget();
            particleTarget.transform.SetParent(targetCell.Cell.transform, false);

            Vector3 unitPos3 = particleTarget.transform.position;
            unitPos3.y += 0.1f;
            particleTarget.transform.position = unitPos3;

            particleSource.externalForces.SetInfluence(0, particleTarget);
            HexGrid.Destroy(particleTarget, 2.5f);

            var main = particleSource.main;
            main.duration = particleSource.main.duration * UnitFrame.HexGrid.GameSpeed;

            //particleSource.main.duration = particleSource.main.duration * UnitFrame.HexGrid.GameSpeed;
            particleSource.Play();

            ParticleSystem particleDust = UnitFrame.HexGrid.MakeParticleSource("Build");
            particleDust.transform.SetParent(targetCell.Cell.transform, false);
            particleDust.transform.position = particleTarget.transform.position;

            particleDust.Play();
            HexGrid.Destroy(particleDust, 2.5f);

            UnitFrame.NextMove = null;
        }
    }
}
