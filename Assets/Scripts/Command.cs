using Engine.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Command : MonoBehaviour
    {
        public GameCommand GameCommand { get; set; }

        private void Update()
        {
            transform.Rotate(Vector3.up); // * Time.deltaTime);
        }
    }
}