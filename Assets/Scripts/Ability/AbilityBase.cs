using Gameplay.Stats;
using Ability.Buff;
using Ability.Result;

namespace Ability
{
    /// <summary>
    /// Shared helpers for ability execution; concrete abilities implement <see cref="Execute"/>.
    /// </summary>
    public abstract class AbilityBase : IAbility
    {
        // Non-cooldown gate only; default allow. CD is AbilitySystem's job.
        public virtual bool CanExecute(in AbilityContext context) => true;

        public abstract AbilityExecuteResult Execute(in AbilityContext context);

        protected bool RollCrit(in AbilityContext context) =>
            context.RandomProvider.NextFloat() < context.Caster.Stats.GetValue(StatType.CritChance);

        // After damage/heal work so the buff does not affect this hit.
        protected void ApplyOnHitBuff(in AbilityContext context)
        {
            BuffConfig buff = context.Config.OnHitBuff;
            if (buff == null)
            {
                return;
            }

            context.TargetBuffApplier.Apply(buff, context.Caster);
        }
    }
}
