using UnityEngine;

namespace Assets.Scripts.Runtime.PlayerPowerUps
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseSlapForce = 10f;
        [SerializeField] private float baseSlapCooldown = 0.8f;
        [SerializeField] private float baseSlapRadius = 1.0f;

        [Header("Defaults")]
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float baseMassMultiplier = 1f;

        // runtime multipliers
        private float moveSpeedMul = 1f;
        private float slapForceMul = 1f;
        private float slapCooldownMul = 1f; // < 1 => più veloce
        private float slapRadiusMul = 1f;
        private float scaleMul = 1f;
        private float massMul = 1f;

        public float MoveSpeed => baseMoveSpeed * moveSpeedMul;
        public float SlapForce => baseSlapForce * slapForceMul;
        public float SlapCooldown => baseSlapCooldown * slapCooldownMul;
        public float SlapRadius => baseSlapRadius * slapRadiusMul;
        public float Scale => baseScale * scaleMul;
        public float MassMultiplier => baseMassMultiplier * massMul;

        public void Apply(PowerUpModifiers m)
        {
            moveSpeedMul *= m.moveSpeedMultiplier;
            slapForceMul *= m.slapForceMultiplier;
            slapCooldownMul *= m.slapCooldownMultiplier;
            slapRadiusMul *= m.slapRadiusMultiplier;
            scaleMul *= m.playerScaleMultiplier;
            massMul *= m.massMultiplier;

            ApplyScaleAndMass();
        }

        public void Remove(PowerUpModifiers m)
        {
            // inverso dei moltiplicatori
            moveSpeedMul /= Safe(m.moveSpeedMultiplier);
            slapForceMul /= Safe(m.slapForceMultiplier);
            slapCooldownMul /= Safe(m.slapCooldownMultiplier);
            slapRadiusMul /= Safe(m.slapRadiusMultiplier);
            scaleMul /= Safe(m.playerScaleMultiplier);
            massMul /= Safe(m.massMultiplier);

            ApplyScaleAndMass();
        }

        private float Safe(float v) => Mathf.Approximately(v, 0f) ? 1f : v;

        private void ApplyScaleAndMass()
        {
            transform.localScale = Vector3.one * Scale;

            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.mass = rb.mass * MassMultiplier;
            }
        }
    }
}
