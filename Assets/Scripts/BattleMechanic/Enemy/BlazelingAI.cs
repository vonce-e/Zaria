// This script handles the Blazeling enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Blazeling: low-HP glass cannon. Killing it triggers a death explosion.
/// </summary>
public class BlazelingAI : EnemyAI
{
    [Tooltip("Damage dealt to the player when Blazeling dies.")]
    public int deathDamage = 15;

    /// <summary>
    /// Death explosion. Returns the damage for CombatManager to apply
    /// directly to the player during the win flow.
    /// </summary>
    /// <param name="player">The unit caught in the explosion.</param>
    public override int OnDeath(Unit player)
    {
        Debug.Log($"{unit.unitName} explodes for {deathDamage} on death!");
        return deathDamage;
    }
}