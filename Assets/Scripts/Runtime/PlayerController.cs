using Assets.Scripts.Runtime.PlayerPowerUps;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        public event Action<PowerUpEffect> OnPowerUpAcquired;
        public event Action<PowerUpEffect> OnPowerUpExpired;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 180f;  // gradi/secondo
        [SerializeField] private float deadzone = 0.1f;

        [Header("Slap Stats")]
        [SerializeField] private float slapForce = 500f;
        [SerializeField] private float slapCooldown = 2f;
        [SerializeField] private float slapRadius = 1.0f;

        [Header("Slap Detection")]
        [SerializeField] private float slapOffset = 0.8f;
        [SerializeField] private LayerMask playerLayerMask;

        [Header("Body (placeholder per futuri powerup)")]
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float baseMass = 1f;

        private Gamepad assignedGamepad;
        private bool useKeyboard;

        // Moltiplicatori runtime (power-up)
        private float moveSpeedMul = 1f;
        private float slapForceMul = 1f;
        private float slapCooldownMul = 1f;
        private float slapRadiusMul = 1f;
        private float scaleMul = 1f;
        private float massMul = 1f;

        // Gestione durate per tipo di effect (refresh durata)
        private readonly Dictionary<PowerUpEffect, Coroutine> activeEffects = new();

        // Compatibilità vecchio speed boost (senza SO)
        private Coroutine legacySpeedRoutine;

        // Slap system
        private float lastSlapTime = -Mathf.Infinity;

        public float CurrentMoveSpeed => moveSpeed * moveSpeedMul;
        public float CurrentSlapForce => slapForce * slapForceMul;
        public float CurrentSlapCooldown => slapCooldown * slapCooldownMul;
        public float CurrentSlapRadius => slapRadius * slapRadiusMul;

        public void Initialize(Gamepad gamepad)
        {
            assignedGamepad = gamepad;
            useKeyboard = (gamepad == null);
        }

        private void Awake()
        {
            // Inizializza la LayerMask per il layer "Player" se non configurata
            if (playerLayerMask == 0)
            {
                playerLayerMask = LayerMask.GetMask("Player");
            }

            // Applica i valori base (scala/massa) all'avvio
            ApplyScaleAndMass();
        }

        private void Update()
        {
            float moveInput = 0f;
            float rotateInput = 0f;

            if (useKeyboard)
                (moveInput, rotateInput) = GetKeyboardInput();
            else if (assignedGamepad != null)
                (moveInput, rotateInput) = GetGamepadInput();

            // Applica rotazione
            if (Mathf.Abs(rotateInput) > 0f)
            {
                transform.Rotate(0f, rotateInput * rotationSpeed * Time.deltaTime, 0f);
            }

            // Applica movimento nella direzione forward
            if (Mathf.Abs(moveInput) > 0f)
            {
                transform.position += transform.forward * moveInput * CurrentMoveSpeed * Time.deltaTime;
            }

            // Slap input
            if (useKeyboard)
            {
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                    TrySlap();
            }
            else if (assignedGamepad != null)
            {
                if (assignedGamepad.rightTrigger.wasPressedThisFrame)
                    TrySlap();
            }
        }

        // =========================
        // SLAP SYSTEM
        // =========================

        private void TrySlap()
        {
            if (Time.time < lastSlapTime + CurrentSlapCooldown)
                return;

            lastSlapTime = Time.time;
            ExecuteSlap();
        }

        private void ExecuteSlap()
        {
            Vector3 slapDirection = transform.forward;
            Vector3 slapOrigin = transform.position + slapDirection * slapOffset;

            Collider[] hits = Physics.OverlapSphere(slapOrigin, CurrentSlapRadius, playerLayerMask);

            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;

                if (hit.TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 pushDir = hit.transform.position - transform.position;
                    pushDir.y = 0f;
                    pushDir.Normalize();
                    rb.AddForce(pushDir * CurrentSlapForce, ForceMode.Impulse);
                }
            }
        }

        // =========================
        // POWER-UP (nuovo sistema)
        // =========================

        public void ApplyPowerUp(PowerUpEffect effect)
        {
            if (effect == null) return;

            // Se lo stesso effect viene ripreso, refresh durata
            if (activeEffects.TryGetValue(effect, out var running) && running != null)
            {
                StopCoroutine(running);
                activeEffects.Remove(effect);
            }

            ApplyModifiers(effect.modifiers);
            OnPowerUpAcquired?.Invoke(effect);

            if (effect.durationSeconds > 0f)
            {
                activeEffects[effect] = StartCoroutine(RemoveAfter(effect));
            }
        }

        private IEnumerator RemoveAfter(PowerUpEffect effect)
        {
            yield return new WaitForSeconds(effect.durationSeconds);
            RemoveModifiers(effect.modifiers);
            activeEffects.Remove(effect);
            OnPowerUpExpired?.Invoke(effect);
        }

        private void ApplyModifiers(PowerUpModifiers m)
        {
            moveSpeedMul *= Safe(m.moveSpeedMultiplier);
            slapForceMul *= Safe(m.slapForceMultiplier);
            slapCooldownMul *= Safe(m.slapCooldownMultiplier);
            slapRadiusMul *= Safe(m.slapRadiusMultiplier);
            scaleMul *= Safe(m.playerScaleMultiplier);
            massMul *= Safe(m.massMultiplier);

            ApplyScaleAndMass();
        }

        private void RemoveModifiers(PowerUpModifiers m)
        {
            moveSpeedMul /= Safe(m.moveSpeedMultiplier);
            slapForceMul /= Safe(m.slapForceMultiplier);
            slapCooldownMul /= Safe(m.slapCooldownMultiplier);
            slapRadiusMul /= Safe(m.slapRadiusMultiplier);
            scaleMul /= Safe(m.playerScaleMultiplier);
            massMul /= Safe(m.massMultiplier);

            ApplyScaleAndMass();
        }

        private float Safe(float v)
        {
            // Se per sbaglio lasci 0 nell'asset, non spacchi tutto
            return Mathf.Approximately(v, 0f) ? 1f : v;
        }

        private void ApplyScaleAndMass()
        {
            transform.localScale = Vector3.one * (baseScale * scaleMul);

            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.mass = baseMass * massMul;
            }
        }

        // =========================
        // COMPATIBILITÀ VECCHIA API
        // =========================
        public void ApplySpeedMultiplier(float multiplier, float durationSeconds)
        {
            // Implementato sopra i moltiplicatori nuovi, così non hai 2 sistemi separati
            if (legacySpeedRoutine != null)
                StopCoroutine(legacySpeedRoutine);

            legacySpeedRoutine = StartCoroutine(LegacySpeedRoutine(multiplier, durationSeconds));
        }

        private IEnumerator LegacySpeedRoutine(float multiplier, float durationSeconds)
        {
            // Applica come moltiplicatore temporaneo (non SO)
            float prev = moveSpeedMul;
            moveSpeedMul = prev * Mathf.Max(0f, multiplier);

            yield return new WaitForSeconds(durationSeconds);

            // Ripristina al valore precedente
            moveSpeedMul = prev;
            legacySpeedRoutine = null;
        }

        // =========================
        // INPUT
        // =========================
        private (float move, float rotate) GetKeyboardInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return (0f, 0f);

            float move = 0f;
            float rotate = 0f;

            if (keyboard.wKey.isPressed) move += 1f;
            if (keyboard.sKey.isPressed) move -= 1f;
            if (keyboard.aKey.isPressed) rotate -= 1f;
            if (keyboard.dKey.isPressed) rotate += 1f;

            return (move, rotate);
        }

        private (float move, float rotate) GetGamepadInput()
        {
            if (assignedGamepad == null) return (0f, 0f);

            Vector2 stick = assignedGamepad.leftStick.ReadValue();

            if (stick.sqrMagnitude < deadzone * deadzone)
                return (0f, 0f);

            return (stick.y, stick.x);
        }
    }
}
