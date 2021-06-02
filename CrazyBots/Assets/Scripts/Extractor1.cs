using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extractor1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    // Update is called once per frame
    void Update()
    {
        if (UnitFrame == null)
            return;

        UnitFrame.Move(this);
        if (UnitFrame.NextMove?.MoveType == MoveType.Extract)
        {
            ParticleSystem particleSource;

            Position from = UnitFrame.NextMove.Positions[1];
            GroundCell sourceCell = UnitFrame.HexGrid.GroundCells[from];

            particleSource = UnitFrame.HexGrid.MakeParticleSource("ExtractSource");
            particleSource.transform.SetParent(sourceCell.transform, false);

            Position to = UnitFrame.NextMove.Positions[0];
            GroundCell targetCell = UnitFrame.HexGrid.GroundCells[to];

            ParticleSystemForceField particleTarget = UnitFrame.HexGrid.MakeParticleTarget();
            particleTarget.transform.SetParent(targetCell.transform, false);

            Vector3 unitPos3 = particleTarget.transform.position;
            unitPos3.y += 0.1f;
            particleTarget.transform.position = unitPos3;

            particleSource.externalForces.SetInfluence(0, particleTarget);
            HexGrid.Destroy(particleTarget, 2.5f);

            particleSource.Play();

            UnitFrame.NextMove = null;
        }
    }
}
