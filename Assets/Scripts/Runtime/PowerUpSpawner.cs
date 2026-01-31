using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class PowerUpSpawner : MonoBehaviour
    {
        [Header("Spawn Area (BoxCollider Trigger)")]
        [SerializeField] private BoxCollider spawnArea;

        [Header("PowerUp Prefabs")]
        [SerializeField] private List<GameObject> powerUpPrefabs = new List<GameObject>();

        [Header("Timing")]
        [SerializeField] private float initialDelay = 2f;
        [SerializeField] private float minSpawnInterval = 4f;
        [SerializeField] private float maxSpawnInterval = 8f;

        [Header("Limits")]
        [SerializeField] private int maxPowerUpsAlive = 3;

        [Header("Ground")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundRaycastExtraHeight = 10f;
        [SerializeField] private float groundRaycastDistance = 200f;

        [Header("Drop From Sky")]
        [SerializeField] private float minDropHeight = 6f;
        [SerializeField] private float maxDropHeight = 10f;

        [Header("Avoid Players (optional)")]
        [SerializeField] private bool avoidPlayers = true;
        [SerializeField] private float minDistanceFromPlayers = 2.5f;

        [Header("Validation")]
        [SerializeField] private int maxAttemptsPerSpawn = 25;

        private readonly List<GameObject> alive = new List<GameObject>();

        private void OnEnable()
        {
            StartCoroutine(SpawnLoop());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                CleanupDead();

                if (alive.Count < maxPowerUpsAlive)
                    SpawnOne();

                yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
            }
        }

        private void SpawnOne()
        {
            if (spawnArea == null) return;
            if (powerUpPrefabs == null || powerUpPrefabs.Count == 0) return;

            GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Count)];

            for (int attempt = 0; attempt < maxAttemptsPerSpawn; attempt++)
            {
                if (!TryGetGroundPoint(out Vector3 groundPoint))
                    continue;

                if (avoidPlayers && IsTooCloseToAnyPlayer(groundPoint))
                    continue;

                float drop = Random.Range(minDropHeight, maxDropHeight);
                Vector3 spawnPos = groundPoint + Vector3.up * drop;

                Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                GameObject instance = Instantiate(prefab, spawnPos, rot);

                // Sicurezza: se ha rigidbody, azzera velocità (evita drift strani se prefab salvato con velocity)
                if (instance.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    // Se vuoi ridurre il rischio di attraversare il terreno:
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }

                alive.Add(instance);
                return;
            }
        }

        private bool TryGetGroundPoint(out Vector3 groundPoint)
        {
            groundPoint = Vector3.zero;

            Bounds b = spawnArea.bounds;
            float x = Random.Range(b.min.x, b.max.x);
            float z = Random.Range(b.min.z, b.max.z);

            // parto da sopra l'area e tiro giù
            Vector3 rayStart = new Vector3(x, b.max.y + groundRaycastExtraHeight, z);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer))
            {
                groundPoint = hit.point;
                return true;
            }

            return false;
        }

        private bool IsTooCloseToAnyPlayer(Vector3 point)
        {
            // Per 4 player va benissimo farlo così: semplice e robusto
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                if (Vector3.Distance(players[i].transform.position, point) < minDistanceFromPlayers)
                    return true;
            }
            return false;
        }

        private void CleanupDead()
        {
            for (int i = alive.Count - 1; i >= 0; i--)
                if (alive[i] == null) alive.RemoveAt(i);
        }
    }
}
