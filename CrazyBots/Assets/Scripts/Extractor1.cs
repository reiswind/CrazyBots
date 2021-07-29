using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extractor1 : MonoBehaviour
{
    public void Extract(HexGrid hexGrid, Move move, UnitBase otherUnit)
    {
        ParticleSystem particleSource;

        Position from = move.Positions[1];
        GroundCell sourceCell = hexGrid.GroundCells[from];

        particleSource = hexGrid.MakeParticleSource("ExtractSource");
        particleSource.transform.SetParent(sourceCell.transform, false);

        Position to = move.Positions[0];
        GroundCell targetCell = hexGrid.GroundCells[to];

        ParticleSystemForceField particleTarget = hexGrid.MakeParticleTarget();
        particleTarget.transform.SetParent(targetCell.transform, false);

        Vector3 unitPos3 = particleTarget.transform.position;
        unitPos3.y += 0.1f;
        particleTarget.transform.position = unitPos3;

        particleSource.externalForces.SetInfluence(0, particleTarget);
        HexGrid.Destroy(particleTarget, 2.5f);

        if (otherUnit != null)
        {
            otherUnit.PartExtracted();
        }
        particleSource.Play();
    }
}
