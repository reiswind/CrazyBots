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
    Build
}

public class GameCanvas : MonoBehaviour
{
    public GameObject MineralText;
    public GameObject UnitText;
    public GameObject PowerText;
    public HexGrid HexGrid;

    public Texture2D NormalCursor;
    public Texture2D BuildCursor;

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
        buildButton.onClick.AddListener(OnClickBuild);
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
        for (int i = 0; i < 1; i++)
        {
            actions[i] = panelSelected.Find("Action" + (i + 1).ToString()).GetComponent<Button>();
            actionText[i] = actions[i].transform.Find("Text").GetComponent<Text>();
            actions[i].name = "Action" + (i + 1).ToString();
        }


        buttons = new Button[12];
        buttonText = new Text[12];
        for (int i = 0; i < 12; i++)
        {
            buttons[i] = panelSelected.Find("Button" + (i + 1).ToString()).GetComponent<Button>();
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

        UpdateCommandButtons();

        SetMode(CanvasMode.Select);
        HexGrid.StartGame();
    }

    void SetMode(CanvasMode newCanvasMode)
    {
        if (canvasMode != newCanvasMode)
        {
            canvasMode = newCanvasMode;
            if (canvasMode == CanvasMode.Select)
            {
                SetActionText(1, "Build");
                Cursor.SetCursor(NormalCursor, new Vector2(0, 0), CursorMode.Auto);
            }
            if (canvasMode == CanvasMode.Build)
            {
                SetActionText(1, "Select");
                Cursor.SetCursor(BuildCursor, new Vector2(0, 0), CursorMode.Auto);
            }
            UpdateCommandButtons();
        }
        leftMouseButtonDown = false;

    }

    void OnClickAction1()
    {
        if (canvasMode == CanvasMode.Build)
        {
            SetMode(CanvasMode.Select);
        }
        else if (canvasMode == CanvasMode.Select)
        {
            SetMode(CanvasMode.Build);
        }
    }

    private Button GetButton(int btn)
    {
        if (btn == 0 || btn > 12)
        {
            return null;
        }
        return buttons[btn-1];
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
        return buttonText[btn-1];
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
        makeWaypointFromHereToNextClick = null;
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
        Button button = GetAction(btn);

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

    private int topSelectedButton;
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

        /*
        // Units with assembler can build
        if (groundCell.Tile.Unit != null)
        {
            // MAy go away
            //if (groundCell.Tile.Unit.Assembler == null)
            //    return false;

            return true;

        }

        foreach (Position pos in HexGrid.UnitsInBuild.Keys)
        {
            if (pos == groundCell.Tile.Pos)
                return false;
        }

        // Is there a ghost that has not been build?
        foreach (UnitBase unitBase in HexGrid.BaseUnits.Values)
        {
            if (unitBase.CurrentPos == groundCell.Tile.Pos)
                return false;
        } 
        */
        return true;
    }

    private void UpdateCommandButtons()
    {
        
        if (canvasMode == CanvasMode.Select)
        {
            SetButtonText(1, "(q) Unit");
            //HideButton(1);
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

            if (topSelectedButton == 0)
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
            else if (topSelectedButton == 1)
            {
                SetButtonText(5, "(a) Attack");
                SetButtonText(6, "(s) Defend");
                SetButtonText(7, "(d) Scout");
                HideButton(8);
                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);

                if (middleSelectedButton != 0)
                    SelectButton(middleSelectedButton);
            }
            return;
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
        SetButtonText(1, "(q) Unit");
        SetButtonText(2, "(w) Building");
        SetButtonText(3, "(e) Defense");
        SetButtonText(4, "(r) Special");
        
        if (topSelectedButton == 0)
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
        else if (topSelectedButton == 1)
        {
            SetButtonText(5, "(a) Assembler", "Assembler");
            SetButtonText(6, "(s) Fighter", "Fighter");
            SetButtonText(7, "(d) Worker", "Worker");
            HideButton(8);
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);

            if (middleSelectedButton != 0)
                SelectButton(middleSelectedButton);
        }
        else if (topSelectedButton == 2)
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
        else if (topSelectedButton == 3)
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
        else if (topSelectedButton == 4)
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

    void SelectTopButton(int btn)
    {
        if (!GetButton(btn).IsActive())
            return;

        if (topSelectedButton != 0 && topSelectedButton != btn)
            UnselectButton(topSelectedButton);
        topSelectedButton = btn;
        if (middleSelectedButton != 0)
        {
            UnselectButton(middleSelectedButton);
            middleSelectedButton = 0;
        }
        UnselectUnitFrame();
        SelectButton(btn);
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

    private UnitBase makeWaypointFromHereToNextClick;
    private GameCommandType nextGameCommandAtClick = GameCommandType.None;
    void OnClickBuild()
    {
        /*
        if (selectedUnitFrame != null && selectedUnitFrame.Temporary)
        {
            SetMode(CanvasMode.Build);
        }
        else
        {*/
            if (HexGrid.UnitsInBuild.ContainsKey(selectedUnitFrame.CurrentPos))
            {
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

    void OnExecuteBuild()
    {
        if (IsAssemblerAt())
        {
            if (selectedBuildBlueprint != null)
            {
                GameCommand gameCommand = new GameCommand();

                gameCommand.UnitId = selectedBuildBlueprint.Name;
                gameCommand.TargetPosition = selectedUnitFrame.CurrentPos;
                gameCommand.GameCommandType = GameCommandType.Build;
                gameCommand.PlayerId = 1;
                HexGrid.GameCommands.Add(gameCommand);
            }
            else
            {

                if (topSelectedButton == 2)
                {
                    makeWaypointFromHereToNextClick = selectedUnitFrame;
                    if (middleSelectedButton == 5)
                        nextGameCommandAtClick = GameCommandType.Attack;
                    else if (middleSelectedButton == 6)
                        nextGameCommandAtClick = GameCommandType.Minerals;
                    else if (middleSelectedButton == 7)
                        nextGameCommandAtClick = GameCommandType.Pipeline;
                }
            }
            selectedBuildBlueprint = null;
            UnselectButtons();
            
            return;
        }

        if (!CanBuildAt(lastSelectedGroundCell))
        {
            return;
        }
        if (selectedUnitFrame == null)
        {
            UnselectButtons();
            return;
        }
        if (selectedUnitFrame.Temporary)
        {
            if (HexGrid.UnitsInBuild.ContainsKey(selectedUnitFrame.CurrentPos))
            {
                // Already used
                UnselectButtons();
                return;
            }

            // Build the temp. unit
            GameCommand gameCommand = new GameCommand();

            gameCommand.UnitId = selectedUnitFrame.MoveUpdateStats.BlueprintName;
            gameCommand.TargetPosition = selectedUnitFrame.CurrentPos;
            gameCommand.GameCommandType = GameCommandType.Build;
            gameCommand.PlayerId = 1;
            HexGrid.GameCommands.Add(gameCommand);


            HexGrid.UnitsInBuild.Add(selectedUnitFrame.CurrentPos, selectedUnitFrame);

            // Turn the temp unit into ghost
            selectedUnitFrame.Temporary = false;

            selectedUnitFrame.PutAtCurrentPosition(false);
            selectedUnitFrame.Assemble(true);
            selectedUnitFrame.gameObject.SetActive(true);
        }
        else
        {
            // Extract the unit
            GameCommand gameCommand = new GameCommand();

            gameCommand.UnitId = selectedUnitFrame.UnitId;
            gameCommand.TargetPosition = selectedUnitFrame.CurrentPos;
            gameCommand.GameCommandType = GameCommandType.Extract;
            gameCommand.PlayerId = 1;
            HexGrid.GameCommands.Add(gameCommand);

            selectedUnitFrame.MoveUpdateStats.MarkedForExtraction = true;
        }
        UnselectButtons();
    }

    void OnClickExtract()
    {
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
        //OnClickBuild();
        UpdateCommandButtons();
    }
    void OnClickBuild6()
    {
        SelectMiddleButton(6);
        //OnClickBuild();
        UpdateCommandButtons();
    }
    void OnClickBuild7()
    {
        SelectMiddleButton(7);
        //OnClickBuild();
        UpdateCommandButtons();
    }
    void OnClickBuild8()
    {
        SelectMiddleButton(8);
        //OnClickBuild();
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

            if (hitByMouseClick.UnitFrame == null && hitByMouseClick.GroundCell != null)
            {
                foreach (UnitBase unitFrame in HexGrid.BaseUnits.Values)
                {
                    if (unitFrame.CurrentPos == hitByMouseClick.GroundCell.Tile.Pos)
                    {
                        hitByMouseClick.UnitFrame = unitFrame;
                        hitByMouseClick.GroundCell = null;
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
                //buildButtonText.text = "Build";
                buildButton.gameObject.SetActive(false);
            }
            else
            {
                buildButton.gameObject.SetActive(true);
                //buildButtonText.text = "Extract";
            }
            selectedUnitFrame = unitBase;
            selectedUnitFrame.SetSelected(true);
        }
    }

    private void UnselectButtons()
    {
        if (topSelectedButton != 0)
        {
            UnselectButton(topSelectedButton);
            topSelectedButton = 0;
        }
        if (middleSelectedButton != 0)
        {
            UnselectButton(middleSelectedButton);
            middleSelectedButton = 0;
        }
        UpdateCommandButtons();
    }

    private UnitBase unitGroup1;
    private UnitBase unitGroup2;

    // Update is called once per frame
    void Update()
    {
        if (canvasMode == CanvasMode.Select)
        {
            UpdateSelectMode();
        }
        if (canvasMode == CanvasMode.Build)
        {
            UpdateBuildMode();
        }
    }

    private bool leftMouseButtonDown;

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

        ExecuteHotkeys();

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

                // Build the temp. unit
                GameCommand gameCommand = new GameCommand();
                gameCommand.UnitId = selectedUnitFrame.MoveUpdateStats.BlueprintName;
                gameCommand.TargetPosition = pos;
                gameCommand.GameCommandType = GameCommandType.Build;
                gameCommand.PlayerId = 1;
                HexGrid.GameCommands.Add(gameCommand);

                if (HexGrid.UnitsInBuild.ContainsKey(pos))
                {
                    HexGrid.UnitsInBuild[pos].Delete();
                    HexGrid.UnitsInBuild.Remove(pos);
                }
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

    void ExecuteHotkeys()
    {
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

        ExecuteHotkeys();

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


        

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (selectedUnitFrame != null && selectedUnitFrame.Temporary)
                OnClickBuild();
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
            if (topSelectedButton != 0)
            {
                UnselectButton(topSelectedButton);
                topSelectedButton = 0;
                UpdateCommandButtons();
            }

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
                makeWaypointFromHereToNextClick = null;
            }
            else
            {
                if (makeWaypointFromHereToNextClick != null)
                {
                    // Move it there
                    GameCommand gameCommand = new GameCommand();

                    gameCommand.UnitId = makeWaypointFromHereToNextClick.UnitId;
                    if (hitByMouseClick.GroundCell != null)
                    {
                        //GroundCell groundCell = HexGrid.GroundCells[hitByMouseClick.UnitFrame.CurrentPos];
                        //groundCell.UnitCommands.Add(unitCommand);

                        gameCommand.TargetPosition = hitByMouseClick.GroundCell.Tile.Pos;
                    }
                    else
                    {
                        gameCommand.TargetPosition = selectedUnitFrame.CurrentPos; // Reset
                    }
                    gameCommand.GameCommandType = nextGameCommandAtClick;
                    HexGrid.GameCommands.Add(gameCommand);

                    selectedUnitFrame.ClearWayPoints(nextGameCommandAtClick);

                    UnitCommand unitCommand = new UnitCommand();
                    unitCommand.GameCommand = gameCommand;
                    unitCommand.Owner = makeWaypointFromHereToNextClick;
                    unitCommand.TargetCell = hitByMouseClick.GroundCell;

                    makeWaypointFromHereToNextClick.UnitCommands.Add(unitCommand);
                    makeWaypointFromHereToNextClick.UpdateWayPoints();

                    makeWaypointFromHereToNextClick = null;

                }
                else
                {
                    SelectMouseClick(hitByMouseClick);
                    /*
                    if (!string.IsNullOrEmpty(SelectedBluePrint))
                    {
                        
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
                        }
                        UnselectButtons();
                    }
                    else
                    {
                    */
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
                        
                        //UnselectButtons();

                    
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
