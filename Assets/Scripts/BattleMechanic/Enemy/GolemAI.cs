// This script handles the Golem enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Golem: heavy hitting enemy. Its signature is a wind-up, it does nothing this
/// turn but buffs its next basic attack to double damage.
/// </summary>
public class GolemAI : EnemyAI
{
    /// <summary>
    /// Charges up: deals no damage now, but sets the next attack to double.
    /// Returns 0 (nothing to dodge this turn).
    /// </summary>
    /// <param name="player">Unused on the wind-up turn.</param>
    protected override int SignatureDamage(Unit player)
    {
        nextAttackMultiplier = 2;
        Debug.Log($"{unit.unitName}'s signature: winding up - next attack is DOUBLED!");
        return 0;
    }
}