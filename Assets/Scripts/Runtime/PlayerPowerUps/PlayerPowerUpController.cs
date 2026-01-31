using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Runtime.PlayerPowerUps
{
    public class PlayerPowerUpController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private FloatingText floatingTextPrefab;

        private readonly Dictionary<PowerUpEffect, Coroutine> running = new();

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
        }

        public void ApplyPowerUp(PowerUpEffect effect)
        {
            if (effect == null)  return;

            if(playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            // Se lo stesso power-up viene ripreso: refresh durata (semplice e funzionale)
            if (running.TryGetValue(effect, out var c) && c != null)
            {
                StopCoroutine(c);
                running.Remove(effect);
            }

            playerController.ApplyPowerUp(effect);
            ShowText(effect.pickupText);

            //if (effect.durationSeconds > 0f)
            //{
            //    running[effect] = StartCoroutine(RemoveAfter(effect));
            //}
        }

        private IEnumerator RemoveAfter(PowerUpEffect effect)
        {
            yield return new WaitForSeconds(effect.durationSeconds);
            //playerController.(effect.modifiers);
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
