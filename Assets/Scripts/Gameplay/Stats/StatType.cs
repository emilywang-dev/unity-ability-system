namespace Gameplay.Stats
{
    /// <summary>
    /// Gameplay stat identifiers with reserved numeric bands so new categories can slot in without renumbering.
    /// </summary>
    public enum StatType
    {
        // Rejected by validation — not a gameplay stat.
        None = 0,

        // Attributes (100-199)
        MaxHealth = 100,
        AttackPower = 120,
        Defense = 130,

        // Ratings (200-299)
        CritChance = 200,
        CritDamage = 210,

        // Movement (300-399)
        MoveSpeed = 300,
    }
}
