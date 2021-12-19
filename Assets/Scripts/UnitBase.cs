using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HighlightPlus;
using System;

namespace Assets.Scripts
{

    public readonly struct UnitAlert
    {
        public UnitAlert(string header, string text, bool isAlert)
        {
            Header = header;
            Text = text;
            IsAlert = isAlert;
        }
        public bool IsAlert { get;  }
        public string Header { get; }
        public string Text { get; }
    }

    public class UnitCommand
    {
        public MapGameCommand GameCommand { get; set; }
        public GameObject GameObject { get; set; }
        public GroundCell TargetCell { get; set; }
        public UnitBase Owner { get; set; }
    }
    public class UnitBaseTileObject
    {
        public GameObject Placeholder { get; set; }
        public GameObject GameObject { get; set; }
        public TileObject TileObject { get; set; }
    }
    public class TransitObject
    {
        public TransitObject()
        {
            Speed = 3.0f;
        }
        public  bool RigidBodyDeactivated { get; set; }

        public float? StartAfterThis { get; set; }
        public float? EndAfterThis { get; set; }
        public float Speed { get; set; }
        public GameObject GameObject { get; set; }
        public GameObject ActivateAtArrival { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Vector3? TargetDirection { get; set; }
        public Quaternion TargetRotation { get; set; }
        public bool DestroyAtArrival { get; set; }
        public bool HideAtArrival { get; set; }
        public bool ScaleDown { get; set; }
        public bool ScaleUp { get; set; }
        public bool TargetReached { get; set; }
    }

    public class HitByBullet
    {
        public HitByBullet(Position2 fireingPosition)
        {
            FireingPosition = fireingPosition;
        }
        public UnitBase TargetUnit { get; set; }
        public TileObjectType HitPartTileObjectType { get; set; }
        public float HitTime { get; set; }
        public TileObject Bullet { get; set; }
        public bool ShieldHit { get; set; }
        public bool UpdateStats { get; set; }
        public bool Deleted { get; set; }
        public bool BulletImpact { get; set; }
        public Position2 FireingPosition { get; set; }
        public Position2 TargetPosition { get; set; }
        public MoveUpdateStats UpdateUnitStats { get; set; }
        public MoveUpdateStats UpdateGroundStats { get; set; }
    }


    public class UnitBase : MonoBehaviour
    {
        public UnitBase()
        {
            UnitCommands = new List<UnitCommand>();
            UnitBaseParts = new List<UnitBasePart>();
            AboveGround = 0f;
            TurnWeaponIntoDirection = Vector3.zero;
            TurnIntoDirection = Direction.C;
            SetAlert(new UnitAlert("Created", "", false));
        }

        internal Position2 CurrentPos { get; set; }
        // Todo: turn into dir
        internal Direction Direction { get; set; }
        internal Position2 DestinationPos { get; set; }
        internal int PlayerId { get; set; }
        internal string UnitId { get; set; }
        public MoveUpdateStats MoveUpdateStats { get; set; }

        public List<UnitBasePart> UnitBaseParts { get; private set; }

        public List<string> PartsThatHaveBeenHit { get; set; }

        internal bool Temporary { get; set; }
        internal bool UnderConstruction { get; set; }
        internal bool HasBeenDestroyed { get; set; }

        
        internal bool IsVisible
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                if (gameObject.activeSelf != value)
                {
                    //isVisible = value;
                    gameObject.SetActive(value);
                }
            }
        }

        private UnitAlert unitAlert;
        public UnitAlert UnitAlert { get { return unitAlert;  } }
        public void SetAlert(UnitAlert unitAlert)
        {
            this.unitAlert = unitAlert;
            if (_alert != null)
            {
                _alert.SetActive(unitAlert.IsAlert);
            }
        }
        public void ResetAlert()
        {
            this.unitAlert = new UnitAlert("", "", false);
            if (_alert != null)
            {
                _alert.SetActive(false);
            }
        }

        internal Vector3 TurnWeaponIntoDirection { get; set; }

        private Rigidbody _rigidbody;
        private GameObject _alert;

        private bool teleportToPosition;
        
        private Vector3 moveToVector;
        private int moveToVectorTimes;

        public void MoveTo(Position2 pos)
        {
            DestinationPos = pos;

            GroundCell currentCell;
            if (!HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out currentCell))
            {
                return;
            }
            GroundCell targetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(DestinationPos, out targetCell))
            {
                Vector3 unitPos3 = targetCell.transform.position;
                unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                
                moveToVectorTimes = (int)(50 * HexGrid.MainGrid.GameSpeed);
                moveToVectorTimes--;
                moveToVector = (unitPos3 - transform.position) / (moveToVectorTimes+1);
                //moveToVector = (unitPos3 - currentCell.transform.position) / moveToVectorTimes;

                if (_rigidbody != null)
                {
                    if (targetCell.transform.position.y > currentCell.transform.position.y)
                    {
                        // Climb up
                        //Debug.Log("Climb up " + moveToVector.ToString());
                        _rigidbody.isKinematic = true;
                    }
                    else
                    {
                        if (_rigidbody.isKinematic)
                        {
                            //Debug.Log("Is up " + moveToVector.ToString());
                            _rigidbody.isKinematic = false;
                        }
                    }
                }
                IsVisible = targetCell.Visible;
            }
        }


        private void Start()
        {
            Transform child = transform.Find("Alert");
            if (child != null)
                _alert = child.gameObject;

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody != null && IsGhost)
                _rigidbody.Sleep();
            if (IsGhost)
            {
                RemoveColider();
            }
            TeleportToPosition(true);
        }


        public void TeleportToPosition(bool force)
        {
            
            
            //moveToVector = Vector3.zero;
            if (CurrentPos == Position2.Null)
                return;

            GroundCell targetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out targetCell))
            {
                Vector3 unitPos3 = targetCell.transform.position;
                unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;

                if (_rigidbody != null && !IsGhost)
                {
                    if (force)
                    {
                        _rigidbody.position = unitPos3;
                        transform.position = unitPos3;
                    }
                    /*
                    Vector3 vector = targetCell.transform.position - transform.position;
                    if (vector.x < 0.1 && vector.x > -0.1 && vector.z < 0.1 && vector.z > -0.1)
                    {

                    }
                    else
                    {
                        //Debug.Log("Teleport Rigidbody " + vector.x + "," + vector.y + "," + vector.z);
                        _rigidbody.position = unitPos3;
                        //_rigidbody.velocity = Vector3.zero;
                    }
                    */
                }
                else
                {
                    transform.position = unitPos3;
                }
                /*
                if (IsVisible != targetCell.Visible)
                {
                    IsVisible = targetCell.Visible;
                    gameObject.SetActive(targetCell.Visible);
                }*/
            }

            if (TurnIntoDirection != Direction.C)
            {
                Position3 relativePosition3 = new Position3(CurrentPos);
                //if (isMoveMode)
                //    neighborPosition3 = relativePosition3.GetNeighbor(commandAttachedUnit.RotatedDirection);
                //else
                Position3 neighborPosition3 = relativePosition3.GetNeighbor(TurnIntoDirection);

                GroundCell neighbor;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(neighborPosition3.Pos, out neighbor))
                {
                    UpdateDirection(neighbor.transform.position, true);
                }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (IsGhost)
                return;
            if (HexGrid.MainGrid.IsPause)
                return;

            //fixedFrameCounter++;

            if (selectionChanged)
            {
                UpdateWayPoints();
                selectionChanged = false;
            }

            if (TurnWeaponIntoDirection != Vector3.zero)
            {
                foreach (UnitBasePart unitBasePart in UnitBaseParts)
                {
                    if (unitBasePart.PartType == TileObjectType.PartWeapon)
                    {
                        GameObject partToTurn;
                        if (unitBasePart.CompleteLevel == 1)
                        {
                            partToTurn = unitBasePart.Part;
                        }
                        else
                        {
                            partToTurn = FindChildNyName(unitBasePart.Part, unitBasePart.Name + unitBasePart.CompleteLevel + "-" + unitBasePart.CompleteLevel);
                        }

                        float str; // = Mathf.Min(2f * Time.deltaTime, 1);
                        str = 8f * Time.deltaTime;

                        // Calculate a rotation a step closer to the target and applies rotation to this object
                        Quaternion lookRotation = Quaternion.LookRotation(TurnWeaponIntoDirection);

                        // Rotate the forward vector towards the target direction by one step
                        Quaternion newrotation = Quaternion.Slerp(partToTurn.transform.rotation, lookRotation, str);
                        partToTurn.transform.rotation = newrotation;

                        float angle = Quaternion.Angle(lookRotation, newrotation);
                        if (angle < 5)
                        {
                            unitBasePart.FireBullet();
                            TurnWeaponIntoDirection = Vector3.zero;
                        }
                        break;
                    }
                }
            }


            float timeNow = Time.time;
            foreach (UnitBasePart unitBasePart in UnitBaseParts)
            {
                if (!HasEngine() && timeNow > unitBasePart.AnimateFrom && timeNow < unitBasePart.AnimateTo)
                {
                    unitBasePart.Part.transform.Rotate(Vector3.up, 6);
                }
                if (unitBasePart.PartType == TileObjectType.PartContainer && !unitBasePart.Destroyed)
                {
                    unitBasePart.Part.transform.Rotate(Vector3.up, 1);
                }
            }

            if (teleportToPosition)
            {
                teleportToPosition = false;
                TeleportToPosition(false);
            }
            else if (_rigidbody != null || DestinationPos != Position2.Null)
            {
                /* OMG!!
                 * How to Set Up Dynamic Water Physics and Boat Movement in Unity
                 * https://www.youtube.com/watch?v=eL_zHQEju8s
                if (transform.position.y < 1f)
                {
                    float displacementMultiplier;
                    displacementMultiplier = Mathf.Clamp01(1 - transform.position.y / 1f) * 3f;
                    y = Mathf.Abs(Physics.gravity.y) * displacementMultiplier;
                    //_rigidbody.AddForce(new Vector3(0, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0), ForceMode.Acceleration);
                }*/

                Position2 position2 = DestinationPos;
                if (DestinationPos == Position2.Null)
                    position2 = CurrentPos;

                GroundCell targetCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(position2, out targetCell))
                {
                    if (_rigidbody != null && !IsGhost)
                    {
                        /*
                        float maxVector = 0.9f;
                        float maxVectorn = -0.9f;

                        Vector3 vector = targetCell.transform.position - transform.position;
                        while (vector.x > maxVector || vector.x < maxVectorn || vector.z > maxVector || vector.z < maxVectorn)
                            vector *= 0.9f;

                        // Shake while not moving
                        if (vector.x < 0.1 && vector.x > -0.1 && vector.z < 0.1 && vector.z > -0.1)
                        {
                            int n = HexGrid.MainGrid.Random.Next(35);
                            if (n == 0) vector.x = 0.05f;
                            if (n == 1) vector.x = -0.05f;
                            if (n == 2) vector.z = 0.05f;
                            if (n == 3) vector.z = -0.05f;
                            if (n == 4) vector.y = 0.2f;
                        }

                        float speed = 2.15f / HexGrid.MainGrid.GameSpeed;
                        float step = speed * Time.deltaTime;
                        */
                        //_rigidbody.MovePosition(transform.position + vector * step);

                        //Debug.Log("MoveToVector" + moveToVector.ToString());


                        if (moveToVectorTimes > 0)
                        {
                            // Avoid accelaration, reason...?
                            _rigidbody.velocity = Vector3.zero;

                            _rigidbody.MovePosition(transform.position + moveToVector);
                            moveToVectorTimes--;
                        }
                        else
                        {
                            /*
                            if (_rigidbody.position == targetCell.transform.position)
                            {
                                _rigidbody.velocity = Vector3.zero;
                            }
                            else
                            {*/
                            float speed = 2.75f / HexGrid.MainGrid.GameSpeed;
                            float step = speed * Time.deltaTime;
                            Vector3 unitPos3 = targetCell.transform.position;
                            unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                            _rigidbody.MovePosition(Vector3.MoveTowards(transform.position, unitPos3, step));

                        }

                        /*
                        float speed = 1.75f / HexGrid.MainGrid.GameSpeed;
                        float step = speed * Time.deltaTime;
                        Vector3 unitPos3 = targetCell.transform.position;
                        unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                        _rigidbody.MovePosition(Vector3.MoveTowards(transform.position, unitPos3, step));
                        */
                    }
                    else
                    {
                        float speed = 1.75f / HexGrid.MainGrid.GameSpeed;
                        float step = speed * Time.deltaTime;
                        Vector3 unitPos3 = targetCell.transform.position;
                        unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                        transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);
                    }
                    if (!IsGhost)
                    {
                        if (IsVisible != targetCell.Visible)
                        {
                            IsVisible = targetCell.Visible;
                            gameObject.SetActive(targetCell.Visible);
                        }
                    }
                }
            }

            if (TurnIntoDirection == Direction.C)
            {
                if (_rigidbody != null)
                {
                    StayUpright();
                }
            }
            else
            {
                Position3 position3 = new Position3(CurrentPos);
                Position3 neighbor = position3.GetNeighbor(TurnIntoDirection);

                GroundCell targetCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(neighbor.Pos, out targetCell))
                {
                    Vector3 unitPos3 = targetCell.transform.position;
                    unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                    if (HasEngine())
                    {
                        UpdateDirection(unitPos3, teleportToPosition);
                    }
                }
            }
            if (teleportToPosition)
            {
                teleportToPosition = false;
            }
        }
        
        private void StayUpright()
        {
            Quaternion deltaQuat = Quaternion.FromToRotation(_rigidbody.transform.up, Vector3.up);

            Vector3 axis;
            float angle;
            deltaQuat.ToAngleAxis(out angle, out axis);

            float dampenFactor = 0.8f; // this value requires tuning
            _rigidbody.AddTorque(-_rigidbody.angularVelocity * dampenFactor, ForceMode.Acceleration);

            float adjustFactor = 0.5f; // this value requires tuning
            _rigidbody.AddTorque(axis.normalized * angle * adjustFactor, ForceMode.Acceleration);
        }

        public void UpdateDirection(Vector3 position, bool teleportToPosition)
        {
            // Determine which direction to rotate towards
            Vector3 targetDirection = position - transform.position;

            float singleStep;
            if (teleportToPosition)
            {
                singleStep = 7;
            }
            else
            {
                //float speed = 1.75f;
                float speed = 3.5f / HexGrid.MainGrid.GameSpeed;
                // The step size is equal to speed times frame time.
                singleStep = speed * Time.deltaTime;
            }
            Vector3 forward = transform.forward;
            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(forward, targetDirection, singleStep, 0.0f);
            if (newDirection.y < -0.1f)
                newDirection.y = -0.1f;
            else if (newDirection.y > 0.1f)
                newDirection.y = 0.1f;

            // Draw a ray pointing at our target in
            //Debug.DrawRay(transform.position, newDirection, Color.red);
            if (_rigidbody != null)
            {
                if (teleportToPosition)
                {
                    _rigidbody.rotation = Quaternion.LookRotation(newDirection);
                    transform.rotation = Quaternion.LookRotation(newDirection);
                }
                else
                {
                    StayUpright();

                    _rigidbody.MoveRotation(Quaternion.LookRotation(newDirection));
                }
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(newDirection);
            }
        }

        internal float AboveGround { get; set; }
        public void PutAtCurrentPosition(bool update, bool updateVisibility)
        {
            if (_rigidbody == null)
            {
                teleportToPosition = true;
            }
            /*
            GroundCell targetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out targetCell))
            {
                if (updateVisibility || IsVisible != targetCell.Visible)
                {
                    if (IsGhost)
                        IsVisible = true;
                    else
                        IsVisible = targetCell.Visible;
                }
            }*/
        }

        public Direction TurnIntoDirection { get; set; }

        public void TurnTo(Direction direction)
        {
            TurnIntoDirection = direction;            
        }

        public void UpdateStats(MoveUpdateStats stats)
        {
            if (stats != null)
            {
                if (stats.Power < 10)
                {
                    //SetAlert(new UnitAlert("LowPower", "NeedReactor", true));
                }
                else
                {
                }
                if (stats.MoveUpdateStatsCommand == null)
                {
                    ResetAlert();
                }
                else
                {
                    MoveUpdateStatsCommand moveUpdateStatsCommand = stats.MoveUpdateStatsCommand;
                    if (moveUpdateStatsCommand.AttachedUnit.UnitId == UnitId)
                    {
                        SetAlert(new UnitAlert(moveUpdateStatsCommand.AttachedUnit.Status, moveUpdateStatsCommand.AttachedUnit.Status, moveUpdateStatsCommand.AttachedUnit.Alert));
                    }
                    if (moveUpdateStatsCommand.FactoryUnit.UnitId == UnitId)
                    {
                        SetAlert(new UnitAlert(moveUpdateStatsCommand.FactoryUnit.Status, moveUpdateStatsCommand.FactoryUnit.Status, moveUpdateStatsCommand.FactoryUnit.Alert));
                    }
                    if (moveUpdateStatsCommand.TransportUnit.UnitId == UnitId)
                    {
                        SetAlert(new UnitAlert(moveUpdateStatsCommand.TransportUnit.Status, moveUpdateStatsCommand.TransportUnit.Status, moveUpdateStatsCommand.TransportUnit.Alert));
                    }
                    if (moveUpdateStatsCommand.TargetUnit.UnitId == UnitId)
                    {
                        SetAlert(new UnitAlert(moveUpdateStatsCommand.TargetUnit.Status, moveUpdateStatsCommand.TargetUnit.Status, moveUpdateStatsCommand.TargetUnit.Alert));
                    }
                }
                

                if (IsActive && stats.Power == 0)
                {
                    DectivateUnit();
                }
                MoveUpdateStats = stats;
                UpdateParts();
            }
        }

        public void Delete()
        {
            HasBeenDestroyed = true;
            Destroy(gameObject);
        }
        private bool selectionChanged;

        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
        }
        public void SetSelected(bool value)
        {
            isSelected = value;
        }

        public bool IsHighlighted { get; private set; }

        private HighlightEffect highlightEffect { get; set; }

        internal void SetHighlighted(bool highlighted)
        {
            //Debug.Log("Highlight: " + this.UnitId + " " + highlighted);
            if (IsHighlighted != highlighted)
            {
                IsHighlighted = highlighted;
                if (highlightEffect)
                {
                    highlightEffect.SetHighlighted(highlighted);
                }
            }
        }
        public bool HasEngine()
        {
            foreach (UnitBasePart unitBasePart in UnitBaseParts)
            {
                if (unitBasePart.PartType == TileObjectType.PartEngine)
                    return true;
            }
            return false;
        }
        internal List<UnitCommand> UnitCommands { get; private set; }

        public void ClearWayPoints(GameCommandType gameCommandType)
        {
            List<UnitCommand> toBeRemoved = new List<UnitCommand>();
            foreach (UnitCommand unitCommand in UnitCommands)
            {
                if (unitCommand.GameObject != null &&
                    unitCommand.GameCommand.GameCommandType == gameCommandType)
                {
                    toBeRemoved.Add(unitCommand);
                }
            }
            foreach (UnitCommand unitCommand in toBeRemoved)
            {
                UnitCommands.Remove(unitCommand);
                HexGrid.Destroy(unitCommand.GameObject);
            }

        }

        public void UpdateWayPoints()
        {
            foreach (UnitCommand unitCommand in UnitCommands)
            {
                if (IsHighlighted)
                {
                    if (unitCommand.GameObject == null)
                    {
                        GroundCell targetCell = HexGrid.MainGrid.GroundCells[unitCommand.GameCommand.TargetPosition];

                        GameObject waypointPrefab = HexGrid.MainGrid.GetResource("Waypoint");

                        unitCommand.GameObject = Instantiate(waypointPrefab, targetCell.transform, false);
                        unitCommand.GameObject.name = "Waypoint";

                        //var go = new GameObject();
                        var lr = unitCommand.GameObject.GetComponent<LineRenderer>();

                        if (unitCommand.GameCommand.GameCommandType == GameCommandType.Attack)
                        {
                            //lr.startWidth = 0.1f;
                            lr.startColor = Color.red;
                            //lr.endWidth = 0.1f;
                            lr.endColor = Color.red;
                        }

                        if (unitCommand.GameCommand.GameCommandType == GameCommandType.Collect)
                        {
                            //lr.startWidth = 0.1f;
                            lr.startColor = Color.green;
                            //lr.endWidth = 0.1f;
                            lr.endColor = Color.green;
                        }

                        Vector3 v1 = transform.position;
                        Vector3 v2 = targetCell.transform.position;
                        v1.y += 0.3f;
                        v2.y += 0.3f;

                        lr.SetPosition(0, v1);
                        lr.SetPosition(1, v2);
                    }
                }
                else
                {
                    if (unitCommand.GameObject != null)
                    {
                        HexGrid.Destroy(unitCommand.GameObject);
                        unitCommand.GameObject = null;
                    }
                }
            }
        }

        public void ChangePlayer(int newPlayerId)
        {
            PlayerId = newPlayerId;

            foreach (UnitBasePart unitBasePart in UnitBaseParts)
            {
                SetPlayerColor(PlayerId, unitBasePart.Part);
            }
        }

        public void Extract(Move move, UnitBase unit, UnitBase otherUnit)
        {
            if (IsVisible)
            {
                Extractor.Extract(move, unit, otherUnit);
            }
        }

        // Reactor burned something
        public void BurnMove(Move move)
        {
            if (IsVisible)
            {
                foreach (UnitBasePart unitBasePart in UnitBaseParts)
                {
                    if (unitBasePart.PartType == TileObjectType.PartReactor)
                    {
                        foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
                        {
                            // Transit the ingredient into the weapon. This is the reloaded ammo. (Can be empty)
                            UnitBaseTileObject unitBaseTileObject;
                            unitBaseTileObject = RemoveTileObject(moveRecipeIngredient);
                            if (unitBaseTileObject != null)
                            {
                                // Transit ingredient
                                TransitObject transitObject = new TransitObject();
                                transitObject.GameObject = unitBaseTileObject.GameObject;
                                transitObject.TargetPosition = unitBasePart.Part.transform.position;
                                transitObject.DestroyAtArrival = true;

                                unitBaseTileObject.GameObject = null;
                                HexGrid.MainGrid.AddTransitTileObject(transitObject);
                            }
                        }

                        TileObjectType tileObjectType = move.MoveRecipe.Ingredients[0].TileObjectType;

                        GameObject gameObject = null;
                        if (tileObjectType == TileObjectType.Mineral)
                        {
                            gameObject = HexGrid.Instantiate(HexGrid.MainGrid.ReactorBurnMineral, unitBasePart.Part.transform);
                        }
                        else if (tileObjectType == TileObjectType.Wood)
                        {
                            gameObject = HexGrid.Instantiate(HexGrid.MainGrid.ReactorBurnWood, unitBasePart.Part.transform);
                        }

                        if (gameObject != null)
                        {
                            Vector3 vector3 = unitBasePart.Part.transform.position;
                            if (MoveUpdateStats.BlueprintName == "Outpost")
                                vector3.y += 0.8f;
                            else
                                vector3.y += 2;
                            gameObject.transform.position = vector3;

                            ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
                            particleSystem.Play();
                            Destroy(gameObject, 3 * HexGrid.MainGrid.GameSpeed);
                        }
                        else
                        {
                            unitBasePart.AnimateFrom = Time.time + (0.3f * HexGrid.MainGrid.GameSpeed);
                            unitBasePart.AnimateTo = Time.time + (0.99f * HexGrid.MainGrid.GameSpeed);
                        }
                        break;
                    }
                }
            }
        }

        public void Upgrade(Move move, UnitBase upgradedUnit)
        {
            foreach (UnitBasePart upgradedBasePart in upgradedUnit.UnitBaseParts)
            {
                if (upgradedBasePart.PartType == move.MoveRecipe.Result)
                {
                    int nextLevel = upgradedBasePart.Level + 1;

                    GameObject upgradedPart = null;
                    if (upgradedBasePart.CompleteLevel > 1)
                    {
                        // Activate the main part
                        GameObject upgradedMainPart;

                        upgradedMainPart = FindChildNyName(upgradedUnit.gameObject, upgradedBasePart.Name + upgradedBasePart.CompleteLevel);
                        if (nextLevel == 1)
                        {
                            upgradedMainPart.SetActive(true);
                            for (int i = 0; i < upgradedMainPart.transform.childCount; i++)
                            {
                                GameObject child = upgradedMainPart.transform.GetChild(i).gameObject;
                                child.SetActive(false);
                            }

                        }
                        GameObject upgradedSubPart;
                        for (int level = 1; level <= upgradedBasePart.CompleteLevel; level++)
                        {
                            upgradedSubPart = FindChildNyName(upgradedMainPart, upgradedBasePart.Name + upgradedBasePart.CompleteLevel + "-" + level);
                            if (upgradedSubPart != null)
                            {
                                if (nextLevel == level)
                                {
                                    upgradedPart = upgradedSubPart;
                                }
                            }
                        }
                    }
                    else
                    {
                        upgradedPart = FindChildNyName(upgradedUnit.gameObject, upgradedBasePart.Name + upgradedBasePart.CompleteLevel);
                    }
                    // Needs to be reinitialzed
                    upgradedBasePart.TileObjectContainer = null;
                    if (upgradedBasePart.Destroyed)
                        upgradedBasePart.Destroyed = false;

                    foreach (MoveRecipeIngredient moveRecipeIngredient in move.MoveRecipe.Ingredients)
                    {
                        UnitBaseTileObject unitBaseTileObject = unitBaseTileObject = RemoveTileObject(moveRecipeIngredient);
                        if (unitBaseTileObject != null && unitBaseTileObject.GameObject != null)
                        {
                            // Transit ingredient
                            TransitObject transitObject = new TransitObject();
                            transitObject.GameObject = unitBaseTileObject.GameObject;
                            transitObject.TargetPosition = transform.position;
                            transitObject.DestroyAtArrival = true;

                            unitBaseTileObject.GameObject = null;
                            HexGrid.MainGrid.AddTransitTileObject(transitObject);
                        }
                    }

                    if (upgradedPart != null)
                    {
                        TileObjectContainer.HidePlaceholders(upgradedPart);

                        // Output transit
                        GameObject upgradedPartClone = Instantiate(upgradedPart, HexGrid.MainGrid.transform);

                        TransitObject transitObject = new TransitObject();
                        transitObject.GameObject = upgradedPartClone;
                        transitObject.TargetPosition = upgradedPart.transform.position;
                        transitObject.DestroyAtArrival = true;
                        transitObject.ScaleUp = true;
                        transitObject.ActivateAtArrival = upgradedPart;
                        transitObject.StartAfterThis = Time.time + (0.5f * HexGrid.MainGrid.GameSpeed);
                        //Debug.Log("Transit after " + transitObject.StartAfterThis + " " + upgradedPart.name);

                        // Reset current pos to assembler
                        upgradedPartClone.transform.position = transform.position;
                        upgradedPartClone.transform.rotation = upgradedPart.transform.rotation;

                        float scalex = 0.4f;
                        float scaley = 0.4f;
                        float scalez = 0.4f;

                        Vector3 scaleChange;
                        scaleChange = new Vector3(scalex, scaley, scalez);
                        upgradedPartClone.transform.localScale = scaleChange;

                        upgradedPartClone.SetActive(true);

                        // Move to position in unit
                        HexGrid.MainGrid.AddTransitTileObject(transitObject);

                        foreach (UnitBasePart unitBasePart in UnitBaseParts)
                        {
                            if (unitBasePart.PartType == TileObjectType.PartAssembler)
                            {
                                GameObject activeGameobject = FindChildNyName(unitBasePart.Part, "Active");
                                if (activeGameobject != null)
                                {
                                    ParticleSystem particleSystem = activeGameobject.GetComponent<ParticleSystem>();
                                    particleSystem.Play();
                                }
                                else
                                {
                                    unitBasePart.AnimateFrom = Time.time + (0.3f * HexGrid.MainGrid.GameSpeed);
                                    unitBasePart.AnimateTo = Time.time + (0.6f * HexGrid.MainGrid.GameSpeed);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            
            if (highlightEffect != null)
                highlightEffect.Refresh();
        }
        

        internal UnitBaseTileObject RemoveTileObject(MoveRecipeIngredient moveRecipeIngredient)
        {
            if (moveRecipeIngredient.SourcePosition != CurrentPos)
            {
                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(moveRecipeIngredient.SourcePosition, out gc))
                {
                    UnitBase unitBase = gc.FindUnit();
                    if (unitBase != null)
                        return unitBase.RemoveTileObject(moveRecipeIngredient);
                }
            }
            if (moveRecipeIngredient.Source == TileObjectType.None || moveRecipeIngredient.Source == TileObjectType.Ground)
            {
                // From ground
                GroundCell gc;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(moveRecipeIngredient.SourcePosition, out gc))
                {
                    foreach (UnitBaseTileObject unitBaseTileObject in gc.GameObjects)
                    {
                        if (unitBaseTileObject.TileObject.TileObjectType == moveRecipeIngredient.TileObjectType &&
                            unitBaseTileObject.GameObject != null)
                        {
                            gc.GameObjects.Remove(unitBaseTileObject);
                            return unitBaseTileObject;
                        }
                    }
                }
            }
            else
            {

                foreach (UnitBasePart unitBasePart in UnitBaseParts)
                {
                    if (moveRecipeIngredient.Source == unitBasePart.PartType)
                    {
                        if (TileObject.CanConvertTileObjectIntoMineral(moveRecipeIngredient.TileObjectType))
                        {
                            // Remove the part, not the content
                            UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();

                            unitBaseTileObject.TileObject = new TileObject();
                            unitBaseTileObject.TileObject.TileObjectType = TileObjectType.Mineral;
                            unitBaseTileObject.TileObject.Direction = Direction.C;

                            GameObject partObject;
                            if (unitBasePart.Level > 0 && unitBasePart.CompleteLevel > 1)
                            {
                                partObject = UnitBase.FindChildNyName(unitBasePart.Part, unitBasePart.Name + unitBasePart.CompleteLevel + "-" + unitBasePart.Level);
                            }
                            else
                            {

                                partObject = unitBasePart.Part;
                            }
                            if (partObject == null)
                            {
                                throw new Exception("Missing partobject");
                            }
                            else
                            {
                                unitBaseTileObject.GameObject = HexGrid.Instantiate(partObject, transform);
                                partObject.SetActive(false);

                                return unitBaseTileObject;
                            }
                        }
                        else
                        {
                            if (unitBasePart.TileObjectContainer == null)
                            {
                                // Happend. Destroyed?
                            }
                            else
                            {
                                // From Container
                                foreach (UnitBaseTileObject unitBaseTileObject in unitBasePart.TileObjectContainer.TileObjects)
                                {
                                    if (unitBaseTileObject.TileObject.TileObjectType == moveRecipeIngredient.TileObjectType &&
                                        unitBaseTileObject.GameObject != null)
                                    {
                                        unitBasePart.TileObjectContainer.Remove(unitBaseTileObject);
                                        return unitBaseTileObject;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }            
            return null;
        }

        public void Fire(Move move)
        {
            if (IsVisible)
            {
                foreach (UnitBasePart unitBasePart in UnitBaseParts)
                {
                    if (unitBasePart.PartType == TileObjectType.PartWeapon)
                    {
                        unitBasePart.Fire(move);
                        break;
                    }
                }
            }
        }

        public void Transport(Move move)
        {
            if (IsVisible)
            {
                Container.Transport(this, move);
            }
        }

        internal void SetSelectColor(int playerId, GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Shield") && !child.name.StartsWith("Ammo") && !child.name.StartsWith("Item"))
                    SetSelectColor(playerId, child);
            }

            MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (meshRenderer.materials.Length == 1)
                {
                    //Destroy(meshRenderer.material);
                    //meshRenderer.material = HexGrid.GetMaterial("glow 1");

                    //meshRenderer.material.shader = Shader.Find("HDRenderPipeline/glow");
                    //meshRenderer.material.SetColor("_BaseColor", color);

                    //Color color = new Color32(27, 34, 46, 0);
                    //Color newColor = Color.HSVToRGB(1, 1, 1, true);

                    //meshRenderer.material.SetFloat("is_selected", 1);
                    meshRenderer.material.SetFloat("Darkness", 3.0f);
                }
            }

        }

        internal void SetMaterialGhost(int playerId, GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                SetMaterialGhost(playerId, child);
            }
            if (unit.name.StartsWith("Mineral") ||
                unit.name.StartsWith("Shield") ||
                unit.name.StartsWith("Ammo") || 
                unit.name.StartsWith("Item"))
            {
                unit.SetActive(false);
            }
            else
            { 
                MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    if (HexMapEditor.IsPlaying)
                    {
                        if (meshRenderer.materials.Length == 1)
                        {
                            Destroy(meshRenderer.material);
                            meshRenderer.material = HexGrid.MainGrid.GetMaterial("UnitGhost");
                            meshRenderer.material.SetColor("PlayerColor", GetPlayerColor(playerId));                            
                            meshRenderer.material.SetFloat("Darkness", 0.9f);
                        }
                    }
                    else
                    {
                        if (meshRenderer.sharedMaterials.Length == 1)
                        {
                            meshRenderer.sharedMaterial = HexGrid.MainGrid.GetMaterial("UnitGhost");
                            //meshRenderer.sharedMaterial.SetColor("PlayerColor", GetPlayerColor(playerId));
                            //meshRenderer.sharedMaterial.SetFloat("Darkness", 0.9f);
                        }
                    }
                }
            }
        }

        internal static GameObject FindChildNyName(GameObject unit, string name)
        {
            if (unit.name.StartsWith(name))
                return unit;
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (child.name.StartsWith(name))
                    return child;

                child = FindChildNyName(child, name);
                if (child != null)
                    return child;
            }
            return null;
        }

        internal static Color GetPlayerColor(int playerId)
        {
            Color color = Color.black;
            //ColorUtility.TryParseHtmlString("#7D0054", out color);
            //return color;

            
            //if (playerId == 1) ColorUtility.TryParseHtmlString("#FFA200", out color);
            if (playerId == 1) ColorUtility.TryParseHtmlString("#0606AD", out color);
            if (playerId == 2) ColorUtility.TryParseHtmlString("#7D0054", out color);
            if (playerId == 3) ColorUtility.TryParseHtmlString("#1FD9D5", out color);
            if (playerId == 4) ColorUtility.TryParseHtmlString("#1FD92B", out color);

            return color;
        }
        public void RemoveColider()
        {
            RemoveColider(gameObject);
        }
            
        internal static void RemoveColider(GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                RemoveColider(child);
            }
            BoxCollider boxCollider = unit.GetComponent<BoxCollider>();
            if (boxCollider != null)
                boxCollider.enabled = false;
        }



        internal static void DeactivateRigidbody(GameObject unit)
        {
            Rigidbody rigidbody = unit.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.Sleep();
            }
        }

        internal static void ActivateRigidbody(GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                ActivateRigidbody(child);
            }
            Rigidbody otherRigid = unit.GetComponent<Rigidbody>();

            if (otherRigid != null)
            {
                Vector3 explosionPos = unit.transform.position;
                explosionPos.y -= 0.1f;

                otherRigid.isKinematic = false;

                otherRigid.AddExplosionForce(250, explosionPos, 1);
                
                /*
                Vector3 vector3 = new Vector3();
                vector3.y = 12 + Random.value * 3;
                vector3.x = Random.value * 3;
                vector3.z = Random.value * 3;

                otherRigid.velocity = vector3;
                otherRigid.rotation = Random.rotation;
                */
            }
        }

        internal static void SetPlayerColor(int playerId, GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Shield") && !child.name.StartsWith("Ammo") && !child.name.StartsWith("Item"))
                    SetPlayerColor(playerId, child);
            }

            MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (HexMapEditor.IsPlaying)
                {
                    if (meshRenderer.materials.Length == 1)
                    {
                        if (meshRenderer.material.shader.name.Contains("Unitshader"))
                        {
                            Destroy(meshRenderer.material);
                            meshRenderer.material = HexGrid.MainGrid.GetMaterial("UnitMaterial");
                        }
                        
                        meshRenderer.material.SetColor("PlayerColor", GetPlayerColor(playerId));
                        meshRenderer.material.SetFloat("Darkness", 0.9f);
                    }
                }
                else
                {
                    if (meshRenderer.sharedMaterials.Length == 1)
                    {
                        //Destroy(meshRenderer.material);
                        //meshRenderer.sharedMaterial = hexGrid.GetMaterial("UnitMaterial");
                        //meshRenderer.sharedMaterial.SetColor("PlayerColor", GetPlayerColor(playerId));
                        //meshRenderer.sharedMaterial.SetFloat("Darkness", 0.9f);
                    }
                }
            }
        }
        public static UnitBase GetUnitFrameColilder(Collider collider)
        {
            UnitBase unitBase = collider.GetComponent<UnitBase>();
            if (unitBase != null) return unitBase;

            Transform transform = collider.transform;

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
        public UnitBasePart PartHitByShell(TileObjectType hitPart, MoveUpdateStats stats)
        {
            foreach (UnitBasePart unitBasePart in UnitBaseParts)
            {
                if (unitBasePart.PartType == hitPart)
                {
                    MoveUpdateUnitPart moveUpdateUnitPart = null;
                    if (stats != null)
                    {
                        foreach (MoveUpdateUnitPart checkUpdateUnitPart in stats.UnitParts)
                        {
                            if (checkUpdateUnitPart.PartType == unitBasePart.PartType)
                            {
                                moveUpdateUnitPart = checkUpdateUnitPart;
                                break;
                            }
                        }
                    }
                    if (unitBasePart.TileObjectContainer != null)
                    {
                        if (moveUpdateUnitPart != null &&
                            moveUpdateUnitPart.Capacity.HasValue)
                        {
                            unitBasePart.TileObjectContainer.ExplodeExceedingCapacity(transform, moveUpdateUnitPart.Capacity.Value);
                        }
                        else
                        {
                            unitBasePart.TileObjectContainer.Explode(transform);
                        }                        
                    }

                    GameObject hitGameObject;
                    hitGameObject = unitBasePart.Part;

                    if (unitBasePart != null && unitBasePart.CompleteLevel > 1)
                    {
                        hitGameObject = FindChildNyName(unitBasePart.Part, unitBasePart.Name + unitBasePart.CompleteLevel + "-" + (unitBasePart.Level));
                    }
                    GroundCell currentCell;

                    if (hitGameObject != null && HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out currentCell))
                    {
                        hitGameObject.SetActive(false);
                        unitBasePart.Destroyed = true;

                        HexGrid.MainGrid.HitUnitPartAnimation(currentCell.transform);

                        GameObject animation;
                        if (HexGrid.MainGrid.Random.Next(2) == 0)
                        {
                            animation = HexGrid.Instantiate<GameObject>(HexGrid.MainGrid.UnitHitByMineral1, currentCell.transform);
                        }
                        else
                        {
                            animation = HexGrid.Instantiate<GameObject>(HexGrid.MainGrid.UnitHitByMineral2, currentCell.transform);
                        }
                        HexGrid.Destroy(animation, 1);

                    }


                    GameObject smoke = FindChildNyName(gameObject, "SmokeEffect");
                    if (smoke != null)
                        smoke.SetActive(true);

                    return unitBasePart;
                }
            }
            return null;
        }

        public void HitByShell()
        {
        }

        private bool IsBuilding()
        {
            if (MoveUpdateStats.UnitParts != null)
            {
                foreach (MoveUpdateUnitPart moveUpdateUnitPart in MoveUpdateStats.UnitParts)
                {
                    if (moveUpdateUnitPart.PartType == TileObjectType.PartEngine)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool IsGhost { get; set; }
        public void Assemble(bool underConstruction)
        {
            Assemble(underConstruction, false);
        }

        private static HighlightProfile defaultProfile;
        private void SetupHighlightEffect()
        {
            if (MoveUpdateStats.BlueprintName == "Outpost")
            {
                if (defaultProfile == null)
                    defaultProfile = highlightEffect.profile;
            }
            else
            {
                highlightEffect.ProfileLoad(defaultProfile);
            }
            highlightEffect.SetHighlighted(false);

            /*
            highlightEffect.ProfileLoad(highlightProfile);
            highlightEffect.overlay = 0;
            highlightEffect.outlineColor = Color.yellow;
            highlightEffect.outlineWidth = 3;
            highlightEffect.glow = 0;
            highlightEffect.innerGlow = 0;*/
        }

        public void Assemble(bool underConstruction, bool ghost)
        {
            highlightEffect = GetComponent<HighlightEffect>();
            if (highlightEffect != null)
                SetupHighlightEffect();
            IsGhost = ghost;

            UnitBaseParts.Clear();
            UnderConstruction = underConstruction;

            // Find the basic parts
            foreach (MoveUpdateUnitPart moveUpdateUnitPart in MoveUpdateStats.UnitParts)
            {
                GameObject part = FindChildNyName(gameObject, moveUpdateUnitPart.Name + moveUpdateUnitPart.CompleteLevel);
                if (part != null)
                {
                    UnitBasePart unitBasePart = new UnitBasePart(this);
                    unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;
                    unitBasePart.IsUnderConstruction = underConstruction;
                    unitBasePart.Level = moveUpdateUnitPart.Level;
                    unitBasePart.Range = moveUpdateUnitPart.Range;
                    unitBasePart.Name = moveUpdateUnitPart.Name;
                    unitBasePart.PartType = moveUpdateUnitPart.PartType;
                    unitBasePart.Part = part;
                    if (!ghost)
                        part.SetActive(false);

                    if (moveUpdateUnitPart.TileObjects != null)
                    {
                        unitBasePart.TileObjectContainer = new TileObjectContainer();
                    }
                    if (IsGhost)
                    {
                        TileObjectContainer.HidePlaceholders(unitBasePart.Part);
                    }
                    UnitBaseParts.Add(unitBasePart);
                }
            }
            if (ghost)
            {
                highlightEffect.SetHighlighted(true);
                highlightEffect.overlay = 0.05f;
                highlightEffect.overlayColor = Color.white;
                highlightEffect.outlineColor = GetPlayerColor(PlayerId);
                //highlightEffect.outlineWidth = 3;
                highlightEffect.glow = 0;
                highlightEffect.innerGlow = 0;

                //SetMaterialGhost(PlayerId, gameObject);
                SetPlayerColor(PlayerId, gameObject);
            }
            else if (underConstruction)
            {
                SetPlayerColor(PlayerId, gameObject);
            }
            else
            {
                SetPlayerColor(PlayerId, gameObject);
            }
            
        }

        public void UpdateParts()
        {
            bool missingPartFound = false;

            foreach (UnitBasePart unitBasePart in UnitBaseParts)
            {
                if (MoveUpdateStats.UnitParts != null)
                {
                    foreach (MoveUpdateUnitPart moveUpdateUnitPart in MoveUpdateStats.UnitParts)
                    {
                        if (unitBasePart.PartType == moveUpdateUnitPart.PartType)
                        {
                            if (unitBasePart.IsUnderConstruction && moveUpdateUnitPart.Exists)
                            {
                                // Change from transparent to reals
                                unitBasePart.IsUnderConstruction = false;
                            }

                            if (unitBasePart.TileObjectContainer == null)
                                unitBasePart.TileObjectContainer = new TileObjectContainer();

                            if (unitBasePart.Level > 0 &&
                                unitBasePart.Level != moveUpdateUnitPart.Level)
                            {
                                if (moveUpdateUnitPart.Capacity.HasValue)
                                {
                                    unitBasePart.TileObjectContainer.ExplodeExceedingCapacity(transform, moveUpdateUnitPart.Capacity.Value);
                                }
                                unitBasePart.Level = moveUpdateUnitPart.Level;
                            }
                            else
                            {
                                if (moveUpdateUnitPart.Capacity.HasValue && unitBasePart.TileObjectContainer != null)
                                {
                                    unitBasePart.TileObjectContainer.UpdateContent(this, unitBasePart.Part, moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity.Value);
                                }
                            }
                            if (unitBasePart.Level < moveUpdateUnitPart.Level)
                            {
                                unitBasePart.Level = moveUpdateUnitPart.Level;
                            }
                            if (moveUpdateUnitPart.Exists)
                            {
                                unitBasePart.Part.SetActive(true);
                            }
                            else
                            {
                                if (moveUpdateUnitPart.Level == 0)
                                {
                                    unitBasePart.Part.SetActive(false);
                                }
                                missingPartFound = true;
                            }
                            if (unitBasePart.PartType == TileObjectType.PartArmor)
                            {
                                Transform shield = transform.Find("Shield");
                                if (shield != null && IsVisible)
                                {
                                    shield.gameObject.SetActive(moveUpdateUnitPart.ShieldActive == true);
                                }
                            }
                            if (unitBasePart.PartType == TileObjectType.PartWeapon)
                            {

                            }
                            break;
                        }
                        
                    }
                }
            }
            bool alive = UnderConstruction;
            foreach (MoveUpdateUnitPart moveUpdateUnitPart in MoveUpdateStats.UnitParts)
            {
                if (moveUpdateUnitPart.Exists)
                    alive = true;
            }
            if (!alive)
            {
                HasBeenDestroyed = true;
                Destroy(gameObject, 10);
            }
            else
            {
                bool damaged = false;
                foreach (UnitBasePart unitBasePart in UnitBaseParts)
                {
                    if (unitBasePart.Destroyed || unitBasePart.Level != unitBasePart.CompleteLevel)
                    {
                        damaged = true;
                    }
                }
                if (!damaged)
                {
                    GameObject smoke = FindChildNyName(gameObject, "SmokeEffect");
                    if (smoke != null)
                        smoke.SetActive(false);
                }
            }
            if (UnderConstruction && missingPartFound == false)
            {
                // First time complete
                UnderConstruction = false;

                ActivateUnit();
            }
        }

        public bool IsActive { get; private set; }

        public void ActivateUnit()
        {
            if (!IsActive)
            {
                GameObject activeAnimation = FindChildNyName(gameObject, "ActiveAnimation");
                if (activeAnimation != null)
                {
                    activeAnimation.SetActive(true);
                }
                GameObject moveAnimation = FindChildNyName(gameObject, "MoveAnimation");
                if (moveAnimation != null)
                {
                    moveAnimation.SetActive(true);
                }
                SetAlert(new UnitAlert("Activated", "Chilling", false));
                AboveGround = 0.08f;
                IsActive = true;
            }
        }

        public void DectivateUnit()
        {
            IsActive = false;

            GameObject activeAnimation = FindChildNyName(gameObject, "ActiveAnimation");
            if (activeAnimation != null)
            {
                activeAnimation.SetActive(false);
            }
            GameObject moveAnimation = FindChildNyName(gameObject, "MoveAnimation");
            if (moveAnimation != null)
            {
                moveAnimation.SetActive(false);
            }
            Vector3 unitPos3 = transform.position;
            unitPos3.y -= AboveGround;
            transform.position = unitPos3;
            SetAlert(new UnitAlert("Dectivated", "", false));

            AboveGround = 0;

        }
    }

}