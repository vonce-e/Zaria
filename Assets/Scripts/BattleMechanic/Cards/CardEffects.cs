// This script handles the behaviour of each card.
// Made by Vonce Chew

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Every card's behaviour implements this interface. The combat system
/// looks up the effect for a played card and calls Apply.
/// </summary>
public interface ICardEffect
{
    // Run this card's effect.
    void Apply(BattleContext ctx, CardData def);
}

/// <summary>
/// Plain attack with a dice bonus.
/// For example : Slash, deal 6 damage, if dice >= 5 deal 3 extra.
/// </summary>
public class SlashEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int dmg = def.GetDamage(ctx.cardUpgradeLevel);
        if (ctx.diceValue >= 5) dmg += 3;
        ctx.DealDamage(dmg);
    }
}

/// <summary>
/// Defense card.
/// For example : Guard, gain 6 defense, if dice is odd gain 2 extra.
/// </summary>
public class GuardEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int def_ = def.GetDefense(ctx.cardUpgradeLevel);
        if (ctx.diceValue % 2 == 1) def_ += 2;
        ctx.caster.block += def_;


        DefenseFlash defenseFlash =
            ctx.caster.GetComponent<DefenseFlash>();

        if (defenseFlash != null)
        {
            defenseFlash.ShowDefenseEffect();
        }

        // Synergy with Parry played the same turn: deal 5 back, gain 3 defense.
        if (ctx.cardsPlayedThisTurn.Contains(CardId.Parry))
        {
            ctx.target.TakeDamage(5);
            ctx.caster.block += 3;
        }
    }
}

/// <summary>
/// Synergy card.
/// For example : Twin Strike, 3 dmg x2, x3 if dice >= 3, doubled if Execution Strike
/// was also played this turn.
/// </summary>
public class TwinStrikeEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int hits = ctx.diceValue >= 3 ? 3 : 2;
        int dmg = 3 * hits;
        if (ctx.cardsPlayedThisTurn.Contains(CardId.ExecutionStrike))
            dmg *= 2;
        ctx.target.TakeDamage(dmg);
    }
}

/// <summary>
/// A card that leaves a lasting status.
/// For example : Deep Cut, deal 4 now, then 2 (or 4 if dice even) for the next 2 turns.
/// </summary>
public class DeepCutEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        ctx.DealDamage(def.GetDamage(ctx.cardUpgradeLevel));

        int bleedPerTurn = (ctx.diceValue % 2 == 0) ? 4 : 2;
        ctx.target.AddStatus(new DamageOverTime(bleedPerTurn, 2));
    }
}

/// <summary>
/// Attack with an energy refund on an odd roll.
/// For example : Quick Jab, deal 5 damage, if dice is odd gain 1 energy.
/// </summary>
public class QuickJabEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        ctx.target.TakeDamage(def.GetDamage(ctx.cardUpgradeLevel));
        if (ctx.diceValue % 2 == 1)
            ctx.caster.energy += 1;
    }
}

/// <summary>
/// Defense card with a high-roll bonus.
/// For example : Brace, gain 7 defense, if dice >= 4 gain 4 extra.
/// </summary>
public class BraceEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int def_ = def.GetDefense(ctx.cardUpgradeLevel);
        if (ctx.diceValue >= 4) def_ += 4;
        ctx.caster.block += def_;
    }
}

/// <summary>
/// Defense card that doubles on an even roll.
/// For example : Adaptive Shield, gain 6 defense, if dice is even gain double.
/// </summary>
public class AdaptiveShieldEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int def_ = def.GetDefense(ctx.cardUpgradeLevel);
        if (ctx.diceValue % 2 == 0) def_ *= 2;
        ctx.caster.block += def_;

        DefenseFlash defenseFlash =
        ctx.caster.GetComponent<DefenseFlash>();

        if (defenseFlash != null)
        {
            defenseFlash.ShowDefenseEffect();
        }
    }
    
}

/// <summary>
/// High-risk attack that scales hard with the dice.
/// For example : Chaos Rend, deal 7 x dice value, but 0 damage if dice <= 3.
/// </summary>
public class ChaosRendEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.diceValue <= 3) return;  // whiffs on a low roll
        int dmg = def.GetDamage(ctx.cardUpgradeLevel) * ctx.diceValue;
        ctx.target.TakeDamage(dmg);
    }
}

/// <summary>
/// Pure energy skill.
/// For example : Energize, if dice >= 3 gain 2 energy.
/// </summary>
public class EnergizeEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.diceValue >= 3)
            ctx.caster.energy = Mathf.Min(
            ctx.caster.energy + 3,
            ctx.combat.energyPerTurn);

        DefenseFlash defenseFlash =
        ctx.caster.GetComponent<DefenseFlash>();

        if (defenseFlash != null)
        {
            defenseFlash.ShowDefenseEffect();
        }
    }


}

/// <summary>
/// Execution attack: huge bonus against a wounded enemy.
/// Execution Strike, deal 6, if dice >= 4 and enemy HP < 50% deal 10 extra.
/// Doubled if Twin Strike was also played this turn.
/// </summary>
public class ExecutionStrikeEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int dmg = def.GetDamage(ctx.cardUpgradeLevel);

        bool enemyWounded = ctx.target.currentHp < ctx.target.maxHp * 0.5f;
        if (ctx.diceValue >= 4 && enemyWounded)
            dmg += 10;

        if (ctx.cardsPlayedThisTurn.Contains(CardId.TwinStrike))
            dmg *= 2;

        ctx.target.TakeDamage(dmg);
    }
}

/// <summary>
/// Blood attack: hurt yourself, hit hard.
/// Blood Slash, lose 3 HP, deal 10 (13 if dice >= 4). If Deep Cut was
/// played this turn, add 2 extra bleed for 3 turns.
/// </summary>
public class BloodSlashEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        ctx.caster.TakeTrueDamage(3);  // self-damage ignores block

        int dmg = def.GetDamage(ctx.cardUpgradeLevel);
        if (ctx.diceValue >= 4) dmg += 3;
        ctx.DealDamage(dmg);

        if (ctx.cardsPlayedThisTurn.Contains(CardId.DeepCut))
            ctx.target.AddStatus(new DamageOverTime(2, 3));
    }
}

/// <summary>
/// Repeating attack scaled by the dice.
/// Fatal Collapse, deal 10, repeat dice-value times. No repeat if dice <= 2.
/// </summary>
public class FatalCollapseEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int dmg = def.GetDamage(ctx.cardUpgradeLevel);
        int repeats = ctx.diceValue <= 2 ? 1 : ctx.diceValue;

        for (int i = 0; i < repeats; i++)
            ctx.DealDamage(dmg);
    }
}

/// <summary>
/// Defensive reflect setup.
/// Parry, gain 5 defense, if dice >= 4 reflect 5 damage when attacked next
/// turn. If Guard was played this turn, deal 5 back and gain 3 defense.
/// </summary>
public class ParryEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        ctx.caster.block += def.GetDefense(ctx.cardUpgradeLevel);

        if (ctx.diceValue >= 4)
            ctx.caster.AddStatus(new ReflectNextTurn(5));

        // Synergy with Guard played the same turn.
        if (ctx.cardsPlayedThisTurn.Contains(CardId.Guard))
        {
            ctx.target.TakeDamage(5);
            ctx.caster.block += 3;
        }
    }
}

/// <summary>
/// Reactive defense based on recent damage.
/// Temporal Guard, gain defense equal to last damage taken. If dice >= 4, gain immunity next round.
/// </summary>
public class TemporalGuardEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        ctx.caster.block += ctx.caster.lastDamageTaken;

        if (ctx.diceValue >= 4)
            ctx.caster.block += 999;  // effectively immune this round

        DefenseFlash defenseFlash =
        ctx.caster.GetComponent<DefenseFlash>();

        if (defenseFlash != null)
        {
            defenseFlash.ShowDefenseEffect();
        }
    }
}

/// <summary>
/// Re-roll skill: roll a second die, keep whichever is higher.
/// Dice Roll, roll an extra dice, use the higher result.
/// </summary>
public class DiceRollEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.combat != null)
            ctx.combat.RollExtraDiceKeepHigher();
    }
}

/// <summary>
/// Dice floor for this turn.
/// Steady Hand, your dice has to roll a minimum of 3 this turn.
/// </summary>
public class SteadyHandEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.combat != null)
            ctx.combat.SetDiceFloorThisTurn(3);
    }
}

/// <summary>
/// Dice floor for next turn.
/// Loaded Dice, your dice has to roll a minimum of 4 next turn.
/// </summary>
public class LoadedDiceEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.combat != null)
            ctx.combat.SetDiceFloorNextTurn(4);
    }
}

/// <summary>
/// Store the current dice as a floor for next turn (simplified Fate Bank).
/// Fate Bank, store your current dice value to guarantee at least that
/// much on your next roll.
/// </summary>
public class FateBankEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.combat != null)
            ctx.combat.SetDiceFloorNextTurn(ctx.diceValue);
    }
}

/// <summary>
/// Queue a repeat for the next card.
/// Double Down, your next card is played twice if dice >= 3.
/// </summary>
public class DoubleDownEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        if (ctx.diceValue >= 3 && ctx.combat != null)
            ctx.combat.SetRepeatNextCard();
    }
}

/// <summary>
/// Defense scaled by the dice.
/// Rolling Barrier, gain 2 defense, multiply by dice value.
/// </summary>
public class RollingBarrierEffect : ICardEffect
{
    public void Apply(BattleContext ctx, CardData def)
    {
        int block = def.GetDefense(ctx.cardUpgradeLevel) * ctx.diceValue;
        ctx.caster.block += block;

        DefenseFlash defenseFlash =
        ctx.caster.GetComponent<DefenseFlash>();

        if (defenseFlash != null)
        {
            defenseFlash.ShowDefenseEffect();
        }
    }
}

/// <summary>
/// The lookup table that maps each CardId to its effect. The combat system
/// calls Get() when a card is played to find what code to run.
/// </summary>
public static class CardEffectRegistry
{
    private static readonly Dictionary<CardId, ICardEffect> _effects =
        new Dictionary<CardId, ICardEffect>
        {
            { CardId.Slash,      new SlashEffect()      },
            { CardId.Guard,      new GuardEffect()      },
            { CardId.TwinStrike, new TwinStrikeEffect() },
            { CardId.DeepCut,    new DeepCutEffect()    },
            { CardId.QuickJab,       new QuickJabEffect()      },
            { CardId.Brace,          new BraceEffect()         },
            { CardId.AdaptiveShield, new AdaptiveShieldEffect() },
            { CardId.ChaosRend,      new ChaosRendEffect()     },
            { CardId.Energize,       new EnergizeEffect()      },
            { CardId.ExecutionStrike, new ExecutionStrikeEffect() },
            { CardId.BloodSlash,      new BloodSlashEffect()      },
            { CardId.FatalCollapse,   new FatalCollapseEffect()   },
            { CardId.Parry,           new ParryEffect()           },
            { CardId.TemporalGuard,   new TemporalGuardEffect()   },
            { CardId.DiceRoll,       new DiceRollEffect()       },
            { CardId.SteadyHand,     new SteadyHandEffect()     },
            { CardId.LoadedDice,     new LoadedDiceEffect()     },
            { CardId.FateBank,       new FateBankEffect()       },
            { CardId.DoubleDown,     new DoubleDownEffect()     },
            { CardId.RollingBarrier, new RollingBarrierEffect() },
        };

    // Looks up the behaviour for a card. Returns null if unimplemented.
    public static ICardEffect Get(CardId id)
    {
        return _effects.TryGetValue(id, out var effect) ? effect : null;
    }
}