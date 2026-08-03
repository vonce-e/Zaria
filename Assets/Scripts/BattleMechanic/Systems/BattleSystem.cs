// This script stages the boss fight in the battle scene.
// Made by Vonce Chew

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

/// <summary>
/// The battle-scene staging manager
/// </summary>
public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    [Header("Player Input")]
    public GameObject playerObj;
    public CharacterController playerCC;
    public PlayerInput playerInput;

    [Header("Transform points")]
    public Transform playerPoint;
    public Transform enemyPoint;

    [Header("Cameras")]
    public GameObject battleCamera;
    public GameObject playerCamera;

    [Header("HUDs")]
    public BattleHudManager playerHUD;
    public BattleHudManager enemyHUD;

    [Header("Combat")]
    public CombatManager combatManager;

    [Header("After winning")]
    [Tooltip("Seconds to wait after a win before loading the next scene.")]
    public float winDelay = 2f;
    [Tooltip("Scene to load after winning. Falls back to RunManager.returnSceneName if empty.")]
    public string nextSceneName;

    [Header("Combat UI")]
    public GameObject combatUIRoot;

    [Header("Run-dependent UI")]
    public PotionBarDisplay potionBar;
    public XpBarUI xpBar;

    private Unit _playerUnit;
    private Unit _enemyUnit;
    public BattleState state;
    public EnemyTelegraphUI enemyTelegraph;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (combatUIRoot != null) combatUIRoot.SetActive(false);
        
    }

    /// <summary>
    /// Stage and start the boss fight.
    /// </summary>
    public IEnumerator SetupBattle()
    {
        // Spawn room model
        if (RunManager.Instance != null && RunManager.Instance.pendingRoomPrefab != null)
            Instantiate(RunManager.Instance.pendingRoomPrefab);

        state = BattleState.START;

        // Stage the scene : move player in, lock them, swap to battle camera.
        TeleportPlayerToBattlePoint();
        LockPlayer();
        SetUpCamera(true);

        // Player unit
        _playerUnit = playerObj.GetComponentInChildren<Unit>();

        // Spawn the boss the dungeon queued in RunManager
        GameObject bossPrefab = (RunManager.Instance != null)
            ? RunManager.Instance.pendingEncounterPrefab
            : null;

        if (bossPrefab == null)
        {
            Debug.LogError("BattleSystem: no boss prefab in RunManager. " +
                           "Did the dungeon call LoadBattle()?");
            yield break;
        }

        GameObject enemyGo = Instantiate(bossPrefab, enemyPoint.position, enemyPoint.rotation);
        _enemyUnit = enemyGo.GetComponent<Unit>();
        _enemyUnit.currentHp = _enemyUnit.maxHp;

        if (RunManager.Instance != null)
            _playerUnit.unitLevel = RunManager.Instance.runState.playerLevel;

        // HUDs
        playerHUD.SetHUD(_playerUnit);
        enemyHUD.SetHUD(_enemyUnit);

        // point CombatManager at boss's AI, then start combat
        combatManager.enemyAI = enemyGo.GetComponent<EnemyAI>();

        if (enemyTelegraph != null)
            enemyTelegraph.enemyAI = enemyGo.GetComponent<EnemyAI>();
        
        yield return new WaitForSeconds(1f);  // brief beat before cards appear

        state = BattleState.PLAYERTURN;
        if (combatUIRoot != null) combatUIRoot.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBattleMusic(); // Plays battle music

        combatManager.BeginCombat(RunManager.Instance.runState, _playerUnit, _enemyUnit);

        // Wires the UI that needs run state
        if (potionBar != null) potionBar.run = RunManager.Instance.runState;
        if (xpBar != null) xpBar.Bind(RunManager.Instance.runState);
    }

    /// <summary>
    /// Called by CombatManager when the fight ends.
    /// </summary>
    /// <param name="won">True if the player won.</param>
    public void EndBattle(bool won)
    {
        if (won)
        {
            state = BattleState.WON;

            bool wasBoss = RunManager.Instance != null && RunManager.Instance.pendingIsBoss;

            // If this was a boss, advance depth.
            if (wasBoss)
                RunManager.Instance.AdvanceDepth();

            // Award coins for the win, scaled by depth. Bosses pay a bonus.
            int coinsWon = 0;
            if (RunManager.Instance != null)
            {
                int depth = RunManager.Instance.currentDepth;
                int baseCoins = wasBoss ? 60 : 20; // bosses worth more
                int perDepth  = wasBoss ? 25 : 10; // and scale faster
                coinsWon = baseCoins + (depth - 1) * perDepth;
                RunManager.Instance.runState.coins += coinsWon;
            }

            // Show the reward message.
            if (RewardPopup.Instance != null)
            {
                string label = wasBoss ? "Boss defeated!" : "Victory!";
                RewardPopup.Instance.Show($"{label} +{coinsWon} coins");
            }

            StartCoroutine(WinThenLeave());
        }
    }

    /// <summary>
    /// Wait a moment after winning, then load the next scene.
    /// </summary>
    private IEnumerator WinThenLeave()
    {
        yield return new WaitForSeconds(winDelay);

        string target = !string.IsNullOrEmpty(nextSceneName)
            ? nextSceneName
            : (RunManager.Instance != null ? RunManager.Instance.returnSceneName : null);

        if (!string.IsNullOrEmpty(target) && SceneLoader.Instance != null)
            SceneLoader.Instance.ChangeScene(target);
        else
            Debug.LogWarning("BattleSystem: no next scene set - staying here.");
    }

    /// <summary>
    /// Move the player to the battle spawn point
    /// </summary>
    private void TeleportPlayerToBattlePoint()
    {
        if (playerCC == null || playerPoint == null)
        {
            Debug.LogWarning("BattleSystem missing player or player point.");
            return;
        }
        playerCC.enabled = false;
        playerCC.transform.SetPositionAndRotation(playerPoint.position, playerPoint.rotation);
        playerCC.enabled = true;
    }

    /// <summary>
    /// Stop the player from walking during the fight.
    /// </summary>
    private void LockPlayer()
    {
        if (playerInput == null)
        {
            Debug.LogWarning("Player input is missing.");
            return;
        }
        playerInput.enabled = false;
    }

    /// <summary>
    /// Swap between the walking camera and the battle camera.
    /// </summary>
    /// <param name="useBattleCamera">True to show the battle view.</param>
    private void SetUpCamera(bool useBattleCamera)
    {
        playerCamera.SetActive(!useBattleCamera);
        battleCamera.SetActive(useBattleCamera);

        Cursor.lockState = useBattleCamera ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = useBattleCamera;

        if (HUDController.instance != null)
        {
            HUDController.instance.SetCrosshair(useBattleCamera);
        }
    }
}