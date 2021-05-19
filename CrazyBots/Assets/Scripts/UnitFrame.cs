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

public class UnitFrame
{
  
    public Position FinalDestination { get; set; }
    public Move NextMove { get; set; }
    public HexGrid HexGrid { get; set; }

    internal MonoBehaviour currentBaseFrame;

    private Engine1 engine1;
    private Container1 container1;
    private Assembler1 assembler1;

    private ParticleSystem particleSource;

    private void Upgrade()
    {
        if (particleSource == null)
        {
            Position from = NextMove.Positions[0];
            HexCell sourceCell = HexGrid.GroundCells[from];

            particleSource = HexGrid.MakeParticleSource();
            particleSource.transform.SetParent(sourceCell.transform, false);
        }

        Position to = NextMove.Positions[1];
        HexCell targetCell = HexGrid.GroundCells[to];

        ParticleSystemForceField particleTarget = HexGrid.MakeParticleTarget();
        particleTarget.transform.SetParent(targetCell.transform, false);

        particleSource.externalForces.SetInfluence(0, particleTarget);
        HexGrid.Destroy(particleTarget, 2.5f);

        particleSource.Play();

        NextMove = null;
    }

    public void Assemble()
    {

        if (NextMove == null || NextMove.Stats == null)
            return;

        Position pos = NextMove.Positions[NextMove.Positions.Count - 1];
        HexCell targetCell = HexGrid.GroundCells[pos];

        bool updatePosition = false;

        if (NextMove.Stats.EngineLevel > 0)
        {
            if (engine1 == null)
            {
                engine1 = HexGrid.Instantiate<Engine1>(HexGrid.Engine1);
                engine1.UnitFrame = this;

                engine1.AboveGround = HexGrid.HexCellHeight + 0.1f;

                currentBaseFrame = engine1;
                currentBaseFrame.transform.SetParent(targetCell.transform, false);

                Vector3 unitPos3 = engine1.transform.position;
                unitPos3.y += HexGrid.HexCellHeight;
                engine1.transform.position = unitPos3;
            }
        }

        if (NextMove.Stats.ContainerLevel > 0)
        {
            if (container1 == null)
            {
                container1 = HexGrid.Instantiate<Container1>(HexGrid.Container1);
                container1.UnitFrame = this;

                if (currentBaseFrame == null)
                {
                    container1.AboveGround = HexGrid.HexCellHeight + 0.1f;

                    currentBaseFrame = container1;
                    currentBaseFrame.transform.SetParent(targetCell.transform, false);

                    Vector3 unitPos3 = container1.transform.position;
                    unitPos3.y += HexGrid.HexCellHeight;
                    container1.transform.position = unitPos3;
                }
                else
                {
                    Vector3 unitPos3 = new Vector3();
                    unitPos3.x = 0.3f; // right
                    unitPos3.x = 0.0f; // left
                    unitPos3.z = 0.15f; // middle
                    unitPos3.z = 0.45f; // front
                    unitPos3.z = -0.05f; // rear

                    unitPos3.y = 0.1f; // 
                    container1.transform.position = unitPos3;

                    container1.transform.SetParent(currentBaseFrame.transform, false);
                }
            }
        }


        if (currentBaseFrame != null && updatePosition)
        {
            //Vector3 unitPos3 = targetCell.transform.localPosition;
            //unitPos3.y += 2;
            //currentBaseFrame.transform.position = unitPos3;
        }

        if (NextMove.Stats.ProductionLevel > 0)
        {
            if (assembler1 == null && currentBaseFrame != null)
            {
                assembler1 = HexGrid.Instantiate<Assembler1>(HexGrid.Assembler1);
                assembler1.UnitFrame = this;
                assembler1.transform.SetParent(targetCell.transform, false);

                Vector3 unitPos3 = assembler1.transform.position; // new Vector3(); // targetCell.transform.localPosition;
                unitPos3.x += 0.3f;
                //unitPos3.z = 0;
                unitPos3.y += HexGrid.HexCellHeight; // 
                assembler1.transform.position = unitPos3;


            }
        }


    }

    public void JumpToTarget(Position pos)
    {
        return;

        if (FinalDestination != null)
        {
            if (currentBaseFrame != null)
            {
                // Did not reach target in time. Jump to it.
                HexCell targetCell = HexGrid.GroundCells[pos];

                Vector3 unitPos3 = targetCell.transform.localPosition;
                //unitPos3.y += currentBaseFrame.AboveGround;
                currentBaseFrame.transform.position = unitPos3;

                currentBaseFrame.transform.LookAt(unitPos3);
            }
            FinalDestination = null;
        }
    }

    public void UpdateMove(MonoBehaviour unit, float aboveGround)
    {
        if (NextMove == null)
            return;

        if (unit != currentBaseFrame)
        {
            // Subparts shall not move
            return;
        }

        if (NextMove.MoveType == MoveType.Delete)
        {

        }
        else if (NextMove.MoveType == MoveType.Upgrade)
        {
            Upgrade();
        }
        else if (NextMove.MoveType == MoveType.UpdateStats)
        {
            Assemble();
        }
        else if (NextMove.MoveType == MoveType.Move || NextMove.MoveType == MoveType.Add)
        {
            FinalDestination = NextMove.Positions[NextMove.Positions.Count - 1];
            HexCell targetCell = HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y += aboveGround;

            float speed = 1.75f / HexGrid.GameSpeed;
            float step = speed * Time.deltaTime;

            unit.transform.position = Vector3.MoveTowards(unit.transform.position, unitPos3, step);

            if (NextMove.MoveType == MoveType.Move)
            {
                UpdateDirection(unitPos3);

                // Nah...
                //unit.transform.position = Vector3.RotateTowards(unit.transform.position, unitPos3, 1, step);
                //unit.transform.LookAt(unitPos3);
            }
        }
    }

    void UpdateDirection(Vector3 position)
    {
        //float speed = 1.75f;
        float speed = 3.5f / HexGrid.GameSpeed;

        // Determine which direction to rotate towards
        Vector3 targetDirection = position - currentBaseFrame.transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(currentBaseFrame.transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(currentBaseFrame.transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        currentBaseFrame.transform.rotation = Quaternion.LookRotation(newDirection);
    }

}
