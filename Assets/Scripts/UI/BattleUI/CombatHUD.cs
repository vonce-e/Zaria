// This script handles the supporting combat HUD: energy text, dice text, End Turn button.

// Made by Vonce Chew

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the supporting combat UI elements: energy text display, dice
/// roll display, and the End Turn button. Subscribes to game events so
/// the displays update automatically when stats change.
/// </summary>
public class CombatHUD : MonoBehaviour
{
    [Header("Game references")]
    public CombatManager combatManager;
    public Unit player;
    public Dice dice;

    [Header("UI elements (drag in from scene)")]
    public TMP_Text energyText;
    public TMP_Text diceText;
    public Button endTurnButton;

    void Start()
    {
        // Energy display: subscribe to player energy changes
        if (player != null)
        {
            player.OnEnergyChanged += HandleEnergyChanged;
            HandleEnergyChanged(player.energy);  // set initial value
        }

        // Dice display: subscribe to dice rolls
        if (dice != null)
        {
            dice.OnDiceRolled += HandleDiceRolled;
            HandleDiceRolled(dice.DiceNumber);   // Sets initial value
        }

        // End Turn button: wire to CombatManager
        if (endTurnButton != null)
            endTurnButton.onClick.AddListener(HandleEndTurnClicked);
    }

    void OnDestroy()
    {
        if (player != null) player.OnEnergyChanged -= HandleEnergyChanged;
        if (dice != null) dice.OnDiceRolled -= HandleDiceRolled;
        if (endTurnButton != null) endTurnButton.onClick.RemoveListener(HandleEndTurnClicked);
    }

    /// <summary>
    /// Refresh the energy display whenever the player's energy changes.
    /// </summary>
    private void HandleEnergyChanged(int newEnergy)
    {
        if (energyText != null)
            energyText.text = $"{newEnergy}/{combatManager.energyPerTurn}";
    }

    /// <summary>
    /// Refresh the dice display whenever the dice is rolled.
    /// </summary>
    private void HandleDiceRolled(int newValue)
    {
        if (diceText != null)
            diceText.text = newValue > 0 ? $"{newValue}" : "-";
    }

    /// <summary>
    /// End Turn button was clicked, tell CombatManager.
    /// </summary>
    private void HandleEndTurnClicked()
    {
        if (combatManager != null)
            combatManager.EndPlayerTurn();
    }
}