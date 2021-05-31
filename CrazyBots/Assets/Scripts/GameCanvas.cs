using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    public GameObject MineralText;
    public GameObject SelectedItemText;
    public HexGrid Game;

    private Text UIMineralText;
    private Text UISelectedItemText;

    // Start is called before the first frame update
    void Start()
    {
        UIMineralText = MineralText.GetComponent<Text>();
        UISelectedItemText = SelectedItemText.GetComponent<Text>();

        Game.StartGame();
    }

    UnitFrame unitFrame = null;
    string selectedObjectText;

    // Update is called once per frame
    void Update()
    {
        if (Game != null && Game.MapInfo != null)
        {
            UIMineralText.text = Game.MapInfo.TotalMetal.ToString();
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;

            unitFrame = null;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, Mathf.Infinity))
            {
                Engine1 engine1 = raycastHit.collider.GetComponent<Engine1>();
                if (engine1 != null) unitFrame = engine1.UnitFrame;
                if (unitFrame == null)
                {
                    Armor armor = raycastHit.collider.GetComponent<Armor>();
                    if (armor != null) unitFrame = armor.UnitFrame;
                }
                if (unitFrame == null)
                {
                    Weapon1 weapon1 = raycastHit.collider.GetComponent<Weapon1>();
                    if (weapon1 != null) unitFrame = weapon1.UnitFrame;
                }
                if (unitFrame == null)
                {
                    Container1 container1 = raycastHit.collider.GetComponent<Container1>();
                    if (container1 != null) unitFrame = container1.UnitFrame;
                }
                if (unitFrame == null)
                {
                    Extractor1 extractor1 = raycastHit.collider.GetComponent<Extractor1>();
                    if (extractor1 != null) unitFrame = extractor1.UnitFrame;
                }
                if (unitFrame == null)
                {
                    Reactor1 reactor1 = raycastHit.collider.GetComponent<Reactor1>();
                    if (reactor1 != null) unitFrame = reactor1.UnitFrame;
                }

                if (unitFrame == null)
                {
                    selectedObjectText = raycastHit.collider.name;
                }
            }
            else
            {
                selectedObjectText = "Nothing";
            }
        }
        if (unitFrame != null && unitFrame.HasBeenDestroyed)
        {
            selectedObjectText = "Destroyed: " + unitFrame.UnitId;
            unitFrame = null;
        }
        if (unitFrame != null)
        {
            if (unitFrame.MoveUpdateStats == null)
            {
                UISelectedItemText.text = "No Stats: " + unitFrame.UnitId;
            }
            else
            {
                string text;

                text = unitFrame.UnitId + "\r\n";
                text += " " + unitFrame.currentPos.X + ", " + unitFrame.currentPos.Y + "\r\n";
                text += " " + unitFrame.MoveUpdateStats.WeaponLoaded.ToString() + "\r\n";

                UISelectedItemText.text = text;

            }
        }
        else if (selectedObjectText != null)
        {
            UISelectedItemText.text = selectedObjectText;
        }
        else
        {

            UISelectedItemText.text = null;
        }
    }
}
