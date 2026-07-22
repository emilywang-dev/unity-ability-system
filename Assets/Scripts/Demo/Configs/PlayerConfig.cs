using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Stats;
using Ability;

namespace Demo
{
    /// <summary>
    /// Player base stats and ability slots for Demo entity construction.
    /// </summary>
    [CreateAssetMenu(menuName = "Demo/Config/PlayerConfig", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField] private StatBaseEntry[] _statBases;
        [SerializeField] private List<AbilitySlot> _abilitySlots = new();

        public IReadOnlyList<AbilitySlot> AbilitySlots => _abilitySlots;

        public void ApplyBaseStats(StatCollection stats)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }

            if (_statBases == null)
            {
                return;
            }

            for (int i = 0; i < _statBases.Length; i++)
            {
                StatBaseEntry entry = _statBases[i];
                stats.SetBaseStat(entry.StatType, entry.BaseValue);
            }
        }
    }
}
