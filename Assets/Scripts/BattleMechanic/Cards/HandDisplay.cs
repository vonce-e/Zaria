// This script handles the on-screen row of cards in the player's hand, and keeps the visible cards in sync.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays the player's hand as a row of card prefabs. Drives itself by
/// reading CombatManager.GetHandForTesting.
/// </summary>
public class HandDisplay : MonoBehaviour
{
    [Header("References")]
    public CombatManager combatManager;   // Where the hand lives
    public CardDatabase cardDatabase;     // Looks up CardData
    public GameObject cardPrefab;         // Prefab with CardView on it
    public Transform handRow;             // Parent transform for the row

    // Currently visible card views, keyed by the CardInstance they represent.
    private Dictionary<CardInstance, CardView> _visible = new Dictionary<CardInstance, CardView>();

    void LateUpdate()
    {
        if (combatManager == null) return;
        Rebuild();
    }

    /// <summary>
    /// Compare the current hand to what's on screen. Add views for cards
    /// that just arrived, remove views for cards that just left.
    /// </summary>
    private void Rebuild()
    {
        var hand = combatManager.GetHandForTesting();
        if (hand == null) return;

        // Spawn views for cards in the hand that don't have one yet
        foreach (var card in hand)
        {
            if (!_visible.ContainsKey(card))
                SpawnView(card);
        }

        // Destroy views for cards that are no longer in the hand.
        // Collect first because can't modify _visible while iterating it.
        var toRemove = new List<CardInstance>();
        foreach (var pair in _visible)
        {
            if (!hand.Contains(pair.Key))
                toRemove.Add(pair.Key);
        }

        foreach (var card in toRemove)
        {
            if (_visible[card] != null)
                Destroy(_visible[card].gameObject);
            _visible.Remove(card);
        }
    }

    /// <summary>
    /// Create one card view and add it to the row.
    /// </summary>
    private void SpawnView(CardInstance card)
    {
        GameObject go = Instantiate(cardPrefab, handRow);
        CardView view = go.GetComponent<CardView>();

        if (view == null)
        {
            Debug.LogError("Card prefab is missing a CardView component.");
            Destroy(go);
            return;
        }

        CardData data = cardDatabase.Get(card.definitionId);
        view.Bind(card, data);
        _visible[card] = view;
    }
}