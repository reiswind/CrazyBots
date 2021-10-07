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


            // Find the extracted tileobjects
            foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
            {
                found = false;

                if (otherUnit == null)
                {
                    Position from = move.Positions[1];

                    GroundCell sourceCell; // = hexGrid.GroundCells[from];

                    if (hexGrid.GroundCells.TryGetValue(from, out sourceCell))
                    {
                        found = false;
                        foreach (UnitBaseTileObject unitBaseTileObject in sourceCell.GameObjects)
                        {
                            if (unitBaseTileObject.TileObject.TileObjectType == tileObject.TileObjectType)
                            {
                                GameObject transitGameObject = null;
                                if (tileObject.TileObjectType == TileObjectType.Tree)
                                {
                                    transitGameObject = unitBaseTileObject.GameObject;

                                    unitBaseTileObject.TileObject.TileObjectType = TileObjectType.TreeTrunk;
                                    unitBaseTileObject.GameObject = hexGrid.CreateDestructable(sourceCell.transform, unitBaseTileObject.TileObject);
                                }
                                else if (tileObject.TileObjectType == TileObjectType.Bush)
                                {
                                    transitGameObject = unitBaseTileObject.GameObject;

                                    unitBaseTileObject.TileObject.TileObjectType = TileObjectType.Gras;
                                    unitBaseTileObject.GameObject = hexGrid.CreateDestructable(sourceCell.transform, unitBaseTileObject.TileObject);
                                }
                                else
                                {
                                    transitGameObject = unitBaseTileObject.GameObject;
                                    sourceCell.GameObjects.Remove(unitBaseTileObject);
                                }
                                if (transitGameObject != null)
                                {
                                    transitGameObject.transform.SetParent(transform, true);

                                    TransitObject transitObject = new TransitObject();
                                    transitObject.GameObject = transitGameObject;
                                    transitObject.TargetPosition = transform.position;
                                    transitObject.HideAtArrival = true;
                                    transitObject.ScaleDown = true;

                                    hexGrid.AddTransitTileObject(transitObject);
                                }

                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            // Bug!
                            //int x = 0;
                        }
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
                                    transitObject.ScaleDown = true;
                                    hexGrid.AddTransitTileObject(transitObject);
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
                                        //unit.InsertGameTileObject(sourceTileObject);
                                        
                                        Vector2 randomPos = Random.insideUnitCircle;
                                        Vector3 unitPos3 = otherUnitBasePart.Part.transform.position;
                                        unitPos3.x += (randomPos.x * 0.5f);
                                        unitPos3.z += (randomPos.y * 0.7f);
                                        sourceTileObject.GameObject.transform.position = unitPos3;

                                        TransitObject transitObject = new TransitObject();
                                        transitObject.GameObject = sourceTileObject.GameObject;
                                        transitObject.TargetPosition = transform.position;
                                        transitObject.HideAtArrival = true;
                                        hexGrid.AddTransitTileObject(transitObject);

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
                                        TransitObject transitObject = new TransitObject();
                                        transitObject.GameObject = otherUnitBasePart.Part;
                                        transitObject.TargetPosition = transform.position;
                                        //transitObject.DestroyAtArrival = true;
                                        //transitObject.ScaleDown = true;
                                        hexGrid.AddTransitTileObject(transitObject);

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
                                    TransitObject transitObject = new TransitObject();
                                    transitObject.GameObject = otherUnitBasePart.Part;
                                    transitObject.TargetPosition = transform.position;
                                    transitObject.DestroyAtArrival = true;
                                    transitObject.ScaleDown = true;
                                    hexGrid.AddTransitTileObject(transitObject);
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
                    //int x = 0;
                }
            }
        }
    }
}
