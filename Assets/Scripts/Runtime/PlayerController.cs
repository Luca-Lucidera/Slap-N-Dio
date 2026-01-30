using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float deadzone = 0.1f;

        private Gamepad assignedGamepad;
        private bool useKeyboard;

        public void Initialize(Gamepad gamepad)
        {
            assignedGamepad = gamepad;
            useKeyboard = (gamepad == null);
        }

        private void Update()
        {
            Vector3 movement = Vector3.zero;

            if (useKeyboard)
            {
                movement = GetKeyboardInput();
            }
            else if (assignedGamepad != null)
            {
                movement = GetGamepadInput();
            }

            if (movement.sqrMagnitude > 0f)
            {
                movement.Normalize();
                transform.position += movement * moveSpeed * Time.deltaTime;
            }
        }

        private Vector3 GetKeyboardInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector3.zero;

            Vector3 movement = Vector3.zero;

            if (keyboard.wKey.isPressed)
                movement.z += 1f;
            if (keyboard.sKey.isPressed)
                movement.z -= 1f;
            if (keyboard.aKey.isPressed)
                movement.x -= 1f;
            if (keyboard.dKey.isPressed)
                movement.x += 1f;

            return movement;
        }

        private Vector3 GetGamepadInput()
        {
            if (assignedGamepad == null) return Vector3.zero;

            Vector2 stick = assignedGamepad.leftStick.ReadValue();

            if (stick.sqrMagnitude > deadzone * deadzone)
            {
                return new Vector3(stick.x, 0f, stick.y);
            }

            return Vector3.zero;
        }
    }
}
