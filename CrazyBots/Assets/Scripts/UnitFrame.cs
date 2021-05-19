using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UnitPart : MonoBehaviour
{
    public float AboveGround { get; set; }
}
public class UnitFrame
{

    public Position FinalDestination { get; set; }
    public Move NextMove { get; set; }
    public HexGrid HexGrid { get; set; }

    internal UnitPart currentBaseFrame;

    private Engine1 engine1;
    private Container1 container1;

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

        bool updatePosition = false;

        if (NextMove.Stats.EngineLevel > 0)
        {
            if (engine1 == null)
            {
                engine1 = HexGrid.Instantiate<Engine1>(HexGrid.Engine1);
                engine1.UnitFrame = this;

                currentBaseFrame = engine1;
                currentBaseFrame.transform.SetParent(HexGrid.transform, false);
                updatePosition = true;
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
                    container1.AboveGround = 1.75f;

                    currentBaseFrame = container1;
                    currentBaseFrame.transform.SetParent(HexGrid.transform, false);
                    updatePosition = true;
                }
                else
                {
                    Vector3 unitPos3 = new Vector3();
                    unitPos3.x = 0;
                    unitPos3.z = 0;
                    unitPos3.y = 1.65f; // 
                    container1.transform.position = unitPos3;

                    container1.transform.SetParent(currentBaseFrame.transform, false);
                }
            }
        }

        if (currentBaseFrame != null && updatePosition)
        {
            Position pos = NextMove.Positions[NextMove.Positions.Count - 1];
            HexCell targetCell = HexGrid.GroundCells[pos];
            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y -= 1;
            currentBaseFrame.transform.position = unitPos3;
        }
    }

    public void JumpToTarget(Position pos)
    {
        if (FinalDestination != null)
        {
            if (currentBaseFrame != null)
            {
                // Did not reach target in time. Jump to it.
                HexCell targetCell = HexGrid.GroundCells[pos];

                Vector3 unitPos3 = targetCell.transform.localPosition;
                unitPos3.y += currentBaseFrame.AboveGround;
                currentBaseFrame.transform.position = unitPos3;

                currentBaseFrame.transform.LookAt(unitPos3);
            }
            FinalDestination = null;
        }
    }

    public void UpdateMove(UnitPart unit)
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
            unitPos3.y += unit.AboveGround;

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
