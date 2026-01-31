using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class PowerUpPickup : MonoBehaviour
    {
        [Header("Effect")]
        [SerializeField] private PowerUpEffectSO effect;

        [Header("Destroy")]
        [SerializeField] private Transform objectToDestroy; // lascia vuoto: prende il root

        private bool collected;

        private void Awake()
        {
            if (objectToDestroy == null)
                objectToDestroy = transform.root;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected) return;

            var player = other.GetComponentInParent<PlayerPowerUpController>();
            if (player == null) return;

            collected = true;

            player.ApplyPowerUp(effect);

            Destroy(objectToDestroy.gameObject);
        }
    }
}
