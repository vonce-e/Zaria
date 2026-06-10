// This script contains everything about the player's current run.
// Made by Vonce Chew

using System;
using System.Collections.Generic;

/// <summary>
/// The player's run wide state: coins, XP driven level, the real deck, and
/// how many enchant points have been spent at the blacksmith.
/// </summary>
[System.Serializable]
public class PlayerRunState
{
    public int coins;

    // The player's XP level. Drives the enchant budget
    public int playerLevel = 1;

    // XP accumulated toward the next level
    public int currentXp = 0;

    // How many upgrades the blacksmith has performed this run
    public int enchantSpent;

    // The player's deck.
    public List<CardInstance> deck = new List<CardInstance>();

    // Maximum upgrades allowed this run, derived from playerLevel.
    public int EnchantBudget => 5 + 5 * (playerLevel / 5);

    // How many upgrades the player still has available
    public int EnchantRemaining => EnchantBudget - enchantSpent;

    /// <summary>
    /// XP required to reach the next level. Increases linearly so early
    /// levels come fast and later ones become real milestones.
    /// Level 1 -> 2 requires 50 XP. Subsequent levels needs +30 more.
    /// </summary>
    public int XpForNextLevel => 50 + 30 * (playerLevel - 1);

    /// <summary>
    /// Fires when XP changes. Passes new XP and the threshold.
    /// </summary>
    [field: NonSerialized]
    public event Action<int, int> OnXpChanged;

    /// <summary>
    /// Fires when the player levels up. Passes the new level.
    /// </summary>
    [field: NonSerialized]
    public event Action<int> OnLeveledUp;

    /// <summary>
    /// Grant XP and handle level-up cascading. Multiple level-ups can
    /// happen in one call if a huge XP reward is granted at once.
    /// </summary>
    /// <param name="amount">XP amount to grant.</param>
    public void GrantXp(int amount)
    {
        if (amount <= 0) return;
        currentXp += amount;

        while (currentXp >= XpForNextLevel)
        {
            currentXp -= XpForNextLevel;
            playerLevel++;
            OnLeveledUp?.Invoke(playerLevel);
        }

        OnXpChanged?.Invoke(currentXp, XpForNextLevel);
    }
}