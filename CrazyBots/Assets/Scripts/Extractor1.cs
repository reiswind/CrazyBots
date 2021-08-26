using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extractor1 : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (tileObjectsInTransit != null)
        {
            List<GameObject> transit = new List<GameObject>();
            transit.AddRange(tileObjectsInTransit);

            foreach (GameObject gameTileObject in transit)
            {
                if (gameTileObject == null)
                {
                    tileObjectsInTransit.Remove(gameTileObject);
                }
                else
                {
                    Vector3 vector3 = transform.position;
                    //vector3.y = 2;

                    float speed = 1.5f / hexGrid.GameSpeed;
                    float step = speed * Time.deltaTime;

                    gameTileObject.transform.position = Vector3.MoveTowards(gameTileObject.transform.position, vector3, step);
                    if (gameTileObject.transform.position == transform.position)
                    {
                        Destroy(gameTileObject.gameObject);
                        tileObjectsInTransit.Remove(gameTileObject);
                    }
                }
            }
            if (tileObjectsInTransit.Count == 0)
                tileObjectsInTransit = null;
        }
    }

    private HexGrid hexGrid;
    private List<GameObject> tileObjectsInTransit;

    public void Extract(HexGrid hexGrid, Move move, UnitBase unit, UnitBase otherUnit)
    {
        this.hexGrid = hexGrid;

        /*
        if (tileObjectsInTranst != null)
        {
            foreach (GameObject gameTileObject in tileObjectsInTranst)
            {
                Destroy(gameTileObject);
            }
            tileObjectsInTranst = null;
        }*/

        bool found;

        if (tileObjectsInTransit == null)
            tileObjectsInTransit = new List<GameObject>();

        if (move.Stats.MoveUpdateGroundStat.TileObjects.Count > 1)
        {
            int sfd = 0;
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
                foreach (GameObject gameTileObject in sourceCell.GameObjects)
                {
                    if (gameTileObject.name == move.OtherUnitId)
                    {
                        sourceCell.GameObjects.Remove(gameTileObject);
                        gameTileObject.transform.SetParent(unit.transform, true);
                        tileObjectsInTransit.Add(gameTileObject);

                        //GameObject destructable;
                        //destructable = hexGrid.CreateDestructable(transform, tileObject);
                        //tileObjectsInTranst.Add(destructable);
                        //destructable.transform.SetParent(unit.transform, true);

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
                                GameObject otherPart;
                                otherPart = otherUnitBasePart.Part;
                                otherPart.transform.SetParent(unit.transform, true);
                                tileObjectsInTransit.Add(otherPart);
                            }
                            found = true;
                            break;
                        }
                        if (otherUnitBasePart.TileObjects != null)
                        {
                            foreach (UnitBaseTileObject sourceTileObject in otherUnitBasePart.TileObjects)
                            {
                                if (tileObject.TileObjectType == sourceTileObject.TileObject.TileObjectType)
                                {
                                    otherUnitBasePart.TileObjects.Remove(sourceTileObject);

                                    GameObject destructable;

                                    //TileObject tileObject1 = new TileObject();
                                    //tileObject1.TileObjectType = TileObjectType.Tree;
                                    if (unit.Container != null)
                                        destructable = hexGrid.CreateTileObject(unit.Container.transform, tileObject);
                                    else
                                        destructable = hexGrid.CreateTileObject(unit.transform, tileObject);

                                    destructable.name = "xx" + tileObject.TileObjectType.ToString() + " to " + otherUnit.UnitId;

                                    Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
                                    Vector3 unitPos3 = otherUnitBasePart.Part.transform.position;
                                    unitPos3.x += (randomPos.x * 0.5f);
                                    unitPos3.z += (randomPos.y * 0.7f);
                                    //unitPos3.y += 1;
                                    destructable.transform.position = unitPos3;

                                    tileObjectsInTransit.Add(destructable);

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
                        if (otherUnitBasePart.PartType == tileObject.TileObjectType)
                        {
                            otherUnit.PartExtracted(tileObject.TileObjectType);

                            if (otherUnitBasePart.Level == 0)
                            {
                                GameObject otherPart;
                                otherPart = otherUnitBasePart.Part;
                                otherPart.transform.SetParent(unit.transform, true);
                                tileObjectsInTransit.Add(otherPart);
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
