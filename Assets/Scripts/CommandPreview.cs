using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class CommandAttachedUnit
    {
        public UnitBase UnitBase { get; set; }
        public GameObject Line { get; set; }

    }

    public class CommandPreview
    {
        public CommandPreview()
        {
            PreviewUnits = new List<CommandAttachedUnit>();

            GameCommand = new MapGameCommand();
            GameCommand.PlayerId = 1;
            GameCommand.TargetPosition = Position.Null;
            GameCommand.DeleteWhenFinished = false;
        }
        
        public MapGameCommand GameCommand { get; set; }
        public Command Command { get; set; }
        public bool Touched { get; set; }
        internal bool IsPreview { get; set; }

        private GameObject previewGameCommand;

        public List<CommandAttachedUnit> PreviewUnits { get; set; }

        public void CreateCommandForBuild(BlueprintCommand blueprintCommand)
        {
            GameCommand.BlueprintCommand = blueprintCommand;
            GameCommand.GameCommandType = blueprintCommand.GameCommandType;
            CreateCommandPreview();
        }

        public void Delete()
        {
            if (previewGameCommand != null)
            {                
                //Command?.SetSelected(false);

                HexGrid.Destroy(previewGameCommand);
                previewGameCommand = null;
            }
        }

        private bool CanBuildAt(GroundCell groundCell)
        {
            if (groundCell != null)
            {
                UnitBase unitBase = groundCell.FindUnit();
                if (unitBase != null)
                {
                    if (!unitBase.HasEngine())
                        return false;
                }

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

        private bool isMoveMode;
        public void SelectMoveMode()
        {
            isMoveMode = true;
        }

        public void CancelCommand()
        {
            MapGameCommand gameCommand = new MapGameCommand();

            gameCommand.TargetPosition = GameCommand.TargetPosition;
            gameCommand.GameCommandType = GameCommandType.Cancel;
            gameCommand.BlueprintCommand = GameCommand.BlueprintCommand;
            gameCommand.PlayerId = 1;
            gameCommand.MoveToPosition = displayPosition;

            HexGrid.MainGrid.GameCommands.Add(gameCommand);
        }

        public void Execute()
        {
            if (displayPosition != Position.Null)
            {
                if (isMoveMode)
                {
                    MapGameCommand gameCommand = new MapGameCommand();

                    gameCommand.TargetPosition = GameCommand.TargetPosition;
                    gameCommand.GameCommandType = GameCommandType.Move;
                    gameCommand.BlueprintCommand = GameCommand.BlueprintCommand;
                    gameCommand.PlayerId = 1;
                    gameCommand.MoveToPosition = displayPosition;

                    HexGrid.MainGrid.GameCommands.Add(gameCommand);
                }
                else
                {
                    GameCommand.GameCommandType = GameCommand.BlueprintCommand.GameCommandType;
                    GameCommand.TargetPosition = displayPosition;
                    IsPreview = false;

                    HexGrid.MainGrid.GameCommands.Add(GameCommand);
                    HexGrid.MainGrid.CommandPreviews.Add(this);

                    GroundCell gc;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(displayPosition, out gc))
                    {
                        gc.UpdateCommands(GameCommand, this);
                    }
                }
            }
        }

        private bool CanCommandAt(GroundCell groundCell)
        {
            if (groundCell != null)
            {
                foreach (TileObject tileObject in groundCell.Stats.MoveUpdateGroundStat.TileObjects)
                {
                    if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                        return false;
                }
            }
            return true;
        }
        public void CreateAtPosition(GroundCell groundCell)
        {
            if (previewGameCommand == null)
                CreateCommandLogo();
            UpdatePositions(groundCell);
        }

        private ulong displayPosition;

        public void SetPosition(GroundCell groundCell)
        {
            if (groundCell == null)
            {
                if (previewGameCommand != null)
                    previewGameCommand.SetActive(false);
                displayPosition = Position.Null;
            }
            else
            {
                bool positionok;
                if (GameCommand.GameCommandType == GameCommandType.Build)
                    positionok = CanBuildAt(groundCell);
                else
                    positionok = CanCommandAt(groundCell);
                if (positionok)
                {
                    displayPosition = groundCell.Pos;
                    UpdatePositions(groundCell);
                    if (previewGameCommand != null)
                        previewGameCommand.SetActive(true);
                }
                else
                {
                    if (previewGameCommand != null)
                        previewGameCommand.SetActive(false);
                    displayPosition = Position.Null;
                }
            }
        }

        public bool CanExecute()
        {
            return displayPosition != Position.Null;
        }

        private static float aboveGround = 2.09f;

        private void UpdatePositions(GroundCell groundCell)
        {
            if (previewGameCommand != null)
            {
                previewGameCommand.transform.SetParent(groundCell.transform, false);

                Vector3 unitPos3 = groundCell.transform.position;
                unitPos3.y += aboveGround;
                previewGameCommand.transform.position = unitPos3;
            }
        }

        private void CreateCommandLogo()
        {
            string layout = "UIBuild";

            if (GameCommand.BlueprintCommand != null &&
                !string.IsNullOrEmpty(GameCommand.BlueprintCommand.Layout))
                layout = GameCommand.BlueprintCommand.Layout;

            previewGameCommand = HexGrid.Instantiate(HexGrid.MainGrid.GetResource(layout));

            Command = previewGameCommand.GetComponent<Command>();
            Command.CommandPreview = this;
        }
        public bool IsSelected { get { return Command.IsSelected; } }
        public void SetSelected(bool value)
        {
            Command.SetSelected(value);
        }
        public void SetActive(bool value)
        {
            if (previewGameCommand != null)
            {
                for (int i = 0; i < previewGameCommand.transform.childCount; i++)
                {
                    GameObject child = previewGameCommand.transform.GetChild(i).gameObject;
                    child.SetActive(value);
                }
            }
        }

        public bool ContainsUnit(UnitBase unitBase)
        {
            foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
            {
                if (commandAttachedUnit.UnitBase == unitBase)
                    return true;
            }
            return false;
        }

        public void CreateCommandPreview()
        {
            CreateCommandLogo();
            IsPreview = true;

            foreach (BlueprintCommandItem blueprintCommandItem in GameCommand.BlueprintCommand.Units)
            {

                Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(blueprintCommandItem.BlueprintName);
                UnitBase previewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint);
                previewUnit.DectivateUnit();
                previewUnit.transform.SetParent(previewGameCommand.transform, false);

                GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
                previewUnitMarker.transform.SetParent(previewUnit.transform, false);

                /*
                Direction dir = Direction.NW;
                
                */
                Vector3 newDirection = new Vector3(); // = Vector3.RotateTowards(previewUnit.transform.position, n.transform.position, 360, 0.0f);
                newDirection.x = -30;
                previewUnit.transform.rotation = Quaternion.LookRotation(newDirection);
                
                Vector3 unitPos3 = previewGameCommand.transform.position;
                if (previewUnit.HasEngine())
                    unitPos3.y -= aboveGround - 0.1f; // Unit Above Ground if active
                else
                    unitPos3.y -= aboveGround;

                previewUnit.transform.position = unitPos3;

                unitPos3.y -= 0.01f;
                previewUnitMarker.transform.position = unitPos3;

                Vector3 scaleChange;
                scaleChange = new Vector3(0.01f, 0.01f, 0.01f);

                previewUnit.gameObject.transform.localScale += scaleChange;

                previewUnit.gameObject.SetActive(true);

                CommandAttachedUnit commandAttachedUnit = new CommandAttachedUnit();
                commandAttachedUnit.UnitBase = previewUnit;

                PreviewUnits.Add(commandAttachedUnit);
            }
        }
        public override string ToString()
        {
            if (GameCommand != null)
                return GameCommand.ToString();

            return base.ToString();
        }
    }
}
