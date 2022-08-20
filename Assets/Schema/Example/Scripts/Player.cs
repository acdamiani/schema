using UnityEngine;

namespace Schema.Example
{
    public class Player : MonoBehaviour
    {
        public float speed = 1f;
        private CharacterController controller;
        private Vector3 v;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            controller.Move(move * Time.deltaTime * speed);

            if (move != Vector3.zero)
                gameObject.transform.forward = move;
        }
    }
}