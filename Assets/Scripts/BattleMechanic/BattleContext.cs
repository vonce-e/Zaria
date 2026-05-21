// This script handles everything a card's effect needs to know when it's played.
// Made by Vonce Chew

using System.Collections.Generic;

/// <summary>
/// A bundle of "what's happening right now" passed to a card's effect when
/// it's played. Built fresh every time a card is used, then thrown away.
/// </summary>
public class BattleContext
{
    // Who played the card
    public Unit caster;

    // The unit the card affects
    public Unit target;

    // The dice value rolled at the start of this turn
    public int diceValue;

    // The upgrade level of the specific card being played
    public int cardUpgradeLevel;

    // Every card already played this turn, in order
    public List<CardId> cardsPlayedThisTurn;
}