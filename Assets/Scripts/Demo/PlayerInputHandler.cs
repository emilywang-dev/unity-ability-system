using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Ability.Result;

namespace Demo
{
    /// <summary>
    /// Demo input adapter: CastAbility → <see cref="Ability.AbilitySystem.TryExecute"/> on the enemy.
    /// </summary>
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private Enemy _enemy;
        // Optional; when unset, defaults to Keyboard Space.
        [SerializeField] private InputActionReference _castAbility;

        private InputAction _fallbackCast;

        private void Awake()
        {
            if (_player == null)
            {
                throw new InvalidOperationException($"{nameof(Player)} is not assigned on {nameof(PlayerInputHandler)}.");
            }

            if (_enemy == null)
            {
                throw new InvalidOperationException($"{nameof(Enemy)} is not assigned on {nameof(PlayerInputHandler)}.");
            }

            if (_castAbility == null || _castAbility.action == null)
            {
                _fallbackCast = new InputAction(
                    name: "CastAbility",
                    type: InputActionType.Button,
                    binding: "<Keyboard>/space");
            }
        }

        private void OnEnable()
        {
            InputAction action = ResolveCastAction();
            action.performed += OnCastPerformed;
            action.Enable();
        }

        private void OnDisable()
        {
            InputAction action = ResolveCastAction();
            action.performed -= OnCastPerformed;
            action.Disable();
        }

        private void OnDestroy()
        {
            _fallbackCast?.Dispose();
        }

        private void OnCastPerformed(InputAction.CallbackContext context)
        {
            TryExecuteResult result = _player.AbilitySystem.TryExecute(
                Player.PrimaryAbilitySlot,
                _player,
                _enemy.ToAbilityTarget());

            if (result.Succeeded)
            {
                return;
            }

            if (result.IsOnCooldown)
            {
                Debug.Log("Ability on cooldown.");
                return;
            }

            Debug.Log($"Ability failed: {result.FailReason}");
        }

        private InputAction ResolveCastAction()
        {
            if (_castAbility != null && _castAbility.action != null)
            {
                return _castAbility.action;
            }

            return _fallbackCast
                ?? throw new InvalidOperationException("Cast ability input action is not configured.");
        }
    }
}
