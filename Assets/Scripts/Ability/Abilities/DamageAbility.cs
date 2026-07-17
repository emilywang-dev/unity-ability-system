using System;
using Gameplay.Combat;
using Gameplay.Stats;
using Ability.Config;
using Ability.Result;

namespace Ability.Abilities
{
    /// <summary>
    /// Single-target AttackPower hit via <see cref="DamagePipeline"/> (crit rolled here).
    /// </summary>
    public sealed class DamageAbility : AbilityBase
    {
        // Instant targeted hit — dead targets are invalid (no CD burn via TryExecute Blocked).
        public override bool CanExecute(in AbilityContext context) =>
            !context.TargetEntity.Damageable.IsDead;

        public override AbilityExecuteResult Execute(in AbilityContext context)
        {
            if (context.TargetEntity.Damageable.IsDead)
            {
                return AbilityExecuteResult.Fail(AbilityFailReason.Blocked);
            }

            if (context.Config is not DamageAbilityConfig config)
            {
                throw new InvalidOperationException(
                    $"DamageAbility requires {nameof(DamageAbilityConfig)}, got {context.Config.GetType().Name}.");
            }

            bool isCrit = RollCrit(in context);
            float damage = context.Caster.Stats.GetValue(StatType.AttackPower) * config.DamageCoefficient;

            var damageContext = new DamageContext(
                context.Caster,
                context.TargetEntity,
                config.DamageType,
                isCrit);

            context.DamagePipeline.Execute(damage, damageContext, context.TargetEventBus);

            ApplyOnHitBuff(in context);
            return AbilityExecuteResult.Success();
        }
    }
}
