// This script handles the fighter in combat: HP, energy, block, and a list of active status effects.
// Made by Vonce Chew

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds combat stats for the player or the enemy, and
/// the list of status effects currently active on them. Fires OnHpChanged
/// whenever HP changes so HUDs and other listeners can react.
/// </summary>
public class Unit : MonoBehaviour
{
    public string unitName;
    public int unitLevel;

    // Base damage this unit deals with a plain attack
    public int damage;

    public int maxHp;
    public int currentHp;

    // Energy spent to play cards, resets to a fixed amount each turn
    [SerializeField]
    private int _energy;

    // Defense that soaks damage, resets to 0 at start of turn
    public int block;

    // Fraction of incoming damage absorbed before block (0 = none, 0.5 = half),
    // set by enemies like Slime.Reset by the enemy itself.
    [Range(0f, 1f)] public float damageReduction = 0f;

    // The most recent damage this unit took, used by Temporal Guard.
    public int lastDamageTaken;

    // Status effects currently affecting this unit
    public List<StatusEffect> statuses = new List<StatusEffect>();

    /// <summary>
    /// Fires every time HP changes. Listeners such as HUDs, particle effects,
    /// death triggers, subscribe to this and react. Passes the new HP value.
    /// </summary>
    public event Action<int> OnHpChanged;

    /// <summary>
    /// Fires every time energy changes. Passes the new energy value.
    /// </summary>
    public event Action<int> OnEnergyChanged;

    /// <summary>
    /// Energy used to play cards. Reset each turn. Wrapped as a property so
    /// setting it from anywhere automatically fires OnEnergyChanged.
    /// </summary>
    public int energy
    {
        get => _energy;
        set
        {
            if (_energy == value) return;
            _energy = value;
            OnEnergyChanged?.Invoke(_energy);
        }
    }

    /// <summary>
    /// Apply damage. Block soaks the hit first, the rest comes off HP.
    /// Block is consumed by the hit even if it fully absorbed the damage.
    /// </summary>
    /// <param name="amount">Incoming damage before block.</param>
    public void TakeDamage(int amount)
    {
        // Absorption (e.g. Slime) reduces damage before block applies.
        if (damageReduction > 0f)
            amount = Mathf.RoundToInt(amount * (1f - damageReduction));

        int afterBlock = amount - block;
        block = Mathf.Max(0, block - amount);

        if (afterBlock > 0)
        {
            lastDamageTaken = afterBlock;
            SetHp(currentHp - afterBlock);
        }
    }

    /// <summary>
    /// Apply damage that ignores block. Used by bleeds and poison.
    /// </summary>
    /// <param name="amount">Damage to deal straight to HP.</param>
    public void TakeTrueDamage(int amount)
    {
        SetHp(currentHp - amount);
    }

    /// <summary>
    /// Heal this unit. Cannot exceed maxHp.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    public void Heal(int amount)
    {
        SetHp(currentHp + amount);
    }

    /// <summary>
    /// Internal HP setter. Clamps to [0, maxHp] and fires the event.
    /// Every HP change goes through here so the event fires always.
    /// </summary>
    /// <param name="newHp">The desired new HP, clamped before being applied.</param>
    private void SetHp(int newHp)
    {
        currentHp = Mathf.Clamp(newHp, 0, maxHp);
        OnHpChanged?.Invoke(currentHp);
    }

    // True when this unit's HP has reached 
    public bool IsDead => currentHp <= 0;

    /// <summary>
    /// Attaches a new status effect to this unit.
    /// </summary>
    public void AddStatus(StatusEffect status)
    {
        statuses.Add(status);
    }

    /// <summary>
    /// True if any active status prevents this unit from taking its turn
    /// e.g. a freeze, checked before statuses tick.
    /// </summary>
    public bool HasBlockingStatus()
    {
        foreach (var s in statuses)
            if (s.BlocksTurn && s.turnsRemaining > 0)
                return true;
        return false;
    }

    /// <summary>
    /// Run all status effects once, count their duration down, and remove
    /// any ones that have expired. This is called at the start of each turn.
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