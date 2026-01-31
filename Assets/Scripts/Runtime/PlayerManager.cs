using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class PlayerManager : MonoBehaviour
    {
        public event Action<int, int> OnPlayerLivesChanged; // (slotIndex, livesRemaining)
        public event Action<int> OnPlayerEliminated; // (slotIndex)

        [Header("Player Materials")]
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material greenMaterial;
        [SerializeField] private Material yellowMaterial;

        [Header("Player Prefab")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Disconnect UI")]
        [SerializeField] private GameObject disconnectOverlay;

        private static readonly Vector3[] SpawnPositions = new Vector3[]
        {
            new Vector3(-3f, 0.5f, 0f),
            new Vector3(-1f, 0.5f, 0f),
            new Vector3(1f, 0.5f, 0f),
            new Vector3(3f, 0.5f, 0f)
        };

        private GameObject keyboardPlayer;
        private Dictionary<int, GameObject> gamepadPlayers = new Dictionary<int, GameObject>();
        private Dictionary<int, int> deviceIdToSlot = new Dictionary<int, int>();
        private HashSet<Transform> deadPlayers = new HashSet<Transform>();
        private HashSet<int> registeredGamepadIds = new HashSet<int>();
        private bool isPausedForDisconnect = false;

        // Sistema vite
        private const int MaxLives = 3;
        private Dictionary<Transform, int> playerLives = new Dictionary<Transform, int>();
        private Dictionary<Transform, int> playerSlotIndex = new Dictionary<Transform, int>();
        private HashSet<Transform> eliminatedPlayers = new HashSet<Transform>();

        private void OnEnable()
        {
            InputSystem.onDeviceChange += HandleDeviceChange;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= HandleDeviceChange;
        }

        private void Start()
        {
            SpawnPlayersFromSessionData();
        }

        private void SpawnPlayersFromSessionData()
        {
            if (PlayerSessionData.Instance == null)
            {
                Debug.LogWarning("[PlayerManager] PlayerSessionData not found. Spawning default keyboard player.");
                SpawnKeyboardPlayer(0);
                return;
            }

            var joinedPlayers = PlayerSessionData.Instance.JoinedPlayers;
            if (joinedPlayers.Count == 0)
            {
                Debug.LogWarning("[PlayerManager] No joined players in session data. Spawning default keyboard player.");
                SpawnKeyboardPlayer(0);
                return;
            }

            foreach (var playerInfo in joinedPlayers)
            {
                if (playerInfo.IsKeyboard)
                {
                    SpawnKeyboardPlayer(playerInfo.SlotIndex);
                }
                else
                {
                    // Find the gamepad by device ID
                    Gamepad gamepad = FindGamepadByDeviceId(playerInfo.GamepadDeviceId);
                    if (gamepad != null)
                    {
                        SpawnGamepadPlayer(gamepad, playerInfo.SlotIndex);
                        registeredGamepadIds.Add(playerInfo.GamepadDeviceId);
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerManager] Gamepad {playerInfo.GamepadDeviceId} not found for slot {playerInfo.SlotIndex}");
                    }
                }
            }

            Debug.Log($"[PlayerManager] Spawned {joinedPlayers.Count} players from session data");
        }

        private Gamepad FindGamepadByDeviceId(int deviceId)
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad gamepad && gamepad.deviceId == deviceId)
                {
                    return gamepad;
                }
            }
            return null;
        }

        private void SpawnKeyboardPlayer(int slotIndex)
        {
            keyboardPlayer = CreatePlayerCube($"Player{slotIndex + 1}_Keyboard", SpawnPositions[slotIndex], GetMaterialForSlot(slotIndex));
            var controller = keyboardPlayer.AddComponent<PlayerController>();
            controller.Initialize(null);

            // Registra vite e slot
            RegisterPlayerLives(keyboardPlayer.transform, slotIndex);

            Debug.Log($"[PlayerManager] Spawned keyboard player in slot {slotIndex}");
        }

        private void SpawnGamepadPlayer(Gamepad gamepad, int slotIndex)
        {
            Material material = GetMaterialForSlot(slotIndex);
            Vector3 position = SpawnPositions[slotIndex];

            string playerName = $"Player{slotIndex + 1}_Gamepad";
            GameObject player = CreatePlayerCube(playerName, position, material);
            var controller = player.AddComponent<PlayerController>();
            controller.Initialize(gamepad);

            gamepadPlayers[gamepad.deviceId] = player;
            deviceIdToSlot[gamepad.deviceId] = slotIndex;

            // Registra vite e slot
            RegisterPlayerLives(player.transform, slotIndex);

            Debug.Log($"[PlayerManager] Spawned {playerName} for gamepad {gamepad.deviceId}");
        }

        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is Gamepad gamepad)) return;

            switch (change)
            {
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    HandleGamepadDisconnected(gamepad);
                    break;

                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    HandleGamepadReconnected(gamepad);
                    break;
            }
        }

        private void HandleGamepadDisconnected(Gamepad gamepad)
        {
            // Only pause if this was a registered player's gamepad
            if (!registeredGamepadIds.Contains(gamepad.deviceId)) return;

            Debug.Log($"[PlayerManager] Registered gamepad {gamepad.deviceId} disconnected. Pausing game.");
            PauseForDisconnect();
        }

        private void HandleGamepadReconnected(Gamepad gamepad)
        {
            // Check if this is a registered gamepad that reconnected
            if (!registeredGamepadIds.Contains(gamepad.deviceId)) return;

            Debug.Log($"[PlayerManager] Registered gamepad {gamepad.deviceId} reconnected. Resuming game.");
            ResumeFromDisconnect();
        }

        private void PauseForDisconnect()
        {
            if (isPausedForDisconnect) return;

            isPausedForDisconnect = true;
            Time.timeScale = 0f;

            if (disconnectOverlay != null)
            {
                disconnectOverlay.SetActive(true);
            }
        }

        private void ResumeFromDisconnect()
        {
            if (!isPausedForDisconnect) return;

            // Check if all registered gamepads are connected
            foreach (int deviceId in registeredGamepadIds)
            {
                if (FindGamepadByDeviceId(deviceId) == null)
                {
                    Debug.Log($"[PlayerManager] Still waiting for gamepad {deviceId} to reconnect.");
                    return;
                }
            }

            isPausedForDisconnect = false;
            Time.timeScale = 1f;

            if (disconnectOverlay != null)
            {
                disconnectOverlay.SetActive(false);
            }
        }

        private Material GetMaterialForSlot(int slotIndex)
        {
            return slotIndex switch
            {
                0 => blueMaterial,
                1 => redMaterial,
                2 => greenMaterial,
                3 => yellowMaterial,
                _ => blueMaterial
            };
        }

        private GameObject CreatePlayerCube(string name, Vector3 position, Material material)
        {
            GameObject cube = Instantiate(playerPrefab, position, Quaternion.identity);
            cube.name = name;
            cube.layer = LayerMask.NameToLayer("Player");

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }

            return cube;
        }

        public List<Transform> GetActivePlayerTransforms()
        {
            List<Transform> transforms = new List<Transform>();

            if (keyboardPlayer != null && !deadPlayers.Contains(keyboardPlayer.transform))
                transforms.Add(keyboardPlayer.transform);

            foreach (var kvp in gamepadPlayers)
            {
                if (kvp.Value != null && !deadPlayers.Contains(kvp.Value.transform))
                    transforms.Add(kvp.Value.transform);
            }

            return transforms;
        }

        public void MarkPlayerAsDead(Transform player)
        {
            deadPlayers.Add(player);
        }

        public void MarkPlayerAsAlive(Transform player)
        {
            deadPlayers.Remove(player);
        }

        private void RegisterPlayerLives(Transform player, int slotIndex)
        {
            playerLives[player] = MaxLives;
            playerSlotIndex[player] = slotIndex;
        }

        /// <summary>
        /// Decrementa una vita al player.
        /// </summary>
        /// <returns>true se può respawnare, false se eliminato definitivamente</returns>
        public bool DecrementLife(Transform player)
        {
            if (eliminatedPlayers.Contains(player))
            {
                return false;
            }

            if (!playerLives.ContainsKey(player))
            {
                Debug.LogWarning($"[PlayerManager] Player {player.name} not registered in lives system");
                return true;
            }

            playerLives[player]--;
            int livesRemaining = playerLives[player];
            int slotIndex = playerSlotIndex[player];

            OnPlayerLivesChanged?.Invoke(slotIndex, livesRemaining);

            if (livesRemaining <= 0)
            {
                eliminatedPlayers.Add(player);
                OnPlayerEliminated?.Invoke(slotIndex);
                Debug.Log($"[PlayerManager] Player {player.name} eliminated!");
                return false;
            }

            Debug.Log($"[PlayerManager] Player {player.name} lost a life. Remaining: {livesRemaining}");
            return true;
        }

        public int GetSlotIndex(Transform player)
        {
            if (playerSlotIndex.TryGetValue(player, out int slot))
            {
                return slot;
            }
            return -1;
        }

        public int GetLives(Transform player)
        {
            if (playerLives.TryGetValue(player, out int lives))
            {
                return lives;
            }
            return 0;
        }

        public bool IsPlayerEliminated(Transform player)
        {
            return eliminatedPlayers.Contains(player);
        }
    }
}
