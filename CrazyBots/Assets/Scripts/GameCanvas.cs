using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

internal class HitByMouseClick
{
    public UnitFrame UnitFrame { get; set; }
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

    // Start is called before the first frame update
    void Start()
    {
        UIMineralText = MineralText.GetComponent<Text>();
        UISelectedObjectText = SelectedObjectText.GetComponent<Text>();
        UISelectedObjectsText = SelectedObjectsText.GetComponent<Text>();

        HexGrid.StartGame();
    }

    private UnitFrame selectedUnitFrame;
    private GroundCell lastSelectedGroundCell;
    

    private HitByMouseClick GetClickedInfo()
    {
        HitByMouseClick hitByMouseClick = null;

        RaycastHit raycastHit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, Mathf.Infinity))
        {
            hitByMouseClick = new HitByMouseClick();
            hitByMouseClick.GroundCell = raycastHit.collider.gameObject.GetComponent<GroundCell>();
            hitByMouseClick.UnitFrame = GetUnitFrameFromRayCast(raycastHit);
            if (hitByMouseClick.UnitFrame != null && hitByMouseClick.GroundCell == null)
            {
                hitByMouseClick.GroundCell = HexGrid.GroundCells[hitByMouseClick.UnitFrame.currentPos];
            }
            if (hitByMouseClick.UnitFrame == null && hitByMouseClick.GroundCell != null)
            {
                foreach (UnitFrame unitFrame in HexGrid.Units.Values)
                {
                    if (unitFrame.currentPos == hitByMouseClick.GroundCell.Tile.Pos)
                    {
                        hitByMouseClick.UnitFrame = unitFrame;
                        break;
                    }
                }
            }
        }

        return hitByMouseClick;
    }

    private UnitFrame GetUnitFrameFromRayCast(RaycastHit raycastHit)
    {
        Engine1 engine1 = raycastHit.collider.GetComponent<Engine1>();
        if (engine1 != null) return engine1.UnitFrame;
        Armor armor = raycastHit.collider.GetComponent<Armor>();
        if (armor != null) return armor.UnitFrame;
        Weapon1 weapon1 = raycastHit.collider.GetComponent<Weapon1>();
        if (weapon1 != null) return weapon1.UnitFrame;
        Assembler1 assembler1 = raycastHit.collider.GetComponent<Assembler1>();
        if (assembler1 != null) return assembler1.UnitFrame;
        Container1 container1 = raycastHit.collider.GetComponent<Container1>();
        if (container1 != null) return container1.UnitFrame;
        Extractor1 extractor1 = raycastHit.collider.GetComponent<Extractor1>();
        if (extractor1 != null) return extractor1.UnitFrame;
        Reactor1 reactor1 = raycastHit.collider.GetComponent<Reactor1>();
        if (reactor1 != null) return reactor1.UnitFrame;

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

        if (Input.GetMouseButtonDown(1))
        {
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
                /*
                if (gameCommand.Append)
                    Debug.Log("Move to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y + " SHIFT");
                else
                    Debug.Log("Move to " + gameCommand.TargetPosition.X + "," + gameCommand.TargetPosition.Y);
                */
            }

        }

        if (Input.GetMouseButtonDown(0))
        {
            HitByMouseClick hitByMouseClick = GetClickedInfo();

            if (hitByMouseClick == null)
            {
                if (lastSelectedGroundCell != null)
                {
                    lastSelectedGroundCell.SetSelected(false);
                }
                lastSelectedGroundCell = null;
                selectedUnitFrame = null;
            }
            else
            {
                if (lastSelectedGroundCell != hitByMouseClick.GroundCell)
                {
                    if (lastSelectedGroundCell != null)
                        lastSelectedGroundCell.SetSelected(false);
                    if (hitByMouseClick.GroundCell != null)
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
            }
        }
        if (selectedUnitFrame != null)
        {
            StringBuilder sb = new StringBuilder();

            UnitFrame unit = selectedUnitFrame;

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
                    if (unit.MoveUpdateStats.ArmorLevel > 0)
                    {
                        sb.AppendLine("Armor: " + unit.MoveUpdateStats.ArmorLevel);
                    }
                    if (unit.MoveUpdateStats.ContainerLevel > 0)
                    {
                        sb.AppendLine("Container: " + unit.MoveUpdateStats.ContainerLevel + " Full: " + unit.MoveUpdateStats.ContainerFull + "%");
                    }
                    if (unit.MoveUpdateStats.EngineLevel > 0)
                    {
                        sb.AppendLine("Engine: " + unit.MoveUpdateStats.EngineLevel);
                    }
                    if (unit.MoveUpdateStats.ExtractorLevel > 0)
                    {
                        sb.AppendLine("Extractor: " + unit.MoveUpdateStats.ExtractorLevel);
                    }
                    if (unit.MoveUpdateStats.ProductionLevel > 0)
                    {
                        sb.Append("Production: " + unit.MoveUpdateStats.ProductionLevel);
                        if (unit.MoveUpdateStats.CanProduce)
                            sb.AppendLine(" CanProduce");
                        else
                            sb.AppendLine("");
                    }
                    if (unit.MoveUpdateStats.RadarLevel > 0)
                    {
                        sb.AppendLine("Radar: " + unit.MoveUpdateStats.RadarLevel);
                    }
                    if (unit.MoveUpdateStats.ReactorLevel > 0)
                    {
                        sb.AppendLine("Reactor: " + unit.MoveUpdateStats.ReactorLevel);
                    }
                    if (unit.MoveUpdateStats.WeaponLevel > 0)
                    {
                        sb.Append("Weapon: " + unit.MoveUpdateStats.WeaponLevel);
                        if (unit.MoveUpdateStats.WeaponLoaded)
                            sb.AppendLine(" Loaded");
                        else
                            sb.AppendLine("");
                    }
                }
            }
            sb.AppendLine("");

            GroundCell gc = HexGrid.GroundCells[unit.currentPos];
            AppendGroundInfo(gc, sb);

            UISelectedObjectText.text = sb.ToString();
            UISelectedObjectsText.text = "Object";
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
            UISelectedObjectsText.text = "NONE";
        }

    }
}
