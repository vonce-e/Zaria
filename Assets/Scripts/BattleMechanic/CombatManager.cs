// This script handles the turn loop, coordinates dice, energy, statuses, card effects, win/lose.
// Made by Vonce Chew

using System.Collections.Generic;
using System.Collections;
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
    
    [Header("Level-up stat growth")]
    [Tooltip("How much max HP increases per level.")]
    public int hpGrowthPerLevel = 5;
    [Tooltip("Damage increases by 1 every N levels.")]
    public int damageGrowthEveryNLevels = 2;

    [Header("Parry/Dodge")]
    public ParryDodgeBar parryDodgeBar;

    private PlayerRunState _run;
    private Unit _player;
    private Unit _enemy;
    private bool _combatEnded;
    private CardPiles _piles = new CardPiles();
    private List<CardId> _cardsPlayedThisTurn = new List<CardId>();
    public event System.Action<bool> OnTurnChanged;

    // Dice manipulation state (set by skill cards).
    private int _diceFloorThisTurn = 0;   // Steady Hand: minimum roll this turn
    private int _diceFloorNextTurn = 0;   // Loaded Dice / Fate Bank: minimum next turn
    private bool _repeatNextCard = false; // Double Down: next card plays twice

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

        // Subscribe to level-ups so we can apply stat growth.
        _run.OnLeveledUp += HandleLevelUp;

        _piles.StartBattle(run);
        _piles.DrawCards(handSize);

        enemyAI.DecideNextIntent();
        StartPlayerTurn();
    }

    /// <summary>
    /// Unsubscribe from PlayerRunState events when this manager is destroyed,
    /// to avoid stale event handlers leaking between scene reloads.
    /// </summary>
    void OnDestroy()
    {
        if (_run != null) _run.OnLeveledUp -= HandleLevelUp;
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

        // Carry over any forced minimum from last turn (Loaded Dice / Fate Bank).
        _diceFloorThisTurn = _diceFloorNextTurn;
        _diceFloorNextTurn = 0;
        _repeatNextCard = false;
        ApplyDiceFloor();

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
            StartCoroutine(EnemyTurn());
            return;
        }

        OnTurnChanged?.Invoke(true);

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
            cardsPlayedThisTurn = _cardsPlayedThisTurn,
            combat = this,
        };

        ICardEffect effect = CardEffectRegistry.Get(card.definitionId);
        if (effect != null)
            effect.Apply(ctx, def);
        else
            Debug.LogWarning($"No effect coded for {card.definitionId} yet.");

        // Double Down: if a repeat was queued, run this card's effect once more.
        // Don't let a repeated card re-trigger another repeat.
        if (_repeatNextCard && effect != null && card.definitionId != CardId.DoubleDown)
        {
            _repeatNextCard = false;
            effect.Apply(ctx, def);
            Debug.Log($"<color=magenta>Double Down: {def.displayName} played twice!</color>");
        }

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
        if (_enemy.IsDead) return;
        StartCoroutine(EnemyTurn());
    }

    /// <summary>
    /// Runs the enemy's planned move. Damaging moves (attack/signature) route
    /// through the parry/dodge bar before damage lands. Then the AI decides
    /// its next move and the player's turn begins.
    /// </summary>
    private IEnumerator EnemyTurn()
    {
        if (_combatEnded) yield break;

        _enemy.block = 0;
        OnTurnChanged?.Invoke(false);
        Debug.Log("Enemy turn.");

        bool frozen = _enemy.HasBlockingStatus();
        _enemy.TickStatuses();

        if (_enemy.IsDead) { Win(); yield break; }

        if (frozen)
        {
            Debug.Log($"{_enemy.unitName} is frozen and skips its turn!");
        }
        else
        {
            // Find out how much damage the planned move deals (0 = no damage).
            int incoming = enemyAI.ExecuteIntent(_player);

            if (incoming > 0)
            {
                // Damaging move, route it through the timing bar.
                yield return StartCoroutine(ResolveAttackWithBar(incoming));
            }

            if (_player.IsDead) { Lose(); yield break; }
        }

        enemyAI.DecideNextIntent();
        StartPlayerTurn();
    }

    /// <summary>
    /// Shows the parry/dodge bar for an incoming hit and applies the outcome:
    /// Parry = no damage + 50% reflected, Dodge = no damage, Hit = full damage,
    /// block from cards still plays out.
    /// </summary>
    /// <param name="incoming">The raw incoming damage before mitigation.</param>
    private IEnumerator ResolveAttackWithBar(int incoming)
    {
        // If no bar is wired up, just take the hit (safe fallback).
        if (parryDodgeBar == null)
        {
            _player.TakeDamage(incoming);
            yield break;
        }

        bool done = false;
        ParryDodgeResult result = ParryDodgeResult.Hit;

        parryDodgeBar.Show(r => { result = r; done = true; });

        // Wait for the player to react (or the bar to time out).
        while (!done)
            yield return null;

        switch (result)
        {
            case ParryDodgeResult.Parry:
                int reflect = Mathf.RoundToInt(incoming * 0.5f);
                _enemy.TakeDamage(reflect);
                Debug.Log($"<color=cyan>PARRY! Reflected {reflect} damage.</color>");
                if (_enemy.IsDead) { Win(); yield break; }
                break;

            case ParryDodgeResult.Dodge:
                Debug.Log("<color=green>DODGE! No damage taken.</color>");
                break;

            case ParryDodgeResult.Hit:
                _player.TakeDamage(incoming);
                Debug.Log($"Hit for {incoming} (after block).");

                foreach (var status in _player.statuses)
                {
                    if (status is ReflectNextTurn reflectStatus)
                    {
                        _enemy.TakeDamage(reflectStatus.reflectAmount);
                        Debug.Log($"<color=cyan>Parry card reflected {reflectStatus.reflectAmount} damage!</color>");
                        if (_enemy.IsDead) { Win(); yield break; }
                        break;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Win flow : grant a card reward via CardRewardService,
    /// then tell BattleSystem to end the fight scene.
    /// </summary>
    private void Win()
    {
        if (_combatEnded) return;
        _combatEnded = true;
        Debug.Log("YOU WIN.");

        // XP reward from the defeated enemy
        EnemyXpReward xpComponent = _enemy.GetComponent<EnemyXpReward>();
        if (xpComponent != null && _run != null)
        {
            int xp = xpComponent.xpReward;
            _run.GrantXp(xp);
            Debug.Log($"Gained {xp} XP. " +
                    $"Now level {_run.playerLevel} ({_run.currentXp}/{_run.XpForNextLevel}).");
        }

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

    /// <summary>
    /// Called by PlayerRunState when a level-up happens. Boosts the
    /// player's max HP, optionally damage, and heals to full as a reward.
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        if (_player == null) return;

        _player.maxHp += hpGrowthPerLevel;
        _player.currentHp = _player.maxHp;  // full heal on level up

        if (damageGrowthEveryNLevels > 0 && newLevel % damageGrowthEveryNLevels == 0)
            _player.damage += 1;

        Debug.Log($"<color=yellow>LEVEL UP! Now level {newLevel}. " +
                $"Max HP: {_player.maxHp}, Damage: {_player.damage}.</color>");
    }

    /// <summary>
    /// Force a minimum dice value for the current turn (Steady Hand).
    /// </summary>
    public void SetDiceFloorThisTurn(int floor)
    {
        _diceFloorThisTurn = floor;
        ApplyDiceFloor();
    }

    /// <summary>
    /// Force a minimum dice value for next turn (Loaded Dice, Fate Bank).
    /// </summary>
    public void SetDiceFloorNextTurn(int floor)
    {
        if (floor > _diceFloorNextTurn) _diceFloorNextTurn = floor;
    }

    /// <summary>
    /// Make the next card played this turn resolve twice (Double Down).
    /// </summary>
    public void SetRepeatNextCard()
    {
        _repeatNextCard = true;
    }

    /// <summary>
    /// Re-roll the dice and keep the higher of the two (Dice Roll card).
    /// </summary>
    public void RollExtraDiceKeepHigher()
    {
        int first = dice.DiceNumber;
        dice.DiceRoll();              // rolls again, fires the UI event
        if (first > dice.DiceNumber)  // if the old one was better, restore it
        {
            dice.DiceNumber = first;
            dice.ForceRefreshUI();    // see note below
        }
    }

    /// <summary>
    /// Clamp the current dice up to the active floor, if any.
    /// </summary>
    private void ApplyDiceFloor()
    {
        if (dice.DiceNumber < _diceFloorThisTurn)
        {
            dice.DiceNumber = _diceFloorThisTurn;
            dice.ForceRefreshUI();
        }
    }
}