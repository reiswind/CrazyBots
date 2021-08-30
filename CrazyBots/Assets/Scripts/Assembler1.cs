using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Assembler1 : MonoBehaviour
    {
        public void Assemble(HexGrid hexGrid, UnitBase unit, UnitBase upgradedUnit, Move move)
        {
            MoveUpdateUnitPart moveUpdateUnitPart = move.Stats.UnitParts[0];
            foreach (UnitBasePart upgradedBasePart in upgradedUnit.UnitBaseParts)
            {
                if (upgradedBasePart.PartType == moveUpdateUnitPart.PartType &&
                    upgradedBasePart.CompleteLevel == moveUpdateUnitPart.Level)
                {
                    TransitObject transitObject = new TransitObject();
                    transitObject.GameObject = upgradedBasePart.Part;
                    transitObject.TargetPosition = upgradedBasePart.Part.transform.position;
                    transitObject.TargetRotation = upgradedBasePart.Part.transform.rotation;

                    // Reset current pos to assembler
                    upgradedBasePart.Part.transform.position = transform.position;


                    upgradedBasePart.Part.SetActive(true);

                    // Move to position in unit
                    unit.AddTransitTileObject(transitObject);
                }
            }


            /*
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
            */
        }
    }
}