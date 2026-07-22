using System;
using UnityEngine;
using Core;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Stats;
using Ability;
using Ability.Buff;

namespace Demo
{
    /// <summary>
    /// Demo player adapter: wires local combat systems and owns <see cref="AbilitySystem"/>.
    /// </summary>
    public sealed class Player : MonoBehaviour, ICombatEntity, IAbilityTargetProvider
    {
        public const int PrimaryAbilitySlot = 0;

        [SerializeField] private PlayerConfig _config;

        private EventBus _localEventBus;
        private StatCollection _stats;
        private Damageable _damageable;
        private IBuffApplier _buffApplier;
        private AbilitySystem _abilitySystem;
        private bool _initialized;

        public bool IsInitialized => _initialized;
        public IReadOnlyStatCollection Stats =>
            _stats ?? throw new InvalidOperationException("Player has not been initialized.");
        public Damageable Damageable =>
            _damageable ?? throw new InvalidOperationException("Player has not been initialized.");
        public AbilitySystem AbilitySystem =>
            _abilitySystem ?? throw new InvalidOperationException("Player has not been initialized.");

        // Shared deps from GameManager — do not construct pipeline / factory / random / buff applier here.
        public void Initialize(
            IRandomProvider randomProvider,
            DamagePipeline damagePipeline,
            IAbilityFactory abilityFactory,
            IBuffApplier buffApplier)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Player has already been initialized.");
            }

            if (_config == null)
            {
                throw new InvalidOperationException($"{nameof(PlayerConfig)} is not assigned on {name}.");
            }

            _buffApplier = buffApplier ?? throw new ArgumentNullException(nameof(buffApplier));
            _localEventBus = new EventBus();
            _stats = new StatCollection(_localEventBus);
            _config.ApplyBaseStats(_stats);
            _damageable = new Damageable(this, _stats, _localEventBus);
            _abilitySystem = new AbilitySystem(
                _config.AbilitySlots,
                _localEventBus,
                randomProvider,
                damagePipeline,
                abilityFactory);
            _initialized = true;
        }

        public AbilityTargetDeps ToAbilityTarget()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Player has not been initialized.");
            }

            return new AbilityTargetDeps(this, _localEventBus, _buffApplier);
        }

        private void Update()
        {
            if (!_initialized)
            {
                return;
            }

            _abilitySystem.Tick(Time.deltaTime);
        }
    }
}
