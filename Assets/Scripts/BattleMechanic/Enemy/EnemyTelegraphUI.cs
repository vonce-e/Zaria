// This script shows the enemy's planned move as a label. Each frame, reads the AI's current intent and writes it to a TMP_Text.
// Made by Vonce Chew

using UnityEngine;
using TMPro;

/// <summary>
/// Displays the enemy's planned next move by reading enemyAI.IntentLabel() and writing it to a label.
/// </summary>
public class EnemyTelegraphUI : MonoBehaviour
{
    public EnemyAI enemyAI;
    public TMP_Text label;

    void Update()
    {
        if (enemyAI == null || label == null) return;
        label.text = enemyAI.IntentLabel();
    }
}