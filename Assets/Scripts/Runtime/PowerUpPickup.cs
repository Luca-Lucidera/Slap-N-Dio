using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class PowerUpPickup : MonoBehaviour
    {
        public enum PowerUpType
        {
            SpeedBoost,
            // TODO: aggiungi qui i futuri power-up:
            // SlapPower,
            // KnockbackResist,
            // Stun,
            // Heal,
        }

        [Header("Type")]
        [SerializeField] private PowerUpType type = PowerUpType.SpeedBoost;

        [Header("Speed Boost Settings")]
        [SerializeField] private float speedMultiplier = 1.75f;
        [SerializeField] private float durationSeconds = 5f;

        [Header("Feedback")]
        [SerializeField] private FloatingText floatingTextPrefab;
        [SerializeField] private string speedBoostText = "Speed up";
        // TODO: placeholder testi futuri
        [SerializeField] private string defaultText = "Power up!";

        [Header("Destroy")]
        [SerializeField] private Transform objectToDestroy; // se vuoto: prende il root

        private bool collected;

        private void Awake()
        {
            if (objectToDestroy == null)
                objectToDestroy = transform.root;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected) return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            collected = true;

            // 1) Applica effetto
            ApplyEffect(player);

            // 2) Feedback testo
            ShowPickupText(player);

            // 3) Distruggi tutto il power-up
            Destroy(objectToDestroy.gameObject);
        }

        private void ApplyEffect(PlayerController player)
        {
            switch (type)
            {
                case PowerUpType.SpeedBoost:
                    player.ApplySpeedMultiplier(speedMultiplier, durationSeconds);
                    break;

                // TODO: esempi placeholder per futuri power-up
                // case PowerUpType.SlapPower:
                //     player.ApplySlapPowerMultiplier(slapMultiplier, durationSeconds);
                //     break;

                // case PowerUpType.KnockbackResist:
                //     player.ApplyKnockbackResistance(resistanceAmount, durationSeconds);
                //     break;

                default:
                    // placeholder: per ora non fa nulla
                    break;
            }
        }

        private void ShowPickupText(PlayerController player)
        {
            if (floatingTextPrefab == null) return;

            string msg = GetTextForType(type);

            var ft = Instantiate(floatingTextPrefab);
            ft.Init(player.transform, msg);
        }

        private string GetTextForType(PowerUpType t)
        {
            switch (t)
            {
                case PowerUpType.SpeedBoost:
                    return speedBoostText;

                // TODO: placeholder futuri
                // case PowerUpType.SlapPower:
                //     return slapPowerText;
                // case PowerUpType.KnockbackResist:
                //     return knockbackResistText;

                default:
                    return defaultText;
            }
        }
    }
}
