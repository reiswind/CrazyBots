using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitCommand
    {
        public GameCommand GameCommand { get; set; }
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


        // Update is called once per frame
        void Update()
        {
            if (selectionChanged)
            {
                UpdateWayPoints();
                selectionChanged = false;
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
                    UpdateDirection(unitPos3);

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
                if (Weapon != null)
                {
                    Weapon.TurnTo(DestinationPos);
                }
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

        private void UpdatePart(UnitBasePart unitBasePart, MoveUpdateUnitPart moveUpdateUnitPart)
        {
            GameObject oldPart = unitBasePart.Part1;
            if (unitBasePart.Level > 0)
            {
                string name = GetPrefabName(moveUpdateUnitPart);

                GameObject newPart = HexGrid.MainGrid.InstantiatePrefab(name);
                newPart.transform.position = unitBasePart.Part1.transform.position;
                newPart.transform.SetParent(transform);
                newPart.name = name;

                SetPlayerColor(PlayerId, newPart);
                unitBasePart.Part1 = newPart;
                if (moveUpdateUnitPart.TileObjects != null)
                {
                    unitBasePart.TileObjectContainer = new TileObjectContainer();
                }
            }
            else
            {
                unitBasePart.Part1 = null;
            }
            if (oldPart != null)
            {
                Destroy(oldPart);
            }
        }

        private string GetPrefabName(MoveUpdateUnitPart moveUpdateUnitPart, int? level = null)
        {
            string name;

            if (!level.HasValue)
                level = moveUpdateUnitPart.Level;
            if (level == 0) 
                level = 1;

            if (moveUpdateUnitPart.CompleteLevel == 91)
            {
                name = moveUpdateUnitPart.Name + level;
            }
            else
            {
                name = moveUpdateUnitPart.Name + moveUpdateUnitPart.CompleteLevel + "-" + level;
            }
            return name;
        }

        private GameObject CreatePartGameObject(MoveUpdateUnitPart moveUpdateUnitPart, bool underConstruction, int? level = null)
        {
            string name = GetPrefabName(moveUpdateUnitPart, level);
            GameObject newPart;
            newPart = HexGrid.MainGrid.InstantiatePrefab(name);
            newPart.transform.SetParent(transform);
            newPart.name = name;

            if (IsGhost)
            {
                SetMaterialGhost(PlayerId, newPart);
                RemoveColider(newPart);
            }
            else if (underConstruction)
            {
                newPart.SetActive(false);
                RemoveColider(newPart);
            }
            else
            {
                SetPlayerColor(PlayerId, newPart);
            }
            DeactivateRigidbody(newPart);
            return newPart;
        }

        private UnitBasePart ReplacePart(Transform part, MoveUpdateUnitPart moveUpdateUnitPart, bool underConstruction)
        {
            UnitBasePart unitBasePart = null;
            if (moveUpdateUnitPart.Level == 0)
            {
                // Hide the template
                part.gameObject.SetActive(false);

                unitBasePart = new UnitBasePart(this);
                unitBasePart.Name = moveUpdateUnitPart.Name;
                unitBasePart.PartType = moveUpdateUnitPart.PartType;
                unitBasePart.Part1 = part.gameObject;
                unitBasePart.Level = moveUpdateUnitPart.Level;
                unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;
                unitBasePart.IsUnderConstruction = underConstruction;

                if (moveUpdateUnitPart.TileObjects != null)
                {
                    unitBasePart.TileObjectContainer = new TileObjectContainer();
                    unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                }
                UnitBaseParts.Add(unitBasePart);
            }
            else
            {
                foreach (UnitBasePart existingPart in UnitBaseParts)
                {
                    if (existingPart.PartType == moveUpdateUnitPart.PartType)
                    {
                        unitBasePart = existingPart;
                        break;
                    }
                }
                if (unitBasePart == null)
                {
                    unitBasePart = new UnitBasePart(this);
                    unitBasePart.Name = moveUpdateUnitPart.Name;
                    unitBasePart.PartType = moveUpdateUnitPart.PartType;
                    unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;

                    if (unitBasePart.CompleteLevel >= 1)
                    {
                        unitBasePart.Part1 = CreatePartGameObject(moveUpdateUnitPart, underConstruction, 1);
                        unitBasePart.Part1.transform.position = part.transform.position;
                    }
                    if (unitBasePart.CompleteLevel >= 2)
                    {
                        unitBasePart.Part2 = CreatePartGameObject(moveUpdateUnitPart, underConstruction, 2);
                        unitBasePart.Part2.transform.position = part.transform.position;
                    }
                    if (unitBasePart.CompleteLevel >= 3)
                    {
                        unitBasePart.Part3 = CreatePartGameObject(moveUpdateUnitPart, underConstruction, 3);
                        unitBasePart.Part3.transform.position = part.transform.position;
                    }

                    // Destroy the placeholder
                    if (UnityEditor.EditorApplication.isPlaying)
                        Destroy(part.gameObject);
                    else
                        DestroyImmediate(part.gameObject);

                    UnitBaseParts.Add(unitBasePart);
                }
                else
                {
                    GameObject newPart;

                    newPart = CreatePartGameObject(moveUpdateUnitPart, underConstruction);
                    newPart.transform.position = part.transform.position;
                    newPart.transform.rotation = part.transform.rotation;
                    
                    unitBasePart.Level = moveUpdateUnitPart.Level;
                    unitBasePart.IsUnderConstruction = underConstruction;

                    if (unitBasePart.Level == 1)
                    {
                        // Destroy the template, replace with final part
                        if (unitBasePart.Part1 != null)
                        {
                            if (UnityEditor.EditorApplication.isPlaying)
                                Destroy(unitBasePart.Part1.gameObject);
                            else
                                DestroyImmediate(unitBasePart.Part1.gameObject);
                        }
                        unitBasePart.Part1 = newPart;
                    }
                    if (unitBasePart.Level == 2)
                    {
                        if (unitBasePart.Part2 != null)
                        {
                            if (UnityEditor.EditorApplication.isPlaying)
                                Destroy(unitBasePart.Part2.gameObject);
                            else
                                DestroyImmediate(unitBasePart.Part2.gameObject);
                        }
                        unitBasePart.Part2 = newPart;
                    }
                    if (unitBasePart.Level == 3)
                    {
                        if (unitBasePart.Part3 != null)
                        {
                            if (UnityEditor.EditorApplication.isPlaying)
                                Destroy(unitBasePart.Part3.gameObject);
                            else
                                DestroyImmediate(unitBasePart.Part3.gameObject);
                        }
                        unitBasePart.Part3 = newPart;
                    }

                }
                if (moveUpdateUnitPart.TileObjects != null)
                {
                    unitBasePart.TileObjectContainer = new TileObjectContainer();
                    unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                }
                else
                {
                    if (moveUpdateUnitPart.TileObjects == null && unitBasePart.TileObjectContainer != null)
                    {
                        unitBasePart.TileObjectContainer = null;
                    }
                }
            }
            return unitBasePart;
        }

        public void Delete()
        {
            HasBeenDestroyed = true;
            Destroy(this.gameObject);
        }
        //private Light selectionLight;
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
                SetPlayerColor(PlayerId, unitBasePart.Part1);
            }
        }

        public void Extract(Move move, UnitBase unit, UnitBase otherUnit)
        {
            if (IsVisible && Extractor != null)
            {
                Extractor.Extract(move, unit, otherUnit);
            }
        }

        public void Upgrade(Move move, UnitBase upgradedUnit)
        {
            if (IsVisible && Assembler != null)
            {
                MoveUpdateUnitPart moveUpdateUnitPart = move.Stats.UnitParts[0];
                foreach (UnitBasePart upgradedBasePart in upgradedUnit.UnitBaseParts)
                {
                    if (upgradedBasePart.PartType == moveUpdateUnitPart.PartType)
                    {
                        Vector3 vector3 = upgradedBasePart.Part1.transform.position;
                        Quaternion rotation = upgradedBasePart.Part1.transform.rotation;
                        UnitBasePart lastPart = upgradedUnit.ReplacePart(upgradedBasePart.Part1.transform, moveUpdateUnitPart, false);

                        GameObject part;
                        if (lastPart.Part3 != null)
                            part = lastPart.Part3;
                        else if (lastPart.Part2 != null)
                            part = lastPart.Part2;
                        else
                            part = lastPart.Part1;

                        TransitObject transitObject = new TransitObject();
                        transitObject.GameObject = part;
                        transitObject.TargetPosition = vector3;
                        transitObject.TargetRotation = rotation;

                        // Reset current pos to assembler
                        part.transform.position = transform.position;
                        part.SetActive(true);

                        // Move to position in unit
                        HexGrid.MainGrid.AddTransitTileObject(transitObject);
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
                        unitBasePart.Fire(move, Weapon);
                    }
                }
            }
            /*

            if (Weapon != null)
            {
                Weapon.Fire(HexGrid, move);
            }*/
        }

        public void Transport(Move move)
        {
            if (Container != null)
            {
                Container.Transport(move);
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
                    if (UnityEditor.EditorApplication.isPlaying)
                    {
                        if (meshRenderer.materials.Length == 1)
                        {
                            Destroy(meshRenderer.material);
                            meshRenderer.material = HexGrid.MainGrid.GetMaterial("ghost 1");
                        }
                    }
                    else
                    {
                        if (meshRenderer.sharedMaterials.Length == 1)
                        {
                            meshRenderer.sharedMaterial = HexGrid.MainGrid.GetMaterial("ghost 1");
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
            if (playerId == 1) ColorUtility.TryParseHtmlString("#FFA200", out color);
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
                if (UnityEditor.EditorApplication.isPlaying)
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
                        meshRenderer.sharedMaterial.SetColor("PlayerColor", GetPlayerColor(playerId));
                        meshRenderer.sharedMaterial.SetFloat("Darkness", 0.9f);
                    }
                }
            }
        }

        public bool IsAssembler()
        {
            if (UnderConstruction) return false;
            return Assembler != null && Engine == null;
        }
        public bool IsContainer()
        {
            if (UnderConstruction) return false;
            return Container != null && Engine == null;
        }

        private Engine1 Engine;
        private Assembler1 Assembler;
        internal Container1 Container;
        private Extractor1 Extractor;
        private Weapon1 Weapon;
        private Reactor1 Reactor;
        private Armor Armor;

        private GameObject AddPart(GameObject foundation, GameObject parent, MoveUpdateUnitPart moveUpdateUnitPart, bool underConstruction)
        {
            float y = 0.01f;
            if (parent != null)
            {
                Renderer rend = parent.GetComponent<Renderer>();
                y += parent.transform.position.y + rend.bounds.size.y; // + 0.1f;
            }

            string name = GetPrefabName(moveUpdateUnitPart);

            // Replace
            GameObject newPart = HexGrid.MainGrid.InstantiatePrefab(name);
            Vector3 vector3 = transform.position;
            vector3.y = y;
            newPart.transform.position = vector3;
            newPart.transform.SetParent(transform);
            newPart.name = name;

            if (underConstruction)
            {
                if (IsGhost)
                    SetMaterialGhost(PlayerId, newPart);
                else
                    newPart.SetActive(false);
            }
            else
            {
                SetPlayerColor(PlayerId, newPart);
            }
            if (foundation != null)
            {
                /*
                SpringJoint springJoint = newPart.AddComponent<SpringJoint>();
                Rigidbody rigidbodyx = foundation.GetComponent<Rigidbody>();
                springJoint.connectedBody = rigidbodyx;
                springJoint.maxDistance = 2;
                //springJoint.minDistance = 0.5f;
                springJoint.enableCollision = true;
                springJoint.breakForce = 5000f;
                //springJoint.spring = 5000;
                //springJoint.breakTorque = 0.0001f;,0
                springJoint.autoConfigureConnectedAnchor = true;*/
            }

            Rigidbody rigidbody = newPart.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                if (underConstruction)
                {
                    rigidbody.Sleep();
                }
                else
                {
                    rigidbody.WakeUp();
                }
            }

            return newPart;
        }

        public void AssembleBuilding()
        {
            if (MoveUpdateStats.UnitParts == null)
                return;

            GameObject lastObject = null;
            GameObject foundation = null;

            foreach (MoveUpdateUnitPart moveUpdateUnitPart in MoveUpdateStats.UnitParts)
            {
                if (moveUpdateUnitPart.PartType == TileObjectType.PartExtractor)
                {
                    lastObject = AddPart(foundation, lastObject, moveUpdateUnitPart, UnderConstruction);
                    foundation = lastObject;
                    AddBasePart(moveUpdateUnitPart, lastObject);
                }
                else //if (moveUpdateUnitPart.PartType == TileObjectType.PartContainer)
                {
                    lastObject = AddPart(foundation, lastObject, moveUpdateUnitPart, UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                    /*
                    if (moveUpdateUnitPart.Level > 1)
                    {
                        lastObject = AddPart(foundation, lastObject, "ContainerPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);
                    }
                    if (moveUpdateUnitPart.Level > 2)
                    {
                        lastObject = AddPart(foundation, lastObject, "ContainerPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);
                    }*/
                }
                /*
                else if (moveUpdateUnitPart.PartType == TileObjectType.PartReactor)
                {

                    lastObject = AddPart(foundation, lastObject, moveUpdateUnitPart, UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                    
                    if (moveUpdateUnitPart.Level == 2)
                    {
                        lastObject = AddPart(foundation, lastObject, "ReactorPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);
                    }
                    if (moveUpdateUnitPart.Level == 3)
                    {
                        lastObject = AddPart(foundation, lastObject, "ReactorPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);

                        lastObject = AddPart(foundation, lastObject, "ReactorPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);

                        lastObject = AddPart(foundation, lastObject, "ReactorPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);
                    }
                    
                }
                else if (moveUpdateUnitPart.PartType == TileObjectType.PartAssembler)
                {
                    lastObject = AddPart(foundation, lastObject, moveUpdateUnitPart, UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                    /*
                    if (moveUpdateUnitPart.Level == 2)
                    {
                        lastObject = AddPart(foundation, lastObject, "AssemblerPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);

                        lastObject = AddPart(foundation, lastObject, "Socket1", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);
                    }
                    if (moveUpdateUnitPart.Level == 3)
                    {
                        lastObject = AddPart(foundation, lastObject, "AssemblerPart", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);

                        lastObject = AddPart(foundation, lastObject, "Socket1", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);

                        lastObject = AddPart(foundation, lastObject, "Socket1", UnderConstruction);
                        AddBasePart(moveUpdateUnitPart, lastObject);

                    }
                    */
                

            }
            UpdateParts();
        }

        private void AddBasePart(MoveUpdateUnitPart moveUpdateUnitPart, GameObject gameObject)
        {
            UnitBasePart unitBasePart = new UnitBasePart(this);
            unitBasePart.Name = moveUpdateUnitPart.Name;
            unitBasePart.PartType = moveUpdateUnitPart.PartType;
            unitBasePart.Part1 = gameObject;
            unitBasePart.Level = moveUpdateUnitPart.Level;
            unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;

            unitBasePart.IsUnderConstruction = UnderConstruction;
            UnitBaseParts.Add(unitBasePart);
        }

        public void PartExtracted(TileObjectType hitPart)
        {
            for (int i = UnitBaseParts.Count - 1; i >= 0; i--)
            {
                UnitBasePart unitBasePart = UnitBaseParts[i];
                if (unitBasePart.PartType == hitPart)
                {
                    unitBasePart.Level--;
                    if (unitBasePart.Level == 0)
                    {
                        /*
                        GroundCell currentCell = HexGrid.GroundCells[CurrentPos];
                        unitBasePart.Part.transform.SetParent(currentCell.transform, true);

                        Rigidbody otherRigid = unitBasePart.Part.GetComponent<Rigidbody>();

                        if (otherRigid != null)
                        {
                            otherRigid.isKinematic = false;

                            Vector3 vector3 = new Vector3();
                            vector3.y = 5;
                            vector3.x = Random.value;
                            vector3.z = Random.value;

                            otherRigid.velocity = vector3;
                            otherRigid.rotation = Random.rotation;
                        }

                        Container1 container = unitBasePart.Part.GetComponent<Container1>();
                        if (container != null)
                        {
                            List<TileObject> tileObjects = new List<TileObject>();
                            container.UpdateContent(HexGrid, tileObjects, 1);
                        }*/

                        unitBasePart.Destroyed = true;
                        SetPlayerColor(0, unitBasePart.Part1);
                        Destroy(unitBasePart.Part1, 8);
                        UnitBaseParts.Remove(unitBasePart);
                    }
                    break;
                }
            }
        }

        public UnitBasePart PartHitByShell(TileObjectType hitPart, MoveUpdateStats stats)
        {
            foreach (UnitBasePart unitBasePart in UnitBaseParts)
            {                
                if (unitBasePart.PartType == hitPart)
                {
                    if (unitBasePart.Level > 991)
                    {
                        UpdateStats(stats);
                    }
                    else
                    {
                        if (unitBasePart.TileObjectContainer != null)
                        {
                            unitBasePart.TileObjectContainer.Explode(transform);
                            List<TileObject> tileObjects = new List<TileObject>();
                            unitBasePart.UpdateContent(tileObjects, 1);
                        }
                        unitBasePart.Level = 0;
                        GroundCell currentCell;

                        if (HexGrid.MainGrid.GroundCells.TryGetValue(CurrentPos, out currentCell))
                        {
                            GameObject part;

                            if (unitBasePart.Part3 != null)
                            {
                                part = unitBasePart.Part3;
                                unitBasePart.Part3 = null;
                            }
                            else if (unitBasePart.Part2 != null)
                            {
                                part = unitBasePart.Part2;
                                unitBasePart.Part2 = null;
                            }
                            else
                            {
                                part = unitBasePart.Part1;
                                Destroy(unitBasePart.Part1, 8);
                                unitBasePart.Part1 = null;
                                unitBasePart.Destroyed = true;
                                UnitBaseParts.Remove(unitBasePart);
                            }

                            SetPlayerColor(0, part);
                            part.transform.SetParent(currentCell.transform, true);

                            ActivateRigidbody(part);
                        }

                        /*
                        Container1 container = unitBasePart.Part.GetComponent<Container1>();
                        if (container != null)
                        {
                            List<TileObject> tileObjects = new List<TileObject>();
                            container.UpdateContent(HexGrid, tileObjects, 1);
                        }*/


                        if (unitBasePart.PartType == TileObjectType.PartEngine) Engine = null;
                        if (unitBasePart.PartType == TileObjectType.PartAssembler) Assembler = null;
                        if (unitBasePart.PartType == TileObjectType.PartContainer) Container = null;
                        if (unitBasePart.PartType == TileObjectType.PartExtractor) Extractor = null;
                        if (unitBasePart.PartType == TileObjectType.PartWeapon) Weapon = null;
                        if (unitBasePart.PartType == TileObjectType.PartReactor) Reactor = null;
                        if (unitBasePart.PartType == TileObjectType.PartArmor) Armor = null;

                        if (UnitBaseParts.Count == 0)
                        {
                            Delete();
                        }
                        else
                        {
                            GameObject smoke = FindChildNyName(gameObject, "SmokeEffect");
                            if (smoke != null)
                                smoke.SetActive(true);
                        }
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
            /*
            ParticleSystem particleSource;

            particleSource = HexGrid.MakeParticleSource("TankExplosion");
            particleSource.transform.SetParent(transform, false);
            particleSource.Play();
            HexGrid.Destroy(particleSource, 2f);*/
        }
        /*
        public void HitByShell(Collision collision)
        {
            string hitPart = GetPartThatHasBeenHit();
            if (hitPart == null)
                return;

            for (int i=unitBaseParts.Count-1; i >= 0; i--)
            {
                UnitBasePart unitBasePart = unitBaseParts[i];
            //foreach (UnitBasePart unitBasePart in unitBaseParts)
            //{
                if (unitBasePart.PartType == hitPart)
                {

                    Rigidbody otherRigid = unitBasePart.Part.GetComponent<Rigidbody>();

                    if (otherRigid != null)
                    {
                        otherRigid.isKinematic = false;


                        otherRigid.rotation = Random.rotation;
                        otherRigid.velocity = collision.relativeVelocity;
                    }

                    ParticleSystem particleSource;

                    particleSource = HexGrid.MakeParticleSource("TankExplosion");
                    particleSource.transform.SetParent(unitBasePart.Part.transform, false);
                    particleSource.Play();
                    HexGrid.Destroy(particleSource, 2f);

                    unitBasePart.Destroyed = true;
                    SetPlayerColor(HexGrid, 0, unitBasePart.Part);                
                    Destroy(unitBasePart.Part, 3);
                    unitBaseParts.Remove(unitBasePart);
                    break;
                }
            }
        }*/


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

            Transform engine;
            Transform ground;
            Transform bigPart;
            Transform part1;
            Transform part2;

            bool isBuilding = IsBuilding();
            if (isBuilding)
            {
                ground = transform.Find("Extractor");
                engine = null;
                bigPart = transform.Find("Socket1");
                part1 = transform.Find("Socket2");
                part2 = transform.Find("Socket3");
            }
            else
            {
                engine = transform.Find("Engine");
                ground = transform.Find("Ground");
                bigPart = transform.Find("BigPart");
                part1 = transform.Find("Part1");
                part2 = transform.Find("Part2");
            }
            Transform sparePart = part1;

            List<MoveUpdateUnitPart> remainingParts = new List<MoveUpdateUnitPart>();
            if (MoveUpdateStats.UnitParts != null)
                remainingParts.AddRange(MoveUpdateStats.UnitParts);

            UnitBasePart addedBasePart;

            // Find the basic parts
            bool groundFound = false;
            foreach (MoveUpdateUnitPart moveUpdateUnitPart in remainingParts)
            {
                if (engine != null && moveUpdateUnitPart.PartType == TileObjectType.PartEngine)
                {
                    ReplacePart(engine, moveUpdateUnitPart, underConstruction);
                    remainingParts.Remove(moveUpdateUnitPart);
                    groundFound = true;
                    break;
                }
                else if (ground != null && moveUpdateUnitPart.PartType == TileObjectType.PartExtractor)
                {
                    addedBasePart = ReplacePart(ground, moveUpdateUnitPart, underConstruction);
                    remainingParts.Remove(moveUpdateUnitPart);
                    groundFound = true;

                    if (isBuilding && addedBasePart.Part1 != null)
                    {
                        Renderer rend = addedBasePart.Part1.GetComponent<Renderer>();

                        Vector3 vector3 = bigPart.transform.position;
                        vector3.y = addedBasePart.Part1.transform.position.y + rend.bounds.size.y; // + 0.1f;
                        bigPart.transform.position = vector3;
                    }
                    
                    break;
                }
            }
            if (groundFound == false)
            {
                if (engine != null)
                    engine.gameObject.SetActive(false);
                if (ground != null)
                    ground.gameObject.SetActive(false);
            }

            // Place big parts
            if (bigPart != null)
            {
                foreach (MoveUpdateUnitPart moveUpdateUnitPart in remainingParts)
                {
                    if (moveUpdateUnitPart.PartType == TileObjectType.PartContainer ||
                        moveUpdateUnitPart.PartType == TileObjectType.PartWeapon ||
                        moveUpdateUnitPart.PartType == TileObjectType.PartReactor ||
                        moveUpdateUnitPart.PartType == TileObjectType.PartAssembler)
                    {
                        addedBasePart = ReplacePart(bigPart, moveUpdateUnitPart, underConstruction);
                        remainingParts.Remove(moveUpdateUnitPart);

                        if (isBuilding && addedBasePart.Part1 != null)
                        {
                            Renderer rend = addedBasePart.Part1.GetComponent<Renderer>();

                            Vector3 vector3 = sparePart.transform.position;
                            vector3.y = addedBasePart.Part1.transform.position.y + rend.bounds.size.y;
                            sparePart.transform.position = vector3;
                        }

                        break;
                    }
                }
            }
            if (sparePart != null)
            {
                // Place remaining parts
                foreach (MoveUpdateUnitPart moveUpdateUnitPart in remainingParts)
                {
                    addedBasePart = ReplacePart(sparePart, moveUpdateUnitPart, underConstruction);
                    sparePart = part2;
                    if (sparePart == null)
                    {
                        break;
                    }
                    if (isBuilding && addedBasePart.Part1 != null)
                    {
                        Renderer rend = addedBasePart.Part1.GetComponent<Renderer>();

                        Vector3 vector3 = part2.transform.position;
                        vector3.y = addedBasePart.Part1.transform.position.y + rend.bounds.size.y;
                        part2.transform.position = vector3;
                    }
                }
            }
            if (sparePart != null)
            {
                sparePart.gameObject.SetActive(false);
                if (sparePart != part2)
                    part2.gameObject.SetActive(false);
            }
            UpdateParts();
        }

        public void UpdateParts()
        {
            Container = null;
            Extractor = null;
            Assembler = null;
            Weapon = null;
            Reactor = null;
            Engine = null;
            Armor = null;

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
                                SetPlayerColor(PlayerId, unitBasePart.Part1);
                            }
                            if (unitBasePart.TileObjectContainer == null)
                                unitBasePart.TileObjectContainer = new TileObjectContainer();

                            if (unitBasePart.Level > 0 &&
                                unitBasePart.Level != moveUpdateUnitPart.Level )
                            {
                                if (moveUpdateUnitPart.Capacity.HasValue)
                                {
                                    unitBasePart.TileObjectContainer.ExplodeExceedingCapacity(transform, moveUpdateUnitPart.Capacity.Value);
                                }
                                unitBasePart.Level = moveUpdateUnitPart.Level;
                                UpdatePart(unitBasePart, moveUpdateUnitPart);

                                // Modifies the UnitBaseParts array
                                //ReplacePart(unitBasePart.Part.transform, moveUpdateUnitPart, false);

                            }

                            if (unitBasePart.Level < moveUpdateUnitPart.Level)
                            {
                                unitBasePart.Level = moveUpdateUnitPart.Level;
                            }
                            
                            if (!moveUpdateUnitPart.Exists)
                                missingPartFound = true;

                            if (unitBasePart.Part1 == null)
                            {

                            }
                            else
                            {
                                Engine1 engine = unitBasePart.Part1.GetComponent<Engine1>();
                                if (engine != null)
                                {
                                    Engine = engine;
                                }
                                Container1 container = unitBasePart.Part1.GetComponent<Container1>();
                                if (container != null)
                                {
                                    Container = container;
                                    unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                                }
                                Extractor1 extractor = unitBasePart.Part1.GetComponent<Extractor1>();
                                if (extractor != null)
                                    Extractor = extractor;
                                Assembler1 assembler = unitBasePart.Part1.GetComponent<Assembler1>();
                                if (assembler != null)
                                {
                                    Assembler = assembler;
                                    unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                                }
                                Weapon1 weapon = unitBasePart.Part1.GetComponent<Weapon1>();
                                if (weapon != null)
                                {
                                    Weapon = weapon;
                                    unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                                    if (moveUpdateUnitPart.TileObjects != null)
                                    {
                                        weapon.UpdateContent(gameObject, unitBasePart.TileObjectContainer);
                                    }
                                }
                                Reactor1 reactor = unitBasePart.Part1.GetComponent<Reactor1>();
                                if (reactor != null)
                                {
                                    Reactor = reactor;
                                    unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                                }
                                Armor armor = unitBasePart.Part1.GetComponent<Armor>();
                                if (armor != null)
                                {
                                    Armor = armor;
                                    Transform shield = transform.Find("Shield");
                                    if (shield != null && IsVisible)
                                    {
                                        shield.gameObject.SetActive(moveUpdateUnitPart.ShieldActive == true);
                                    }
                                }
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
                if (Engine != null)
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
                }
                IsActive = true;
            }
        }

        public void DectivateUnit()
        {
            IsActive = false;
            if (Engine != null)
            {
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

}