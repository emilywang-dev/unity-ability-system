using UnityEngine;
using Gameplay.Combat;

namespace Ability.Config
{
    /// <summary>
    /// Damage coefficient and <see cref="DamageType"/> for <see cref="Abilities.DamageAbility"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Ability/Config/DamageAbilityConfig", fileName = "DamageAbilityConfig")]
    public class DamageAbilityConfig : AbilityConfig
    {
        [SerializeField] private float _damageCoefficient = 1f;
        [SerializeField] private DamageType _damageType = DamageType.Physical;

        public float DamageCoefficient => _damageCoefficient;
        public DamageType DamageType => _damageType;
    }
}
