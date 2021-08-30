using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCommand
{
    public GameCommand GameCommand { get; set; }
    public GameObject GameObject { get; set; }
    public GroundCell TargetCell { get; set; }
    public UnitBase Owner { get; set; }
}
public class UnitBaseTileObject
{
    //public GameObject GameObject { get; set; }
    public TileObject TileObject { get; set; }
}
public class TransitObject
{
    public GameObject GameObject { get; set; }
    public Vector3 TargetPosition { get; set; }
    public Quaternion TargetRotation { get; set; }
    public bool DestroyAtArrival { get; set; }
}

public class UnitBasePart
{
    public UnitBasePart()
    {

    }
    public string Name { get; set; }
    public TileObjectType PartType { get; set; }
    public int Level { get; set; }
    public int CompleteLevel { get; set; }
    public bool IsUnderConstruction { get; set; }
    public bool Destroyed { get; set; }
    public GameObject Part { get; set; }
    public List<UnitBaseTileObject> TileObjects { get; set; }

    public void ClearContainer()
    {

    }
}

public class UnitBase : MonoBehaviour
{
    public UnitBase()
    {

        UnitCommands = new List<UnitCommand>();
        UnitBaseParts = new List<UnitBasePart>();
        AboveGround =0f;
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

    private List<TransitObject> tileObjectsInTransit;

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
            GroundCell targetCell = HexGrid.GroundCells[DestinationPos];
            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y += HexGrid.hexCellHeight + AboveGround;

            float speed = 1.75f / HexGrid.GameSpeed;
            float step = speed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);
            UpdateDirection(unitPos3);
        }
        if (tileObjectsInTransit != null)
        {
            List<TransitObject> transit = new List<TransitObject>();
            transit.AddRange(tileObjectsInTransit);

            foreach (TransitObject transitObject in transit)
            {
                if (transitObject.GameObject == null)
                {
                    tileObjectsInTransit.Remove(transitObject);
                }
                else
                {
                    Vector3 vector3 = transitObject.TargetPosition;
                    //vector3.y = 2;

                    float speed = 1.5f / HexGrid.GameSpeed;
                    float step = speed * Time.deltaTime;

                    transitObject.GameObject.transform.position = Vector3.MoveTowards(transitObject.GameObject.transform.position, vector3, step);

                    if (transitObject.TargetRotation != null)
                    {
                        //LookAtDirection(HexGrid, transitObject.GameObject.transform, transitObject.GameObject.transform.position);

                        //vector3 = Vector3.RotateTowards(transitObject.GameObject.transform.rotation, transitObject.TargetRotation, step, step);
                    }

                    if (transitObject.GameObject.transform.position == transitObject.TargetPosition)
                    {
                        if (transitObject.DestroyAtArrival)
                        {
                            Destroy(transitObject.GameObject);
                        }
                        else
                        {
                            // int x=0;
                        }
                        tileObjectsInTransit.Remove(transitObject);
                    }
                }
            }
            if (tileObjectsInTransit.Count == 0)
                tileObjectsInTransit = null;
        }
    }

    public void AddTransitTileObject(TransitObject transitObject)
    {
        if (tileObjectsInTransit == null)
            tileObjectsInTransit = new List<TransitObject>();
        tileObjectsInTransit.Add(transitObject);
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
        
        GroundCell targetCell = HexGrid.GroundCells[CurrentPos];
        Vector3 unitPos3 = targetCell.transform.localPosition;

        if (!update)
        {
            unitPos3.y += HexGrid.hexCellHeight + AboveGround;
            transform.position = unitPos3;
            transform.SetParent(HexGrid.transform, false);
        }
        else
        {
            unitPos3.y += HexGrid.hexCellHeight + AboveGround;
            transform.position = unitPos3;
        }
        
        if (tileObjectsInTransit != null)
        {
            foreach (TransitObject transitObject in tileObjectsInTransit)
            {
                if (transitObject.DestroyAtArrival)
                    Destroy(transitObject.GameObject);
            }
            tileObjectsInTransit = null;
        }
    }

    public void MoveTo(Position pos)
    {
        DestinationPos = pos;
        if (Weapon != null)
        {
            Weapon.TurnTo(HexGrid, DestinationPos);
        }
    }

    public void UpdateStats(MoveUpdateStats stats)
    {
        if (stats != null)
        {
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

    private void ReplacePart(Transform part, MoveUpdateUnitPart moveUpdateUnitPart, bool underConstruction) 
    {
        // Replace
        GameObject newPart = HexGrid.InstantiatePrefab(moveUpdateUnitPart.Name);
        newPart.transform.position = part.transform.position;
        newPart.transform.SetParent(transform);
        newPart.name = moveUpdateUnitPart.Name;

        if (underConstruction)
        {
            newPart.SetActive(false);
            //SetMaterialGhost(PlayerId, newPart);
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
        UnitBasePart unitBasePart = new UnitBasePart();
        unitBasePart.Name = moveUpdateUnitPart.Name;
        unitBasePart.PartType = moveUpdateUnitPart.PartType;
        unitBasePart.Part = newPart;
        unitBasePart.Level = moveUpdateUnitPart.Level;
        unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;

        unitBasePart.IsUnderConstruction = underConstruction;

        if (moveUpdateUnitPart.TileObjects != null)
        {
            unitBasePart.TileObjects = new List<UnitBaseTileObject>();
            foreach (TileObject tileObject in moveUpdateUnitPart.TileObjects)
            {
                UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
                unitBaseTileObject.TileObject = tileObject;
                unitBasePart.TileObjects.Add(unitBaseTileObject);
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

                    GameObject waypointPrefab = HexGrid.GetTerrainResource("Waypoint");

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
        if (Extractor != null)
        {
            Extractor.Extract(HexGrid, move, unit, otherUnit);
        }
    }

    public void Upgrade(Move move, UnitBase upgradedUnit)
    {
        if (Assembler != null)
        {
            Assembler.Assemble(HexGrid, this, upgradedUnit, move);
        }
    }
    public void Fire(Move move)
    {
        if (Weapon != null)
        {
            Weapon.Fire(HexGrid, move);
        }
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
            if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Ammo"))
                SetMaterialGhost(playerId, child);
        }

        MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material[] newMaterials = new Material[meshRenderer.materials.Length];
            for (int i = 0; i < meshRenderer.materials.Length; i++)
            {
                Material material = meshRenderer.materials[i];
                if (material.name.StartsWith("Player"))
                {
                    Destroy(material);
                    if (playerId == 1) newMaterials[i] = HexGrid.GetMaterial("PlayerTransparent");
                    if (playerId == 2) newMaterials[i] = HexGrid.GetMaterial("PlayerTransparent");
                    if (playerId == 3) newMaterials[i] = HexGrid.GetMaterial("PlayerTransparent");
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
        for (int i=0; i < unit.transform.childCount; i++)
        {
            GameObject child = unit.transform.GetChild(i).gameObject;
            if (!child.name.StartsWith("Mineral") && !child.name.StartsWith("Ammo"))
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
            }
            else
            {
                //return;

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

    private GameObject AddPart(GameObject foundation, GameObject parent, string name, bool underConstruction)
    {
        float y = 0.01f;
        if (parent != null)
        {
            Renderer rend = parent.GetComponent<Renderer>();
            y += parent.transform.position.y + rend.bounds.size.y; // + 0.1f;
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
            newPart.SetActive(false);
            //SetMaterialGhost(PlayerId, newPart);
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
                lastObject = AddPart(foundation, lastObject, "Foundation", UnderConstruction);
                foundation = lastObject;
                AddBasePart(moveUpdateUnitPart, lastObject);
            }
            else if (moveUpdateUnitPart.PartType == TileObjectType.PartContainer)
            {
                lastObject = AddPart(foundation, lastObject, "ContainerPart", UnderConstruction);
                AddBasePart(moveUpdateUnitPart, lastObject);

                if (moveUpdateUnitPart.Level > 1)
                {
                    lastObject = AddPart(foundation, lastObject, "ContainerPart", UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                }
                if (moveUpdateUnitPart.Level > 2)
                {
                    lastObject = AddPart(foundation, lastObject, "ContainerPart", UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                }
            }
            else if (moveUpdateUnitPart.PartType == TileObjectType.PartReactor)
            {
                if (moveUpdateUnitPart.Level == 1)
                {
                    lastObject = AddPart(foundation, lastObject, "ReactorPart", UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                }
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
                if (moveUpdateUnitPart.Level == 1)
                {
                    lastObject = AddPart(foundation, lastObject, "AssemblerPart", UnderConstruction);
                    AddBasePart(moveUpdateUnitPart, lastObject);
                }
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
            }

        }
        UpdateParts();
    }

    private void AddBasePart(MoveUpdateUnitPart moveUpdateUnitPart, GameObject gameObject)
    {
        UnitBasePart unitBasePart = new UnitBasePart();
        unitBasePart.Name = moveUpdateUnitPart.Name;
        unitBasePart.PartType = moveUpdateUnitPart.PartType;
        unitBasePart.Part = gameObject;
        unitBasePart.Level = moveUpdateUnitPart.Level;
        unitBasePart.CompleteLevel = moveUpdateUnitPart.CompleteLevel;

        unitBasePart.IsUnderConstruction = UnderConstruction;
        UnitBaseParts.Add(unitBasePart);
    }

    /*
    private string GetPartThatHasBeenHit()
    {
        if (PartsThatHaveBeenHit == null)
        {
            return null;
        }
        string hitPart = PartsThatHaveBeenHit[0];
        PartsThatHaveBeenHit.Remove(hitPart);
        if (PartsThatHaveBeenHit.Count == 0)
            PartsThatHaveBeenHit = null;
        return hitPart;
    }*/

    public void PartExtracted(TileObjectType hitPart)
    {
        if (!shellHit)
        {
            int x = 0;
        }

        shellHit = false;

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

    private bool shellHit;

    public void HitByShell()
    {
        shellHit = true;

        ParticleSystem particleSource;

        particleSource = HexGrid.MakeParticleSource("TankExplosion");
        particleSource.transform.SetParent(transform, false);
        particleSource.Play();
        HexGrid.Destroy(particleSource, 2f);
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

    public void Assemble(bool underConstruction)
    {
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
                    if (unitBasePart.Name == moveUpdateUnitPart.Name)
                    {
                        if (unitBasePart.IsUnderConstruction && moveUpdateUnitPart.Exists)
                        {
                            // Change from transparent to reals
                            unitBasePart.IsUnderConstruction = false;
                            SetPlayerColor(HexGrid, PlayerId, unitBasePart.Part);
                        }
                        if (moveUpdateUnitPart.TileObjects != null)
                        {
                            unitBasePart.TileObjects = new List<UnitBaseTileObject>();
                            foreach (TileObject tileObject in moveUpdateUnitPart.TileObjects)
                            {
                                UnitBaseTileObject unitBaseTileObject = new UnitBaseTileObject();
                                unitBaseTileObject.TileObject = tileObject;
                                unitBasePart.TileObjects.Add(unitBaseTileObject);
                            }
                        }
                        if (unitBasePart.Level != moveUpdateUnitPart.Level)
                        {
                            unitBasePart.Level = moveUpdateUnitPart.Level;
                        }

                        //unitBasePart.Part.SetActive(unitBasePart.IsUnderConstruction || moveUpdateUnitPart.Exists);
                        if (moveUpdateUnitPart.Exists)
                        {
                            Engine1 engine = unitBasePart.Part.GetComponent<Engine1>();
                            if (engine != null)
                            {
                                Engine = engine;
                            }
                            Container1 container = unitBasePart.Part.GetComponent<Container1>();
                            if (container != null)
                            {

                                Container = container;
                                container.UpdateContent(HexGrid, moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Extractor1 extractor = unitBasePart.Part.GetComponent<Extractor1>();
                            if (extractor != null)
                                Extractor = extractor;
                            Assembler1 assembler = unitBasePart.Part.GetComponent<Assembler1>();
                            if (assembler != null)
                            {
                                Assembler = assembler;
                                assembler.UpdateContent(HexGrid, moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Weapon1 weapon = unitBasePart.Part.GetComponent<Weapon1>();
                            if (weapon != null)
                            {
                                Weapon = weapon;
                                weapon.UpdateContent(HexGrid, moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Reactor1 reactor = unitBasePart.Part.GetComponent<Reactor1>();
                            if (reactor != null)
                            {
                                Reactor = reactor;
                                reactor.UpdateContent(HexGrid, moveUpdateUnitPart.TileObjects, moveUpdateUnitPart.Capacity);
                            }
                            Armor armor = unitBasePart.Part.GetComponent<Armor>();
                            if (armor != null)
                            {
                                Armor = armor;
                                Transform shield = transform.Find("Shield");
                                if (shield != null)
                                {
                                    shield.gameObject.SetActive(moveUpdateUnitPart.ShieldActive == true);
                                }
                            }
                        }
                        else
                        {
                            missingPartFound = true;
                        }
                    }
                }
            }
        }

        if (UnderConstruction && missingPartFound == false)
            UnderConstruction = false;
    }
}
