using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extractor1 : MonoBehaviour
{
    public UnitFrame UnitFrame { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UnitFrame.Move(this);
        if (UnitFrame.NextMove?.MoveType == MoveType.Extract)
        {
            //if (extractSource == null)
            {
                Position from = UnitFrame.NextMove.Positions[UnitFrame.NextMove.Positions.Count - 1];
                HexCell targetCell = UnitFrame.HexGrid.GroundCells[from];

                ParticleSystem extractSourcePrefab = Resources.Load<ParticleSystem>("ExtractSource");
                ParticleSystem extractSource;
                extractSource = Instantiate(extractSourcePrefab, targetCell.transform, false);
                extractSource.transform.SetParent(targetCell.transform, false);

                Destroy(extractSource, 0.5f);

                Vector3 unitPos3 = targetCell.transform.position;
                unitPos3.y += 0.3f; // hexGrid.hexCellHeight;

                //pos.y = 0.3f;
                //extractSource.transform.localPosition = unitPos3;


                ParticleSystemForceField extractTargetPrefab = Resources.Load<ParticleSystemForceField>("ExtractTarget");
                ParticleSystemForceField extractTarget = Instantiate(extractTargetPrefab, transform, false);

                extractSource.externalForces.SetInfluence(0, extractTarget);
                extractTarget.transform.SetParent(targetCell.transform, false);

                Destroy(extractTarget, 0.5f);

                unitPos3 = new Vector3();
                //pos.y = 0.3f;
                //extractTarget.transform.localPosition = unitPos3;
            }
            UnitFrame.NextMove = null;
        }
        
    }
}
