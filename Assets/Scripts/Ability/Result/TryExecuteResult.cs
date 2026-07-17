using System;

namespace Ability.Result
{
    /// <summary>
    /// Outcome of <see cref="AbilitySystem.TryExecute"/>, covering both
    /// system-level rejection (cooldown) and the ability's own execution
    /// outcome via the shared <see cref="AbilityFailReason"/>.
    /// </summary>
    /// <remarks>
    /// Cooldown stays a separate flag — <see cref="AbilitySystem"/> owns it before CanExecute/Execute.
    /// </remarks>
    public readonly struct TryExecuteResult
    {
        public readonly bool Succeeded;
        // Not an AbilityFailReason — cooldown is gated by AbilitySystem before Execute.
        public readonly bool IsOnCooldown;
        // Meaningful only when Failed is true.
        public readonly AbilityFailReason FailReason;

        // True only for CanExecute/Execute failure — not success and not cooldown.
        public bool Failed => !Succeeded && !IsOnCooldown;

        private TryExecuteResult(bool succeeded, bool isOnCooldown, AbilityFailReason failReason)
        {
            Succeeded = succeeded;
            IsOnCooldown = isOnCooldown;
            FailReason = failReason;
        }

        public static TryExecuteResult Success() =>
            new(true, false, AbilityFailReason.None);

        public static TryExecuteResult OnCooldown() =>
            new(false, true, AbilityFailReason.None);

        public static TryExecuteResult Fail(AbilityFailReason reason)
        {
            if (reason == AbilityFailReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "A concrete fail reason is required.");
            }

            return new(false, false, reason);
        }
    }
}
