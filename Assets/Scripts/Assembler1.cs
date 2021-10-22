using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Assembler1 : MonoBehaviour
    {
        public void Assemble(UnitBase unit, UnitBase upgradedUnit, Move move)
        {
            MoveUpdateUnitPart moveUpdateUnitPart = move.Stats.UnitParts[0];
            foreach (UnitBasePart upgradedBasePart in upgradedUnit.UnitBaseParts)
            {
                if (upgradedBasePart.PartType == moveUpdateUnitPart.PartType)
                {
                    TransitObject transitObject = new TransitObject();
                    transitObject.GameObject = upgradedBasePart.Part1;
                    transitObject.TargetPosition = upgradedBasePart.Part1.transform.position;
                    transitObject.TargetRotation = upgradedBasePart.Part1.transform.rotation;

                    // Reset current pos to assembler
                    upgradedBasePart.Part1.transform.position = transform.position;
                    upgradedBasePart.Part1.SetActive(true);

                    // Move to position in unit
                    HexGrid.MainGrid.AddTransitTileObject(transitObject);
                }
            }
        }
    }
}