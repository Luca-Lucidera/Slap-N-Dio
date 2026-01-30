using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class GamepadMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float deadzone = 0.1f;

        private void Update()
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;

            Vector2 stick = gamepad.leftStick.ReadValue();

            if (stick.sqrMagnitude > deadzone * deadzone)
            {
                Vector3 movement = new Vector3(stick.x, 0f, stick.y);
                transform.position += movement * moveSpeed * Time.deltaTime;
            }
        }
    }
}