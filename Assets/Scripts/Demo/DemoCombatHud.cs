using UnityEngine;

namespace Demo
{
    /// <summary>
    /// Minimal Play-mode overlay: HP and primary-slot cooldown for Demo recordings.
    /// </summary>
    public sealed class DemoCombatHud : MonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private Enemy _enemy;

        private void OnGUI()
        {
            if (_player == null || _enemy == null || !_player.IsInitialized || !_enemy.IsInitialized)
            {
                return;
            }

            float playerHp = _player.Damageable.CurrentHealth;
            float enemyHp = _enemy.Damageable.CurrentHealth;
            float cooldown = _player.AbilitySystem.CooldownQuery.GetNormalized(Player.PrimaryAbilitySlot);

            GUILayout.BeginArea(new Rect(12f, 12f, 280f, 90f));
            GUILayout.Label($"Player HP: {playerHp:0}");
            GUILayout.Label($"Enemy HP: {enemyHp:0}");
            GUILayout.Label($"Cooldown: {cooldown:0.00}");
            GUILayout.EndArea();
        }
    }
}
