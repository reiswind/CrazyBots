using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extractor1 : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (moveToContainer != null)
        {
            Vector3 unitPos3 = transform.position;

            float speed = 2.0f / hexGrid.GameSpeed;
            float step = speed * Time.deltaTime;

            moveToContainer.transform.position = Vector3.MoveTowards(moveToContainer.transform.position, transform.position, step);
            if (moveToContainer.transform.position == transform.position)
            {
                moveToContainer.SetActive(false);
                //Destroy(moveToContainer);
                moveToContainer = null;
            }
        }
    }

    private HexGrid hexGrid;
    private GameObject moveToContainer;

    public void Extract(HexGrid hexGrid, Move move, UnitBase unit, UnitBase otherUnit)
    {
        this.hexGrid = hexGrid;

        if (moveToContainer != null)
        {
            moveToContainer.SetActive(false);
            //Destroy(moveToContainer);
            moveToContainer = null;
        }
        bool found = false;

        foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
        {
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
