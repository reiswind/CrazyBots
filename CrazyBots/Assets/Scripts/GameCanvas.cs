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
        UnselectUnitFrame();

        Blueprint blueprint = HexGrid.game.Blueprints.FindBlueprint(bluePrintname);
        if (blueprint == null)
        {
            
        }
        else
        {
            SelectUnitFrame(HexGrid.CreateTempUnit(blueprint));

            if (lastSelectedGroundCell != null)
            {
                selectedUnitFrame.CurrentPos = lastSelectedGroundCell.Tile.Pos;
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
        else if (selectedUnitFrame == null || selectedUnitFrame.Temporary)
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
                    SetButtonText(10, "(x) Container3", "Container");
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
                    SetButtonText(9, "(y) Shield", "Assembler");
                    SetButtonText(10, "(x) Container");
                    SetButtonText(11, "(c) Engine2");
                    HideButton(12);
                }
                else if (middleSelectedButton == 6)
                {
                    SetButtonText(9, "(y) Shield", "Fighter");
                    SetButtonText(10, "(x) Weapon2");
                    SetButtonText(11, "(c) Engine2");
                    HideButton(12);
                }
                else if (middleSelectedButton == 7)
                {
                    SetButtonText(9, "(y) Shield", "Worker");
                    SetButtonText(10, "(x) Container2");
                    SetButtonText(11, "(c) Engine2");
                    HideButton(12);
                }
                if (lowSelectedButton != 0)
                    SelectButton(lowSelectedButton);

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
        else
        {
            ShowButton(1);
            SetButtonText(1, selectedUnitFrame.name);
            for (int i = 2; i <= 12; i++)
            {
                HideButton(i);
            }
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

    void OnClickBuild()
    {
        if (selectedUnitFrame == null)
        {
            return;
        }
        if (selectedUnitFrame.Temporary)
        {
            if (HexGrid.UnitsInBuild.ContainsKey(selectedUnitFrame.CurrentPos))
            {
                // Already used
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

            buildButton.gameObject.SetActive(false);
            UpdateCommandButtons();
        }
        else
        {
            // Build the temp. unit
            GameCommand gameCommand = new GameCommand();

            gameCommand.UnitId = selectedUnitFrame.UnitId;
            gameCommand.TargetPosition = selectedUnitFrame.CurrentPos;
            gameCommand.GameCommandType = GameCommandType.Extract;
            gameCommand.PlayerId = 1;
            HexGrid.GameCommands.Add(gameCommand);

            selectedUnitFrame.MoveUpdateStats.MarkedForExtraction = true;
            buildButton.gameObject.SetActive(false);
        }
    }

    void OnClickBuild1()
    {
        if (selectedUnitFrame == null || selectedUnitFrame.Temporary)
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
        SelectLowButton(9);
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


        if (gc.UnitCommands != null)
        {
            foreach (UnitCommand unitCommand in gc.UnitCommands)
            {
                sb.AppendLine("Command: " + unitCommand.GameCommand.ToString() + " Owner: " + unitCommand.Owner.UnitId);
            }
        }

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
                        UnselectUnitFrame();
                        SelectUnitFrame(hitByMouseClick.UnitFrame);
                        /*
                        if (hitByMouseClick.UnitFrame != null)
                            hitByMouseClick.UnitFrame.SetSelected(true);
                        selectedUnitFrame = hitByMouseClick.UnitFrame;*/
                    }

                    //if (selectedUnitFrame != null || lastSelectedGroundCell != null)
                    {
                        UpdateCommandButtons();
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
                        if (part.PartType == "Extractor")
                        {
                            panelExtractor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelExtractor.SetActive(true);
                        }
                        if (part.PartType == "Weapon")
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
                        if (part.PartType == "Assembler")
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
                        if (part.PartType == "Reactor")
                        {
                            panelReactor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelReactor.SetActive(true);
                        }
                        if (part.PartType == "Armor")
                        {
                            panelArmor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelArmor.SetActive(true);
                        }
                        if (part.PartType == "Engine")
                        {
                            panelEngine.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelEngine.SetActive(true);
                        }
                        if (part.PartType == "Container")
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
