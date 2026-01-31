using System.Collections.Generic;
using Assets.Scripts.Runtime.PlayerPowerUps;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.Runtime.UI
{
    [System.Serializable]
    public class PlayerHUDSlot
    {
        public GameObject playerBox;
        public CanvasGroup canvasGroup;
        public Image[] lifeImages;
        public TextMeshProUGUI powerUpText;
    }

    public class HUDManager : MonoBehaviour
    {
        [Header("Player HUD Slots")]
        [SerializeField] private PlayerHUDSlot[] playerSlots = new PlayerHUDSlot[4];

        [Header("Auto-Find Settings")]
        [SerializeField] private bool autoFindReferences = true;
        [SerializeField] private Transform panelTransform;

        [Header("Delay Settings")]
        [SerializeField] private float playerSearchDelay = 0.5f;

        private PlayerManager playerManager;
        private Dictionary<PlayerPowerUpController, int> powerUpControllerToSlot = new Dictionary<PlayerPowerUpController, int>();

        private void Awake()
        {
            if (autoFindReferences)
            {
                AutoFindReferences();
            }
        }

        private void Start()
        {
            playerManager = FindFirstObjectByType<PlayerManager>();

            if (playerManager != null)
            {
                playerManager.OnPlayerLivesChanged += HandlePlayerLivesChanged;
                playerManager.OnPlayerEliminated += HandlePlayerEliminated;
            }

            InitializeHUD();

            // Cerca i player dopo un breve delay per permettere lo spawn
            Invoke(nameof(RegisterPowerUpControllers), playerSearchDelay);
        }

        private void OnDestroy()
        {
            if (playerManager != null)
            {
                playerManager.OnPlayerLivesChanged -= HandlePlayerLivesChanged;
                playerManager.OnPlayerEliminated -= HandlePlayerEliminated;
            }

            UnregisterPowerUpControllers();
        }

        private void AutoFindReferences()
        {
            // Trova il Panel se non assegnato
            if (panelTransform == null)
            {
                panelTransform = transform.Find("Panel");
            }

            if (panelTransform == null)
            {
                Debug.LogWarning("[HUDManager] Panel not found!");
                return;
            }

            // Inizializza gli slot se necessario
            if (playerSlots == null || playerSlots.Length < 4)
            {
                playerSlots = new PlayerHUDSlot[4];
            }

            // Trova i Player Box per nome
            string[] boxNames = { "Player 1 Box", "Player 2 Box", "Player 3 Box", "Player 4 Box" };

            for (int i = 0; i < 4; i++)
            {
                if (playerSlots[i] == null)
                {
                    playerSlots[i] = new PlayerHUDSlot();
                }

                Transform boxTransform = panelTransform.Find(boxNames[i]);
                if (boxTransform == null)
                {
                    Debug.LogWarning($"[HUDManager] {boxNames[i]} not found!");
                    continue;
                }

                playerSlots[i].playerBox = boxTransform.gameObject;
                playerSlots[i].canvasGroup = boxTransform.GetComponent<CanvasGroup>();

                // Trova Nome abilità (powerUpText)
                Transform nomeAbilita = boxTransform.Find("Nome abilità");
                if (nomeAbilita != null)
                {
                    playerSlots[i].powerUpText = nomeAbilita.GetComponent<TextMeshProUGUI>();
                }

                // Trova le vite
                playerSlots[i].lifeImages = new Image[3];
                for (int j = 0; j < 3; j++)
                {
                    Transform lifeTransform = boxTransform.Find($"Life {j + 1}");
                    if (lifeTransform != null)
                    {
                        playerSlots[i].lifeImages[j] = lifeTransform.GetComponent<Image>();
                    }
                }

                Debug.Log($"[HUDManager] Auto-found references for {boxNames[i]}");
            }
        }

        private void RegisterPowerUpControllers()
        {
            var controllers = FindObjectsByType<PlayerPowerUpController>(FindObjectsSortMode.None);
            foreach (var controller in controllers)
            {
                var playerController = controller.GetComponent<PlayerController>();
                if (playerController != null && playerManager != null)
                {
                    int slotIndex = playerManager.GetSlotIndex(controller.transform);
                    if (slotIndex >= 0)
                    {
                        powerUpControllerToSlot[controller] = slotIndex;
                        controller.OnPowerUpTextChanged += (text) => HandlePowerUpTextChanged(slotIndex, text);
                        Debug.Log($"[HUDManager] Registered PowerUpController for slot {slotIndex}");
                    }
                }
            }
        }

        private void UnregisterPowerUpControllers()
        {
            powerUpControllerToSlot.Clear();
        }

        private void HandlePowerUpTextChanged(int slotIndex, string powerUpName)
        {
            if (string.IsNullOrEmpty(powerUpName))
            {
                ClearPowerUpText(slotIndex);
            }
            else
            {
                SetPowerUpText(slotIndex, powerUpName);
            }
        }

        private void InitializeHUD()
        {
            int playerCount = 2; // Default minimo

            if (PlayerSessionData.Instance != null && PlayerSessionData.Instance.JoinedPlayers.Count > 0)
            {
                playerCount = PlayerSessionData.Instance.JoinedPlayers.Count;
            }

            // Mostra solo i box per i giocatori attivi
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (playerSlots[i] == null) continue;

                if (playerSlots[i].playerBox != null)
                {
                    playerSlots[i].playerBox.SetActive(i < playerCount);
                }

                // Inizializza powerup text vuoto
                if (playerSlots[i].powerUpText != null)
                {
                    playerSlots[i].powerUpText.text = "";
                }

                // Reset alpha del CanvasGroup
                if (playerSlots[i].canvasGroup != null)
                {
                    playerSlots[i].canvasGroup.alpha = 1f;
                }
            }

            Debug.Log($"[HUDManager] Initialized HUD for {playerCount} players");
        }

        private void HandlePlayerLivesChanged(int slotIndex, int livesRemaining)
        {
            UpdatePlayerLives(slotIndex, livesRemaining);
        }

        private void HandlePlayerEliminated(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= playerSlots.Length) return;

            var slot = playerSlots[slotIndex];
            if (slot == null) return;

            if (slot.canvasGroup != null)
            {
                slot.canvasGroup.alpha = 0.3f;
            }

            Debug.Log($"[HUDManager] Player {slotIndex + 1} eliminated");
        }

        public void UpdatePlayerLives(int slotIndex, int livesRemaining)
        {
            if (slotIndex < 0 || slotIndex >= playerSlots.Length) return;

            var slot = playerSlots[slotIndex];
            if (slot == null || slot.lifeImages == null) return;

            for (int i = 0; i < slot.lifeImages.Length; i++)
            {
                if (slot.lifeImages[i] != null)
                {
                    slot.lifeImages[i].enabled = i < livesRemaining;
                }
            }

            Debug.Log($"[HUDManager] Player {slotIndex + 1} lives updated to {livesRemaining}");
        }

        public void SetPowerUpText(int slotIndex, string powerUpName)
        {
            if (slotIndex < 0 || slotIndex >= playerSlots.Length) return;

            var slot = playerSlots[slotIndex];
            if (slot == null) return;

            if (slot.powerUpText != null)
            {
                slot.powerUpText.text = powerUpName;
            }
        }

        public void ClearPowerUpText(int slotIndex)
        {
            SetPowerUpText(slotIndex, "");
        }
    }
}
