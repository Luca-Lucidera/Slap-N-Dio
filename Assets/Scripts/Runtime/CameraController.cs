using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerManager playerManager;

        [Header("Distance Settings")]
        [SerializeField] private float minDistance = 15f;
        [SerializeField] private float maxDistance = 35f;

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothSpeed = 3f;
        [SerializeField] private float zoomSmoothSpeed = 2f;

        [Header("Camera Angle")]
        [SerializeField] private float cameraAngle = 45f;

        private float currentDistance;
        private Vector3 currentVelocity;
        private float distanceVelocity;

        private void Start()
        {
            currentDistance = minDistance;
        }

        private void LateUpdate()
        {
            if (playerManager == null) return;

            var playerTransforms = playerManager.GetActivePlayerTransforms();
            if (playerTransforms.Count == 0) return;

            Vector3 centerPoint = CalculateCenterPoint(playerTransforms);
            float playerSpread = CalculatePlayerSpread(playerTransforms, centerPoint);
            float targetDistance = CalculateTargetDistance(playerTransforms.Count, playerSpread);

            currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, 1f / zoomSmoothSpeed);

            Vector3 targetPosition = CalculateCameraPosition(centerPoint, currentDistance);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / positionSmoothSpeed);

            transform.LookAt(centerPoint);
        }

        private Vector3 CalculateCenterPoint(System.Collections.Generic.List<Transform> players)
        {
            if (players.Count == 1)
                return players[0].position;

            Vector3 sum = Vector3.zero;
            foreach (var player in players)
            {
                sum += player.position;
            }
            return sum / players.Count;
        }

        private float CalculatePlayerSpread(System.Collections.Generic.List<Transform> players, Vector3 center)
        {
            if (players.Count <= 1)
                return 0f;

            float maxDistance = 0f;
            foreach (var player in players)
            {
                float distance = Vector3.Distance(player.position, center);
                if (distance > maxDistance)
                    maxDistance = distance;
            }
            return maxDistance;
        }

        private float CalculateTargetDistance(int playerCount, float spread)
        {
            float countFactor = Mathf.InverseLerp(1, 4, playerCount);
            float spreadFactor = Mathf.Clamp01(spread / 10f);

            float combinedFactor = Mathf.Max(countFactor, spreadFactor);

            return Mathf.Lerp(minDistance, maxDistance, combinedFactor);
        }

        private Vector3 CalculateCameraPosition(Vector3 center, float distance)
        {
            float angleRad = cameraAngle * Mathf.Deg2Rad;
            float height = distance * Mathf.Sin(angleRad);
            float backDistance = distance * Mathf.Cos(angleRad);

            return new Vector3(center.x, center.y + height, center.z - backDistance);
        }
    }
}
