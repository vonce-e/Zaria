// This script handles the rulebook for potions.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Every potion in the game, referenced by name.
/// </summary>
public enum PotionId
{
    TimeWarp, GlassCannon, PhantomGuard, NewBeginning,
    StasisChamber, Rewind, Sacrifice, Recharge
}

/// <summary>
/// Static definition of a potion
/// </summary>
[CreateAssetMenu(menuName = "Zaria/Potion")]
public class PotionData : ScriptableObject
{
    public PotionId id;
    public string displayName;
    [TextArea] public string description;
    public Sprite artwork; // plain icon (used in combat)
    public Sprite fullImage; // full card with icon + description (used in shop)
}

/// <summary>
/// The player's copy of a potion they're carrying. Held in PlayerRunState's inventory.
/// </summary>
[System.Serializable]
public class PotionInstance
{
    public PotionId definitionId;

    public PotionInstance(PotionId id)
    {
        definitionId = id;
    }
}