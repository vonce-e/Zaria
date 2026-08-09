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

    // True if the player has already used a potion this turn (for UI grey-out).
    public bool PotionUsedThisTurn => _potionUsedThisTurn;
    private bool _potionUsedThisTurn = false;

    // Dice manipulation state (set by skill cards).
    private int _diceFloorThisTurn = 0;   // Steady Hand: minimum roll this turn
    private int _diceFloorNextTurn = 0;   // Loaded Dice / Fate Bank: minimum next turn
    private bool _repeatNextCard = false; // Double Down: next card plays twice

    private CardInstance _lastPlayedCard;

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

        enemyAI.combat = this;
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
        _potionUsedThisTurn = false;
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

        // Tick down potion damage multipliers (Glass Cannon, Recharge).
        if (_player.outgoingMultiplierTurns > 0)
        {
            _player.outgoingMultiplierTurns--;
            if (_player.outgoingMultiplierTurns <= 0)
                _player.outgoingDamageMultiplier = 1f;
        }
        if (_player.incomingMultiplierTurns > 0)
        {
            _player.incomingMultiplierTurns--;
            if (_player.incomingMultiplierTurns <= 0)
                _player.incomingDamageMultiplier = 1f;
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

        int enemyHpBefore = _enemy.currentHp;   // measures damage dealt

        if (effect != null)
            effect.Apply(ctx, def);
        else
            Debug.LogWarning($"No effect coded for {card.definitionId} yet.");

        int dealt = enemyHpBefore - _enemy.currentHp;
        if (dealt > 0) enemyAI.lastPlayerAttackDamage = dealt;

        // Double Down : if a repeat was queued, run this card's effect once more.
        // Don't let a repeated card re-trigger another repeat.
        if (_repeatNextCard && effect != null && card.definitionId != CardId.DoubleDown)
        {
            _repeatNextCard = false;
            effect.Apply(ctx, def);
            Debug.Log($"<color=magenta>Double Down: {def.displayName} played twice!</color>");
        }

        _cardsPlayedThisTurn.Add(card.definitionId);
        _piles.PlayCard(card);
        AudioManager.Instance.CardPlay();
        _lastPlayedCard = card;

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
            // Passive checks
            enemyAI.OnEnemyTurnStart();

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
                AudioManager.Instance.ParrySuccess();
                Debug.Log($"<color=cyan>PARRY! Reflected {reflect} damage.</color>");
                if (_enemy.IsDead) { Win(); yield break; }
                break;

            case ParryDodgeResult.Dodge:
                AudioManager.Instance.DodgeSuccess();
                Debug.Log("<color=green>DODGE! No damage taken.</color>");
                break;

            case ParryDodgeResult.Hit:
                EnemyAnimator anim = _enemy.GetComponent<EnemyAnimator>();
                if (anim != null) anim.PlayAttack(); // Plays enemy attack animation

                _player.TakeDamage(incoming);
                AudioManager.Instance.AttackHit();
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
        if (_combatEnded)
        {
            return;
        }

        EnemyAnimator enemyAnim = _enemy.GetComponent<EnemyAnimator>();

        if (enemyAnim != null)
        {
            enemyAnim.PlayDeath();
        }

        // On-death effect (e.g. Blazeling's explosion). May kill the player.
        int deathDmg = enemyAI.OnDeath(_player);
        AudioManager.Instance.EnemyDeath();

        if (deathDmg > 0)
        {
            _player.TakeTrueDamage(deathDmg);   // Bypasses block, not dodgeable
            Debug.Log($"{_enemy.unitName}'s death effect deals {deathDmg} damage.");
            if (_player.IsDead)
            {
                Lose();        // mutual death - player loses despite the kill
                return;
            }
        }
        _combatEnded = true;
        AudioManager.Instance.Victory();

        // change scene
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

        // Card reward
        string cardPart = "";
        if (CardRewardService.Instance != null)
        {
            CardRewardService.Instance.GrantCombatReward(
                rewardCommon, rewardRare, rewardMythic, rewardLegendary);

            CardData won = CardRewardService.Instance.LastGrantedCard;
            if (won != null) cardPart = $"Got {won.displayName}! ";
        }

        // Coins (depth-scaled, boss bonus)
        int coinsWon = 0;
        string title = "Victory!";
        if (RunManager.Instance != null)
        {
            bool wasBoss = RunManager.Instance.pendingIsBoss;
            int depth = RunManager.Instance.currentDepth;
            coinsWon = (wasBoss ? 60 : 20) + (depth - 1) * (wasBoss ? 25 : 10);
            RunManager.Instance.runState.coins += coinsWon;
            if (wasBoss) title = "Boss defeated!";
        }

        // Reward message
        if (RewardPopup.Instance != null)
            RewardPopup.Instance.Show($"{title} {cardPart}+{coinsWon} coins");

        if (BattleSystem.Instance != null)
            BattleSystem.Instance.EndBattle(true);
    }

    /// <summary>
    /// Lose flow: hand off to BattleSystem's end of fight cleanup.
    /// </summary>
    private void Lose()
    {
        if (_combatEnded) return;
        _combatEnded = true;
        AudioManager.Instance.Defeat();
        // change scene
        Debug.Log("YOU LOSE.");

        if (DeathScreen.Instance != null)
            DeathScreen.Instance.Show(); // Show death screen
            
        if (BattleSystem.Instance != null)
            BattleSystem.Instance.EndBattle(false);
    }

    /// <summary>
    /// The player's current hand, read by HandDisplay to render cards.
    /// </summary>
    public List<CardInstance> CurrentHand => _piles.hand;

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
        AudioManager.Instance.LevelUp();

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

    /// <summary>
    /// Move up to 'count' random cards from the player's hand back into the
    /// deck (used by Null's signature). Returns how many were actually moved.
    /// </summary>
    /// <param name="count">How many cards to attempt to shuffle away.</param>
    public int ShuffleRandomCardsFromHand(int count)
    {
        int moved = 0;
        for (int i = 0; i < count; i++)
        {
            if (_piles.hand.Count == 0) break;
            int idx = Random.Range(0, _piles.hand.Count);
            CardInstance card = _piles.hand[idx];
            _piles.hand.RemoveAt(idx);
            _piles.deck.Add(card);
            moved++;
        }
        if (moved > 0)
            Debug.Log($"{moved} card(s) shuffled from hand back into the deck.");
        return moved;
    }

    /// <summary>
    /// Try to use a potion this turn. Enforces the once-per-turn rule, applies
    /// the effect, and removes the potion from the run inventory.
    /// </summary>
    /// <param name="potion">The potion instance to use.</param>
    public bool TryUsePotion(PotionInstance potion)
    {
        if (_combatEnded) return false;

        if (_potionUsedThisTurn)
        {
            Debug.Log("You can only use one potion per turn.");
            return false;
        }

        IPotionEffect effect = PotionEffectRegistry.Get(potion.definitionId);
        if (effect == null)
        {
            Debug.LogWarning($"No effect coded for potion {potion.definitionId}.");
            return false;
        }

        var ctx = new PotionContext { player = _player, enemy = _enemy, combat = this };
        effect.Apply(ctx);

        _potionUsedThisTurn = true;
        AudioManager.Instance.PotionDrink();
        _run.potions.Remove(potion);

        // A potion might have killed the enemy (or the player).
        if (_enemy.IsDead) { Win(); return true; }
        if (_player.IsDead) { Lose(); return true; }

        return true;
    }

    /// <summary>
    /// Discard the whole hand and draw a fresh one of the same size.
    /// Used by the New Beginning potion.
    /// </summary>
    public void DiscardHandAndRedraw()
    {
        // Move every card in hand to the discard pile.
        var current = new List<CardInstance>(_piles.hand);
        foreach (var card in current)
            _piles.PlayCard(card);   // PlayCard moves hand -> discard

        // Draw back up to the normal hand size.
        while (_piles.hand.Count < handSize)
        {
            if (_piles.DrawCard() == null) break;
        }
    }

    /// <summary>
    /// Move the last played card from the discard pile back into the hand.
    /// Used by the Rewind potion. Returns true if a card was returned.
    /// </summary>
    public bool ReturnLastPlayedCardToHand()
    {
        if (_lastPlayedCard == null) return false;

        if (_piles.discard.Contains(_lastPlayedCard))
        {
            _piles.discard.Remove(_lastPlayedCard);
            _piles.hand.Add(_lastPlayedCard);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Time Warp : grant an immediate bonus turn (fresh energy, dice, and a
    /// refilled hand) without the enemy acting first, then queue a skip so the
    /// player misses their following turn as the cost.
    /// </summary>
    public void GrantExtraTurn()
    {
        // Cost : skip the next normal player turn (enemy acts during it).
        _player.AddStatus(new SkipTurn(1));

        // Bonus turn now : fresh resources, same as a normal turn start.
        _player.energy = energyPerTurn;
        _player.block = 0;
        _cardsPlayedThisTurn.Clear();
        _potionUsedThisTurn = true;   // the potion the player just drank counts as this turn's use
        dice.DiceRoll();

        while (_piles.hand.Count < handSize)
        {
            if (_piles.DrawCard() == null) break;
        }

        Debug.Log("<color=orange>Time Warp: bonus turn! You'll skip your next turn.</color>");
    }
}