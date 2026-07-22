using Gameplay.Combat;
using Ability.Buff;

namespace Demo
{
    /// <summary>
    /// Temporary <see cref="IBuffApplier"/> so Demo can build valid target deps before BuffSystem exists.
    /// </summary>
    public sealed class NoOpBuffApplier : IBuffApplier
    {
        public void Apply(BuffConfig config, ICombatEntity instigator = null)
        {
            // Intentionally no-op until BuffSystem exists.
        }
    }
}
