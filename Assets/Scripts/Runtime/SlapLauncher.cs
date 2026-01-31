using Assets.Scripts.Runtime.PlayerPowerUps;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class SlapLauncher : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private GameObject slapPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float forwardOffset = 1.5f;

        private float nextTime;

        private void Awake()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (spawnPoint == null) spawnPoint = transform;
        }

        private void Update()
        {
            // Placeholder: premendo Space lanci lo schiaffo.
            // Dopo lo colleghi al tuo input per player (gamepad ecc.)
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                TrySlap(Vector3.forward);
            }
        }

        public void TrySlap(Vector3 dir)
        {
            if (Time.time < nextTime) return;
            if (slapPrefab == null || stats == null) return;

            nextTime = Time.time + stats.SlapCooldown;

            Vector3 pos = spawnPoint.position + dir.normalized * forwardOffset;
            var go = Instantiate(slapPrefab, pos, Quaternion.identity);

            if (!go.TryGetComponent<Rigidbody>(out _))
                go.AddComponent<Rigidbody>();

            var slap = go.GetComponent<SlapCube>();
            if (slap == null) slap = go.AddComponent<SlapCube>();

            slap.Setup(stats.SlapForce, stats.SlapRadius, dir);
        }
    }
}
