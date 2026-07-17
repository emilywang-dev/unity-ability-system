using System;
using System.Collections.Generic;
using Core;
using Core.Events;
using Gameplay.Combat;
using Ability.Config;
using Ability.Cooldown;
using Ability.Result;

namespace Ability
{
    /// <summary>
    /// Owns an entity's ability slots and cooldowns; single entry point for TryExecute.
    /// </summary>
    /// <remarks>
    /// Cooldown starts only after <see cref="IAbility.Execute"/> succeeds.
    /// Slot flyweights are created via <see cref="IAbilityFactory"/> at construction.
    /// </remarks>
    public sealed class AbilitySystem
    {
        private readonly AbilityConfig[] _configs;
        private readonly IAbility[] _abilities;
        private readonly CooldownManager _cooldownManager;
        private readonly IRandomProvider _randomProvider;
        private readonly DamagePipeline _damagePipeline;

        // Read-only surface for HUD / callers; mutable state stays on CooldownManager.
        public ICooldownQuery CooldownQuery => _cooldownManager;

        public AbilitySystem(
            IReadOnlyList<AbilitySlot> slots,
            IEventBus localEventBus,
            IRandomProvider randomProvider,
            DamagePipeline damagePipeline,
            IAbilityFactory abilityFactory)
        {
            if (slots == null)
            {
                throw new ArgumentNullException(nameof(slots));
            }

            _randomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
            _damagePipeline = damagePipeline ?? throw new ArgumentNullException(nameof(damagePipeline));
            abilityFactory = abilityFactory ?? throw new ArgumentNullException(nameof(abilityFactory));
            _cooldownManager = new CooldownManager(
                localEventBus ?? throw new ArgumentNullException(nameof(localEventBus)));

            _configs = new AbilityConfig[slots.Count];
            _abilities = new IAbility[slots.Count];
            for (int i = 0; i < slots.Count; i++)
            {
                AbilitySlot slot = slots[i];
                if (slot.Config == null || slot.AbilityType == AbilityType.None)
                {
                    throw new InvalidOperationException(
                        $"Ability slot {i} is missing config or ability type.");
                }

                Type expectedConfigType = abilityFactory.GetExpectedConfigType(slot.AbilityType);
                if (!expectedConfigType.IsInstanceOfType(slot.Config))
                {
                    throw new InvalidOperationException(
                        $"Ability slot {i} AbilityType {slot.AbilityType} expects {expectedConfigType.Name}, " +
                        $"got {slot.Config.GetType().Name}.");
                }

                _configs[i] = slot.Config;
                _abilities[i] = abilityFactory.Create(slot.AbilityType);
            }
        }

        public TryExecuteResult TryExecute(
            int abilityIndex,
            ICombatEntity caster,
            AbilityTargetDeps target)
        {
            if (abilityIndex < 0 || abilityIndex >= _abilities.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(abilityIndex));
            }

            if (caster == null)
            {
                throw new ArgumentNullException(nameof(caster));
            }

            if (!target.IsValid)
            {
                throw new ArgumentException(
                    "Target dependencies are invalid (missing entity, event bus, or buff applier).",
                    nameof(target));
            }

            if (_cooldownManager.IsOnCooldown(abilityIndex))
            {
                return TryExecuteResult.OnCooldown();
            }

            AbilityConfig config = _configs[abilityIndex];
            IAbility ability = _abilities[abilityIndex];
            var context = new AbilityContext(
                caster,
                target,
                abilityIndex,
                config,
                _damagePipeline,
                _cooldownManager,
                _randomProvider);

            if (!ability.CanExecute(in context))
            {
                return TryExecuteResult.Fail(AbilityFailReason.Blocked);
            }

            AbilityExecuteResult executeResult = ability.Execute(in context);
            if (executeResult.Succeeded)
            {
                _cooldownManager.StartCooldown(abilityIndex, config.CooldownDuration);
                return TryExecuteResult.Success();
            }

            return TryExecuteResult.Fail(executeResult.FailReason);
        }

        public void Tick(float deltaTime)
        {
            _cooldownManager.Tick(deltaTime);
        }
    }
}
