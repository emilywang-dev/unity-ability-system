namespace Ability
{
    /// <summary>
    /// Builds <see cref="AbilityTargetDeps"/> for TryExecute / self-buff.
    /// </summary>
    public interface IAbilityTargetProvider
    {
        AbilityTargetDeps ToAbilityTarget();
    }
}
