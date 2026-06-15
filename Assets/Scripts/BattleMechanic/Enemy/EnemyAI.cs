// This script is the enemy's brain. Picks attack/defend/signature each turn and reveals 
// its choice one turn early so the player can plan around it.
// Made by Vonce Chew

using UnityEngine;
 
/// <summary>
/// The categories of move an enemy can plan.
/// </summary>
public enum EnemyActionType { Attack, Defend, Signature }

/// <summary>
/// What the enemy plans to do. The player sees this as the hint label
/// above the enemy during the player's own turn.
/// </summary>
[System.Serializable]
public class EnemyIntent
{
    public EnemyActionType type;
    public int value;
}

/// <summary>
/// Base enemy AI. Each turn it weighs attack vs defend vs signature and
/// publishes its choice so the player can see it before the enemy acts.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    /// <summary>
    /// This enemy's own Unit component.
    /// </summary>
    public Unit unit;
 
    [Header("Behaviour (0 = never, 1 = always)")]
    [Range(0f, 1f)] public float defendChance = 0.25f;
    [Range(0f, 1f)] public float signatureChance = 0.15f;
    public int defendAmount = 5;
 
    /// <summary>
    /// The current plan. Read by the hint UI and ExecuteIntent.
    /// </summary>
    public EnemyIntent CurrentIntent { get; private set; }
 
    /// <summary>
    /// Choose the next move. Defensive bias rises when HP is low.
    /// </summary>
    public void DecideNextIntent()
    {
        bool lowHp = unit.currentHp <= unit.maxHp * 0.3f;
        float defend = defendChance + (lowHp ? 0.3f : 0f);
 
        EnemyIntent intent = new EnemyIntent();
 
        if (Random.value < signatureChance)
        {
            intent.type = EnemyActionType.Signature;
            intent.value = unit.damage * 2;
        }
        else if (Random.value < defend)
        {
            intent.type = EnemyActionType.Defend;
            intent.value = defendAmount;
        }
        else
        {
            intent.type = EnemyActionType.Attack;
            intent.value = unit.damage;
        }
 
        CurrentIntent = intent;
    }
 
    /// <summary>
    /// Carry out the planned move. Defend applies its own block immediately.
    /// Attack and Signature return their damage so CombatManager can route it
    /// through the parry/dodge bar before applying it.
    /// </summary>
    /// <param name="player">The unit being acted on.</param>
    /// <returns>Damage to be applied to the player (0 for defend).</returns>
    public int ExecuteIntent(Unit player)
    {
        switch (CurrentIntent.type)
        {
            case EnemyActionType.Attack:
                Debug.Log($"{unit.unitName} attacks for {unit.damage}.");
                return unit.damage;

            case EnemyActionType.Defend:
                unit.block += defendAmount;
                Debug.Log($"{unit.unitName} defends (+{defendAmount} block).");
                return 0;

            case EnemyActionType.Signature:
                return SignatureDamage(player);
        }
        return 0;
    }
 
    /// <summary>
    /// Short label for the on-screen hint. The TelegraphUI text reads this
    /// each frame to display the planned move.
    /// </summary>
    public string IntentLabel()
    {
        switch (CurrentIntent.type)
        {
            case EnemyActionType.Attack:    return $"Attack ({CurrentIntent.value})";
            case EnemyActionType.Defend:    return "Defend";
            case EnemyActionType.Signature: return "Special!";
            default:                        return "?";
        }
    }
 
    /// <summary>
    /// Each enemy's unique move. Default returns double damage. Override in a
    /// subclass to give a specific enemy its real signature. Non-damage
    /// signatures like heals and buffs can apply their effect here and return 0.
    /// </summary>
    /// <param name="player">The unit being acted on.</param>
    /// <returns>Damage to route through the timing bar.</returns>
    protected virtual int SignatureDamage(Unit player)
    {
        int dmg = unit.damage * 2;
        Debug.Log($"{unit.unitName} uses its signature for {dmg}!");
        return dmg;
    }
}