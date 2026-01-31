using Assets.Scripts.Runtime.PlayerPowerUps;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float deadzone = 0.1f;

        [Header("Slap stats (placeholder per futuri powerup)")]
        [SerializeField] private float slapForce = 10f;
        [SerializeField] private float slapCooldown = 0.8f;
        [SerializeField] private float slapRadius = 1.0f;

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
            // Applica i valori base (scala/massa) all'avvio
            ApplyScaleAndMass();
        }

        private void Update()
        {
            Vector3 movement = Vector3.zero;

            if (useKeyboard)
                movement = GetKeyboardInput();
            else if (assignedGamepad != null)
                movement = GetGamepadInput();

            if (movement.sqrMagnitude > 0f)
            {
                movement.Normalize();
                transform.position += movement * CurrentMoveSpeed * Time.deltaTime;
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
        private Vector3 GetKeyboardInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector3.zero;

            Vector3 movement = Vector3.zero;

            if (keyboard.wKey.isPressed) movement.z += 1f;
            if (keyboard.sKey.isPressed) movement.z -= 1f;
            if (keyboard.aKey.isPressed) movement.x -= 1f;
            if (keyboard.dKey.isPressed) movement.x += 1f;

            return movement;
        }

        private Vector3 GetGamepadInput()
        {
            if (assignedGamepad == null) return Vector3.zero;

            Vector2 stick = assignedGamepad.leftStick.ReadValue();

            if (stick.sqrMagnitude > deadzone * deadzone)
                return new Vector3(stick.x, 0f, stick.y);

            return Vector3.zero;
        }
    }
}
