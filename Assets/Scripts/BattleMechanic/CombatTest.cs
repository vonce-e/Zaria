// This script is the throwaway test for the turn-based system
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.InputSystem;

public class CombatTest : MonoBehaviour
{
    public CombatManager combatManager;
    public Unit player;
    public Unit enemy;

    [Header("HUDs (drag in from the scene)")]
    public BattleHudManager playerHUD;
    public BattleHudManager enemyHUD;

    void Start()
    {
        player.currentHp = player.maxHp;
        enemy.currentHp = enemy.maxHp;

        // Build a test run with a small deck of cards
        var run = new PlayerRunState { playerLevel = 1 };
        run.deck.Add(new CardInstance(CardId.Slash));
        run.deck.Add(new CardInstance(CardId.Guard));
        run.deck.Add(new CardInstance(CardId.DeepCut));
        run.deck.Add(new CardInstance(CardId.TwinStrike));
        for (int i = 0; i < 6; i++)
            run.deck.Add(new CardInstance(CardId.Slash));

        combatManager.BeginCombat(run, player, enemy);

        if (HUDController.instance != null)
            HUDController.instance.SetBattleHUD(true);

        if (playerHUD != null) playerHUD.SetHUD(player);
        if (enemyHUD != null) enemyHUD.SetHUD(enemy);

        Debug.Log("Combat test ready. " +
                  "Press 1-5 to play cards, SPACE to end turn.");
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TryPlay(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) TryPlay(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) TryPlay(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) TryPlay(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) TryPlay(4);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            combatManager.EndPlayerTurn();
    }

    /// <summary>
    /// Play the Nth card from the player's current hand. Real UI in
    /// </summary>
    /// <param name="index">Position in the hand (0-based).</param>
    void TryPlay(int index)
    {
        var hand = combatManager.GetHandForTesting();
        if (hand == null || index >= hand.Count)
        {
            Debug.Log($"No card at slot {index + 1}.");
            return;
        }
        combatManager.TryPlayCard(hand[index]);
    }
}