// This script handles the shop UI panel.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for the shop room. Renders the shop's stock, processes purchases via
/// the Shop logic, and shows coins/feedback. Opens with a PlayerRunState.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Logic + data")]
    public Shop shop;
    public CardDatabase cardDatabase;
    public PotionDatabase potionDatabase;
    public PlayerRunState run;

    [Header("Panel")]
    public GameObject panelRoot;
    public GameObject slotPrefab;       // a ShopSlotView prefab
    public Transform slotGrid;          // Grid Layout Group holds the slots

    [Header("Buttons + labels")]
    public Button refreshButton;
    public Button closeButton;
    public TMP_Text coinsText;
    public TMP_Text messageText;

    private readonly List<ShopSlotView> _slots = new List<ShopSlotView>();

    void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void Start()
    {
        if (refreshButton != null) refreshButton.onClick.AddListener(OnRefresh);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    /// <summary>
    /// Open the shop for a run : generate stock and show the panel.
    /// </summary>
    /// <param name="runState">The run whose coins/deck to use.</param>
    public void Open(PlayerRunState runState)
    {
        run = runState;
        if (panelRoot != null) panelRoot.SetActive(true);
        shop.GenerateStock();
        SetMessage("");
        RebuildSlots();
        RefreshInfo();
    }

    /// <summary>
    /// Hides the shop panel
    /// </summary>
    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    /// <summary>
    /// Rebuild all slot views from the shop's current stock.
    /// </summary>
    private void RebuildSlots()
    {
        foreach (var s in _slots) if (s != null) Destroy(s.gameObject);
        _slots.Clear();

        foreach (var item in shop.currentStock)
        {
            GameObject go = Instantiate(slotPrefab, slotGrid);
            ShopSlotView view = go.GetComponent<ShopSlotView>();
            if (view == null) { Destroy(go); continue; }

            view.Bind(item, GetArt(item));
            view.OnClicked += HandleSlotClicked;
            _slots.Add(view);
        }
        RefreshAffordability();
    }

    /// <summary>
    /// Look up the sprite for a shop item (card or potion).
    /// </summary>
    private Sprite GetArt(ShopItem item)
    {
        if (item.type == ShopItemType.Card)
        {
            CardData d = cardDatabase.Get(item.cardId);
            return d != null ? d.artwork : null;
        }
        PotionData pd = potionDatabase.Get(item.potionId);
        return pd != null ? pd.artwork : null;
    }

    /// <summary>
    /// A slot was clicked, try to buy it.
    /// </summary>
    private void HandleSlotClicked(ShopSlotView view)
    {
        Shop.BuyResult result = shop.Buy(run, view.Item);
        switch (result)
        {
            case Shop.BuyResult.Success:
                SetMessage("Purchased!");
                view.Bind(view.Item, GetArt(view.Item));  // re-render to show SOLD
                break;
            case Shop.BuyResult.NotEnoughCoins: SetMessage("Not enough coins."); break;
            case Shop.BuyResult.AlreadySold:    SetMessage("Already sold."); break;
            case Shop.BuyResult.DeckFull:       SetMessage("Your deck is full (30)."); break;
        }
        RefreshInfo();
        RefreshAffordability();
    }

    /// <summary>
    /// Reroll the shop stock for coins
    /// </summary>
    private void OnRefresh()
    {
        if (shop.Refresh(run))
        {
            SetMessage("Shop refreshed.");
            RebuildSlots();
            RefreshInfo();
        }
        else
        {
            SetMessage("Not enough coins to refresh.");
        }
    }

    /// <summary>
    /// Update the coins label
    /// </summary>
    private void RefreshInfo()
    {
        if (coinsText != null) coinsText.text = $"Coins: {run.coins}";
    }

    /// <summary>
    /// Grey out items that are sold or unaffordable
    /// </summary>
    private void RefreshAffordability()
    {
        foreach (var slot in _slots)
        {
            bool canBuy = !slot.Item.sold && run.coins >= slot.Item.price;
            slot.SetInteractable(canBuy);
        }
    }

    /// <summary>
    /// Show a feedback message
    /// </summary>
    private void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }
}