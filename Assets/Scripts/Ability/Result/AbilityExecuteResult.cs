using System;

namespace Ability.Result
{
    /// <summary>
    /// Outcome of <see cref="IAbility.Execute"/>.
    /// </summary>
    /// <remarks>
    /// Cooldown is gated by <see cref="AbilitySystem"/> before Execute; started only when this result succeeds.
    /// </remarks>
    public readonly struct AbilityExecuteResult
    {
        public readonly bool Succeeded;
        // Meaningful only when Succeeded is false.
        public readonly AbilityFailReason FailReason;

        private AbilityExecuteResult(bool succeeded, AbilityFailReason failReason)
        {
            Succeeded = succeeded;
            FailReason = failReason;
        }

        public static AbilityExecuteResult Success() =>
            new(true, AbilityFailReason.None);

        public static AbilityExecuteResult Fail(AbilityFailReason reason)
        {
            if (reason == AbilityFailReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "A concrete fail reason is required.");
            }

            return new(false, reason);
        }
    }
}
