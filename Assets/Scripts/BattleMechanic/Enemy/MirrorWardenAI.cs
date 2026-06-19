// This script handles the Mirror Warden enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Mirror Warden: a reactive enemy. Its signature reflects the player's own
/// last attack back at reduced power.
/// </summary>
public class MirrorWardenAI : EnemyAI
{
    [Range(0f, 1f)]
    [Tooltip("Fraction of the player's last attack damage to copy.")]
    public float copyFraction = 0.7f;

    /// <summary>
    /// Returns 70% of the player's last recorded attack damage as a
    /// dodgeable hit. Returns 0 if the player hasn't attacked yet.
    /// </summary>
    /// <param name="player">The unit being attacked.</param>
    protected override int SignatureDamage(Unit player)
    {
        int copied = Mathf.RoundToInt(lastPlayerAttackDamage * copyFraction);
        Debug.Log($"{unit.unitName}'s signature: mirrors {copied} " +
                  $"(70% of your last {lastPlayerAttackDamage}).");
        return copied;
    }
}