using Assets.Scripts;
using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildItem : MonoBehaviour
{
    private BlueprintCommand blueprintCommand;
    private GameCanvas gameCanvas;

    public void SetCommand(GameCanvas gameCanvas, BlueprintCommand blueprintCommand)
    {
        this.blueprintCommand = blueprintCommand;
        this.gameCanvas = gameCanvas;

        Text text = transform.Find("Text").GetComponent<Text>();
        if (text != null)
        {
            text.text = blueprintCommand.Name;
        }
        Transform unitGameObject = transform.Find("Panel");
        if (unitGameObject != null)
        {
            CanvasUnit unit = unitGameObject.GetComponent<CanvasUnit>();
            if (unit != null)
            {
                unit.ShowBluePrint(blueprintCommand.Name, 1);
            }
        }
    }
    void OnClick()
    {
        gameCanvas.PreviewExecuteCommand(blueprintCommand);
    }
        
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }
}
