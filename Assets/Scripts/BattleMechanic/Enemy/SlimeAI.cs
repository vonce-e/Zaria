// This script handles the Slime enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Slime: Its signature halves incoming damage until its next turn.
/// </summary>
public class SlimeAI : EnemyAI
{
    [Range(0f, 1f)]
    [Tooltip("Fraction of incoming damage absorbed while the signature is active.")]
    public float absorbFraction = 0.5f;

    /// <summary>
    /// Reset the absorption at the start of each of Slime's turns, so the
    /// buff only lasts through the player's single turn in between.
    /// </summary>
    public override void OnEnemyTurnStart()
    {
        unit.damageReduction = 0f;
    }

    /// <summary>
    /// Harden: gain damage absorption until next turn.
    /// </summary>
    /// <param name="player">Unused for this signature.</param>
    protected override int SignatureDamage(Unit player)
    {
        unit.damageReduction = absorbFraction;
        Debug.Log($"{unit.unitName}'s signature: absorbing {absorbFraction * 100f}% " +
                  "damage until its next turn.");
        return 0;
    }
}