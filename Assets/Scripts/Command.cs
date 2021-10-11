using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    internal class CommandAttachedUnit
    {
        public UnitBase UnitBase { get; set; }
        public GameObject Line { get; set; }

    }

    public class Command : MonoBehaviour
    {
        public GameCommand GameCommand { get; set; }

        private bool IsSelected { get; set; }
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
                        UpdateAttachedUnits();
                        meshRenderer.material.SetFloat("Darkness", 3.0f);
                    }
                    else
                    {
                        RemoveAttachedUnits();
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
            foreach (string unitId in GameCommand.AttachedUnits)
            {
                UnitBase unitBase = null;
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
                        unitBase.SetSelected(true);
                    }
                }
                stringBuilder.Append(unitId);
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
                    commandAttachedUnit.UnitBase.SetSelected(false);
                }
            }
        }

        private void Update()
        {
            if (IsSelected)
            {
                foreach (CommandAttachedUnit commandAttachedUnit in selectedCommandUnits.Values)
                {
                    if (commandAttachedUnit.Line == null)
                    {
                        GameObject waypointPrefab = HexGrid.MainGrid.GetResource("Waypoint");

                        commandAttachedUnit.Line = Instantiate(waypointPrefab, transform, false);
                        commandAttachedUnit.Line.name = "Waypoint";
                    }
                    

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

                    if (commandAttachedUnit.UnitBase.gameObject == null)
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
            transform.Rotate(Vector3.up); // * Time.deltaTime);
        }
    }
}