namespace Gameplay.Stats
{
    /// <summary>
    /// Read-only access to clamped computed stat values.
    /// </summary>
    public interface IReadOnlyStatCollection
    {
        /// <summary>
        /// Returns the clamped computed value. The stat must already have a base value set, or this throws.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The stat has not been initialized.</exception>
        float GetValue(StatType statType);

        /// <summary>
        /// Same as <see cref="GetValue"/>, but returns false instead of throwing when the stat was never initialized.
        /// </summary>
        bool TryGetValue(StatType statType, out float value);
    }
}
