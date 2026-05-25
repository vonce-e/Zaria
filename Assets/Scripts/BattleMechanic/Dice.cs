// This script handles the random dice number generation.
// Made by Vonce Chew
using UnityEngine;

public class Dice : MonoBehaviour
{
    // Variables
    [HideInInspector]
    public int DiceNumber;

    /// <summary>
    /// This function handles the dice roll mechanic
    /// </summary>
    public void DiceRoll()
    {
        // Plays dice roll animation

        DiceNumber = UnityEngine.Random.Range(1, 7); // Returns random int between and including 1-6
    }
}
