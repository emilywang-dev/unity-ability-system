namespace Ability.Result
{
    /// <summary>
    /// Shared failure reason for ability execution; not used for cooldown rejection.
    /// </summary>
    /// <remarks>
    /// Cooldown rejection uses <see cref="TryExecuteResult.OnCooldown"/> only.
    /// Concrete abilities may also return these reasons from <see cref="IAbility.Execute"/>
    /// (e.g. dead target → <c>Blocked</c>).
    /// </remarks>
    public enum AbilityFailReason
    {
        None = 0,
        // CanExecute false, or Execute rejected an invalid/unavailable target (e.g. dead).
        Blocked,
        // Execute aborted mid-cast (e.g. caster type mismatch).
        Interrupted
    }
}
