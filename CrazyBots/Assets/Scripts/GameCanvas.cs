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

internal enum CanvasMode
{
    None,
    Select,
    Build,
    Attack,
    Mineral
}

public class GameCanvas : MonoBehaviour
{
    public GameObject MineralText;
    public GameObject UnitText;
    public GameObject PowerText;
    public HexGrid HexGrid;

    public Texture2D NormalCursor;
    public Texture2D BuildCursor;
    public Texture2D AttackCursor;

    private Text UIMineralText;
    private Text UIUnitText;
    private Text UIPowerText;

    private CanvasMode canvasMode;
    internal string SelectedBluePrint { get; set; }

    public Sprite SelectedButtonBackground;
    public Sprite ButtonBackground;

    private Text[] buttonText;
    private Text[] actionText;
    private Button[] buttons;
    private Button[] actions;
    private Button buildButton;
    private Text buildButtonText;

    private GameObject panelEngine;
    private GameObject panelExtractor;
    private GameObject panelContainer;
    private GameObject panelAssembler;
    private GameObject panelArmor;
    private GameObject panelWeapon;
    private GameObject panelReactor;
    private Text headerText;
    private Text headerSubText;
    private Text headerGroundText;

    // Start is called before the first frame update
    void Start()
    {
        UIMineralText = MineralText.GetComponent<Text>();
        UIUnitText = UnitText.GetComponent<Text>();
        UIPowerText = PowerText.GetComponent<Text>();

        Transform inGamePanel = transform.Find("InGame");
        Transform gameControlPanel = inGamePanel.Find("GameControl");

        Transform panelItem = gameControlPanel.Find("PanelItem");
        headerText = panelItem.Find("HeaderText").GetComponent<Text>();
        headerSubText = panelItem.Find("SubText").GetComponent<Text>();
        headerGroundText = panelItem.Find("GroundText").GetComponent<Text>();

        headerText.text = "";
        headerSubText.text = "";
        headerGroundText.text = "";

        buildButton = panelItem.Find("BuildButton").GetComponent<Button>();
        buildButton.gameObject.SetActive(false);
        buildButton.onClick.AddListener(OnClickExtract);
        buildButtonText = buildButton.transform.Find("Text").GetComponent<Text>();

        Transform panelParts = panelItem.Find("PanelParts");

        panelEngine = panelParts.Find("PanelEngine").gameObject;
        panelExtractor = panelParts.Find("PanelExtractor").gameObject;
        panelContainer = panelParts.Find("PanelContainer").gameObject;
        panelAssembler = panelParts.Find("PanelAssembler").gameObject;
        panelArmor = panelParts.Find("PanelArmor").gameObject;
        panelWeapon = panelParts.Find("PanelWeapon").gameObject;
        panelReactor = panelParts.Find("PanelReactor").gameObject;

        Transform panelSelected = gameControlPanel.Find("PanelSelected");

        actions = new Button[12];
        actionText = new Text[12];
        for (int i = 0; i < 4; i++)
        {
            actions[i] = panelSelected.Find("Action" + (i + 1).ToString()).GetComponent<Button>();
            actionText[i] = actions[i].transform.Find("Text").GetComponent<Text>();
            actions[i].name = "Action" + (i + 1).ToString();
        }

        Transform panelFrame = panelSelected.Find("PanelFrame");

        buttons = new Button[12];
        buttonText = new Text[12];
        for (int i = 0; i < 12; i++)
        {
            buttons[i] = panelFrame.Find("Button" + (i + 1).ToString()).GetComponent<Button>();
            buttonText[i] = buttons[i].transform.Find("Text").GetComponent<Text>();
            buttons[i].name = "Button" + (i + 1).ToString();
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

        actions[0].onClick.AddListener(OnClickAction1);
        actions[1].onClick.AddListener(OnClickAction2);
        actions[2].onClick.AddListener(OnClickAction3);
        actions[3].onClick.AddListener(OnClickAction4);

        SetActionText(1, "(t) Select");
        SetActionText(2, "(g) Attack");
        SetActionText(3, "(b) Minerals");
        SetActionText(4, "(b) Build");

        UpdateCommandButtons();

        SetMode(CanvasMode.Select);
        HexGrid.StartGame();
    }

    void SetMode(CanvasMode newCanvasMode)
    {
        if (canvasMode != newCanvasMode)
        {
            canvasMode = newCanvasMode;

            UpdateCommandButtons();
            UnSelectTopButton();

            if (canvasMode == CanvasMode.Select)
            {
                SelectAction(1);
                UnselectAction(2);
                UnselectAction(3);
                UnselectAction(4);

                Cursor.SetCursor(NormalCursor, new Vector2(0, 0), CursorMode.Auto);

                if (topSelectedSelectButton == 0)
                    topSelectedSelectButton = 1;
                UnselectButton(1);
            }

            if (canvasMode == CanvasMode.Attack)
            {
                UnselectAction(1);
                SelectAction(2);
                UnselectAction(3);
                UnselectAction(4);

                Cursor.SetCursor(AttackCursor, new Vector2(0, 0), CursorMode.Auto);

                if (topSelectedAttackButton == 0)
                    topSelectedAttackButton = 1;
                SelectTopButton(topSelectedAttackButton);
            }

            if (canvasMode == CanvasMode.Mineral)
            {
                UnselectAction(1);
                UnselectAction(2);
                SelectAction(3);
                UnselectAction(4);

                Cursor.SetCursor(AttackCursor, new Vector2(0, 0), CursorMode.Auto);

                if (topSelectedMineralsButton == 0)
                    topSelectedMineralsButton = 1;
                SelectTopButton(topSelectedMineralsButton);
            }

            if (canvasMode == CanvasMode.Build)
            {
                UnselectAction(1);
                UnselectAction(2);
                UnselectAction(3);
                SelectAction(4);

                Cursor.SetCursor(BuildCursor, new Vector2(0, 0), CursorMode.Auto);

                if (topSelectedBuildButton == 0)
                    topSelectedBuildButton = 1;
                SelectTopButton(topSelectedBuildButton);
            }
        }
        leftMouseButtonDown = false;

    }

    void OnClickAction2()
    {
        if (canvasMode != CanvasMode.Attack)
        {            
            SetMode(CanvasMode.Attack);
        }
    }

    void OnClickAction3()
    {
        if (canvasMode != CanvasMode.Mineral)
        {
            SetMode(CanvasMode.Mineral);
        }
    }

    void OnClickAction4()
    {
        if (canvasMode != CanvasMode.Build)
        {
            SetMode(CanvasMode.Build);
        }
    }

    void OnClickAction1()
    {
        if (canvasMode != CanvasMode.Select)
        {
            SetMode(CanvasMode.Select);
        }
    }

    private Button GetButton(int btn)
    {
        if (btn == 0 || btn > 12)
        {
            return null;
        }
        return buttons[btn - 1];
    }

    private Button GetAction(int btn)
    {
        if (btn == 0 || btn > 12)
        {
            return null;
        }
        return actions[btn - 1];
    }

    private Text GetButtonText(int btn)
    {
        return buttonText[btn - 1];
    }

    private Text GetActionText(int btn)
    {
        return actionText[btn - 1];
    }

    private void UnselectUnitFrame()
    {
        if (selectedUnitFrame != null)
        {
            if (selectedUnitFrame.Temporary && selectedUnitFrame.CurrentPos != null)
            {
                Destroy(selectedUnitFrame.gameObject);
            }
            else
            {
                selectedUnitFrame.SetSelected(false);
            }

            selectedUnitFrame = null;
        }
        selectedBuildBlueprint = null;
        buildButton.gameObject.SetActive(false);
    }



    private void SelectBlueprint(string bluePrintname)
    {

        Blueprint blueprint = HexGrid.game.Blueprints.FindBlueprint(bluePrintname);
        if (blueprint == null)
        {
            selectedBuildBlueprint = null;
        }
        else
        {
            /*
            if (IsAssemblerAt() ||
                IsContainerAt())
            {
                selectedBuildBlueprint = null;

                // Can build only moveable units
                foreach (BlueprintPart blueprintPart in blueprint.Parts)
                {
                    if (blueprintPart.PartType.StartsWith("Engine"))
                    {
                        selectedBuildBlueprint = blueprint;
                        break;
                    }
                }
            }
            else
            {*/
            UnselectUnitFrame();
            SelectUnitFrame(HexGrid.CreateTempUnit(blueprint));
            selectedBuildBlueprint = blueprint;
        }
    }

    private void SetActionText(int btn, string text)
    {
        ShowAction(btn);
        GetActionText(btn).text = text;
    }

    private void SetButtonText(int btn, string text, string bluePrintName = null)
    {
        ShowButton(btn);
        Button button = GetButton(btn);

        GetButtonText(btn).text = text;

        if (bluePrintName != null)
        {
            button.name = bluePrintName;
        }
        else
        {
            button.name = "Button" + btn;
        }
    }

    private void HideButton(int btn)
    {
        GetButton(btn).gameObject.SetActive(false);
    }
    private void HideAction(int btn)
    {
        GetAction(btn).gameObject.SetActive(false);
    }

    private void SelectAction(int btn)
    {
        Button button = GetAction(btn);
        button.image.sprite = SelectedButtonBackground;
    }
    private void UnselectAction(int btn)
    {
        GetAction(btn).image.sprite = ButtonBackground;
    }

    private void SelectButton(int btn)
    {
        Button button = GetButton(btn);
        button.image.sprite = SelectedButtonBackground;

        SelectBlueprint(button.name);
    }
    private void UnselectButton(int btn)
    {
        GetButton(btn).image.sprite = ButtonBackground;
    }

    private void ShowAction(int btn)
    {
        GetAction(btn).gameObject.SetActive(true);
    }
    private void ShowButton(int btn)
    {
        GetButton(btn).gameObject.SetActive(true);
    }

    private int topSelectedSelectButton;
    private int topSelectedAttackButton;
    private int topSelectedMineralsButton;
    private int topSelectedBuildButton;
    private int middleSelectedButton;


    private bool IsAssemblerAt()
    {
        if (selectedUnitFrame != null &&
            selectedUnitFrame.IsAssembler())
            return true;

        // Units with assembler can build
        /*
        if (groundCell != null &&
            groundCell.Tile.Unit != null)
        {
            if (groundCell.Tile.Unit.Assembler == null)
                return false;

            return true;

        }*/
        return false;
    }

    private bool IsUnitAt()
    {
        if (selectedUnitFrame != null)
            return true;

        // Units with assembler can build
        /*
        if (groundCell != null &&
            groundCell.Tile.Unit != null)
        {
            return true;

        }*/
        return false;
    }

    private bool IsContainerAt()
    {
        if (selectedUnitFrame != null &&
            selectedUnitFrame.IsContainer())
            return true;

        /*
        // Units with assembler can build
        if (groundCell != null &&
            groundCell.Tile.Unit != null)
        {
            if (groundCell.Tile.Unit.Container == null)
                return false;

            return true;

        }*/
        return false;
    }

    private bool CanBuildAt(GroundCell groundCell)
    {
        if (groundCell == null ||
            groundCell.Tile.NumberOfDestructables > 0 ||
            groundCell.Tile.NumberOfObstacles > 0)
        {
            return false;
        }
        return true;
    }

    private void UpdateCommandButtons()
    {
        if (canvasMode == CanvasMode.Select)
        {
            SetButtonText(1, "(q) Extract");
            HideButton(2);
            HideButton(3);
            HideButton(4);
            HideButton(5);
            HideButton(6);
            HideButton(7);
            HideButton(8);
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);
        }

        if (canvasMode == CanvasMode.Attack)
        {
            SetButtonText(1, "(q) Attack");
            SetButtonText(2, "(w) Defend");
            SetButtonText(3, "(e) Scout");
            HideButton(4);
            HideButton(5);
            HideButton(6);
            HideButton(7);
            HideButton(8);
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);
        }

        if (canvasMode == CanvasMode.Mineral)
        {
            SetButtonText(1, "(q) Collect");
            HideButton(2);
            HideButton(3);
            HideButton(4);
            HideButton(5);
            HideButton(6);
            HideButton(7);
            HideButton(8);
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);
        }
        /*
        if (IsAssemblerAt())
        {
            SetButtonText(1, "(q) Unit");
            SetButtonText(2, "(w) Waypoint");
            HideButton(3);

            if (selectedUnitFrame.MoveUpdateStats.MarkedForExtraction)
                HideButton(4);
            else
                SetButtonText(4, "(r) Extract");
        }
        else if (IsContainerAt())
        {
            HideButton(1);
            SetButtonText(2, "(w) Waypoint");
            HideButton(3);
            if (selectedUnitFrame.MoveUpdateStats.MarkedForExtraction)
                HideButton(4);
            else
                SetButtonText(4, "(r) Extract");
        }
        else if (IsUnitAt())
        {
            HideButton(1);
            HideButton(2);
            HideButton(3);
            if (selectedUnitFrame.MoveUpdateStats.MarkedForExtraction)
                HideButton(4);
            else
                SetButtonText(4, "(r) Extract");
            HideButton(5);
            HideButton(6);
            HideButton(7);
            HideButton(8);
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);
            return;
        }
        else if (!CanBuildAt(lastSelectedGroundCell))
        {
            HideButton(1);
            HideButton(2);
            HideButton(3);
            HideButton(4);
            HideButton(5);
            HideButton(6);
            HideButton(7);
            HideButton(8);
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);
            return;
        }
        else
        { 
        */
        if (canvasMode == CanvasMode.Build)
        {
            SetButtonText(1, "(q) Building");
            SetButtonText(2, "(w) Defense");
            SetButtonText(3, "(e) Special");
            HideButton(4);

            if (topSelectedBuildButton == 0)
            {
                HideButton(5);
                HideButton(6);
                HideButton(7);
                HideButton(8);
                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);
            }
            else if (topSelectedBuildButton == 1)
            {
                SetButtonText(5, "(a) Factory", "Factory");
                SetButtonText(6, "(s) Container", "Container");
                SetButtonText(7, "(d) Reactor", "Reactor");
                SetButtonText(8, "(f) Radar");

                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);

                if (middleSelectedButton != 0)
                    SelectButton(middleSelectedButton);
            }
            else if (topSelectedBuildButton == 2)
            {
                SetButtonText(5, "(a) Turret", "Turret");
                HideButton(6);
                HideButton(7);
                HideButton(8);

                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);

                if (middleSelectedButton != 0)
                    SelectButton(middleSelectedButton);
            }
            else if (topSelectedBuildButton == 3)
            {
                SetButtonText(5, "(a) Outpost", "Outpost");
                HideButton(6);
                HideButton(7);
                HideButton(8);

                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);

                if (middleSelectedButton != 0)
                    SelectButton(middleSelectedButton);
            }
        }
    }

    void UnSelectTopButton()
    {
        if (topSelectedSelectButton != 0)
        {
            UnselectButton(topSelectedSelectButton);
            //topSelectedSelectButton = 0;
        }
        if (topSelectedAttackButton != 0)
        {
            UnselectButton(topSelectedAttackButton);
            //topSelectedAttackButton = 0;
        }
        if (topSelectedMineralsButton != 0)
        {
            UnselectButton(topSelectedMineralsButton);
            //topSelectedMineralsButton = 0;
        }
        if (topSelectedBuildButton != 0)
        {
            UnselectButton(topSelectedBuildButton);
            //topSelectedBuildButton = 0;
        }
    }

    void SelectTopButton(int btn)
    {
        if (!GetButton(btn).IsActive())
            return;

        UnSelectTopButton();

        if (middleSelectedButton != 0)
        {
            UnselectButton(middleSelectedButton);
            middleSelectedButton = 0;
        }
        UnselectUnitFrame();
        SelectButton(btn);

        if (canvasMode == CanvasMode.Select)
            topSelectedSelectButton = btn;
        if (canvasMode == CanvasMode.Attack)
            topSelectedAttackButton = btn;
        if (canvasMode == CanvasMode.Mineral)
            topSelectedMineralsButton = btn;
        if (canvasMode == CanvasMode.Build)
            topSelectedBuildButton = btn;
    }

    void SelectMiddleButton(int btn)
    {
        if (!GetButton(btn).IsActive())
            return;

        if (middleSelectedButton != 0 && middleSelectedButton != btn)
            UnselectButton(middleSelectedButton);
        middleSelectedButton = btn;
        SelectButton(btn);
    }


    //private GameCommandType nextGameCommandAtClick = GameCommandType.None;
    void OnClickExtract()
    {
        if (HexGrid.UnitsInBuild.ContainsKey(selectedUnitFrame.CurrentPos))
        {
            if (HexGrid.UnitsInBuild[selectedUnitFrame.CurrentPos] != null)
                HexGrid.UnitsInBuild[selectedUnitFrame.CurrentPos].Delete();
            HexGrid.UnitsInBuild.Remove(selectedUnitFrame.CurrentPos);
        }

        // Extract the unit
        GameCommand gameCommand = new GameCommand();

        gameCommand.UnitId = selectedUnitFrame.UnitId;
        gameCommand.TargetPosition = selectedUnitFrame.CurrentPos;
        gameCommand.GameCommandType = GameCommandType.Extract;
        gameCommand.PlayerId = 1;
        HexGrid.GameCommands.Add(gameCommand);

        selectedUnitFrame.MoveUpdateStats.MarkedForExtraction = true;
    }

    void OnClickBuild1()
    {
        SelectTopButton(1);
        UpdateCommandButtons();
    }
    void OnClickBuild2()
    {
        SelectTopButton(2);
        UpdateCommandButtons();
    }
    void OnClickBuild3()
    {
        SelectTopButton(3);
        UpdateCommandButtons();
    }
    void OnClickBuild4()
    {
        SelectTopButton(4);
        UpdateCommandButtons();
    }

    void OnClickBuild5()
    {
        SelectMiddleButton(5);
        UpdateCommandButtons();
    }
    void OnClickBuild6()
    {
        SelectMiddleButton(6);
        UpdateCommandButtons();
    }
    void OnClickBuild7()
    {
        SelectMiddleButton(7);
        UpdateCommandButtons();
    }
    void OnClickBuild8()
    {
        SelectMiddleButton(8);
        UpdateCommandButtons();
    }

    void OnClickBuild9()
    {
        SelectMiddleButton(9);
    }
    void OnClickBuild10()
    {
        SelectMiddleButton(10);
    }
    void OnClickBuild11()
    {
        SelectMiddleButton(11);
    }
    void OnClickBuild12()
    {
        SelectMiddleButton(12);
    }


    private UnitBase selectedUnitFrame;
    private Blueprint selectedBuildBlueprint;
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
            else if (hitByMouseClick.UnitFrame == null && hitByMouseClick.GroundCell != null)
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

        if (raycastHit.collider.transform.parent != null)
        {
            unitBase = raycastHit.collider.transform.parent.GetComponent<UnitBase>();
            if (unitBase != null) return unitBase;
        }
        return null;
    }

    private void AppendGroundInfo(GroundCell gc)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("Position: " + gc.Tile.Pos.X + ", " + gc.Tile.Pos.Y);
        if (gc.Tile.Metal > 0)
            sb.Append(" Minerals: " + gc.Tile.Metal);

        sb.Append(" Owner: " + gc.Tile.Owner);

        if (selectedUnitFrame == null)
        {
            if (gc.Tile.NumberOfDestructables > 0)
            {
                headerText.text = "Destructable";
            }
            else if (gc.Tile.NumberOfObstacles > 0)
            {
                headerText.text = "Obstacle";
            }
            else
            {
                headerText.text = "Ground";
            }
            headerSubText.text = sb.ToString();

            sb.Clear();
            if (gc.Tile.NumberOfDestructables > 0)
                sb.Append("Items: " + gc.Tile.NumberOfDestructables);
            if (gc.Tile.NumberOfObstacles > 0)
                sb.Append("Obstacles: " + gc.Tile.NumberOfObstacles);

            headerGroundText.text = sb.ToString();
        }
        else
        {
            headerGroundText.text = sb.ToString();
        }

        /*
        if (gc.UnitCommands != null)
        {
            foreach (UnitCommand unitCommand in gc.UnitCommands)
            {
                sb.AppendLine("Command: " + unitCommand.GameCommand.ToString() + " Owner: " + unitCommand.Owner.UnitId);
            }
        }*/

    }

    private void HideAllParts()
    {
        panelEngine.SetActive(false);
        panelExtractor.SetActive(false);
        panelContainer.SetActive(false);
        panelAssembler.SetActive(false);
        panelArmor.SetActive(false);
        panelWeapon.SetActive(false);
        panelReactor.SetActive(false);
    }


    private void SelectMouseClick(HitByMouseClick hitByMouseClick)
    {
        if (hitByMouseClick.UnitFrame != null && hitByMouseClick.GroundCell == null)
        {
            //hitByMouseClick.GroundCell = HexGrid.GroundCells[hitByMouseClick.UnitFrame.CurrentPos];
        }
        if (lastSelectedGroundCell != hitByMouseClick.GroundCell)
        {
            if (lastSelectedGroundCell != null)
                lastSelectedGroundCell.SetSelected(false);
            if (hitByMouseClick.GroundCell != null)
                hitByMouseClick.GroundCell.SetSelected(true);
            lastSelectedGroundCell = hitByMouseClick.GroundCell;
        }

        if (selectedUnitFrame != hitByMouseClick.UnitFrame)
        {
            UnselectUnitFrame();
            SelectUnitFrame(hitByMouseClick.UnitFrame);
        }
        //if (selectedUnitFrame == null)
        {
            UpdateCommandButtons();
        }
    }

    private void SelectUnitFrame(UnitBase unitBase)
    {
        if (unitBase != null)
        {
            if (unitBase.Temporary)
            {
                buildButton.gameObject.SetActive(false);
            }
            else
            {
                buildButton.gameObject.SetActive(true);
            }
            selectedUnitFrame = unitBase;
            selectedUnitFrame.SetSelected(true);
        }
    }

    /*
    private void UnselectButtons()
    {
        if (topSelectedAttackButton != 0)
        {
            UnselectButton(topSelectedAttackButton);
            topSelectedAttackButton = 0;
        }
        if (middleSelectedButton != 0)
        {
            UnselectButton(middleSelectedButton);
            middleSelectedButton = 0;
        }
        UpdateCommandButtons();
    }*/

    private UnitBase unitGroup1;
    private UnitBase unitGroup2;

    // Update is called once per frame
    void Update()
    {
        ExecuteHotkeys();

        if (canvasMode == CanvasMode.Select)
        {
            UpdateSelectMode();
        }
        if (canvasMode == CanvasMode.Build)
        {
            UpdateBuildMode();
        }
        if (canvasMode == CanvasMode.Attack || canvasMode == CanvasMode.Mineral)
        {
            UpdateAttackMode();
        }
    }

    private bool leftMouseButtonDown;

    void UpdateAttackMode()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SetMode(CanvasMode.Select);

            if (lastSelectedGroundCell != null)
            {
                lastSelectedGroundCell.SetSelected(false);
                lastSelectedGroundCell = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            leftMouseButtonDown = true;
            lastSelectedGroundCell = null;
        }
        if (Input.GetMouseButtonUp(0))
        {
            leftMouseButtonDown = false;
        }

        

        HitByMouseClick hitByMouseClick = GetClickedInfo();

        GroundCell selectGroundCell = null;
        if (hitByMouseClick != null)
        {
            selectGroundCell = hitByMouseClick.GroundCell;
        }
        if (lastSelectedGroundCell != selectGroundCell)
        {
            if (lastSelectedGroundCell != null)
                lastSelectedGroundCell.SetSelected(false);
            lastSelectedGroundCell = selectGroundCell;

            if (leftMouseButtonDown &&
                lastSelectedGroundCell != null &&
                CanBuildAt(lastSelectedGroundCell))
            {
                Position pos = lastSelectedGroundCell.Tile.Pos;
                foreach (GameCommand gameCommand1 in HexGrid.GameCommands)
                {
                    if (gameCommand1.TargetPosition == pos)
                    {
                        HexGrid.GameCommands.Remove(gameCommand1);
                        break;
                    }
                }

                if (HexGrid.UnitsInBuild.ContainsKey(pos))
                {
                    // Remove it
                    HexGrid.UnitsInBuild.Remove(pos);

                    // Cancel the command
                    GameCommand gameCommand = new GameCommand();
                    gameCommand.TargetPosition = pos;
                    gameCommand.GameCommandType = GameCommandType.Cancel;
                    gameCommand.PlayerId = 1;
                    HexGrid.GameCommands.Add(gameCommand);

                    lastSelectedGroundCell.SetAttack(false);

                }
                else
                {
                    // Build the temp. unit
                    GameCommand gameCommand = new GameCommand();
                    //gameCommand.UnitId;
                    gameCommand.TargetPosition = pos;

                    if (canvasMode == CanvasMode.Attack)
                        gameCommand.GameCommandType = GameCommandType.Attack;
                    if (canvasMode == CanvasMode.Mineral)
                        gameCommand.GameCommandType = GameCommandType.Minerals;
                    gameCommand.PlayerId = 1;
                    HexGrid.GameCommands.Add(gameCommand);

                    HexGrid.UnitsInBuild.Add(pos, null);

                    lastSelectedGroundCell.SetAttack(true);
                }
            }
        }
    }
    

    void UpdateBuildMode()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SetMode(CanvasMode.Select);

            if (lastSelectedGroundCell != null)
            {
                lastSelectedGroundCell.SetSelected(false);
                lastSelectedGroundCell = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            leftMouseButtonDown = true;
            lastSelectedGroundCell = null;
        }
        if (Input.GetMouseButtonUp(0))
        {
            leftMouseButtonDown = false;
        }

        HitByMouseClick hitByMouseClick = GetClickedInfo();

        GroundCell selectGroundCell = null;
        if (hitByMouseClick != null)
        {
            selectGroundCell = hitByMouseClick.GroundCell;
        }
        if (lastSelectedGroundCell != selectGroundCell)
        {
            if (lastSelectedGroundCell != null)
                lastSelectedGroundCell.SetSelected(false);
            lastSelectedGroundCell = selectGroundCell;

            if (leftMouseButtonDown &&
                selectedUnitFrame != null &&
                lastSelectedGroundCell != null && 
                CanBuildAt(lastSelectedGroundCell))
            {
                //lastSelectedGroundCell.SetSelected(true);

                Position pos = lastSelectedGroundCell.Tile.Pos;
                selectedUnitFrame.CurrentPos = pos;

                foreach (GameCommand gameCommand1 in HexGrid.GameCommands)
                {
                    if (gameCommand1.TargetPosition == pos)
                    {
                        HexGrid.GameCommands.Remove(gameCommand1);
                        break;
                    }
                }

                if (HexGrid.UnitsInBuild.ContainsKey(pos))
                {
                    // Remove it
                    if (HexGrid.UnitsInBuild[pos] != null)
                        HexGrid.UnitsInBuild[pos].Delete();
                    HexGrid.UnitsInBuild.Remove(pos);

                    // Cancel the command
                    GameCommand gameCommand = new GameCommand();
                    gameCommand.UnitId = selectedUnitFrame.MoveUpdateStats.BlueprintName;
                    gameCommand.TargetPosition = pos;
                    gameCommand.GameCommandType = GameCommandType.Cancel;
                    gameCommand.PlayerId = 1;
                    HexGrid.GameCommands.Add(gameCommand);
                }
                else
                {

                    // Build the temp. unit
                    GameCommand gameCommand = new GameCommand();
                    gameCommand.UnitId = selectedUnitFrame.MoveUpdateStats.BlueprintName;
                    gameCommand.TargetPosition = pos;
                    gameCommand.GameCommandType = GameCommandType.Build;
                    gameCommand.PlayerId = 1;
                    HexGrid.GameCommands.Add(gameCommand);

                    HexGrid.UnitsInBuild.Add(pos, selectedUnitFrame);

                    // Turn the temp unit into ghost
                    selectedUnitFrame.Temporary = false;
                    selectedUnitFrame.PutAtCurrentPosition(false);
                    selectedUnitFrame.Assemble(true);
                    selectedUnitFrame.gameObject.SetActive(true);

                    // Prepare next
                    if (selectedBuildBlueprint == null)
                    {
                        int x = 0;
                    }
                    else
                    {
                        selectedUnitFrame = HexGrid.CreateTempUnit(selectedBuildBlueprint);
                    }
                }
            }

        }
    }

    void ExecuteHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            OnClickAction1();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            OnClickAction2();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            OnClickAction3();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnClickBuild1();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnClickBuild2();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnClickBuild3();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnClickBuild4();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            OnClickBuild5();
            //OnClickBuild();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            OnClickBuild6();
            //OnClickBuild();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnClickBuild7();
            //OnClickBuild();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnClickBuild8();
            //OnClickBuild();
        }
    }

    void UpdateSelectMode()
    {

        if (HexGrid != null && HexGrid.MapInfo != null)
        {
            if (HexGrid.MapInfo.PlayerInfo.ContainsKey(1))
            {
                MapPlayerInfo mapPlayerInfo = HexGrid.MapInfo.PlayerInfo[1];
                //UIMineralText.text = mapPlayerInfo.TotalMetal + " / " + mapPlayerInfo.TotalCapacity;
                UIMineralText.text = mapPlayerInfo.TotalMetal + " / " + mapPlayerInfo.TotalCapacity + " " + HexGrid.MapInfo.TotalMetal.ToString();
                UIUnitText.text = mapPlayerInfo.TotalUnits.ToString();
                UIPowerText.text = mapPlayerInfo.TotalPower.ToString();
            }
            else
            {
                UIMineralText.text = "Dead" + HexGrid.MapInfo.TotalMetal.ToString(); ;
            }
        }

        bool ctrl = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (ctrl)
            {            
                if (selectedUnitFrame != null)
                    unitGroup1 = selectedUnitFrame;
            }
            else if (unitGroup1 != null)
            {
                HitByMouseClick hitByMouseClick = new HitByMouseClick();
                hitByMouseClick.UnitFrame = unitGroup1;
                SelectMouseClick(hitByMouseClick);
                UpdateCommandButtons();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (ctrl)
            {
                if (selectedUnitFrame != null)
                    unitGroup2 = selectedUnitFrame;
            }
            else if (unitGroup2 != null)
            {
                HitByMouseClick hitByMouseClick = new HitByMouseClick();
                hitByMouseClick.UnitFrame = unitGroup2;
                SelectMouseClick(hitByMouseClick);
                UpdateCommandButtons();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SelectedBluePrint = null;
            UpdateCommandButtons();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetMode(CanvasMode.Select);

            if (lastSelectedGroundCell != null)
            {
                lastSelectedGroundCell.SetSelected(false);
                lastSelectedGroundCell = null;
            }
            UnselectUnitFrame();
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
                SelectMouseClick(hitByMouseClick);

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
                    UnselectUnitFrame();
                    SelectUnitFrame(hitByMouseClick.UnitFrame);

                }
            }
        }
        if (selectedUnitFrame != null)
        {
            UnitBase unit = selectedUnitFrame;

            if (unit.HasBeenDestroyed)
            {
                UnselectUnitFrame();
            }
            if (unit.MoveUpdateStats == null)
            {
            }
            else
            {
                if (unit.MoveUpdateStats != null)
                {
                    HideAllParts();

                    headerText.text = unit.MoveUpdateStats.BlueprintName;

                    if (unit.HasBeenDestroyed)
                    {
                        headerSubText.text = "Destroyed";
                    }
                    else if (unit.Temporary)
                    {
                        headerSubText.text = "Preview";
                    }
                    else if (unit.UnderConstruction)
                    {
                        headerSubText.text = "Under construction";
                    }
                    else if (unit.MoveUpdateStats.MarkedForExtraction)
                    {
                        headerSubText.text = "MarkedForExtraction";
                    }

                    else
                    {
                        headerSubText.text = "";
                    }
                    headerSubText.text += " " + unit.UnitId;

                    headerSubText.text += " Power: " + unit.MoveUpdateStats.Power;

                    string state;

                    foreach (MoveUpdateUnitPart part in unit.MoveUpdateStats.UnitParts)
                    {
                        if (!part.Exists && !unit.Temporary)
                        {
                            state = " Missing";
                        }
                        else
                        {
                            state = "";
                        }
                        if (part.PartType.StartsWith("Extractor"))
                        {
                            panelExtractor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelExtractor.SetActive(true);
                        }
                        if (part.PartType.StartsWith("Weapon"))
                        {
                            panelWeapon.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelWeapon.SetActive(true);

                            StringBuilder sb = new StringBuilder();
                            sb.Append("Ammunition  ");
                            if (part.Minerals.HasValue)
                                sb.Append(part.Minerals.Value);
                            else
                                sb.Append("0");

                            if (part.Capacity.HasValue)
                                sb.Append("/" + part.Capacity.Value);

                            panelWeapon.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                        }
                        if (part.PartType.StartsWith("Assembler"))
                        {
                            panelAssembler.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelAssembler.SetActive(true);

                            StringBuilder sb = new StringBuilder();
                            sb.Append("Minerals  ");
                            if (part.Minerals.HasValue)
                                sb.Append(part.Minerals.Value);
                            else
                                sb.Append("0");

                            if (part.Capacity.HasValue)
                                sb.Append("/" + part.Capacity.Value);

                            panelAssembler.transform.Find("Content").GetComponent<Text>().text = sb.ToString();

                            sb = new StringBuilder();
                            if (part.BildQueue != null)
                            {
                                foreach (string b in part.BildQueue)
                                {
                                    if (sb.Length > 0) sb.Append(" ");
                                    sb.Append(b);
                                }
                            }
                            panelAssembler.transform.Find("BuildQueue").GetComponent<Text>().text = sb.ToString();

                        }
                        if (part.PartType.StartsWith("Reactor"))
                        {
                            panelReactor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelReactor.SetActive(true);

                            StringBuilder sb = new StringBuilder();
                            sb.Append("Minerals  ");
                            if (part.Minerals.HasValue)
                                sb.Append(part.Minerals.Value);
                            else
                                sb.Append("0");

                            if (part.Capacity.HasValue)
                                sb.Append("/" + part.Capacity.Value);

                            sb.Append(" Power  ");
                            if (part.AvailablePower.HasValue)
                                sb.Append(part.AvailablePower.Value);
                            else
                                sb.Append("0");
                            panelReactor.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                            
                        }
                        if (part.PartType.StartsWith("Armor"))
                        {
                            panelArmor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelArmor.SetActive(true);

                            StringBuilder sb = new StringBuilder();
                            sb.Append("Power  ");
                            if (part.ShieldPower.HasValue)
                                sb.Append(part.ShieldPower.Value);
                            else
                                sb.Append("0");

                            if (part.ShieldActive == true)
                                sb.Append(" Active");

                            panelArmor.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                        }
                        if (part.PartType.StartsWith("Engine"))
                        {
                            panelEngine.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelEngine.SetActive(true);
                        }
                        if (part.PartType.StartsWith("Container"))
                        {
                            panelContainer.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelContainer.SetActive(true);

                            StringBuilder sb = new StringBuilder();
                            sb.Append("Minerals  ");
                            if (part.Minerals.HasValue)
                                sb.Append(part.Minerals.Value);
                            else
                                sb.Append("0");

                            if (part.Capacity.HasValue)
                                sb.Append("/" + part.Capacity.Value);

                            panelContainer.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                        }
                    }
                }
            }

            if (unit.CurrentPos != null)
            {
                GroundCell gc = HexGrid.GroundCells[unit.CurrentPos];
                AppendGroundInfo(gc);
            }
        }
        else if (lastSelectedGroundCell != null)
        {
            if (selectedUnitFrame == null)
            {
                headerText.text = "";
                HideAllParts();
            }
            GroundCell gc = lastSelectedGroundCell;

            AppendGroundInfo(gc);
        }
        else
        {
            headerText.text = "";
            headerSubText.text = "";
            HideAllParts();
        }

    }
}
