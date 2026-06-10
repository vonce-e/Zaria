// This script handles the Demo scene CombatTest.
// Made by Vonce Chew

using System.Collections;
using UnityEngine;

/// <summary>
/// Drives the demo scene. Boots straight into a tuned combat encounter and
/// chains a second one if configured.
/// </summary>
public class DemoCombatRunner : MonoBehaviour
{
    [Header("Game references")]
    public CombatManager combatManager;
    public Unit player;
    public Unit firstEnemy;

    [Header("HUDs")]
    public BattleHudManager playerHUD;
    public BattleHudManager enemyHUD;

    [Header("XP UI")]
    public XpBarUI xpBarUI;

    [Header("Demo deck")]
    public CardId[] starterDeck = new CardId[]
    {
        CardId.Slash, CardId.Slash, CardId.Slash, CardId.Slash,
        CardId.Guard, CardId.Guard,
        CardId.DeepCut, CardId.DeepCut,
        CardId.TwinStrike, CardId.TwinStrike
    };

    private PlayerRunState _run;
    private Unit _currentEnemy;

    void Start()
    {
        // Make sure the cursor is usable even if other scripts tried to lock it.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Heal both units to full so re-running the demo from scratch is consistent.
        player.currentHp = player.maxHp;
        firstEnemy.currentHp = firstEnemy.maxHp;

        // Build the player's deck from the configured starter list.
        _run = new PlayerRunState { playerLevel = 1 };
        foreach (var id in starterDeck)
            _run.deck.Add(new CardInstance(id));

        if (xpBarUI != null)
            xpBarUI.Bind(_run);

        _currentEnemy = firstEnemy;
        combatManager.BeginCombat(_run, player, _currentEnemy);

        // Wait one frame so HUDController.Start finishes, then show the HUDs.
        StartCoroutine(BindHudsNextFrame());

        Debug.Log("Demo combat started. Good luck on stage!");
    }

    /// <summary>
    /// Enable the battle HUD parent and bind both BattleHudManagers to
    /// their units. Done one frame late to win the race vs HUDController.
    /// </summary>
    private IEnumerator BindHudsNextFrame()
    {
        yield return null;

        if (HUDController.instance != null)
            HUDController.instance.SetBattleHUD(true);

        if (playerHUD != null) playerHUD.SetHUD(player);
        if (enemyHUD != null && _currentEnemy != null) enemyHUD.SetHUD(_currentEnemy);
    }
}