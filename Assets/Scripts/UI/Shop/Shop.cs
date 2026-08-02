// This script handles the shop logic.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

// Whether a shop slot holds a card or a potion.
public enum ShopItemType { Card, Potion }

/// <summary>
/// One thing for sale: a card or potion, its price, and sold state.
/// </summary>
[System.Serializable]
public class ShopItem
{
    public ShopItemType type;
    public CardId cardId;        // valid when type == Card
    public PotionId potionId;    // valid when type == Potion
    public int price;
    public bool sold;
}

/// <summary>
/// Shop-room logic. Builds a random stock of cards (Common/Rare only) and
/// potions, prices them, and processes purchases.
/// </summary>
public class Shop : MonoBehaviour
{
    public CardDatabase cardDatabase;

    [Header("Stock")]
    public int slotCount = 6;
    public int refreshCost = 10;

    [Header("Price ranges (inclusive)")]
    public int commonMin = 15, commonMax = 25;
    public int rareMin = 32, rareMax = 48;
    public int potionMin = 24, potionMax = 36;

    // The 6 items currently for sale
    public List<ShopItem> currentStock = new List<ShopItem>();

    private List<CardId> _commonRarePool;
    private List<PotionId> _potionPool;

    // The outcome of a purchase attempt, for the UI to message
    public enum BuyResult { Success, NotEnoughCoins, AlreadySold, DeckFull }

    /// <summary>
    /// Roll a fresh set of items into currentStock.
    /// </summary>
    public void GenerateStock()
    {
        BuildPools();
        currentStock.Clear();
        for (int i = 0; i < slotCount; i++)
            currentStock.Add(MakeRandomItem());
    }

    /// <summary>
    /// Build the card/potion pools (Common + Rare cards, all potions)
    /// </summary>
    private void BuildPools()
    {
        if (_commonRarePool == null)
        {
            _commonRarePool = new List<CardId>();
            foreach (CardId id in System.Enum.GetValues(typeof(CardId)))
            {
                CardData data = cardDatabase.Get(id);
                if (data == null) continue;
                if (data.rarity == Rarity.Common || data.rarity == Rarity.Rare)
                    _commonRarePool.Add(id);
            }
        }
        if (_potionPool == null)
        {
            _potionPool = new List<PotionId>();
            foreach (PotionId id in System.Enum.GetValues(typeof(PotionId)))
                _potionPool.Add(id);
        }
    }

    /// <summary>
    /// Make one random item : 50/50 card-or-potion, priced by rarity
    /// </summary>
    private ShopItem MakeRandomItem()
    {
        ShopItem item = new ShopItem();
        bool pickCard = Random.value < 0.5f && _commonRarePool.Count > 0;

        if (pickCard)
        {
            item.type = ShopItemType.Card;
            item.cardId = _commonRarePool[Random.Range(0, _commonRarePool.Count)];
            Rarity r = cardDatabase.Get(item.cardId).rarity;
            item.price = (r == Rarity.Common)
                ? Random.Range(commonMin, commonMax + 1)
                : Random.Range(rareMin, rareMax + 1);
        }
        else
        {
            item.type = ShopItemType.Potion;
            item.potionId = _potionPool[Random.Range(0, _potionPool.Count)];
            item.price = Random.Range(potionMin, potionMax + 1);
        }
        return item;
    }

    /// <summary>
    /// Buys an item
    /// </summary>
    /// <param name="run">The player's run state.</param>
    /// <param name="item">The shop item to buy.</param>
    public BuyResult Buy(PlayerRunState run, ShopItem item)
    {
        if (item.sold) return BuyResult.AlreadySold;
        if (run.coins < item.price) return BuyResult.NotEnoughCoins;

        if (item.type == ShopItemType.Card)
        {
            if (run.deck.Count >= 30) return BuyResult.DeckFull;
            run.deck.Add(new CardInstance(item.cardId));
        }
        else
        {
            run.potions.Add(new PotionInstance(item.potionId));
        }

        run.coins -= item.price;
        item.sold = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.Purchase(); // Purchase Audio

        return BuyResult.Success;
    }

    /// <summary>
    /// Reroll the stock
    /// </summary>
    /// <param name="run">The player's run state.</param>
    public bool Refresh(PlayerRunState run)
    {
        if (run.coins < refreshCost) return false;
        run.coins -= refreshCost;
        GenerateStock();

        if (AudioManager.Instance != null)
            AudioManager.Instance.ButtonClick(); // Upgrade Audio

        return true;
    }
}