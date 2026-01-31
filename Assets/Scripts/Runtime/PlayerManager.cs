using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Materials")]
        [SerializeField] private Material blueMaterial;
        [SerializeField] private Material redMaterial;
        [SerializeField] private Material greenMaterial;
        [SerializeField] private Material yellowMaterial;

        [Header("Player Prefab")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 5f;

        private static readonly Vector3[] SpawnPositions = new Vector3[]
        {
            new Vector3(-3f, 0.5f, 0f),
            new Vector3(-1f, 0.5f, 0f),
            new Vector3(1f, 0.5f, 0f),
            new Vector3(3f, 0.5f, 0f)
        };

        private GameObject keyboardPlayer;
        private Dictionary<int, GameObject> gamepadPlayers = new Dictionary<int, GameObject>();
        private List<int> gamepadSlots = new List<int>();

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
            SpawnKeyboardPlayer();
            
            foreach (var device in InputSystem.devices)
            {
                if (device is Gamepad gamepad)
                {
                    TrySpawnGamepadPlayer(gamepad);
                }
            }
        }

        private void SpawnKeyboardPlayer()
        {
            keyboardPlayer = CreatePlayerCube("Player1_Keyboard", SpawnPositions[0], blueMaterial);
            var controller = keyboardPlayer.AddComponent<PlayerController>();
            controller.Initialize(null);
        }

        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is Gamepad gamepad)) return;

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    TrySpawnGamepadPlayer(gamepad);
                    break;

                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    RemoveGamepadPlayer(gamepad);
                    break;
            }
        }

        private void TrySpawnGamepadPlayer(Gamepad gamepad)
        {
            if (gamepadPlayers.ContainsKey(gamepad.deviceId)) return;
            if (gamepadSlots.Count >= 3) return;

            int slotIndex = GetNextAvailableSlot();
            if (slotIndex < 0) return;

            int playerIndex = slotIndex + 1;
            Material material = GetMaterialForSlot(slotIndex);
            Vector3 position = SpawnPositions[playerIndex];

            string playerName = $"Player{playerIndex + 1}_Gamepad{slotIndex + 1}";
            GameObject player = CreatePlayerCube(playerName, position, material);
            var controller = player.AddComponent<PlayerController>();
            controller.Initialize(gamepad);

            gamepadPlayers[gamepad.deviceId] = player;
            gamepadSlots.Add(slotIndex);

            Debug.Log($"[PlayerManager] Spawned {playerName} for gamepad {gamepad.deviceId}");
        }

        private void RemoveGamepadPlayer(Gamepad gamepad)
        {
            if (!gamepadPlayers.TryGetValue(gamepad.deviceId, out GameObject player)) return;

            int slotIndex = gamepadSlots.FindIndex(s => gamepadPlayers[gamepad.deviceId] == player);
            
            gamepadPlayers.Remove(gamepad.deviceId);
            
            for (int i = 0; i < gamepadSlots.Count; i++)
            {
                if (gamepadPlayers.ContainsKey(gamepad.deviceId) == false)
                {
                    var keys = new List<int>(gamepadPlayers.Keys);
                    bool found = false;
                    foreach (var key in keys)
                    {
                        if (gamepadPlayers[key] == player)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found && slotIndex >= 0)
                    {
                        gamepadSlots.Remove(slotIndex);
                        break;
                    }
                }
            }

            if (player != null)
            {
                Debug.Log($"[PlayerManager] Removed player for gamepad {gamepad.deviceId}");
                Destroy(player);
            }
        }

        private int GetNextAvailableSlot()
        {
            for (int i = 0; i < 3; i++)
            {
                if (!gamepadSlots.Contains(i))
                    return i;
            }
            return -1;
        }

        private Material GetMaterialForSlot(int slotIndex)
        {
            return slotIndex switch
            {
                0 => redMaterial,
                1 => greenMaterial,
                2 => yellowMaterial,
                _ => redMaterial
            };
        }

        private GameObject CreatePlayerCube(string name, Vector3 position, Material material)
        {
            GameObject cube = Instantiate(playerPrefab, position, Quaternion.identity);
            cube.name = name;

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }

            return cube;
        }
    }
}
