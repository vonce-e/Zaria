// The rulebook for the cards.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Every unique card in the game. Used so code can refer to cards by name
/// instead of by string, which is safer (typos become compile errors).
/// </summary>
public enum CardId
{
    Slash, TwinStrike, QuickJab, ExecutionStrike, DeepCut, BloodSlash,
    ChaosRend, FatalCollapse, Guard, Brace, Parry, AdaptiveShield,
    RollingBarrier, TemporalGuard, DiceRoll, Energize, SteadyHand,
    DoubleDown, LoadedDice, FateBank
}

/// <summary>
/// The broad category a card falls into.
/// </summary>
public enum CardType { Attack, Defense, Skill }

/// <summary>
/// How rare a card is. Drives energy cost and drop rates.
/// </summary>
public enum Rarity { Common, Rare, Mythic, Legendary }

/// <summary>
/// One step of an upgrade. If a card has 2 of these in its tiers list, it
/// can be upgraded twice.
/// </summary>
[System.Serializable]
public class UpgradeTier
{
    public int bonusDamage;
    public int bonusDefense;
    [TextArea] public string upgradeDescription;
}

/// <summary>
/// The shared definition of a single card. Every Slash in the game reads
/// from the same CardData asset. Not modified by Blacksmith.
/// </summary>
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

    /// <summary>
    /// One entry per upgrade level. Empty array = card can't be upgraded.
    /// </summary>
    public UpgradeTier[] upgradeTiers;

    /// <summary>
    /// The maximum upgrade level this card supports.
    /// </summary>
    public int MaxUpgradeLevel => upgradeTiers != null ? upgradeTiers.Length : 0;

    /// <summary>
    /// Returns the actual damage at a given upgrade level.
    /// </summary>
    /// <param name="upgradeLevel">0 for base, 1+ for upgraded.</param>
    public int GetDamage(int upgradeLevel)
    {
        int dmg = baseDamage;
        if (upgradeLevel > 0 && upgradeLevel <= MaxUpgradeLevel)
            dmg += upgradeTiers[upgradeLevel - 1].bonusDamage;
        return dmg;
    }

    /// <summary>
    /// Returns the actual defense at a given upgrade level.
    /// </summary>
    /// <param name="upgradeLevel">0 for base, 1+ for upgraded.</param>
    public int GetDefense(int upgradeLevel)
    {
        int def = baseDefense;
        if (upgradeLevel > 0 && upgradeLevel <= MaxUpgradeLevel)
            def += upgradeTiers[upgradeLevel - 1].bonusDefense;
        return def;
    }
}

/// <summary>
/// The Player's personal copy of a card.
/// </summary>
[System.Serializable]
public class CardInstance
{
    public CardId definitionId;
    public int upgradeLevel;

    /// <summary>
    /// Makes a fresh, unupgraded copy of a card.
    /// </summary>
    public CardInstance(CardId id)
    {
        definitionId = id;
        upgradeLevel = 0;
    }
}