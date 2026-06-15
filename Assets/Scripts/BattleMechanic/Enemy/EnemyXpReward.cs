// This script handles how much XP this enemy grants when defeated.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Drag onto any enemy GameObject. CombatManager reads this on Win()
/// to know how much XP to grant the player.
/// </summary>
public class EnemyXpReward : MonoBehaviour
{
    [Tooltip("XP granted to the player when this enemy is defeated.")]
    public int xpReward = 10;
}