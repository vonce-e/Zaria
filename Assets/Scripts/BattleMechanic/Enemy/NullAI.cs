// This script handles the Null boss enemy.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Null: a disruption boss. Its signature strips cards out of the player's
/// hand (back into the deck) and punishes them with damage scaled by how
/// many cards it managed to shuffle.
/// </summary>
public class NullAI : EnemyAI
{
    [Tooltip("How many cards to shuffle from the player's hand back to the deck.")]
    public int cardsToShuffle = 2;

    [Tooltip("Damage dealt per card actually shuffled.")]
    public int damagePerCard = 8;

    /// <summary>
    /// Shuffles cards out of the player's hand via CombatManager, then
    /// returns the damage (8 per shuffled card) to route through the bar.
    /// </summary>
    /// <param name="player">The player unit (damage applied via the bar).</param>
    protected override int SignatureDamage(Unit player)
    {
        int shuffled = 0;
        if (combat != null)
            shuffled = combat.ShuffleRandomCardsFromHand(cardsToShuffle);

        int dmg = shuffled * damagePerCard;
        Debug.Log($"{unit.unitName}'s signature: shuffled {shuffled} card(s) away, " +
                  $"dealing {dmg} damage.");
        return dmg;
    }
}