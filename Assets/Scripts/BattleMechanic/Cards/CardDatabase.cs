// A list of every CardData asset
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Zaria/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> allCards = new List<CardData>();

    private Dictionary<CardId, CardData> _lookup;

    public CardData Get(CardId id)
    {
        // Build the lookup table the first time it's used.
        if (_lookup == null)
        {
            _lookup = new Dictionary<CardId, CardData>();
            foreach (var card in allCards)
                _lookup[card.id] = card;
        }
        return _lookup[id];
    }
}