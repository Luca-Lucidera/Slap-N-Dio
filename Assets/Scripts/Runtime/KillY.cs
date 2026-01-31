using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Runtime
{
	[RequireComponent(typeof(BoxCollider))]
	public class KillY: MonoBehaviour
	{
        [SerializeField] private Transform respawnPoint;

        private WaitForSeconds waitForSeconds;
        private PlayerManager playerManager;

        void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
            waitForSeconds = new WaitForSeconds(5f);
        }

        void Start()
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerController>(out _))
            {
                StartCoroutine(RespawnPlayer(other.gameObject));
            }
            else
            {
                Destroy(other.gameObject);
            }
        }

        private IEnumerator RespawnPlayer(GameObject player)
        {
            // Decrementa vita e controlla se può respawnare
            bool canRespawn = playerManager != null && playerManager.DecrementLife(player.transform);

            playerManager?.MarkPlayerAsDead(player.transform);
            player.SetActive(false);

            if (!canRespawn)
            {
                // Player eliminato definitivamente
                yield break;
            }

            yield return waitForSeconds;

            player.transform.position = respawnPoint.position;
            player.SetActive(true);
            playerManager?.MarkPlayerAsAlive(player.transform);
        }
    }
}