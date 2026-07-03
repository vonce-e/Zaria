// This script handles the random dice number generation.
// Made by Vonce Chew
using UnityEngine;
using System;

public class Dice : MonoBehaviour
{
    // Variables
    [HideInInspector]
    public int DiceNumber;

    /// <summary>
    /// Fires when the dice is rolled. Passes the new value (1-6).
    /// </summary>
    public event Action<int> OnDiceRolled;

    /// <summary>
    /// Roll the dice. Generates a random value 1-6, stores it in DiceNumber,
    /// and fires OnDiceRolled so any listening UI can update.
    /// </summary>
    public void DiceRoll()
    {
        DiceNumber = UnityEngine.Random.Range(1, 7);
        OnDiceRolled?.Invoke(DiceNumber);
        AudioManager.Instance.DiceRoll();
    }

    /// <summary>
    /// Re-fire the dice event without rolling, so the UI shows a value that
    /// was forced by a card (dice floor, re-roll, stored value).
    /// </summary>
    public void ForceRefreshUI()
    {
        OnDiceRolled?.Invoke(DiceNumber);
    }
}