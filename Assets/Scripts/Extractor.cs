using Engine.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Extractor
    {
        public static void TransitOtherPart(UnitBase unit, UnitBasePart otherUnitBasePart)
        {
            GameObject extractedUnitPart = otherUnitBasePart.Part;
            /*
            if (otherUnitBasePart.CompleteLevel > 1)
            {
                extractedUnitPart = UnitBase.FindChildNyName(otherUnitBasePart.Part, otherUnitBasePart.Name + otherUnitBasePart.CompleteLevel + "-" + otherUnitBasePart.Level);
            }*/


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
                TransitObject lastTransitObject = null;
                float delayStart = 0;

                foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
                {
                    GroundCell targetCell;
                    if (!HexGrid.MainGrid.GroundCells.TryGetValue(moveRecipeIngredient.TargetPosition, out targetCell))
                    {
                        throw new Exception("Wrong");
                    }
                    GroundCell sourceCell;
                    if (!HexGrid.MainGrid.GroundCells.TryGetValue(moveRecipeIngredient.SourcePosition, out sourceCell))
                    {
                        throw new Exception("Wrong");
                    }

                    if (otherUnit == null)
                    {
                        lastTransitObject = ExtractFromGroundIntoStructure(moveRecipeIngredient, sourceCell, unit, delayStart);
                    }
                    else
                    {
                        lastTransitObject = ExtractFromStructureToStructure(unit, otherUnit, ref lastTransitObject, ref delayStart, moveRecipeIngredient, sourceCell);
                    }
                    delayStart += 0.01f;
                }
                if (lastTransitObject != null)
                {
                    lastTransitObject.UnitId = unit.UnitId;
                }
            }
        }

        private static TransitObject ExtractFromStructureToStructure(UnitBase unit, UnitBase otherUnit, ref TransitObject lastTransitObject, ref float delayStart, MoveRecipeIngredient moveRecipeIngredient, GroundCell sourceCell)
        {
            Direction direction = Position2.GetDirection(moveRecipeIngredient.SourcePosition, moveRecipeIngredient.TargetPosition);
            Vector3 sourcePosition;
            if (otherUnit.HasEngine())
            {
                sourcePosition = GetTargetPostionInUnit(otherUnit);
            }
            else
            {
                sourcePosition = otherUnit.GetDeliveryPos(direction);
            }

            GameObject transitGameObject = RemoveObjectFromOtherUnit(otherUnit, moveRecipeIngredient, sourceCell, sourcePosition);
            if (transitGameObject == null)
            {
                throw new Exception("Missing source");
            }
            transitGameObject.transform.rotation = UnityEngine.Random.rotation;

            // Transit ingredient
            TransitObject transitObject = new TransitObject();
            transitObject.GameObject = transitGameObject;

            if (unit.HasEngine())
            {
                transitObject.TargetPosition = GetTargetPostionInUnit(unit);
            }
            else
            {
                transitObject.TargetPosition = unit.GetDeliveryPos(direction);
            }

            transitObject.DestroyAtArrival = true;
            transitObject.StartAfterThis = Time.time + (delayStart * HexGrid.MainGrid.GameSpeed);
            delayStart += 0.01f;

            if (!TileObject.IsTileObjectTypeCollectable(moveRecipeIngredient.TileObjectType))
                transitObject.ScaleDown = true;

            HexGrid.MainGrid.AddTransitTileObject(transitObject);
            return transitObject;
        }

        private static GameObject RemoveObjectFromOtherUnit(UnitBase otherUnit, MoveRecipeIngredient moveRecipeIngredient, GroundCell sourceCell, Vector3 sourcePosition)
        {
            GameObject transitGameObject;

            UnitBaseTileObject unitBaseTileObject = otherUnit.RemoveTileObject(moveRecipeIngredient);
            if (unitBaseTileObject == null || unitBaseTileObject.GameObject == null)
            {
                // May happen if the unit extracts something and in the next move, another unit extracts from this unit.
                // The other unit will try to extract, what has been added previouly. But in the client, the container is
                // updated later through the update stats. (where the unit will be empty).

                // For now it's ok, to transit a ghost 
                TileObject tileObject = new TileObject();
                tileObject.TileObjectType = moveRecipeIngredient.TileObjectType;
                tileObject.TileObjectKind = moveRecipeIngredient.TileObjectKind;
                //transitType = moveRecipeIngredient.TileObjectType;
                transitGameObject = HexGrid.MainGrid.CreateDestructable(sourceCell.transform, tileObject, CollectionType.Single);
                transitGameObject.transform.position = sourcePosition;
            }
            else
            {
                transitGameObject = unitBaseTileObject.GameObject;
                //transitType = unitBaseTileObject.TileObject.TileObjectType;
                unitBaseTileObject.GameObject = null;
                transitGameObject.transform.position = sourcePosition;
            }
            return transitGameObject;
        }

        private static GameObject FindIndigrientOnGround (MoveRecipeIngredient moveRecipeIngredient, GroundCell sourceCell)
        {
            GameObject transitGameObject = null;

            foreach (UnitBaseTileObject groundBaseTileObject in sourceCell.GameObjects)
            {
                bool isSame = false;
                if (groundBaseTileObject.TileObject.TileObjectType == moveRecipeIngredient.TileObjectType)
                    isSame = true;
                else
                {
                    if (groundBaseTileObject.TileObject.TileObjectType == TileObjectType.Bush ||
                        groundBaseTileObject.TileObject.TileObjectType == TileObjectType.Tree)
                    {
                        if (moveRecipeIngredient.TileObjectType == TileObjectType.Wood)
                            isSame = true;
                    }
                }
                // Loop until match
                if (isSame)
                {
                    if (groundBaseTileObject.GameObject == null ||
                        groundBaseTileObject.CollectionType != CollectionType.Single)
                    {
                        TileObject tileObject = new TileObject();
                        tileObject.TileObjectType = groundBaseTileObject.TileObject.TileObjectType;
                        tileObject.TileObjectKind = TileObjectKind.None;
                        transitGameObject = HexGrid.MainGrid.CreateDestructable(sourceCell.transform, tileObject, CollectionType.Single);

                        HexGrid.Destroy(groundBaseTileObject.GameObject);
                        groundBaseTileObject.GameObject = null;
                    }
                    else
                    {
                        transitGameObject = groundBaseTileObject.GameObject;
                    }
                    sourceCell.GameObjects.Remove(groundBaseTileObject);
                    break;
                }
            }
            return transitGameObject;
        }

        private static TransitObject ExtractFromGroundIntoStructure(MoveRecipeIngredient moveRecipeIngredient, GroundCell sourceCell, UnitBase unit, float delayStart)
        {
            GameObject transitGameObject = FindIndigrientOnGround(moveRecipeIngredient, sourceCell);
            if (transitGameObject == null)
            {
                throw new Exception("Missing source");
            }
            TransitObject transitObject;

            transitGameObject.transform.rotation = UnityEngine.Random.rotation;

            // Transit ingredient from ground to structure
            transitObject = new TransitObject();
            transitObject.GameObject = transitGameObject;

            if (unit.HasEngine())
            {
                transitObject.TargetPosition = GetTargetPostionInUnit(unit);
            }
            else
            {
                Direction direction = Position2.GetDirection(moveRecipeIngredient.SourcePosition, moveRecipeIngredient.TargetPosition);
                transitObject.TargetPosition = unit.GetDeliveryPos(direction);
            }
            transitObject.DestroyAtArrival = true;
            transitObject.StartAfterThis = Time.time + (delayStart * HexGrid.MainGrid.GameSpeed);

            if (!TileObject.IsTileObjectTypeCollectable(moveRecipeIngredient.TileObjectType))
                transitObject.ScaleDown = true;

            HexGrid.MainGrid.AddTransitTileObject(transitObject);

            return transitObject;
        }

        private static Vector3 GetTargetPostionInUnit(UnitBase unit)
        {
            foreach (UnitBasePart unitBasePart in unit.UnitBaseParts)
            {
                if (unitBasePart.PartType == TileObjectType.PartExtractor)
                {
                    return unitBasePart.Part.transform.position;
                }
            }
            return unit.transform.position;
        }
    }
}
