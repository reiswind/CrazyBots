using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{


    public class Command : MonoBehaviour
    {
        public CommandPreview CommandPreview { get; set; }

        public bool IsSelected { get; private set; }

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
                        meshRenderer.material.SetFloat("Darkness", 3.0f);
                    }
                    else
                    {
                        //RemoveAttachedUnits();
                        meshRenderer.material.SetFloat("Darkness", 1.0f);
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
                commandAttachedUnit.UnitBase.SetSelected(false);
            }
        }

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
        }
        
        void UpdateDirection(Vector3 position)
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
            if (IsSelected)
            {
                UpdateAttachedUnits();

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

                    if (commandAttachedUnit.UnitBase == null ||
                        commandAttachedUnit.UnitBase.gameObject == null)
                    {
                        lr.endColor = Color.red;
                    }
                    else
                    {
                        Vector3 v2 = commandAttachedUnit.UnitBase.transform.position;
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