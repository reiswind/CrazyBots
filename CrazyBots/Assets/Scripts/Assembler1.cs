using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    private ParticleSystem particleSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

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

                particleSource = UnitFrame.HexGrid.MakeParticleSource();
                particleSource.transform.SetParent(sourceCell.transform, false);
            }

            Position to = UnitFrame.NextMove.Positions[1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[to];

            ParticleSystemForceField particleTarget = UnitFrame.HexGrid.MakeParticleTarget();
            particleTarget.transform.SetParent(targetCell.transform, false);

            particleSource.externalForces.SetInfluence(0, particleTarget);
            HexGrid.Destroy(particleTarget, 2.5f);

            particleSource.Play();

            UnitFrame.NextMove = null;
        }
    }
}
