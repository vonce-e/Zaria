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
    public GameObject xpBarContainer; // XP Bar Slider

    private Unit _playerUnit;
    private Unit _enemyUnit;
    public BattleState state;
    public EnemyTelegraphUI enemyTelegraph;

    private GameObject _enemyGo;   // the spawned enemy, created on scene load

    [Header("Player Level Scaling")]
    [SerializeField] private int basePlayerMaxHp = 75;
    [SerializeField] private int hpGrowthPerLevel = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (combatUIRoot != null) combatUIRoot.SetActive(false);
        
    }

    private void Start()
    {
        SpawnEnemy();
    }

    /// <summary>
    /// Spawns the queued enemy at its point when the scene loads, so the
    /// player sees it standing there before walking into the battle trigger.
    /// </summary>
    private void SpawnEnemy()
    {
        GameObject prefab = (RunManager.Instance != null)
            ? RunManager.Instance.pendingEncounterPrefab
            : null;

        if (prefab == null)
        {
            Debug.LogError("BattleSystem: no enemy prefab in RunManager.");
            return;
        }

        _enemyGo = Instantiate(prefab, enemyPoint.position, enemyPoint.rotation);
        _enemyUnit = _enemyGo.GetComponent<Unit>();
        _enemyUnit.currentHp = _enemyUnit.maxHp;

        // Apply depth scaling now, at spawn.
        if (RunManager.Instance != null)
            EnemyScaling.Apply(_enemyUnit, RunManager.Instance.currentDepth);

        // Set the enemy HUD so its info shows
        if (enemyHUD != null) enemyHUD.SetHUD(_enemyUnit);
    }

    /// <summary>
    /// Stage and start the boss fight.
    /// </summary>
    public IEnumerator SetupBattle()
    {

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

        _enemyUnit.currentHp = _enemyUnit.maxHp;

        if (RunManager.Instance != null)
        {
            int savedLevel = RunManager.Instance.runState.playerLevel;

            _playerUnit.unitLevel = savedLevel;

            _playerUnit.maxHp = basePlayerMaxHp + (savedLevel - 1) * hpGrowthPerLevel;

            _playerUnit.currentHp = _playerUnit.maxHp;
        }

        if (playerHUD != null)
        {
            playerHUD.SetHUD(_playerUnit);
        }

        // point CombatManager at boss's AI, then start combat
        combatManager.enemyAI = _enemyGo.GetComponent<EnemyAI>();

        if (enemyTelegraph != null)
            enemyTelegraph.enemyAI = _enemyGo.GetComponent<EnemyAI>();
        
        yield return new WaitForSeconds(0.5f);  // brief beat before cards appear

        state = BattleState.PLAYERTURN;
        if (combatUIRoot != null) combatUIRoot.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBattleMusic(); // Plays battle music

        combatManager.BeginCombat(RunManager.Instance.runState, _playerUnit, _enemyUnit);

        // Wires the UI that needs run state
        if (potionBar != null) potionBar.run = RunManager.Instance.runState;

        if (xpBar != null) // hides xp text
        {
            xpBar.Bind(RunManager.Instance.runState);
            xpBar.gameObject.SetActive(false);   // hidden during the fight
        }

        if (xpBarContainer != null) // hides xp slider
        {
            xpBarContainer.SetActive(false);
        }
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

            // If this was a boss, advance depth.
            if (RunManager.Instance != null && RunManager.Instance.pendingIsBoss)
                RunManager.Instance.AdvanceDepth();

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