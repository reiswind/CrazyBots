using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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

    // Start is called before the first frame update
    void Start()
    {
        UIMineralText = MineralText.GetComponent<Text>();
        UISelectedObjectText = SelectedObjectText.GetComponent<Text>();
        UISelectedObjectsText = SelectedObjectsText.GetComponent<Text>();

        HexGrid.StartGame();
    }

    private UnitBase selectedUnitFrame;
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

                gameCommand.UnitId = "Fighter";
                gameCommand.TargetPosition = hitByMouseClick.GroundCell.Tile.Pos;
                gameCommand.GameCommandType = GameCommandType.Build;
                gameCommand.PlayerId = 1;
                HexGrid.GameCommands.Add(gameCommand);

                HexGrid.CreateGhost(gameCommand.UnitId, hitByMouseClick.GroundCell.Tile.Pos);
            }

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
