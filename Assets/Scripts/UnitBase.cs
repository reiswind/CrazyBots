using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Assets.Scripts
{
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

        }
        public GameObject GameObject { get; set; }
        public GameObject ActivateAtArrival { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Quaternion TargetRotation { get; set; }
        public bool DestroyAtArrival { get; set; }
        public bool HideAtArrival { get; set; }
        public bool ScaleDown { get; set; }
    }

    public class HitByBullet
    {
        public HitByBullet(ulong fireingPosition)
        {
            FireingPosition = fireingPosition;
        }
        public UnitBase TargetUnit { get; set; }
        public TileObjectType HitPartTileObjectType { get; set; }
        public float HitTime { get; set; }
        public TileObject Bullet { get; set; }
        public bool BulletImpact { get; set; }
        public ulong FireingPosition { get; set; }
        public ulong TargetPosition { get; set; }
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
        }

        internal ulong CurrentPos { get; set; }
        // Todo: turn into dir
        internal Direction Direction { get; set; }
        internal ulong DestinationPos { get; set; }
        internal int PlayerId { get; set; }
        internal string UnitId { get; set; }
        public MoveUpdateStats MoveUpdateStats { get; set; }

        public List<UnitBasePart> UnitBaseParts { get; private set; }

        public List<string> PartsThatHaveBeenHit { get; set; }

        internal bool Temporary { get; set; }
        internal bool UnderConstruction { get; set; }
        internal bool HasBeenDestroyed { get; set; }
        internal bool IsVisible { get; set; }

        internal Vector3 TurnWeaponIntoDirection { get; set; }

        // Update is called once per frame
        void Update()
        {
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
                        float str; // = Mathf.Min(2f * Time.deltaTime, 1);
                        str = 8f * Time.deltaTime;

                        // Calculate a rotation a step closer to the target and applies rotation to this object
                        Quaternion lookRotation = Quaternion.LookRotation(TurnWeaponIntoDirection);

                        // Rotate the forward vector towards the target direction by one step
                        Quaternion newrotation = Quaternion.Slerp(unitBasePart.Part.transform.rotation, lookRotation, str);
                        unitBasePart.Part.transform.rotation = newrotation;

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
            if (DestinationPos != Position.Null)
            {
                GroundCell targetCell;
                if (HexGrid.MainGrid.GroundCells.TryGetValue(DestinationPos, out targetCell))
                {
                    Vector3 unitPos3 = targetCell.transform.localPosition;
                    unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;

                    float speed = 1.75f / HexGrid.MainGrid.GameSpeed;
                    float step = speed * Time.deltaTime;

                    transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);
                    if (HasEngine())
                    {
                        UpdateDirection(unitPos3);
                    }
                    if (IsVisible != targetCell.Visible)
                    {
                        IsVisible = targetCell.Visible;
                        gameObject.SetActive(targetCell.Visible);
                    }
                }
            }
        }

        void UpdateDirection(Vector3 position)
        {
            //float speed = 1.75f;
            float speed = 3.5f / HexGrid.MainGrid.GameSpeed;

            // Determine which direction to rotate towards
            Vector3 targetDirection = position - transform.position;

            // The step size is equal to speed times frame time.
            float singleStep = speed * Time.deltaTime;

            Vector3 forward = transform.forward;
            // Rotate the forward vector towards the target direction by one step
            Vector3 newDirection = Vector3.RotateTowards(forward, targetDirection, singleStep, 0.0f);
            newDirection.y = 0;

            // Draw a ray pointing at our target in
            //Debug.DrawRay(transform.position, newDirection, Color.red);

            // Calculate a rotation a step closer to the target and applies rotation to this object
            transform.rotation = Quaternion.LookRotation(newDirection);
        }

        private float AboveGround { get; set; }
        public void PutAtCurrentPosition(bool update)
        {
            transform.SetParent(HexGrid.MainGrid.transform, false);

            GroundCell targetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out targetCell))
            {
                Vector3 unitPos3 = targetCell.transform.localPosition;
                if (!update)
                {
                    unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                    transform.position = unitPos3;
                }
                else
                {
                    unitPos3.y += HexGrid.MainGrid.hexCellHeight + AboveGround;
                    transform.position = unitPos3;
                }
                if (!HasEngine())
                {
                    /*
                    Direction dir = Direction;
                    if (dir == Direction.C)
                        dir = Direction.NW;

                    GroundCell n = targetCell.GetNeighbor(dir);
                    if (n != null)
                    {
                        Vector3 newDirection = Vector3.RotateTowards(transform.position, n.transform.position, 360, 0.0f);
                        newDirection.y = 0;
                        transform.rotation = Quaternion.LookRotation(newDirection);
                    }*/
                }
                if (IsVisible = targetCell.Visible)
                {
                    IsVisible = targetCell.Visible;
                    gameObject.SetActive(targetCell.Visible);
                }
            }
        }


        public void MoveTo(ulong pos)
        {
            DestinationPos = pos;

            GroundCell targetCell;
            if (HexGrid.MainGrid.GroundCells.TryGetValue(DestinationPos, out targetCell))
            {
                /*
                if (Weapon != null)
                {
                    Weapon.TurnTo(DestinationPos);
                }*/
                if (IsVisible != targetCell.Visible)
                {
                    IsVisible = targetCell.Visible;
                    gameObject.SetActive(targetCell.Visible);
                }
            }
        }

        public void UpdateStats(MoveUpdateStats stats)
        {
            if (stats != null)
            {
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
            Destroy(this.gameObject);
        }
        private bool selectionChanged;
        public bool IsSelected { get; private set; }
        internal void SetSelected(bool selected)
        {
            if (IsSelected != selected)
            {
                selectionChanged = true;
                IsSelected = selected;

                if (!Temporary)
                {
                    if (IsSelected)
                    {
                        //selectionLight = HexGrid.CreateSelectionLight(gameObject);
                        SetSelectColor(PlayerId, gameObject);
                    }
                    else
                    {
                        SetPlayerColor(PlayerId, gameObject);

                        //Destroy(selectionLight);
                    }
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
                if (IsSelected)
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

        public void Upgrade(Move move, UnitBase upgradedUnit)
        {
            if (IsVisible)
            {
                MoveUpdateUnitPart moveUpdateUnitPart = move.Stats.UnitParts[0];
                foreach (UnitBasePart upgradedBasePart in upgradedUnit.UnitBaseParts)
                {
                    if (upgradedBasePart.PartType == moveUpdateUnitPart.PartType)
                    {
                        GameObject upgradedPart = null;
                        if (moveUpdateUnitPart.CompleteLevel > 1)
                        {
                            // Activate the main part
                            GameObject upgradedMainPart;

                            upgradedMainPart = FindChildNyName(upgradedUnit.gameObject, moveUpdateUnitPart.Name + moveUpdateUnitPart.CompleteLevel);
                            if (moveUpdateUnitPart.Level == 1)
                            {
                                upgradedMainPart.SetActive(true);
                                for (int i = 0; i < upgradedMainPart.transform.childCount; i++)
                                {
                                    GameObject child = upgradedMainPart.transform.GetChild(i).gameObject;
                                    child.SetActive(false);
                                }

                            }
                            GameObject upgradedSubPart;
                            for (int level = 1; level <= moveUpdateUnitPart.CompleteLevel; level++)
                            {
                                upgradedSubPart = FindChildNyName(upgradedMainPart, moveUpdateUnitPart.Name + moveUpdateUnitPart.CompleteLevel + "-" + level);
                                if (upgradedSubPart != null)
                                {
                                    if (moveUpdateUnitPart.Level == level)
                                    {
                                        upgradedPart = upgradedSubPart;
                                    }
                                }
                            }
                        }
                        else
                        {
                            upgradedPart = FindChildNyName(upgradedUnit.gameObject, moveUpdateUnitPart.Name + moveUpdateUnitPart.CompleteLevel);
                        }
                        if (upgradedPart != null)
                        {
                            GameObject upgradedPartClone = Instantiate(upgradedPart);

                            TransitObject transitObject = new TransitObject();
                            transitObject.GameObject = upgradedPartClone;
                            transitObject.TargetPosition = upgradedPart.transform.position;
                            transitObject.DestroyAtArrival = true;
                            transitObject.ActivateAtArrival = upgradedPart;

                            // Reset current pos to assembler
                            upgradedPartClone.transform.position = transform.position;
                            upgradedPartClone.transform.rotation = upgradedPart.transform.rotation;
                            upgradedPartClone.SetActive(true);

                            // Move to position in unit
                            HexGrid.MainGrid.AddTransitTileObject(transitObject);
                        }
                    }
                }
            }
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
            Container.Transport(this, move);
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
                            meshRenderer.material = HexGrid.MainGrid.GetMaterial("Outline");
                            meshRenderer.material.SetColor("PlayerColor", GetPlayerColor(playerId));                            
                            meshRenderer.material.SetFloat("Darkness", 0.9f);
                        }
                    }
                    else
                    {
                        if (meshRenderer.sharedMaterials.Length == 1)
                        {
                            meshRenderer.sharedMaterial = HexGrid.MainGrid.GetMaterial("Outline");
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
            if (playerId == 4) ColorUtility.TryParseHtmlString("#1F2ED9", out color);

            return color;
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
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                DeactivateRigidbody(child);
            }

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
                otherRigid.isKinematic = false;

                Vector3 vector3 = new Vector3();
                vector3.y = 12 + Random.value * 3;
                vector3.x = Random.value * 3;
                vector3.z = Random.value * 3;

                otherRigid.velocity = vector3;
                otherRigid.rotation = Random.rotation;
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

                    if (moveUpdateUnitPart != null && moveUpdateUnitPart.CompleteLevel > 1)
                    {
                        hitGameObject = FindChildNyName(unitBasePart.Part, moveUpdateUnitPart.Name + moveUpdateUnitPart.CompleteLevel + "-" + (moveUpdateUnitPart.Level+1));
                    }
                    GroundCell currentCell;

                    if (hitGameObject != null && HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out currentCell))
                    {
                        hitGameObject.SetActive(false);

                        // Clone the part
                        GameObject part = Instantiate(hitGameObject);

                        Destroy(part, 8);

                        unitBasePart.Destroyed = true;
                        SetPlayerColor(0, part);
                        part.transform.SetParent(currentCell.transform, true);

                        ActivateRigidbody(part);
                    }
                    bool alive = false;
                    foreach (UnitBasePart testBasePart in UnitBaseParts)
                    {
                        if (!testBasePart.Destroyed)
                        {
                            alive = true;
                            break;
                        }
                    }

                    if (!alive)
                    {
                        Delete();
                    }
                    else
                    {
                        GameObject smoke = FindChildNyName(gameObject, "SmokeEffect");
                        if (smoke != null)
                            smoke.SetActive(true);
                    }
                    return unitBasePart;
                }
            }
            return null;
        }

        public ParticleSystem TankExplosionParticles;

        public void HitByShell()
        {
            if (TankExplosionParticles != null)
            {
                ParticleSystem particles= Instantiate(TankExplosionParticles, transform);
                particles.Play();
            }
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
        public void Assemble(bool underConstruction, bool ghost)
        {
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
                    unitBasePart.Name = moveUpdateUnitPart.Name;
                    unitBasePart.PartType = moveUpdateUnitPart.PartType;
                    unitBasePart.Part = part;

                    if (moveUpdateUnitPart.TileObjects != null)
                    {
                        unitBasePart.TileObjectContainer = new TileObjectContainer();
                    }
                    UnitBaseParts.Add(unitBasePart);
                }
            }
            if (ghost)
            {
                SetMaterialGhost(PlayerId, gameObject);
                DeactivateRigidbody(gameObject);
            }
            else if (underConstruction)
            {
                SetPlayerColor(PlayerId, gameObject);
            }
            else
            {
                SetPlayerColor(PlayerId, gameObject);
            }
            UpdateParts();
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
                            }
                            else
                            {
                                if (moveUpdateUnitPart.Level == 0)
                                {
                                    unitBasePart.Part.SetActive(false);
                                }
                                missingPartFound = true;
                            }

                            Transform shield = transform.Find("Shield");
                            if (shield != null && IsVisible)
                            {
                                shield.gameObject.SetActive(moveUpdateUnitPart.ShieldActive == true);
                            }

                            if (unitBasePart.PartType == TileObjectType.PartWeapon)
                            {

                            }
                            break;
                        }
                        
                    }
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

                AboveGround = 0.1f;

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

            AboveGround = 0;

        }
    }

}