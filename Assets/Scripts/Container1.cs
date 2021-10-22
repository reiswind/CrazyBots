using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    

    public class Container1 : MonoBehaviour
    {
        public void Transport(Move move)
        {
            Vector3 launchPosition;
            launchPosition = transform.position;
            launchPosition.y += 1;

            GameObject shellprefab = HexGrid.MainGrid.GetResource("Transport");

            GameObject shellObject = Instantiate(shellprefab);
            Transport transport = shellObject.GetComponent<Transport>();

            ulong pos = move.Positions[move.Positions.Count - 1];

            Vector3 targetPosition;

            targetPosition = HexGrid.MainGrid.GroundCells[pos].transform.position;
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