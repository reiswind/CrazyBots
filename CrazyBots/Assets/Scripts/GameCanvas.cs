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
    public HexGrid HexGrid;

    private Text UIMineralText;

    internal string SelectedBluePrint { get; set; }

    public Sprite SelectedButtonBackground;
    public Sprite ButtonBackground;

    private Text[] buttonText;
    private Button[] buttons;
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

        buttons = new Button[12];
        buttonText = new Text[12];
        for (int i = 0; i < 12; i++)
        {
            buttons[i] = panelSelected.Find("Button" + (i+1).ToString()).GetComponent<Button>();
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

        UpdateCommandButtons();

        HexGrid.StartGame();
    }

    private Button GetButton(int btn)
    {
        if (btn == 0 || btn > 12)
        {
            return null;
        }
        return buttons[btn-1];
    }

    private Text GetButtonText(int btn)
    {
        return buttonText[btn-1];
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

    private void SelectUnitFrame(UnitBase unitBase)
    {
        if (unitBase != null)
        {
            if (unitBase.Temporary)
            {
                buildButtonText.text = "Build";
                buildButton.gameObject.SetActive(true);
            }
            else
            {
                buildButton.gameObject.SetActive(true);
                buildButtonText.text = "Extract";
            }
            selectedUnitFrame = unitBase;
            selectedUnitFrame.SetSelected(true);
        }
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
            if (IsAssemblerAt(lastSelectedGroundCell) ||
                IsContainerAt(lastSelectedGroundCell))
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
            {
                UnselectUnitFrame();
                SelectUnitFrame(HexGrid.CreateTempUnit(blueprint));

                if (lastSelectedGroundCell != null)
                {
                    selectedUnitFrame.CurrentPos = lastSelectedGroundCell.Tile.Pos;
                }
            }
        }
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

    private void ShowButton(int btn)
    {
        GetButton(btn).gameObject.SetActive(true);
    }

    private int topSelectedButton;
    private int middleSelectedButton;


    private bool IsAssemblerAt(GroundCell groundCell)
    {
        // Units with assembler can build
        if (groundCell.Tile.Unit != null)
        {
            if (groundCell.Tile.Unit.Assembler == null)
                return false;

            return true;

        }
        return false;
    }

    private bool IsContainerAt(GroundCell groundCell)
    {
        // Units with assembler can build
        if (groundCell.Tile.Unit != null)
        {
            if (groundCell.Tile.Unit.Container == null)
                return false;

            return true;

        }
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

        // Units with assembler can build
        if (groundCell.Tile.Unit != null)
        {
            if (groundCell.Tile.Unit.Assembler == null)
                return false;

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

        return true;
    }

    private void UpdateCommandButtons()
    {
        if (!CanBuildAt(lastSelectedGroundCell))
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

        if (IsAssemblerAt(lastSelectedGroundCell) || IsContainerAt(lastSelectedGroundCell))
        {
            SetButtonText(1, "(q) Unit");
            SetButtonText(2, "(w) Waypoint");
            HideButton(3);
            HideButton(4);
        }
        else
        {
            SetButtonText(1, "(q) Unit");
            SetButtonText(2, "(w) Building");
            SetButtonText(3, "(e) Defense");
            SetButtonText(4, "(r) Special");
        }
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
        }
        else if (topSelectedButton == 2)
        {
            if (IsAssemblerAt(lastSelectedGroundCell) || IsContainerAt(lastSelectedGroundCell))
            {
                SetButtonText(5, "(a) Attack");
                if (IsContainerAt(lastSelectedGroundCell))
                {
                    SetButtonText(6, "(s) Minerals");
                    SetButtonText(7, "(d) Pipeline");
                }
                else
                {
                    HideButton(6);
                    HideButton(7);
                }
                HideButton(8);
            }
            else
            {
                SetButtonText(5, "(a) Assembler");
                SetButtonText(6, "(s) Container", "Container");
                SetButtonText(7, "(d) Reactor", "Reactor");
                SetButtonText(8, "(f) Radar");
            }
            HideButton(9);
            HideButton(10);
            HideButton(11);
            HideButton(12);
            
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
        middleSelectedButton = 0;
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
        if (IsAssemblerAt(lastSelectedGroundCell))
        {
            if (selectedBuildBlueprint != null)
            {
                GameCommand gameCommand = new GameCommand();

                gameCommand.UnitId = selectedBuildBlueprint.Name;
                gameCommand.TargetPosition = lastSelectedGroundCell.Tile.Pos;
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

            selectedUnitFrame.PutAtCurrentPosition();
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
        OnClickBuild();
        UpdateCommandButtons();
    }
    void OnClickBuild6()
    {
        SelectMiddleButton(6);
        OnClickBuild();
        UpdateCommandButtons();
    }
    void OnClickBuild7()
    {
        SelectMiddleButton(7);
        OnClickBuild();
        UpdateCommandButtons();
    }
    void OnClickBuild8()
    {
        SelectMiddleButton(8);
        OnClickBuild();
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

    private void AppendGroundInfo(GroundCell gc)
    {

        StringBuilder sb = new StringBuilder();

        sb.Append("Position: " + gc.Tile.Pos.X + ", " + gc.Tile.Pos.Y);
        if (gc.Tile.Metal > 0)
            sb.Append(" Minerals: " + gc.Tile.Metal);
        if (selectedUnitFrame == null)
        {
            headerText.text = "Ground";
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


    private void SelectUnitFrame(HitByMouseClick hitByMouseClick)
    {
        if (hitByMouseClick.UnitFrame != null && hitByMouseClick.GroundCell == null)
        {
            hitByMouseClick.GroundCell = HexGrid.GroundCells[hitByMouseClick.UnitFrame.CurrentPos];
        }
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
        if (HexGrid != null && HexGrid.MapInfo != null)
        {
            UIMineralText.text = HexGrid.MapInfo.TotalMetal.ToString();
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
                SelectUnitFrame(hitByMouseClick);
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
                SelectUnitFrame(hitByMouseClick);
                UpdateCommandButtons();
            }
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
            OnClickBuild();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            OnClickBuild6();
            OnClickBuild();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            OnClickBuild7();
            OnClickBuild();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnClickBuild8();
            OnClickBuild();
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

            if (lastSelectedGroundCell != null)
            {
                lastSelectedGroundCell.SetSelected(false);
                lastSelectedGroundCell = null;
            }
            UnselectUnitFrame();
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
                makeWaypointFromHereToNextClick = null;
            }
            else
            {
                if (makeWaypointFromHereToNextClick != null)
                {
                    // Move it there
                    GameCommand gameCommand = new GameCommand();

                    gameCommand.UnitId = makeWaypointFromHereToNextClick.UnitId;
                    gameCommand.TargetPosition = hitByMouseClick.GroundCell.Tile.Pos;
                    gameCommand.GameCommandType = nextGameCommandAtClick;

                    //gameCommand.Append = ShifKeyIsDown;
                    HexGrid.GameCommands.Add(gameCommand);

                    //if (!ShifKeyIsDown)
                        selectedUnitFrame.ClearWayPoints(nextGameCommandAtClick);

                    UnitCommand unitCommand = new UnitCommand();
                    unitCommand.GameCommand = gameCommand;
                    unitCommand.Owner = makeWaypointFromHereToNextClick;
                    unitCommand.TargetCell = hitByMouseClick.GroundCell;

                    makeWaypointFromHereToNextClick.UnitCommands.Add(unitCommand);
                    makeWaypointFromHereToNextClick.UpdateWayPoints();

                    hitByMouseClick.GroundCell.UnitCommands.Add(unitCommand);

                    makeWaypointFromHereToNextClick = null;

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
                        UnselectButtons();
                    }
                    else
                    {
                        SelectUnitFrame(hitByMouseClick);
                        /*
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
                        */
                        UnselectButtons();

                    }
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

                        }
                        if (part.PartType.StartsWith("Reactor"))
                        {
                            panelReactor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelReactor.SetActive(true);
                        }
                        if (part.PartType.StartsWith("Armor"))
                        {
                            panelArmor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelArmor.SetActive(true);
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
