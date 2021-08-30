using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    

    public class Container1 : MonoBehaviour
    {
        public void Transport(HexGrid hexGrid, Move move)
        {
            Vector3 launchPosition;
            launchPosition = transform.position;
            launchPosition.y += 1;

            GameObject shellprefab = hexGrid.GetUnitResource("Transport");

            GameObject shellObject = Instantiate(shellprefab);
            Transport transport = shellObject.GetComponent<Transport>();

            Position pos = move.Positions[move.Positions.Count - 1];

            transport.HexGrid = hexGrid;

            Vector3 targetPosition;

            targetPosition = hexGrid.GroundCells[pos].transform.position;
            targetPosition.y += 1;

            transport.TargetPosition = targetPosition;

            transport.gameObject.hideFlags = HideFlags.HideAndDontSave;
            //transport.transform.SetPositionAndRotation(launchPosition, transform.rotation);
            //transport.transform.position = launchPosition;

            Vector3 newDirection = Vector3.RotateTowards(launchPosition, targetPosition, 360, 360);
            //transform.rotation = Quaternion.LookRotation(newDirection);

            transport.transform.SetPositionAndRotation(launchPosition, Quaternion.LookRotation(newDirection));


            Destroy(shellObject, 5f);
        }
    }
}