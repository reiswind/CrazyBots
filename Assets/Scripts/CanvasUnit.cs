using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class CanvasUnit : MonoBehaviour
    {
        internal void Hide()
        {
            gameObject.SetActive(false);
        }
        void Update()
        {
            
            if (gameObject.transform.childCount == 1)
            {
                GameObject unit = gameObject.transform.GetChild(0).gameObject;
                unit.transform.Rotate(Vector3.up, 0.2f);
             }
            //gameObject.transform.Rotate(Vector3.up);
        }

        private string BlueprintName { get; set; }

        internal void ShowBluePrint(string blueprintName, int playerId)
        {

            if (BlueprintName != blueprintName)
            {
                int childs = gameObject.transform.childCount;
                for (int i = childs - 1; i >= 0; i--)
                {
                    DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
                }
                Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(blueprintName);
                UnitBase unitBaseIcon = HexGrid.MainGrid.CreateTempUnit(blueprint, playerId, false, false, true);

                //unitBaseIcon.ActivateUnit();
                unitBaseIcon.Direction = Direction.SE;
                unitBaseIcon.transform.SetParent(gameObject.transform);
                unitBaseIcon.transform.position = new Vector3(0, 0, 0);
                unitBaseIcon.transform.localPosition = new Vector3(0, -25, 0);
                unitBaseIcon.transform.localScale = new Vector3(blueprint.GuiScaling, blueprint.GuiScaling, blueprint.GuiScaling);

                BlueprintName = blueprintName;
            }
            gameObject.SetActive(true);

        }

    }
}