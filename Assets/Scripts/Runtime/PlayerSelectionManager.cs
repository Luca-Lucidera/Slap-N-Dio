using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    /// <summary>
    /// Manages player joining in the Select Player Scene.
    /// Keyboard player auto-joins at start. Gamepad players join by pressing A/X.
    /// </summary>
    public class PlayerSelectionManager : MonoBehaviour
    {
        public static PlayerSelectionManager Instance { get; private set; }

        /// <summary>
        /// Event fired when a player joins or leaves a slot.
        /// Parameters: slotIndex, isJoined
        /// </summary>
        public event Action<int, bool> OnPlayerJoinChanged;

        private List<PlayerJoinInfo> joinedPlayers = new List<PlayerJoinInfo>();
        private HashSet<int> joinedGamepadIds = new HashSet<int>();

        private const int MaxGamepadPlayers = 3;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Auto-join keyboard player in slot 0
            JoinKeyboardPlayer();
        }

        private void Update()
        {
            // Check for gamepad button presses to join
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad gamepad)
                {
                    // Skip already joined gamepads
                    if (joinedGamepadIds.Contains(gamepad.deviceId))
                        continue;

                    // Check if A/X button (buttonSouth) was pressed
                    if (gamepad.buttonSouth.wasPressedThisFrame)
                    {
                        TryJoinGamepad(gamepad);
                    }
                }
            }
        }

        private void JoinKeyboardPlayer()
        {
            var keyboardInfo = new PlayerJoinInfo(0, true);
            joinedPlayers.Add(keyboardInfo);
            
            Debug.Log("[PlayerSelectionManager] Keyboard player joined in slot 0");
            OnPlayerJoinChanged?.Invoke(0, true);
        }

        private void TryJoinGamepad(Gamepad gamepad)
        {
            // Check if we have room for more gamepad players
            int gamepadCount = 0;
            foreach (var player in joinedPlayers)
            {
                if (!player.IsKeyboard)
                    gamepadCount++;
            }

            if (gamepadCount >= MaxGamepadPlayers)
            {
                Debug.Log($"[PlayerSelectionManager] Cannot join gamepad {gamepad.deviceId}: max players reached");
                return;
            }

            // Find next available slot (slots 1-3 for gamepads)
            int slotIndex = GetNextAvailableGamepadSlot();
            if (slotIndex < 0)
            {
                Debug.Log($"[PlayerSelectionManager] Cannot join gamepad {gamepad.deviceId}: no slots available");
                return;
            }

            var gamepadInfo = new PlayerJoinInfo(slotIndex, false, gamepad.deviceId);
            joinedPlayers.Add(gamepadInfo);
            joinedGamepadIds.Add(gamepad.deviceId);

            Debug.Log($"[PlayerSelectionManager] Gamepad {gamepad.deviceId} joined in slot {slotIndex}");
            OnPlayerJoinChanged?.Invoke(slotIndex, true);
        }

        private int GetNextAvailableGamepadSlot()
        {
            HashSet<int> usedSlots = new HashSet<int>();
            foreach (var player in joinedPlayers)
            {
                usedSlots.Add(player.SlotIndex);
            }

            // Gamepad slots are 1, 2, 3
            for (int i = 1; i <= 3; i++)
            {
                if (!usedSlots.Contains(i))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Called when starting the game. Transfers joined players to PlayerSessionData.
        /// </summary>
        public void FinalizeSelection()
        {
            if (PlayerSessionData.Instance == null)
            {
                Debug.LogError("[PlayerSelectionManager] PlayerSessionData not found!");
                return;
            }

            PlayerSessionData.Instance.SetJoinedPlayers(joinedPlayers);
            Debug.Log($"[PlayerSelectionManager] Finalized selection with {joinedPlayers.Count} players");
        }

        /// <summary>
        /// Check if a specific slot has a joined player.
        /// </summary>
        public bool IsSlotJoined(int slotIndex)
        {
            foreach (var player in joinedPlayers)
            {
                if (player.SlotIndex == slotIndex)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the number of joined players.
        /// </summary>
        public int GetJoinedPlayerCount()
        {
            return joinedPlayers.Count;
        }
    }
}
