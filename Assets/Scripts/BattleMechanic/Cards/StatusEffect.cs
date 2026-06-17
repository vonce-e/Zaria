// This script handles multi-turn effects.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Base class for anything that sticks on a Unit and lasts a few turns.
/// </summary>
public abstract class StatusEffect
{
    // How many more turns this status will tick before expiring.
    public int turnsRemaining;

    // Make a new status with the given duration.
    protected StatusEffect(int duration)
    {
        turnsRemaining = duration;
    }

    // Runs at the start of the owner's turn.
    public virtual void OnTurnStart(Unit owner) { }

    // Runs once when the status wears off.
    public virtual void OnExpire(Unit owner) { }

    // True for statuses that stop a unit from taking its turn.
    public virtual bool BlocksTurn => false;
}

/// <summary>
/// Damage every turn status effect for bleed and poison.
/// Used by Deep Cut, Hound's signature, Vatrax's signature.
/// </summary>
public class DamageOverTime : StatusEffect
{
    private readonly int _damagePerTurn;

    // Bleed/poison effect.
    public DamageOverTime(int damagePerTurn, int duration) : base(duration)
    {
        _damagePerTurn = damagePerTurn;
    }

    public override void OnTurnStart(Unit owner)
    {
        owner.TakeTrueDamage(_damagePerTurn);
        Debug.Log($"{owner.unitName} takes {_damagePerTurn} lingering damage " +
                  $"({turnsRemaining - 1} turn(s) left).");
    }
}

/// <summary>
/// Weaken a stat each turn.
/// Suppressor: "reduces your base energy by 1 for 2 turns."
/// </summary>
public class EnergyDrain : StatusEffect
{
    private readonly int _amount;

    // Energy drain effect.
    public EnergyDrain(int amount, int duration) : base(duration)
    {
        _amount = amount;
    }

    public override void OnTurnStart(Unit owner)
    {
        owner.energy = Mathf.Max(0, owner.energy - _amount);
        Debug.Log($"{owner.unitName} is suppressed (-{_amount} energy this turn).");
    }
}

/// <summary>
/// Skip a turn, freeze or stun.
/// Stasis Chamber potion: "freeze your opponent's next turn."
/// </summary>
public class SkipTurn : StatusEffect
{
    // Make a freeze/stun effect.
    public SkipTurn(int duration) : base(duration) { }

    public override bool BlocksTurn => true;
}

/// <summary>
/// Set by the Parry card. If the unit holding it is attacked while it's
/// active, the attacker takes reflectAmount damage back. Lasts one turn.
/// </summary>
public class ReflectNextTurn : StatusEffect
{
    public int reflectAmount;

    public ReflectNextTurn(int amount) : base(1)   // 1-turn duration
    {
        reflectAmount = amount;
    }

    // Marker status - the reflection is checked by the attack flow, not here.
    public override void OnTurnStart(Unit owner) { }
}