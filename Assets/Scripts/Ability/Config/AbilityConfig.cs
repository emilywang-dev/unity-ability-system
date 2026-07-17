using UnityEngine;
using Ability.Buff;

namespace Ability.Config
{
    /// <summary>
    /// Designer-facing configuration shared by every ability type.
    /// </summary>
    public abstract class AbilityConfig : ScriptableObject
    {
        [SerializeField] private float _cooldownDuration;
        [SerializeField] private BuffConfig _onHitBuff;

        public float CooldownDuration => _cooldownDuration;
        // Applied to Target after a successful hit path (instigator = caster); self-buff via ToAbilityTarget.
        public BuffConfig OnHitBuff => _onHitBuff;
    }
}
