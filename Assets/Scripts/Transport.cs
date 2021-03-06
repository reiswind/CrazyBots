using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts
{
    public class Transport : MonoBehaviour
    {
        internal Vector3 TargetPosition { get; set; }


        // Update is called once per frame
        void Update()
        {
            if (TargetPosition != null)
            {
                float speed = 2.75f / HexGrid.MainGrid.GameSpeed;
                float step = speed * Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
                if (transform.position == TargetPosition)
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}