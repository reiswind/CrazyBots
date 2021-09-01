using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Extractor1 : MonoBehaviour
    {
        public void Extract(HexGrid hexGrid, Move move, UnitBase unit, UnitBase otherUnit)
        {
            bool found;

            if (move.Stats.MoveUpdateGroundStat.TileObjects.Count == 12)
            {
                int x = 0;
            }

            // Find the extracted tileobjects
            foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
            {
                found = false;

                if (otherUnit == null)
                {
                    Position from = move.Positions[1];
                    GroundCell sourceCell = hexGrid.GroundCells[from];

                    found = false;
                    foreach (UnitBaseTileObject unitBaseTileObject in sourceCell.GameObjects)
                    {
                        if (unitBaseTileObject.TileObject.TileObjectType == tileObject.TileObjectType)
                        {
                            sourceCell.GameObjects.Remove(unitBaseTileObject);
                            unitBaseTileObject.GameObject.transform.SetParent(transform, true);

                            TransitObject transitObject = new TransitObject();
                            transitObject.GameObject = unitBaseTileObject.GameObject;
                            transitObject.TargetPosition = transform.position;
                            transitObject.HideAtArrival = true;

                            unit.AddTransitTileObject(transitObject);
                            unit.InsertGameTileObject(unitBaseTileObject);

                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // Bug!
                        int x = 0;
                    }
                }
                else
                {
                    if (unit.PlayerId == otherUnit.PlayerId)
                    {
                        // Find source tile in friendly otherUnit
                        foreach (UnitBasePart otherUnitBasePart in otherUnit.UnitBaseParts)
                        {
                            if (otherUnitBasePart.PartType == tileObject.TileObjectType)
                            {
                                // Extract from friendly unit
                                otherUnit.PartExtracted(tileObject.TileObjectType);

                                if (otherUnitBasePart.Level == 0)
                                {
                                    TransitObject transitObject = new TransitObject();
                                    transitObject.GameObject = otherUnitBasePart.Part;
                                    transitObject.TargetPosition = transform.position;
                                    transitObject.DestroyAtArrival = true;
                                    unit.AddTransitTileObject(transitObject);

                                    /*
                                    GameObject otherPart;
                                    otherPart = otherUnitBasePart.Part;
                                    otherPart.transform.SetParent(unit.transform, true);
                                    unit.AddTransitTileObject(otherPart);*/
                                }
                                found = true;
                                break;
                            }
                            if (otherUnitBasePart.TileObjectContainer != null)
                            {
                                foreach (UnitBaseTileObject sourceTileObject in otherUnitBasePart.TileObjectContainer.TileObjects)
                                {
                                    /*
                                    if (otherUnitBasePart.TileObjects != null)
                                    {
                                        foreach (UnitBaseTileObject otherTileObject in otherUnitBasePart.TileObjects)
                                        {
                                            if (otherTileObject.TileObject.TileObjectType == tileObject.TileObjectType)
                                            {
                                                /*
                                                TransitObject transitObject = new TransitObject();
                                                transitObject.GameObject = otherTileObject.Part;
                                                transitObject.TargetPosition = transform.position;
                                                transitObject.DestroyAtArrival = true;
                                                unit.AddTransitTileObject(transitObject);* /

                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found)
                                            break;
                                    }*/

                                    if (tileObject.TileObjectType == sourceTileObject.TileObject.TileObjectType)
                                    {
                                        otherUnitBasePart.TileObjectContainer.Remove(sourceTileObject);

                                        if (sourceTileObject.GameObject == null)
                                        {
                                            if (unit.Container != null)
                                                sourceTileObject.GameObject = hexGrid.CreateTileObject(unit.Container.transform, tileObject);
                                            else
                                                sourceTileObject.GameObject = hexGrid.CreateTileObject(unit.transform, tileObject);

                                            sourceTileObject.GameObject.name = "xx" + tileObject.TileObjectType.ToString() + " to " + otherUnit.UnitId;
                                        }
                                        else
                                        {
                                            sourceTileObject.GameObject.SetActive(true);
                                            sourceTileObject.GameObject.transform.SetParent(transform, true);
                                        }
                                        unit.InsertGameTileObject(sourceTileObject);
                                        
                                        Vector2 randomPos = Random.insideUnitCircle;
                                        Vector3 unitPos3 = otherUnitBasePart.Part.transform.position;
                                        unitPos3.x += (randomPos.x * 0.5f);
                                        unitPos3.z += (randomPos.y * 0.7f);
                                        sourceTileObject.GameObject.transform.position = unitPos3;

                                        TransitObject transitObject = new TransitObject();
                                        transitObject.GameObject = sourceTileObject.GameObject;
                                        transitObject.TargetPosition = transform.position;
                                        transitObject.HideAtArrival = true;
                                        unit.AddTransitTileObject(transitObject);

                                        found = true;
                                        break;
                                    }
                                }
                            }
                            if (found)
                                break;
                        }
                    }
                    else
                    {
                        // Extract Part in enemy otherUnit
                        // Find source tile in friendly otherUnit
                        foreach (UnitBasePart otherUnitBasePart in otherUnit.UnitBaseParts)
                        {
                            if (otherUnitBasePart.TileObjectContainer != null)
                            {
                                foreach (UnitBaseTileObject otherTileObject in otherUnitBasePart.TileObjectContainer.TileObjects)
                                {
                                    if (otherTileObject.TileObject.TileObjectType == tileObject.TileObjectType)
                                    {
                                        /*
                                        TransitObject transitObject = new TransitObject();
                                        transitObject.GameObject = otherTileObject.Part;
                                        transitObject.TargetPosition = transform.position;
                                        transitObject.DestroyAtArrival = true;
                                        unit.AddTransitTileObject(transitObject);*/

                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                    break;
                            }
                            if (otherUnitBasePart.PartType == tileObject.TileObjectType)
                            {
                                otherUnit.PartExtracted(tileObject.TileObjectType);

                                if (otherUnitBasePart.Level == 0)
                                {
                                    /*
                                    GameObject otherPart;
                                    otherPart = otherUnitBasePart.Part;
                                    otherPart.transform.SetParent(unit.transform, true);
                                    unit.AddTransitTileObject(otherPart);
                                    */
                                    TransitObject transitObject = new TransitObject();
                                    transitObject.GameObject = otherUnitBasePart.Part;
                                    transitObject.TargetPosition = transform.position;
                                    transitObject.DestroyAtArrival = true;
                                    unit.AddTransitTileObject(transitObject);
                                }
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found)
                {
                    // Bug!
                    int x = 0;
                }
                /*
                // Remove tileobject from the other unit
                foreach (UnitBasePart unitBasePart in unit.UnitBaseParts)
                    {
                        if (unitBasePart.TileObjects != null)
                        {
                            foreach (UnitBaseTileObject targetTileObject in unitBasePart.TileObjects)
                            {
                                if (tileObject.TileObjectType == targetTileObject.TileObject.TileObjectType &&
                                    targetTileObject.GameObject == null)
                                {
                                    // This is the target UnitBaseTileObject
                                    if (otherUnit == null)
                                    {
                                        Position from = move.Positions[1];
                                        GroundCell sourceCell = hexGrid.GroundCells[from];

                                        foreach (GameObject gameTileObject in sourceCell.TileContainer)
                                        {
                                            if (gameTileObject.name == move.OtherUnitId)
                                            {
                                                sourceCell.TileContainer.Remove(gameTileObject);
                                                targetTileObject.GameObject = gameTileObject;
                                                break;
                                            }
                                        }
                                        if (targetTileObject.GameObject == null)
                                        {
                                            int x = 0;
                                        }
                                        else
                                        {
                                            found = true;
                                            moveToContainer = targetTileObject.GameObject;
                                            moveToContainer.transform.SetParent(unit.transform, true);
                                        }
                                    }
                                    else
                                    {
                                        // Find source tile in otherUmit
                                        foreach (UnitBasePart otherUnitBasePart in otherUnit.UnitBaseParts)
                                        {
                                            if (otherUnitBasePart.TileObjects != null)
                                            {
                                                foreach (UnitBaseTileObject sourceTileObject in otherUnitBasePart.TileObjects)
                                                {
                                                    if (tileObject.TileObjectType == sourceTileObject.TileObject.TileObjectType &&
                                                        sourceTileObject.GameObject != null)
                                                    {
                                                        moveToContainer = targetTileObject.GameObject;
                                                        moveToContainer.transform.SetParent(unit.transform, true);
                                                        moveToContainer.SetActive(true);

                                                        found = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }


                            }
                            if (found)
                                break;
                        }
                        else
                        {
                            int nottileobn = 0;
                        }
                    }
                    if (!found)
                    {
                        int vvveyNotFoundException = 0;
                    }
                }*/
            }
            /*
        if (otherUnit == null)
        {
            Position from = move.Positions[1];
            GroundCell sourceCell = hexGrid.GroundCells[from];

            foreach (GameObject gameTileObject in sourceCell.TileContainer)
            {
                if (gameTileObject.name == move.OtherUnitId)
                {
                    sourceCell.TileContainer.Remove(gameTileObject);
                    moveToContainer = gameTileObject;
                    break;
                }
            }
            if (moveToContainer == null)
            {
                int x = 0;
            }
            else
            {
                Position to = move.Positions[0];
                GroundCell targetCell = hexGrid.GroundCells[to];

                moveToContainer.transform.SetParent(unit.transform, true);

                // Extract this one
                //Destroy(gameTileObject);
            }
        }
        else
        {
            foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
            {
                foreach (UnitBasePart unitBasePart in otherUnit.UnitBaseParts)
                {
                    if (unitBasePart.TileObjects != null)
                    {
                        foreach (UnitBaseTileObject otherTileObject in unitBasePart.TileObjects)
                        {
                            if (tileObject.TileObjectType == otherTileObject.TileObject.TileObjectType)
                            {
                                int x7 = 0;
                            }
                        }
                    }
                }

                break;
            }

            int x = 0;*/


            /*
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
            */
            /*
            if (otherUnit != null)
            {
                otherUnit.PartExtracted();
            }*/
            //particleSource.Play();
        }
    }
}
