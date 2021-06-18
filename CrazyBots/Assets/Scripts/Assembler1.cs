using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assembler1 : MonoBehaviour
{
    private MineralContainer mineralContainer = new MineralContainer();
    internal int Level { get; set; }

    public void Assemble(HexGrid hexGrid, Move move)
    {
        ParticleSystem particleSource;
        
        Position from = move.Positions[0];
        GroundCell sourceCell = hexGrid.GroundCells[from];

        particleSource = hexGrid.MakeParticleSource("ExtractSource");
        particleSource.transform.SetParent(sourceCell.transform, false);

        Position to = move.Positions[move.Positions.Count - 1];
        GroundCell targetCell = hexGrid.GroundCells[to];

        ParticleSystemForceField particleTarget = hexGrid.MakeParticleTarget();
        particleTarget.transform.SetParent(targetCell.transform, false);

        Vector3 unitPos3 = particleTarget.transform.position;
        unitPos3.y += 0.1f;
        particleTarget.transform.position = unitPos3;

        particleSource.externalForces.SetInfluence(0, particleTarget);
        HexGrid.Destroy(particleTarget, 2.5f);

        //var main = particleSource.main;
        //main.duration = particleSource.main.duration * hexGrid.GameSpeed;

        //particleSource.main.duration = particleSource.main.duration * UnitFrame.HexGrid.GameSpeed;
        particleSource.Play();

        ParticleSystem particleDust = hexGrid.MakeParticleSource("Build");
        particleDust.transform.SetParent(targetCell.transform, false);
        particleDust.transform.position = particleTarget.transform.position;

        particleDust.Play();
        HexGrid.Destroy(particleDust, 2.5f);
    }

    public void UpdateContent(HexGrid hexGrid, int? minerals, int? capacity)
    {
        mineralContainer.UpdateContent(hexGrid, this.gameObject, minerals, capacity);
    }
}
