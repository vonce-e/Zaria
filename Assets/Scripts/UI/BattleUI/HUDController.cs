using UnityEngine;
using TMPro;
using System.Collections;

public class HUDController : MonoBehaviour
{
    public static HUDController instance;

    [Header("Player HUD")]
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private TMP_Text depthText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool useDungeonInformation;

    [Header("Timer Appearance")]
    [SerializeField] private float warningTime = 120f;
    [SerializeField] private float dangerTime = 30f;
    [SerializeField] private Color normalTimerColor = Color.white;
    [SerializeField] private Color warningTimerColor = Color.yellow;
    [SerializeField] private Color dangerTimerColor = Color.red;

    [Header("Forced boss room encounter")]
    [SerializeField] private float forcedBossDelay = 2f;
    [SerializeField] private string timerExpiredMessage =
    "Time has expired. The boss awaits...";
    
    [Header("Boss battle enemy")]
    [SerializeField] private EnemyPool fallbackEnemyPool;
    [SerializeField] private string battleSceneName = "BattleScene";

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

                if (runManager.remainingDungeonTime <= 0f)
                {
                    runManager.remainingDungeonTime = 0f;
                    runManager.dungeonTimerExpired = true;

                    StartCoroutine(ForceBossEncounter());
                }
            }

            int currentCoins = runManager.runState.coins;

            if (coinsText != null && currentCoins != previouslyDisplayedCoins)
            {
                coinsText.text = currentCoins.ToString();
                previouslyDisplayedCoins = currentCoins;
            }

            if (depthText != null && runManager.currentDepth != previouslyDisplayedDepth)
            {
                depthText.text = $"Level: {runManager.currentDepth}";
                previouslyDisplayedDepth = runManager.currentDepth;
            }

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
    private IEnumerator ForceBossEncounter()
    {   
        // This is to reuse the reward popup text broadcast because it has a fade than creating a new script
        if (RewardPopup.Instance != null)
        {
            RewardPopup.Instance.Show(timerExpiredMessage);
        }

        yield return new WaitForSeconds(forcedBossDelay);

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

            if (RunManager.Instance == null || fallbackEnemyPool == null)
            {
                Debug.LogWarning("Cannot start fallback boss battle.");
                yield break;
            }

            int currentDepth = RunManager.Instance.currentDepth;
            GameObject boss = fallbackEnemyPool.GetRandomBoss(currentDepth);

            if (boss == null)
            {
                Debug.LogWarning("No fallback boss was found in the EnemyPool.");
                yield break;
            }

            string returnScene = UnityEngine.SceneManagement
                .SceneManager.GetActiveScene().name;

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
        
    public void SetCrosshair(bool useCrosshair)
    {
        crosshair.SetActive(!useCrosshair);
    }
    
    public void EnableInteractionText(string text)
    {
        if (interactionParent == null && interactionText == null)
        {
            return;
        }

        interactionText.text = text + " (" + Settings.interactKey.ToString() + ")";
        interactionParent.gameObject.SetActive(true);
    }

    public void DisableInteractionText()
    {
        if (interactionParent == null)
        {
            return;
        }
        
        interactionParent.SetActive(false);
    }
}
