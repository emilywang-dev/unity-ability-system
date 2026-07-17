using Gameplay.Combat;

namespace Ability.Buff
{
    /// <summary>
    /// Applies a buff to the owner this applier was created for.
    /// </summary>
    public interface IBuffApplier
    {
        void Apply(BuffConfig config, ICombatEntity instigator = null);
    }
}
