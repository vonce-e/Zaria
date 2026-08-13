// This script is the visual representation of a single card on screen. Sits on the card prefab.
// Made by Vonce Chew

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// The view layer for one card. The hand display creates one of these per
/// card in the player's hand, calls Bind to set the artwork, and destroys
/// it when the card leaves the hand.
/// </summary>
public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI references (drag in from prefab)")]
    public Image artworkImage;   // Image component on the prefab

    /// <summary>
    /// Outer border that gets tinted based on the card's rarity.
    /// </summary>
    public Image borderImage;

    /// <summary>
    /// The card instance this view represents. Used by P3 to play it.
    /// </summary>
    public CardInstance CardInstance { get; private set; }

    /// <summary>
    /// The card data this view represents.
    /// </summary>
    public CardData CardData { get; private set; }

    /// <summary>
    /// Fires when this card is clicked. Passes itself so the listener knows which card.
    /// </summary>
    public event Action<CardView> OnClicked;

    [Tooltip("The badge background (circle). Holds the level text as a child.")]
    public GameObject upgradeBadgeRoot;
    [Tooltip("The '+N' text on the badge.")]
    public TMP_Text upgradeBadgeText;

    private bool _interactable = true;

    /// <summary>
    /// Turn this card on or off visually. Disabled cards are greyed out and
    /// ignore clicks. Used when the player can't afford the card.
    /// </summary>
    /// <param name="canPlay">True if the card is playable.</param>
    public void SetInteractable(bool canPlay)
    {
        _interactable = canPlay;
        if (artworkImage != null)
            artworkImage.color = canPlay ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    /// <summary>Unity calls this when the card is clicked.</summary>
    /// <param name="eventData">Click info from Unity's event system.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable) return;
        OnClicked?.Invoke(this);

         if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ButtonClick();
        }
    }

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

        if (borderImage != null)
            borderImage.color = GetRarityColor(data.rarity);

        // Show the upgrade badge (circle + text) only if upgraded.
        if (upgradeBadgeRoot != null)
        {
            if (instance.upgradeLevel > 0)
            {
                if (upgradeBadgeText != null)
                    upgradeBadgeText.text = "+" + instance.upgradeLevel;
                upgradeBadgeRoot.SetActive(true);
            }
            else
            {
                upgradeBadgeRoot.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Called when the mouse enters the card. Scales up slightly.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_interactable) return;
        transform.localScale = new Vector3(1.1f, 1.1f, 1f);
    }

    /// <summary>
    /// Called when the mouse leaves the card. Returns to normal scale.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    /// <summary>Maps each rarity to a border colour.</summary>
    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:    return new Color(0.3f, 0.55f, 1.0f);   // blue
            case Rarity.Rare:      return new Color(1.0f, 0.65f, 0.1f);   // yellow/orange
            case Rarity.Mythic:    return new Color(0.9f, 0.15f, 0.15f);  // red
            case Rarity.Legendary: return new Color(1.0f, 0.85f, 0.2f);   // bright gold
            default:               return Color.white;
        }
    }

    /// <summary>
    /// Fade this card to transparent over the given duration, then destroy
    /// it. Used when a card is played, so it doesn't just vanish.
    /// </summary>
    /// <param name="duration">How long the fade takes in seconds.</param>
    public System.Collections.IEnumerator FadeAndDestroy(float duration = 0.3f)
    {
        if (artworkImage == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float t = 0f;
        Color startColor = artworkImage.color;
        Color startBorder = borderImage != null ? borderImage.color : Color.white;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / duration);

            Color c = startColor;
            c.a = alpha;
            artworkImage.color = c;

            if (borderImage != null)
            {
                Color b = startBorder;
                b.a = alpha;
                borderImage.color = b;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}