// This script listens for turn changes and flashes a label saying whose turn it is.
// Made by Vonce Chew

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Shows "Your Turn" or "Enemy Turn" briefly at the top of the screen
/// when each turn begins. Hooks into CombatManager.OnTurnChanged.
/// </summary>
public class TurnStateText : MonoBehaviour
{
    public CombatManager combatManager;
    public TMP_Text label;
    public float displayDuration = 1.5f;

    void Start()
    {
        if (combatManager != null)
            combatManager.OnTurnChanged += HandleTurnChanged;

        if (label != null)
            label.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (combatManager != null)
            combatManager.OnTurnChanged -= HandleTurnChanged;
    }

    /// <summary>
    /// Show the label briefly, then hide it.
    /// </summary>
    private void HandleTurnChanged(bool isPlayerTurn)
    {
        if (label == null) return;

        label.text = isPlayerTurn ? "Your Turn" : "Enemy Turn";
        label.color = isPlayerTurn ? Color.white : new Color(1f, 0.4f, 0.4f);

        StopAllCoroutines();
        StartCoroutine(FlashLabel());
    }

    /// <summary>
    /// Show for displayDuration seconds, then hide.
    /// </summary>
    private IEnumerator FlashLabel()
    {
        label.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        label.gameObject.SetActive(false);
    }
}