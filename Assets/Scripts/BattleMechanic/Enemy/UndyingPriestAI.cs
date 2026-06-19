// This script handles the Undying Priest enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Undying Priest: a stalling enemy. Once per fight, when it falls below
/// half health, it heals back to full at the start of its turn.
/// </summary>
public class UndyingPriestAI : EnemyAI
{
    private bool _hasRegenerated = false;

    /// <summary>
    /// Checked at the start of the priest's turn. If below 50% and it
    /// hasn't healed yet this combat, restore to full HP.
    /// </summary>
    public override void OnEnemyTurnStart()
    {
        if (_hasRegenerated) return;
        if (unit.currentHp > unit.maxHp * 0.5f) return;

        unit.currentHp = unit.maxHp;
        _hasRegenerated = true;
        Debug.Log($"{unit.unitName} regenerates to full HP! (once per combat)");
    }
}