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
    /*
    public class CommandAttachedItem
    {
        public CommandAttachedItem(MapGameCommand mapGameCommandItem)
        {
            MapGameCommand = mapGameCommandItem;
            AttachedUnit = new CommandAttachedUnit(mapGameCommandItem.AttachedUnit);
            FactoryUnit = new CommandAttachedUnit(mapGameCommandItem.FactoryUnit);
            TransportUnit = new CommandAttachedUnit(mapGameCommandItem.TransportUnit);
            TargetUnit = new CommandAttachedUnit(mapGameCommandItem.TargetUnit);
        }
        public MapGameCommand MapGameCommand { get; set; }
        
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
    }*/

    public class CommandPreview
    {
        private static int ClientCommandId;
        public CommandPreview()
        {


        }

        public static int GetNextClientCommandId()
        {
            return ++ClientCommandId;
        }

        public CommandAttachedUnit AttachedUnit { get; private set; }
        public CommandAttachedUnit FactoryUnit { get; private set; }
        public CommandAttachedUnit TransportUnit { get; private set; }
        public CommandAttachedUnit TargetUnit { get; private set; }

        public MapGameCommand GameCommand { get; private set; }
        public Command Command { get; set; }
        public CommandPreview NextCommandPreview { get; set; }
        internal bool IsPreview { get; set; }

        private GameObject previewGameCommand;
        private GameObject alertGameObject;

        private void Initialize(MapGameCommand gameCommand)
        {
            GameCommand = gameCommand;
            GameCommand.PlayerId = 1;
            GameCommand.ClientId = GetNextClientCommandId();
            GameCommand.TargetPosition = Position2.Null;

            displayDirection = gameCommand.Direction;
            displayPosition = gameCommand.TargetPosition;
            displayRadius = gameCommand.Radius;

            AttachedUnit = new CommandAttachedUnit(GameCommand.AttachedUnit);
            FactoryUnit = new CommandAttachedUnit(GameCommand.FactoryUnit);
            TransportUnit = new CommandAttachedUnit(GameCommand.TransportUnit);
            TargetUnit = new CommandAttachedUnit(GameCommand.TargetUnit);
        }

        public void CreateCommandPreview(MapGameCommand gameCommand)
        {
            Initialize(gameCommand);

            CreateCommandLogo();
            IsPreview = true;
            UpdateCommandPreview(gameCommand);
        }

        public UnitBase UnitBase { get; set; }

        /// <summary>
        /// Command for unit at no specific position
        /// </summary>
        /// <param name="unitBase"></param>
        /// <param name="gameCommandType"></param>
        public void CreateCommand(UnitBase unitBase, GameCommandType gameCommandType)
        {
            UnitBase = unitBase;

            MapGameCommand gameCommand = new MapGameCommand();
            gameCommand.Layout = "UINone";
            gameCommand.GameCommandType = gameCommandType;
            gameCommand.Direction = unitBase.Direction;
            gameCommand.UnitId = unitBase.UnitId;
            gameCommand.PlayerId = unitBase.PlayerId;
            gameCommand.BlueprintName = unitBase.MoveUpdateStats.BlueprintName;
            gameCommand.Direction = unitBase.Direction;
            Initialize(gameCommand);

            CreateCommandLogo();
            IsPreview = true;
            UpdateCommandPreview(GameCommand);
        }

        public void CreateCommandForBuild(BlueprintCommand blueprint, string factoryUnitId)
        {
            MapGameCommand gameCommand = new MapGameCommand();
            gameCommand.Layout = blueprint.Layout;
            gameCommand.GameCommandType = GameCommandType.Build;
            gameCommand.FollowUpUnitCommand = blueprint.FollowUpUnitCommand;
            gameCommand.Direction = Direction.N;
            gameCommand.UnitId = factoryUnitId;
            gameCommand.BlueprintName = blueprint.Name;

            Initialize(gameCommand);

            if (GameCommand.GameCommandType == GameCommandType.Collect)
                displayRadius = 3;

            CreateCommandLogo();
            IsPreview = true;
            UpdateCommandPreview(GameCommand);
        }

        /// <summary>
        /// Command for unit at this position
        /// </summary>
        /// <param name="unitBase"></param>
        /// <param name="groundCell"></param>
        public void SelectCommandType(UnitBase unitBase, GroundCell groundCell)
        {
            if (groundCell != null)
                displayPosition = groundCell.Pos;

            GameCommandType gameCommandType = GameCommandType.AttackMove;
            if (unitBase != null)
            {
                UnitBasePart container = unitBase.GetContainer();
                if (container != null)
                {

                    foreach (TileObject tileObject in groundCell.Stats.MoveUpdateGroundStat.TileObjects)
                    {
                        if (TileObject.IsTileObjectTypeCollectable(tileObject.TileObjectType) &&
                            container.TileObjectContainer.IsSpaceFor(tileObject))
                        {
                            gameCommandType = GameCommandType.Collect;
                        }
                    }
                }
            }
            GameCommand.GameCommandType = gameCommandType;
        }



        public void Delete()
        {
            DeletePreviewItems();
            if (previewGameCommand != null)
            {
                HexGrid.Destroy(previewGameCommand);
                previewGameCommand = null;
            }
        }
        public void DeletePreviewItems()
        {
            if (fireLineRenderer != null)
            {
                HexGrid.Destroy(fireLineRenderer.gameObject);
                fireLineRenderer = null;
            }
            if (attackLineRenderer != null)
            {
                HexGrid.Destroy(attackLineRenderer.gameObject);
                attackLineRenderer = null;
            }

            if (AttachedUnit.GhostUnit != null)
            {
                AttachedUnit.GhostUnit.Delete();
                AttachedUnit.GhostUnit = null;
            }
            if (AttachedUnit.GhostUnitBounds != null)
            {
                AttachedUnit.GhostUnitBounds.Destroy();
                AttachedUnit.GhostUnitBounds = null;
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

            if (AttachedUnit.GhostUnit != null)
                AttachedUnit.GhostUnit.SetHighlighted(isHighlighted);
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


        //private UnitBase addPreviewGhost;
        //private GameObject addPreviewUnitMarker;
        /*
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

        }*/
        /*
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
        }*/

        public void CancelCommand()
        {
            MapGameCommand gameCommand = new MapGameCommand();

            gameCommand.ClientId = CommandPreview.GetNextClientCommandId();
            gameCommand.CommandId = GameCommand.CommandId;
            gameCommand.PlayerId = GameCommand.PlayerId;
            gameCommand.GameCommandType = GameCommandType.Cancel;

            HexGrid.MainGrid.GameCommands.Add(gameCommand);

            Delete();
        }
        public bool CanExecute()
        {
            return displayPosition != Position2.Null;
        }

        public void Execute()
        {
            //if (displayPosition != Position2.Null)
            {
                /*
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
                {*/
                //foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
                {
                    if (AttachedUnit.GhostUnitBounds != null)
                    {
                        AttachedUnit.GhostUnitBounds.Destroy();
                        AttachedUnit.GhostUnitBounds = null;
                    }

                    //if (commandAttachedUnit.AttachedUnit.Position3 == gameCommandItem.Position3)
                    {
                        //gameCommandItem.Direction = commandAttachedUnit.AttachedUnit.RotatedDirection;
                        //gameCommandItem.RotatedDirection = commandAttachedUnit.AttachedUnit.RotatedDirection;
                    }

                }

                //GameCommand.GameCommandType = GameCommand.GameCommandType;
                GameCommand.TargetPosition = displayPosition;
                GameCommand.Direction = displayDirection;
                GameCommand.Radius = displayRadius;
                IsPreview = false;

                if (GameCommand.NextGameCommand != null)
                {
                    GameCommand.NextGameCommand.TargetPosition = displayPosition;
                    GameCommand.NextGameCommand.Direction = displayDirection;
                    GameCommand.NextGameCommand.Radius = displayRadius;
                }
                HexGrid.MainGrid.AddCommand(this);
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
            /*
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
            */
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
                    if (UnitBase != null)
                        SelectCommandType(UnitBase, groundCell);

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
            if (NextCommandPreview != null)
            {
                NextCommandPreview.SetPosition(groundCell);
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

            if (turnRight)
                displayDirection = Dir.TurnRight(displayDirection);
            else
                displayDirection = Dir.TurnLeft(displayDirection);


                Position3 position3;
                if (turnRight)
                    position3 = AttachedUnit.RotatedPosition3.RotateRight();
                else
                    position3 = AttachedUnit.RotatedPosition3.RotateLeft();
                AttachedUnit.RotatedPosition3 = position3;
                AttachedUnit.RotatedDirection = displayDirection;

                if (AttachedUnit.GhostUnit != null)
                    AttachedUnit.GhostUnit.Direction = displayDirection;
            
            GroundCell gc;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(DisplayPosition, out gc))
            {
                UpdatePositions(gc);
            }

        }
        private void Hide()
        {
            if (previewGameCommand != null)
                previewGameCommand.SetActive(false);

            displayPosition = Position2.Null;
            //foreach (CommandAttachedItem commandAttachedUnit in PreviewUnits)
            {
                AttachedUnit.IsVisible = false;
                if (AttachedUnit.GhostUnit != null)
                    AttachedUnit.GhostUnit.IsVisible = false;
                if (AttachedUnit.GhostUnitBounds != null)
                    AttachedUnit.GhostUnitBounds.IsVisible = false;
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

        private LineRenderer fireLineRenderer;

        private void UpdateFirePosition(GroundCell groundCell)
        {
            if (fireLineRenderer != null)
            {
                HexGrid.Destroy(fireLineRenderer.gameObject);
                fireLineRenderer = null;
            }

            GameCommand.TargetPosition = groundCell.Pos;
            //previewGameCommand.transform.position = groundCell.transform.position;
            Debug.Log("Command fire preview " + GameCommand.UnitId + " to " + GameCommand.TargetPosition.ToString());
            if (GameCommand.UnitId != null)
            {
                UnitBase unitBase;
                if (HexGrid.MainGrid.BaseUnits.TryGetValue(GameCommand.UnitId, out unitBase))
                {
                    UnitBasePart partWeapon = null;
                    foreach (UnitBasePart unitBasePart in unitBase.UnitBaseParts)
                    {
                        if (unitBasePart.PartType == TileObjectType.PartWeapon && unitBasePart.Level == unitBasePart.CompleteLevel)
                        {
                            partWeapon = unitBasePart;
                            break;
                        }
                    }
                    if (partWeapon != null)
                    {
                        Transform transformWeapon = partWeapon.Part.transform;
                        Transform barrelTrans = partWeapon.Part.transform.Find("Barrel");

                        List<Position2> positions = unitBase.GetHitablePositions();
                        if (positions.Contains(GameCommand.TargetPosition))
                        {
                            Vector3 groundPos3 = groundCell.transform.position;

                            float angle;
                            Vector3 velocityVector = partWeapon.CalcBallisticVelocityVector(barrelTrans.position, groundPos3, out angle);
                            //Rotate the barrel
                            //The equation we use assumes that if we are rotating the gun up from the
                            //pointing "forward" position, the angle increase from 0, but our gun's angles
                            //decreases from 360 degress when we are rotating up
                            barrelTrans.localEulerAngles = new Vector3(360f - angle, 0f, 0f);

                            //Rotate the gun turret towards the target
                            transformWeapon.LookAt(groundPos3);
                            transformWeapon.eulerAngles = new Vector3(0f, transformWeapon.rotation.eulerAngles.y, 0f);

                            Vector3 currentPos = barrelTrans.position;
                            Vector3 currentVel = velocityVector;

                            GameObject lineRendererObject = new GameObject();
                            lineRendererObject.name = "UnitLine";
                            LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
                            lineRenderer.transform.SetParent(HexGrid.MainGrid.transform, false);
                            lineRenderer.material = HexGrid.MainGrid.GetMaterial("Player1");
                            lineRenderer.startWidth = 0.05f;
                            lineRenderer.endWidth = 0.05f;

                            UnitBullet.DrawTrajectoryPath(lineRenderer, currentVel, currentPos);

                            fireLineRenderer = lineRenderer;
                        }
                    }
                }
            }
        
        }

        public static Vector3[] SmoothLine(Vector3[] inputPoints, float segmentSize)
        {
            //create curves
            AnimationCurve curveX = new AnimationCurve();
            AnimationCurve curveY = new AnimationCurve();
            AnimationCurve curveZ = new AnimationCurve();

            //create keyframe sets
            Keyframe[] keysX = new Keyframe[inputPoints.Length];
            Keyframe[] keysY = new Keyframe[inputPoints.Length];
            Keyframe[] keysZ = new Keyframe[inputPoints.Length];

            //set keyframes
            for (int i = 0; i < inputPoints.Length; i++)
            {
                keysX[i] = new Keyframe(i, inputPoints[i].x);
                keysY[i] = new Keyframe(i, inputPoints[i].y);
                keysZ[i] = new Keyframe(i, inputPoints[i].z);
            }

            //apply keyframes to curves
            curveX.keys = keysX;
            curveY.keys = keysY;
            curveZ.keys = keysZ;

            //smooth curve tangents
            for (int i = 0; i < inputPoints.Length; i++)
            {
                curveX.SmoothTangents(i, 0);
                curveY.SmoothTangents(i, 0);
                curveZ.SmoothTangents(i, 0);
            }

            //list to write smoothed values to
            List<Vector3> lineSegments = new List<Vector3>();

            //find segments in each section
            for (int i = 0; i < inputPoints.Length; i++)
            {
                //add first point
                lineSegments.Add(inputPoints[i]);

                //make sure within range of array
                if (i + 1 < inputPoints.Length)
                {
                    //find distance to next point
                    float distanceToNext = Vector3.Distance(inputPoints[i], inputPoints[i + 1]);

                    //number of segments
                    int segments = (int)(distanceToNext / segmentSize);

                    //add segments
                    for (int s = 1; s < segments; s++)
                    {
                        //interpolated time on curve
                        float time = ((float)s / (float)segments) + (float)i;

                        //sample curves to find smoothed position
                        Vector3 newSegment = new Vector3(curveX.Evaluate(time), curveY.Evaluate(time), curveZ.Evaluate(time));

                        //add to list
                        lineSegments.Add(newSegment);
                    }
                }
            }

            return lineSegments.ToArray();
        }


        private LineRenderer attackLineRenderer;

        private void UpdateAttackPosition(GroundCell groundCell)
        {
            if (attackLineRenderer != null)
            {
                HexGrid.Destroy(attackLineRenderer.gameObject);
                attackLineRenderer = null;
            }

            GameCommand.TargetPosition = groundCell.Pos;
            //previewGameCommand.transform.position = groundCell.transform.position;
            Debug.Log("Command attack preview " + GameCommand.UnitId + " to " + GameCommand.TargetPosition.ToString());

            string unitID;
            if (IsPreview)
            {
                unitID = GameCommand.UnitId;
            }
            else

            {
                unitID = GameCommand.AttachedUnit.UnitId;
            }
            if (unitID != null)
            {
                UnitBase unitBase;
                if (HexGrid.MainGrid.BaseUnits.TryGetValue(unitID, out unitBase))
                {
                    List<Position2> path = HexGrid.MainGrid.FindPath(unitBase.CurrentPos, groundCell.Pos, unitBase.UnitId);
                    if (path != null && path.Count > 1)
                    {
                        if (IsPreview || displayDirection == Direction.C)
                        {
                            displayDirection = Position3.CalcDirection(path[path.Count - 2], path[path.Count - 1]);
                            AttachedUnit.RotatedDirection = displayDirection;
                            AttachedUnit.Direction = displayDirection;

                            /*
                            Position3 dir = new Position3(path[path.Count - 2]);
                            foreach (Position3 n in dir.Neighbors)
                            {
                                if (n.Pos == path[path.Count - 1])
                                {
                                    displayDirection = n.Direction;
                                    AttachedUnit.RotatedDirection = displayDirection;
                                    AttachedUnit.Direction = displayDirection;
                                    break;
                                }
                            }*/

                        }

                        GameObject lineRendererObject = new GameObject();
                        lineRendererObject.name = "AttackLine";

                        LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
                        lineRenderer.transform.SetParent(HexGrid.MainGrid.transform, false);
                        lineRenderer.material = HexGrid.MainGrid.GetMaterial("Player1");
                        lineRenderer.startWidth = 0.1f;
                        lineRenderer.endWidth = 0.1f;

                        List<Vector3> positions = new List<Vector3>();
                        for (int i = 0; i < path.Count; i++)
                        {
                            Position2 pos = path[i];

                            GroundCell pathCell;
                            if (HexGrid.MainGrid.GroundCells.TryGetValue(pos, out pathCell))
                            {
                                Vector3 pathPosition = pathCell.transform.position;
                                pathPosition.y += 0.2f;
                                positions.Add(pathPosition);
                            }
                        }

                        Vector3[] sm = SmoothLine(positions.ToArray(), 0.01f);
                        lineRenderer.positionCount = sm.Length;
                        for (int i = 0; i < sm.Length; i++)
                        {
                            lineRenderer.SetPosition(i, sm[i]);
                        }

                        attackLineRenderer = lineRenderer;
                    }
                }
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
            if (GameCommand.GameCommandType == GameCommandType.Fire)
            {
                UpdateFirePosition(groundCell);
                return;
            }
            if (GameCommand.GameCommandType == GameCommandType.AttackMove)
            {
                UpdateAttackPosition(groundCell);
            }

            Vector3 unitPos3 = groundCell.transform.position;
            if (GameCommand.GameCommandType == GameCommandType.Build)
                unitPos3.y += 1.5f;
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
                    Command.UpdateDirection(neighbor.transform.position);
                }
            }

            Position3 relativePosition3 = centerPosition3.Add(AttachedUnit.RotatedPosition3);

            AttachedUnit.GhostUnit.CurrentPos = relativePosition3.Pos;
            AttachedUnit.GhostUnit.TeleportToPosition(true);
            AttachedUnit.IsVisible = true;

            AttachedUnit.GhostUnit.IsVisible = true;
            if (AttachedUnit.GhostUnitBounds != null)
            {
                AttachedUnit.GhostUnitBounds.IsVisible = true;
                AttachedUnit.GhostUnitBounds.Update();
            }

            if (displayDirection != Direction.C)
            {
                neighborPosition3 = relativePosition3.GetNeighbor(displayDirection);

                GroundCell neighbor;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(neighborPosition3.Pos, out neighbor))
                {
                    AttachedUnit.GhostUnit.UpdateDirection(neighbor.transform.position, true);
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
            string layout = GameCommand.Layout;
            if (layout == null) layout = "UINone";

            previewGameCommand = HexGrid.Instantiate(HexGrid.MainGrid.GetResource(layout));
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
                if (displayRadius > 0)
                {
                    collectUnitBounds = new CollectBounds(displayPosition, displayRadius);
                    collectUnitBounds.Update(1);
                }
            }

            if (visible == true)
            {
                if (AttachedUnit.GhostUnitBounds != null)
                {
                    AttachedUnit.GhostUnitBounds.Destroy();
                    AttachedUnit.GhostUnitBounds = null;
                }
                if (AttachedUnit.GhostUnit != null)
                {
                    // On select
                    AttachedUnit.GhostUnitBounds = new UnitBounds(AttachedUnit.GhostUnit);

                    if (IsPreview)
                    {
                        AttachedUnit.GhostUnitBounds.AddBuildGrid();
                    }
                    AttachedUnit.GhostUnitBounds.Update();
                }
            }
            else
            {
                if (AttachedUnit.GhostUnitBounds != null)
                {
                    AttachedUnit.GhostUnitBounds.Destroy();
                    AttachedUnit.GhostUnitBounds = null;
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
            if (AttachedUnit.GhostUnit == unitBase)
                return true;

            return false;
        }

        public bool UpdateCommandPreview(MapGameCommand gameCommand)
        {
            bool updatePosition = false;
            if (GameCommand.GameCommandType != gameCommand.GameCommandType)
            {
                DeletePreviewItems();
            }

            GameCommand = gameCommand;

            if (GameCommand.GameCommandType == GameCommandType.ItemRequest ||
                GameCommand.GameCommandType == GameCommandType.Collect ||
                GameCommand.GameCommandType == GameCommandType.Fire)
            {
                // Do not show the worker to build
            }
            else if (GameCommand.GameCommandType == GameCommandType.AttackMove ||
                     GameCommand.GameCommandType == GameCommandType.Build)
            {
                if (AttachedUnit.GhostUnit == null && GameCommand.BlueprintName != null)
                {
                    Blueprint blueprint = HexGrid.MainGrid.game.Blueprints.FindBlueprint(GameCommand.BlueprintName);
                    UnitBase previewUnit = HexGrid.MainGrid.CreateTempUnit(blueprint, GameCommand.PlayerId);

                    previewUnit.Direction = GameCommand.Direction;
                    //Debug.Log("Unit points to " + previewUnit.Direction.ToString());

                    previewUnit.DectivateUnit();
                    previewUnit.transform.SetParent(HexGrid.MainGrid.transform, false);

                    AttachedUnit.GhostUnit = previewUnit;
                    AttachedUnit.Direction = displayDirection;
                    AttachedUnit.RotatedDirection = displayDirection;
                    AttachedUnit.IsVisible = true;
                }
                updatePosition = true;
            }
            return updatePosition;
        }

        public override string ToString()
        {
            if (GameCommand != null)
                return GameCommand.ToString();

            return base.ToString();
        }
    }
}
