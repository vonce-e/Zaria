// This script handles the behaviour of each card.
// Made by Vonce Chew

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
        ctx.target.TakeDamage(dmg);
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
        ctx.target.TakeDamage(def.GetDamage(ctx.cardUpgradeLevel));

        int bleedPerTurn = (ctx.diceValue % 2 == 0) ? 4 : 2;
        ctx.target.AddStatus(new DamageOverTime(bleedPerTurn, 2));
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
            // Add the rest of the 20 cards here, one line each.
        };

    // Looks up the behaviour for a card. Returns null if unimplemented.
    public static ICardEffect Get(CardId id)
    {
        return _effects.TryGetValue(id, out var effect) ? effect : null;
    }
}