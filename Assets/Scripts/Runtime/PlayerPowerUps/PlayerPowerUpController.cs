using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Runtime.PlayerPowerUps
{
    public class PlayerPowerUpController : MonoBehaviour
    {
        public event Action<string> OnPowerUpTextChanged;

        [SerializeField] private PlayerController playerController;
        [SerializeField] private FloatingText floatingTextPrefab;

        private readonly Dictionary<PowerUpEffect, Coroutine> running = new();
        private string currentPowerUpName;

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            if (playerController != null)
            {
                playerController.OnPowerUpExpired += HandlePowerUpExpired;
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
            {
                playerController.OnPowerUpExpired -= HandlePowerUpExpired;
            }
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

            // Notifica la UI del powerup attivo
            currentPowerUpName = effect.powerUpName;
            OnPowerUpTextChanged?.Invoke(currentPowerUpName);

            if (effect.durationSeconds > 0f)
            {
                running[effect] = StartCoroutine(TrackPowerUpDuration(effect));
            }
        }

        private void HandlePowerUpExpired(PowerUpEffect effect)
        {
            running.Remove(effect);
            // Quando un powerup scade, svuota il testo
            currentPowerUpName = "";
            OnPowerUpTextChanged?.Invoke(currentPowerUpName);
        }

        private IEnumerator TrackPowerUpDuration(PowerUpEffect effect)
        {
            yield return new WaitForSeconds(effect.durationSeconds);
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
