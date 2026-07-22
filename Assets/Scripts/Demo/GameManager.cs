using System;
using UnityEngine;
using Core;
using Gameplay.Combat;
using Gameplay.Combat.Modifiers;
using Ability;
using Ability.Buff;

namespace Demo
{
    /// <summary>
    /// Demo composition root: shared pipeline/factory/random/buff applier and entity Initialize order.
    /// </summary>
    /// <remarks>
    /// Other Demo components must not touch initialized Player/Enemy state in Awake.
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private Enemy _enemy;

        private DamagePipeline _damagePipeline;
        private AbilityFactory _abilityFactory;
        private IRandomProvider _randomProvider;
        private IBuffApplier _buffApplier;

        private void Awake()
        {
            if (_player == null)
            {
                throw new InvalidOperationException($"{nameof(Player)} is not assigned on {nameof(GameManager)}.");
            }

            if (_enemy == null)
            {
                throw new InvalidOperationException($"{nameof(Enemy)} is not assigned on {nameof(GameManager)}.");
            }

            _randomProvider = new UnityRandomProvider();
            _abilityFactory = new AbilityFactory();
            // Stateless NoOp is safe to share; real BuffSystem must be per-entity.
            _buffApplier = new NoOpBuffApplier();

            // Both multiplicative today (order commutative); Defense then Crit is the intended
            // "mitigate base, then apply crit multiplier" read order for future non-commutative mods.
            IDamageModifier[] modifiers =
            {
                new DefenseModifier(),
                new CritModifier(),
            };
            _damagePipeline = new DamagePipeline(modifiers);

            _player.Initialize(_randomProvider, _damagePipeline, _abilityFactory, _buffApplier);
            _enemy.Initialize(_buffApplier);
        }
    }
}
