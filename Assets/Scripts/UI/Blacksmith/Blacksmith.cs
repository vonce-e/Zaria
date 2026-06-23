// This script handles the Blacksmith shop UI Logic.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// The enchantment-room logic.
/// </summary>
public class Blacksmith : MonoBehaviour
{
    // Reference to the card database for rarity-based coin values
    public CardDatabase cardDatabase;

    // Smallest deck size a discard may leave behind
    public int minimumDeckSize = 10;

    /// <summary>
    /// The outcome of a blacksmith action, tells the UI what happened.
    /// </summary>
    public enum Result
    {
        Success,
        NoBudgetLeft,
        AlreadyMaxLevel,
        DeckTooSmall
    }

    /// <summary>
    /// Tries to upgrade one card. Spends 1 enchant point on success.
    /// Fails if the card is already maxed or the budget is empty.
    /// </summary>
    /// <param name="run">The player's run state (holds the budget).</param>
    /// <param name="card">The specific card instance to upgrade.</param>
    public Result UpgradeCard(PlayerRunState run, CardInstance card)
    {
        CardData def = cardDatabase.Get(card.definitionId);

        if (card.upgradeLevel >= def.MaxUpgradeLevel)
            return Result.AlreadyMaxLevel;

        if (run.EnchantRemaining <= 0)
            return Result.NoBudgetLeft;

        card.upgradeLevel++;
        run.enchantSpent++;
        return Result.Success;
    }

    /// <summary>
    /// Removes a card from the deck and gives the player coins for it.
    /// Refuses if removing would put the deck below minimumDeckSize.
    /// </summary>
    /// <param name="run">The player's run state.</param>
    /// <param name="card">The card to discard.</param>
    public Result DiscardCard(PlayerRunState run, CardInstance card)
    {
        if (run.deck.Count - 1 < minimumDeckSize)
            return Result.DeckTooSmall;

        CardData def = cardDatabase.Get(card.definitionId);
        run.deck.Remove(card);
        run.coins += CoinValue(def.rarity);
        return Result.Success;
    }

    /// <summary>
    /// How many coins a card of this rarity is worth when discarded.
    /// </summary>
    /// <param name="rarity">The card's rarity.</param>
    private int CoinValue(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:    return 10;
            case Rarity.Rare:      return 25;
            case Rarity.Mythic:    return 50;
            case Rarity.Legendary: return 100;
            default:               return 10;
        }
    }
}