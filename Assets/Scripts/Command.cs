using Engine.Interface;
using HighlightPlus;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{


    public class Command : MonoBehaviour
    {
        public CommandPreview CommandPreview { get; set; }

        public void Awake()
        {
            highlightEffect = GetComponent<HighlightEffect>();
        }

        private HighlightEffect highlightEffect { get; set; }
        private List<UnitBase> highlightedUnits = new List<UnitBase>();
        private Dictionary<Position2, GroundCell> highlightedGroundCells = new Dictionary<Position2, GroundCell>();

        internal void SetHighlighted(bool isHighlighted)
        {
            if (IsHighlighted != isHighlighted)
            {

                IsHighlighted = isHighlighted;
                if (highlightEffect)
                {
                    if (isHighlighted)
                        highlightEffect.outlineColor = Color.yellow;
                    else
                        highlightEffect.outlineColor = UnitBase.GetPlayerColor(CommandPreview.GameCommand.PlayerId);
                }

                if (!IsHighlighted)
                {
                    foreach (GroundCell groundCell in highlightedGroundCells.Values)
                    {
                        groundCell.SetHighlighted(false);
                    }
                    highlightedGroundCells.Clear();

                    foreach (UnitBase unitBase in highlightedUnits)
                    {
                        unitBase.SetHighlighted(false);
                    }
                    highlightedUnits.Clear();
                }
            }
        }

        public bool IsHighlighted { get; private set; }
        public bool IsSelected { get; private set; }

        private void OnDestroy()
        {
            foreach (GameObject line in lineRendererList)
            {
                Destroy(line);
            }
            lineRendererList.Clear();
            SetHighlighted(false);
        }

        public void SetSelected(bool value)
        {
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (value != IsSelected)
                {
                    IsSelected = value;
                    if (value)
                    {
                        //UpdateAttachedUnits();
                        //meshRenderer.material.SetFloat("Darkness", 3.0f);
                    }
                    else
                    {
                        //RemoveAttachedUnits();
                        //meshRenderer.material.SetFloat("Darkness", 1.0f);
                    }
                }
            }
        }

        private Dictionary<UnitBase, CommandAttachedItem> selectedCommandUnits = new Dictionary<UnitBase, CommandAttachedItem>();

        private void RemoveAttachedUnits()
        {
            foreach (CommandAttachedItem commandAttachedUnit in selectedCommandUnits.Values)
            {
                if (commandAttachedUnit.AttachedUnit.Line != null)
                {
                    Destroy(commandAttachedUnit.AttachedUnit.Line);
                    commandAttachedUnit.AttachedUnit.Line = null;
                }
                commandAttachedUnit.AttachedUnit.GhostUnit.SetHighlighted(false);
            }
        }

        public void UpdateDirection(Vector3 position)
        {
            // Determine which direction to rotate towards
            Vector3 targetDirection = position - transform.position;

            Vector3 forward = transform.forward;
            Vector3 newDirection = Vector3.RotateTowards(forward, targetDirection, 7, 7);
            newDirection.y = 0;

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        private List<GameObject> lineRendererList = new List<GameObject>();


        void UpdateArrow(LineRenderer cachedLineRenderer, Vector3 from, Vector3 to)
        {
            float PercentHead = 0.4f;
            Vector3 ArrowOrigin = from;
            Vector3 ArrowTarget = to;

            cachedLineRenderer.widthCurve = new AnimationCurve(
                new Keyframe(0, 0.4f)
                , new Keyframe(0.999f - PercentHead, 0.4f)  // neck of arrow
                , new Keyframe(1 - PercentHead, 1f)  // max width of arrow head
                , new Keyframe(1, 0f));  // tip of arrow
            cachedLineRenderer.SetPositions(new Vector3[] {
              ArrowOrigin
              , Vector3.Lerp(ArrowOrigin, ArrowTarget, 0.999f - PercentHead)
              , Vector3.Lerp(ArrowOrigin, ArrowTarget, 1 - PercentHead)
              , ArrowTarget });
        }

        private void DrawLine(Vector3 from, Vector3 to)
        {
            GameObject lineRendererObject = new GameObject();
            lineRendererObject.name = "UnitLine";

            LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
            lineRenderer.transform.SetParent(HexGrid.MainGrid.transform, false);
            lineRenderer.material = HexGrid.MainGrid.GetMaterial("Player1");
            
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, from); // transform.position);
            lineRenderer.SetPosition(1, to); // unit.position);

            lineRendererList.Add(lineRendererObject);
        }

        private void Update()
        {
            if (CommandPreview == null)
                return;

            foreach (GameObject line in lineRendererList)
            {
                Destroy(line);
            }
            lineRendererList.Clear();

            Position3 targetPosition3;
            if (CommandPreview.IsMoveMode)
            {
                targetPosition3 = new Position3(CommandPreview.DisplayPosition);
            }
            else
            {
                targetPosition3 = new Position3(CommandPreview.GameCommand.TargetPosition);
            }
            if (IsSelected)
            {
                foreach (MapGameCommandItem mapGameCommandItem in CommandPreview.GameCommand.GameCommandItems)
                {
                    UnitBase attachedUnit = null;
                    if (mapGameCommandItem.AttachedUnit?.UnitId != null)
                    {
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.AttachedUnit.UnitId, out attachedUnit))
                        {
                            if (CommandPreview.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                            {
                                // Draws a line to the source container...good?
                            }
                            else
                            {
                                DrawLine(transform.position, attachedUnit.transform.position);
                            }
                        }
                    }

                    UnitBase factoryUnit = null;
                    if (mapGameCommandItem.FactoryUnit?.UnitId != null)
                    {
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.FactoryUnit.UnitId, out factoryUnit))
                        {
                            DrawLine(transform.position, factoryUnit.transform.position);
                        }
                    }

                    UnitBase targetUnit = null;
                    if (mapGameCommandItem.TargetUnit?.UnitId != null)
                    {
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.TargetUnit.UnitId, out targetUnit))
                        {
                            DrawLine(transform.position, targetUnit.transform.position);
                        }
                    }
                    UnitBase transportUnit = null;
                    if (mapGameCommandItem.TransportUnit?.UnitId != null)
                    {
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.TransportUnit.UnitId, out transportUnit))
                        {
                            if (attachedUnit != null)
                                DrawLine(attachedUnit.transform.position, transportUnit.transform.position);
                            if (targetUnit != null)
                                DrawLine(targetUnit.transform.position, transportUnit.transform.position);
                        }
                    }
                }
            }

            //bool showAlert = false;

            // Display Ghost?
            foreach (CommandAttachedItem commandAttachedUnit in CommandPreview.PreviewUnits)
            {
                /*
                if (commandAttachedUnit.MapGameCommandItem.Alert)
                {
                    showAlert = true;
                }*/
                if (string.IsNullOrEmpty(commandAttachedUnit.MapGameCommandItem.AttachedUnit.UnitId))
                {
                    // Display Ghost, unit does not exist
                    if (commandAttachedUnit.AttachedUnit.IsVisible)
                    {

                    }
                    else
                    {
                        //Debug.Log("Activate: commandAttachedUnit.UnitBase");
                        // Real unit missing, show ghost
                        commandAttachedUnit.AttachedUnit.IsVisible = true;
                        if (commandAttachedUnit.AttachedUnit.GhostUnit != null)
                            commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = true;
                        if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                            commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = true;
                    }
                }
                else
                {
                    UnitBase realUnit;
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(commandAttachedUnit.MapGameCommandItem.AttachedUnit.UnitId, out realUnit))
                    {
                        if (commandAttachedUnit.AttachedUnit.IsVisible)
                        {
                            if (CommandPreview.GameCommand.GameCommandType == GameCommandType.Build)
                            {
                                // Build: Ghost stays until completion
                            }
                            else
                            {
                                // Unit exists
                                if (realUnit.UnderConstruction)
                                {
                                    // Wait for completion (Keep Ghost)
                                    //commandAttachedUnit.Marker.SetActive(true);
                                }
                                else
                                {
                                    // Unit complete, hide ghost
                                    //Debug.Log("Deactivate: commandAttachedUnit.UnitBase");

                                    // Real unit exists, deactivate ghost
                                    if (commandAttachedUnit.AttachedUnit.GhostUnit != null)
                                        commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = false;
                                }
                            }
                        }
                        if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                        {
                            Position3 relativePosition3 = targetPosition3.Add(commandAttachedUnit.AttachedUnit.RotatedPosition3);
                            if (IsSelected)
                            {
                                commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = true;
                            }
                            else
                            {
                                if (realUnit.CurrentPos == relativePosition3.Pos)
                                {
                                    commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = false;
                                }
                                else
                                {
                                    commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = true;
                                }
                            }
                        }
                        realUnit.SetHighlighted(IsHighlighted);
                    }
                    else
                    {
                        if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                        {
                            if (IsSelected)
                            {
                                commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = true;
                            }
                            else
                            {
                                commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = true;
                            }
                        }
                    }
                }
            }

            //CommandPreview.ShowAlert(showAlert);
            

            if (IsHighlighted)
            {
                List<Position2> remainingPos = new List<Position2>();
                remainingPos.AddRange(highlightedGroundCells.Keys);

                Position2 center;
                if (CommandPreview.IsPreview || CommandPreview.IsInSubCommandMode)
                    center = CommandPreview.DisplayPosition;
                else
                    center = CommandPreview.GameCommand.TargetPosition;
                if (center != Position2.Null)
                {
                    Position3 centerPosition3 = new Position3(center);

                    if (CommandPreview.GameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        //if (CommandPreview.CollectBounds == null)
                        //    CommandPreview.UpdateCommandPreview();
                        /*
                        List<Position3> groundPositions = centerPosition3.GetNeighbors(CommandPreview.GameCommand.Radius);
                        foreach (Position3 position3 in groundPositions)
                        {
                            Position2 position2 = position3.Pos;
                            GroundCell gc;

                            if (highlightedGroundCells.TryGetValue(position2, out gc))
                            {
                                if (gc.NumberOfCollectables == 0)
                                {
                                    gc.SetHighlighted(false);
                                }
                                else
                                {
                                    remainingPos.Remove(position2);
                                }
                            }
                            else
                            {

                                if (HexGrid.MainGrid.GroundCells.TryGetValue(position2, out gc))
                                {
                                    if (gc.NumberOfCollectables > 0)
                                    {
                                        gc.SetHighlighted(IsHighlighted);
                                        highlightedGroundCells.Add(gc.Pos, gc);
                                        remainingPos.Remove(position2);
                                    }
                                }
                                else
                                {

                                }
                            }
                        }*/
                    }
                }

                List<UnitBase> remainHighlighted = new List<UnitBase>();
                remainHighlighted.AddRange(highlightedUnits);

                if (CommandPreview.IsMoveMode)
                {
                    if (CommandPreview.GameCommand.GameCommandType != GameCommandType.Collect)
                    {
                        foreach (CommandAttachedItem commandAttachedItem in CommandPreview.PreviewUnits)
                        {
                            Position3 relativePosition3 = targetPosition3.Add(commandAttachedItem.AttachedUnit.RotatedPosition3);
                            Position2 position2 = relativePosition3.Pos;
                            GroundCell gc;
                            if (HexGrid.MainGrid.GroundCells.TryGetValue(position2, out gc))
                            {
                                Vector3 unitPos3 = gc.transform.position;
                                unitPos3.y += 0.10f;
                                //commandAttachedUnit.Marker.transform.position = unitPos3;
                                if (commandAttachedItem.AttachedUnit.GhostUnitBounds != null)
                                    commandAttachedItem.AttachedUnit.GhostUnitBounds.Update();
                                if (commandAttachedItem.AttachedUnit.GhostUnit != null)
                                    commandAttachedItem.AttachedUnit.GhostUnit.IsVisible = true;
                                commandAttachedItem.AttachedUnit.IsVisible = true;

                                //remainingPos.Remove(position2);
                            }
                            else
                            {
                                //gc.SetHighlighted(IsHighlighted);
                                //highlightedGroundCells.Add(gc.Pos, gc);
                                //remainingPos.Remove(position2);
                            }
                        }
                    }
                }
                else
                {

                    // Highlight attached units
                    if (CommandPreview.GameCommand.GameCommandType == GameCommandType.Collect ||
                        CommandPreview.GameCommand.GameCommandType == GameCommandType.Build ||
                        CommandPreview.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                    {
                        foreach (MapGameCommandItem mapGameCommandItem in CommandPreview.GameCommand.GameCommandItems)
                        {
                            //break;
                            /*
                            if (CommandPreview.GameCommand.GameCommandType != GameCommandType.Collect)
                            {
                                Position3 relativePosition3 = targetPosition3.Add(mapGameCommandItem.BlueprintCommandItem.Position3);

                                Position2 position2 = relativePosition3.Pos;
                                GroundCell gc;
                                if (highlightedGroundCells.TryGetValue(position2, out gc))
                                {
                                    remainingPos.Remove(position2);
                                }
                                else
                                {
                                    gc = HexGrid.MainGrid.GroundCells[position2];
                                    gc.SetHighlighted(IsHighlighted);
                                    highlightedGroundCells.Add(gc.Pos, gc);
                                    remainingPos.Remove(position2);
                                }
                            }
                            */
                            if (!string.IsNullOrEmpty(mapGameCommandItem.AttachedUnit.UnitId))
                            {
                                UnitBase unitBase;
                                if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.AttachedUnit.UnitId, out unitBase))
                                {
                                    
                                    if (!highlightedUnits.Contains(unitBase))
                                        highlightedUnits.Add(unitBase);
                                    else
                                        remainHighlighted.Remove(unitBase);
                                    
                                    unitBase.SetHighlighted(IsHighlighted);
                                }
                            }
                            if (!string.IsNullOrEmpty(mapGameCommandItem.FactoryUnit.UnitId))
                            {
                                UnitBase unitBase;
                                if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.FactoryUnit.UnitId, out unitBase))
                                {                                    
                                    if (!highlightedUnits.Contains(unitBase))
                                        highlightedUnits.Add(unitBase);
                                    else
                                        remainHighlighted.Remove(unitBase);
                                    unitBase.SetHighlighted(IsHighlighted);
                                }
                            }
                            if (!string.IsNullOrEmpty(mapGameCommandItem.TransportUnit.UnitId))
                            {
                                UnitBase unitBase;
                                if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.TransportUnit.UnitId, out unitBase))
                                {
                                    if (!highlightedUnits.Contains(unitBase))
                                        highlightedUnits.Add(unitBase);
                                    else
                                        remainHighlighted.Remove(unitBase);
                                    unitBase.SetHighlighted(IsHighlighted);
                                }
                            }
                            if (!string.IsNullOrEmpty(mapGameCommandItem.TargetUnit.UnitId))
                            {
                                UnitBase unitBase;
                                if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.TargetUnit.UnitId, out unitBase))
                                {
                                    if (!highlightedUnits.Contains(unitBase))
                                        highlightedUnits.Add(unitBase);
                                    else
                                        remainHighlighted.Remove(unitBase);
                                    unitBase.SetHighlighted(IsHighlighted);
                                }
                            }
                        }
                    }
                }
                foreach (Position2 position2 in remainingPos)
                {
                    highlightedGroundCells[position2].SetHighlighted(false);
                    highlightedGroundCells.Remove(position2);
                }
                if (remainHighlighted.Count > 0)
                {
                    foreach (UnitBase unitBase in remainHighlighted)
                    {
                        unitBase.SetHighlighted(false);
                        highlightedUnits.Remove(unitBase);
                    }
                }
            }
            if (false && IsSelected)
            {
                //UpdateAttachedUnits();

                foreach (CommandAttachedItem commandAttachedUnit in selectedCommandUnits.Values) //CommandPreview.PreviewUnits)
                {
                    if (commandAttachedUnit.AttachedUnit.Line == null)
                    {
                        GameObject waypointPrefab = HexGrid.MainGrid.GetResource("Waypoint");

                        commandAttachedUnit.AttachedUnit.Line = Instantiate(waypointPrefab, transform, false);
                        commandAttachedUnit.AttachedUnit.Line.name = "Waypoint";
                    }

                    commandAttachedUnit.AttachedUnit.Line.SetActive(true);
                    var lr = commandAttachedUnit.AttachedUnit.Line.GetComponent<LineRenderer>();

                    /*
                    if (com unitCommand.GameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        //lr.startWidth = 0.1f;
                        lr.startColor = Color.red;
                        //lr.endWidth = 0.1f;
                        lr.endColor = Color.red;
                    }

                    if (unitCommand.GameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        //lr.startWidth = 0.1f;
                        lr.startColor = Color.green;
                        //lr.endWidth = 0.1f;
                        lr.endColor = Color.green;
                    }*/

                    Vector3 v1 = transform.position;

                    if (commandAttachedUnit.AttachedUnit.GhostUnit == null ||
                        commandAttachedUnit.AttachedUnit.GhostUnit.gameObject == null)
                    {
                        lr.endColor = Color.red;
                    }
                    else
                    {
                        Vector3 v2 = commandAttachedUnit.AttachedUnit.GhostUnit.transform.position;
                        v1.y += 0.3f;
                        v2.y += 0.3f;

                        lr.SetPosition(0, v1);
                        lr.SetPosition(1, v2);
                    }
                }
            }
            /* Na... rotates all
            if (!CommandPreview.IsPreview)
            {
                if (CommandPreview.GameCommand.GameCommandType == GameCommandType.Attack)
                    transform.Rotate(Vector3.right);
                else
                    transform.Rotate(Vector3.up); // * Time.deltaTime);
            }*/

            else
            {
                foreach (CommandAttachedItem commandAttachedUnit in selectedCommandUnits.Values) //CommandPreview.PreviewUnits)
                {
                    if (commandAttachedUnit.AttachedUnit.Line != null)
                        commandAttachedUnit.AttachedUnit.Line.SetActive(false);
                }
            }
        }
    }
}