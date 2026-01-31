using TMPro;
using UnityEngine;

namespace Assets.Scripts.Runtime.UI
{
    /// <summary>
    /// Attach to each Background Player UI element to update text when players join.
    /// </summary>
    public class PlayerSlotUI : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        
        private TextMeshProUGUI text;
        private string joinPromptText = "Premi per joinare";

        private void Awake()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (PlayerSelectionManager.Instance != null)
            {
                PlayerSelectionManager.Instance.OnPlayerJoinChanged += HandleJoinChanged;
                
                // Check initial state
                if (PlayerSelectionManager.Instance.IsSlotJoined(slotIndex))
                {
                    UpdateText(true);
                }
                else
                {
                    UpdateText(false);
                }
            }
            else
            {
                Debug.LogWarning($"[PlayerSlotUI] PlayerSelectionManager not found for slot {slotIndex}");
            }
        }

        private void OnDestroy()
        {
            if (PlayerSelectionManager.Instance != null)
            {
                PlayerSelectionManager.Instance.OnPlayerJoinChanged -= HandleJoinChanged;
            }
        }

        private void HandleJoinChanged(int slot, bool joined)
        {
            if (slot != slotIndex) return;
            UpdateText(joined);
        }

        private void UpdateText(bool joined)
        {
            if (text == null) return;
            text.text = joined ? $"Player {slotIndex + 1}" : joinPromptText;
        }
    }
}
