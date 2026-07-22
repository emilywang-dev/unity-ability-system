using System;
using Gameplay.Stats;

namespace Demo
{
    /// <summary>
    /// One base-stat row for Demo entity configs (<see cref="PlayerConfig"/> / <see cref="EnemyConfig"/>).
    /// </summary>
    [Serializable]
    public struct StatBaseEntry
    {
        public StatType StatType;
        public float BaseValue;
    }
}
