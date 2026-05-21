// This script handles the HP, energy, block, and a list of active status effects during combat.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds combat stats and the list of status effects currently active on the player or enemy.
/// </summary>
public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;

    // Base damage this unit deals with a plain attack
    public int damage;

    public int maxHp;
    public int currentHp;

    // Spent to play cards. Reset to a fixed amount each turn.
    public int energy;

    // Defense that soaks damage. Resets to 0 at turn start.
    public int block;

    // Status effects currently affecting this unit.
    public List<StatusEffect> statuses = new List<StatusEffect>();

    /// <summary>
    /// Apply damage. Block soaks the hit first, the rest comes off HP.
    /// Block is consumed by the hit even if it fully absorbed the damage.
    /// </summary>
    /// <param name="amount">Incoming damage before block.</param>
    public void TakeDamage(int amount)
    {
        int afterBlock = amount - block;
        block = Mathf.Max(0, block - amount);

        if (afterBlock > 0)
            currentHp -= afterBlock;

        if (currentHp < 0)
            currentHp = 0;
    }

    /// <summary>
    /// Apply damage that ignores block. Used by bleeds and poison.
    /// </summary>
    /// <param name="amount">Damage to deal straight to HP.</param>
    public void TakeTrueDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp < 0)
            currentHp = 0;
    }

    // True when unit's HP has reached 0.
    public bool IsDead => currentHp <= 0;

    // Attaches a new status effect to this unit
    public void AddStatus(StatusEffect status)
    {
        statuses.Add(status);
    }

    // True if any active status prevents this unit from taking its turn, e.g. freeze.
    public bool HasBlockingStatus()
    {
        foreach (var s in statuses)
            if (s.BlocksTurn && s.turnsRemaining > 0)
                return true;
        return false;
    }

    /// <summary>
    /// Run all status effects once, count their duration down, and remove
    /// any that have expired. Called at the start of each turn.
    /// </summary>
    public void TickStatuses()
    {
        var snapshot = new List<StatusEffect>(statuses);
        foreach (var s in snapshot)
        {
            s.OnTurnStart(this);
            s.turnsRemaining--;

            if (s.turnsRemaining <= 0)
            {
                s.OnExpire(this);
                statuses.Remove(s);
            }
        }
    }
}