// This script handles the chest interactions.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Grants cards and coins to the persistent run on interaction, then disables itself.
/// </summary>
public class ChestInteraction : MonoBehaviour
{
    [Header("Loot")]
    [Tooltip("How many cards the chest gives.")]
    public int cardCount = 1;
    [Tooltip("How many coins the chest gives.")]
    public int coinReward = 30;

    private bool _opened = false;

    /// <summary>
    /// Called by the Interactable's onInteract event. Grants the
    /// loot once, then marks the chest empty.
    /// </summary>
    public void OpenChest()
    {
        if (_opened)
        {
            Debug.Log("This chest is already empty.");
            return;
        }

        if (RunManager.Instance == null)
        {
            Debug.LogWarning("ChestInteraction: no RunManager - is a run active?");
            return;
        }

        // coins straight to the run
        RunManager.Instance.runState.coins += coinReward;

        // cards via the existing reward service
        if (CardRewardService.Instance != null)
        {
            for (int i = 0; i < cardCount; i++)
                CardRewardService.Instance.OpenChest();  // grants a random card
        }

        _opened = true;
        Debug.Log($"Chest opened: +{coinReward} coins, +{cardCount} card(s).");

        // empty the chest so it can't be reused
        gameObject.SetActive(false);
    }
}