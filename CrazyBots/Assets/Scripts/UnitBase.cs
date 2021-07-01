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

public class UnitBasePart
{
    public string Name { get; set; }
    public bool IsUnderConstruction { get; set; }
    public GameObject Part { get; set; }

}

public class UnitBase : MonoBehaviour
{
    public UnitBase()
    {
        UnitCommands = new List<UnitCommand>();
        unitBaseParts = new List<UnitBasePart>();

        AboveGround =0f;
    }

    public HexGrid HexGrid { get; set; }
    internal Position CurrentPos { get; set; }
    internal Position DestinationPos { get; set; }
    internal int PlayerId { get; set; }
    internal string UnitId { get; set; }
    public MoveUpdateStats MoveUpdateStats { get; set; }

    private List<UnitBasePart> unitBaseParts;
    
    internal bool Temporary { get; set; }
    internal bool UnderConstruction { get; set; }
    internal bool HasBeenDestroyed { get; set; }
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
            SetMaterialGhost(PlayerId, newPart);
        else
            SetPlayerColor(HexGrid, PlayerId, newPart);

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
        unitBasePart.Part = newPart;
        unitBasePart.IsUnderConstruction = underConstruction;
        unitBaseParts.Add(unitBasePart);
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

                    if (unitCommand.GameCommand.GameCommandType == GameCommandType.Minerals)
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

        foreach (UnitBasePart unitBasePart in unitBaseParts)
        {
            SetPlayerColor(HexGrid, PlayerId, unitBasePart.Part);
        }
    }

    public void Extract(Move move)
    {
        if (Extractor != null)
        {
            Extractor.Extract(HexGrid, move);
        }
    }

    public void Upgrade(Move move)
    {
        if (Assembler != null)
        {
            Assembler.Assemble(HexGrid, move);
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
    private Container1 Container;
    private Extractor1 Extractor;
    private Weapon1 Weapon;
    private Reactor1 Reactor;
    private Armor Armor;

    public void Assemble(bool underConstruction)
    {
        UnderConstruction = underConstruction;
        unitBaseParts.Clear();

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
            if (engine != null && moveUpdateUnitPart.PartType.StartsWith("Engine"))
            {
                ReplacePart(engine, moveUpdateUnitPart, underConstruction);
                remainingParts.Remove(moveUpdateUnitPart);
                groundFound = true;
                break;
            }
            else if (ground != null && moveUpdateUnitPart.PartType.StartsWith("Extractor"))
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
                if (moveUpdateUnitPart.PartType.StartsWith("Container") ||
                    moveUpdateUnitPart.PartType.StartsWith("Weapon") ||
                    moveUpdateUnitPart.PartType.StartsWith("Reactor") ||
                    moveUpdateUnitPart.PartType.StartsWith("Assembler"))
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

        foreach (UnitBasePart unitBasePart in unitBaseParts)
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
                    unitBasePart.Part.SetActive(unitBasePart.IsUnderConstruction || moveUpdateUnitPart.Exists);
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
                            container.UpdateContent(HexGrid, moveUpdateUnitPart.Minerals, moveUpdateUnitPart.Capacity);
                        }
                        Extractor1 extractor = unitBasePart.Part.GetComponent<Extractor1>();
                        if (extractor != null)
                            Extractor = extractor;
                        Assembler1 assembler = unitBasePart.Part.GetComponent<Assembler1>();
                        if (assembler != null)
                        {
                            Assembler = assembler;
                            assembler.UpdateContent(HexGrid, moveUpdateUnitPart.Minerals, moveUpdateUnitPart.Capacity);
                        }
                        Weapon1 weapon = unitBasePart.Part.GetComponent<Weapon1>();
                        if (weapon != null)
                        {
                            Weapon = weapon;
                            weapon.UpdateContent(HexGrid, moveUpdateUnitPart.Minerals, moveUpdateUnitPart.Capacity);
                        }
                        Reactor1 reactor = unitBasePart.Part.GetComponent<Reactor1>();
                        if (reactor != null)
                        {
                            Reactor = reactor;
                            reactor.UpdateContent(HexGrid, moveUpdateUnitPart.Minerals, moveUpdateUnitPart.Capacity);
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
        if (UnderConstruction && missingPartFound == false)
            UnderConstruction = false;
    }
}
