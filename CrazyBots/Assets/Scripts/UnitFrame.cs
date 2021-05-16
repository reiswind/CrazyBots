using Engine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{

    public class UnitFrame : MonoBehaviour
    {
        public int X;
        public int Z;

        public Move NextMove { get; set; }
        public HexGrid HexGrid { get; set; }
        private Position finalDestination;
        void Update()
        {
            if (NextMove != null)
            {
                finalDestination = NextMove.Positions[1];
                HexCell targetCell = HexGrid.GroundCells[finalDestination];

                Vector3 unitPos3 = targetCell.transform.localPosition;
                unitPos3.y += 0.4f;
                //transform.localPosition = unitPos3;

                float speed = 1.75f;
                float step = speed * Time.deltaTime;
                //transform.position = Vector3.MoveTowards(transform.position, targetCell.transform.localPosition, step);
                transform.position = Vector3.MoveTowards(transform.position, unitPos3, step);

                //transform.m
                transform.LookAt(targetCell.transform);

                //NextMove = null;
            }
        }

        public void JumpToTarget()
        { 
            if (finalDestination != null)
            {
                // Did not reach target in time. Jump to it.
                HexCell targetCell = HexGrid.GroundCells[finalDestination];

                Vector3 unitPos3 = targetCell.transform.localPosition;
                unitPos3.y += 0.4f;
                transform.position = unitPos3;
                finalDestination = null;
            }
        }

    }
}
