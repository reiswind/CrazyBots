using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class HitByMouseClick
{
    public UnitBase UnitFrame { get; set; }
    public GroundCell GroundCell { get; set; }
}

public class GameCanvas : MonoBehaviour
{
    public GameObject MineralText;
    public GameObject SelectedObjectText;
    public GameObject SelectedObjectsText;
    public HexGrid HexGrid;

    private Text UIMineralText;
    private Text UISelectedObjectsText;
    private Text UISelectedObjectText;

    internal string SelectedBluePrint { get; set; }

    public Sprite SelectedButtonBackground;
    public Sprite ButtonBackground;

    private Text[] buttonText;
    private Button[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        UIMineralText = MineralText.GetComponent<Text>();
        UISelectedObjectText = SelectedObjectText.GetComponent<Text>();
        UISelectedObjectsText = SelectedObjectsText.GetComponent<Text>();

        Transform inGamePanel = transform.Find("InGame");

        Transform gameControlPanel = inGamePanel.Find("GameControl");
        Transform panelSelected = gameControlPanel.Find("PanelSelected");

        buttons = new Button[12];
        buttonText = new Text[12];
        for (int i = 0; i < 12; i++)
        {
            buttons[i] = panelSelected.Find("Button" + (i+1).ToString()).GetComponent<Button>();
            buttonText[i] = buttons[i].transform.Find("Text").GetComponent<Text>();
            buttons[i].name = i.ToString();
        }
        buttons[0].onClick.AddListener(OnClickBuild1);
        buttons[1].onClick.AddListener(OnClickBuild2);
        buttons[2].onClick.AddListener(OnClickBuild3);
        buttons[3].onClick.AddListener(OnClickBuild4);
        buttons[4].onClick.AddListener(OnClickBuild5);
        buttons[5].onClick.AddListener(OnClickBuild6);
        buttons[6].onClick.AddListener(OnClickBuild7);
        buttons[7].onClick.AddListener(OnClickBuild8);

        buttons[8].onClick.AddListener(OnClickBuild9);
        buttons[9].onClick.AddListener(OnClickBuild10);
        buttons[10].onClick.AddListener(OnClickBuild11);
        buttons[11].onClick.AddListener(OnClickBuild12);

        UpdateCommandButtons();

        HexGrid.StartGame();
    }

    private Button GetButton(int btn)
    {
        return buttons[btn-1];
    }

    private Text GetButtonText(int btn)
    {
        return buttonText[btn-1];
    }

    private void SetButtonText(int btn, string text)
    {
        ShowButton(btn);
        GetButtonText(btn).text = text;
    }

    private void HideButton(int btn)
    {
        GetButton(btn).gameObject.SetActive(false);
    }

    private void SelectButton(int btn)
    {
        GetButton(btn).image.sprite = SelectedButtonBackground;
    }
    private void UnselectButton(int btn)
    {
        GetButton(btn).image.sprite = ButtonBackground;
    }

    private void ShowButton(int btn)
    {
        GetButton(btn).gameObject.SetActive(true);
    }

    private int topSelectedButton;
    private int middleSelectedButton;
    private int lowSelectedButton;

    private void UpdateCommandButtons()
    {
        if (selectedUnitFrame == null && lastSelectedGroundCell == null)
        {
            topSelectedButton = 0;
            middleSelectedButton = 0;
            lowSelectedButton = 0;

            for (int i = 1; i <= 12; i++)
            {
                HideButton(i);
            }
        }
        else if (selectedUnitFrame == null)
        {
            SetButtonText(1, "(q) Building");
            SetButtonText(2, "(w) Unit");
            SetButtonText(3, "(e) Defense");
            SetButtonText(4, "(r) Special");

            if (topSelectedButton == 0)
            {
                HideButton(5);
                HideButton(6);
                HideButton(7);
                HideButton(8);
            }
            else if (topSelectedButton == 1)
            {
                SetButtonText(5, "(a) Assembler");
                SetButtonText(6, "(s) Container");
                SetButtonText(7, "(d) Reactor");
                SetButtonText(8, "(f) Radar");

                if (middleSelectedButton == 0)
                {
                    HideButton(9);
                    HideButton(10);
                    HideButton(11);
                    HideButton(12);
                }
                else if (middleSelectedButton == 5)
                {
                    SetButtonText(9, "(y) Shield");
                    SetButtonText(10, "(x) Container1");
                    HideButton(11);
                    HideButton(12);
                }
                else if (middleSelectedButton == 6)
                {
                    SetButtonText(9, "(y) Shield");
                    SetButtonText(10, "(x) Container3");
                    HideButton(11);
                    HideButton(12);
                }
                else if (middleSelectedButton == 7)
                {
                    SetButtonText(9, "(y) Shield");
                    SetButtonText(10, "(x) Reactor3");
                    HideButton(11);
                    HideButton(12);
                }

            }
            else if (topSelectedButton == 2)
            {
                SetButtonText(5, "(a) Assembler");
                SetButtonText(6, "(s) Fighter");
                SetButtonText(7, "(d) Worker");
                HideButton(8);

                if (middleSelectedButton == 0 || middleSelectedButton == 8)
                {
                    HideButton(9);
                    HideButton(10);
                    HideButton(11);
                    HideButton(12);
                }
                else if (middleSelectedButton == 5)
                {
                    SetButtonText(9, "(y) Shield");
                    SetButtonText(10, "(x) Container");
                    SetButtonText(11, "(c) Engine2");
                    HideButton(12);
                }
                else if (middleSelectedButton == 6)
                {
                    SetButtonText(9, "(y) Shield");
                    SetButtonText(10, "(x) Weapon2");
                    SetButtonText(11, "(c) Engine2");
                    HideButton(12);
                }
                else if (middleSelectedButton == 7)
                {
                    SetButtonText(9, "(y) Shield");
                    SetButtonText(10, "(x) Container2");
                    SetButtonText(11, "(c) Engine2");
                    HideButton(12);
                }

            }
            else if (topSelectedButton == 3)
            {
                SetButtonText(5, "(a) Turret");
                HideButton(6);
                HideButton(7);
                HideButton(8);

                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);
            }
            else if (topSelectedButton == 4)
            {
                SetButtonText(5, "(a) Forschung");
                HideButton(6);
                HideButton(7);
                HideButton(8);

                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);
            }
        }
        else
        {
            ShowButton(1);
            SetButtonText(1, selectedUnitFrame.name);
            HideButton(2);
            HideButton(3);
            HideButton(4);
        }
    }

    void SelectTopButton(int btn)
    {
        if (topSelectedButton != 0 && topSelectedButton != btn)
            UnselectButton(topSelectedButton);
        topSelectedButton = btn;
        SelectButton(btn);
        UpdateCommandButtons();
    }

    void SelectMiddleButton(int btn)
    {
        if (middleSelectedButton != 0 && middleSelectedButton != btn)
            UnselectButton(middleSelectedButton);
        middleSelectedButton = btn;
        SelectButton(btn);
        UpdateCommandButtons();
    }

    void SelectLowButton(int btn)
    {
        if (lowSelectedButton != 0 && lowSelectedButton != btn)
            UnselectButton(lowSelectedButton);
        lowSelectedButton = btn;
        SelectButton(btn);
        UpdateCommandButtons();
    }

    void OnClickBuild1()
    {
        SelectTopButton(1);
    }
    void OnClickBuild2()
    {
        SelectTopButton(2);
    }
    void OnClickBuild3()
    {
        SelectTopButton(3);
    }
    void OnClickBuild4()
    {
        SelectTopButton(4);
    }

    void OnClickBuild5()
    {
        SelectMiddleButton(5);
    }
    void OnClickBuild6()
    {
        SelectMiddleButton(6);
    }
    void OnClickBuild7()
    {
        SelectMiddleButton(7);
    }
    void OnClickBuild8()
    {
        SelectMiddleButton(8);
    }

    void OnClickBuild9()
    {
        SelectLowButton(0);
    }
    void OnClickBuild10()
    {
        SelectLowButton(10);
    }
    void OnClickBuild11()
    {
        SelectLowButton(11);
    }
    void OnClickBuild12()
    {
        SelectLowButton(12);
    }


    private UnitBase selectedUnitFrame;
    private GroundCell lastSelectedGroundCell;
    

    private HitByMouseClick GetClickedInfo()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return null;

        HitByMouseClick hitByMouseClick = null;

        RaycastHit raycastHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, Mathf.Infinity))
        {
            hitByMouseClick = new HitByMouseClick();
            hitByMouseClick.GroundCell = raycastHit.collider.gameObject.GetComponent<GroundCell>();
            hitByMouseClick.UnitFrame = GetUnitFrameFromRayCast(raycastHit);
            if (hitByMouseClick.UnitFrame != null && hitByMouseClick.GroundCell == null)
            {
                hitByMouseClick.GroundCell = HexGrid.GroundCells[hitByMouseClick.UnitFrame.CurrentPos];
            }
            if (hitByMouseClick.UnitFrame == null && hitByMouseClick.GroundCell != null)
            {
                foreach (UnitBase unitFrame in HexGrid.BaseUnits.Values)
                {
                    if (unitFrame.CurrentPos == hitByMouseClick.GroundCell.Tile.Pos)
                    {
                        hitByMouseClick.UnitFrame = unitFrame;
                        break;
                    }
                }
            }
        }

        return hitByMouseClick;
    }

    private UnitBase GetUnitFrameFromRayCast(RaycastHit raycastHit)
    {
        UnitBase unitBase = raycastHit.collider.GetComponent<UnitBase>();
        if (unitBase != null) return unitBase;

        unitBase = raycastHit.collider.transform.parent.GetComponent<UnitBase>();
        if (unitBase != null) return unitBase;

        return null;
    }

    private void AppendGroundInfo(GroundCell gc, StringBuilder sb)
    {
        sb.AppendLine("Position: " + gc.Tile.Pos.X + ", " + gc.Tile.Pos.Y);
        if (gc.Tile.Metal > 0)
            sb.AppendLine("Minerals: " + gc.Tile.Metal);
        if (gc.Tile.NumberOfDestructables > 0)
            sb.AppendLine("NumberOfDestructables: " + gc.Tile.NumberOfDestructables);
        if (gc.Tile.NumberOfObstacles > 0)
            sb.AppendLine("NumberOfObstacles: " + gc.Tile.NumberOfObstacles);

        if (gc.UnitCommands != null)
        {
            foreach (UnitCommand unitCommand in gc.UnitCommands)
            {
                sb.AppendLine("Command: " + unitCommand.GameCommand.ToString() + " Owner: " + unitCommand.Owner.UnitId);
            }
        }

    }


    private bool ShifKeyIsDown;

    // Update is called once per frame
    void Update()
    {


        if (HexGrid != null && HexGrid.MapInfo != null)
        {
            UIMineralText.text = HexGrid.MapInfo.TotalMetal.ToString();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
            ShifKeyIsDown = true;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            ShifKeyIsDown = false;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnClickBuild1();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnClickBuild2();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnClickBuild3();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UISelectedObjectsText.text = "";
            SelectedBluePrint = null;
            UpdateCommandButtons();
        }

        if (Input.GetMouseButtonDown(1))
        {
            //SelectedBluePrint = UISelectedBuildText.text = "";
            //UISelectedObjectsText.text = "";
            //SelectedBluePrint = null;
            if (lastSelectedGroundCell != null)
            {
                lastSelectedGroundCell.SetSelected(false);
                lastSelectedGroundCell = null;
            }
            if (selectedUnitFrame != null)
            {
                selectedUnitFrame.SetSelected(false);
                selectedUnitFrame = null;
            }
            UpdateCommandButtons();

            /*
            HitByMouseClick hitByMouseClick = GetClickedInfo();

            if (selectedUnitFrame != null && hitByMouseClick.GroundCell != null) // && groundCell.Tile.CanMoveTo())
            {
                if (selectedUnitFrame.IsAssembler())
                {
                    // Move it there
                    GameCommand gameCommand = new GameCommand();

                    gameCommand.UnitId = selectedUnitFrame.UnitId;
                    gameCommand.TargetPosition = hitByMouseClick.GroundCell.Tile.Pos;

                    if (hitByMouseClick.GroundCell.Tile.Metal > 0)
                        gameCommand.GameCommandType = GameCommandType.Minerals;
                    else
                        gameCommand.GameCommandType = GameCommandType.Attack;

                    gameCommand.Append = ShifKeyIsDown;
                    HexGrid.GameCommands.Add(gameCommand);

                    if (!ShifKeyIsDown)
                        selectedUnitFrame.ClearWayPoints();

                    UnitCommand unitCommand = new UnitCommand();
                    unitCommand.GameCommand = gameCommand;
                    unitCommand.Owner = selectedUnitFrame;
                    unitCommand.TargetCell = hitByMouseClick.GroundCell;

                    selectedUnitFrame.UnitCommands.Add(unitCommand);
                    selectedUnitFrame.UpdateWayPoints();

                    hitByMouseClick.GroundCell.UnitCommands.Add(unitCommand);
                }
                else
                {
                    if (lastSelectedGroundCell != null)
                    {
                        lastSelectedGroundCell.SetSelected(false);
                        lastSelectedGroundCell = null;
                    }
                }
            }
            else
            {
                // Build something
                GameCommand gameCommand = new GameCommand();

                gameCommand.UnitId = SelectedBluePrint;
                gameCommand.TargetPosition = hitByMouseClick.GroundCell.Tile.Pos;
                gameCommand.GameCommandType = GameCommandType.Build;
                gameCommand.PlayerId = 1;
                HexGrid.GameCommands.Add(gameCommand);

                HexGrid.CreateGhost(gameCommand.UnitId, hitByMouseClick.GroundCell.Tile.Pos);
            }*/

            /*
            if (gameCommand.Append)
                Debug.Log("Move to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y + " SHIFT");
            else
                Debug.Log("Move to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y);
            */


        }

        if (Input.GetMouseButtonDown(0))
        {
            HitByMouseClick hitByMouseClick = GetClickedInfo();

            if (hitByMouseClick == null)
            {
                /*
                if (lastSelectedGroundCell != null)
                {
                    lastSelectedGroundCell.SetSelected(false);
                }
                lastSelectedGroundCell = null;
                selectedUnitFrame = null;

                SelectedBluePrint = null;
                UpdateCommandButtons();*/
            }
            else
            {
                if (!string.IsNullOrEmpty(SelectedBluePrint))
                {
                    /*
                    // Build something
                    if (selectedUnitFrame == null)
                    {
                        GameCommand gameCommand = new GameCommand();

                        gameCommand.UnitId = SelectedBluePrint;
                        gameCommand.TargetPosition = hitByMouseClick.GroundCell.Tile.Pos;
                        gameCommand.GameCommandType = GameCommandType.Build;
                        gameCommand.PlayerId = 1;
                        HexGrid.GameCommands.Add(gameCommand);

                        HexGrid.CreateGhost(gameCommand.UnitId, hitByMouseClick.GroundCell.Tile.Pos);

                        if (!ShifKeyIsDown)
                            SelectedBluePrint = UISelectedBuildText.text = "";
                    }*/
                }
                else
                {
                    if (lastSelectedGroundCell != hitByMouseClick.GroundCell)
                    {
                        if (lastSelectedGroundCell != null)
                            lastSelectedGroundCell.SetSelected(false);
                        if (hitByMouseClick.GroundCell != null && hitByMouseClick.UnitFrame == null)
                            hitByMouseClick.GroundCell.SetSelected(true);
                    }
                    lastSelectedGroundCell = hitByMouseClick.GroundCell;

                    if (selectedUnitFrame != hitByMouseClick.UnitFrame)
                    {
                        if (selectedUnitFrame != null)
                            selectedUnitFrame.SetSelected(false);
                        if (hitByMouseClick.UnitFrame != null)
                            hitByMouseClick.UnitFrame.SetSelected(true);
                    }
                    selectedUnitFrame = hitByMouseClick.UnitFrame;

                    if (selectedUnitFrame != null || lastSelectedGroundCell != null)
                    {
                        //SelectedBluePrint = UISelectedObjectsText.text = selectedUnitFrame.MoveUpdateStats.BlueprintName;
                        UpdateCommandButtons();
                    }
                }
            }
        }
        if (selectedUnitFrame != null)
        {
            StringBuilder sb = new StringBuilder();

            UnitBase unit = selectedUnitFrame;

            sb.AppendLine("Unit: " + unit.UnitId);
            if (unit.HasBeenDestroyed)
            {
                sb.AppendLine("Destroyed");
                selectedUnitFrame = null;
            }
            if (unit.MoveUpdateStats == null)
            {
                sb.AppendLine("No Stats: " + unit.UnitId);
            }
            else
            {
                if (unit.MoveUpdateStats != null)
                {
                    sb.AppendLine("Blueprint: " + unit.MoveUpdateStats.BlueprintName);

                    foreach (MoveUpdateUnitPart part in unit.MoveUpdateStats.UnitParts)
                    {
                        sb.Append("  " + part.Name);
                        if (part.Exists)
                        {
                            if (part.Minerals.HasValue)
                                sb.Append("  " + part.Minerals.Value);
                            if (part.Capacity.HasValue)
                                sb.Append("/" + part.Capacity.Value);
                        }
                        else
                        {
                            sb.Append("  Missing");
                        }
                        sb.AppendLine();
                    }
                }
            }
            sb.AppendLine("");

            GroundCell gc = HexGrid.GroundCells[unit.CurrentPos];
            AppendGroundInfo(gc, sb);

            UISelectedObjectText.text = sb.ToString();
            
        }
        else if (lastSelectedGroundCell != null)
        {
            GroundCell gc = lastSelectedGroundCell;

            StringBuilder sb = new StringBuilder();
            AppendGroundInfo(gc, sb);

            UISelectedObjectText.text = sb.ToString();
        }
        else
        {

            UISelectedObjectText.text = null;
            UISelectedObjectsText.text = "";
        }

    }
}
