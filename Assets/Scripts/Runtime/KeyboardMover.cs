using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class KeyboardMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            Vector3 movement = Vector3.zero;

            if (keyboard.wKey.isPressed)
                movement.z += 1f;
            if (keyboard.sKey.isPressed)
                movement.z -= 1f;
            if (keyboard.aKey.isPressed)
                movement.x -= 1f;
            if (keyboard.dKey.isPressed)
                movement.x += 1f;

            if (movement.sqrMagnitude > 0f)
            {
                movement.Normalize();
                transform.position += movement * moveSpeed * Time.deltaTime;
            }
        }
    }
}