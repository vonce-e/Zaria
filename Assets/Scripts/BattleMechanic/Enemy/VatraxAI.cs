// This script handles the Vatrax boss enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Vatrax: a high-damage boss. Its signature combines a dodgeable burst
/// hit with a damage-over-time burn the player can't react to.
/// </summary>
public class VatraxAI : EnemyAI
{
    /// <summary>
    /// Applies a 2-turn burn directly, then returns the 12 burst damage so
    /// the burst routes through the parry/dodge bar.
    /// </summary>
    /// <param name="player">The unit being attacked.</param>
    protected override int SignatureDamage(Unit player)
    {
        player.AddStatus(new DamageOverTime(6, 2));
        Debug.Log($"{unit.unitName}'s signature: 12 burst + 6 burn for 2 turns.");
        return 12;  // dodgeable burst
    }
}