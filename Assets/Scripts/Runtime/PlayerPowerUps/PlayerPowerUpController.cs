using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Runtime
{
    public class PlayerPowerUpController : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private FloatingText floatingTextPrefab;

        private readonly Dictionary<PowerUpEffectSO, Coroutine> running = new();

        private void Awake()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
        }

        public void ApplyPowerUp(PowerUpEffectSO effect)
        {
            if (effect == null || stats == null) return;

            // Se lo stesso power-up viene ripreso: refresh durata (semplice e funzionale)
            if (running.TryGetValue(effect, out var c) && c != null)
            {
                StopCoroutine(c);
                running.Remove(effect);
            }

            stats.Apply(effect.modifiers);
            ShowText(effect.pickupText);

            if (effect.durationSeconds > 0f)
            {
                running[effect] = StartCoroutine(RemoveAfter(effect));
            }
        }

        private IEnumerator RemoveAfter(PowerUpEffectSO effect)
        {
            yield return new WaitForSeconds(effect.durationSeconds);
            stats.Remove(effect.modifiers);
            running.Remove(effect);
        }

        private void ShowText(string msg)
        {
            if (floatingTextPrefab == null) return;

            var ft = Instantiate(floatingTextPrefab);
            ft.Init(transform, msg);
        }
    }
}
