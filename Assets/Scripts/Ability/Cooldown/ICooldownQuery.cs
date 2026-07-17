namespace Ability.Cooldown
{
    /// <summary>
    /// Read-only cooldown state for ability slots.
    /// </summary>
    public interface ICooldownQuery
    {
        bool IsOnCooldown(int abilityIndex);

        float GetRemaining(int abilityIndex);

        float GetDuration(int abilityIndex);

        float GetNormalized(int abilityIndex);
    }
}
