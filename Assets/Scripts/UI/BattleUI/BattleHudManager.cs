// This script handles the HUD for one fighter, name, level, HP bar. One per Unit on screen.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The HUD strip for one fighter. Shows their name, level, and a HP bar
/// that updates live as they take damage or heal. Subscribes to the
/// Unit's OnHpChanged event so the slider always reflects current HP.
/// </summary>
public class BattleHudManager : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;
    public Slider healthSlider;
    public TMP_Text hpText;

    private Unit _trackedUnit;

    /// <summary>
    /// Bind this HUD to a Unit. Sets all text/slider values immediately
    /// and subscribes to OnHpChanged so future changes update the bar.
    /// Call this once at the start of combat.
    /// </summary>
    /// <param name="unit">The fighter this HUD should display.</param>
    public void SetHUD(Unit unit)
    {
        // If this HUD was previously tracking a different unit, unsubscribe
        // so it doesn't get duplicate event callbacks.
        if (_trackedUnit != null)
            _trackedUnit.OnHpChanged -= HandleHpChanged;

        _trackedUnit = unit;

        nameText.text = unit.unitName;

        if (levelText != null)
            levelText.text = "Lvl " + unit.unitLevel; // Null check for enemy level text

        healthSlider.maxValue = unit.maxHp;
        healthSlider.value = unit.currentHp;
        if (hpText != null) hpText.text = $"{unit.currentHp} / {unit.maxHp}";

        unit.OnHpChanged += HandleHpChanged;
    }

    /// <summary>
    /// Manual HP setter.
    /// </summary>
    /// <param name="hp">New HP value to display.</param>
    public void SetHp(int hp)
    {
        healthSlider.value = hp;
        if (hpText != null && _trackedUnit != null)
            hpText.text = $"{hp} / {_trackedUnit.maxHp}";
    }

    /// <summary>
    /// Called by the Unit when its HP changes. Just forwards to SetHp -
    /// could be extended later to play a damage flash, shake the bar, etc.
    /// </summary>
    /// <param name="newHp">The Unit's new HP value.</param>
    private void HandleHpChanged(int newHp)
    {
        SetHp(newHp);
    }

    /// <summary>
    /// Unity calls this when the GameObject is destroyed. Unsubscribe
    /// from the event to avoid null references.
    /// </summary>
    private void OnDestroy()
    {
        if (_trackedUnit != null)
            _trackedUnit.OnHpChanged -= HandleHpChanged;
    }
}