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

    public void PlaceOnGround(MonoBehaviour part, MonoBehaviour engine1, HexCell targetCell, HexGrid hexGrid)
    {
        if (engine1 == null)
        {
            part.transform.SetParent(hexGrid.transform, false);

            Vector3 unitPos3 = targetCell.transform.position;
            unitPos3.y += hexGrid.hexCellHeight - 0.10f;
            part.transform.position = unitPos3;
        }
    }

    public void PlacePart(MonoBehaviour part, MonoBehaviour engine1, HexCell targetCell, HexGrid hexGrid)
    {
        if (engine1 == null)
        {
            part.transform.SetParent(hexGrid.transform, false);

            Vector3 unitPos3 = targetCell.transform.position;
            unitPos3.y += hexGrid.hexCellHeight + 0.05f;

            if (centerLeft == true)
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
            else if (rearRight == true)
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

            part.transform.position = unitPos3;
        }
        else
        {
            part.transform.SetParent(engine1.transform, false);

            Vector3 unitPos3 = engine1.transform.position;

            unitPos3.y += 0.09f; // Engine height

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

    public UnitFrame()
    {
        unitLayout = new UnitLayout();
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

        //if (NextMove == null || NextMove.Stats == null)
        //    return;

        //Position pos = NextMove.Positions[NextMove.Positions.Count - 1];
        HexCell targetCell = HexGrid.GroundCells[currentPos];


        // Place the engine
        if (stats.EngineLevel > 0)
        {
            if (engine1 == null && stats.EngineLevel == 1)
            {
                engine1 = HexGrid.Instantiate<Engine1>(HexGrid.Engine1);
                engine1.UnitFrame = this;
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
            if (weapon1 == null && stats.ContainerLevel == 1)
            {
                weapon1 = HexGrid.Instantiate<Weapon1>(HexGrid.Weapon1);
                weapon1.UnitFrame = this;
                SetPlayerColor(weapon1);
               
                if (!unitLayout.PlaceWeapon(weapon1, engine1, targetCell, HexGrid))
                {

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
                if (engine1 == null)
                {
                    extractor1 = HexGrid.Instantiate<Extractor1>(HexGrid.ExtractorGround1);
                    unitLayout.PlaceOnGround(extractor1, engine1, targetCell, HexGrid);
                }
                else
                {
                    extractor1 = HexGrid.Instantiate<Extractor1>(HexGrid.Extractor1);
                    unitLayout.PlacePart(extractor1, engine1, targetCell, HexGrid);
                }
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
                container1 = HexGrid.Instantiate<Container1>(HexGrid.Container1);
                container1.UnitFrame = this;
                SetPlayerColor(container1);

                unitLayout.PlacePart(container1, engine1, targetCell, HexGrid);

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
                assembler1 = HexGrid.Instantiate<Assembler1>(HexGrid.Assembler1);
                assembler1.UnitFrame = this;
                SetPlayerColor(assembler1);

                unitLayout.PlacePart(assembler1, engine1, targetCell, HexGrid);
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
            if (reactor1 == null && stats.ProductionLevel == 1)
            {
                reactor1 = HexGrid.Instantiate<Reactor1>(HexGrid.Reactor1);
                reactor1.UnitFrame = this;
                SetPlayerColor(reactor1);

                unitLayout.PlacePart(reactor1, engine1, targetCell, HexGrid);
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
        }
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
