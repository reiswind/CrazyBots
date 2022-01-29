using Engine.Ants;
using Engine.Interface;
using Engine.Master;
using HighlightPlus;
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
        public CommandAttachedUnit(MapGameCommandItemUnit mapGameCommandItemUnit)
        {
            MapGameCommandItemUnit = mapGameCommandItemUnit;
            Direction = Direction.C;
        }
        public MapGameCommandItemUnit MapGameCommandItemUnit { get; private set; }

        public bool IsVisible { get; set; }
        public Position3 Position3 { get; set; }
        public Position3 RotatedPosition3 { get; set; }

        public Direction Direction { get; set; }
        public Direction RotatedDirection { get; set; }
        public UnitBase GhostUnit { get; set; }
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
    public class CommandAttachedItem
    {
        public CommandAttachedItem(MapGameCommandItem mapGameCommandItem)
        {
            MapGameCommandItem = mapGameCommandItem;
            AttachedUnit = new CommandAttachedUnit(mapGameCommandItem.AttachedUnit);
            FactoryUnit = new CommandAttachedUnit(mapGameCommandItem.FactoryUnit);
            TransportUnit = new CommandAttachedUnit(mapGameCommandItem.TransportUnit);
            TargetUnit = new CommandAttachedUnit(mapGameCommandItem.TargetUnit);
        }
        public MapGameCommandItem MapGameCommandItem { get; set; }
        
        public CommandAttachedUnit AttachedUnit { get; private set; }
        public CommandAttachedUnit FactoryUnit { get; private set; }
        public CommandAttachedUnit TransportUnit { get; private set; }
        public CommandAttachedUnit TargetUnit { get; private set; }

        public void Delete()
        {
            AttachedUnit.Delete();
            FactoryUnit.Delete();
            TransportUnit.Delete();
            TargetUnit.Delete();
        }
    }

    public class CommandPreview
    {
        public CommandPreview()
        {
            PreviewUnits = new List<CommandAttachedItem>();

            GameCommand = new MapGameCommand();
            GameCommand.PlayerId = 1;
            GameCommand.TargetPosition = Position2.Null;
            GameCommand.DeleteWhenFinished = false;
            displayDirection = Direction.N;
        }
        
        public MapGameCommand GameCommand { get; set; }
        public Command Command { get; set; }
        internal bool IsPreview { get; set; }
        public BlueprintCommand Blueprint { get; private set; }

        private GameObject previewGameCommand;
        private GameObject alertGameObject;

        public List<CommandAttachedItem> PreviewUnits { get; set; }

        public void CreateCommandForBuild(BlueprintCommand blueprint)
        {
            Blueprint = blueprint;
            GameCommand.Layout = blueprint.Layout;
            GameCommand.GameCommandType = blueprint.GameCommandType;
            GameCommand.Direction = displayDirection;

            if (GameCommand.GameCommandType == GameCommandType.Collect)
                displayRadius = 3;
        
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
                HexGrid.Destroy(previewGameCommand);
                previewGameCommand = null;
            }
            foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                if (commandAttachedUnit.AttachedUnit.GhostUnit != null)
                {
                    commandAttachedUnit.AttachedUnit.GhostUnit.Delete();
                    commandAttachedUnit.AttachedUnit.GhostUnit = null;
                }
                if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                {
                    commandAttachedUnit.AttachedUnit.GhostUnitBounds.Destroy();
                    commandAttachedUnit.AttachedUnit.GhostUnitBounds = null;
                }
            }
        }

        internal void SetHighlighted(bool isHighlighted)
        {
            Command.SetHighlighted(isHighlighted);

            if (GameCommand.GameCommandType == GameCommandType.Collect)
            {
                if (isHighlighted && collectUnitBounds == null)
                {
                    GroundCell gc;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(GameCommand.TargetPosition, out gc))
                    {
                        UpdatePositions(gc);
                    }
                }
                else if (!isHighlighted && collectUnitBounds != null)
                {
                    collectUnitBounds.Destroy();
                    collectUnitBounds = null;
                }
            }
            foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                if (commandAttachedUnit.AttachedUnit.GhostUnit != null)
                    commandAttachedUnit.AttachedUnit.GhostUnit.SetHighlighted(isHighlighted);
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

                bool canMove = false;
                if (GameCommand.GameCommandType == GameCommandType.Collect)
                {
                    if (groundCell.Stats.MoveUpdateGroundStat.Owner == 1)
                        canMove = true;
                }
                else if (GameCommand.GameCommandType == GameCommandType.Build)
                {
                    if (groundCell.Stats.MoveUpdateGroundStat.Owner == 1)
                    {
                        canMove = TileObject.CanMoveTo(groundCell.TileCounter);
                    }
                }
                else
                {
                    canMove = TileObject.CanMoveTo(groundCell.TileCounter);
                }
                if (!canMove)
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
            addPreviewGhost = HexGrid.MainGrid.CreateTempUnit(blueprint, GameCommand.PlayerId);
            
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
                foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
                {
                    commandAttachedUnit.AttachedUnit.IsVisible = false;
                    commandAttachedUnit.AttachedUnit.RotatedPosition3 = commandAttachedUnit.AttachedUnit.Position3;
                    commandAttachedUnit.AttachedUnit.RotatedDirection = commandAttachedUnit.AttachedUnit.Direction;

                    commandAttachedUnit.AttachedUnit.GhostUnit.SetHighlighted(false);
                    commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = false;
                }
                displayDirection = GameCommand.Direction;

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

            gameCommand.CommandId = GameCommand.CommandId;
            gameCommand.PlayerId = GameCommand.PlayerId;
            gameCommand.GameCommandType = GameCommandType.Cancel;

            HexGrid.MainGrid.GameCommands.Add(gameCommand);

            Delete();
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
                //Debug.Log("Add unit at " + groundCell.Pos + " DP: " + displayPosition);

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
                UpdateAllUnitBounds(true);
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

                    /* Command is updated next turn if return here else: */
                    mapGameCommandItem = new MapGameCommandItem(GameCommand);
                    mapGameCommandItem.BlueprintName = "Fighter";
                    mapGameCommandItem.Direction = addPreviewGhost.TurnIntoDirection;
                    mapGameCommandItem.RotatedDirection = addPreviewGhost.TurnIntoDirection;
                    mapGameCommandItem.Position3 = relativePosition3;
                    GameCommand.GameCommandItems.Add(mapGameCommandItem);

                    CommandAttachedItem commandAttachedItem = new CommandAttachedItem(mapGameCommandItem);
                    commandAttachedItem.AttachedUnit.Position3 = relativePosition3;
                    commandAttachedItem.AttachedUnit.RotatedPosition3 = relativePosition3;
                    commandAttachedItem.AttachedUnit.Direction = addPreviewGhost.TurnIntoDirection;
                    commandAttachedItem.AttachedUnit.RotatedDirection = addPreviewGhost.TurnIntoDirection;
                    commandAttachedItem.AttachedUnit.GhostUnit = addPreviewGhost;
                    commandAttachedItem.AttachedUnit.GhostUnitBounds = new UnitBounds(addPreviewGhost);
                    commandAttachedItem.AttachedUnit.GhostUnitBounds.AddBuildGrid();
                    commandAttachedItem.AttachedUnit.IsVisible = true;

                    PreviewUnits.Add(commandAttachedItem);
                    addPreviewGhost = null;
                    addPreviewUnitMarker = null;
                }
                else if (isMoveMode)
                {
                    MapGameCommand moveGameCommand = new MapGameCommand();

                    moveGameCommand.CommandId = GameCommand.CommandId;
                    moveGameCommand.TargetPosition = displayPosition;
                    moveGameCommand.GameCommandType = GameCommandType.Move;
                    moveGameCommand.Radius = displayRadius;
                    moveGameCommand.PlayerId = 1;
                    moveGameCommand.Direction = displayDirection;

                    foreach (CommandAttachedItem commandAttachItem in PreviewUnits)
                    {
                        MapGameCommandItem gameCommandItem = new MapGameCommandItem(moveGameCommand);

                        gameCommandItem.Position3 = commandAttachItem.AttachedUnit.Position3;
                        gameCommandItem.Direction = commandAttachItem.AttachedUnit.Direction;
                        gameCommandItem.BlueprintName = commandAttachItem.MapGameCommandItem.BlueprintName;
                        gameCommandItem.RotatedPosition3 = commandAttachItem.AttachedUnit.RotatedPosition3;
                        gameCommandItem.RotatedDirection = commandAttachItem.AttachedUnit.RotatedDirection;

                        moveGameCommand.GameCommandItems.Add(gameCommandItem);
                    }
                    HexGrid.MainGrid.GameCommands.Add(moveGameCommand);

                    // Preview remains, the real gamecommand should be at the same position
                    isMoveMode = false;
                }
                else
                {
                    foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
                    {
                        if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                        {
                            commandAttachedUnit.AttachedUnit.GhostUnitBounds.Destroy();
                            commandAttachedUnit.AttachedUnit.GhostUnitBounds = null;
                        }
                        foreach (MapGameCommandItem gameCommandItem in GameCommand.GameCommandItems)
                        {
                            if (commandAttachedUnit.AttachedUnit.Position3 == gameCommandItem.Position3)
                            {
                                gameCommandItem.Direction = commandAttachedUnit.AttachedUnit.RotatedDirection;
                                gameCommandItem.RotatedDirection = commandAttachedUnit.AttachedUnit.RotatedDirection;
                            }
                        }
                    }

                    GameCommand.GameCommandType = GameCommand.GameCommandType;
                    GameCommand.TargetPosition = displayPosition;
                    GameCommand.Direction = displayDirection;
                    GameCommand.Radius = displayRadius;                    
                    IsPreview = false;

                    // Remove the command after the structure is complete
                    if (GameCommand.GameCommandType == GameCommandType.Build)
                        GameCommand.DeleteWhenFinished = true;

                    HexGrid.MainGrid.GameCommands.Add(GameCommand);
                    HexGrid.MainGrid.CreatedCommandPreviews.Add(this);
                }
            }
        }
        /*
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
        }*/

        private int displayRadius;
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
                        //Debug.Log("addPreviewGhost: " + groundCell.Pos);
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
                        //Debug.Log("addPreviewGhost NO GROUND");
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

                        if (!IsPreview || GameCommand.GameCommandType == GameCommandType.ItemRequest)
                        {
                            positionok = true;
                        }
                        else
                        {
                            positionok = CanBuildAt(groundCell);
                        }
                        if (positionok)
                        {
                            //Debug.Log("SetPosition: " + groundCell.Pos);

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
        public void IncreaseRadius()
        {
            if (collectUnitBounds != null && displayRadius < 6)
            {
                collectUnitBounds.Destroy();
                displayRadius++;
                collectUnitBounds = new CollectBounds(displayPosition, displayRadius);
                collectUnitBounds.Update(1);
            }
        }
        public void DecreaseRadius()
        {
            if (collectUnitBounds != null && displayRadius > 1)
            {
                collectUnitBounds.Destroy();
                displayRadius--;
                collectUnitBounds = new CollectBounds(displayPosition, displayRadius);
                collectUnitBounds.Update(1);
            }
        }
        public void RotateCommand(bool turnRight)
        {
            if (addPreviewGhost == null)
            {
                if (turnRight)
                    displayDirection = Dir.TurnRight(displayDirection);
                else
                    displayDirection = Dir.TurnLeft(displayDirection);

                foreach (CommandAttachedItem commandAttachedItem in PreviewUnits)
                {
                    Position3 position3;
                    if (turnRight)
                        position3 = commandAttachedItem.AttachedUnit.RotatedPosition3.RotateRight();
                    else
                        position3 = commandAttachedItem.AttachedUnit.RotatedPosition3.RotateLeft();
                    commandAttachedItem.AttachedUnit.RotatedPosition3 = position3;
                    commandAttachedItem.AttachedUnit.RotatedDirection = displayDirection;

                    if (commandAttachedItem.AttachedUnit.GhostUnit != null)
                        commandAttachedItem.AttachedUnit.GhostUnit.Direction = displayDirection;
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
                addPreviewGhost.TurnIntoDirection = Dir.TurnRight(addPreviewGhost.TurnIntoDirection);
                
            }
        }
        private void Hide()
        {
            if (previewGameCommand != null)
                previewGameCommand.SetActive(false);

            displayPosition = Position2.Null;
            foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                commandAttachedUnit.AttachedUnit.IsVisible = false;
                commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = false;
                if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                    commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = false;
            }
            if (collectUnitBounds != null)
            {
                collectUnitBounds.Destroy();
                collectUnitBounds = null;
            }
        }

        private CollectBounds collectUnitBounds;
        public CollectBounds CollectBounds
        {
            get
            {
                return collectUnitBounds;
            }
        }

        private void UpdateCollectPosition(GroundCell groundCell)
        {
            Vector3 unitPos3 = groundCell.transform.position;
            unitPos3.y += 1.0f;
            previewGameCommand.transform.position = unitPos3;

            if (collectUnitBounds != null)
                collectUnitBounds.Destroy();

            if (IsPreview || IsSelected || IsHighlighted)
            {
                collectUnitBounds = new CollectBounds(groundCell.Pos, displayRadius);
                collectUnitBounds.Update(1);
            }
        }

        public void UpdatePositions(GroundCell groundCell)
        {
            if (previewGameCommand == null)
                return;
            if (GameCommand.GameCommandType == GameCommandType.Collect)
            {
                UpdateCollectPosition(groundCell);
                return;
            }

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
                {

                    //Debug.Log("Logo UpdateDirection to " + displayDirection.ToString());

                    Command.UpdateDirection(neighbor.transform.position);
                }
            }
            foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                Position3 relativePosition3 = centerPosition3.Add(commandAttachedUnit.AttachedUnit.RotatedPosition3);

                commandAttachedUnit.AttachedUnit.GhostUnit.CurrentPos = relativePosition3.Pos;
                commandAttachedUnit.AttachedUnit.GhostUnit.TeleportToPosition(true);
                commandAttachedUnit.AttachedUnit.IsVisible = true;

                commandAttachedUnit.AttachedUnit.GhostUnit.IsVisible = true;
                if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                {
                    commandAttachedUnit.AttachedUnit.GhostUnitBounds.IsVisible = true;
                    commandAttachedUnit.AttachedUnit.GhostUnitBounds.Update();
                }

                if (displayDirection != Direction.C)
                {
                    //if (isMoveMode)
                    //    neighborPosition3 = relativePosition3.GetNeighbor(commandAttachedUnit.RotatedDirection);
                    //else
                    neighborPosition3 = relativePosition3.GetNeighbor(displayDirection);

                    GroundCell neighbor;
                    if (HexGrid.MainGrid.GroundCells.TryGetValue(neighborPosition3.Pos, out neighbor))
                    {
                        commandAttachedUnit.AttachedUnit.GhostUnit.UpdateDirection(neighbor.transform.position, true);
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

            UnitBase.SetPlayerColor(GameCommand.PlayerId, previewGameCommand);
            HighlightEffect highlightEffect = previewGameCommand.GetComponent<HighlightEffect>();
            if (highlightEffect != null)
            {
                highlightEffect.SetHighlighted(true);
                highlightEffect.overlay = 0.05f;
                highlightEffect.overlayColor = Color.white;
                highlightEffect.outlineColor = UnitBase.GetPlayerColor(GameCommand.PlayerId);
            }

            Transform alert = previewGameCommand.transform.Find("Alert");
            if (alert != null)
            {
                alertGameObject = alert.gameObject;
                alertGameObject.SetActive(false);
            }
            Command = previewGameCommand.GetComponent<Command>();
            Command.CommandPreview = this;

            //Debug.Log("Logo points to " + displayDirection.ToString());

        }
        public bool IsSelected
        {
            get
            {
                if (Command == null) return false;
                return Command.IsSelected;
            }
        }
        public bool IsHighlighted
        {
            get
            {
                if (Command == null) return false;
                return Command.IsHighlighted;
            }
        }

        private void UpdateAllUnitBounds(bool visible)
        {

            if (collectUnitBounds != null)
            {
                collectUnitBounds.Destroy();
                collectUnitBounds = null;
            }

            if (visible == true)
            {
                collectUnitBounds = new CollectBounds(displayPosition, displayRadius);                
                collectUnitBounds.Update(1);
            }
            
            foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                if (visible == true)
                {
                    if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                    {
                        commandAttachedUnit.AttachedUnit.GhostUnitBounds.Destroy();
                        commandAttachedUnit.AttachedUnit.GhostUnitBounds = null;
                    }
                    if (commandAttachedUnit.AttachedUnit.GhostUnit != null)
                    {
                        // On select
                        commandAttachedUnit.AttachedUnit.GhostUnitBounds = new UnitBounds(commandAttachedUnit.AttachedUnit.GhostUnit);

                        if (IsPreview || IsMoveMode)
                        {
                            commandAttachedUnit.AttachedUnit.GhostUnitBounds.AddBuildGrid();
                        }
                        commandAttachedUnit.AttachedUnit.GhostUnitBounds.Update();
                    }
                }
                else
                {
                    if (commandAttachedUnit.AttachedUnit.GhostUnitBounds != null)
                    {
                        commandAttachedUnit.AttachedUnit.GhostUnitBounds.Destroy();
                        commandAttachedUnit.AttachedUnit.GhostUnitBounds = null;
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
            foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                if (commandAttachedUnit.AttachedUnit.GhostUnit == unitBase)
                    return true;
            }
            return false;
        }

        public bool UpdateCommandPreview(MapGameCommand gameCommand)
        {
            bool updatePosition = false;
            GameCommand = gameCommand;

            if (GameCommand.GameCommandType == GameCommandType.ItemRequest ||
                GameCommand.GameCommandType == GameCommandType.Collect)
            {
                // Do not show the worker to build
            }
            else
            {
                List<CommandAttachedItem> remainingPreviews = new List<CommandAttachedItem>();
                remainingPreviews.AddRange(PreviewUnits);

                foreach (MapGameCommandItem mapGameCommandItem in GameCommand.GameCommandItems)
                {
                    CommandAttachedItem commandAttachedItem = null;
                    foreach (CommandAttachedItem searchAttachedUnit in PreviewUnits)
                    {
                        if (searchAttachedUnit.AttachedUnit.Position3 == mapGameCommandItem.Position3)
                        {
                            commandAttachedItem = searchAttachedUnit;
                            break;
                        }
                    }
                    if (commandAttachedItem == null)
                    {
                        if (mapGameCommandItem.BlueprintName != null)
                        {
                            Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(mapGameCommandItem.BlueprintName);
                            UnitBase previewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint, GameCommand.PlayerId);
                            
                            previewUnit.Direction = GameCommand.Direction;
                            //Debug.Log("Unit points to " + previewUnit.Direction.ToString());

                            previewUnit.DectivateUnit();
                            previewUnit.transform.SetParent(HexGrid.MainGrid.transform, false);

                            //GameObject previewUnitMarker = HexGrid.MainGrid.InstantiatePrefab("GroundFrame");
                            //previewUnitMarker.transform.SetParent(HexGrid.MainGrid.transform, false);

                            // On create select                            
                            commandAttachedItem = new CommandAttachedItem(mapGameCommandItem);
                            commandAttachedItem.AttachedUnit.GhostUnit = previewUnit;
                            commandAttachedItem.AttachedUnit.Direction = displayDirection;
                            commandAttachedItem.AttachedUnit.RotatedDirection = displayDirection;
                            commandAttachedItem.AttachedUnit.Position3 = mapGameCommandItem.Position3;
                            commandAttachedItem.AttachedUnit.RotatedPosition3 = mapGameCommandItem.Position3;
                            commandAttachedItem.AttachedUnit.IsVisible = true;
                            PreviewUnits.Add(commandAttachedItem);
                        }
                        updatePosition = true;
                    }
                    else
                    {
                        commandAttachedItem.MapGameCommandItem = mapGameCommandItem;
                        remainingPreviews.Remove(commandAttachedItem);
                    }
                }
                foreach (CommandAttachedItem searchAttachedUnit in remainingPreviews)
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
            displayDirection = gameCommand.Direction;
            displayPosition = gameCommand.TargetPosition;
            displayRadius = gameCommand.Radius;

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
