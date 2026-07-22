using System;
using Ability.Config;

namespace Ability
{
    /// <summary>
    /// Designer slot: config asset + <see cref="AbilityType"/> (factory creates the flyweight).
    /// </summary>
    [Serializable]
    public struct AbilitySlot
    {
        public AbilityType AbilityType;
        public AbilityConfig Config;
    }
}
