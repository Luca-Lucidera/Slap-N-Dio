using UnityEngine;

namespace Assets.Scripts.Runtime
{
    [System.Serializable]
    public struct PowerUpModifiers
    {
        public float moveSpeedMultiplier;
        public float slapForceMultiplier;
        public float slapCooldownMultiplier;
        public float slapRadiusMultiplier;
        public float playerScaleMultiplier;
        public float massMultiplier;

        public static PowerUpModifiers Identity()
        {
            return new PowerUpModifiers
            {
                moveSpeedMultiplier = 1f,
                slapForceMultiplier = 1f,
                slapCooldownMultiplier = 1f,
                slapRadiusMultiplier = 1f,
                playerScaleMultiplier = 1f,
                massMultiplier = 1f
            };
        }
    }

    [CreateAssetMenu(menuName = "GameJam/PowerUp Effect", fileName = "PowerUpEffect_")]
    public class PowerUpEffectSO : ScriptableObject
    {
        [Header("UI")]
        public string powerUpName = "POWER UP";
        public string pickupText = "Power up!";
        public float durationSeconds = 5f; // 0 = istantaneo

        [Header("Modifiers")]
        public PowerUpModifiers modifiers = PowerUpModifiers.Identity();
    }
}
