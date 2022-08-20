using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Schema.Example
{
    public class Player : MonoBehaviour
    {
        private CharacterController controller;
        private Vector3 v;
        public float speed = 1f;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            controller.Move(move * Time.deltaTime * speed);

            if (move != Vector3.zero)
                gameObject.transform.forward = move;
        }
    }
}