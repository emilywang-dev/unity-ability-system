using System;
using System.Collections.Generic;
using Ability.Abilities;
using Ability.Config;

namespace Ability
{
    /// <summary>
    /// Default factory for Ability-layer skills (Demo may Register or wrap for Dash, etc.).
    /// </summary>
    public sealed class AbilityFactory : IAbilityFactory
    {
        private readonly struct Registration
        {
            public readonly Func<IAbility> Creator;
            public readonly Type ExpectedConfigType;

            public Registration(Func<IAbility> creator, Type expectedConfigType)
            {
                Creator = creator;
                ExpectedConfigType = expectedConfigType;
            }
        }

        private readonly Dictionary<AbilityType, Registration> _registrations = new();

        public AbilityFactory()
        {
            Register(AbilityType.Damage, () => new DamageAbility(), typeof(DamageAbilityConfig));
        }

        // Adds a type once; duplicate keys throw — Demo should Register new types or wrap, not overwrite.
        public void Register(AbilityType abilityType, Func<IAbility> creator, Type expectedConfigType)
        {
            if (abilityType == AbilityType.None)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(abilityType),
                    abilityType,
                    "AbilityType.None cannot be registered.");
            }

            if (creator == null)
            {
                throw new ArgumentNullException(nameof(creator));
            }

            if (expectedConfigType == null)
            {
                throw new ArgumentNullException(nameof(expectedConfigType));
            }

            if (!typeof(AbilityConfig).IsAssignableFrom(expectedConfigType))
            {
                throw new ArgumentException(
                    $"{expectedConfigType.Name} must derive from {nameof(AbilityConfig)}.",
                    nameof(expectedConfigType));
            }

            if (_registrations.ContainsKey(abilityType))
            {
                throw new InvalidOperationException(
                    $"AbilityType.{abilityType} is already registered.");
            }

            _registrations[abilityType] = new Registration(creator, expectedConfigType);
        }

        public IAbility Create(AbilityType abilityType) =>
            GetRegistration(abilityType).Creator();

        public Type GetExpectedConfigType(AbilityType abilityType) =>
            GetRegistration(abilityType).ExpectedConfigType;

        private Registration GetRegistration(AbilityType abilityType)
        {
            if (!_registrations.TryGetValue(abilityType, out Registration registration))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(abilityType),
                    abilityType,
                    abilityType == AbilityType.None
                        ? "AbilityType.None cannot be created."
                        : $"No Ability-layer implementation for {abilityType}.");
            }

            return registration;
        }
    }
}
