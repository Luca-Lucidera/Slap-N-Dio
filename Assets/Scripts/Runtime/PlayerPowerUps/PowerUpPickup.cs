using UnityEngine;

namespace Assets.Scripts.Runtime.PlayerPowerUps
{
    public class PowerUpPickup : MonoBehaviour
    {
        [Header("Effect")]
        [SerializeField] private PowerUpEffect effect;

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
            if (effect == null) return;

            // Prende il PlayerController anche se il collider è su un child del player
            var player = other.GetComponent<PlayerPowerUpController>();
            if (player == null) return;

            collected = true;

            player.ApplyPowerUp(effect);

            Destroy(objectToDestroy.gameObject);
        }
    }
}
