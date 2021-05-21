using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine1 : MonoBehaviour
{
    public float AboveGround { get; set; }
    public UnitFrame UnitFrame { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        AboveGround = 0.3f;

        //Mesh mesh = Resources.Load<Mesh>("Meshes/Engine1");
        //GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void Update()
    {
        UnitFrame.Move(this);
        if (UnitFrame.NextMove?.MoveType == MoveType.Move) // || UnitFrame.NextMove?.MoveType == MoveType.Add)
        {
            Position FinalDestination = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
            HexCell targetCell = UnitFrame.HexGrid.GroundCells[FinalDestination];

            Vector3 unitPos3 = targetCell.transform.localPosition;
            unitPos3.y += UnitFrame.HexGrid.hexCellHeight + 0.05f;

            float speed = 1.75f / UnitFrame.HexGrid.GameSpeed;
            float step = speed * Time.deltaTime;

            transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);

            if (UnitFrame.NextMove.MoveType == MoveType.Move)
            {
                UpdateDirection(unitPos3);
            }
        }

        /*
        if (UnitFrame.NextMove?.MoveType == MoveType.Extract)
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
        }*/

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
    void UpdateDirection(Vector3 position)
    {
        //float speed = 1.75f;
        float speed = 3.5f / UnitFrame.HexGrid.GameSpeed;

        // Determine which direction to rotate towards
        Vector3 targetDirection = position - transform.position;

        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate the forward vector towards the target direction by one step
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        // Draw a ray pointing at our target in
        Debug.DrawRay(transform.position, newDirection, Color.red);

        // Calculate a rotation a step closer to the target and applies rotation to this object
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

}
