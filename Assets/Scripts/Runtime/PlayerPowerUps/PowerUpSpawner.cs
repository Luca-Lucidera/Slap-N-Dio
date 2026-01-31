using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class PowerUpSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class WeightedPowerUp
        {
            public GameObject prefab;

            [Tooltip("Peso relativo (puoi pensarla come percentuale se la somma fa 100). 0 = mai.")]
            [Min(0f)] public float weight = 1f;
        }

        [Header("Spawn Area (BoxCollider Trigger)")]
        [SerializeField] private BoxCollider spawnArea;

        [Header("PowerUp Pool (Prefab + Weight)")]
        [SerializeField] private List<WeightedPowerUp> powerUpPool = new List<WeightedPowerUp>();

        [Header("Timing")]
        [SerializeField] private float initialDelay = 2f;
        [SerializeField] private float minSpawnInterval = 4f;
        [SerializeField] private float maxSpawnInterval = 8f;

        [Header("Limits")]
        [SerializeField] private int maxPowerUpsAlive = 3;

        [Header("Spawn Padding (no spawn near edges)")]
        [SerializeField] private float edgePadding = 2f;

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

            GameObject prefab = PickWeightedPrefab();
            if (prefab == null) return;

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

                if (instance.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }

                alive.Add(instance);
                return;
            }
        }

        private GameObject PickWeightedPrefab()
        {
            if (powerUpPool == null || powerUpPool.Count == 0)
                return null;

            float total = 0f;
            for (int i = 0; i < powerUpPool.Count; i++)
            {
                var item = powerUpPool[i];
                if (item == null || item.prefab == null) continue;
                if (item.weight <= 0f) continue;
                total += item.weight;
            }

            if (total <= 0f)
                return null;

            float r = Random.Range(0f, total);

            for (int i = 0; i < powerUpPool.Count; i++)
            {
                var item = powerUpPool[i];
                if (item == null || item.prefab == null) continue;
                if (item.weight <= 0f) continue;

                r -= item.weight;
                if (r <= 0f)
                    return item.prefab;
            }

            // fallback per edge case floating point
            for (int i = powerUpPool.Count - 1; i >= 0; i--)
                if (powerUpPool[i] != null && powerUpPool[i].prefab != null && powerUpPool[i].weight > 0f)
                    return powerUpPool[i].prefab;

            return null;
        }

        private bool TryGetGroundPoint(out Vector3 groundPoint)
        {
            groundPoint = Vector3.zero;

            Bounds b = spawnArea.bounds;

            float pad = Mathf.Max(0f, edgePadding);
            float minX = b.min.x + pad;
            float maxX = b.max.x - pad;
            float minZ = b.min.z + pad;
            float maxZ = b.max.z - pad;

            if (minX >= maxX || minZ >= maxZ)
                return false;

            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);

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
