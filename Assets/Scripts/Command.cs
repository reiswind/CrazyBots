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
                    highlightEffect.SetHighlighted(IsHighlighted);
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

        /*
        public void UpdateAttachedUnits()
        {
            List<UnitBase> allCommandUnits = new List<UnitBase>();
            allCommandUnits.AddRange(selectedCommandUnits.Keys);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (MapGameCommandItem mapGameCommandItem in CommandPreview.GameCommand.GameCommandItems)
            {
                string unitId = mapGameCommandItem.AttachedUnitId;
                if (unitId != null)
                {
                    UnitBase unitBase;
                    if (unitId.StartsWith("Assembler"))
                    {
                        unitBase = HexGrid.MainGrid.BaseUnits[unitId.Substring(10)];
                    }
                    else
                    {
                        HexGrid.MainGrid.BaseUnits.TryGetValue(unitId, out unitBase);
                    }
                    if (unitBase != null)
                    {
                        allCommandUnits.Remove(unitBase);
                        if (!selectedCommandUnits.ContainsKey(unitBase))
                        {
                            CommandAttachedUnit commandAttachedUnit = new CommandAttachedUnit();
                            commandAttachedUnit.UnitBase = unitBase;
                            selectedCommandUnits.Add(unitBase, commandAttachedUnit);
                            //unitBase.SetSelected(true);
                        }
                    }
                    stringBuilder.Append(unitId);
                }
            }
            foreach (UnitBase unitBase in allCommandUnits)
            {
                CommandAttachedUnit commandAttachedUnit;
                if (selectedCommandUnits.TryGetValue(unitBase, out commandAttachedUnit))
                {
                    if (commandAttachedUnit.Line != null)
                    {
                        Destroy(commandAttachedUnit.Line);
                        commandAttachedUnit.Line = null;
                    }
                    //commandAttachedUnit.UnitBase.SetSelected(false);
                    selectedCommandUnits.Remove(unitBase);
                }
            }
        }*/
        
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

        private void Update()
        {
            if (CommandPreview == null)
                return;

            Position3 targetPosition3;
            if (CommandPreview.IsMoveMode)
            {
                targetPosition3 = new Position3(CommandPreview.DisplayPosition);
            }
            else
            {
                targetPosition3 = new Position3(CommandPreview.GameCommand.TargetPosition);
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
                        commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = true;
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
                        }
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
                        CommandPreview.GameCommand.GameCommandType == GameCommandType.Build)
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