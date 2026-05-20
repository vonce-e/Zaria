// This script is the scene-side wrapper for the CardRewardSystem script.
// Made by Vonce Chew

using UnityEngine;

public class CardRewardService : MonoBehaviour
{
    public static CardRewardService Instance;

    // The card database
    public CardDatabase cardDatabase;

    /// The player's run
    public PlayerRunState run;

    private CardRewardSystem _system;

    /// <summary>
    /// Builds the system and wires up a simple Console-log popup.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;

        if (run == null) run = new PlayerRunState { playerLevel = 1 };
        _system = new CardRewardSystem(cardDatabase, run);

        _system.OnCardOffered += (def, result) =>
        {
            if (result == CardRewardSystem.AddResult.Added)
                Debug.Log($"<color=lime>+ {def.displayName} added to deck!</color>");
            else
                Debug.Log($"<color=orange>Deck is full - {def.displayName} refused.</color>");
        };
    }

    /// <summary>
    /// Called by a chest's onInteract event. Chests skew slightly toward
    /// rares vs an enemy drop.
    /// </summary>
    public void OpenChest()
    {
        _system.GrantRandomReward(
            commonWeight: 55, rareWeight: 35,
            mythicWeight: 10, legendaryWeight: 0);
    }

    /// <summary>
    /// Called by CombatManager.Win. Each encounter passes its own rarity
    /// weights so different enemies can drop different rarities.
    /// </summary>
    /// <param name="common">Relative chance of Common.</param>
    /// <param name="rare">Relative chance of Rare.</param>
    /// <param name="mythic">Relative chance of Mythic.</param>
    /// <param name="legendary">Relative chance of Legendary.</param>
    public void GrantCombatReward(int common = 70, int rare = 25, int mythic = 5, int legendary = 0)
    {
        _system.GrantRandomReward(common, rare, mythic, legendary);
    }

    /// <summary>
    /// Reserved for the shop. Once the player pays, the shop calls this
    /// with the specific card they bought.
    /// </summary>
    /// <param name="id">The card the player bought.</param>
    public void GrantSpecificCard(CardId id)
    {
        _system.AddCard(id);
    }
}