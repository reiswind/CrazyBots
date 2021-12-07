using Engine.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameCanvas : MonoBehaviour
    {
        public GameObject MineralText;
        public GameObject UnitText;
        public GameObject PowerText;

        public Texture2D NormalCursor;
        public Texture2D BuildCursor;
        public Texture2D AttackCursor;

        private Text UIMineralText;
        private Text UIUnitText;
        private Text UIPowerText;

        private CanvasMode canvasMode;

        public Sprite SelectedButtonBackground;
        public Sprite ButtonBackground;

        private Text[] buttonText;
        private Text[] actionText;
        private Button[] buttons;
        private Button[] actions;
        private Transform panelParts;
        private GameObject panelEngine;
        private GameObject panelExtractor;
        private GameObject panelContainer;
        private GameObject panelAssembler;
        private GameObject panelArmor;
        private GameObject panelWeapon;
        private GameObject panelReactor;
        private GameObject panelCommand;
        private Text headerText;
        private Text headerSubText;
        private Text headerGroundText;

        private Text alertHeaderText;
        private Text alertText;

        private LayerMask mask;

        // Start is called before the first frame update
        void Start()
        {
            mask = LayerMask.GetMask("Terrain", "Units", "UI");

            UIMineralText = MineralText.GetComponent<Text>();
            UIUnitText = UnitText.GetComponent<Text>();
            UIPowerText = PowerText.GetComponent<Text>();

            Transform inGamePanel = transform.Find("InGame");
            Transform gameControlPanel = inGamePanel.Find("GameControl");

            Transform panelItem = gameControlPanel.Find("PanelItem");
            headerText = panelItem.Find("HeaderText").GetComponent<Text>();
            headerSubText = panelItem.Find("SubText").GetComponent<Text>();
            headerGroundText = panelItem.Find("GroundText").GetComponent<Text>();
            alertHeaderText = panelItem.Find("AlertHeader").GetComponent<Text>();
            alertText = panelItem.Find("AlertText").GetComponent<Text>();

            headerText.text = "";
            headerSubText.text = "";
            headerGroundText.text = "";
            alertHeaderText.text = "";
            alertText.text = "";

            /*
            buildButton = panelItem.Find("BuildButton").GetComponent<Button>();
            buildButton.gameObject.SetActive(false);
            buildButton.onClick.AddListener(OnClickExtract);
            buildButtonText = buildButton.transform.Find("Text").GetComponent<Text>();
            */
            panelParts = panelItem.Find("PanelParts");

            panelEngine = panelParts.Find("PanelEngine").gameObject;
            panelExtractor = panelParts.Find("PanelExtractor").gameObject;
            panelContainer = panelParts.Find("PanelContainer").gameObject;
            panelAssembler = panelParts.Find("PanelAssembler").gameObject;
            panelArmor = panelParts.Find("PanelArmor").gameObject;
            panelWeapon = panelParts.Find("PanelWeapon").gameObject;
            panelReactor = panelParts.Find("PanelReactor").gameObject;
            panelCommand = panelParts.Find("PanelCommand").gameObject;

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

            //SetActionText(1, "(t) Select");
            //SetActionText(2, "(g) Command");
            //SetActionText(3, "(b) Build");            
            HideAction(1);
            HideAction(2);
            HideAction(3);
            HideAction(4);

            SetMode(CanvasMode.Select);
            HexGrid.MainGrid.StartGame();

            UpdateCommandButtons();
        }

        void SetMode(CanvasMode newCanvasMode)
        {
            if (canvasMode != newCanvasMode)
            {
                canvasMode = newCanvasMode;

                UpdateCommandButtons();

                SetActionText(1, canvasMode.ToString());

                if (canvasMode == CanvasMode.Select)
                {
                    SelectAction(1);
                    UnselectAction(2);
                    UnselectAction(3);
                    UnselectAction(4);

                    UnselectButton(1);
                }
            }
            //leftMouseButtonDown = false;
        }

        void OnClickAction1()
        {
            if (canvasMode != CanvasMode.Select)
            {
                SetMode(CanvasMode.Select);
            }
        }

        void OnClickAction2()
        {
        }

        void OnClickAction3()
        {

        }

        void OnClickAction4()
        {

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

        private UnitBase highlightedUnitBase;

        private void HighlightUnitFrame(UnitBase unitBase)
        {
            if (selectedUnitFrame != null && selectedUnitFrame == unitBase)
            {
                // Keep it highlighted
            }
            else
            {
                if (highlightedUnitBase != unitBase)
                {
                    UnHighlightUnitFrame();
                    highlightedUnitBase = unitBase;
                    if (unitBase != null)
                        highlightedUnitBase.SetHighlighted(true);
                }
            }
        }

        private void UnHighlightUnitFrame()
        {
            if (highlightedUnitBase != null)
            {
                if (selectedUnitFrame != null && selectedUnitFrame == highlightedUnitBase)
                {
                    // Keep it highlighted
                }
                else
                {
                    highlightedUnitBase.SetHighlighted(false);
                    highlightedUnitBase = null;
                }
            }
        }

        private void HighlightGameCommand(CommandPreview commandPreview)
        {
            if (selectedCommandPreview != null && selectedCommandPreview == commandPreview)
            {
                // Keep it highlighted
            }
            else
            {
                if (highlightedCommandPreview != commandPreview)
                {
                    UnHighlightGameCommand();
                    highlightedCommandPreview = commandPreview;
                    if (commandPreview != null)
                    {
                        highlightedCommandPreview.Command.SetHighlighted(true);
                        highlightedCommandPreview.SetActive(true);
                    }
                }
            }
        }

        private void UnHighlightGameCommand()
        {
            if (highlightedCommandPreview != null)
            {
                if (selectedCommandPreview != null && selectedCommandPreview == highlightedCommandPreview)
                {
                    // Keep it highlighted
                }
                else
                {
                    if (highlightedCommandPreview.Command != null)
                        highlightedCommandPreview.Command.SetHighlighted(false);
                    highlightedCommandPreview.SetActive(false);
                    highlightedCommandPreview = null;
                }
            }
        }


        private void CloseCommandPreview()
        {
            if (selectedCommandPreview != null)
            {
                if (selectedCommandPreview.IsPreview)
                    selectedCommandPreview.Delete();
                selectedCommandPreview.SetSelected(false);
                selectedCommandPreview.SetActive(false);
                selectedCommandPreview = null;
            }
        }

        private void SelectUnitFrame(UnitBase unitBase)
        {
            if (unitBase != selectedUnitFrame)
            {
                UnselectUnitFrame();
                if (unitBase != null)
                {
                    selectedUnitFrame = unitBase;
                    selectedUnitFrame.SetSelected(true);

                    selectedUnitBounds = new UnitBounds(unitBase);
                    selectedUnitBounds.Update();
                }
            }
        }

        private void UnselectUnitFrame()
        {
            if (selectedUnitFrame != null)
            {

                if (selectedUnitFrame.Temporary && selectedUnitFrame.CurrentPos != Position2.Null)
                {
                    // Debug.Log("UnselectUnitFrame Temporary " + selectedUnitFrame.CurrentPos.ToString());
                    Destroy(selectedUnitFrame.gameObject);
                }
                else
                {
                    //Debug.Log("UnselectUnitFrame " + selectedUnitFrame.CurrentPos.ToString());
                    selectedUnitFrame.SetSelected(false);
                }
                if (selectedUnitBounds != null)
                {
                    selectedUnitBounds.Destroy();
                    selectedUnitBounds = null;
                }
                selectedUnitFrame = null;
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

        private int selectedBuildButton;

        private bool IsMovable(BlueprintCommand blueprintCommand)
        {
            foreach (BlueprintCommandItem blueprintCommandItem in blueprintCommand.Units)
            {
                Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(blueprintCommandItem.BlueprintName);
                foreach (BlueprintPart blueprintPart in blueprint.Parts)
                {
                    if (blueprintPart.PartType == TileObjectType.PartEngine)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateCommandButtons()
        {
            if (canvasMode == CanvasMode.Select)
            {
                int idx = 1;
                if (HexGrid.MainGrid.game != null)
                {
                    foreach (BlueprintCommand blueprintCommand in HexGrid.MainGrid.game.Blueprints.Commands)
                    {
                        if (blueprintCommand.GameCommandType != GameCommandType.Build || !IsMovable(blueprintCommand))
                        {
                            SetButtonText(idx, blueprintCommand.Name);
                            idx++;
                        }
                    }
                }
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Command)
            {
                if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Build)
                {
                    // Cannot move buildings
                    HideButton(1);
                }
                else
                {
                    SetButtonText(1, "(e) Move");
                }
                HideButton(2);
                HideButton(3);
                //SetButtonText(3, "(e) Reinforce");
                SetButtonText(4, "(r) Cancel");

                int idx = 5;
                if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Attack)
                {
                    if (HexGrid.MainGrid.game != null)
                    {
                        foreach (BlueprintCommand blueprintCommand in HexGrid.MainGrid.game.Blueprints.Commands)
                        {
                            if (blueprintCommand.GameCommandType == GameCommandType.Build && IsMovable(blueprintCommand))
                            {
                                SetButtonText(idx, blueprintCommand.Name);
                                idx++;
                            }
                            if (idx >= 12) break;
                        }
                    }
                }
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Unit)
            {
                HideButton(1);
                HideButton(2);
                HideButton(3);
                SetButtonText(4, "(r) Extract");

                int idx = 5;
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Preview)
            {
                if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Collect)
                {
                    HideButton(1);
                    HideButton(2);
                }
                else
                {
                    SetButtonText(1, "(q) Rotate right");
                    SetButtonText(2, "(e) Rotate left");
                }
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
        }


        void CancelCommand()
        {
            if (selectedCommandPreview != null)
            {
                selectedCommandPreview.CancelCommand();
                CloseCommandPreview();
                SelectNothing();
            }
        }

        void AddUnitCommand(string bluePrint)
        {
            if (selectedCommandPreview != null)
            {
                selectedCommandPreview.AddUnitCommand(bluePrint);
                SetMode(CanvasMode.Preview);
            }
        }
        void MoveCommand()
        {
            if (selectedCommandPreview != null)
            {
                selectedCommandPreview.SelectMoveMode();
                SetMode(CanvasMode.Preview);
            }
        }

        void RotateCommand()
        {
            if (selectedCommandPreview != null)
            {
                selectedCommandPreview.RotateCommand();
                SetMode(CanvasMode.Preview);
            }
        }

        private CommandPreview selectedCommandPreview;

        void ExecuteCommand(int btn)
        {
            if (selectedBuildButton != btn)
            {
                if (selectedBuildButton != 0)
                    UnselectButton(selectedBuildButton);

                selectedBuildButton = btn;
            }
            if (canvasMode == CanvasMode.Select)
            {
                BlueprintCommand blueprint = HexGrid.MainGrid.game.Blueprints.Commands[btn - 1];

                selectedCommandPreview = new CommandPreview();
                selectedCommandPreview.CreateCommandForBuild(blueprint);
                selectedCommandPreview.SetSelected(true);
                highlightedCommandPreview = selectedCommandPreview;
                SetMode(CanvasMode.Preview);
            }

            if (canvasMode == CanvasMode.Preview)
            {
                if (btn == 1)
                {
                    RotateCommand();
                }
            }
            if (canvasMode == CanvasMode.Command)
            {
                if (btn == 1)
                {
                    MoveCommand();
                }
                else if (btn == 4)
                {
                    CancelCommand();
                }
                else if (btn == 5)
                {
                    AddUnitCommand("Fighter");
                }
            }
        }

        void OnClickBuild1()
        {
            ExecuteCommand(1);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(1);
            else if (canvasMode == CanvasMode.Unit)
                SelectActionUnit(1);
            else if (canvasMode == CanvasMode.Select)
                MarkUnitForExtraction();*/
        }
        void OnClickBuild2()
        {
            ExecuteCommand(2);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(2);
            if (canvasMode == CanvasMode.Unit)
                SelectActionUnit(2);
            else if (canvasMode == CanvasMode.Select)
                CancelCommand();*/
        }
        void OnClickBuild3()
        {
            ExecuteCommand(3);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(3);
            if (canvasMode == CanvasMode.Unit)
                SelectActionUnit(3);*/
        }
        void OnClickBuild4()
        {
            ExecuteCommand(4);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(4);
            if (canvasMode == CanvasMode.Unit)
                SelectActionUnit(4);
            */
        }

        void OnClickBuild5()
        {
            ExecuteCommand(5);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(5);
            */
        }
        void OnClickBuild6()
        {
            ExecuteCommand(6);
        }
        void OnClickBuild7()
        {
            ExecuteCommand(7);
        }
        void OnClickBuild8()
        {
            ExecuteCommand(8);
        }
        void OnClickBuild9()
        {
            ExecuteCommand(9);
        }
        void OnClickBuild10()
        {
            ExecuteCommand(10);
        }
        void OnClickBuild11()
        {
            ExecuteCommand(11);
        }
        void OnClickBuild12()
        {
            ExecuteCommand(12);
        }


        private UnitBase selectedUnitFrame;
        private UnitBounds selectedUnitBounds;
        private GroundCell lastSelectedGroundCell;

        private HitByMouseClick GetClickedInfo()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return null;

            HitByMouseClick hitByMouseClick = null;

            RaycastHit[] raycastHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, mask);
            CommandPreview commandPreview = null;
            if (raycastHits != null && raycastHits.Length > 0)
            {
                hitByMouseClick = new HitByMouseClick();

                int num = 0;
                foreach (RaycastHit raycastHit in raycastHits)
                {
                    //GroundCell debuggc = raycastHit.collider.gameObject.GetComponent<GroundCell>();
                    //if (debuggc != null)
                    //    Debug.Log(num + " Raycast hit GroundCell " + debuggc.Pos.ToString());

                    num++;

                    if (hitByMouseClick.GroundCell == null)
                    {
                        hitByMouseClick.GroundCell = raycastHit.collider.gameObject.transform.parent.GetComponentInParent<GroundCell>();
                        if (hitByMouseClick.GroundCell != null)
                        {
                            //Debug.Log("Raycast hit GroundCell " + Position.GetX(hitByMouseClick.GroundCell.Pos) + "," + Position.GetY(hitByMouseClick.GroundCell.Pos));
                        }
                    }
                    if (commandPreview == null)
                    {
                        Command command = raycastHit.collider.gameObject.GetComponent<Command>();
                        if (command != null)
                            commandPreview = command.CommandPreview;
                    }

                    UnitBase unitBase = GetUnitFrameFromRayCast(raycastHit);
                    if (unitBase == null)
                    {
                        //Debug.Log(num + " NO hit Unit ");
                    }
                    else
                    {
                        //Debug.Log(num + " Raycast hit Unit " + unitBase.UnitId);

                        /*
                        if (unitBase.IsGhost)
                            Debug.Log("Raycast hit Ghost " + unitBase.UnitId);
                        else
                            Debug.Log("Raycast hit Unit " + unitBase.UnitId);
                        */

                        hitByMouseClick.Units.Add(unitBase);
                    }
                }
                // Find command by pos
                if (commandPreview == null && hitByMouseClick.GroundCell != null && HexGrid.MainGrid.CommandPreviews != null)
                {
                    foreach (CommandPreview commandPreview1 in HexGrid.MainGrid.CommandPreviews)
                    {
                        if (commandPreview1.GameCommand.TargetPosition == hitByMouseClick.GroundCell.Pos)
                        {
                            commandPreview = commandPreview1;
                            break;
                        }
                    }
                }
                hitByMouseClick.Update(commandPreview);
            }

            return hitByMouseClick;
        }

        private UnitBase GetUnitFrameFromRayCast(RaycastHit raycastHit)
        {
            UnitBase unitBase = raycastHit.collider.GetComponent<UnitBase>();
            if (unitBase != null) return unitBase;

            Transform transform = raycastHit.collider.transform;

            while (transform.parent != null)
            {
                unitBase = transform.parent.GetComponent<UnitBase>();
                if (unitBase != null) return unitBase;
                if (transform.parent == null)
                    break;
                transform = transform.parent;
            }
            return null;
        }

        private void AppendGroundInfo(GroundCell gc, bool showTitle)
        {
            /*
            GameCommand gameCommand = null;
            if (HexGrid.ActiveGameCommands != null &&
                HexGrid.ActiveGameCommands.ContainsKey(lastSelectedGroundCell.Pos))
            {
                gameCommand = HexGrid.ActiveGameCommands[lastSelectedGroundCell.Pos];
            }*/

            StringBuilder sb = new StringBuilder();

            sb.Append("P: " + gc.Pos.ToString());

            //sb.Append(" TI: " + gc.Stats.MoveUpdateGroundStat.TerrainTypeIndex);
            //sb.Append(" PI: " + gc.Stats.MoveUpdateGroundStat.PlantLevel);
            //sb.Append(" Z: " + gc.Stats.MoveUpdateGroundStat.ZoneId);
            sb.Append(" Owner: " + gc.Stats.MoveUpdateGroundStat.Owner);

            if (showTitle)
            {
                headerText.text = "Ground";
                headerSubText.text = sb.ToString();

                sb.Clear();
                if (gc.Stats.MoveUpdateGroundStat.IsUnderwater)
                {
                    sb.Append("Underwater");
                }
                else if (gc.GameObjects.Count > 0)
                {
                    int mins = 0;
                    int other = 0;
                    foreach (UnitBaseTileObject item in gc.GameObjects)
                    {
                        if (item.TileObject.TileObjectType == TileObjectType.Mineral)
                            mins++;
                        else
                            other++;
                    }
                    sb.Append("Mins: " + mins + " Other: " + other);
                }
                headerGroundText.text = sb.ToString();
            }
            else
            {
                headerGroundText.text = sb.ToString();
            }
        }

        private void HideAllParts()
        {
            for (int i = 0; i < panelParts.childCount; i++)
            {
                GameObject child = panelParts.transform.GetChild(i).gameObject;
                if (child.name.StartsWith("Panel"))
                    child.SetActive(false);

            }
            /*
            panelEngine.SetActive(false);
            panelExtractor.SetActive(false);
            panelContainer.SetActive(false);
            panelAssembler.SetActive(false);
            panelArmor.SetActive(false);
            panelWeapon.SetActive(false);
            panelReactor.SetActive(false);
            panelCommand.SetActive(false);
            */
        }



        private void UnSelectGroundCell()
        {
            if (lastSelectedGroundCell != null)
            {
                //Debug.Log("UnSelectGroundCell " + lastSelectedGroundCell.Pos.ToString());

                lastSelectedGroundCell.SetHighlighted(false);
                lastSelectedGroundCell = null;
            }
        }
        private void SelectGroundCell(GroundCell groundCell)
        {
            if (lastSelectedGroundCell != groundCell)
            {
                UnSelectGroundCell();
                groundCell.SetHighlighted(true);
                lastSelectedGroundCell = groundCell;
            }
        }
        // Update is called once per frame
        void Update()
        {
            UpdateHeader();
            ExecuteHotkeys();

            if (canvasMode == CanvasMode.Select)
            {
                UpdateSelectMode();
            }
            else if (canvasMode == CanvasMode.Preview)
            {
                UpdateCommandPreviewMode();
            }
            else if (canvasMode == CanvasMode.Command)
            {
                UpdateCommandMode();
            }
            else if (canvasMode == CanvasMode.Unit)
            {
                UpdateUnitMode();
            }
        }


        private bool CheckMouseButtons()
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("LEFT MOUSE DOWN");
                lastSelectedGroundCell = null;
            }
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("LEFT MOUSE UP");
            }
            if (Input.GetMouseButtonDown(1))
            {
                //Debug.Log("RIGHT MOUSE DOWN");

                SelectNothing();
                UnSelectGroundCell();
                return true;

            }
            return false;
        }

        private bool leftMouseButtonDown;

        void UpdateCommandPreviewMode()
        {
            if (CheckMouseButtons()) return;
            if (selectedCommandPreview == null) return;

            HitByMouseClick hitByMouseClick = GetClickedInfo();
            if (hitByMouseClick != null && hitByMouseClick.GroundCell != null)
            {
                //selectedCommandPreview.SetPosition(hitByMouseClick.GroundCell);
            }
            else
            {
                selectedCommandPreview.SetPosition(null);
                return;
            }
            HideAllParts();
            if (selectedCommandPreview.Command != null && !selectedCommandPreview.Command.IsHighlighted)
            {
                selectedCommandPreview.SetHighlighted(true);
            }
            DisplayGameCommand(selectedCommandPreview);

            bool executeCommand = false;
            if (Input.GetMouseButtonDown(0))
            {
                leftMouseButtonDown = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                leftMouseButtonDown = false;
                executeCommand = true;
                Debug.Log("Input.GetMouseButtonUp");
            }
            else
            {
                if (leftMouseButtonDown && !executeCommand)
                {
                    if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        selectedCommandPreview.AddUnit(hitByMouseClick.GroundCell);
                    }
                }
                else
                {
                    selectedCommandPreview.SetPosition(hitByMouseClick.GroundCell);
                }
            }
            if (executeCommand && selectedCommandPreview.CanExecute())
            {
                bool wasSubCommandMode = selectedCommandPreview.IsInSubCommandMode;
                selectedCommandPreview.Execute();

                if (wasSubCommandMode)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        // Repeat command
                        //selectedCommandPreview.SetSelected(false);
                        //selectedCommandPreview.SetActive(false);


                        AddUnitCommand("Fighter");

                        // Keep executed command
                        //BlueprintCommand blueprintCommand = selectedCommandPreview.Blueprint;

                        //selectedCommandPreview = new CommandPreview();
                        //selectedCommandPreview.CreateCommandForBuild(blueprintCommand);

                        //selectedCommandPreview.SetSelected(true);
                        //SetMode(CanvasMode.Command);
                    }
                    else
                    {
                        if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Attack)
                        {
                            SetMode(CanvasMode.Command);
                        }
                        else
                        {
                            selectedCommandPreview.SetSelected(false);
                            selectedCommandPreview.SetActive(false);
                            selectedCommandPreview = null;

                            SetMode(CanvasMode.Select);
                        }
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        // Repeat command
                        selectedCommandPreview.SetSelected(false);
                        selectedCommandPreview.SetActive(false);

                        BlueprintCommand blueprintCommandCopy = selectedCommandPreview.Blueprint;

                        selectedCommandPreview = new CommandPreview();
                        selectedCommandPreview.CreateCommandForBuild(blueprintCommandCopy);
                    }
                    else if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        SetMode(CanvasMode.Command);
                    }
                    else
                    {
                        selectedCommandPreview.SetSelected(false);
                        selectedCommandPreview.SetActive(false);
                        selectedCommandPreview = null;

                        SetMode(CanvasMode.Select);
                    }
                }
            }
        }

        void ExecuteHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SelectNothing();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {

            }
            if (Input.GetKeyDown(KeyCode.G))
            {

            }
            if (Input.GetKeyDown(KeyCode.B))
            {

            }

            if (Input.GetKeyDown(KeyCode.Q))
            {

            }
            if (Input.GetKeyDown(KeyCode.W))
            {

            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canvasMode == CanvasMode.Preview)
                {
                    RotateCommand();
                }
                if (canvasMode == CanvasMode.Command)
                {
                    MoveCommand();
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {

            }

            if (Input.GetKeyDown(KeyCode.A))
            {

            }
            if (Input.GetKeyDown(KeyCode.S))
            {

            }
            if (Input.GetKeyDown(KeyCode.D))
            {

            }
            if (Input.GetKeyDown(KeyCode.F))
            {

            }
        }

        void UpdateHeader()
        {
            if (HexGrid.MainGrid.MapInfo != null)
            {
                if (HexGrid.MainGrid.MapInfo.PlayerInfo.ContainsKey(1))
                {
                    MapPlayerInfo mapPlayerInfo = HexGrid.MainGrid.MapInfo.PlayerInfo[1];
                    //UIMineralText.text = mapPlayerInfo.TotalMetal + " / " + mapPlayerInfo.TotalCapacity;
                    UIMineralText.text = mapPlayerInfo.TotalMinerals + " / " + mapPlayerInfo.TotalCapacity + " " + HexGrid.MainGrid.MapInfo.TotalMetal.ToString();
                    UIUnitText.text = mapPlayerInfo.TotalUnits.ToString();
                    UIPowerText.text = mapPlayerInfo.TotalPower.ToString() + "%  " + mapPlayerInfo.PowerOutInTurns.ToString();
                }
                else
                {
                    UIMineralText.text = "Dead" + HexGrid.MainGrid.MapInfo.TotalMetal.ToString();
                }
                int totalUnits = 0;
                foreach (MapPlayerInfo mapPlayerInfo1 in HexGrid.MainGrid.MapInfo.PlayerInfo.Values)
                {
                    totalUnits += mapPlayerInfo1.TotalUnits;
                }
                UIUnitText.text = totalUnits.ToString();
            }
        }

        void UpdateSelectMode()
        {

            /*bool ctrl = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            
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
            }*/
            /*
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SelectedBluePrint = null;
                UpdateCommandButtons();
            }*/

            if (CheckMouseButtons()) return;

            HitByMouseClick hitByMouseClick = GetClickedInfo();
            HighlightMouseOver(hitByMouseClick);
            UpdateMouseOver(hitByMouseClick);

            if (hitByMouseClick != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //Debug.Log("LEFT MOUSE DOWN PRESSED");
                    SelectWithLeftClick(hitByMouseClick);
                }
            }
        }

        void UpdateCommandMode()
        {
            if (selectedUnitFrame == null)
            {
                int x = 0;
            }
            if (CheckMouseButtons()) return;
            HitByMouseClick hitByMouseClick = GetClickedInfo();
            HighlightMouseOver(hitByMouseClick);

            HideAllParts();
            if (selectedUnitFrame != null)
            {
                if (selectedUnitFrame.CurrentPos != selectedUnitBounds.Pos)
                {
                    selectedUnitBounds.Destroy();
                    selectedUnitBounds.Update();
                }
                DisplayUnitframe(selectedUnitFrame);
            }
            else
            {
                if (selectedUnitBounds != null)
                {
                    selectedUnitBounds.Destroy();
                    selectedUnitBounds = null;
                }
                DisplayGameCommand(selectedCommandPreview);
            }
            //UpdateMouseOver(hitByMouseClick);

            if (hitByMouseClick != null)
            {
                if (Input.GetMouseButtonDown(1) && hitByMouseClick.GroundCell != null)
                {
                    // Move with RMB
                    /*
                    currentCommandPreview.SelectMoveMode();
                    currentCommandPreview.SetPosition(hitByMouseClick.GroundCell);
                    currentCommandPreview.Execute();*/
                }
                if (Input.GetMouseButtonDown(0))
                {
                    SelectWithLeftClick(hitByMouseClick);
                }
            }
        }

        void UpdateUnitMode()
        {
            if (selectedUnitFrame == null)
            {
                // Unit killed...
                SetMode(CanvasMode.Select);
                return;
            }
            if (CheckMouseButtons()) return;
            HideAllParts();
            DisplayUnitframe(selectedUnitFrame);

            if (selectedUnitFrame != null &&
                selectedUnitFrame.CurrentPos != selectedUnitBounds.Pos)
            {
                selectedUnitBounds.Destroy();
                selectedUnitBounds.Update();
            }
            HitByMouseClick hitByMouseClick = GetClickedInfo();
            HighlightMouseOver(hitByMouseClick);

            if (hitByMouseClick != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    SelectWithLeftClick(hitByMouseClick);
                }
            }
        }

        private void SelectWithLeftClick(HitByMouseClick hitByMouseClick)
        {
            if (selectedCommandPreview != null &&
                selectedCommandPreview != hitByMouseClick.CommandPreview)
            {
                CloseCommandPreview();
            }
            if (selectedUnitFrame != null)
            {
                // The highlighted is the one on mouse over, not the selected one
                if (selectedUnitBounds != null)
                {
                    selectedUnitBounds.Destroy();
                    selectedUnitBounds = null;
                }

                selectedUnitFrame.SetHighlighted(false);
                selectedUnitFrame.SetSelected(false);
                selectedUnitFrame = null;
            }

            if (hitByMouseClick.CommandPreview != null)
            {
                if (selectedCommandPreview != null)
                {
                    selectedCommandPreview.SetSelected(false);
                }
                selectedCommandPreview = hitByMouseClick.CommandPreview;
                selectedCommandPreview.SetSelected(true);

                StringBuilder sb = new StringBuilder();

                sb.Append("GameCommandType: ");
                sb.AppendLine(selectedCommandPreview.GameCommand.GameCommandType.ToString());

                if (selectedCommandPreview.Blueprint != null)
                {
                    sb.Append("Blueprint: ");
                    sb.AppendLine(selectedCommandPreview.Blueprint.Name);
                }
                sb.Append("DisplayPosition: ");
                sb.AppendLine(selectedCommandPreview.DisplayPosition.ToString());

                foreach (MapGameCommandItem gameCommandItem in selectedCommandPreview.GameCommand.GameCommandItems)
                {                    
                    sb.AppendLine("AttachedUnit" + gameCommandItem.AttachedUnit.ToString());
                    sb.AppendLine("FactoryUnit" + gameCommandItem.FactoryUnit.ToString());
                    sb.AppendLine("TransportUnit" + gameCommandItem.TransportUnit.ToString());
                    sb.AppendLine("TargetUnit" + gameCommandItem.TargetUnit.ToString());
                    sb.AppendLine(gameCommandItem.ToString());
                }

                Debug.Log(sb.ToString());

                if (hitByMouseClick.UnitBase != null)
                {
                    HighlightUnitFrame(hitByMouseClick.UnitBase);
                    SelectUnitFrame(hitByMouseClick.UnitBase);
                }

                if (canvasMode == CanvasMode.Command)
                    UpdateCommandButtons();
                else
                    SetMode(CanvasMode.Command);
            }
            else if (hitByMouseClick.UnitBase != null)
            {
                HighlightUnitFrame(hitByMouseClick.UnitBase);
                SelectUnitFrame(hitByMouseClick.UnitBase);
                SetMode(CanvasMode.Unit);
            }
            else
            {
                SelectNothing();
            }
        }

        private void UpdateMouseOver(HitByMouseClick hitByMouseClick)
        {
            if (hitByMouseClick == null)
            {
                ShowNothing();
            }
            else
            {
                if (hitByMouseClick.CommandPreview != null)
                {
                    HideAllParts();

                    if (highlightedCommandPreview != hitByMouseClick.CommandPreview)
                    {
                        if (highlightedCommandPreview != null)
                        {
                            if (canvasMode == CanvasMode.Command && highlightedCommandPreview == selectedCommandPreview)
                            {
                                // Leave it visible
                            }
                            else
                            {
                                //highlightedCommandPreview.Command.SetHighlighted(false);
                                highlightedCommandPreview.SetActive(false);
                            }
                        }
                        //HighlightGameCommand(hitByMouseClick.CommandPreview);
                    }

                    if (hitByMouseClick.UnitBase != null)
                    {
                        //HighlightUnitFrame(hitByMouseClick.UnitBase);
                        DisplayUnitframe(hitByMouseClick.UnitBase);
                    }
                    else
                    {
                        //UnHighlightUnitFrame();
                        DisplayGameCommand(highlightedCommandPreview);
                    }
                }
                else if (hitByMouseClick.UnitBase != null)
                {
                    HideAllParts();
                    //UnSelectGroundCell();
                    //HighlightUnitFrame(hitByMouseClick.UnitBase);
                    //UnHighlightGameCommand();
                    DisplayUnitframe(hitByMouseClick.UnitBase);
                }
                else if (hitByMouseClick.GroundCell != null)
                {
                    ShowNothing();
                    AppendGroundInfo(hitByMouseClick.GroundCell, true);
                }
                else
                {
                    ShowNothing();
                }
            }
        }

        private void HighlightMouseOver(HitByMouseClick hitByMouseClick)
        {
            if (hitByMouseClick == null)
            {
                HighlightNothing();
            }
            else
            {
                if (hitByMouseClick.CommandPreview != null)
                {
                    if (highlightedCommandPreview != hitByMouseClick.CommandPreview)
                    {
                        if (highlightedCommandPreview != null)
                        {
                            if (canvasMode == CanvasMode.Command && highlightedCommandPreview == selectedCommandPreview)
                            {
                                // Leave it visible
                            }
                            else
                            {
                                highlightedCommandPreview.Command.SetHighlighted(false);
                            }
                        }
                        HighlightGameCommand(hitByMouseClick.CommandPreview);
                    }

                    if (hitByMouseClick.UnitBase != null)
                    {
                        HighlightUnitFrame(hitByMouseClick.UnitBase);
                    }
                    else
                    {
                        UnHighlightUnitFrame();
                    }
                }
                else if (hitByMouseClick.UnitBase != null)
                {
                    UnSelectGroundCell();
                    HighlightUnitFrame(hitByMouseClick.UnitBase);
                    UnHighlightGameCommand();
                }
                else if (hitByMouseClick.GroundCell != null)
                {
                    HighlightNothing();
                }
                else
                {
                    HighlightNothing();
                }
            }
        }

        private void HighlightNothing()
        {
            UnSelectGroundCell();
            UnHighlightUnitFrame();
            UnHighlightGameCommand();
        }

        private void ShowNothing()
        {
            HighlightNothing();

            HideAllParts();
            headerText.text = "";
            headerSubText.text = "";
            headerGroundText.text = "";
            alertHeaderText.text = "";
            alertText.text = "";
        }

        private void SelectNothing()
        {
            ShowNothing();
            if (selectedUnitFrame != null)
            {
                UnselectUnitFrame();
            }
            if (selectedCommandPreview != null && selectedCommandPreview.IsInSubCommandMode)
            {
                selectedCommandPreview.CancelSubCommand();
                SetMode(CanvasMode.Command);
            }
            else
            {
                CloseCommandPreview();
                SetMode(CanvasMode.Select);
            }
        }

        private CommandPreview highlightedCommandPreview;

        private void DisplayUpdateStatsCommand(UnitBase unit)
        {
            alertHeaderText.text = unit.UnitAlert.Header;
            alertText.text = unit.UnitAlert.Text;
        }

        private void DisplayGameCommand(CommandPreview commandPreview)
        {
            if (commandPreview == null) return;

            Position2 position2;
            if (commandPreview.IsPreview)
            {
                position2 = commandPreview.DisplayPosition;
            }
            else if (commandPreview.IsMoveMode)
            {
                position2 = commandPreview.DisplayPosition;
            }
            else
            {
                position2 = commandPreview.GameCommand.TargetPosition;
            }
            MapGameCommand gameCommand = commandPreview.GameCommand;
            headerText.text = gameCommand.GameCommandType.ToString() + " Pl: " + gameCommand.PlayerId;
            //headerSubText.text = "Radius " + gameCommand.Radius.ToString() + " sel: " + commandPreview.IsSelected.ToString();

            foreach (MapGameCommandItem gameCommandItem in gameCommand.GameCommandItems)
            {
                headerSubText.text = gameCommandItem.AttachedUnit.Status;
                if (gameCommandItem.AttachedUnit.Alert)
                    headerSubText.text += " ALERT";
                break;
            }
            /*
            panelCommand.transform.Find("Partname").GetComponent<Text>().text = gameCommand.GameCommandType.ToString();
            panelCommand.transform.Find("Content").GetComponent<Text>().text = "Radius " + gameCommand.Radius.ToString();
            panelCommand.SetActive(true);
            */

            if (position2 != Position2.Null)
            {
                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position2, out gc))
                    AppendGroundInfo(gc, false);
                else
                    headerGroundText.text = "";
            }
            else
            {
                headerGroundText.text = "";
            }

            /*
            foreach (MapGameCommandItem gameCommandItem in gameCommand.GameCommandItems)
            {
                GameObject commandPart = Instantiate(panelCommand, panelParts);
                commandPart.transform.Find("Partname").GetComponent<Text>().text = gameCommandItem.BlueprintCommandItem.BlueprintName;
                commandPart.transform.Find("Content").GetComponent<Text>().text = gameCommandItem.AttachedUnitId;
                commandPart.SetActive(commandPart);
            }*/
        }

        private void DisplayUnitframe(UnitBase unit)
        {
            if (unit == null)
            {
                return;
            }
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
                    headerText.text = unit.MoveUpdateStats.BlueprintName;

                    if (unit.HasBeenDestroyed)
                    {
                        headerSubText.text = "Destroyed ";
                    }
                    else if (unit.Temporary)
                    {
                        headerSubText.text = "Preview ";
                    }
                    else if (unit.UnderConstruction)
                    {
                        headerSubText.text = "Under construction ";
                    }
                    else if (unit.MoveUpdateStats.MarkedForExtraction)
                    {
                        headerSubText.text = "MarkedForExtraction ";
                    }
                    else
                    {
                        headerSubText.text = "";
                    }

                    headerSubText.text += unit.UnitId;
                    headerSubText.text += " Power: " + unit.MoveUpdateStats.Power;

                    DisplayUpdateStatsCommand(unit);

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
                        if (part.PartType == TileObjectType.PartExtractor)
                        {
                            panelExtractor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelExtractor.SetActive(true);
                        }
                        if (part.PartType == TileObjectType.PartWeapon)
                        {
                            panelWeapon.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelWeapon.SetActive(true);

                            if (part.Exists)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("Ammunition  ");
                                sb.Append(part.TileObjects.Count);

                                if (part.Capacity.HasValue)
                                    sb.Append("/" + part.Capacity.Value);

                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                            }
                            else
                            {
                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = "Destroyed";
                            }
                        }
                        if (part.PartType == TileObjectType.PartAssembler)
                        {
                            panelAssembler.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelAssembler.SetActive(true);

                            if (part.Exists)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("Minerals  ");
                                sb.Append(part.TileObjects.Count);

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
                            else
                            {
                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = "Destroyed";
                            }

                        }
                        if (part.PartType == TileObjectType.PartReactor)
                        {
                            panelReactor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelReactor.SetActive(true);

                            if (part.Exists)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("Minerals  ");
                                sb.Append(part.TileObjects.Count);

                                if (part.Capacity.HasValue)
                                    sb.Append("/" + part.Capacity.Value);

                                sb.Append(" Power  ");
                                if (part.AvailablePower.HasValue)
                                    sb.Append(part.AvailablePower.Value);
                                else
                                    sb.Append("0");
                                panelReactor.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                            }
                            else
                            {
                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = "Destroyed";
                            }
                        }
                        if (part.PartType == TileObjectType.PartArmor)
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
                        if (part.PartType == TileObjectType.PartEngine)
                        {
                            panelEngine.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelEngine.SetActive(true);
                        }
                        if (part.PartType == TileObjectType.PartContainer)
                        {
                            panelContainer.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelContainer.SetActive(true);

                            if (part.Exists)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.Append("Minerals  ");
                                sb.Append(part.TileObjects.Count);

                                if (part.Capacity.HasValue)
                                    sb.Append("/" + part.Capacity.Value);

                                panelContainer.transform.Find("Content").GetComponent<Text>().text = sb.ToString();
                            }
                            else
                            {
                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = "Destroyed";
                            }
                        }
                    }
                }
            }

            if (unit.CurrentPos != Position2.Null)
            {
                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(unit.CurrentPos, out gc))
                    AppendGroundInfo(gc, false);
                else
                    headerGroundText.text = "";
            }
            else
            {
                headerGroundText.text = "";
            }
        }
    }

    internal class HitByMouseClick
    {
        public HitByMouseClick()
        {
            Units = new List<UnitBase>();
        }

        public List<UnitBase> Units { get; private set; }
        public GroundCell GroundCell { get; set; }
        public CommandPreview CommandPreview { get; private set; }
        public UnitBase UnitBase { get; private set; }

        public void Update(CommandPreview commandPreview)
        {
            foreach (UnitBase unitBase in Units)
            {
                CommandPreview unitCommandPreview = HexGrid.MainGrid.FindCommandForUnit(unitBase);
                if (unitCommandPreview != null)
                {
                    CommandPreview = unitCommandPreview;
                    UnitBase = unitBase;
                    break;
                }
            }
            if (UnitBase == null && Units.Count > 0)
            {
                // Just the first one
                UnitBase = Units[0];
            }
            if (CommandPreview == null && commandPreview != null)
            {
                CommandPreview = commandPreview;
            }
        }
    }
    internal enum CanvasMode
    {
        None,
        Select,
        Unit,
        Preview,
        Command
    }

}