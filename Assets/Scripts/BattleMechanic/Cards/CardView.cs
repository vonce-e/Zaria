// This script is the visual representation of a single card on screen. Sits on the card prefab.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The view layer for one card. The hand display creates one of these per
/// card in the player's hand, calls Bind to set the artwork, and destroys
/// it when the card leaves the hand.
/// </summary>
public class CardView : MonoBehaviour
{
    [Header("UI references (drag in from prefab)")]
    public Image artworkImage;   // Image component on the prefab

    /// <summary>
    /// The card instance this view represents. Used by P3 to play it.
    /// </summary>
    public CardInstance CardInstance { get; private set; }

    /// <summary>
    /// The card data this view represents.
    /// </summary>
    public CardData CardData { get; private set; }

    /// <summary>
    /// Fill in the card's visuals from a player's CardInstance and the
    /// shared CardData. Sets the image to the card's artwork.
    /// </summary>
    /// <param name="instance">The player's copy of the card.</param>
    /// <param name="data">The shared card definition.</param>
    public void Bind(CardInstance instance, CardData data)
    {
        CardInstance = instance;
        CardData = data;

        if (artworkImage != null && data.artwork != null)
            artworkImage.sprite = data.artwork;
        else if (artworkImage != null)
            Debug.LogWarning($"Card {data.displayName} has no artwork assigned.");
    }
}