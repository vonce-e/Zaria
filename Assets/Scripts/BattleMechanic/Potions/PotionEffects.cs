// This script handles the behaviour for each potion.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// What an effect needs to do its job: who's drinking it, the enemy, and a
/// reference to combat for hand/turn/energy operations.
/// </summary>
public class PotionContext
{
    public Unit player;
    public Unit enemy;
    public CombatManager combat;
}

/// <summary>
/// Every potion's behaviour implements this interface.
/// </summary>
public interface IPotionEffect
{
    void Apply(PotionContext ctx);
}

/// <summary>
/// Phantom Guard: gain defense equal to your missing health.
/// The lower you are, the bigger the shield.
/// </summary>
public class PhantomGuardPotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        int missing = ctx.player.maxHp - ctx.player.currentHp;
        ctx.player.block += missing;
        Debug.Log($"Phantom Guard: +{missing} block (equal to missing HP).");
    }
}

/// <summary>
/// Stasis Chamber: freeze the enemy's next turn, but lose 3 energy now.
/// </summary>
public class StasisChamberPotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        ctx.enemy.AddStatus(new SkipTurn(1));
        ctx.player.energy = Mathf.Max(0, ctx.player.energy - 3);
        Debug.Log("Stasis Chamber: enemy frozen next turn, -3 energy.");
    }
}

/// <summary>
/// New Beginning: discard your current hand and draw a fresh one.
/// </summary>
public class NewBeginningPotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        if (ctx.combat != null)
            ctx.combat.DiscardHandAndRedraw();
        Debug.Log("New Beginning: drew a fresh hand.");
    }
}

/// <summary>
/// Glass Cannon: double your attack damage this turn, but take 50% more
/// damage on the next enemy attack.
/// </summary>
public class GlassCannonPotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        ctx.player.outgoingDamageMultiplier = 2f;
        ctx.player.outgoingMultiplierTurns = 1;   // this turn only

        ctx.player.incomingDamageMultiplier = 1.5f;
        ctx.player.incomingMultiplierTurns = 1;   // through the next enemy hit
        Debug.Log("Glass Cannon: 2x damage this turn, but +50% damage taken.");
    }
}

/// <summary>
/// Recharge: gain 5 energy now, but deal 33% less damage for 2 turns.
/// </summary>
public class RechargePotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        ctx.player.energy += 5;

        ctx.player.outgoingDamageMultiplier = 0.67f;
        ctx.player.outgoingMultiplierTurns = 2;
        Debug.Log("Recharge: +5 energy, but -33% damage for 2 turns.");
    }
}

/// <summary>
/// Grants energy at the start of the owner's turn for a few turns. Used by the Sacrifice potion.
/// </summary>
public class EnergyRegen : StatusEffect
{
    public int amountPerTurn;

    public EnergyRegen(int amount, int duration) : base(duration)
    {
        amountPerTurn = amount;
    }

    /// <summary>
    /// Add energy at the owner's turn start (after their normal reset).
    /// </summary>
    public override void OnTurnStart(Unit owner)
    {
        owner.energy += amountPerTurn;
    }
}

/// <summary>
/// Sacrifice: lose 10% of your max HP, but gain +2 energy for the next 2
/// turns. Won't reduce you below 1 HP.
/// </summary>
public class SacrificePotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        int loss = Mathf.RoundToInt(ctx.player.maxHp * 0.1f);
        int safeLoss = Mathf.Min(loss, ctx.player.currentHp - 1);  // keep >= 1 HP
        if (safeLoss > 0)
            ctx.player.TakeTrueDamage(safeLoss);

        ctx.player.AddStatus(new EnergyRegen(2, 2));
        Debug.Log($"Sacrifice: lost {safeLoss} HP, +2 energy for 2 turns.");
    }
}

/// <summary>
/// Rewind: return the last card you played to your hand. Costs 1 energy
/// now.
/// </summary>
public class RewindPotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        if (ctx.combat != null && ctx.combat.ReturnLastPlayedCardToHand())
        {
            ctx.player.energy = Mathf.Max(0, ctx.player.energy - 1);
            Debug.Log("Rewind: returned your last card to hand (-1 energy).");
        }
        else
        {
            Debug.Log("Rewind: no card to return.");
        }
    }
}

/// <summary>
/// Time Warp : take an extra turn immediately with fresh resources, but skip
/// your next turn as the cost.
/// </summary>
public class TimeWarpPotion : IPotionEffect
{
    public void Apply(PotionContext ctx)
    {
        if (ctx.combat != null)
            ctx.combat.GrantExtraTurn();
    }
}

/// <summary>
/// The lookup table mapping each PotionId to its effect. Mirrors
/// CardEffectRegistry. Add a line per potion as you implement it.
/// </summary>
public static class PotionEffectRegistry
{
    private static readonly Dictionary<PotionId, IPotionEffect> _effects =
        new Dictionary<PotionId, IPotionEffect>
        {
            { PotionId.PhantomGuard,  new PhantomGuardPotion()  },
            { PotionId.StasisChamber, new StasisChamberPotion() },
            { PotionId.NewBeginning,  new NewBeginningPotion()  },
            { PotionId.GlassCannon,   new GlassCannonPotion()   },
            { PotionId.Recharge,      new RechargePotion()      },
            { PotionId.TimeWarp,      new TimeWarpPotion()      },
            { PotionId.Sacrifice,     new SacrificePotion()     },
            { PotionId.Rewind,        new RewindPotion()        },
        };

    /// <summary>
    /// Find a potion's behaviour. Returns null if unimplemented.
    /// </summary>
    public static IPotionEffect Get(PotionId id)
    {
        return _effects.TryGetValue(id, out var effect) ? effect : null;
    }
}