// A list of every CardData asset
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A phonebook of every card in the game.
/// </summary>
[CreateAssetMenu(menuName = "Zaria/Card Database")]
public class CardDatabase : ScriptableObject
{
    /// Every CardData asset in the game.
    public List<CardData> allCards = new List<CardData>();

    private Dictionary<CardId, CardData> _lookup;

    /// <summary>
    /// Returns the CardData for the given id. Builds the lookup table the
    /// first time it's called, then caches it for the rest of the session.
    /// </summary>
    /// <param name="id">The card to find.</param>
    public CardData Get(CardId id)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<CardId, CardData>();
            foreach (var card in allCards)
                _lookup[card.id] = card;
        }
        return _lookup[id];
    }
}