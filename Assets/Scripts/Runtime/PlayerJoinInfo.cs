using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    /// <summary>
    /// Data structure to hold player join information between scenes.
    /// </summary>
    public class PlayerJoinInfo
    {
        public int SlotIndex { get; set; }
        public int GamepadDeviceId { get; set; }
        public bool IsKeyboard { get; set; }

        public PlayerJoinInfo(int slotIndex, bool isKeyboard, int gamepadDeviceId = -1)
        {
            SlotIndex = slotIndex;
            IsKeyboard = isKeyboard;
            GamepadDeviceId = gamepadDeviceId;
        }
    }
}
