// This script contains everything about the player's current run.
// Made by Vonce Chew

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

    // How many upgrades the blacksmith has performed this run
    public int enchantSpent;

    // The player's deck.
    public List<CardInstance> deck = new List<CardInstance>();

    // Maximum upgrades allowed this run, derived from playerLevel.
    public int EnchantBudget => 5 + 5 * (playerLevel / 5);

    // How many upgrades the player still has available
    public int EnchantRemaining => EnchantBudget - enchantSpent;
}
