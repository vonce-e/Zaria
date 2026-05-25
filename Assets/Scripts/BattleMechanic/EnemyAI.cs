// This script is a placeholder for the actual enemy brain in the future. It only attacks.
// Made by Vonce Chew

using UnityEngine;

/// <summary>The categories of move an enemy can plan.</summary>
public enum EnemyActionType { Attack, Defend, Signature }

/// <summary>
/// What the enemy plans to do. Read by CombatManager when the enemy acts
/// and by the on screen hint popup.
/// </summary>
[System.Serializable]
public class EnemyIntent
{
    public EnemyActionType type;
    public int value;
}

/// <summary>
/// Minimal enemy brain for testing. Always picks Attack.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    // This enemy's own Unit component
    public Unit unit;

    // Enemy's current intent, read by CombatManager when the enemy acts
    public EnemyIntent CurrentIntent { get; private set; }

    // Always picks Attack
    public void DecideNextIntent()
    {
        CurrentIntent = new EnemyIntent
        {
            type = EnemyActionType.Attack,
            value = unit.damage
        };
    }

    // Carry out the planned move 
    public void ExecuteIntent(Unit player)
    {
        player.TakeDamage(unit.damage);
        Debug.Log($"{unit.unitName} attacks for {unit.damage}.");
    }

    // Short label for the hint popup
    public string IntentLabel()
    {
        return $"Attack ({CurrentIntent.value})";
    }
}