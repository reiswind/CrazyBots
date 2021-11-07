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
            GameCommand.TargetPosition = Position2.Null;
            GameCommand.DeleteWhenFinished = false;
        }
        
        public MapGameCommand GameCommand { get; set; }
        public Command Command { get; set; }
        public bool Touched { get; set; }
        internal bool IsPreview { get; set; }

        private GameObject previewGameCommand;

        public List<CommandAttachedUnit> PreviewUnits { get; set; }

        public void CreateCommandForBuild(MapBlueprintCommand blueprintCommand)
        {
            GameCommand.BlueprintCommand = blueprintCommand;
            GameCommand.GameCommandType = blueprintCommand.GameCommandType;
            
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

        internal void SetHighlighted(bool isHighlighted)
        {
            Command.SetHighlighted(isHighlighted);

            foreach (MapGameCommandItem mapGameCommandItem in GameCommand.GameCommandItems)
            {
                if (!string.IsNullOrEmpty(mapGameCommandItem.AttachedUnitId))
                {
                    UnitBase unitBase;
                    if (HexGrid.MainGrid.BaseUnits.TryGetValue(mapGameCommandItem.AttachedUnitId, out unitBase))
                    {
                        unitBase.SetHighlighted(isHighlighted);
                    }
                }
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

        private UnitBase addPreviewUnit;
        public void AddUnitCommand(string bluePrint)
        {
            Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(bluePrint);
            addPreviewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint);
            addPreviewUnit.DectivateUnit();
            addPreviewUnit.transform.SetParent(previewGameCommand.transform, false);

            GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
            previewUnitMarker.transform.SetParent(addPreviewUnit.transform, false);

            Vector3 unitPos3 = addPreviewUnit.transform.position;
            unitPos3.y -= 0.1f;
            previewUnitMarker.transform.position = unitPos3;
        }
        public void CancelSubCommand()
        {
            if (isMoveMode)
            {
                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(GameCommand.TargetPosition, out gc))
                {
                    SetPosition(gc);
                }
                isMoveMode = false;
            }
            if (addPreviewUnit != null)
            {
                addPreviewUnit.Delete();
                addPreviewUnit = null;
            }
        }

        public bool IsInSubCommandMode
        {
            get
            {
                return isMoveMode || addPreviewUnit != null;
            }
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
        public bool CanExecute()
        {
            if (addPreviewUnit == null)
            {
                return displayPosition != Position2.Null;
            }
            else
            {
                return displayPosition != Position2.Null;
            }
        }
        public void Execute()
        {
            if (displayPosition != Position2.Null)
            {
                if (addPreviewUnit != null)
                {
                    // Add unit
                    MapGameCommand gameCommand = new MapGameCommand();

                    gameCommand.TargetPosition = GameCommand.TargetPosition;
                    gameCommand.GameCommandType = GameCommandType.AddUnits;
                    gameCommand.BlueprintCommand = GameCommand.BlueprintCommand;
                    gameCommand.PlayerId = 1;

                    Position3 cubePosition = new Position3(displayPosition);
                    Position3 commandCenter = new Position3(GameCommand.TargetPosition);
                    Position3 relative = commandCenter.Subtract(cubePosition);

                    MapBlueprintCommandItem blueprintCommandItem = new MapBlueprintCommandItem();
                    blueprintCommandItem.BlueprintName = "Fighter";
                    blueprintCommandItem.Direction = Direction.N; //??
                    blueprintCommandItem.CubePosition = relative;
                    gameCommand.BlueprintCommand.Units.Add(blueprintCommandItem);

                    addPreviewUnit.Delete();
                    addPreviewUnit = null;

                    //HexGrid.MainGrid.GameCommands.Add(gameCommand);
                    GroundCell gc;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(GameCommand.TargetPosition, out gc))
                    {
                        CreateCommandPreview(gc);
                    }
                    
                }
                else if (isMoveMode)
                {
                    MapGameCommand gameCommand = new MapGameCommand();

                    gameCommand.TargetPosition = GameCommand.TargetPosition;
                    gameCommand.GameCommandType = GameCommandType.Move;
                    gameCommand.BlueprintCommand = GameCommand.BlueprintCommand;
                    gameCommand.PlayerId = 1;
                    gameCommand.MoveToPosition = displayPosition;

                    HexGrid.MainGrid.GameCommands.Add(gameCommand);
                    // Preview remains, the real gamecomand should be at the same position
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
        /*
        public void CreateAtPosition(GroundCell groundCell)
        {
            if (previewGameCommand == null)
                CreateCommandLogo();
            UpdatePositions(groundCell);
        }*/

        private Position2 displayPosition;

        public void SetPosition(GroundCell groundCell)
        {
            if (previewGameCommand == null)
            {
                CreateCommandPreview(groundCell);
            }
            if (addPreviewUnit != null)
            {
                if(groundCell == null)
                {
                    addPreviewUnit.gameObject.SetActive(false);
                    displayPosition = Position2.Null;
                }
                else
                {
                    bool positionok;
                    positionok = CanCommandAt(groundCell);
                    if (positionok)
                    {
                        addPreviewUnit.transform.SetParent(groundCell.transform, false);
                        addPreviewUnit.gameObject.SetActive(true);
                        displayPosition = groundCell.Pos;
                    }
                    else
                    {
                        addPreviewUnit.gameObject.SetActive(false);
                        displayPosition = Position2.Null;
                    }
                }
            }
            else
            {
                if (groundCell == null)
                {
                    if (previewGameCommand != null)
                        previewGameCommand.SetActive(false);
                    displayPosition = Position2.Null;
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
                        displayPosition = Position2.Null;
                    }
                }
            }
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

            if (previewGameCommand != null)
            {
                HexGrid.Destroy(previewGameCommand);
            }
            previewGameCommand = HexGrid.Instantiate(HexGrid.MainGrid.GetResource(layout));

            Command = previewGameCommand.GetComponent<Command>();
            Command.CommandPreview = this;
        }
        public bool IsSelected
        {
            get
            {
                if (Command == null) return false;
                return Command.IsSelected;
            }
        }
        public void SetSelected(bool value)
        {
            if (Command != null)
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

        public void CreateCommandPreview(GroundCell groundCell)
        {
            CreateCommandLogo();
            IsPreview = true;
            UpdatePositions(groundCell);

            foreach (MapBlueprintCommandItem blueprintCommandItem in GameCommand.BlueprintCommand.Units)
            {
                break;

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
                newDirection.x = 0;
                previewUnit.transform.rotation = Quaternion.LookRotation(newDirection);
                
                Vector3 unitPos3 = previewGameCommand.transform.position;
                if (previewUnit.HasEngine())
                    unitPos3.y -= aboveGround - 0.1f; // Unit Above Ground if active
                else
                    unitPos3.y -= aboveGround;

                Position3 groundCubePos = new Position3(groundCell.Pos);
                Position3 unitCubePos;
                //if (blueprintCommandItem.CubePosition == null)
                //    unitCubePos = groundCubePos;
                //else
                    unitCubePos = groundCubePos.Add(blueprintCommandItem.CubePosition);

                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(unitCubePos.Pos, out gc))
                {
                    //unitPos3.x += groundCell.transform.position.x - gc.transform.position.x;
                    //unitPos3.z += groundCell.transform.position.z - gc.transform.position.z;
                }

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
