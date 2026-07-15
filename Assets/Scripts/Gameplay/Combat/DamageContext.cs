using System;
using Gameplay.Stats;

namespace Gameplay.Combat
{
    /// <summary>
    /// Pre-resolution hit input for the modifier chain (not the post-hit
    /// <see cref="Gameplay.Combat.Events.DamageEvent"/>).
    /// </summary>
    /// <remarks>
    /// <c>Source</c> and <c>Target</c> required — no environmental / no-source damage path.
    /// </remarks>
    public readonly struct DamageContext
    {
        public DamageContext(
            ICombatEntity source,
            ICombatEntity target,
            DamageType damageType,
            bool isCrit)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            DamageType = damageType;
            IsCrit = isCrit;
        }

        public ICombatEntity Source { get; }

        public ICombatEntity Target { get; }

        public DamageType DamageType { get; }

        /// <summary>
        /// Crit flag for the modifier chain. Rolled at the ability layer via
        /// <see cref="Core.IRandomProvider"/> before this context is built.
        /// </summary>
        public bool IsCrit { get; }
    }
}
