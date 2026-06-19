// This script handles the Hound enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Hound: fast, low-HP attacker. Its signature applies a damage-over-time
/// bleed rather than a burst hit.
/// </summary>
public class HoundAI : EnemyAI
{
    /// <summary>
    /// Applies a 3-turn bleed instead of a burst hit.
    /// </summary>
    /// <param name="player">The unit to bleed.</param>
    protected override int SignatureDamage(Unit player)
    {
        player.AddStatus(new DamageOverTime(4, 3));
        Debug.Log($"{unit.unitName}'s signature: 4 bleed for 3 turns.");
        return 0;
    }
}