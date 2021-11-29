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
        public CommandAttachedUnit(MapGameCommandItem mapGameCommandItem)
        {
            MapGameCommandItem = mapGameCommandItem;
            Direction = Direction.C;
        }
        public MapGameCommandItem MapGameCommandItem { get; set; }
        public bool IsVisible { get; set; }
        public Position3 Position3 { get; set; }
        public Position3 RotatedPosition3 { get; set; }

        public Direction Direction { get; set; }
        public Direction RotatedDirection { get; set; }
        public UnitBase GhostUnit { get; set; }
        //public GameObject Marker { get; set; }
        public UnitBounds GhostUnitBounds { get; set; }
        public GameObject Line { get; set; }

        public void Delete()
        {
            if (GhostUnit != null)
            {
                GhostUnit.Delete();
                GhostUnit = null;
            }
            if (GhostUnitBounds != null)
            {
                GhostUnitBounds.Destroy();
                GhostUnitBounds = null;
            }
            if (Line != null)
            {
                HexGrid.Destroy(Line);
                Line = null;
            }
        }
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
            displayDirection = Direction.N;
        }
        
        public MapGameCommand GameCommand { get; set; }
        public Command Command { get; set; }
        public bool Touched { get; set; }
        internal bool IsPreview { get; set; }
        public BlueprintCommand Blueprint { get; private set; }

        private GameObject previewGameCommand;
        private GameObject alertGameObject;

        public List<CommandAttachedUnit> PreviewUnits { get; set; }

        public void CreateCommandForBuild(BlueprintCommand blueprint)
        {
            Blueprint = blueprint;
            GameCommand.Layout = blueprint.Layout;
            GameCommand.GameCommandType = blueprint.GameCommandType;
            if (GameCommand.GameCommandType == GameCommandType.Collect)
                GameCommand.Radius = 3;
        
            foreach (BlueprintCommandItem blueprintCommandItem in blueprint.Units)
            {
                MapGameCommandItem mapGameCommandItem = new MapGameCommandItem(GameCommand, blueprintCommandItem);
                mapGameCommandItem.Direction = displayDirection;
                GameCommand.GameCommandItems.Add(mapGameCommandItem);
            }
            CreateCommandLogo();
            IsPreview = true;
            UpdateCommandPreview(GameCommand);
        }

        public void Delete()
        {
            if (previewGameCommand != null)
            {                
                //Command?.SetSelected(false);

                HexGrid.Destroy(previewGameCommand);
                previewGameCommand = null;
            }
            foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
            {
                if (commandAttachedUnit.GhostUnit != null)
                {
                    commandAttachedUnit.GhostUnit.Delete();
                    commandAttachedUnit.GhostUnit = null;
                }
                if (commandAttachedUnit.GhostUnitBounds != null)
                {
                    commandAttachedUnit.GhostUnitBounds.Destroy();
                    commandAttachedUnit.GhostUnitBounds = null;
                }
            }
        }

        internal void SetHighlighted(bool isHighlighted)
        {
            Command.SetHighlighted(isHighlighted);
            foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
            {
                commandAttachedUnit.GhostUnit.SetHighlighted(isHighlighted);
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
        public bool IsMoveMode
        {
            get
            {
                return isMoveMode;
            }
        }
        public void SelectMoveMode()
        {
            isMoveMode = true;

            // Append build grid
            UpdateAllUnitBounds(true);
        }

        private UnitBase addPreviewGhost;
        private GameObject addPreviewUnitMarker;
        public void AddUnitCommand(string bluePrint)
        {

            Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(bluePrint);
            addPreviewGhost = HexGrid.MainGrid.CreateTempUnit(blueprint);
            addPreviewGhost.DectivateUnit();
            addPreviewGhost.Direction = GameCommand.Direction;
            addPreviewGhost.TurnIntoDirection = GameCommand.Direction;
            addPreviewGhost.transform.SetParent(HexGrid.MainGrid.transform, false);
            addPreviewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
            addPreviewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);
            /*
            Vector3 unitPos3 = addPreviewGhost.transform.position;
            unitPos3.y += 0.1f;
            addPreviewUnitMarker.transform.position = unitPos3;*/

            /*
            Position3 relativePosition3 = gam.Add(commandAttachedUnit.RotatedPosition3);
            Position3 neighborPosition3 = relativePosition3.GetNeighbor(displayDirection);
            GroundCell neighbor;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(neighborPosition3.Pos, out neighbor))
            {
                addPreviewUnit.UpdateDirection(neighbor.transform.position);
            }
            */
        }
        public void CancelSubCommand()
        {
            if (isMoveMode)
            {
                foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
                {
                    commandAttachedUnit.IsVisible = false;
                    commandAttachedUnit.RotatedPosition3 = commandAttachedUnit.Position3;
                    commandAttachedUnit.RotatedDirection = commandAttachedUnit.Direction;

                    commandAttachedUnit.GhostUnit.SetHighlighted(false);
                    commandAttachedUnit.GhostUnit.IsVisible = false;
                }

                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(GameCommand.TargetPosition, out gc))
                {
                    SetPosition(gc);
                }
                isMoveMode = false;
                UpdateAllUnitBounds(true);
            }
            if (addPreviewGhost != null)
            {
                addPreviewGhost.Delete();
                addPreviewGhost = null;
            }
            if (addPreviewUnitMarker != null)
            {
                HexGrid.Destroy(addPreviewUnitMarker);
                addPreviewUnitMarker = null;
            }
        }

        public bool IsInSubCommandMode
        {
            get
            {
                return isMoveMode || addPreviewGhost != null;
            }
        }

        public void CancelCommand()
        {
            MapGameCommand gameCommand = new MapGameCommand();

            gameCommand.TargetPosition = GameCommand.TargetPosition;
            gameCommand.GameCommandType = GameCommandType.Cancel;
            gameCommand.PlayerId = 1;

            HexGrid.MainGrid.GameCommands.Add(gameCommand);
        }
        public bool CanExecute()
        {
            if (addPreviewGhost == null)
            {
                return displayPosition != Position2.Null;
            }
            else
            {
                return displayPosition != Position2.Null;
            }
        }

        private int moveCounter;
        public int ValidAfterThisMoveNr
        {
            get
            {
                return moveCounter;
            }
        }

        public void AddUnit(GroundCell groundCell)
        {
            if (!CanBuildAt(groundCell))
            {
                return;
            }
            if (displayPosition == Position2.Null)
            {
                return;
            }
            Position3 displayPosition3 = new Position3(groundCell.Pos);
            Position3 commandCenter = new Position3(displayPosition);
            Position3 relativePosition3 = displayPosition3.Subtract(commandCenter);


            //Position3 displayPosition3 = new Position3(displayPosition);
            //Position3 unitPos = new Position3(groundCell.Pos);
            //Position3 relativePosition3 = displayPosition3.Subtract(unitPos);
            //Position3 relativePosition3 = unitPos.Subtract(displayPosition3);

            bool exists = false;
            foreach (MapGameCommandItem mapGameCommandItem in GameCommand.GameCommandItems)
            {
                if (mapGameCommandItem.Position3 == relativePosition3)
                {
                    exists = true;
                }
            }
            if (!exists)
            {
                Debug.Log("Add unit at " + groundCell.Pos + " DP: " + displayPosition);

                MapGameCommandItem primary = GameCommand.GameCommandItems[0];
                MapGameCommandItem mapGameCommandItem = new MapGameCommandItem(GameCommand);
                mapGameCommandItem.Position3 = relativePosition3;
                mapGameCommandItem.RotatedPosition3 = mapGameCommandItem.Position3;

                mapGameCommandItem.BlueprintName = primary.BlueprintName;
                mapGameCommandItem.Direction = primary.Direction;

                GameCommand.GameCommandItems.Add(mapGameCommandItem);

                UpdateCommandPreview(GameCommand);

                GroundCell displayCell = HexGrid.MainGrid.GroundCells[displayPosition];
                UpdatePositions(displayCell);
            }
        }

        public void Execute()
        {
            if (displayPosition != Position2.Null)
            {
                if (addPreviewGhost != null)
                {
                    // Add unit
                    MapGameCommand mapGameCommand = new MapGameCommand();

                    mapGameCommand.TargetPosition = GameCommand.TargetPosition;
                    mapGameCommand.GameCommandType = GameCommandType.AddUnits;
                    mapGameCommand.PlayerId = 1;

                    Position3 displayPosition3 = new Position3(displayPosition);
                    Position3 commandCenter = new Position3(GameCommand.TargetPosition);
                    Position3 relativePosition3 = displayPosition3.Subtract(commandCenter);

                    MapGameCommandItem mapGameCommandItem = new MapGameCommandItem(mapGameCommand);
                    mapGameCommandItem.BlueprintName = "Fighter";
                    mapGameCommandItem.Direction = GameCommand.Direction;
                    mapGameCommandItem.Position3 = relativePosition3;
                    mapGameCommand.GameCommandItems.Add(mapGameCommandItem);

                    HexGrid.MainGrid.GameCommands.Add(mapGameCommand);
                    moveCounter = HexGrid.MainGrid.MoveCounter + 1;

                    /* Command is updated next turn if return here else: */
                    mapGameCommandItem = new MapGameCommandItem(GameCommand);
                    mapGameCommandItem.BlueprintName = "Fighter";
                    mapGameCommandItem.Direction = addPreviewGhost.TurnIntoDirection;
                    mapGameCommandItem.RotatedDirection = addPreviewGhost.TurnIntoDirection;
                    mapGameCommandItem.Position3 = relativePosition3;
                    GameCommand.GameCommandItems.Add(mapGameCommandItem);

                    CommandAttachedUnit commandAttachedUnit = new CommandAttachedUnit(mapGameCommandItem);
                    commandAttachedUnit.Position3 = relativePosition3;
                    commandAttachedUnit.RotatedPosition3 = relativePosition3;
                    commandAttachedUnit.Direction = addPreviewGhost.TurnIntoDirection;
                    commandAttachedUnit.RotatedDirection = addPreviewGhost.TurnIntoDirection;
                    commandAttachedUnit.GhostUnit = addPreviewGhost;
                    commandAttachedUnit.GhostUnitBounds = new UnitBounds(addPreviewGhost);
                    commandAttachedUnit.GhostUnitBounds.AddBuildGrid();
                    commandAttachedUnit.IsVisible = true;

                    PreviewUnits.Add(commandAttachedUnit);
                    addPreviewGhost = null;
                    addPreviewUnitMarker = null;

                    /*
                    GroundCell gc;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(GameCommand.TargetPosition, out gc))
                    {
                        CreateCommandPreview(gameCommand);
                    }*/

                }
                else if (isMoveMode)
                {
                    MapGameCommand gameCommand = new MapGameCommand();

                    gameCommand.TargetPosition = GameCommand.TargetPosition;
                    gameCommand.GameCommandType = GameCommandType.Move;
                    gameCommand.Radius = GameCommand.Radius;
                    gameCommand.PlayerId = 1;
                    gameCommand.MoveToPosition = displayPosition;
                    gameCommand.Direction = displayDirection;

                    foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
                    {
                        MapGameCommandItem gameCommandItem = new MapGameCommandItem(gameCommand);

                        gameCommandItem.Position3 = commandAttachedUnit.Position3;
                        gameCommandItem.Direction = commandAttachedUnit.Direction;
                        gameCommandItem.BlueprintName = commandAttachedUnit.MapGameCommandItem.BlueprintName;
                        gameCommandItem.RotatedPosition3 = commandAttachedUnit.RotatedPosition3;
                        gameCommandItem.RotatedDirection = commandAttachedUnit.RotatedDirection;

                        gameCommand.GameCommandItems.Add(gameCommandItem);
                    }

                    HexGrid.MainGrid.GameCommands.Add(gameCommand);
                    moveCounter = HexGrid.MainGrid.MoveCounter + 1;

                    // Preview remains, the real gamecommand should be at the same position
                    isMoveMode = false;
                }
                else
                {
                    foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
                    {
                        if (commandAttachedUnit.GhostUnitBounds != null)
                        {
                            commandAttachedUnit.GhostUnitBounds.Destroy();
                            commandAttachedUnit.GhostUnitBounds = null;
                        }
                        foreach (MapGameCommandItem gameCommandItem in GameCommand.GameCommandItems)
                        {
                            if (commandAttachedUnit.Position3 == gameCommandItem.Position3)
                            {
                                gameCommandItem.Direction = commandAttachedUnit.RotatedDirection;
                                gameCommandItem.RotatedDirection = commandAttachedUnit.RotatedDirection;
                            }
                        }
                    }

                    GameCommand.GameCommandType = GameCommand.GameCommandType;
                    GameCommand.TargetPosition = displayPosition;
                    GameCommand.Direction = displayDirection;
                    IsPreview = false;

                    // Remove the command after the structure is complete
                    if (GameCommand.GameCommandType == GameCommandType.Build)
                        GameCommand.DeleteWhenFinished = true;

                    HexGrid.MainGrid.GameCommands.Add(GameCommand);
                    HexGrid.MainGrid.CommandPreviews.Add(this);
                    moveCounter = HexGrid.MainGrid.MoveCounter + 1;

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
            if (groundCell == null)
                return false;

            if (groundCell.Stats.MoveUpdateGroundStat.IsUnderwater)
                return false;
            if (groundCell.FindUnit() != null)
                return false;

            foreach (TileObject tileObject in groundCell.Stats.MoveUpdateGroundStat.TileObjects)
            {
                if (TileObject.IsTileObjectTypeObstacle(tileObject.TileObjectType))
                    return false;
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
        private Direction displayDirection = Direction.C;
        public Position2 DisplayPosition
        {
            get
            {
                return displayPosition;
            }
        }

        public void SetPosition(GroundCell groundCell)
        {
            if (previewGameCommand == null)
            {
                CreateCommandPreview(GameCommand);
                displayDirection = GameCommand.Direction;
            }
            if (addPreviewGhost != null)
            {
                if (groundCell == null)
                {
                    addPreviewGhost.IsVisible = false;
                    addPreviewUnitMarker.SetActive(false);
                    displayPosition = Position2.Null;
                }
                else
                {
                    bool positionok;
                    positionok = CanBuildAt(groundCell);
                    if (positionok)
                    {
                        Debug.Log("addPreviewGhost: " + groundCell.Pos);
                        addPreviewGhost.IsVisible = true;
                        addPreviewGhost.CurrentPos = groundCell.Pos;
                        addPreviewGhost.PutAtCurrentPosition(true, true);

                        Vector3 unitPos3 = addPreviewGhost.transform.position;
                        unitPos3.y += 0.10f;
                        addPreviewUnitMarker.transform.position = unitPos3;
                        addPreviewUnitMarker.SetActive(true);

                        displayPosition = groundCell.Pos;
                    }
                    else
                    {
                        Debug.Log("addPreviewGhost NO GROUND");
                        addPreviewGhost.IsVisible = false;
                        addPreviewUnitMarker.SetActive(false);
                        displayPosition = Position2.Null;
                    }
                }
            }
            else
            {
                if (groundCell == null)
                {
                    if (displayPosition != Position2.Null)
                    {
                        Hide();
                    }
                }
                else
                {
                    if (displayPosition == Position2.Null || displayPosition != groundCell.Pos)
                    {
                        bool positionok;

                        if (GameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            positionok = true;
                        }
                        else
                        {
                            positionok = CanBuildAt(groundCell);
                        }
                        if (positionok)
                        {
                            Debug.Log("SetPosition: " + groundCell.Pos);

                            if (previewGameCommand != null)
                                previewGameCommand.SetActive(true);

                            displayPosition = groundCell.Pos;
                            UpdatePositions(groundCell);
                        }
                        else
                        {
                            Hide();
                        }
                    }
                }
            }
        }

        public void RotateCommand()
        {
            if (addPreviewGhost == null)
            {
                displayDirection = Tile.TurnRight(displayDirection);

                foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
                {
                    Position3 position3;
                    position3 = commandAttachedUnit.RotatedPosition3.RotateRight();
                    commandAttachedUnit.RotatedPosition3 = position3;
                    commandAttachedUnit.RotatedDirection = displayDirection;
                }
                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(DisplayPosition, out gc))
                {
                    UpdatePositions(gc);
                }
            }
            else
            {
                // Turn only preview
                addPreviewGhost.TurnIntoDirection = Tile.TurnRight(addPreviewGhost.TurnIntoDirection);
                
            }
        }
        private void Hide()
        {
            Debug.Log("SetPosition NO GROUND null");
            if (previewGameCommand != null)
                previewGameCommand.SetActive(false);
            displayPosition = Position2.Null;
            foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
            {
                commandAttachedUnit.IsVisible = false;
                commandAttachedUnit.GhostUnit.IsVisible = false;
                commandAttachedUnit.GhostUnitBounds.IsVisible = false;
            }
        }

        private void UpdatePositions(GroundCell groundCell)
        {
            if (previewGameCommand != null)
            {
                Vector3 unitPos3 = groundCell.transform.position;
                if (GameCommand.GameCommandType == GameCommandType.Build)
                    unitPos3.y += 1.5f;
                else if (GameCommand.GameCommandType == GameCommandType.Attack ||
                         GameCommand.GameCommandType == GameCommandType.Collect)
                    unitPos3.y += 1.0f;
                else if (GameCommand.GameCommandType == GameCommandType.ItemRequest)
                    unitPos3.y += 2.0f;
                else
                    unitPos3.y += 2.0f;

                previewGameCommand.transform.position = unitPos3;

                Position3 centerPosition3 = new Position3(displayPosition);

                Position3 neighborPosition3;
                if (displayDirection != Direction.C)
                {
                    neighborPosition3 = centerPosition3.GetNeighbor(displayDirection);
                    GroundCell neighbor;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(neighborPosition3.Pos, out neighbor))
                        Command.UpdateDirection(neighbor.transform.position);
                }
                foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
                {
                    Position3 relativePosition3 = centerPosition3.Add(commandAttachedUnit.RotatedPosition3);
                    
                    commandAttachedUnit.GhostUnit.CurrentPos = relativePosition3.Pos;
                    commandAttachedUnit.GhostUnit.TeleportToPosition(true);
                    commandAttachedUnit.IsVisible = true;

                    commandAttachedUnit.GhostUnit.IsVisible = true;
                    unitPos3 = commandAttachedUnit.GhostUnit.transform.position;
                    unitPos3.y += 0.10f;

                    commandAttachedUnit.GhostUnitBounds.IsVisible = true;
                    commandAttachedUnit.GhostUnitBounds.Update();

                    if (displayDirection != Direction.C)
                    {
                        neighborPosition3 = relativePosition3.GetNeighbor(displayDirection);
                        GroundCell neighbor;
                        if (HexGrid.MainGrid.GroundCells.TryGetValue(neighborPosition3.Pos, out neighbor))
                        {
                            commandAttachedUnit.GhostUnit.UpdateDirection(neighbor.transform.position, true);
                        }
                    }
                }
            }
        }

        public void ShowAlert(bool showAlert)
        {
            if (alertGameObject != null)
            {
                alertGameObject.SetActive(showAlert);
            }
        }


        private void CreateCommandLogo()
        {
            if (previewGameCommand != null)
            {
                HexGrid.Destroy(previewGameCommand);
            }
            previewGameCommand = HexGrid.Instantiate(HexGrid.MainGrid.GetResource(GameCommand.Layout));
            previewGameCommand.transform.SetParent(HexGrid.MainGrid.transform, false);

            Transform alert = previewGameCommand.transform.Find("Alert");
            if (alert != null)
            {
                alertGameObject = alert.gameObject;
                alertGameObject.SetActive(false);
            }
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

        private void UpdateAllUnitBounds(bool visible)
        {
            foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
            {
                if (visible == true)
                {
                    if (commandAttachedUnit.GhostUnitBounds != null)
                    {
                        commandAttachedUnit.GhostUnitBounds.Destroy();
                        commandAttachedUnit.GhostUnitBounds = null;
                    }
                    if (commandAttachedUnit.GhostUnit != null)
                    {
                        // On select
                        commandAttachedUnit.GhostUnitBounds = new UnitBounds(commandAttachedUnit.GhostUnit);

                        if (IsPreview || IsMoveMode)
                        {
                            if (GameCommand.GameCommandType == GameCommandType.Collect)
                            {
                                commandAttachedUnit.GhostUnitBounds.AddCollectRange(GameCommand.Radius);
                            }
                            else
                            {
                                commandAttachedUnit.GhostUnitBounds.AddBuildGrid();
                            }
                        }
                        commandAttachedUnit.GhostUnitBounds.Update();
                    }
                }
                else
                {
                    if (commandAttachedUnit.GhostUnitBounds != null)
                    {
                        commandAttachedUnit.GhostUnitBounds.Destroy();
                        commandAttachedUnit.GhostUnitBounds = null;
                    }
                }
            }
        }

        public void SetSelected(bool value)
        {
            if (Command != null)
                Command.SetSelected(value);

            UpdateAllUnitBounds(value);
        }
        
        public void SetActive(bool value)
        {
            /*
            if (previewGameCommand != null)
            {
                for (int i = 0; i < previewGameCommand.transform.childCount; i++)
                {
                    GameObject child = previewGameCommand.transform.GetChild(i).gameObject;
                    child.SetActive(value);
                }
            }*/
        }

        public bool ContainsUnit(UnitBase unitBase)
        {
            foreach (CommandAttachedUnit commandAttachedUnit in PreviewUnits)
            {
                if (commandAttachedUnit.GhostUnit == unitBase)
                    return true;
            }
            return false;
        }

        public bool UpdateCommandPreview(MapGameCommand gameCommand)
        {

            bool updatePosition = false;
            GameCommand = gameCommand;

            if (GameCommand.GameCommandType == GameCommandType.ItemRequest)
            {
                // Do not show the worker to build
            }
            else
            {
                List<CommandAttachedUnit> remainingPreviews = new List<CommandAttachedUnit>();
                remainingPreviews.AddRange(PreviewUnits);

                foreach (MapGameCommandItem mapGameCommandItem in GameCommand.GameCommandItems)
                {
                    CommandAttachedUnit commandAttachedUnit = null;
                    foreach (CommandAttachedUnit searchAttachedUnit in PreviewUnits)
                    {
                        if (searchAttachedUnit.Position3 == mapGameCommandItem.Position3)
                        {
                            commandAttachedUnit = searchAttachedUnit;
                            break;
                        }
                    }
                    if (commandAttachedUnit == null)
                    {
                        if (mapGameCommandItem.BlueprintName != null)
                        {
                            Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(mapGameCommandItem.BlueprintName);
                            UnitBase previewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint);
                            previewUnit.Direction = GameCommand.Direction;
                            previewUnit.DectivateUnit();
                            previewUnit.transform.SetParent(HexGrid.MainGrid.transform, false);

                            GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
                            previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);

                            // On create select                            
                            commandAttachedUnit = new CommandAttachedUnit(mapGameCommandItem);
                            commandAttachedUnit.GhostUnit = previewUnit;
                            commandAttachedUnit.Direction = displayDirection;
                            commandAttachedUnit.RotatedDirection = displayDirection;
                            commandAttachedUnit.Position3 = mapGameCommandItem.Position3;
                            commandAttachedUnit.RotatedPosition3 = mapGameCommandItem.Position3;
                            commandAttachedUnit.IsVisible = true;
                            PreviewUnits.Add(commandAttachedUnit);
                        }
                        updatePosition = true;
                    }
                    else
                    {
                        commandAttachedUnit.MapGameCommandItem = mapGameCommandItem;
                        remainingPreviews.Remove(commandAttachedUnit);
                    }
                }
                foreach (CommandAttachedUnit searchAttachedUnit in remainingPreviews)
                {
                    searchAttachedUnit.Delete();
                    PreviewUnits.Remove(searchAttachedUnit);
                }
            }
            return updatePosition;
        }

        public void CreateCommandPreview(MapGameCommand gameCommand)
        {
            GameCommand = gameCommand;
            CreateCommandLogo();
            IsPreview = true;
            UpdateCommandPreview(gameCommand);
        }

        public override string ToString()
        {
            if (GameCommand != null)
                return GameCommand.ToString();

            return base.ToString();
        }
    }
}
