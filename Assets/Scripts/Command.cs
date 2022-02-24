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

        //private Dictionary<UnitBase, CommandAttachedItem> selectedCommandUnits = new Dictionary<UnitBase, CommandAttachedItem>();

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

        public void DrawLine(Vector3 from, Vector3 to)
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

            targetPosition3 = new Position3(CommandPreview.GameCommand.TargetPosition);
            if (IsSelected)
            {
                MapGameCommand gameCommand = CommandPreview.GameCommand;
                UnitBase attachedUnit = null;
                if (gameCommand.AttachedUnit?.UnitId != null)
                {
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(gameCommand.AttachedUnit.UnitId, out attachedUnit))
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
                if (gameCommand.FactoryUnit?.UnitId != null)
                {
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(gameCommand.FactoryUnit.UnitId, out factoryUnit))
                    {
                        DrawLine(transform.position, factoryUnit.transform.position);
                    }
                }

                UnitBase targetUnit = null;
                if (gameCommand.TargetUnit?.UnitId != null)
                {
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(gameCommand.TargetUnit.UnitId, out targetUnit))
                    {
                        DrawLine(transform.position, targetUnit.transform.position);
                    }
                }
                UnitBase transportUnit = null;
                if (gameCommand.TransportUnit?.UnitId != null)
                {
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(gameCommand.TransportUnit.UnitId, out transportUnit))
                    {
                        if (attachedUnit != null)
                            DrawLine(attachedUnit.transform.position, transportUnit.transform.position);
                        if (targetUnit != null)
                            DrawLine(targetUnit.transform.position, transportUnit.transform.position);
                    }
                }
            }
            if (CommandPreview.GameCommand.GameCommandType == GameCommandType.Fire)
            {
                
            }
            else
            {
                //bool showAlert = false;

                // Display Ghost?
                //foreach (CommandAttachedItem commandAttachedUnit in CommandPreview.PreviewUnits)
                {
                    /*
                    if (commandAttachedUnit.MapGameCommandItem.Alert)
                    {
                        showAlert = true;
                    }*/

                    if (string.IsNullOrEmpty(CommandPreview.GameCommand.AttachedUnit.UnitId))
                    {
                        // Display Ghost, unit does not exist
                        if (CommandPreview.AttachedUnit.IsVisible)
                        {

                        }
                        else
                        {
                            //Debug.Log("Activate: commandAttachedUnit.UnitBase");
                            // Real unit missing, show ghost
                            CommandPreview.AttachedUnit.IsVisible = true;
                            if (CommandPreview.AttachedUnit.GhostUnit != null)
                                CommandPreview.AttachedUnit.GhostUnit.IsVisible = true;
                            if (CommandPreview.AttachedUnit.GhostUnitBounds != null)
                                CommandPreview.AttachedUnit.GhostUnitBounds.IsVisible = true;
                        }
                    }
                    else
                    {
                        UnitBase realUnit;
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(CommandPreview.GameCommand.AttachedUnit.UnitId, out realUnit))
                        {
                            if (CommandPreview.AttachedUnit.IsVisible)
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
                                        // NO, keep it until unit arrives and command is complete
                                        //if (commandAttachedUnit.AttachedUnit.GhostUnit != null)
                                        //    commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = false;
                                    }
                                }
                            }
                            if (CommandPreview.AttachedUnit.GhostUnitBounds != null)
                            {
                                Position3 relativePosition3 = targetPosition3.Add(CommandPreview.AttachedUnit.RotatedPosition3);
                                if (IsSelected)
                                {
                                    CommandPreview.AttachedUnit.GhostUnitBounds.IsVisible = true;
                                }
                                else
                                {
                                    if (realUnit.CurrentPos == relativePosition3.Pos)
                                    {
                                        CommandPreview.AttachedUnit.GhostUnitBounds.IsVisible = false;
                                    }
                                    else
                                    {
                                        CommandPreview.AttachedUnit.GhostUnitBounds.IsVisible = true;
                                    }
                                }
                            }
                            if (IsSelected)
                                realUnit.SetHighlighted(IsHighlighted);
                        }
                        else
                        {
                            if (CommandPreview.AttachedUnit.GhostUnitBounds != null)
                            {
                                if (IsSelected)
                                {
                                    CommandPreview.AttachedUnit.GhostUnitBounds.IsVisible = true;
                                }
                                else
                                {
                                    CommandPreview.AttachedUnit.GhostUnitBounds.IsVisible = true;
                                }
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
                if (CommandPreview.IsPreview)
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


                // Highlight attached units
                if (CommandPreview.GameCommand.GameCommandType == GameCommandType.Collect ||
                    CommandPreview.GameCommand.GameCommandType == GameCommandType.Build ||
                    CommandPreview.GameCommand.GameCommandType == GameCommandType.ItemRequest)
                {
                    MapGameCommand mapGameCommand = CommandPreview.GameCommand;
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
                    if (!string.IsNullOrEmpty(mapGameCommand.AttachedUnit.UnitId))
                    {
                        UnitBase unitBase;
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommand.AttachedUnit.UnitId, out unitBase))
                        {

                            if (!highlightedUnits.Contains(unitBase))
                                highlightedUnits.Add(unitBase);
                            else
                                remainHighlighted.Remove(unitBase);

                            unitBase.SetHighlighted(IsHighlighted);
                        }
                    }
                    if (!string.IsNullOrEmpty(mapGameCommand.FactoryUnit.UnitId))
                    {
                        UnitBase unitBase;
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommand.FactoryUnit.UnitId, out unitBase))
                        {
                            if (!highlightedUnits.Contains(unitBase))
                                highlightedUnits.Add(unitBase);
                            else
                                remainHighlighted.Remove(unitBase);
                            unitBase.SetHighlighted(IsHighlighted);
                        }
                    }
                    if (!string.IsNullOrEmpty(mapGameCommand.TransportUnit.UnitId))
                    {
                        UnitBase unitBase;
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommand.TransportUnit.UnitId, out unitBase))
                        {
                            if (!highlightedUnits.Contains(unitBase))
                                highlightedUnits.Add(unitBase);
                            else
                                remainHighlighted.Remove(unitBase);
                            unitBase.SetHighlighted(IsHighlighted);
                        }
                    }
                    if (!string.IsNullOrEmpty(mapGameCommand.TargetUnit.UnitId))
                    {
                        UnitBase unitBase;
                        if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommand.TargetUnit.UnitId, out unitBase))
                        {
                            if (!highlightedUnits.Contains(unitBase))
                                highlightedUnits.Add(unitBase);
                            else
                                remainHighlighted.Remove(unitBase);
                            unitBase.SetHighlighted(IsHighlighted);
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

                //foreach (CommandAttachedItem commandAttachedUnit in selectedCommandUnits.Values) //CommandPreview.PreviewUnits)
                {
                    if (CommandPreview.AttachedUnit.Line == null)
                    {
                        GameObject waypointPrefab = HexGrid.MainGrid.GetResource("Waypoint");

                        CommandPreview.AttachedUnit.Line = Instantiate(waypointPrefab, transform, false);
                        CommandPreview.AttachedUnit.Line.name = "Waypoint";
                    }

                    CommandPreview.AttachedUnit.Line.SetActive(true);
                    var lr = CommandPreview.AttachedUnit.Line.GetComponent<LineRenderer>();

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

                    if (CommandPreview.AttachedUnit.GhostUnit == null ||
                        CommandPreview.AttachedUnit.GhostUnit.gameObject == null)
                    {
                        lr.endColor = Color.red;
                    }
                    else
                    {
                        Vector3 v2 = CommandPreview.AttachedUnit.GhostUnit.transform.position;
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
                
            }
        }
    }
}