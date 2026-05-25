// This script handles the turn loop, coordinates dice, energy, statuses, card effects, win/lose.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The combat spine. Drives the turn flow, tracks the dice value and what
/// cards have been played this turn, hands cards' behaviour the right
/// BattleContext, and ends the fight when someone dies.
/// </summary>
public class CombatManager : MonoBehaviour
{
    public CardDatabase cardDatabase;
    public Dice dice;
    public EnemyAI enemyAI;
    public int handSize = 5;
    public int energyPerTurn = 5;

    [Header("Reward weights for this encounter")]
    public int rewardCommon    = 70;
    public int rewardRare      = 25;
    public int rewardMythic    = 5;
    public int rewardLegendary = 0;

    private PlayerRunState _run;
    private Unit _player;
    private Unit _enemy;
    private bool _combatEnded;
    private CardPiles _piles = new CardPiles();
    private List<CardId> _cardsPlayedThisTurn = new List<CardId>();

    /// <summary>
    /// Called by BattleSystem or CombatTest once everything is staged.
    /// Sets up the card piles, draws the opening hand, asks the enemy
    /// to plan its first move, then starts the player's turn.
    /// </summary>
    /// <param name="run">The player's run state (deck lives here).</param>
    /// <param name="player">The player's Unit.</param>
    /// <param name="enemy">The enemy's Unit.</param>
    public void BeginCombat(PlayerRunState run, Unit player, Unit enemy)
    {
        _run = run;
        _player = player;
        _enemy = enemy;

        _piles.StartBattle(run);
        _piles.DrawCards(handSize);

        enemyAI.DecideNextIntent();
        StartPlayerTurn();
    }

    /// <summary>
    /// Starts a fresh player turn, reset energy and block, clear
    /// per turn synergy memory, roll the dice, tick statuses, refill the
    /// hand, then wait for input. Skips the turn if the player is frozen.
    /// </summary>
    private void StartPlayerTurn()
    {
        if (_combatEnded) return;

        _player.energy = energyPerTurn;
        _player.block = 0;
        _cardsPlayedThisTurn.Clear();
        dice.DiceRoll();

        while (_piles.hand.Count < handSize)
        {
            if (_piles.DrawCard() == null) break;
        }

        bool frozen = _player.HasBlockingStatus();
        _player.TickStatuses();

        if (_player.IsDead) { Lose(); return; }

        if (frozen)
        {
            Debug.Log("You are frozen and lose your turn!");
            EnemyTurn();
            return;
        }

        Debug.Log($"Player turn. Dice: {dice.DiceNumber}. Energy: {_player.energy}. " +
                  $"Enemy plans to: {enemyAI.IntentLabel()}");
        Debug.Log($"Hand: {DescribeHand()}");
    }

    /// <summary>
    /// Called when the player clicks a card. Validates the move (in hand,
    /// enough energy), runs the card's effect, moves it to discard, and
    /// triggers Win() if the enemy died from the play.
    /// </summary>
    /// <param name="card">The card the player wants to play.</param>
    public bool TryPlayCard(CardInstance card)
    {
        if (_combatEnded) return false;

        CardData def = cardDatabase.Get(card.definitionId);

        if (!_piles.hand.Contains(card))
        {
            Debug.Log("That card isn't in your hand.");
            return false;
        }

        if (_player.energy < def.energyCost)
        {
            Debug.Log($"Not enough energy ({_player.energy}/{def.energyCost}).");
            return false;
        }

        _player.energy -= def.energyCost;

        var ctx = new BattleContext
        {
            caster = _player,
            target = _enemy,
            diceValue = dice.DiceNumber,
            cardUpgradeLevel = card.upgradeLevel,
            cardsPlayedThisTurn = _cardsPlayedThisTurn
        };

        ICardEffect effect = CardEffectRegistry.Get(card.definitionId);
        if (effect != null)
            effect.Apply(ctx, def);
        else
            Debug.LogWarning($"No effect coded for {card.definitionId} yet.");

        _cardsPlayedThisTurn.Add(card.definitionId);
        _piles.PlayCard(card);

        Debug.Log($"Played {def.displayName}. Enemy HP: {_enemy.currentHp}. " +
                  $"Your energy: {_player.energy}.");

        Debug.Log($"Hand: {DescribeHand()}");

        if (_enemy.IsDead)
        {
            Win();
            return true;
        }
        return true;
    }

    /// <summary>
    /// Called from the End Turn button. Skips straight to the enemy's turn.
    /// </summary>
    public void EndPlayerTurn()
    {
        if (_combatEnded) return;
        if (_enemy.IsDead) return;
        EnemyTurn();
    }

    /// <summary>
    /// Runs the enemy's planned move, then has the AI decide its next move
    /// </summary>
    private void EnemyTurn()
    {
        if (_combatEnded) return;
        
        _enemy.block = 0;
        Debug.Log("Enemy turn.");

        bool frozen = _enemy.HasBlockingStatus();
        _enemy.TickStatuses();

        if (_enemy.IsDead) { Win(); return; }

        if (frozen)
        {
            Debug.Log($"{_enemy.unitName} is frozen and skips its turn!");
        }
        else
        {
            enemyAI.ExecuteIntent(_player);
            if (_player.IsDead) { Lose(); return; }
        }

        enemyAI.DecideNextIntent();
        StartPlayerTurn();
    }

    /// <summary>
    /// Win flow: grant a card reward via CardRewardService,
    /// then tell BattleSystem to end the fight scene.
    /// </summary>
    private void Win()
    {
        if (_combatEnded) return;
        _combatEnded = true;
        Debug.Log("YOU WIN.");
        if (CardRewardService.Instance != null)
        {
            CardRewardService.Instance.GrantCombatReward(
                rewardCommon, rewardRare, rewardMythic, rewardLegendary);
        }
    }

    /// <summary>
    /// Lose flow: hand off to BattleSystem's end of fight cleanup.
    /// </summary>
    private void Lose()
    {
        if (_combatEnded) return;
        _combatEnded = true;
        Debug.Log("YOU LOSE.");
    }

    // ----- TESTING HELPERS -----
    /// <summary>
    /// Read-only access to the player's current hand. ONLY for testing.
    /// </summary>
    public List<CardInstance> GetHandForTesting() => _piles.hand;

    /// <summary>
    /// Debug friendly summary of the current hand. Used in turn start log
    /// so the tester can see which key plays which card.
    /// </summary>
    private string DescribeHand()
    {
        if (_piles.hand.Count == 0) return "(empty)";
        var parts = new List<string>();
        for (int i = 0; i < _piles.hand.Count; i++)
        {
            var def = cardDatabase.Get(_piles.hand[i].definitionId);
            parts.Add($"[{i + 1}] {def.displayName} ({def.energyCost}e)");
        }
        return string.Join("  ", parts);
    }
}