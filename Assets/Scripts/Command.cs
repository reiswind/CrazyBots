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

        
        private Dictionary<UnitBase, CommandAttachedUnit> selectedCommandUnits = new Dictionary<UnitBase, CommandAttachedUnit>();

        private void RemoveAttachedUnits()
        {
            foreach (CommandAttachedUnit commandAttachedUnit in selectedCommandUnits.Values)
            {
                if (commandAttachedUnit.Line != null)
                {
                    Destroy(commandAttachedUnit.Line);
                    commandAttachedUnit.Line = null;
                }
                commandAttachedUnit.GhostUnit.SetHighlighted(false);
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
            //float speed = 1.75f;
            float speed = 3.5f / HexGrid.MainGrid.GameSpeed;

            // Determine which direction to rotate towards
            Vector3 targetDirection = position - transform.position;

            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            Vector3 forward = transform.forward;
            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(forward, targetDirection, singleStep, 0.0f);
            newDirection.y = 0;

            // Draw a ray pointing at our target in
            //Debug.DrawRay(transform.position, newDirection, Color.red);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        private void Update()
        {
            Position3 targetPosition3;
            if (CommandPreview.IsMoveMode)
            {
                targetPosition3 = new Position3(CommandPreview.DisplayPosition);
            }
            else
            {
                targetPosition3 = new Position3(CommandPreview.GameCommand.TargetPosition);
            }

            // Display Ghost?
            foreach (CommandAttachedUnit commandAttachedUnit in CommandPreview.PreviewUnits)
            {
                if (string.IsNullOrEmpty(commandAttachedUnit.MapGameCommandItem.AttachedUnitId))
                {
                    // Display Ghost, unit does not exist
                    if (commandAttachedUnit.IsVisible)
                    {

                    }
                    else
                    {
                        //Debug.Log("Activate: commandAttachedUnit.UnitBase");
                        // Real unit missing, show ghost
                        commandAttachedUnit.IsVisible = true;
                        commandAttachedUnit.GhostUnit.IsVisible = true;
                        commandAttachedUnit.Marker.SetActive(true);
                    }
                }
                else
                {
                    UnitBase realUnit;
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(commandAttachedUnit.MapGameCommandItem.AttachedUnitId, out realUnit))
                    {
                        if (commandAttachedUnit.IsVisible)
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
                                commandAttachedUnit.GhostUnit.IsVisible = false;
                            }
                        }
                        Position3 relativePosition3 = targetPosition3.Add(commandAttachedUnit.RotatedPosition3);
                        if (IsSelected)
                        {
                            commandAttachedUnit.Marker.SetActive(true);
                        }
                        else
                        {
                            if (realUnit.CurrentPos == relativePosition3.Pos)
                            {
                                commandAttachedUnit.Marker.SetActive(false);
                            }
                            else
                            {
                                commandAttachedUnit.Marker.SetActive(true);
                            }
                        }
                        realUnit.SetHighlighted(IsHighlighted);
                    }
                }
            }
            

            if (IsHighlighted)
            {
                List<Position2> remainingPos = new List<Position2>();
                remainingPos.AddRange(highlightedGroundCells.Keys);

                Position2 center;
                if (CommandPreview.IsPreview || CommandPreview.IsInSubCommandMode)
                    center = CommandPreview.DisplayPosition;
                else
                    center = CommandPreview.GameCommand.TargetPosition;
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
                            gc = HexGrid.MainGrid.GroundCells[position2];

                            if (gc.NumberOfCollectables > 0)
                            {
                                gc.SetHighlighted(IsHighlighted);
                                highlightedGroundCells.Add(gc.Pos, gc);
                                remainingPos.Remove(position2);
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
                        foreach (CommandAttachedUnit commandAttachedUnit in CommandPreview.PreviewUnits)
                        {
                            Position3 relativePosition3 = targetPosition3.Add(commandAttachedUnit.RotatedPosition3);
                            Position2 position2 = relativePosition3.Pos;
                            GroundCell gc;
                            if (HexGrid.MainGrid.GroundCells.TryGetValue(position2, out gc))
                            {
                                Vector3 unitPos3 = gc.transform.position;
                                unitPos3.y += 0.10f;
                                commandAttachedUnit.Marker.transform.position = unitPos3;

                                commandAttachedUnit.GhostUnit.IsVisible = true;
                                commandAttachedUnit.IsVisible = true;

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
                            if (!string.IsNullOrEmpty(mapGameCommandItem.AttachedUnitId))
                            {
                                UnitBase unitBase;
                                if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.AttachedUnitId, out unitBase))
                                {
                                    
                                    if (!highlightedUnits.Contains(unitBase))
                                        highlightedUnits.Add(unitBase);
                                    else
                                        remainHighlighted.Remove(unitBase);
                                    
                                    unitBase.SetHighlighted(IsHighlighted);
                                }
                            }
                            if (!string.IsNullOrEmpty(mapGameCommandItem.FactoryUnitId))
                            {
                                UnitBase unitBase;
                                if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.FactoryUnitId, out unitBase))
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

                foreach (CommandAttachedUnit commandAttachedUnit in selectedCommandUnits.Values) //CommandPreview.PreviewUnits)
                {
                    if (commandAttachedUnit.Line == null)
                    {
                        GameObject waypointPrefab = HexGrid.MainGrid.GetResource("Waypoint");

                        commandAttachedUnit.Line = Instantiate(waypointPrefab, transform, false);
                        commandAttachedUnit.Line.name = "Waypoint";
                    }

                    commandAttachedUnit.Line.SetActive(true);
                    var lr = commandAttachedUnit.Line.GetComponent<LineRenderer>();

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

                    if (commandAttachedUnit.GhostUnit == null ||
                        commandAttachedUnit.GhostUnit.gameObject == null)
                    {
                        lr.endColor = Color.red;
                    }
                    else
                    {
                        Vector3 v2 = commandAttachedUnit.GhostUnit.transform.position;
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
                foreach (CommandAttachedUnit commandAttachedUnit in selectedCommandUnits.Values) //CommandPreview.PreviewUnits)
                {
                    if (commandAttachedUnit.Line != null)
                        commandAttachedUnit.Line.SetActive(false);
                }
            }
        }
    }
}