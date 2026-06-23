// This script handles the potion icon in the potion bar. Clickable to use, hover to show a
// tooltip.
// Made by Vonce Chew

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// The view for one carried potion. Click fires OnClicked; hover shows a
/// tooltip with the potion's description. Greys out when not usable.
/// </summary>
public class PotionView : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI references")]
    public Image artworkImage;

    /// <summary>
    /// The potion this view represents.
    /// </summary>
    public PotionInstance PotionInstance { get; private set; }

    /// <summary>
    /// The potion's static data (name, description, art).
    /// </summary>
    public PotionData PotionData { get; private set; }

    /// <summary>
    /// Fires when clicked. Passes itself so the bar knows which potion.
    /// </summary>
    public event Action<PotionView> OnClicked;

    /// <summary>
    /// Fires on hover enter, with this potion's data, for a tooltip.
    /// </summary>
    public event Action<PotionData> OnHoverEnter;

    /// <summary>
    /// Fires on hover exit, hide the tooltip.
    /// </summary>
    public event Action OnHoverExit;

    private bool _interactable = true;

    /// <summary>
    /// Fill in artwork and remember which potion this is.
    /// </summary>
    public void Bind(PotionInstance instance, PotionData data)
    {
        PotionInstance = instance;
        PotionData = data;

        if (artworkImage != null && data != null && data.artwork != null)
            artworkImage.sprite = data.artwork;
    }

    /// <summary>
    /// Grey out and disable clicks when a potion can't be used.
    /// </summary>
    /// <param name="canUse">True if the potion is usable right now.</param>
    public void SetInteractable(bool canUse)
    {
        _interactable = canUse;
        if (artworkImage != null)
            artworkImage.color = canUse ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable) return;
        OnClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHoverEnter?.Invoke(PotionData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();
    }
}