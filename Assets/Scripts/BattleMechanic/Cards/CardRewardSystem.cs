// This script handles card acquisition.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds cards to the player's deck.
/// Enforces the 30 card cap, weights random picks by rarity, and executes
/// an event so UI can react. Chests, combat rewards, and shop all go through AddCard.
/// </summary>
public class CardRewardSystem
{
    private readonly CardDatabase _database;
    private readonly PlayerRunState _run;

    // Max Deck Capacity
    public const int MaxDeckSize = 30;

    /// UI Popup when card is offered
    public enum AddResult { Added, DeckFull }

    /// <summary>
    /// Fires whenever a card is offered, whether accepted or refused.
    /// </summary>
    public System.Action<CardData, AddResult> OnCardOffered;

    /// <summary>
    /// Build the system around a specific database and run state.
    /// </summary>
    /// <param name="database">The full card database for lookups.</param>
    /// <param name="run">The player's run state (where the deck lives).</param>
    public CardRewardSystem(CardDatabase database, PlayerRunState run)
    {
        _database = database;
        _run = run;
    }

    /// <summary>
    /// The core add. Adds a specific card to the deck if room exists,
    /// otherwise refuses. Fires OnCardOffered either way so the UI knows.
    /// </summary>
    /// <param name="id">The card to add.</param>
    public AddResult AddCard(CardId id)
    {
        if (_run.deck.Count >= MaxDeckSize)
        {
            CardData defFull = _database.Get(id);
            OnCardOffered?.Invoke(defFull, AddResult.DeckFull);
            Debug.Log($"Deck full ({MaxDeckSize}/{MaxDeckSize}). " +
                      $"{defFull.displayName} was refused.");
            return AddResult.DeckFull;
        }

        _run.deck.Add(new CardInstance(id));
        CardData def = _database.Get(id);
        OnCardOffered?.Invoke(def, AddResult.Added);
        Debug.Log($"Added {def.displayName} to deck " +
                  $"({_run.deck.Count}/{MaxDeckSize}).");
        return AddResult.Added;
    }

    /// <summary>
    /// Picks a random card of the given rarity from the database, then adds it.
    /// </summary>
    /// <param name="rarity">Which rarity tier to draw from.</param>
    public AddResult GrantRandomCardOfRarity(Rarity rarity)
    {
        List<CardData> pool = new List<CardData>();
        foreach (var card in _database.allCards)
            if (card.rarity == rarity)
                pool.Add(card);

        if (pool.Count == 0)
        {
            Debug.LogWarning($"No cards of rarity {rarity} in the database.");
            return AddResult.DeckFull;
        }

        CardData pick = pool[Random.Range(0, pool.Count)];
        return AddCard(pick.id);
    }

    /// <summary>
    /// Rolls a rarity using the given weights, then grants a random card
    /// of that rarity.
    /// </summary>
    /// <param name="commonWeight">Relative chance of Common.</param>
    /// <param name="rareWeight">Relative chance of Rare.</param>
    /// <param name="mythicWeight">Relative chance of Mythic.</param>
    /// <param name="legendaryWeight">Relative chance of Legendary.</param>
    public AddResult GrantRandomReward(int commonWeight = 70,
                                       int rareWeight = 25,
                                       int mythicWeight = 5,
                                       int legendaryWeight = 0)
    {
        int total = commonWeight + rareWeight + mythicWeight + legendaryWeight;
        int roll = Random.Range(0, total);

        Rarity chosen;
        if (roll < commonWeight) chosen = Rarity.Common;
        else if (roll < commonWeight + rareWeight) chosen = Rarity.Rare;
        else if (roll < commonWeight + rareWeight + mythicWeight) chosen = Rarity.Mythic;
        else chosen = Rarity.Legendary;

        return GrantRandomCardOfRarity(chosen);
    }
}