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
    public CombatManager combatManager; // Where the hand lives
    public CardDatabase cardDatabase; // Looks up CardData
    public GameObject cardPrefab; // Prefab with CardView on it
    public Transform handRow; // Parent transform for the row

    /// <summary>
    /// The player whose energy this display tracks (for greying out unaffordable cards).
    /// </summary>
    public Unit player;

    // Currently visible card views, keyed by the CardInstance they represent.
    private Dictionary<CardInstance, CardView> _visible = new Dictionary<CardInstance, CardView>();

    /// <summary>
    /// Subscribe to the player's energy changes once at startup.
    /// </summary>
    void Start()
    {
        if (player != null)
            player.OnEnergyChanged += HandleEnergyChanged;
    }

    /// <summary>
    /// Unsubscribe to prevent memory leaks when this object is destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (player != null)
            player.OnEnergyChanged -= HandleEnergyChanged;
    }
    
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
            {
                _visible[card].OnClicked -= HandleCardClicked;
                Destroy(_visible[card].gameObject);
            }
            _visible.Remove(card);
        }

        RefreshAffordability();
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
        view.OnClicked += HandleCardClicked;
        _visible[card] = view;
    }

    /// <summary>
    /// A card was clicked, asks CombatManager to play it.
    /// </summary>
    /// <param name="view">The card view the player clicked.</param>
    private void HandleCardClicked(CardView view)
    {
        combatManager.TryPlayCard(view.CardInstance);
    }

    /// <summary>
    /// Energy changed, re-evaluate which cards are playable.
    /// </summary>
    /// <param name="newEnergy">The player's new energy value (unused; we just need a refresh).</param>
    private void HandleEnergyChanged(int newEnergy)
    {
        RefreshAffordability();
    }

    /// <summary>
    /// Update each visible card's playable state based on the player's
    /// current energy. Cards too expensive are greyed out and unclickable.
    /// </summary>
    private void RefreshAffordability()
    {
        if (player == null) return;

        foreach (var pair in _visible)
        {
            CardData data = pair.Value.CardData;
            bool canAfford = player.energy >= data.energyCost;
            pair.Value.SetInteractable(canAfford);
        }
    }
}