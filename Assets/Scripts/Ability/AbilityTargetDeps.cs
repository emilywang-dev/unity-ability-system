using System;
using Core.Events;
using Gameplay.Combat;
using Ability.Buff;

namespace Ability
{
    /// <summary>
    /// Target entity plus its bus and buff applier for <see cref="AbilitySystem.TryExecute"/>.
    /// </summary>
    /// <remarks>
    /// Self buff: pass the caster with <see cref="IAbilityTargetProvider.ToAbilityTarget"/>.
    /// Non-null deps are fail-fast in the constructor; semantic validity is not checked here.
    /// </remarks>
    public readonly struct AbilityTargetDeps
    {
        public readonly ICombatEntity Entity;
        public readonly IEventBus EventBus;
        public readonly IBuffApplier BuffApplier;

        // default(AbilityTargetDeps) is invalid — use this instead of three null checks.
        public bool IsValid => Entity != null && EventBus != null && BuffApplier != null;

        public AbilityTargetDeps(ICombatEntity entity, IEventBus eventBus, IBuffApplier buffApplier)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            BuffApplier = buffApplier ?? throw new ArgumentNullException(nameof(buffApplier));
        }
    }
}
