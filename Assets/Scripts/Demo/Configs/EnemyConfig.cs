using System;
using UnityEngine;
using Gameplay.Stats;

namespace Demo
{
    /// <summary>
    /// Enemy base stats for Demo entity construction (dummy target; no ability slots).
    /// </summary>
    [CreateAssetMenu(menuName = "Demo/Config/EnemyConfig", fileName = "EnemyConfig")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [SerializeField] private StatBaseEntry[] _statBases;

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
