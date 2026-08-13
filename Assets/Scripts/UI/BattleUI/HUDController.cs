// This script handles the dungeon HUD (timer, coins, depth, interaction text).
// Made by Vonce Chew

using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Runs the on-screen dungeon HUD. Shows the run timer (counts down and
/// changes colour as it gets low), the player's coins and current depth, the
/// interaction prompt, and the crosshair. When the timer hits zero it forces
/// the player into a boss fight.
/// </summary>
public class HUDController : MonoBehaviour
{
    public static HUDController instance;

    [Header("Player HUD")]
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private TMP_Text depthText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool useDungeonInformation; // only dungeon scenes show this HUD

    [Header("Timer Appearance")]
    [SerializeField] private float warningTime = 120f; // turn yellow under this many seconds
    [SerializeField] private float dangerTime = 30f; // turn red under this many seconds
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.yellow;
    [SerializeField] private Color dangerTimerColor = Color.red;

    [Header("Forced boss room encounter")]
    [SerializeField] private float forcedBossDelay = 2f; // pause before sending player to the boss
    [SerializeField] private string timerExpiredMessage =
    "Time has expired. The boss awaits...";

    [Header("Boss battle enemy")]
    [SerializeField] private EnemyPool fallbackEnemyPool; // used if no boss/miniboss portal exists
    [SerializeField] private string battleSceneName = "BattleScene";

    // Remember the last shown values so we only update the text when they change.
    private int previouslyDisplayedCoins = -1;
    private int previouslyDisplayedDepth = -1;

    [SerializeField] private GameObject interactionParent;
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private GameObject crosshair;

    /// <summary>
    /// This will update the timer, coins and the run the player is on.
    /// </summary>
    private void Update()
    {
        // Only show the HUD in dungeon scenes, and hide it when a menu is open.
        bool showPlayerHUD = useDungeonInformation && !UIState.IsPanelOpen;

        if (playerHUD != null && playerHUD.activeSelf != showPlayerHUD)
        {
            playerHUD.SetActive(showPlayerHUD);
        }

        if (useDungeonInformation && RunManager.Instance != null)
        {
            RunManager runManager = RunManager.Instance;

            // Pauses the timer when the menus are open
            if (!runManager.dungeonTimerExpired && !UIState.IsPanelOpen)
            {
                runManager.remainingDungeonTime -= Time.deltaTime;

                // Timer ran out - lock it at 0 and force the boss fight.
                if (runManager.remainingDungeonTime <= 0f)
                {
                    runManager.remainingDungeonTime = 0f;
                    runManager.dungeonTimerExpired = true;

                    StartCoroutine(ForceBossEncounter());
                }
            }

            // Update the coins text only when the coin count actually changed.
            int currentCoins = runManager.runState.coins;

            if (coinsText != null && currentCoins != previouslyDisplayedCoins)
            {
                coinsText.text = currentCoins.ToString();
                previouslyDisplayedCoins = currentCoins;
            }

            // Same for depth - only update when it changes.
            if (depthText != null && runManager.currentDepth != previouslyDisplayedDepth)
            {
                depthText.text = $"Depth: {runManager.currentDepth}";
                previouslyDisplayedDepth = runManager.currentDepth;
            }

            // Update the timer text (format as M:SS) and colour it by urgency.
            if (timerText != null)
            {
                int totalSeconds = Mathf.CeilToInt(runManager.remainingDungeonTime);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                timerText.text = $"{minutes}:{seconds:00}";

                if (totalSeconds <= dangerTime)
                {
                    timerText.color = dangerTimerColor;
                }
                else if (totalSeconds <= warningTime)
                {
                    timerText.color = warningTimerColor;
                }
                else
                {
                    timerText.color = normalTimerColor;
                }
            }
        }
    }

    // TODO: Move forced encounter logic into RunManager after presentation.
    /// <summary>
    /// Runs when the dungeon timer expires. Shows a message, waits a moment,
    /// then sends the player to a boss. Prefers a real boss room, then a
    /// miniboss portal, and finally spawns a fallback boss if neither exists.
    /// </summary>
    private IEnumerator ForceBossEncounter()
    {
        // This is to reuse the reward popup text broadcast because it has a fade than creating a new script
        if (RewardPopup.Instance != null)
        {
            RewardPopup.Instance.Show(timerExpiredMessage);
        }

        yield return new WaitForSeconds(forcedBossDelay);

        // Look for boss/miniboss portals already in the scene.
        BossRoomTrigger bossTrigger = FindFirstObjectByType<BossRoomTrigger>();
        EnemyPortal enemyPortal = FindFirstObjectByType<EnemyPortal>();

        // Sends them to a boss room if a boss portal was found
        if (bossTrigger != null)
        {
            Debug.Log("Normal boss portal was chosen.");
            bossTrigger.TeleportPlayer();
            yield break;
        }
        // Send them to a miniboss room if a miniboss portal was found
        else if (enemyPortal != null)
        {
            Debug.Log("Mini boss portal was chosen.");
            enemyPortal.StartForcedDepthBattle();
            yield break;
        }
        // If no miniboss or boss portal exists, just force a teleport
        else
        {
            Debug.Log("No boss or miniboss portal found. Starting fallback boss battle.");

            // Need a run and a pool to pick a fallback boss from.
            if (RunManager.Instance == null || fallbackEnemyPool == null)
            {
                Debug.LogWarning("Cannot start fallback boss battle.");
                yield break;
            }

            // Pick a boss scaled to the current depth.
            int currentDepth = RunManager.Instance.currentDepth;
            GameObject boss = fallbackEnemyPool.GetRandomBoss(currentDepth);

            if (boss == null)
            {
                Debug.LogWarning("No fallback boss was found in the EnemyPool.");
                yield break;
            }

            // Come back to this same dungeon scene after the fight.
            string returnScene = UnityEngine.SceneManagement
                .SceneManager.GetActiveScene().name;

            // Load the battle (isBoss = true so depth advances on the win).
            RunManager.Instance.LoadBattle(
                boss,
                battleSceneName,
                returnScene,
                true
            );
        }
    }

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Show or hide the crosshair (hidden while interacting/menus).
    /// </summary>
    public void SetCrosshair(bool useCrosshair)
    {
        crosshair.SetActive(!useCrosshair);
    }

    /// <summary>
    /// Show the "press E to..." prompt with the current interact key.
    /// </summary>
    /// <param name="text">The action text, e.g. "Open Shop".</param>
    public void EnableInteractionText(string text)
    {
        if (interactionParent == null && interactionText == null)
        {
            return;
        }

        // Tack the current interact key onto the message, e.g. "Open Shop (E)".
        interactionText.text = text + " (" + Settings.interactKey.ToString() + ")";
        interactionParent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the interaction prompt (player looked away).
    /// </summary>
    public void DisableInteractionText()
    {
        if (interactionParent == null)
        {
            return;
        }

        interactionParent.SetActive(false);
    }
}