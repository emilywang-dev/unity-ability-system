using System;
using UnityEngine;
using Core.Events;
using Gameplay.Combat;
using Gameplay.Stats;
using Ability;
using Ability.Buff;

namespace Demo
{
    /// <summary>
    /// Demo enemy dummy: local stats/health only — no abilities or AI.
    /// </summary>
    public sealed class Enemy : MonoBehaviour, ICombatEntity, IAbilityTargetProvider
    {
        [SerializeField] private EnemyConfig _config;

        private EventBus _localEventBus;
        private StatCollection _stats;
        private Damageable _damageable;
        private IBuffApplier _buffApplier;
        private bool _initialized;

        public bool IsInitialized => _initialized;
        public IReadOnlyStatCollection Stats =>
            _stats ?? throw new InvalidOperationException("Enemy has not been initialized.");
        public Damageable Damageable =>
            _damageable ?? throw new InvalidOperationException("Enemy has not been initialized.");

        public void Initialize(IBuffApplier buffApplier)
        {
            if (_initialized)
            {
                throw new InvalidOperationException("Enemy has already been initialized.");
            }

            if (_config == null)
            {
                throw new InvalidOperationException($"{nameof(EnemyConfig)} is not assigned on {name}.");
            }

            _buffApplier = buffApplier ?? throw new ArgumentNullException(nameof(buffApplier));
            _localEventBus = new EventBus();
            _stats = new StatCollection(_localEventBus);
            _config.ApplyBaseStats(_stats);
            _damageable = new Damageable(this, _stats, _localEventBus);
            _initialized = true;
        }

        public AbilityTargetDeps ToAbilityTarget()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("Enemy has not been initialized.");
            }

            return new AbilityTargetDeps(this, _localEventBus, _buffApplier);
        }
    }
}
