using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine1 : UnitPart
{
    // Start is called before the first frame update
    void Awake()
    {
        AboveGround = 0.3f;

        //Mesh mesh = Resources.Load<Mesh>("Meshes/Engine1");
        //GetComponent<MeshFilter>().sharedMesh = mesh;
    }



    internal UnitFrame UnitFrame { get; set; }

    

    void Update()
    {
        if (UnitFrame.currentBaseFrame == this)
        {
            if (UnitFrame.NextMove != null && UnitFrame.NextMove.MoveType == MoveType.Extract)
            {
                //if (extractSource == null)
                {
                    Position from = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
                    HexCell targetCell = UnitFrame.HexGrid.GroundCells[from];

                    ParticleSystem extractSourcePrefab = Resources.Load<ParticleSystem>("ExtractSource");
                    ParticleSystem extractSource;
                    extractSource = Instantiate(extractSourcePrefab, targetCell.transform, false);

                    Destroy(extractSource, 0.5f);

                    Vector3 pos = new Vector3();
                    //pos.y = 0.3f;
                    extractSource.transform.localPosition = pos;


                    ParticleSystemForceField extractTargetPrefab = Resources.Load<ParticleSystemForceField>("ExtractTarget");
                    ParticleSystemForceField extractTarget = Instantiate(extractTargetPrefab, transform, false);

                    extractSource.externalForces.SetInfluence(0, extractTarget);
                    Destroy(extractTarget, 0.5f);

                    pos = new Vector3();
                    //pos.y = 0.3f;
                    extractTarget.transform.localPosition = pos;
                }
                UnitFrame.NextMove = null;
            }
            else
            {
                UnitFrame.UpdateMove(this);
            }
        }
        /*
        if (UnitFrame.NextMove == null)
            return;
        if (UnitFrame.NextMove.MoveType == MoveType.Delete)
        {
            
        }
        else if (UnitFrame.NextMove.MoveType == MoveType.Move || UnitFrame.NextMove.MoveType == MoveType.Add)
        {
            UnitFrame.FinalDestination = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[UnitFrame.FinalDestination];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y += AboveGround;

            float speed = 1.75f;
            float step = speed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);

            if (UnitFrame.NextMove.MoveType == MoveType.Move)
            {
                // Nah...
                //transform.position = Vector3.RotateTowards(transform.position, unitPos3, step, 1);
                transform.LookAt(unitPos3);
            }
        }
        */
    }

    public void JumpToTarget(Position pos)
    {
        //if (FinalDestination != null)
        {
            // Did not reach target in time. Jump to it.
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[pos];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            //unitPos3.y = 3; //-= AboveGround;
            unitPos3.y += AboveGround;
            transform.position = unitPos3;
            //FinalDestination = null;
        }
    }

}
