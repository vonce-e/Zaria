// This script handles the Blacksmith UI panel.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for the enchantment room. Displays the run deck, lets the player pick
/// a card and upgrade or discard it, and shows budget/coins/feedback.
/// </summary>
public class BlacksmithUI : MonoBehaviour
{
    [Header("Logic + data")]
    public Blacksmith blacksmith;
    public CardDatabase cardDatabase;
    public PlayerRunState run;          // set when the panel opens

    [Header("Panel")]
    public GameObject panelRoot;        // the whole panel, toggled on/off

    [Header("Deck grid")]
    public GameObject cardPrefab;       // reuses the CardView prefab
    public Transform cardGrid;          // a Grid Layout Group holds the cards

    [Header("Action buttons")]
    public Button upgradeButton;
    public Button discardButton;
    public Button closeButton;

    [Header("Info labels")]
    public TMP_Text budgetText;
    public TMP_Text coinsText;
    public TMP_Text messageText;

    private readonly Dictionary<CardInstance, CardView> _views =
        new Dictionary<CardInstance, CardView>();
    private CardInstance _selected;

    void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void Start()
    {
        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgrade);
        if (discardButton != null) discardButton.onClick.AddListener(OnDiscard);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    /// <summary>
    /// Open the blacksmith for a given run. Builds the deck grid and shows the panel.
    /// </summary>
    /// <param name="runState">The run whose deck to edit.</param>
    public void Open(PlayerRunState runState)
    {
        run = runState;
        _selected = null;
        if (panelRoot != null) panelRoot.SetActive(true);

        // Free the cursor so the player can click cards and buttons.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetMessage("");
        RebuildGrid();
        RefreshInfo();
        RefreshButtons();
        UIState.PanelOpened();
    }

    /// <summary>
    /// Hide the panel.
    /// </summary>
    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        // Re-lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        UIState.PanelClosed();
    }

    /// <summary>
    /// Destroy and recreate the whole deck grid from the run deck.
    /// </summary>
    private void RebuildGrid()
    {
        foreach (var pair in _views)
            if (pair.Value != null) Destroy(pair.Value.gameObject);
        _views.Clear();

        if (run == null) return;

        foreach (var card in run.deck)
        {
            GameObject go = Instantiate(cardPrefab, cardGrid);
            CardView view = go.GetComponent<CardView>();
            if (view == null) { Destroy(go); continue; }

            CardData data = cardDatabase.Get(card.definitionId);
            view.Bind(card, data);
            view.OnClicked += HandleCardClicked;
            _views[card] = view;
        }
    }

    /// <summary>
    /// A card was clicked, mark it selected and update buttons.
    /// </summary>
    /// <param name="view">The clicked card view.</param>
    private void HandleCardClicked(CardView view)
    {
        _selected = view.CardInstance;
        SetMessage($"Selected {cardDatabase.Get(_selected.definitionId).displayName}.");
        RefreshButtons();
        HighlightSelected();
    }

    /// <summary>
    /// Upgrade the selected card via the Blacksmith logic.
    /// </summary>
    private void OnUpgrade()
    {
        if (_selected == null) { SetMessage("Pick a card first."); return; }

        Blacksmith.Result result = blacksmith.UpgradeCard(run, _selected);
        switch (result)
        {
            case Blacksmith.Result.Success:        SetMessage("Upgraded!"); break;
            case Blacksmith.Result.NoBudgetLeft:   SetMessage("No enchant budget left."); break;
            case Blacksmith.Result.AlreadyMaxLevel:SetMessage("That card is already maxed."); break;
        }
        RefreshInfo();
        RefreshButtons();
    }

    /// <summary>
    /// Discard the selected card for coins via the Blacksmith logic.
    /// </summary>
    private void OnDiscard()
    {
        if (_selected == null) { SetMessage("Pick a card first."); return; }

        Blacksmith.Result result = blacksmith.DiscardCard(run, _selected);
        if (result == Blacksmith.Result.Success)
        {
            SetMessage("Discarded for coins.");
            _selected = null;
            RebuildGrid();   // the card is gone, rebuild the grid
        }
        else if (result == Blacksmith.Result.DeckTooSmall)
        {
            SetMessage("Your deck can't get any smaller.");
        }
        RefreshInfo();
        RefreshButtons();
    }

    /// <summary>
    /// Update the budget and coins labels from the run.
    /// </summary>
    private void RefreshInfo()
    {
        if (run == null) return;
        if (budgetText != null) budgetText.text = $"Enchants left: {run.EnchantRemaining}";
        if (coinsText != null)  coinsText.text = $"Coins: {run.coins}";
    }

    /// <summary>
    /// Enable/disable the action buttons based on the selection.
    /// </summary>
    private void RefreshButtons()
    {
        bool hasSelection = _selected != null;
        if (upgradeButton != null) upgradeButton.interactable = hasSelection;
        if (discardButton != null) discardButton.interactable = hasSelection;
    }

    /// <summary>
    /// Tint the selected card and reset the others.
    /// </summary>
    private void HighlightSelected()
    {
        foreach (var pair in _views)
        {
            bool isSel = pair.Key == _selected;
            // Subtle highlight via scale; keeps it simple and code-only.
            pair.Value.transform.localScale = isSel ? Vector3.one * 1.08f : Vector3.one;
        }
    }

    /// <summary>
    /// Show a feedback message to the player.
    /// </summary>
    /// <param name="msg">The message text.</param>
    private void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }
}