// This script handles the card data
// Made by Vonce Chew
using UnityEngine;

// Every card in the game.
public enum CardId
{
    Slash, TwinStrike, QuickJab, ExecutionStrike, DeepCut, BloodSlash,
    ChaosRend, FatalCollapse, Guard, Brace, Parry, AdaptiveShield,
    RollingBarrier, TemporalGuard, DiceRoll, Energize, SteadyHand,
    DoubleDown, LoadedDice, FateBank
}

public enum CardType { Attack, Defense, Skill }

public enum Rarity { Common, Rare, Mythic, Legendary }

// One upgrade step. If a card has 2 of these, it can be upgraded twice.
[System.Serializable]
public class UpgradeTier
{
    public int bonusDamage;
    public int bonusDefense;
    [TextArea] public string upgradeDescription; // Shows upgrade description to player.
}

[CreateAssetMenu(menuName = "Zaria/Card")]
public class CardData : ScriptableObject
{
    public CardId id;
    public string displayName;
    public CardType cardType;
    public Rarity rarity;
    public int energyCost;
    [TextArea] public string description;
    public Sprite artwork;

    public int baseDamage;
    public int baseDefense;

    // One entry per upgrade level. Empty array = card can't be upgraded.
    public UpgradeTier[] upgradeTiers;

    // How many times the blacksmith can upgrade this card.
    public int MaxUpgradeLevel => upgradeTiers != null ? upgradeTiers.Length : 0;

    // The actual damage a card does at a given upgrade level.
    public int GetDamage(int upgradeLevel)
    {
        int dmg = baseDamage;
        if (upgradeLevel > 0) dmg += upgradeTiers[upgradeLevel - 1].bonusDamage;
        return dmg;
    }

    public int GetDefense(int upgradeLevel)
    {
        int def = baseDefense;
        if (upgradeLevel > 0) def += upgradeTiers[upgradeLevel - 1].bonusDefense;
        return def;
    }
}

// The player's copy of a card in their deck. This is the thing the blacksmith changes.
[System.Serializable]
public class CardInstance
{
    public CardId definitionId;
    public int upgradeLevel;

    public CardInstance(CardId id)
    {
        definitionId = id;
        upgradeLevel = 0;
    }
}