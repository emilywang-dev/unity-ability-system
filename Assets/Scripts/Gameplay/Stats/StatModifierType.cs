namespace Gameplay.Stats
{
    /// <summary>
    /// Additive offset before multiply, or Percent summed into one multiplier (not chained per-modifier).
    /// </summary>
    public enum StatModifierType
    {
        Additive = 0,
        Percent
    }
}