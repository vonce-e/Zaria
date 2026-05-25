// This script contains the player's three card piles during combat.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deck, hand, and discard pile for one battle. Holds the same CardInstance
/// objects that live in PlayerRunState.deck, never copies. Any
/// upgrade the blacksmith made shows up automatically in combat.
/// </summary>
public class CardPiles
{
    // Face-down draw pile.
    public List<CardInstance> deck = new List<CardInstance>();

    // Cards currently in the player's hand, playable this turn.
    public List<CardInstance> hand = new List<CardInstance>();

    // Used cards. Gets shuffled back when the deck runs out.
    public List<CardInstance> discard = new List<CardInstance>();

    /// <summary>
    /// Call once at the start of every battle. Copies the player's run
    /// deck into the draw pile by reference and shuffles.
    /// </summary>
    /// <param name="run">The player's run state.</param>
    public void StartBattle(PlayerRunState run)
    {
        deck.Clear();
        hand.Clear();
        discard.Clear();
        deck.AddRange(run.deck);
        Shuffle(deck);
    }

    /// <summary>
    /// Draws one card from the top of the deck into the hand. If the deck
    /// is empty, shuffles the discard pile back in first.
    /// </summary>
    public CardInstance DrawCard()
    {
        if (deck.Count == 0)
            ReshuffleDiscardIntoDeck();

        if (deck.Count == 0)
            return null;

        CardInstance drawn = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawn);
        return drawn;
    }

    /// <summary>Draws several cards at once, e.g. an opening hand.</summary>
    /// <param name="amount">How many cards to draw.</param>
    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
            DrawCard();
    }

    /// <summary>
    /// Moves a card from the hand into the discard pile. Called after
    /// a card has been used.
    /// </summary>
    /// <param name="card">The card to discard.</param>
    public void PlayCard(CardInstance card)
    {
        if (hand.Remove(card))
            discard.Add(card);
    }

    /// <summary>
    /// Empties the discard pile back into the deck and shuffles. Called
    /// automatically by DrawCard when the deck runs out.
    /// </summary>
    private void ReshuffleDiscardIntoDeck()
    {
        deck.AddRange(discard);
        discard.Clear();
        Shuffle(deck);
    }

    /// <summary>
    /// Shuffle.
    /// </summary>
    /// <param name="pile">The list to shuffle in place.</param>
    private void Shuffle(List<CardInstance> pile)
    {
        for (int i = pile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pile[i], pile[j]) = (pile[j], pile[i]);
        }
    }
}