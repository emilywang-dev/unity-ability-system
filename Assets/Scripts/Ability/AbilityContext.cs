using System;
using Core;
using Core.Events;
using Gameplay.Combat;
using Ability.Buff;
using Ability.Config;
using Ability.Cooldown;

namespace Ability
{
    /// <summary>
    /// Per-cast dependencies for one ability invocation.
    /// </summary>
    /// <remarks>
    /// Built fresh each <see cref="AbilitySystem.TryExecute"/>.
    /// Do not cache on a shared factory-created <see cref="IAbility"/> flyweight.
    /// </remarks>
    public readonly struct AbilityContext
    {
        public readonly ICombatEntity Caster;
        public readonly AbilityTargetDeps Target;
        public readonly int AbilityIndex;
        public readonly AbilityConfig Config;
        public readonly DamagePipeline DamagePipeline;
        // Read-only surface; mutable cooldown state stays on CooldownManager in AbilitySystem.
        public readonly ICooldownQuery CooldownQuery;
        // Roll crit here. Pipeline just reads DamageContext.IsCrit.
        public readonly IRandomProvider RandomProvider;

        public ICombatEntity TargetEntity => Target.Entity;
        public IEventBus TargetEventBus => Target.EventBus;
        public IBuffApplier TargetBuffApplier => Target.BuffApplier;

        public AbilityContext(
            ICombatEntity caster,
            AbilityTargetDeps target,
            int abilityIndex,
            AbilityConfig config,
            DamagePipeline damagePipeline,
            ICooldownQuery cooldownQuery,
            IRandomProvider randomProvider)
        {
            Caster = caster ?? throw new ArgumentNullException(nameof(caster));

            if (!target.IsValid)
            {
                throw new ArgumentException(
                    "Target dependencies are invalid (missing entity, event bus, or buff applier).",
                    nameof(target));
            }
            Target = target;

            if (abilityIndex < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(abilityIndex),
                    abilityIndex,
                    "Ability index must be non-negative.");
            }
            AbilityIndex = abilityIndex;

            Config = config ?? throw new ArgumentNullException(nameof(config));
            DamagePipeline = damagePipeline ?? throw new ArgumentNullException(nameof(damagePipeline));
            CooldownQuery = cooldownQuery ?? throw new ArgumentNullException(nameof(cooldownQuery));
            RandomProvider = randomProvider ?? throw new ArgumentNullException(nameof(randomProvider));
        }
    }
}
