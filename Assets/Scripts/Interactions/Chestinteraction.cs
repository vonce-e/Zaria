// This script handles the chest interactions.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Grants cards and coins to the persistent run on interaction, then disables itself.
/// </summary>
public class ChestInteraction : MonoBehaviour
{
    [Header("Cards (randomized each open)")]
    [Tooltip("Fewest cards the chest can give.")]
    public int minCards = 1;
    [Tooltip("Most cards the chest can give.")]
    public int maxCards = 3;

    [Header("Coins (scales with depth of run.)")]
    [Tooltip("Base coins at depth 1.")]
    public int baseCoins = 30;

    [Tooltip("Extra coins added per depth level.")]
    public int coinsPerDepth = 15;

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
            Debug.LogWarning("ChestInteraction: no RunManager, is a run active?");
            return;
        }

        // Current depth (1 = first map). Falls back to 1 if not set.
        int depth = RunManager.Instance.currentDepth;
        if (depth < 1) depth = 1;

        // Coins scale with depth: base + (depth-1) * perDepth.
        int coinReward = baseCoins + (depth - 1) * coinsPerDepth;
        RunManager.Instance.runState.coins += coinReward;

        // Card count is random within the range (inclusive).
        int cardCount = Random.Range(minCards, maxCards + 1);

        // Grant cards and collect their names for the message.
        System.Collections.Generic.List<string> cardNames =
            new System.Collections.Generic.List<string>();

        if (CardRewardService.Instance != null)
        {
            for (int i = 0; i < cardCount; i++)
            {
                CardRewardService.Instance.OpenChest();

                // Capture the card that was just granted
                CardData granted = CardRewardService.Instance.LastGrantedCard;

                if (granted != null)
                {
                    cardNames.Add(granted.displayName);
                }
            }
        }

        if (RewardPopup.Instance != null)
        {
            string cardList = cardNames.Count > 0
                ? string.Join(", ", cardNames)
                : "no cards (deck full)";
            RewardPopup.Instance.Show($"Chest opened! {cardList} +{coinReward} coins");
        }

        _opened = true;
        Debug.Log($"Chest opened (depth {depth}): +{coinReward} coins, +{cardCount} card(s).");
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.ChestOpen(); // Chest Opened Audio

        // empty the chest so it can't be reused
        gameObject.SetActive(false);
    }
}