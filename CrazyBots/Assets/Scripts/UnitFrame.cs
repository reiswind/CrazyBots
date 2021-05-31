using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnitPart : MonoBehaviour
{
    
}

public class UnitLayoutPart
{
    public float PosX { get; set; }
    public float PosY { get; set; }
}

public class UnitLayout
{
    public UnitLayout()
    {
        Parts = new List<UnitLayoutPart>();
    }
    public List<UnitLayoutPart> Parts { get; private set; }

    // Possible positions
    bool frontLeft = true;
    bool frontRight = true;

    bool centerLeft = true;
    bool centerRight = true;

    bool rearLeft = true;
    bool rearRight = true;

    internal Frame frame1; // Frame level 1
    internal Frame frame2; // Frame level 2
    internal Frame frame3; // Frame level 3

    public bool PlaceWeapon(MonoBehaviour container1, MonoBehaviour foundationPart, HexCell targetCell, HexGrid hexGrid)
    {
        if (frontLeft == false || frontRight == false || centerLeft == false || centerRight == false)
        {
            // Does not fit
            return false;
        }
        if (foundationPart == null)
        {
        }
        else
        {
            Vector3 unitPos3 = new Vector3();
            unitPos3.y += 0.09f; // Engine height

            unitPos3.z += 0.25f; // front
            //unitPos3.x += 0.15f; // middle

            frontLeft = false;
            frontRight = false;

            centerLeft = false;
            centerRight = false;

            container1.transform.position = unitPos3;
            container1.transform.SetParent(foundationPart.transform, false);
        }
        return true;
    }

    public bool PlaceContainer(MonoBehaviour container1, MonoBehaviour foundationPart, HexCell targetCell, HexGrid hexGrid)
    {
        if (rearLeft == false || rearRight == false || centerLeft == false || centerRight == false)
        {
            // Does not fit
            return false;
        }
        if (foundationPart == null)
        {
        }
        else
        {
            Vector3 unitPos3 = new Vector3();
            unitPos3.y += 0.19f; // Engine height

            unitPos3.z -= 0.14f; // front
            //unitPos3.x += 0.15f; // middle

            rearLeft = false;
            rearRight = false;

            centerLeft = false;
            centerRight = false;

            container1.transform.position = unitPos3;
            container1.transform.SetParent(foundationPart.transform, false);
        }
        return true;
    }


    public bool PlaceOnTop(MonoBehaviour bigPart, MonoBehaviour foundationPart, HexCell targetCell, HexGrid hexGrid, float y)
    {
        if (foundationPart == null)
        {
        }
        else
        {
            Vector3 unitPos3 = new Vector3();

            unitPos3.y += y;

            bigPart.transform.position = unitPos3;
            bigPart.transform.SetParent(foundationPart.transform, false);

        }
        return true;
    }


    public bool PlaceBigPart(MonoBehaviour bigPart, MonoBehaviour foundationPart, HexCell targetCell, HexGrid hexGrid)
    {
        if (foundationPart == null)
        {
        }
        else
        {
            Vector3 unitPos3 = new Vector3();
            /*
            if (frame1 == null)
            {
                frame1 = hexGrid.InstantiatePrefab<Frame>("Frame");
                unitPos3 = new Vector3();
                frame1.transform.position = unitPos3;
                frame1.transform.SetParent(foundationPart.transform, false);

                unitPos3.y += 0.3f; // Height level 1
            }
            else if (frame2 == null)
            {
                frame2 = hexGrid.InstantiatePrefab<Frame>("Frame2");
                unitPos3 = new Vector3();
                unitPos3.y += 0.6f; // Height level 1
                frame2.transform.position = unitPos3;
                frame2.transform.SetParent(foundationPart.transform, false);

                //unitPos3.y += 0.0f; // Height level 1 + 2
            }
            else if (frame3 == null)
            {
                frame3 = hexGrid.InstantiatePrefab<Frame>("Frame3");
                unitPos3 = new Vector3();
                unitPos3.y += 0.9f; // Height level 1 + 2
                frame3.transform.position = unitPos3;
                frame3.transform.SetParent(foundationPart.transform, false);

                //unitPos3.y += 1.2f; // Height level 1 + 2 + 3
            }*/


            frontLeft = false;
            frontRight = false;
            centerLeft = false;
            centerRight = false;
            rearLeft = false;
            rearRight = false;

            bigPart.transform.position = unitPos3;
            bigPart.transform.SetParent(foundationPart.transform, false);

        }
        return true;
    }

    public void PlaceOnGround(MonoBehaviour part, HexCell targetCell, HexGrid hexGrid)
    {
        try
        {
            Vector3 unitPos3 = targetCell.Cell.transform.position;
            unitPos3.y += hexGrid.hexCellHeight;
            part.transform.position = unitPos3;

            part.transform.SetParent(hexGrid.transform, false);
        }
        catch (Exception err)
        {
            Debug.Log(err);
        }
    }

    public void PlacePart(MonoBehaviour part, MonoBehaviour parent, HexCell targetCell, HexGrid hexGrid)
    {
        if (parent == null)
        {
            Vector3 unitPos3 = new Vector3();
            unitPos3.y += hexGrid.hexCellHeight + 0.25f;

            if (rearRight == true)
            {
                unitPos3.z -= 0.45f; // rear
                unitPos3.x += 0.4f; // right
                rearRight = false;
            }
            else if (rearLeft == true)
            {
                unitPos3.z -= 0.35f; // rear
                unitPos3.x -= 0.3f; // left
                rearLeft = false;
            }
            else if (centerLeft == true)
            {
                unitPos3.x -= 0.3f; // left
                centerLeft = false;
            }
            else if (centerRight == true)
            {
                unitPos3.x += 0.3f; // right
                centerRight = false;
            }
            else if (frontRight == true)
            {
                unitPos3.z += 0.25f; // front
                unitPos3.x += 0.3f; // right
                frontRight = false;
            }
            else if (frontLeft == true)
            {
                unitPos3.z += 0.25f; // front
                unitPos3.x -= 0.3f; // left
                frontLeft = false;
            }
            part.transform.position = unitPos3;
            part.transform.SetParent(hexGrid.transform, false);
        }
        else
        {
            Vector3 unitPos3 = new Vector3();

            if (parent is Engine1)
                unitPos3.y += 0.09f; // Engine height
            else
                unitPos3.y += 2.03f; // Foundation height

            if (rearRight == true)
            {
                unitPos3.z -= 0.25f; // rear
                unitPos3.x += 0.15f; // right
                rearRight = false;
            }
            else if (rearLeft == true)
            {
                unitPos3.z -= 0.25f; // rear
                unitPos3.x -= 0.15f; // left
                rearLeft = false;
            }
            else if (centerLeft == true)
            {
                unitPos3.x -= 0.15f; // left
                centerLeft = false;
            }
            else if (centerRight == true)
            {
                unitPos3.x += 0.15f; // right
                centerRight = false;
            }
            else if (frontRight == true)
            {
                unitPos3.z += 0.30f; // front
                unitPos3.x += 0.15f; // right
                frontRight = false;
            }
            else if (frontLeft == true)
            {
                unitPos3.z += 0.30f; // front
                unitPos3.x -= 0.15f; // left
                frontLeft = false;
            }
            

            /*
            unitPos3.x = 0.3f; // right
            unitPos3.x = 0.0f; // left

            unitPos3.z = 0.15f; // middle
            unitPos3.z = 0.45f; // front
            unitPos3.z = -0.05f; // rear

            unitPos3.y = 0.1f; // 
            */

            part.transform.position = unitPos3;
            part.transform.SetParent(parent.transform, false);
        }
    }

}


public class UnitFrame
{
  
    public Position FinalDestination { get; set; }
    public Move NextMove { get; set; }
    public HexGrid HexGrid { get; set; }

    private UnitLayout unitLayout;

    private Engine1 engine1;
    private Container1 container;

    private Assembler1 assembler;
    private Weapon1 weapon1;
    private Extractor1 extractor1;
    private Reactor1 reactor1;
    private Armor armor;

    // Current postions
    internal Position currentPos;
    internal int playerId;
    internal string UnitId { get; set; }
    internal MonoBehaviour foundationPart;

    internal bool HasBeenDestroyed { get; set; }

    public UnitFrame()
    {
        unitLayout = new UnitLayout();
    }

    public void Delete()
    {
        HasBeenDestroyed = true;

        float delay = 0.3f * HexGrid.GameSpeed;

        foundationPart = null;
        if (weapon1 != null)
        {
            HexGrid.Destroy(weapon1.gameObject, delay);
            weapon1 = null;
        }
        if (extractor1 != null)
        {
            HexGrid.Destroy(extractor1.gameObject, delay);
            extractor1 = null;
        }
        if (container != null)
        {
            HexGrid.Destroy(container.gameObject, delay);
            container = null;
        }
        if (assembler != null)
        {
            HexGrid.Destroy(assembler.gameObject, delay);
            assembler = null;
        }
        if (reactor1 != null)
        {
            HexGrid.Destroy(reactor1.gameObject, delay);
            reactor1 = null;
        }
        if (engine1 != null)
        {
            HexGrid.Destroy(engine1.gameObject, delay);
            engine1 = null;
        }
    }

    internal static Material playerMaterial1;
    internal static Material playerMaterial2;
    internal static Material playerMaterial3;

    internal static void SetPlayerColor(int playerId, GameObject unit)
    {
        if (playerMaterial1 == null)
        {
            playerMaterial1 = Resources.Load<Material>("Materials/Player1");
            playerMaterial2 = Resources.Load<Material>("Materials/Player2");
            playerMaterial3 = Resources.Load<Material>("Materials/Player3");
        }
        MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();

        //Material[] newMaterials = new Material[meshRenderer.materials.Length];
        for (int i = 0; i < meshRenderer.materials.Length; i++)
        {
            Material material = meshRenderer.materials[i];
            if (material.name.StartsWith("Player"))
            {
                if (playerId == 1) material.color = playerMaterial1.color;
                if (playerId == 2) material.color = playerMaterial2.color;
                if (playerId == 3) material.color = playerMaterial3.color;
                
            }
            else
            {
                //newMaterials[i] = material;
            }
        }
        //meshRenderer.materials = newMaterials;
    }

    private void ReparentParts()
    {
        if (engine1 != null && engine1 != foundationPart)
        {
            engine1.transform.SetParent(foundationPart.transform.parent);
        }
        if (weapon1 != null && weapon1 != foundationPart)
        {
            weapon1.transform.SetParent(foundationPart.transform.parent);
        }
        if (assembler != null && assembler != foundationPart)
        {
            assembler.transform.SetParent(foundationPart.transform.parent);
        }
        if (container != null && container != foundationPart)
        {
            container.transform.SetParent(foundationPart.transform.parent);
        }
        if (reactor1 != null && reactor1 != foundationPart)
        {
            reactor1.transform.SetParent(foundationPart.transform.parent);
        }
        if (extractor1 != null && extractor1 != foundationPart)
        {
            extractor1.transform.SetParent(foundationPart.transform.parent);
        }
        

    }

    public void Assemble()
    {
        MoveUpdateStats stats = MoveUpdateStats;
        if (stats == null)
            return;

        if (currentPos == null)
            return;

        /*
        NextMove.Stats.EngineLevel
        NextMove.Stats.ArmorLevel **
        NextMove.Stats.ContainerLevel
        NextMove.Stats.ExtractorLevel
        NextMove.Stats.WeaponLevel
        NextMove.Stats.ReactorLevel
        NextMove.Stats.ProductionLevel
        */

        HexCell targetCell = HexGrid.GroundCells[currentPos];

        // Place the engine
        if (stats.EngineLevel > 0)
        {
            if (engine1 == null && stats.EngineLevel == 1)
            {
                engine1 = HexGrid.InstantiatePrefab<Engine1>("Engine1");

                engine1.UnitFrame = this;
                engine1.UnitId = UnitId;
                engine1.name = UnitId + "-Engine";
                foundationPart = engine1;
                SetPlayerColor(playerId, engine1.gameObject);

                unitLayout.PlaceOnGround(engine1, targetCell, HexGrid);
            }
        }
        else
        {
            if (foundationPart != null && engine1 == foundationPart)
            {
                ReparentParts();
                foundationPart = null;
            }
            if (engine1 != null)
            {
                HexGrid.Destroy(engine1.gameObject);
                engine1 = null;
            }
        }


        // Place big extractor
        if (stats.ExtractorLevel > 0)
        {
            if (extractor1 == null && stats.ExtractorLevel == 1)
            {
                if (foundationPart == null)
                {
                    extractor1 = HexGrid.InstantiatePrefab<Extractor1>("ExtractorGround1");
                    unitLayout.PlaceOnGround(extractor1, targetCell, HexGrid);
                    foundationPart = extractor1;

                    extractor1.name = UnitId + "-Extractor";
                    extractor1.UnitFrame = this;

                    SetPlayerColor(playerId, extractor1.gameObject);
                }
            }
        }
        else
        {
            if (foundationPart != null && extractor1 == foundationPart)
            {
                ReparentParts();
                foundationPart = null;
            }
            if (extractor1 != null)
            {
                HexGrid.Destroy(extractor1.gameObject);
                extractor1 = null;
            }
        }

        if (stats.ProductionLevel > 0)
        {
            if (assembler == null || assembler.Level != stats.ProductionLevel)
            {
                if (assembler != null)
                {
                    HexGrid.Destroy(assembler.gameObject);
                    assembler = null;
                }
                assembler = HexGrid.InstantiatePrefab<Assembler1>("Assembler" + stats.ProductionLevel);
                assembler.UnitFrame = this;
                assembler.Level = stats.ProductionLevel;
                assembler.name = UnitId + "-Assembler";

                SetPlayerColor(playerId, assembler.gameObject);

                if (foundationPart == null)
                {
                    unitLayout.PlaceOnGround(assembler, targetCell, HexGrid);
                    foundationPart = assembler;
                }
                else
                {
                    if (unitLayout.frame1 != null)
                    {
                        HexGrid.Destroy(unitLayout.frame1.gameObject);
                        unitLayout.frame1 = null;
                    }
                    unitLayout.PlaceBigPart(assembler, foundationPart, targetCell, HexGrid);
                }
            }
        }
        else
        {
            if (assembler != null)
            {
                HexGrid.Destroy(assembler.gameObject);
                assembler = null;
            }
        }


        if (stats.ContainerLevel > 0)
        {
            if (container == null ||  container.Level != stats.ContainerLevel)
            {
                if (container != null)
                {
                    HexGrid.Destroy(container.gameObject);
                    container = null;
                }
                container = HexGrid.InstantiatePrefab<Container1>("Container" + stats.ContainerLevel);
                container.UnitFrame = this;
                container.name = UnitId + "-Container";
                container.Level = stats.ContainerLevel;

                SetPlayerColor(playerId, container.gameObject);
                if (foundationPart is Extractor1)
                {
                    if (stats.ProductionLevel > 0)
                    {
                        // Put it on top of the assembler
                        unitLayout.PlaceOnTop(container, foundationPart, targetCell, HexGrid, 0.6f);
                    }
                    else
                    {
                        unitLayout.PlaceOnTop(container, foundationPart, targetCell, HexGrid, 0.3f);
                    }
                }
                else
                {
                    if (foundationPart == null)
                    {
                        unitLayout.PlaceOnGround(container, targetCell, HexGrid);
                        foundationPart = container;
                    }
                    else
                    {
                        // Blueprint
                        if (!unitLayout.PlaceContainer(container, foundationPart, targetCell, HexGrid))
                        {

                        }
                        //unitLayout.PlacePart(container, foundationPart, targetCell, HexGrid);
                    }
                }
            }
            container.UpdateContent(stats.ContainerFull);
        }
        else
        {
            if (container != null)
            {
                HexGrid.Destroy(container.gameObject);
                container = null;
            }
        }

        
        if (stats.ReactorLevel > 0)
        {
            if (reactor1 == null && stats.ReactorLevel == 1)
            {
                reactor1 = HexGrid.InstantiatePrefab<Reactor1>("Reactor1");
                reactor1.UnitFrame = this;
                reactor1.name = UnitId + "-Reactor1";

                SetPlayerColor(playerId, reactor1.gameObject);
                if (foundationPart is Extractor1)
                {
                    if (stats.ProductionLevel > 0 && stats.ContainerLevel > 0)
                    {
                        // Put it on top of the container
                        unitLayout.PlaceOnTop(reactor1, foundationPart, targetCell, HexGrid, 0.8f);
                    }
                    else
                    {
                        unitLayout.PlaceBigPart(reactor1, foundationPart, targetCell, HexGrid);
                    }
                }
                else
                {
                    if (foundationPart == null)
                    {
                        unitLayout.PlaceOnGround(reactor1, targetCell, HexGrid);
                        foundationPart = reactor1;
                    }
                    else
                    {
                        unitLayout.PlacePart(reactor1, foundationPart, targetCell, HexGrid);
                    }
                }
            }
        }
        else
        {
            if (reactor1 != null)
            {
                HexGrid.Destroy(reactor1.gameObject);
                reactor1 = null;
            }
        }

        if (stats.WeaponLevel > 0)
        {
            if (weapon1 == null) // && stats.WeaponLevel == 1)
            {
                weapon1 = HexGrid.InstantiatePrefab<Weapon1>("Weapon" + stats.WeaponLevel);
                weapon1.UnitFrame = this;
                weapon1.name = UnitId + "-Weapon";
                SetPlayerColor(playerId, weapon1.gameObject);
                
                if (foundationPart == null)
                {
                    unitLayout.PlaceOnGround(weapon1, targetCell, HexGrid);
                    foundationPart = weapon1;
                }
                else if (foundationPart is Extractor1)
                {
                    unitLayout.PlaceBigPart(weapon1, foundationPart, targetCell, HexGrid);
                }
                else
                {
                    if (!unitLayout.PlaceWeapon(weapon1, foundationPart, targetCell, HexGrid))
                    {

                    }
                }
            }
            weapon1.UpdateContent(stats.WeaponLoaded);
        }
        else
        {
            if (weapon1 != null)
            {
                HexGrid.Destroy(weapon1.gameObject);
                weapon1 = null;
            }
        }


        // Place small extractor
        if (stats.ExtractorLevel > 0)
        {
            if (extractor1 == null && stats.ExtractorLevel == 1)
            {
                if (foundationPart != null)
                {
                    extractor1 = HexGrid.InstantiatePrefab<Extractor1>("Extractor1");
                    unitLayout.PlacePart(extractor1, foundationPart, targetCell, HexGrid);

                    extractor1.name = UnitId + "-Extractor";
                    extractor1.UnitFrame = this;

                    SetPlayerColor(playerId, extractor1.gameObject);
                }
            }
        }
        // Place small armoir
        if (stats.ArmorLevel > 0)
        {
            if (armor == null && stats.ArmorLevel == 1)
            {
                if (foundationPart != null)
                {
                    armor = HexGrid.InstantiatePrefab<Armor>("Armor1");
                    unitLayout.PlacePart(armor, foundationPart, targetCell, HexGrid);

                    armor.name = UnitId + "-Armor";
                    armor.UnitFrame = this;

                    SetPlayerColor(playerId, armor.gameObject);
                }
            }
        }
        else
        {
            if (armor != null)
            {
                HexGrid.Destroy(armor.gameObject);
                armor = null;
            }
        }
    }

    public void JumpToTarget(Position pos)
    {
        return;
        /*
        if (FinalDestination != null)
        {
            if (engine1 != null)
            {
                // Did not reach target in time. Jump to it.
                HexCell targetCell = HexGrid.GroundCells[pos];

                Vector3 unitPos3 = targetCell.transform.localPosition;
                //unitPos3.y += currentBaseFrame.AboveGround;
                engine1.transform.position = unitPos3;

                engine1.transform.LookAt(unitPos3);
            }
            FinalDestination = null;
        }*/
    }

    public MoveUpdateStats MoveUpdateStats { get; set; }

    public void UpdateStats(MoveUpdateStats stats)
    {
        MoveUpdateStats = stats;
        Assemble();
    }

    public void Move(MonoBehaviour unit)
    {
        if (NextMove?.MoveType == MoveType.Hit) // && unit == foundationPart)
        {
            MoveUpdateStats = NextMove.Stats;
            Assemble();
            NextMove = null;
        }
        

        if (NextMove?.MoveType == MoveType.UpdateStats)
        {
            NextMove = null;
        }
        if (NextMove?.MoveType == MoveType.Add || NextMove?.MoveType == MoveType.Move)
        {
            currentPos = NextMove.Positions[NextMove.Positions.Count - 1];
        }
    }
}
