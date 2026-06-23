// This script handles the shop slot view.
// Made by Vonce Chew

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// The view for one item for sale. Shows its art and price, and greys out when not purchasable.
/// </summary>
public class ShopSlotView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI references")]
    public Image artworkImage;
    public TMP_Text priceText;

    // The shop item this slot represents 
    public ShopItem Item { get; private set; }

    // Fires when the slot is clicked to buy
    public event Action<ShopSlotView> OnClicked;

    private bool _interactable = true;

    /// <summary>
    /// Set the slot's art and price label, shows SOLD when bought
    /// </summary>
    /// <param name="item">The item to display.</param>
    /// <param name="art">The card/potion sprite.</param>
    public void Bind(ShopItem item, Sprite art)
    {
        Item = item;
        if (artworkImage != null && art != null) artworkImage.sprite = art;
        if (priceText != null) priceText.text = item.sold ? "SOLD" : $"{item.price}c";
    }

    /// <summary>
    /// Grey out and block clicks when the item can't be bought.
    /// </summary>
    /// <param name="canBuy">True if purchasable right now.</param>
    public void SetInteractable(bool canBuy)
    {
        _interactable = canBuy;
        if (artworkImage != null)
            artworkImage.color = canBuy ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable) return;
        OnClicked?.Invoke(this);
    }
}