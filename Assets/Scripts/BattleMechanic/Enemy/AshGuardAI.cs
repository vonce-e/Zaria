// This script handles the Ash Guard enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Ash Guard : slow and tanky. Its signature hardens its guard instead of
/// attacking, making it hard to break through.
/// </summary>
public class AshGuardAI : EnemyAI
{
    /// <summary>
    /// Adds 5 block to itself.
    /// </summary>
    /// <param name="player">Unused for this signature.</param>
    protected override int SignatureDamage(Unit player)
    {
        unit.block += 5;
        Debug.Log($"{unit.unitName}'s signature: hardens guard (+5 block).");
        return 0;
    }
}