// This script handles the on-screen XP bar. Shows current level, current XP, and the threshold.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's level and XP progress. Subscribes to
/// PlayerRunState events so the slider and label update automatically
/// whenever XP is granted or a level-up happens.
/// </summary>
public class XpBarUI : MonoBehaviour
{
    [Header("UI references")]
    public Slider xpSlider;
    public TMP_Text label;

    private PlayerRunState _run;

    /// <summary>
    /// Bind this UI to a PlayerRunState. Call once after the run starts.
    /// </summary>
    /// <param name="run">The run state to track.</param>
    public void Bind(PlayerRunState run)
    {
        // Unsubscribe from any previous run before binding the new one.
        if (_run != null)
        {
            _run.OnXpChanged -= HandleXpChanged;
            _run.OnLeveledUp -= HandleLeveledUp;
        }

        _run = run;
        if (_run == null) return;

        _run.OnXpChanged += HandleXpChanged;
        _run.OnLeveledUp += HandleLeveledUp;

        // Set initial display.
        Refresh();
    }

    void OnDestroy()
    {
        if (_run != null)
        {
            _run.OnXpChanged -= HandleXpChanged;
            _run.OnLeveledUp -= HandleLeveledUp;
        }
    }

    /// <summary>
    /// XP changed : update slider and label.
    /// </summary>
    private void HandleXpChanged(int currentXp, int threshold)
    {
        Refresh();
    }

    /// <summary>
    /// Leveled up : refresh, since threshold also changed.
    /// </summary>
    private void HandleLeveledUp(int newLevel)
    {
        Refresh();
    }

    /// <summary>
    /// Push current run values to slider and label.
    /// </summary>
    private void Refresh()
    {
        if (_run == null) return;

        if (xpSlider != null)
        {
            xpSlider.maxValue = _run.XpForNextLevel;
            xpSlider.value = _run.currentXp;
        }

        if (label != null)
            label.text = $"Lvl {_run.playerLevel}  |  {_run.currentXp} / {_run.XpForNextLevel} XP";
    }
}