// This script handles the Suppressor enemy.

using UnityEngine;

/// <summary>
/// Suppressor: a control enemy. Its signature cuts the player's energy,
/// limiting how many cards they can play for two turns.
/// </summary>
public class SuppressorAI : EnemyAI
{
    /// <summary>
    /// Applies a 2-turn energy drain.
    /// </summary>
    /// <param name="player">The unit to suppress.</param>
    protected override int SignatureDamage(Unit player)
    {
        player.AddStatus(new EnergyDrain(1, 2));
        Debug.Log($"{unit.unitName}'s signature: -1 energy for 2 turns.");
        return 0;
    }
}