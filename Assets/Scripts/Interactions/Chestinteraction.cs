// This script handles the chest interactions.
// Made by Vonce Chew

using Unity.VisualScripting;
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

    [Range(0,100)]
    public int cardDropChance = 15;

    [Header("Coins (scales with depth of run.)")]
    [Tooltip("Base coins at depth 1.")]
    public int baseCoins = 30;

    [Tooltip("Extra coins added per depth level.")]
    [Range(0, 100)] public int lowCoinChance = 65;
    [Range(0, 100)] public int mediumCoinChance = 25;

    public Vector2Int lowCoinRange = new Vector2Int(5, 20);
    public Vector2Int mediumCoinRange = new Vector2Int(21, 30);
    public Vector2Int highCoinRange = new Vector2Int(31, 50);
    public int coinsPerDepth = 5;

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
        int coinChanceRoll = Random.Range(0,100);
        int coinReward;
        int depthBonus = (depth - 1) * coinsPerDepth;
        
        if (coinChanceRoll < lowCoinChance)
        {
            coinReward = Random.Range(lowCoinRange.x, lowCoinRange.y + 1);
        }
        else if (coinChanceRoll < lowCoinChance + mediumCoinChance)
        {
            coinReward = Random.Range(mediumCoinRange.x, mediumCoinRange.y + 1);
        }
        else
        {
            coinReward = Random.Range(highCoinRange.x, highCoinRange.y + 1);
        }

        coinReward += depthBonus;

        RunManager.Instance.runState.coins += coinReward;

        // Stores the name of any card successfully awarded.
        System.Collections.Generic.List<string> cardNames =
            new System.Collections.Generic.List<string>();

        // Decide whether this chest awards a card.
        int cardChanceRoll = Random.Range(0, 100);

        if (cardChanceRoll < cardDropChance &&
            CardRewardService.Instance != null)
        {
            CardRewardService.Instance.OpenChest();

            CardData granted =
                CardRewardService.Instance.LastGrantedCard;

            if (granted != null)
            {
                cardNames.Add(granted.displayName);
            }
        }

        if (RewardPopup.Instance != null)
        {
            if (cardNames.Count > 0)
            {
                string cardList = string.Join(",", cardNames);
                
                RewardPopup.Instance.Show($"Chest opened! Found {cardList} and {coinReward} coins!");
            }
            else
            {
                RewardPopup.Instance.Show($"Chest opened! Found {coinReward} coins!");
            }
        }

        _opened = true;
        Debug.Log(
        $"Chest opened (depth {depth}): " +
        $"+{coinReward} coins, +{cardNames.Count} card(s).");
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.ChestOpen(); // Chest Opened Audio

        // empty the chest so it can't be reused
        gameObject.SetActive(false);
    }
}