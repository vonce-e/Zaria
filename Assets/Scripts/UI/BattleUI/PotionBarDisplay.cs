// This script handles the on-screen row of potions the player is carrying.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays the player's potion inventory as a row of clickable icons.
/// </summary>
public class PotionBarDisplay : MonoBehaviour
{
    [Header("References")]
    public CombatManager combatManager;
    public PotionDatabase potionDatabase;
    public PlayerRunState run;            // set this when the run/combat starts
    public GameObject potionPrefab;
    public Transform potionRow;

    [Header("Tooltip (optional)")]
    public GameObject tooltipRoot;        // a panel that toggles on hover
    public TMP_Text tooltipText;

    private Dictionary<PotionInstance, PotionView> _visible =
        new Dictionary<PotionInstance, PotionView>();

    void Start()
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }

    void LateUpdate()
    {
        if (combatManager == null || run == null) return;
        Rebuild();
    }

    /// <summary>
    /// Sync the on-screen icons with the run's potion inventory, then
    /// update their usable/greyed state.
    /// </summary>
    private void Rebuild()
    {
        // Add icons for newly-acquired potions.
        foreach (var potion in run.potions)
        {
            if (!_visible.ContainsKey(potion))
                SpawnView(potion);
        }

        // Remove icons for potions no longer held.
        var toRemove = new List<PotionInstance>();
        foreach (var pair in _visible)
            if (!run.potions.Contains(pair.Key))
                toRemove.Add(pair.Key);

        foreach (var potion in toRemove)
        {
            if (_visible[potion] != null)
            {
                _visible[potion].OnClicked -= HandleClicked;
                _visible[potion].OnHoverEnter -= HandleHoverEnter;
                _visible[potion].OnHoverExit -= HandleHoverExit;
                Destroy(_visible[potion].gameObject);
            }
            _visible.Remove(potion);
        }

        RefreshUsable();
    }

    /// <summary>
    /// Create one potion icon and subscribe to its events.
    /// </summary>
    private void SpawnView(PotionInstance potion)
    {
        GameObject go = Instantiate(potionPrefab, potionRow);
        PotionView view = go.GetComponent<PotionView>();

        if (view == null)
        {
            Debug.LogError("Potion prefab is missing a PotionView component.");
            Destroy(go);
            return;
        }

        PotionData data = potionDatabase.Get(potion.definitionId);
        view.Bind(potion, data);
        view.OnClicked += HandleClicked;
        view.OnHoverEnter += HandleHoverEnter;
        view.OnHoverExit += HandleHoverExit;
        _visible[potion] = view;
    }

    /// <summary>
    /// A potion icon was clicked, try to use it.
    /// </summary>
    private void HandleClicked(PotionView view)
    {
        HandleHoverExit();
        combatManager.TryUsePotion(view.PotionInstance);
    }

    /// <summary>
    /// Show the tooltip with the hovered potion's name + description.
    /// </summary>
    private void HandleHoverEnter(PotionData data)
    {
        if (tooltipRoot == null || tooltipText == null || data == null) return;
        tooltipText.text = $"<b>{data.displayName}</b>\n{data.description}";
        tooltipRoot.SetActive(true);
    }

    /// <summary>
    /// Hide the tooltip.
    /// </summary>
    private void HandleHoverExit()
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }

    /// <summary>
    /// Grey out every potion once one has been used this turn; otherwise
    /// keep them all usable.
    /// </summary>
    private void RefreshUsable()
    {
        bool canUse = !combatManager.PotionUsedThisTurn;
        foreach (var pair in _visible)
            pair.Value.SetInteractable(canUse);
    }
}