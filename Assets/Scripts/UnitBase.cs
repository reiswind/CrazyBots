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
        public HitByBullet(Position fireingPosition)
        {
            FireingPosition = fireingPosition;
        }
        public UnitBase TargetUnit { get; set; }
        public TileObjectType HitPartTileObjectType { get; set; }
        public float HitTime { get; set; }
        public TileObject Bullet { get; set; }
        public bool BulletImpact { get; set; }
        public Position FireingPosition { get; set; }
        public Position TargetPosition { get; set; }
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

        public HexGrid HexGrid { get; set; }
        internal Position CurrentPos { get; set; }
        internal Position DestinationPos { get; set; }
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

            if (DestinationPos != null)
            {
                GroundCell targetCell;
                if (HexGrid.GroundCells.TryGetValue(DestinationPos, out targetCell))
                {
                    Vector3 unitPos3 = targetCell.transform.localPosition;
                    unitPos3.y += HexGrid.hexCellHeight + AboveGround;

                    float speed = 1.75f / HexGrid.GameSpeed;
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
            float speed = 3.5f / HexGrid.GameSpeed;

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
            transform.SetParent(HexGrid.transform, false);

            GroundCell targetCell;
            if (HexGrid.GroundCells.TryGetValue(CurrentPos, out targetCell))
            {
                Vector3 unitPos3 = targetCell.transform.localPosition;
                if (!update)
                {
                    unitPos3.y += HexGrid.hexCellHeight + AboveGround;
                    transform.position = unitPos3;
                }
                else
                {
                    unitPos3.y += HexGrid.hexCellHeight + AboveGround;
                    transform.position = unitPos3;
                }
                if (IsVisible = targetCell.Visible)
                {
                    IsVisible = targetCell.Visible;
                    gameObject.SetActive(targetCell.Visible);
                }
            }
        }


        public void MoveTo(Position pos)
        {
            DestinationPos = pos;

            GroundCell targetCell;
            if (HexGrid.GroundCells.TryGetValue(DestinationPos, out targetCell))
            {
                if (Weapon != null)
                {
                    Weapon.TurnTo(HexGrid, DestinationPos);
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

        /*
        private GameObject GetPartByName(string name)
        {
            if (engine != null && engine.name == name)
                return engine.gameObject;
            if (ground != null && ground.name == name)
                return ground.gameObject;
            if (bigPart != null && bigPart.name == name)
                return bigPart.gameObject;
            if (part1 != null && part1.name == name)
                return part1.gameObject;
            if (part2 != null && part2.name == name)
                return part2.gameObject;

            return null;
        }*/

        private void UpdatePart(UnitBasePart unitBasePart, MoveUpdateUnitPart moveUpdateUnitPart, bool underConstruction)
        {
            GameObject oldPart = unitBasePart.Part;
            if (unitBasePart.Level > 0)
            {
                string name = moveUpdateUnitPart.Name + moveUpdateUnitPart.Level;
                GameObject newPart = HexGrid.InstantiatePrefab(name);
                newPart.transform.position = unitBasePart.Part.transform.position;
                newPart.transform.SetParent(transform);
                newPart.name = name;

                SetPlayerColor(HexGrid, PlayerId, newPart);
                unitBasePart.Part = newPart;
            }
            if (oldPart != null)
                Destroy(oldPart);
        }

        private void ReplacePart(Transform part, MoveUpdateUnitPart moveUpdateUnitPart, bool underConstruction)
        {
            // Replace
            string name;
            if (underConstruction && moveUpdateUnitPart.Level == 0)
            {
                name = moveUpdateUnitPart.Name + "1";
            }
            else
            {
                name = moveUpdateUnitPart.Name + moveUpdateUnitPart.Level;
            }
            GameObject newPart = HexGrid.InstantiatePrefab(name);
            newPart.transform.position = part.transform.position;
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
                SetPlayerColor(HexGrid, PlayerId, newPart);
            }
            Destroy(part.gameObject);
            
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
            UnitBasePart unitBasePart = new UnitBasePart(this);
            unitBasePart.Name = moveUpdateUnitPart.Name;
            unitBasePart.PartType = moveUpdateUnitPart.PartType;
            unitBasePart.Part = newPart;
            unitBasePart.Level = moveUpdateUnitPart.Level;
            unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;

            unitBasePart.IsUnderConstruction = underConstruction;

            if (moveUpdateUnitPart.TileObjects != null)
            {
                unitBasePart.TileObjectContainer = new TileObjectContainer();
                /*
                foreach (TileObject tileObject in moveUpdateUnitPart.TileObjects)
                {
                    UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
                    unitBaseTileObject.TileObject = tileObject;
                    unitBasePart.TileObjectContainer.Add(unitBaseTileObject);
                }*/
            }
            else
            {
                if (unitBasePart.TileObjectContainer != null)
                {
                    unitBasePart.TileObjectContainer = null;
                }
            }
            UnitBaseParts.Add(unitBasePart);
        }

        public void Delete()
        {
            HasBeenDestroyed = true;
            Destroy(this.gameObject);
        }
        private Light selectionLight;
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
                        selectionLight = HexGrid.CreateSelectionLight(gameObject);
                    }
                    else
                    {
                        Destroy(selectionLight);
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
                        GroundCell targetCell = HexGrid.GroundCells[unitCommand.GameCommand.TargetPosition];

                        GameObject waypointPrefab = HexGrid.GetResource("Waypoint");

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
                SetPlayerColor(HexGrid, PlayerId, unitBasePart.Part);
            }
        }

        public void Extract(Move move, UnitBase unit, UnitBase otherUnit)
        {
            if (IsVisible && Extractor != null)
            {
                Extractor.Extract(HexGrid, move, unit, otherUnit);
            }
        }

        public void Upgrade(Move move, UnitBase upgradedUnit)
        {
            if (IsVisible && Assembler != null)
            {
                Assembler.Assemble(HexGrid, this, upgradedUnit, move);
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
                Container.Transport(HexGrid, move);
            }
        }

        internal void SetMaterialGhost(int playerId, GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Ammo") && !child.name.StartsWith("Item"))
                    SetMaterialGhost(playerId, child);
            }

            MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (meshRenderer.materials.Length == 1)
                {
                    Destroy(meshRenderer.material);                   
                    meshRenderer.material = HexGrid.GetMaterial("PlayerTransparent");
                }
                else
                {
                    Material[] newMaterials = new Material[meshRenderer.materials.Length];
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                    {
                        Material material = meshRenderer.materials[i];
                        if (material.name.StartsWith("Player"))
                        {
                            Destroy(material);
                            newMaterials[i] = HexGrid.GetMaterial("PlayerTransparent");
                        }
                        else
                        {
                            Destroy(material);
                            newMaterials[i] = HexGrid.GetMaterial("TransparentFrame");
                        }
                    }
                    meshRenderer.materials = newMaterials;
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

        internal static void SetPlayerColor(HexGrid hexGrid, int playerId, GameObject unit)
        {
            for (int i = 0; i < unit.transform.childCount; i++)
            {
                GameObject child = unit.transform.GetChild(i).gameObject;
                if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Ammo") && !child.name.StartsWith("Item"))
                    SetPlayerColor(hexGrid, playerId, child);
            }

            MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (meshRenderer.materials.Length == 1)
                {
                    Destroy(meshRenderer.material);
                    if (playerId == 0)
                        meshRenderer.material = hexGrid.GetMaterial("Player0");
                    if (playerId == 0) meshRenderer.material = hexGrid.GetMaterial("Player0");
                    if (playerId == 1) meshRenderer.material = hexGrid.GetMaterial("Player1");
                    if (playerId == 2) meshRenderer.material = hexGrid.GetMaterial("Player2");
                    if (playerId == 3) meshRenderer.material = hexGrid.GetMaterial("Player3");
                    if (playerId == 4) meshRenderer.material = hexGrid.GetMaterial("Player4");
                }
                else
                {
                    Material[] newMaterials = new Material[meshRenderer.materials.Length];
                    for (int i = 0; i < meshRenderer.materials.Length; i++)
                    {
                        Material material = meshRenderer.materials[i];
                        if (material.name.StartsWith("Player"))
                        {
                            Destroy(material);
                            if (playerId == 0) newMaterials[i] = hexGrid.GetMaterial("Player0");
                            if (playerId == 1) newMaterials[i] = hexGrid.GetMaterial("Player1");
                            if (playerId == 2) newMaterials[i] = hexGrid.GetMaterial("Player2");
                            if (playerId == 3) newMaterials[i] = hexGrid.GetMaterial("Player3");
                            if (playerId == 4) newMaterials[i] = hexGrid.GetMaterial("Player4");
                        }
                        else
                        {
                            Destroy(material);
                            newMaterials[i] = hexGrid.GetMaterial("MyFrame");
                        }
                    }
                    meshRenderer.materials = newMaterials;
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

            string name;
            if (moveUpdateUnitPart.Level == 0)
            {
                name = moveUpdateUnitPart.Name + "1";
            }
            else
            {
                name = moveUpdateUnitPart.Name + moveUpdateUnitPart.Level;
            }

            // Replace
            GameObject newPart = HexGrid.InstantiatePrefab(name);
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
                SetPlayerColor(HexGrid, PlayerId, newPart);
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
            unitBasePart.Part = gameObject;
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
                        SetPlayerColor(HexGrid, 0, unitBasePart.Part);
                        Destroy(unitBasePart.Part, 8);
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
                    if (unitBasePart.Level > 1)
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

                        if (HexGrid.GroundCells.TryGetValue(CurrentPos, out currentCell))
                        {
                            unitBasePart.Part.transform.SetParent(currentCell.transform, true);

                            Rigidbody otherRigid = unitBasePart.Part.GetComponent<Rigidbody>();

                            if (otherRigid != null)
                            {
                                otherRigid.isKinematic = false;

                                Vector3 vector3 = new Vector3();
                                vector3.y = 15;
                                //vector3.x = Random.value;
                                //vector3.z = Random.value;

                                otherRigid.velocity = vector3;
                                //otherRigid.rotation = Random.rotation;
                            }
                        }

                        /*
                        Container1 container = unitBasePart.Part.GetComponent<Container1>();
                        if (container != null)
                        {
                            List<TileObject> tileObjects = new List<TileObject>();
                            container.UpdateContent(HexGrid, tileObjects, 1);
                        }*/

                        unitBasePart.Destroyed = true;
                        SetPlayerColor(HexGrid, 0, unitBasePart.Part);
                        Destroy(unitBasePart.Part, 8);
                        UnitBaseParts.Remove(unitBasePart);

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
            if (IsBuilding())
            {
                AssembleBuilding();
            }

            Transform engine;
            Transform ground;
            Transform bigPart;
            Transform part1;
            Transform part2;

            engine = transform.Find("Engine");
            ground = transform.Find("Ground");
            bigPart = transform.Find("BigPart");
            part1 = transform.Find("Part1");
            part2 = transform.Find("Part2");

            Transform sparePart = part1;

            List<MoveUpdateUnitPart> remainingParts = new List<MoveUpdateUnitPart>();
            if (MoveUpdateStats.UnitParts != null)
                remainingParts.AddRange(MoveUpdateStats.UnitParts);

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
                    ReplacePart(ground, moveUpdateUnitPart, underConstruction);
                    ground.name = moveUpdateUnitPart.Name;

                    remainingParts.Remove(moveUpdateUnitPart);
                    groundFound = true;
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
                        ReplacePart(bigPart, moveUpdateUnitPart, underConstruction);
                        remainingParts.Remove(moveUpdateUnitPart);
                        break;
                    }
                }
            }
            if (sparePart != null)
            {
                // Place remaining parts
                foreach (MoveUpdateUnitPart moveUpdateUnitPart in remainingParts)
                {
                    ReplacePart(sparePart, moveUpdateUnitPart, underConstruction);
                    sparePart = part2;
                    if (sparePart == null)
                        break;
                }
            }
            if (sparePart != null)
            {
                sparePart.gameObject.SetActive(false);
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
                        //if (unitBasePart.Name == moveUpdateUnitPart.Name)
                        if (unitBasePart.PartType == moveUpdateUnitPart.PartType)
                        {
                            if (unitBasePart.IsUnderConstruction && moveUpdateUnitPart.Exists)
                            {
                                // Change from transparent to reals
                                unitBasePart.IsUnderConstruction = false;
                                SetPlayerColor(HexGrid, PlayerId, unitBasePart.Part);
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
                                UpdatePart(unitBasePart, moveUpdateUnitPart, false);

                                // Replace the tile container
                                if (moveUpdateUnitPart.TileObjects != null)
                                {
                                    unitBasePart.TileObjectContainer = new TileObjectContainer();
                                }
                            }
                            
                            if (unitBasePart.Level < moveUpdateUnitPart.Level)
                            {
                                unitBasePart.Level = moveUpdateUnitPart.Level;
                            }
                            
                            if (!moveUpdateUnitPart.Exists)
                                missingPartFound = true;

                            Engine1 engine = unitBasePart.Part.GetComponent<Engine1>();
                            if (engine != null)
                            {
                                Engine = engine;
                            }
                            Container1 container = unitBasePart.Part.GetComponent<Container1>();
                            if (container != null)
                            {
                                Container = container;
                                unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Extractor1 extractor = unitBasePart.Part.GetComponent<Extractor1>();
                            if (extractor != null)
                                Extractor = extractor;
                            Assembler1 assembler = unitBasePart.Part.GetComponent<Assembler1>();
                            if (assembler != null)
                            {
                                Assembler = assembler;
                                unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Weapon1 weapon = unitBasePart.Part.GetComponent<Weapon1>();
                            if (weapon != null)
                            {
                                Weapon = weapon;
                                unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                                if (moveUpdateUnitPart.TileObjects != null)
                                {
                                    weapon.UpdateContent(HexGrid, unitBasePart.TileObjectContainer);
                                }
                            }
                            Reactor1 reactor = unitBasePart.Part.GetComponent<Reactor1>();
                            if (reactor != null)
                            {
                                Reactor = reactor;
                                unitBasePart.UpdateContent(moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Armor armor = unitBasePart.Part.GetComponent<Armor>();
                            if (armor != null)
                            {
                                Armor = armor;
                                Transform shield = transform.Find("Shield");
                                if (shield != null && IsVisible)
                                {
                                    shield.gameObject.SetActive(moveUpdateUnitPart.ShieldActive == true);
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
            if (Engine != null)
            {
                GameObject activeAnimation = FindChildNyName(gameObject, "ActiveAnimation");
                if (activeAnimation != null)
                {
                    activeAnimation.SetActive(true);
                }
                AboveGround = 0.1f;
            }
            IsActive = true;
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

                Vector3 unitPos3 = transform.position;
                unitPos3.y -= AboveGround;
                transform.position = unitPos3;

                AboveGround = 0;
            }
        }
    }

}