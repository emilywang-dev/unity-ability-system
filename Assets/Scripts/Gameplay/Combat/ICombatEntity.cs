using Gameplay.Stats;

namespace Gameplay.Combat
{
    /// <summary>
    /// Minimal combat surface: live stats and health — no bus, transform, or state machine.
    /// </summary>
    public interface ICombatEntity
    {
        IReadOnlyStatCollection Stats { get; }

        Damageable Damageable { get; }
    }
}