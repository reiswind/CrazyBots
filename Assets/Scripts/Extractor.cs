using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Extractor
    {
        private static void TransitOtherPart(UnitBase unit, UnitBasePart otherUnitBasePart, TileObject extractedPart)
        {
            GameObject extractedUnitPart = otherUnitBasePart.Part;

            if (otherUnitBasePart.CompleteLevel > 1)
            { 
                extractedUnitPart = UnitBase.FindChildNyName(otherUnitBasePart.Part, otherUnitBasePart.Name + otherUnitBasePart.CompleteLevel + "-" + otherUnitBasePart.Level);           
            }


            if (extractedUnitPart != null)
            {
                // Clone the part
                GameObject part = HexGrid.Instantiate(extractedUnitPart);

                //UnitBase.SetPlayerColor(0, part);
                part.transform.SetParent(unit.transform, false);
                part.transform.position = otherUnitBasePart.Part.transform.position;

                TransitObject transitObject = new TransitObject();
                transitObject.GameObject = part;
                transitObject.TargetPosition = unit.transform.position;
                transitObject.DestroyAtArrival = true;
                transitObject.ScaleDown = true;
                HexGrid.MainGrid.AddTransitTileObject(transitObject);

                extractedUnitPart.SetActive(false);
            }
            /*
            if (otherUnitBasePart.Level == 0)
            {
            }
            else if (extractedPart.TileObjectType == TileObjectType.PartContainer ||
                     extractedPart.TileObjectType == TileObjectType.PartReactor)
            {
                string name = otherUnitBasePart.Name + "1";
                GameObject newPart = HexGrid.MainGrid.InstantiatePrefab(name);
                newPart.transform.position = otherUnitBasePart.Part.transform.position;
                newPart.transform.SetParent(unit.transform);
                newPart.name = name;
                UnitBase.SetPlayerColor(otherUnitBasePart.UnitBase.PlayerId, newPart);

                // Transit one container part
                TransitObject transitObject = new TransitObject();
                transitObject.GameObject = newPart;
                transitObject.TargetPosition = unit.transform.position;
                transitObject.DestroyAtArrival = true;
                transitObject.ScaleDown = true;
                HexGrid.MainGrid.AddTransitTileObject(transitObject);
            }*/
        }

        public static void Extract(Move move, UnitBase unit, UnitBase otherUnit)
        {
            if (move.MoveRecipe != null && move.MoveRecipe.Ingredients.Count > 0)
            {
                foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
                {
                    // Transit the ingredient into the weapon. This is the reloaded ammo. (Can be empty)
                    UnitBaseTileObject unitBaseTileObject;
                    unitBaseTileObject = otherUnit.RemoveTileObject(moveRecipeIngredient);
                    if (unitBaseTileObject != null)
                    {
                        // Transit ingredient
                        TransitObject transitObject = new TransitObject();
                        transitObject.GameObject = unitBaseTileObject.GameObject;
                        transitObject.TargetPosition = unit.transform.position;
                        transitObject.DestroyAtArrival = true;

                        unitBaseTileObject.GameObject = null;
                        HexGrid.MainGrid.AddTransitTileObject(transitObject);
                    }
                }
                return;
            }

            bool found;


            // Find the extracted tileobjects
            foreach (TileObject tileObject in move.Stats.MoveUpdateGroundStat.TileObjects)
            {
                found = false;

                if (otherUnit == null)
                {
                    Position2 from = move.Positions[1];

                    GroundCell sourceCell; // = hexGrid.GroundCells[from];

                    if (HexGrid.MainGrid.GroundCells.TryGetValue(from, out sourceCell))
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
                                    unitBaseTileObject.GameObject = HexGrid.MainGrid.CreateDestructable(sourceCell.transform, unitBaseTileObject.TileObject);
                                }
                                else if (tileObject.TileObjectType == TileObjectType.Bush)
                                {
                                    transitGameObject = unitBaseTileObject.GameObject;

                                    unitBaseTileObject.TileObject.TileObjectType = TileObjectType.Gras;
                                    unitBaseTileObject.GameObject = HexGrid.MainGrid.CreateDestructable(sourceCell.transform, unitBaseTileObject.TileObject);
                                }
                                else
                                {
                                    transitGameObject = unitBaseTileObject.GameObject;
                                    sourceCell.GameObjects.Remove(unitBaseTileObject);
                                }
                                if (transitGameObject != null)
                                {
                                    transitGameObject.transform.SetParent(unit.transform, true);

                                    TransitObject transitObject = new TransitObject();
                                    transitObject.GameObject = transitGameObject;
                                    transitObject.TargetPosition = unit.transform.position;
                                    transitObject.HideAtArrival = true;

                                    if (tileObject.TileObjectType == TileObjectType.Mineral &&
                                        tileObject.TileObjectKind != TileObjectKind.None)
                                    {
                                        int xx = 0;
                                    }
                                    if (tileObject.TileObjectType != TileObjectType.Mineral)
                                        transitObject.ScaleDown = true;

                                    HexGrid.MainGrid.AddTransitTileObject(transitObject);
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
                                TransitOtherPart(unit, otherUnitBasePart, tileObject);

                                /*
                                if (otherUnitBasePart.Level == 0)
                                {
                                    TransitObject transitObject = new TransitObject();
                                    transitObject.GameObject = otherUnitBasePart.Part;
                                    transitObject.TargetPosition = transform.position;
                                    transitObject.DestroyAtArrival = true;
                                    transitObject.ScaleDown = true;
                                    hexGrid.AddTransitTileObject(transitObject);
                                }
                                */
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
                                            /*if (unit.Container != null) ???
                                                sourceTileObject.GameObject = HexGrid.MainGrid.CreateTileObject(unit.Container.transform, tileObject);
                                            else*/
                                                sourceTileObject.GameObject = HexGrid.MainGrid.CreateTileObject(unit.transform, tileObject);

                                            sourceTileObject.GameObject.name = "xx" + tileObject.TileObjectType.ToString() + " to " + otherUnit.UnitId;
                                        }
                                        else
                                        {
                                            sourceTileObject.GameObject.SetActive(true);
                                            sourceTileObject.GameObject.transform.SetParent(unit.transform, true);
                                        }

                                        Vector2 randomPos = Random.insideUnitCircle;
                                        Vector3 unitPos3 = otherUnitBasePart.Part.transform.position;
                                        unitPos3.x += (randomPos.x * 0.5f);
                                        unitPos3.z += (randomPos.y * 0.7f);
                                        sourceTileObject.GameObject.transform.position = unitPos3;

                                        TransitObject transitObject = new TransitObject();
                                        transitObject.GameObject = sourceTileObject.GameObject;
                                        transitObject.TargetPosition = unit.transform.position;
                                        transitObject.HideAtArrival = true;
                                        HexGrid.MainGrid.AddTransitTileObject(transitObject);

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
                                if (TileObject.CanConvertTileObjectIntoMineral(tileObject.TileObjectType))
                                {
                                    if (otherUnitBasePart.PartType == tileObject.TileObjectType)
                                    {
                                        TransitOtherPart(unit, otherUnitBasePart, tileObject);
                                        found = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    foreach (UnitBaseTileObject otherTileObject in otherUnitBasePart.TileObjectContainer.TileObjects)
                                    {
                                        if (otherTileObject.TileObject.TileObjectType == tileObject.TileObjectType)
                                        {
                                            TransitObject transitObject = new TransitObject();
                                            transitObject.GameObject = otherUnitBasePart.Part;
                                            transitObject.TargetPosition = unit.transform.position;
                                            //transitObject.DestroyAtArrival = true;
                                            //transitObject.ScaleDown = true;
                                            HexGrid.MainGrid.AddTransitTileObject(transitObject);

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
