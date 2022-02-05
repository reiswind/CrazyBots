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
    }
    void OnClick()
    {
        gameCanvas.ExecuteCommand(blueprintCommand);
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
