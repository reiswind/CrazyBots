using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    internal class HitByMouseClick
    {
        public UnitBase UnitFrame { get; set; }
        public GroundCell GroundCell { get; set; }
        public GameCommand GameCommand { get; set; }
    }

    internal enum CanvasMode
    {
        None,
        Select,
        Build,
        Collect,
        Attack,
        Unit,
        Command
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
        //private Button buildButton;
        //private Text buildButtonText;

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

            /*
            buildButton = panelItem.Find("BuildButton").GetComponent<Button>();
            buildButton.gameObject.SetActive(false);
            buildButton.onClick.AddListener(OnClickExtract);
            buildButtonText = buildButton.transform.Find("Text").GetComponent<Text>();
            */
            Transform panelParts = panelItem.Find("PanelParts");

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
            HexGrid.StartGame();

            UpdateCommandButtons();
        }

        void SetMode(CanvasMode newCanvasMode)
        {
            if (canvasMode != newCanvasMode)
            {
                canvasMode = newCanvasMode;

                UpdateCommandButtons();
                //UnSelectTopButton();

                SetActionText(1, canvasMode.ToString());

                if (canvasMode == CanvasMode.Select)
                {
                    SelectAction(1);
                    UnselectAction(2);
                    UnselectAction(3);
                    UnselectAction(4);

                    //Cursor.SetCursor(NormalCursor, new Vector2(0, 0), CursorMode.Auto);

                    //if (topSelectedSelectButton == 0)
                    //    topSelectedSelectButton = 1;
                    UnselectButton(1);
                }

                if (canvasMode == CanvasMode.Unit)
                {
                    UnselectAction(1);
                    SelectAction(2);
                    UnselectAction(3);
                    UnselectAction(4);

                    //Cursor.SetCursor(AttackCursor, new Vector2(0, 0), CursorMode.Auto);

                    /*
                    if (topSelectedAttackButton == 0)
                        topSelectedAttackButton = 1;
                    SelectTopButton(topSelectedAttackButton);
                    */
                }
                /*
                if (canvasMode == CanvasMode.Mineral)
                {
                    UnselectAction(1);
                    UnselectAction(2);
                    SelectAction(3);
                    UnselectAction(4);

                    //Cursor.SetCursor(AttackCursor, new Vector2(0, 0), CursorMode.Auto);

                }*/

                if (canvasMode == CanvasMode.Build)
                {
                    UnselectAction(1);
                    UnselectAction(2);
                    SelectAction(3);
                    UnselectAction(3);

                    //Cursor.SetCursor(BuildCursor, new Vector2(0, 0), CursorMode.Auto);

                    //if (topSelectedBuildButton == 0)
                    //    topSelectedBuildButton = 1;
                    //SelectTopButton(topSelectedBuildButton);
                }
            }
            leftMouseButtonDown = false;

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
            if (canvasMode != CanvasMode.Unit)
            {
                SetMode(CanvasMode.Unit);
            }
        }

        void OnClickAction3()
        {
            if (canvasMode != CanvasMode.Build)
            {
                SetMode(CanvasMode.Build);
            }
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

        private void UnselectGameCommand()
        {
            executedBlueprintCommand = null;
            movedGameCommand = null;

            if (selectedGameCommand != null)
            {
                selectedGameCommand = null;
            }
            if (previewGameCommand != null)
            {
                Destroy(previewGameCommand);
                previewGameCommand = null;
            }
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

        //private int topSelectedSelectButton;
        //private int topSelectedAttackButton;
        //private int topSelectedMineralsButton;
        //private int topSelectedBuildButton;
        //private int middleSelectedButton;

        private int selectedBuildButton;
        private int selectedUnitButton;


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
            if (groundCell != null)
            {
                // SAME as Tileobject
                MoveUpdateGroundStat stats = groundCell.Stats.MoveUpdateGroundStat;
                if (stats.IsUnderwater)
                    return false;

                int mins = 0;
                foreach (TileObject tileObject in stats.TileObjects)
                {
                    if (tileObject.TileObjectType == TileObjectType.Mineral)
                    {
                        mins++;
                    }
                    else if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType))
                        return false;
                    else if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                        return false;
                }

                if (mins >= 20)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        private bool CanCommandAt(GroundCell groundCell)
        {
            if (groundCell == null)
            {
                foreach (TileObject tileObject in groundCell.Stats.MoveUpdateGroundStat.TileObjects)
                {
                    if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                        return false;
                }
            }
            return true;
        }

        private void UpdateCommandButtons()
        {
            if (canvasMode == CanvasMode.Select)
            {
                int idx = 1;
                if (HexGrid.game != null)
                {
                    foreach (BlueprintCommand blueprintCommand in HexGrid.game.Blueprints.Commands)
                    {
                        SetButtonText(idx, blueprintCommand.Name);
                        idx++;
                    }
                }
                while (idx <= 12)
                {
                    HideButton(idx++);
                }
            }

            if (canvasMode == CanvasMode.Unit)
            {
                SetButtonText(1, "(q) Attack");
                SetButtonText(2, "(w) Defend");
                SetButtonText(3, "(e) Scout");
                SetButtonText(4, "(r) Collect");
                HideButton(5);
                HideButton(6);
                HideButton(7);
                HideButton(8);
                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);
            }
            if (canvasMode == CanvasMode.Command)
            {
                SetButtonText(1, "(q) Move");
                SetButtonText(2, "(w) Attack");
                SetButtonText(3, "(e) Reinforce");
                SetButtonText(4, "(r) Cancel");
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
                SetButtonText(1, "(q) Rotate right");
                SetButtonText(2, "(w) Rotate left");
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

            if (canvasMode == CanvasMode.Build)
            {
                SetButtonText(1, "(q) Factory", "Factory");
                SetButtonText(2, "(w) Container", "Container");
                SetButtonText(3, "(e) Reactor", "Reactor");
                SetButtonText(4, "(e) Turret", "Turret");

                SetButtonText(5, "(a) Outpost", "Outpost");

                HideButton(6);
                HideButton(7);
                HideButton(8);
                HideButton(9);
                HideButton(10);
                HideButton(11);
                HideButton(12);

                if (selectedBuildButton != 0)
                {
                    UnselectButton(selectedBuildButton);
                    selectedBuildButton = 0;
                }
                selectedBuildButton = 0;
            }
        }
        

        void SelectActionUnit(int btn)
        {
            if (selectedUnitButton != btn)
            {
                if (selectedUnitButton != 0)
                    UnselectButton(selectedUnitButton);

                selectedUnitButton = btn;
                SelectButton(btn);
            }
        }

        void SelectBuildUnit(int btn)
        {
            if (selectedBuildButton != btn)
            {
                if (selectedBuildButton != 0)
                    UnselectButton(selectedBuildButton);

                selectedBuildButton = btn;
                SelectButton(btn);
            }
            SelectBlueprint(GetButton(btn).name);
            selectedUnitFrame.Assemble(true);
        }

        private GameObject previewGameCommand;
        private GameCommand movedGameCommand;

        void MoveCommand()
        {
            if (selectedGameCommand == null)
                return;
            movedGameCommand = selectedGameCommand;
            previewGameCommand = CreateCommandPreview(selectedGameCommand.BlueprintCommand);
        }
        static GameObject CreateCommandPreview(BlueprintCommand blueprintCommand)
        {
            GameObject previewGameCommand;
            string layout = "UIBuild";

            if (blueprintCommand != null &&
                !string.IsNullOrEmpty(blueprintCommand.Layout))
                layout = blueprintCommand.Layout;

            previewGameCommand = Instantiate(HexGrid.MainGrid.GetResource(layout));
            UnitBase.RemoveColider(previewGameCommand);

            Command command = previewGameCommand.GetComponent<Command>();
            command.IsPreview = true;

            foreach (BlueprintCommandItem blueprintCommandItem in blueprintCommand.Units)
            {
                Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(blueprintCommandItem.BlueprintName);
                UnitBase previewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint);
                previewUnit.DectivateUnit();

                UnitBase.RemoveColider(previewUnit.gameObject);
                previewUnit.gameObject.SetActive(true);
                previewUnit.transform.SetParent(previewGameCommand.transform, false);
            }
            return previewGameCommand;
        }

        void CancelCommand()
        {
            if (selectedGameCommand == null)
                return;

            // Just in case it has not been transmitted yet
            HexGrid.ActiveGameCommands.Remove(selectedGameCommand.TargetPosition);

            GameCommand gameCommand = new GameCommand();

            gameCommand.GameCommandType = GameCommandType.Cancel;
            gameCommand.TargetPosition = selectedGameCommand.TargetPosition;
            gameCommand.PlayerId = selectedGameCommand.PlayerId;

            HexGrid.GameCommands.Add(gameCommand.TargetPosition, gameCommand);

            GroundCell groundCell = HexGrid.GroundCells[selectedGameCommand.TargetPosition];
            groundCell.RemoveGameCommand(selectedGameCommand);

            UnselectGameCommand();

            SetMode(CanvasMode.Select);

        }

        void MarkUnitForExtraction()
        {
            // Extract the unit
            GameCommand gameCommand = new GameCommand();

            gameCommand.UnitId = selectedUnitFrame.UnitId;
            gameCommand.TargetPosition = selectedUnitFrame.CurrentPos;
            gameCommand.GameCommandType = GameCommandType.Extract;
            gameCommand.PlayerId = 1;
            HexGrid.GameCommands.Add(selectedUnitFrame.CurrentPos, gameCommand);

            selectedUnitFrame.MoveUpdateStats.MarkedForExtraction = true;
        }

        private BlueprintCommand executedBlueprintCommand;
        void ExecuteCommand(int btn)
        {

            if (selectedBuildButton != btn)
            {
                if (selectedBuildButton != 0)
                    UnselectButton(selectedBuildButton);

                selectedBuildButton = btn;
                SelectButton(btn);
            }
            if (canvasMode == CanvasMode.Select)
            {
                BlueprintCommand blueprintCommand = HexGrid.game.Blueprints.Commands[btn - 1];
                if (blueprintCommand.GameCommandType == GameCommandType.Build)
                {
                    SelectBlueprint(blueprintCommand.Units[0].BlueprintName);

                    canvasMode = CanvasMode.Build;
                    executedBlueprintCommand = blueprintCommand;
                }
                else if (blueprintCommand.GameCommandType == GameCommandType.Collect)
                {
                    canvasMode = CanvasMode.Collect;
                    executedBlueprintCommand = blueprintCommand;
                }
                else if (blueprintCommand.GameCommandType == GameCommandType.Attack)
                {
                    SetMode(CanvasMode.Attack);
                    
                    executedBlueprintCommand = blueprintCommand;
                    previewGameCommand = CreateCommandPreview(executedBlueprintCommand);
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
        private GameCommand selectedGameCommand;
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

                Command command = raycastHit.collider.gameObject.GetComponent<Command>();
                hitByMouseClick.GroundCell = raycastHit.collider.gameObject.GetComponent<GroundCell>();
                hitByMouseClick.UnitFrame = GetUnitFrameFromRayCast(raycastHit);

                if (command != null)
                {
                    hitByMouseClick.GameCommand = command.GameCommand;
                    if (hitByMouseClick.GroundCell == null && hitByMouseClick.GameCommand != null)
                    {
                        hitByMouseClick.GroundCell = HexGrid.GroundCells[hitByMouseClick.GameCommand.TargetPosition];
                    }
                }

                if (hitByMouseClick.UnitFrame != null && hitByMouseClick.GroundCell == null)
                {
                    if (hitByMouseClick.UnitFrame.CurrentPos == null)
                    {
                        Debug.Log("Raycast hit " + raycastHit.collider.gameObject.name);
                    }
                    else
                    {
                        hitByMouseClick.GroundCell = HexGrid.GroundCells[hitByMouseClick.UnitFrame.CurrentPos];
                    }
                }
                else if (hitByMouseClick.UnitFrame == null && hitByMouseClick.GroundCell != null)
                {
                    foreach (UnitBase unitFrame in HexGrid.BaseUnits.Values)
                    {
                        if (unitFrame.CurrentPos == hitByMouseClick.GroundCell.Pos)
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

        private void AppendGroundInfo(GroundCell gc)
        {
            /*
            GameCommand gameCommand = null;
            if (HexGrid.ActiveGameCommands != null &&
                HexGrid.ActiveGameCommands.ContainsKey(lastSelectedGroundCell.Pos))
            {
                gameCommand = HexGrid.ActiveGameCommands[lastSelectedGroundCell.Pos];
            }*/

            StringBuilder sb = new StringBuilder();

            sb.Append("P: " + gc.Pos.X + ", " + gc.Pos.Y);

            //sb.Append(" TI: " + gc.Stats.MoveUpdateGroundStat.TerrainTypeIndex);
            //sb.Append(" PI: " + gc.Stats.MoveUpdateGroundStat.PlantLevel);
            sb.Append(" Z: " + gc.Stats.MoveUpdateGroundStat.ZoneId);
            //sb.Append(" Owner: " + gc.Stats.MoveUpdateGroundStat.Owner);

            if (selectedUnitFrame == null)
            {
                /*
                if (gameCommand != null)
                {
                    if (gameCommand.GameCommandType == GameCommandType.Collect)
                    {
                        headerText.text = "Collect";
                    }
                    else if (gameCommand.GameCommandType == GameCommandType.Attack)
                    {
                        headerText.text = "Attack";
                    }
                    else if (gameCommand.GameCommandType == GameCommandType.Defend)
                    {
                        headerText.text = "Defend";
                    }
                    else if (gameCommand.GameCommandType == GameCommandType.Scout)
                    {
                        headerText.text = "Scout";
                    }
                    else
                    {
                        headerText.text = "Command";
                    }
                }
                else
                */
                {
                    if (gc.GameObjects.Count > 0)
                    {
                        headerText.text = "Destructable";
                    }
                    else
                    {
                        headerText.text = "Ground";
                    }
                }
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
            panelCommand.SetActive(false);
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
                /*
                if (unitBase.Temporary)
                {
                    buildButton.gameObject.SetActive(false);
                }
                else
                {
                    buildButton.gameObject.SetActive(true);
                }*/
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
            else if (canvasMode == CanvasMode.Build)
            {
                UpdateBuildMode();
            }
            else if (canvasMode == CanvasMode.Unit)
            {
                UpdateAttackMode();
            }
            else if (canvasMode == CanvasMode.Collect)
            {
                UpdateCollectMode();
            }
            else if (canvasMode == CanvasMode.Attack)
            {
                UpdateAttackMode();
            }
            else if (canvasMode == CanvasMode.Command)
            {
                UpdateCommandMode();
            }
        }

        private bool leftMouseButtonDown;

        private bool CheckMouseButtons()
        {
            if (Input.GetMouseButtonDown(0))
            {
                leftMouseButtonDown = true;
                lastSelectedGroundCell = null;
            }
            if (Input.GetMouseButtonUp(0))
            {
                leftMouseButtonDown = false;
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (canvasMode != CanvasMode.Select)
                {
                    UnselectGameCommand();
                    UnselectUnitFrame();
                    
                    SetMode(CanvasMode.Select);

                    if (lastSelectedGroundCell != null)
                    {
                        lastSelectedGroundCell.SetSelected(false);
                        lastSelectedGroundCell = null;
                    }
                    return true;
                }
            }
            return false;
        }

        private void UpdateCommandPreviewPosition(HitByMouseClick hitByMouseClick)
        {
            if (previewGameCommand != null && hitByMouseClick != null &&
                hitByMouseClick.GroundCell != null)
            {
                Debug.Log("preview " + hitByMouseClick.GroundCell.Pos.X + ", " + hitByMouseClick.GroundCell.Pos.Y);

                previewGameCommand.transform.SetParent(hitByMouseClick.GroundCell.transform, false);

                Vector3 unitPos3 = hitByMouseClick.GroundCell.transform.position;
                unitPos3.y += 0.09f;
                previewGameCommand.transform.position = unitPos3;
            }
        }

        void UpdateCommandMode()
        {
            if (CheckMouseButtons()) return;

            HitByMouseClick hitByMouseClick = GetClickedInfo();
            UpdateCommandPreviewPosition(hitByMouseClick);

            if (leftMouseButtonDown)
            {
                if (hitByMouseClick != null)
                {
                    lastSelectedGroundCell = hitByMouseClick.GroundCell;

                    if (leftMouseButtonDown &&
                        lastSelectedGroundCell != null &&
                        movedGameCommand != null &&
                        CanCommandAt(lastSelectedGroundCell))
                    {
                        GameCommand gameCommand = new GameCommand();

                        gameCommand.TargetPosition = movedGameCommand.TargetPosition;
                        gameCommand.GameCommandType = movedGameCommand.GameCommandType;
                        gameCommand.BlueprintCommand = movedGameCommand.BlueprintCommand;
                        gameCommand.GameCommandType = GameCommandType.Move;
                        gameCommand.PlayerId = 1;
                        gameCommand.MoveToPosition = lastSelectedGroundCell.Pos;

                        HexGrid.GameCommands.Add(lastSelectedGroundCell.Pos, gameCommand);

                        UnselectGameCommand();
                        UpdateSelectMode();
                    }
                    else
                    {
                        if (hitByMouseClick.GameCommand != selectedGameCommand)
                        {
                            if (movedGameCommand != null)
                            {

                            }
                            else
                            {
                                UnselectGameCommand();
                                UpdateSelectMode();
                            }
                        }
                    }
                }
            }
        }

        void UpdateAttackMode()
        {
            if (CheckMouseButtons()) return;

            HitByMouseClick hitByMouseClick = GetClickedInfo();
            UpdateCommandPreviewPosition(hitByMouseClick);

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
                    CanCommandAt(lastSelectedGroundCell))
                {
                    Position pos = lastSelectedGroundCell.Pos;
                    if (HexGrid.GameCommands.ContainsKey(pos))
                        HexGrid.GameCommands.Remove(pos);

                    if (HexGrid.ActiveGameCommands.ContainsKey(pos))
                    {
                        // Remove it
                        /*
                        HexGrid.ActiveGameCommands.Remove(pos);

                        // Cancel the command
                        GameCommand gameCommand = new GameCommand();
                        gameCommand.TargetPosition = pos;
                        gameCommand.GameCommandType = GameCommandType.Cancel;
                        gameCommand.PlayerId = 1;
                        HexGrid.GameCommands.Add(pos, gameCommand);
                        */
                        //lastSelectedGroundCell.SetAttack(false);
                    }
                    else
                    {
                        GameCommand gameCommand = new GameCommand();

                        gameCommand.TargetPosition = pos;
                        gameCommand.GameCommandType = executedBlueprintCommand.GameCommandType;
                        gameCommand.BlueprintCommand = executedBlueprintCommand;
                        gameCommand.GameCommandType = GameCommandType.Attack;

                        gameCommand.PlayerId = 1;
                        HexGrid.GameCommands.Add(pos, gameCommand);

                        GroundCell groundCell = HexGrid.GroundCells[gameCommand.TargetPosition];
                        CellGameCommand cellGameCommand = groundCell.UpdateCommands(gameCommand, null);
                        // XXX
                        if (selectedBuildButton != 0)
                        {
                            UnselectButton(selectedBuildButton);
                            selectedBuildButton = 0;
                        }

                        UnselectUnitFrame();
                        UnselectGameCommand();
                        selectedGameCommand = cellGameCommand.GameCommand;

                        DisplaySelectedGameCommand();
                        
                        SetMode(CanvasMode.Command);
                        
                    }
                }
            }
        }

        void UpdateCollectMode()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (selectedBuildButton != 0)
                {
                    UnselectButton(selectedBuildButton);
                    selectedBuildButton = 0;
                }
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

                if (!leftMouseButtonDown)
                {
                    // Preview
                    if (lastSelectedGroundCell == null)
                    {
                    }
                    else
                    {
                    }
                }

                if (leftMouseButtonDown &&
                    lastSelectedGroundCell != null)
                {
                    Position pos = lastSelectedGroundCell.Pos;

                    GameCommand gameCommand = new GameCommand();
                    gameCommand.TargetPosition = pos;
                    gameCommand.GameCommandType = executedBlueprintCommand.GameCommandType;
                    gameCommand.BlueprintCommand = executedBlueprintCommand;
                    gameCommand.PlayerId = 1;
                    HexGrid.GameCommands.Add(pos, gameCommand);

                    lastSelectedGroundCell.UpdateCommands(gameCommand, selectedUnitFrame);
                    selectedUnitFrame = null;

                    // Build one per click
                    SetMode( CanvasMode.Select);
                }
            }
        }

        void UpdateBuildMode()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (selectedBuildButton != 0)
                {
                    UnselectButton(selectedBuildButton);
                    selectedBuildButton = 0;
                }

                UnselectUnitFrame();
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

                if (!leftMouseButtonDown)
                {
                    // Preview
                    if (lastSelectedGroundCell == null || !CanBuildAt(lastSelectedGroundCell))
                    {
                        if (selectedUnitFrame != null && selectedUnitFrame.CurrentPos != null)
                        {
                            selectedUnitFrame.CurrentPos = null;
                            selectedUnitFrame.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (selectedUnitFrame != null &&
                            selectedUnitFrame.CurrentPos != lastSelectedGroundCell.Pos)
                        {
                            selectedUnitFrame.CurrentPos = lastSelectedGroundCell.Pos;
                            selectedUnitFrame.PutAtCurrentPosition(true);
                            selectedUnitFrame.gameObject.SetActive(true);
                        }
                    }
                }

                if (leftMouseButtonDown &&
                    selectedUnitFrame != null &&
                    lastSelectedGroundCell != null &&
                    CanBuildAt(lastSelectedGroundCell))
                {
                    Position pos = lastSelectedGroundCell.Pos;
                    selectedUnitFrame.CurrentPos = pos;

                    if (HexGrid.GameCommands.ContainsKey(pos))
                        HexGrid.GameCommands.Remove(pos);

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
                        HexGrid.GameCommands.Add(pos, gameCommand);
                    }
                    else
                    {
                        // Build the temp. unit
                        GameCommand gameCommand = new GameCommand();
                        //gameCommand.UnitId = selectedUnitFrame.MoveUpdateStats.BlueprintName;
                        gameCommand.TargetPosition = pos;
                        gameCommand.GameCommandType = GameCommandType.Build;
                        gameCommand.BlueprintCommand = executedBlueprintCommand;
                        gameCommand.PlayerId = 1;
                        HexGrid.GameCommands.Add(pos, gameCommand);

                        HexGrid.UnitsInBuild.Add(pos, selectedUnitFrame);

                        lastSelectedGroundCell.UpdateCommands(gameCommand, selectedUnitFrame);
                        selectedUnitFrame = null;

                        // Prepare next (not working)
                        //SelectBuildUnit(selectedBuildButton);

                        // Build one per click
                        SetMode(CanvasMode.Select);

                    }
                }

            }
            if (selectedUnitFrame != null)
            {
                DisplaySelectedUnitframe();
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
            /*
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SelectedBluePrint = null;
                UpdateCommandButtons();
            }*/

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

                CanvasMode nextCanvasMode = canvasMode;
                if (hitByMouseClick != null)
                {
                    if (hitByMouseClick.GameCommand != null)
                    {
                        UnselectGameCommand();
                        UnselectUnitFrame();
                        selectedGameCommand = hitByMouseClick.GameCommand;
                        nextCanvasMode = CanvasMode.Command;
                    }
                    else
                    {
                        UnselectGameCommand();

                        if (lastSelectedGroundCell != hitByMouseClick.GroundCell)
                        {
                            if (lastSelectedGroundCell != null)
                                lastSelectedGroundCell.SetSelected(false);
                            if (hitByMouseClick.GroundCell != null && hitByMouseClick.UnitFrame == null)
                                hitByMouseClick.GroundCell.SetSelected(true);

                            lastSelectedGroundCell = hitByMouseClick.GroundCell;
                        }

                        if (selectedUnitFrame != hitByMouseClick.UnitFrame)
                        {
                            UnselectUnitFrame();
                            SelectUnitFrame(hitByMouseClick.UnitFrame);
                        }
                    }
                    SetMode(nextCanvasMode);
                }
            }
            if (selectedUnitFrame != null)
            {
                DisplaySelectedUnitframe();
            }
            else if (selectedGameCommand != null)
            {
                DisplaySelectedGameCommand();
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

        private void DisplaySelectedGameCommand()
        {
            if (selectedGameCommand == null)
            {
                return;
            }
            if (selectedGameCommand.CommandComplete)
            {
                UnselectGameCommand();
                
                return;
            }
            HideAllParts();
            //selectedGameCommand.SetSelected(true);
            GameCommand gameCommand =  selectedGameCommand;

            headerText.text = gameCommand.BlueprintCommand.Name;
            headerSubText.text = gameCommand.GameCommandType.ToString() + " " + gameCommand.UnitId;
            headerGroundText.text = gameCommand.TargetPosition.X + ", " + gameCommand.TargetPosition.Y;

            //selectedGameCommand.UpdateAttachedUnits(HexGrid);
            /*
            List<UnitBase> allCommandUnits = new List<UnitBase>();
            allCommandUnits.AddRange(selectedCommandUnits);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string unitId in gameCommand.AttachedUnits)
            {
                UnitBase unitBase = null;
                if (unitId.StartsWith("Assembler"))
                {
                    unitBase = HexGrid.BaseUnits[unitId.Substring(10)];
                }
                else
                {
                    HexGrid.BaseUnits.TryGetValue(unitId, out unitBase);
                }
                if (unitBase != null)
                {
                    allCommandUnits.Remove(unitBase);
                    if (!selectedCommandUnits.Contains(unitBase))
                    {
                        selectedCommandUnits.Add(unitBase);
                        unitBase.SetSelected(true);
                    }
                }
                stringBuilder.Append(unitId);
            }
            foreach (UnitBase unitBase in allCommandUnits)
            {
                unitBase.SetSelected(false);
            }
            */
            //panelCommand.transform.Find("Partname").GetComponent<Text>().text = stringBuilder.ToString();
            panelCommand.SetActive(true);
        }

        private void DisplaySelectedUnitframe()
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
                        if (unit.MoveUpdateStats.MoveUpdateStatsCommand == null)
                        {
                            headerSubText.text = "";
                        }
                        else
                        {
                            MoveUpdateStatsCommand cmd = unit.MoveUpdateStats.MoveUpdateStatsCommand;

                            headerSubText.text = cmd.GameCommandType.ToString() + " at " + cmd.TargetPosition.X + "," + cmd.TargetPosition.Y;
                        }
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

            if (unit.CurrentPos != null)
            {
                GroundCell gc;
                if (HexGrid.GroundCells.TryGetValue(unit.CurrentPos, out gc))
                    AppendGroundInfo(gc);
            }
        }
    }
}