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
        public Button ButtonFaster;
        public Button ButtonPause;
        public Button ButtonSlower;

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
        private Button[] buttons;

        private Transform panelBuild;

        private Transform panelParts;
        private GameObject panelEngine;
        private GameObject panelExtractor;
        private GameObject panelContainer;
        private GameObject panelAssembler;
        private GameObject panelArmor;
        private GameObject panelWeapon;
        private GameObject panelReactor;
        private CanvasUnit panelUnit;

        private Text headerText;
        private Text headerSubText;
        private Text headerGroundText;

        private Text alertHeaderText;
        private Text alertText;

        private LayerMask mask;

        private List<CanvasItem> canvasItems;

        // Start is called before the first frame update
        void Start()
        {
            mask = LayerMask.GetMask("Terrain", "Units", "UI");

            UIMineralText = MineralText.GetComponent<Text>();
            UIUnitText = UnitText.GetComponent<Text>();
            UIPowerText = PowerText.GetComponent<Text>();

            Transform inGamePanel = transform.Find("InGame");
            Transform gameControlPanel = inGamePanel.Find("GameControl");

            panelBuild = gameControlPanel.Find("PanelBuild");
            panelBuild.gameObject.SetActive(false);

            Transform panelSelected = gameControlPanel.Find("PanelSelected");
            headerText = panelSelected.Find("HeaderText").GetComponent<Text>();
            headerSubText = panelSelected.Find("SubText").GetComponent<Text>();
            headerGroundText = panelSelected.Find("GroundText").GetComponent<Text>();
            alertHeaderText = panelSelected.Find("AlertHeader").GetComponent<Text>();
            alertText = panelSelected.Find("AlertText").GetComponent<Text>();

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
            panelUnit = panelSelected.Find("PanelUnit").GetComponent<CanvasUnit>();

            panelParts = panelSelected.Find("PanelParts");

            panelEngine = panelParts.Find("PanelEngine").gameObject;
            panelExtractor = panelParts.Find("PanelExtractor").gameObject;
            panelContainer = panelParts.Find("PanelContainer").gameObject;
            panelAssembler = panelParts.Find("PanelAssembler").gameObject;
            panelArmor = panelParts.Find("PanelArmor").gameObject;
            panelWeapon = panelParts.Find("PanelWeapon").gameObject;
            panelReactor = panelParts.Find("PanelReactor").gameObject;

            canvasItems = new List<CanvasItem>();
            for (int i = 0; i < panelContainer.transform.childCount; i++)
            {
                Transform transform = panelContainer.transform.GetChild(i);
                string name = transform.gameObject.name;
                if (name.StartsWith("Item"))
                {
                    CanvasItem canvasItem = transform.gameObject.GetComponent<CanvasItem>();
                    if (name == "ItemMineral")                    
                        canvasItem.TileObjectType = TileObjectType.Mineral;
                    if (name == "ItemWood")
                        canvasItem.TileObjectType = TileObjectType.Wood;
                    if (name == "ItemStone")
                        canvasItem.TileObjectType = TileObjectType.Stone;

                    canvasItem.Count = transform.Find("Count").GetComponent<Text>();
                    canvasItem.State = transform.Find("State").gameObject;
                    canvasItem.Icon = transform.Find("Icon").gameObject;
                    canvasItem.TileObjectState = TileObjectState.None;

                    canvasItem.Button = transform.gameObject.GetComponent<Button>();
                    if (canvasItem.Button != null)
                    {
                        canvasItem.Button.onClick.AddListener(delegate { OnClickItem(canvasItem); });
                    }

                    canvasItems.Add(canvasItem);
                }
            }

            Transform panelAction = gameControlPanel.Find("PanelAction");

            /*
            actions = new Button[12];
            actionText = new Text[12];
            for (int i = 0; i < 4; i++)
            {
                actions[i] = panelSelected.Find("Action" + (i + 1).ToString()).GetComponent<Button>();
                actionText[i] = actions[i].transform.Find("Text").GetComponent<Text>();
                actions[i].name = "Action" + (i + 1).ToString();
            }*/

            Transform panelFrame = panelAction.Find("PanelFrame");

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

            /*
            actions[0].onClick.AddListener(OnClickAction1);
            actions[1].onClick.AddListener(OnClickAction2);
            actions[2].onClick.AddListener(OnClickAction3);
            actions[3].onClick.AddListener(OnClickAction4);
            */
            if (ButtonFaster != null)
                ButtonFaster.onClick.AddListener(OnFaster);
            if (ButtonPause != null)
                ButtonPause.onClick.AddListener(OnPause); 
            if (ButtonSlower != null)
                ButtonSlower.onClick.AddListener(OnSlower);

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

                if (canvasMode == CanvasMode.Select)
                {
                    UnselectButton(1);
                }
            }
            //leftMouseButtonDown = false;
        }

        void OnFaster()
        {
            HexGrid.MainGrid.RunFaster();
        }

        void OnPause()
        {
            HexGrid.MainGrid.Pause();
        }
        void OnSlower()
        {
            HexGrid.MainGrid.RunSlower();
        }

        private Button GetButton(int btn)
        {
            if (btn == 0 || btn > 12)
            {
                return null;
            }
            return buttons[btn - 1];
        }

        private Text GetButtonText(int btn)
        {
            return buttonText[btn - 1];
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
                        highlightedCommandPreview.SetHighlighted(true);
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
                    highlightedCommandPreview.SetHighlighted(false);
                    highlightedCommandPreview.SetActive(false);
                    highlightedCommandPreview = null;
                }
            }
        }


        private void CloseCommandPreview()
        {
            if (selectedCommandPreview != null)
            {
                UnHighlightGameCommand();
                if (selectedCommandPreview.IsPreview)
                    selectedCommandPreview.Delete();
                selectedCommandPreview.SetHighlighted(false);
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
                    selectedUnitFrame.SetHighlighted(false);
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

        private void UnselectButton(int btn)
        {
            GetButton(btn).image.sprite = ButtonBackground;
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
                SetButtonText(1, "(q) Build");
                HideButton(2);
                HideButton(3);
                SetButtonText(4, "(t) Collect");

                int idx = 5;
                /*
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
                */
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Command)
            {
                HideButton(1);
                HideButton(2);
                HideButton(3);
                HideButton(4);
                HideButton(5);
                HideButton(6);
                HideButton(7);
                SetButtonText(8, "(r) Cancel");

                int idx = 9;
                /*
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
                }*/
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Unit)
            {
                if (selectedUnitFrame.HasAssembler())
                {
                    SetButtonText(1, "(q) Build");
                }
                else
                {
                    // Cannot move buildings
                    HideButton(1);
                }

                if (selectedUnitFrame.HasEngine())
                {
                    SetButtonText(2, "(e) Move");
                }
                else
                { 
                    // Cannot move buildings
                    HideButton(2);
                }

                if (selectedUnitFrame.HasWeaponAndCanFire())
                {
                    SetButtonText(3, "(r) Throw");
                }
                else
                {
                    // Cannot move buildings
                    HideButton(3);
                }

                if (selectedUnitFrame.HasExtractor())
                {
                    SetButtonText(4, "(t) Collect");
                }
                else
                {
                    // Cannot move buildings
                    HideButton(4);
                }


                SetButtonText(5, "( ) Clone");
                if (selectedUnitFrame.MoveUpdateStats.MarkedForExtraction)
                    HideButton(6);
                else
                    SetButtonText(6, "( ) Delete");



                UnitBasePart container = selectedUnitFrame.GetContainer();
                if (selectedUnitFrame.HasExtractor() && container != null && container.TileObjectContainer.TileObjects.Count > 0)
                { 
                    SetButtonText(7, "( ) Unload");
                }
                else
                {
                    // Cannot move buildings
                    HideButton(7);
                }

                if (selectedUnitFrame.MoveUpdateStats.Automatic)
                    HideButton(8);
                else
                    SetButtonText(8, "( ) CancelCmd");

                int idx = 9;
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Preview)
            {
                if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Collect)
                {
                    SetButtonText(1, "(q) Decrease");
                    SetButtonText(2, "(e) Increase");
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

        void IncreaseRadius()
        {
            selectedCommandPreview.IncreaseRadius();
        }
        void DecreaseRadius()
        {
            selectedCommandPreview.DecreaseRadius();
        }

        void RotateCommand(bool turnRight)
        {
            if (selectedCommandPreview != null)
            {
                selectedCommandPreview.RotateCommand(turnRight);
                SetMode(CanvasMode.Preview);
            }
        }

        private CommandPreview selectedCommandPreview;

        internal void PreviewExecuteCommand(BlueprintCommand blueprintCommand)
        {
            HideBuildMenu();

            selectedCommandPreview = new CommandPreview();
            selectedCommandPreview.CreateCommand(blueprintCommand, selectedUnitFrame?.UnitId);
            selectedCommandPreview.SetSelected(true);
            highlightedCommandPreview = selectedCommandPreview;

            SetMode(CanvasMode.Preview);
            /*
            if (blueprintCommand.GameCommandType == GameCommandType.Build)
            {
                MapGameCommand nextGameCommand = new MapGameCommand();
                nextGameCommand.ClientId = CommandPreview.GetNextClientCommandId();
                selectedCommandPreview.GameCommand.NextGameCommand = nextGameCommand;

                CommandPreview attackmovePreview = new CommandPreview();                
                attackmovePreview.CreateCommandPreview(nextGameCommand);
                attackmovePreview.SetSelected(true);
                selectedCommandPreview.NextCommandPreview = attackmovePreview;
            }*/
        }
        /*
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
                selectedCommandPreview.CreateCommandForBuild(blueprint, null);
                selectedCommandPreview.SetSelected(true);
                highlightedCommandPreview = selectedCommandPreview;
                SetMode(CanvasMode.Preview);

                return;
            }

            if (canvasMode == CanvasMode.Preview)
            {
                if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Collect)
                {
                    if (btn == 1)
                    {
                        IncreaseRadius();
                    }
                    if (btn == 2)
                    {
                        DecreaseRadius();
                    }
                }
                else
                {
                    if (btn == 1)
                    {
                        RotateCommand(true);
                    }
                    if (btn == 2)
                    {
                        RotateCommand(false);
                    }
                }
            }
            if (canvasMode == CanvasMode.Command)
            {
                if (btn == 1)
                {
                    
                }
                else if (btn == 4)
                {
                    CancelCommand();
                }
                
            }
        }
        */

        void OnClickItem(CanvasItem canvasItem)
        {
            if (canvasItem.TileObjectState == TileObjectState.None)
            {
                canvasItem.TileObjectState = TileObjectState.Accept;
            }
            else if (canvasItem.TileObjectState == TileObjectState.Accept)
            {
                canvasItem.TileObjectState = TileObjectState.Deny;
            }
            else if (canvasItem.TileObjectState == TileObjectState.Deny)
            {
                canvasItem.TileObjectState = TileObjectState.None;
            }
            canvasItem.UpdateImage();

            if (selectedUnitFrame != null && selectedUnitFrame.MoveUpdateStats != null && selectedUnitFrame.MoveUpdateStats.MoveUnitItemOrders != null)
            {
                foreach (MoveUnitItemOrder order in selectedUnitFrame.MoveUpdateStats.MoveUnitItemOrders)
                {
                    if (order.TileObjectType == canvasItem.TileObjectType)
                    {
                        order.TileObjectState = canvasItem.TileObjectState;

                        MapGameCommand gameCommand = new MapGameCommand();

                        gameCommand.PlayerId = selectedUnitFrame.PlayerId;
                        gameCommand.GameCommandType = GameCommandType.ItemOrder;
                        gameCommand.UnitId = selectedUnitFrame.UnitId;
                        gameCommand.ClientId = CommandPreview.GetNextClientCommandId();
                        gameCommand.MoveUnitItemOrders = new List<MoveUnitItemOrder>();

                        MoveUnitItemOrder moveUnitItemOrder = new MoveUnitItemOrder();
                        moveUnitItemOrder.TileObjectType = order.TileObjectType;
                        moveUnitItemOrder.TileObjectState = order.TileObjectState;
                        gameCommand.MoveUnitItemOrders.Add(moveUnitItemOrder);

                        HexGrid.MainGrid.GameCommands.Add(gameCommand);

                        break;
                    }
                }
            }
        }

        private bool buildMenuInit;
        void ShowBuildMenu()
        {
            if (!buildMenuInit)
            {
                buildMenuInit = true;

                List<BlueprintCommand> commands = new List<BlueprintCommand>();
                if (HexGrid.MainGrid.game != null)
                {
                    foreach (BlueprintCommand blueprintCommand in HexGrid.MainGrid.game.Blueprints.Commands)
                    {
                        if (blueprintCommand.GameCommandType == GameCommandType.Build)
                        {
                            commands.Add(blueprintCommand);
                        }
                    }
                }

                Transform panelFrame = panelBuild.Find("PanelFrame");
                for (int i = 0; i < 12; i++)
                {
                    Transform transform = panelFrame.Find("Button" + (i + 1).ToString());
                    if (transform != null)
                    {
                        BuildItem buildItem = transform.GetComponent<BuildItem>();
                        if (buildItem == null || commands.Count == 0)
                        {
                            transform.gameObject.SetActive(false);
                        }
                        else
                        {
                            buildItem.SetCommand(this, commands[0]);
                            commands.RemoveAt(0);

                            transform.gameObject.SetActive(true);
                        }
                    }
                }
            }
            panelBuild.gameObject.SetActive(true);
        }
        void HideBuildMenu()
        {
            panelBuild.gameObject.SetActive(false);
        }

        void ExecuteCollectCommand()
        {
            if (HexGrid.MainGrid.game != null)
            {
                foreach (BlueprintCommand blueprintCommand in HexGrid.MainGrid.game.Blueprints.Commands)
                {
                    if (blueprintCommand.GameCommandType == GameCommandType.Collect)
                    {
                        PreviewExecuteCommand(blueprintCommand);
                        break;
                    }
                }
            }
        }

        void SelectedUnitCommand(GameCommandType gameCommandType)
        {
            selectedCommandPreview = new CommandPreview();
            selectedCommandPreview.CreateCommand(selectedUnitFrame, gameCommandType);
            //selectedCommandPreview.GameCommand.FollowUpUnitCommand = FollowUpUnitCommand.DeleteCommand;
            selectedCommandPreview.SetSelected(true);
            highlightedCommandPreview = selectedCommandPreview;
            SetMode(CanvasMode.Preview);
        }

        void OnClickBuild1()
        {
            if (canvasMode == CanvasMode.Select)
                ShowBuildMenu();
            if (canvasMode == CanvasMode.Unit)
            {
                if (selectedUnitFrame.HasAssembler())
                {
                    ShowBuildMenu();
                }
            }
        }
        void OnClickBuild2()
        {
            if (canvasMode == CanvasMode.Unit)
            {
                if (selectedUnitFrame.MoveUpdateStats?.MoveUpdateStatsCommand?.GameCommandType == GameCommandType.HoldPosition)
                    SelectedUnitCommand(GameCommandType.HoldPosition);
                else
                    SelectedUnitCommand(GameCommandType.AttackMove);
            }
            //ExecuteCommand(2);
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
            if (canvasMode == CanvasMode.Unit)
                SelectedUnitCommand(GameCommandType.Fire);

            //ExecuteCommand(3);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(3);
            if (canvasMode == CanvasMode.Unit)
                SelectActionUnit(3);*/
        }
        void OnClickBuild4()
        {
            if (canvasMode == CanvasMode.Select)
                ExecuteCollectCommand();
            else if (canvasMode == CanvasMode.Unit)
                SelectedUnitCommand(GameCommandType.Collect);
            
        }

        void OnClickBuild5()
        {
            //ExecuteCommand(5);
            /*
            if (canvasMode == CanvasMode.Build)
                SelectBuildUnit(5);
            */
        }
        void OnClickBuild6()
        {
            if (canvasMode == CanvasMode.Unit)
            {
                selectedCommandPreview = new CommandPreview();
                selectedCommandPreview.CreateCommand(selectedUnitFrame, GameCommandType.Extract);
                selectedCommandPreview.Execute();
                selectedUnitFrame.MoveUpdateStats.MarkedForExtraction = true;
                UpdateCommandButtons();
            }
        }
        void OnClickBuild8()
        {
            if (canvasMode == CanvasMode.Unit)
            {
                selectedCommandPreview = new CommandPreview();
                selectedCommandPreview.CreateCommand(selectedUnitFrame, GameCommandType.Automate);
                selectedCommandPreview.Execute();
                selectedUnitFrame.MoveUpdateStats.Automatic = true;
                UpdateCommandButtons();
            }
            else if (canvasMode == CanvasMode.Command)
                CancelCommand();
        }
        void OnClickBuild7()
        {
            if (canvasMode == CanvasMode.Select)
            {

            }
                //ExecuteCollectCommand();
            else if (canvasMode == CanvasMode.Unit)
                SelectedUnitCommand(GameCommandType.Unload);
            //else if (canvasMode == CanvasMode.Command)
                //CancelCommand();
        }
        void OnClickBuild9()
        {
            //ExecuteCommand(9);
        }
        void OnClickBuild10()
        {
           // ExecuteCommand(10);
        }
        void OnClickBuild11()
        {
            //ExecuteCommand(11);
        }
        void OnClickBuild12()
        {
            //ExecuteCommand(12);
        }


        private UnitBase selectedUnitFrame;
        private UnitBounds selectedUnitBounds;
        private GroundCell lastHighlightedGroundCell;

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

                    GroundCell hitGroundCell = null;

                    if (raycastHit.collider.gameObject.name == "HexCell1")
                    {
                        //Debug.Log(num + " Raycast parent hit  " + raycastHit.collider.gameObject.transform.parent.name);
                        hitGroundCell = raycastHit.collider.gameObject.transform.parent.GetComponent<GroundCell>();
                    }
                    else
                    { 
                        //Debug.Log(num + " Raycast hit  " + raycastHit.collider.gameObject.name);
                        hitGroundCell = raycastHit.collider.gameObject.transform.GetComponent<GroundCell>();
                    }

                    num++;
                    
                    if (hitGroundCell != null)
                    {
                        //Debug.Log("Raycast hit GroundCell " + hitGroundCell.Pos.ToString());
                    }
                    else
                    {
                        /*
                        hitGroundCell = raycastHit.collider.gameObject.transform.parent.GetComponentInParent<GroundCell>();
                        if (hitGroundCell != null)
                        {
                            Debug.Log("Raycast hit Parent GroundCell " + hitGroundCell.Pos.ToString());
                        }
                        else
                        {
                            hitGroundCell = raycastHit.collider.gameObject.transform.parent.GetComponentInChildren<GroundCell>();
                            if (hitGroundCell != null)
                            {
                                Debug.Log("Raycast hit Child GroundCell " + hitGroundCell.Pos.ToString());
                            }
                        }*/
                    }
                    
                    if (hitGroundCell != null)
                    {
                        // Take the last hit, which sould be the topmost
                        hitByMouseClick.GroundCell = hitGroundCell;
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
                        if (!unitBase.IsGhost)
                            hitByMouseClick.Units.Add(unitBase);
                    }
                }
                // Find command by pos no more, select unit
                if (hitByMouseClick.Units.Count == 0)
                {
                    if (commandPreview == null && hitByMouseClick.GroundCell != null && HexGrid.MainGrid.CommandPreviews != null)
                    {
                        foreach (CommandPreview commandPreview1 in HexGrid.MainGrid.CommandPreviews.Values)
                        {
                            if (commandPreview1.GameCommand.TargetPosition == hitByMouseClick.GroundCell.Pos)
                            {
                                commandPreview = commandPreview1;
                                break;
                            }
                        }
                    }
                }
                hitByMouseClick.Update(commandPreview);
            }

            return hitByMouseClick;
        }

        private static UnitBase GetUnitFrameFromRayCast(RaycastHit raycastHit)
        {
            return UnitBase.GetUnitFrameColilder(raycastHit.collider);
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
            //sb.Append(" Owner: " + gc.Stats.MoveUpdateGroundStat.Owner);
            sb.Append(" cntInt: " + gc.cntInt);

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
            panelUnit.Hide();
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

        private void UnHighlightGroundCell()
        {
            if (lastHighlightedGroundCell != null)
            {
                //Debug.Log("UnSelectGroundCell " + lastSelectedGroundCell.Pos.ToString());

                lastHighlightedGroundCell.SetHighlighted(false);
                lastHighlightedGroundCell = null;
            }
        }
        private void SelectGroundCell(GroundCell groundCell)
        {
            if (lastHighlightedGroundCell != groundCell)
            {
                UnHighlightGroundCell();
                groundCell.SetHighlighted(true);
                lastHighlightedGroundCell = groundCell;
            }
        }
        // Update is called once per frame
        void Update()
        {
            foreach (CanvasItem canvasItem in canvasItems)
            {
                if (canvasItem.Icon != null)
                {
                    canvasItem.Icon.transform.Rotate(Vector3.up, 0.1f);
                }

            }
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
                lastHighlightedGroundCell = null;
            }
            if (Input.GetMouseButtonUp(0))
            {
                //Debug.Log("LEFT MOUSE UP");
            }
            if (Input.GetMouseButtonDown(1))
            {
                //Debug.Log("RIGHT MOUSE DOWN");

                SelectNothing();
                UnHighlightGroundCell();
                return true;

            }
            return false;
        }

        

        void UpdateCommandPreviewMode()
        {
            if (Input.GetMouseButtonUp(1) && selectedCommandPreview != null)
            {
                selectedCommandPreview.Execute();
                if (selectedUnitFrame != null)
                    SetMode(CanvasMode.Unit);
                else
                    SetMode(CanvasMode.Select);
                return;
            }
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
                //leftMouseButtonDown = true;
            }
            if (Input.GetMouseButtonUp(0))
            {
                //leftMouseButtonDown = false;
                executeCommand = true;
                //Debug.Log("Input.GetMouseButtonUp");
            }
            else
            {
                selectedCommandPreview.SetPosition(hitByMouseClick.GroundCell);
            }
            if (executeCommand && selectedCommandPreview.CanExecute())
            {
                selectedCommandPreview.Execute();

                // Cannot repeat commands for units, they are not stacked now
                if (Input.GetKey(KeyCode.LeftShift) && selectedCommandPreview.GameCommand.UnitId == null)
                {
                    // Repeat command
                    selectedCommandPreview.SetSelected(false);
                    selectedCommandPreview.SetActive(false);

                    MapGameCommand gameCommand = selectedCommandPreview.GameCommand;
                    MapGameCommand mapGameCommand = new MapGameCommand();
                    mapGameCommand.Layout = gameCommand.Layout;
                    mapGameCommand.GameCommandType = gameCommand.GameCommandType;
                    mapGameCommand.Direction = gameCommand.Direction;
                    mapGameCommand.UnitId = gameCommand.UnitId;
                    mapGameCommand.PlayerId = gameCommand.PlayerId;
                    mapGameCommand.BlueprintName = gameCommand.BlueprintName;
                    mapGameCommand.FollowUpUnitCommand = gameCommand.FollowUpUnitCommand;

                    //GameCommandType gameCommandType = selectedCommandPreview.GameCommand.GameCommandType;
                    //string unitId = selectedCommandPreview.GameCommand.UnitId;

                    selectedCommandPreview = new CommandPreview();
                    selectedCommandPreview.CreateCommandPreview(mapGameCommand);
                    selectedCommandPreview.SetSelected(true);
                    selectedCommandPreview.SetActive(true);
                    /*
                    if (gameCommandType == GameCommandType.Build)
                        selectedCommandPreview.CreateCommandForBuild(blueprintCommandCopy, unitId);
                    else
                        selectedCommandPreview.CreateCommand(selectedUnitFrame, gameCommandType);
                    */
                }
                else
                {
                    selectedCommandPreview.SetSelected(false);
                    selectedCommandPreview.SetActive(false);
                    selectedCommandPreview = null;
                    if (selectedUnitFrame != null)
                        SetMode(CanvasMode.Unit);
                    else
                        SetMode(CanvasMode.Select);
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
                if (canvasMode == CanvasMode.Preview)
                {
                    if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        DecreaseRadius();
                    }
                    else
                    {
                        RotateCommand(false);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (canvasMode == CanvasMode.Preview)
                {
                    if (selectedCommandPreview.GameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        IncreaseRadius();
                    }
                    else
                    { 
                        RotateCommand(true);
                    }
                }
                if (canvasMode == CanvasMode.Command)
                {
                    //MoveCommand();
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
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
            //if (CheckMouseButtons()) return;
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("LEFT MOUSE DOWN");
                lastHighlightedGroundCell = null;
            }

            HideAllParts();
            UpdateCommandButtons(); // Reflect changes in unti state (like ammo)
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
                if (Input.GetMouseButtonDown(1) && hitByMouseClick.GroundCell != null)
                {
                    Debug.Log("Input.GetMouseButtonDown");
                    GameCommandType gameCommandType;
                    if (selectedUnitFrame.MoveUpdateStats?.MoveUpdateStatsCommand?.GameCommandType == GameCommandType.HoldPosition)
                        gameCommandType= GameCommandType.HoldPosition;
                    else
                        gameCommandType = GameCommandType.AttackMove;

                    selectedCommandPreview = new CommandPreview();
                    selectedCommandPreview.CreateCommand(selectedUnitFrame, gameCommandType); // hitByMouseClick.GroundCell);
                    selectedCommandPreview.SetSelected(true);
                    highlightedCommandPreview = selectedCommandPreview;
                    SetMode(CanvasMode.Preview);
                    selectedCommandPreview.UpdatePositions(hitByMouseClick.GroundCell);

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

        private void SelectWithLeftClick(HitByMouseClick hitByMouseClick)
        {
            SelectNothing();

            if (hitByMouseClick.UnitBase != null)
            {
                HighlightUnitFrame(hitByMouseClick.UnitBase);
                SelectUnitFrame(hitByMouseClick.UnitBase);
                SetMode(CanvasMode.Unit);
            }
            else
            {
                if (hitByMouseClick.CommandPreview != null)
                {
                    if (selectedCommandPreview != null)
                    {
                        selectedCommandPreview.SetSelected(false);
                    }
                    HighlightGameCommand(hitByMouseClick.CommandPreview);

                    selectedCommandPreview = hitByMouseClick.CommandPreview;
                    selectedCommandPreview.SetSelected(true);

                    StringBuilder sb = new StringBuilder();

                    sb.Append("GameCommandType: ");
                    sb.AppendLine(selectedCommandPreview.GameCommand.GameCommandType.ToString());

                    if (selectedCommandPreview.GameCommand.BlueprintName != null)
                    {
                        sb.Append("Blueprint: ");
                        sb.AppendLine(selectedCommandPreview.GameCommand.BlueprintName);
                    }
                    sb.Append("DisplayPosition: ");
                    sb.AppendLine(selectedCommandPreview.DisplayPosition.ToString());

                    sb.AppendLine("AttachedUnit" + selectedCommandPreview.GameCommand.AttachedUnit.ToString());
                    sb.AppendLine("FactoryUnit" + selectedCommandPreview.GameCommand.FactoryUnit.ToString());
                    sb.AppendLine("TransportUnit" + selectedCommandPreview.GameCommand.TransportUnit.ToString());
                    sb.AppendLine("TargetUnit" + selectedCommandPreview.GameCommand.TargetUnit.ToString());
                    sb.AppendLine(selectedCommandPreview.GameCommand.ToString());

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
                                highlightedCommandPreview.SetActive(false);
                            }
                        }
                        HighlightGameCommand(hitByMouseClick.CommandPreview);
                    }

                    if (hitByMouseClick.UnitBase != null)
                    {
                        DisplayUnitframe(hitByMouseClick.UnitBase);
                    }
                    else
                    {
                        DisplayGameCommand(highlightedCommandPreview);
                    }
                }
                else if (hitByMouseClick.UnitBase != null)
                {
                    HideAllParts();
                    DisplayUnitframe(hitByMouseClick.UnitBase);
                }
                else if (hitByMouseClick.GroundCell != null)
                {
                    ShowNothing();
                    DisplayGroundInfo(hitByMouseClick.GroundCell);
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
                                highlightedCommandPreview.SetHighlighted(false);
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
                    UnHighlightGroundCell();
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
            UnHighlightGroundCell();
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
            HideBuildMenu();
            UnselectUnitFrame();

            CloseCommandPreview();
            SetMode(CanvasMode.Select);
            
            ShowNothing();
        }

        private CommandPreview highlightedCommandPreview;

        private void DisplayUpdateStatsCommand(UnitBase unit)
        {
            if (unit.MoveUpdateStats == null)
            {
                alertHeaderText.text = "NoStats!";
                alertText.text = "";
            }
            else if (unit.MoveUpdateStats.MoveUpdateStatsCommand == null)
            {
                alertHeaderText.text = "Automatic";
                alertText.text = "";
            }
            else
            {
                alertHeaderText.text = unit.MoveUpdateStats.MoveUpdateStatsCommand.GameCommandType.ToString();
                alertText.text = unit.MoveUpdateStats.MoveUpdateStatsCommand.GameCommandState.ToString();
                //unit.UnitAlert.Text;
            }
        }

        private void DisplayGroundInfo(GroundCell groundCell)
        {
            AppendGroundInfo(groundCell, true);

            panelContainer.transform.Find("Partname").GetComponent<Text>().text = "Resources";
            panelContainer.SetActive(true);
            UpdateContainer(groundCell.TileCounter, null);
        }

        private void DisplayGameCommand(CommandPreview commandPreview)
        {
            if (commandPreview == null) return;

            Position2 position2;
            if (commandPreview.IsPreview)
            {
                position2 = commandPreview.DisplayPosition;
            }
            else
            {
                position2 = commandPreview.GameCommand.TargetPosition;
            }
            MapGameCommand gameCommand = commandPreview.GameCommand;
            headerText.text = gameCommand.GameCommandType.ToString() + " State: " + gameCommand.GameCommandState; //.PlayerId;
            //headerSubText.text = "Radius " + gameCommand.Radius.ToString() + " sel: " + commandPreview.IsSelected.ToString();

            headerSubText.text = gameCommand.AttachedUnit.Status;
            if (gameCommand.AttachedUnit.Alert)
                headerSubText.text += " ALERT";


            if (commandPreview.GameCommand.GameCommandType == GameCommandType.Collect &&
                commandPreview.CollectBounds != null)
            {
                TileCounter tileCounter = new TileCounter();

                foreach (Position2 position in commandPreview.CollectBounds.CollectedPositions)
                {
                    GroundCell gc;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(position, out gc))
                    {
                        tileCounter.Add(gc.TileCounter);
                    }
                }

                panelContainer.transform.Find("Partname").GetComponent<Text>().text = "Resources";
                panelContainer.SetActive(true);
                UpdateContainer(tileCounter, null);
            }
            else
            {

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

        private void UpdateContainer(TileCounter tileCounter, UnitBase unitBase)
        {
            foreach (CanvasItem canvasItem in canvasItems)
            {
                canvasItem.TileObjectState = TileObjectState.None;
            }

            if (unitBase != null && unitBase.MoveUpdateStats != null && unitBase.MoveUpdateStats.MoveUnitItemOrders != null)
            {
                foreach (MoveUnitItemOrder order in unitBase.MoveUpdateStats.MoveUnitItemOrders)
                {
                    foreach (CanvasItem canvasItem in canvasItems)
                    {
                        if (canvasItem.TileObjectType == order.TileObjectType)
                        {
                            canvasItem.TileObjectState = order.TileObjectState;
                        }
                    }
                }
            }

            foreach (CanvasItem canvasItem in canvasItems)
            {
                if (canvasItem.TileObjectType == TileObjectType.Mineral)
                    canvasItem.SetCount(tileCounter.Mineral);
                if (canvasItem.TileObjectType == TileObjectType.Wood)
                    canvasItem.SetCount(tileCounter.Wood);
                if (canvasItem.TileObjectType == TileObjectType.Stone)
                    canvasItem.SetCount(tileCounter.Stone);
                canvasItem.UpdateImage();
                canvasItem.State.SetActive(false);
            }
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

            bool showContainer = false;
            TileCounter tileCounter = new TileCounter();
            if (unit.MoveUpdateStats == null)
            {
            }
            else
            {
                if (unit.MoveUpdateStats != null)
                {
                    panelUnit.ShowBluePrint(unit.MoveUpdateStats.BlueprintName, unit.PlayerId);

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
                    //headerSubText.text += " Dir: " + unit.MoveUpdateStats.Direction;

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
                            //panelExtractor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            //panelExtractor.SetActive(true);
                        }
                        if (part.PartType == TileObjectType.PartWeapon)
                        {
                            panelWeapon.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            if (part.TileObjects != null)
                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = "Ammo " + part.TileObjects.Count.ToString();
                            else
                                panelWeapon.transform.Find("Content").GetComponent<Text>().text = "No Container";
                            panelWeapon.SetActive(true);

                            /*
                            if (part.TileObjects != null)
                            {
                                tileCounter.Add(part.TileObjects.AsReadOnly());
                                showContainer = true;
                            }*/
                        }
                        if (part.PartType == TileObjectType.PartAssembler)
                        {
                            panelAssembler.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelAssembler.SetActive(true);

                            /*
                            if (part.TileObjects != null)
                            {
                                tileCounter.Add(part.TileObjects.AsReadOnly());
                                showContainer = true;
                            }*/
                        }
                        if (part.PartType == TileObjectType.PartReactor)
                        {
                            panelReactor.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            panelReactor.transform.Find("Content").GetComponent<Text>().text = "Power " + part.AvailablePower.ToString();
                            panelReactor.SetActive(true);

                            /*
                            if (part.TileObjects != null)
                            {
                                tileCounter.Add(part.TileObjects.AsReadOnly());
                                showContainer = true;
                            }*/
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
                            //panelEngine.transform.Find("Partname").GetComponent<Text>().text = part.Name + state;
                            //panelEngine.SetActive(true);
                        }
                        if (part.PartType == TileObjectType.PartContainer && part.TileObjects != null)
                        {
                            tileCounter.Add(part.TileObjects.AsReadOnly());
                            showContainer = true;
                        }
                    }
                }
            }
            if (showContainer)
            {
                panelContainer.transform.Find("Partname").GetComponent<Text>().text = "Container";
                panelContainer.SetActive(true);
                UpdateContainer(tileCounter, unit);
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