using System;

namespace Ability
{
    /// <summary>
    /// Creates flyweight <see cref="IAbility"/> instances from <see cref="AbilityType"/>.
    /// </summary>
    public interface IAbilityFactory
    {
        IAbility Create(AbilityType abilityType);

        // Used by AbilitySystem ctor to fail-fast when slot Config type mismatches AbilityType.
        Type GetExpectedConfigType(AbilityType abilityType);
    }
}
