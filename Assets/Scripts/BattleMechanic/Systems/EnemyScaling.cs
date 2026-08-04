// This script scales an enemy's stats based on how deep the player is in the run.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Applies depth-based scaling to an enemy's HP and damage.
/// </summary>
public static class EnemyScaling
{
    // How much stats grow per depth level beyond the first.
    private const float HpGrowthPerDepth     = 0.15f;
    private const float DamageGrowthPerDepth = 0.12f;

    /// <summary>
    /// Scale this enemy's HP and damage for the given depth.
    /// </summary>
    /// <param name="enemy">The spawned enemy's Unit.</param>
    /// <param name="depth">Current run depth (1 = first map).</param>
    public static void Apply(Unit enemy, int depth)
    {
        if (enemy == null) return;
        if (depth < 1) depth = 1;

        // Depth 1 = no scaling
        int steps = depth - 1;
        float hpMultiplier     = 1f + HpGrowthPerDepth * steps;
        float damageMultiplier = 1f + DamageGrowthPerDepth * steps;

        // Scale max HP
        enemy.maxHp = Mathf.RoundToInt(enemy.maxHp * hpMultiplier);
        enemy.currentHp = enemy.maxHp;

        // Scale damage.
        enemy.damage = Mathf.RoundToInt(enemy.damage * damageMultiplier);

        Debug.Log($"Enemy scaled for depth {depth}: " +
                  $"HP x{hpMultiplier:0.00} (={enemy.maxHp}), " +
                  $"DMG x{damageMultiplier:0.00} (={enemy.damage}).");
    }
}