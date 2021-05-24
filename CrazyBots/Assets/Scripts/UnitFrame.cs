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

    public bool PlaceWeapon(MonoBehaviour container1, MonoBehaviour engine1, HexCell targetCell, HexGrid hexGrid)
    {
        if (frontLeft == false || frontRight == false || centerLeft == false || centerRight == false)
        {
            // Does not fit
            return false;
        }
        if (engine1 == null)
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
            container1.transform.SetParent(engine1.transform, false);
        }
        return true;
    }

    public bool PlaceBigPart(MonoBehaviour container1, MonoBehaviour engine1, HexCell targetCell, HexGrid hexGrid)
    {
        if (frontLeft == false || frontRight == false || centerLeft == false || centerRight == false)
        {
            // Does not fit
            return false;
        }
        if (engine1 == null)
        {
        }
        else
        {
            Vector3 unitPos3 = new Vector3();
            unitPos3.y += 0.05f; // Engine height

            //unitPos3.z += 0.25f; // front
            //unitPos3.x += 0.15f; // middle

            frontLeft = false;
            frontRight = false;

            centerLeft = false;
            centerRight = false;

            container1.transform.position = unitPos3;
            container1.transform.SetParent(engine1.transform, false);
        }
        return true;
    }

    public void PlaceOnGround(MonoBehaviour part, HexCell targetCell, HexGrid hexGrid)
    {
        Vector3 unitPos3 = targetCell.transform.position;
        unitPos3.y += hexGrid.hexCellHeight - 0.02f;
        part.transform.position = unitPos3;

        part.transform.SetParent(hexGrid.transform, false);
    }

    public void PlacePart(MonoBehaviour part, MonoBehaviour parent, HexCell targetCell, HexGrid hexGrid)
    {
        if (parent == null)
        {
            Vector3 unitPos3 = new Vector3();
            unitPos3.y += hexGrid.hexCellHeight + 0.05f;

            if (rearRight == true)
            {
                unitPos3.z -= 0.45f; // rear
                unitPos3.x += 0.3f; // right
                rearRight = false;
            }
            else if (rearLeft == true)
            {
                unitPos3.z -= 0.45f; // rear
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
                unitPos3.z += 0.45f; // front
                unitPos3.x += 0.3f; // right
                frontRight = false;
            }
            else if (frontLeft == true)
            {
                unitPos3.z += 0.45f; // front
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
                unitPos3.y += 0.03f; // Foundation height

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
                unitPos3.z += 0.45f; // front
                unitPos3.x += 0.15f; // right
                frontRight = false;
            }
            else if (frontLeft == true)
            {
                unitPos3.z += 0.45f; // front
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
    private Container1 container1;
    private Assembler1 assembler1;
    private Weapon1 weapon1;
    private Extractor1 extractor1;
    private Reactor1 reactor1;

    // Current postions
    internal Position currentPos;
    internal int playerId;
    internal string UnitId { get; set; }
    internal MonoBehaviour foundationPart;

    public UnitFrame()
    {
        unitLayout = new UnitLayout();
    }

    public void Delete()
    {
        foundationPart = null;
        if (weapon1 != null)
        {
            HexGrid.Destroy(weapon1.gameObject);
            weapon1 = null;
        }
        if (extractor1 != null)
        {
            HexGrid.Destroy(extractor1.gameObject);
            extractor1 = null;
        }
        if (container1 != null)
        {
            HexGrid.Destroy(container1.gameObject);
            container1 = null;
        }
        if (assembler1 != null)
        {
            HexGrid.Destroy(assembler1.gameObject);
            assembler1 = null;
        }
        if (reactor1 != null)
        {
            HexGrid.Destroy(reactor1.gameObject);
            reactor1 = null;
        }
        if (engine1 != null)
        {
            HexGrid.Destroy(engine1.gameObject);
            engine1 = null;
        }
    }

    private void SetPlayerColor(MonoBehaviour unit)
    {
        Material playerMaterial = Resources.Load<Material>("Materials/Player" + playerId);
        MeshRenderer meshRenderer = unit.GetComponent<MeshRenderer>();

        Material[] newMaterials = new Material[meshRenderer.materials.Count()];

        for (int i = 0; i < meshRenderer.materials.Count(); i++)
        {
            Material material = meshRenderer.materials[i];
            if (material.name.StartsWith("Player"))
            {
                newMaterials[i] = playerMaterial;
            }
            else
            {
                newMaterials[i] = material;
            }
        }
        meshRenderer.materials = newMaterials;
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
                engine1.name = "Engine-" + UnitId;
                foundationPart = engine1;
                SetPlayerColor(engine1);

                engine1.transform.SetParent(HexGrid.transform, false);

                Vector3 unitPos3 = targetCell.transform.position;
                unitPos3.y += HexGrid.hexCellHeight - 0.05f; // Start beyond assembler
                engine1.transform.position = unitPos3;
            }
        }
        else
        {
            if (engine1 != null)
            {
                HexGrid.Destroy(engine1);
                engine1 = null;
            }
        }

        if (stats.WeaponLevel > 0)
        {
            if (weapon1 == null && stats.WeaponLevel == 1)
            {
                weapon1 = HexGrid.InstantiatePrefab<Weapon1>("Weapon1");
                weapon1.UnitFrame = this;
                weapon1.name = "Weapon-" + UnitId;
                SetPlayerColor(weapon1);

                if (foundationPart == null)
                {
                    unitLayout.PlaceOnGround(weapon1, targetCell, HexGrid);
                    foundationPart = weapon1;
                }
                else
                {
                    if (!unitLayout.PlaceWeapon(weapon1, foundationPart, targetCell, HexGrid))
                    {

                    }
                }
            }
        }
        else
        {
            if (weapon1 != null)
            {
                HexGrid.Destroy(weapon1);
                weapon1 = null;
            }
        }

        if (stats.ExtractorLevel > 0)
        {
            if (extractor1 == null && stats.ExtractorLevel == 1)
            {
                if (foundationPart == null)
                {
                    extractor1 = HexGrid.InstantiatePrefab<Extractor1>("ExtractorGround1");
                    unitLayout.PlaceOnGround(extractor1, targetCell, HexGrid);
                    foundationPart = extractor1;
                }
                else
                {
                    extractor1 = HexGrid.InstantiatePrefab<Extractor1>("Extractor1");
                    unitLayout.PlacePart(extractor1, foundationPart, targetCell, HexGrid);
                }
                extractor1.name = "Extractor-" + UnitId;
                extractor1.UnitFrame = this;
                SetPlayerColor(extractor1);
            }
        }
        else
        {
            if (extractor1 != null)
            {
                HexGrid.Destroy(extractor1);
                extractor1 = null;
            }
        }


        if (stats.ContainerLevel > 0)
        {
            if (container1 == null && stats.ContainerLevel == 1)
            {
                //container1 = HexGrid.Instantiate<Container1>(HexGrid.Container1);
                container1 = HexGrid.InstantiatePrefab<Container1>("Container1");
                container1.UnitFrame = this;
                container1.name = "Container-" + UnitId;

                SetPlayerColor(container1);
                if (foundationPart is Extractor1 && stats.ProductionLevel > 0)
                {
                    Vector3 unitPos3 = new Vector3();
                    unitPos3.y += 0.15f;
                    container1.transform.position = unitPos3;
                    container1.transform.SetParent(foundationPart.transform, false);
                }
                else
                {
                    if (foundationPart == null)
                    {
                        unitLayout.PlaceOnGround(container1, targetCell, HexGrid);
                        foundationPart = extractor1;
                    }
                    else
                    {
                        unitLayout.PlacePart(container1, foundationPart, targetCell, HexGrid);
                    }
                }
            }
            container1.UpdateContent(stats.ContainerFull);
        }
        else
        {
            if (container1 != null)
            {
                HexGrid.Destroy(container1);
                container1 = null;
            }
        }

        if (stats.ProductionLevel > 0)
        {
            if (assembler1 == null && stats.ProductionLevel == 1)
            {
                assembler1 = HexGrid.InstantiatePrefab<Assembler1>("Assembler1");
                assembler1.UnitFrame = this;
                SetPlayerColor(assembler1);

                if (foundationPart == null)
                {
                    unitLayout.PlaceOnGround(assembler1, targetCell, HexGrid);
                    foundationPart = assembler1;
                }
                else
                {
                    unitLayout.PlaceBigPart(assembler1, foundationPart, targetCell, HexGrid);
                }
            }
        }
        else
        {
            if (assembler1 != null)
            {
                HexGrid.Destroy(assembler1);
                assembler1 = null;
            }
        }

        if (stats.ReactorLevel > 0)
        {
            if (reactor1 == null && stats.ReactorLevel == 1)
            {
                reactor1 = HexGrid.InstantiatePrefab<Reactor1>("Reactor1");
                reactor1.UnitFrame = this;
                SetPlayerColor(reactor1);

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
        else
        {
            if (reactor1 != null)
            {
                HexGrid.Destroy(reactor1);
                reactor1 = null;
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
        if (NextMove?.MoveType == MoveType.Upgrade)
        {

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
