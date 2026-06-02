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
    }
}