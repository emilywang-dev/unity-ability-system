using Ability.Result;

namespace Ability
{
    /// <summary>
    /// Ability implementation. Per-cast state comes from <see cref="AbilityContext"/>.
    /// </summary>
    public interface IAbility
    {
        // Non-cooldown gate only; false skips Execute with no side effects. CD is AbilitySystem's job.
        bool CanExecute(in AbilityContext context);

        AbilityExecuteResult Execute(in AbilityContext context);
    }
}
