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
                depthText.text = $"Depth: {runManager.currentDepth}";
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

    private IEnumerator ForceBossEncounter()
    {   
        // This is to reuse the reward popup text broadcast because it has a fade than creating a new script
        if (RewardPopup.Instance != null)
        {
            RewardPopup.Instance.Show(timerExpiredMessage);
        }

        yield return new WaitForSeconds(forcedBossDelay);

        BossRoomTrigger bossTrigger = FindFirstObjectByType<BossRoomTrigger>();

        if (bossTrigger != null)
        {
            Debug.Log("Normal boss portal was chosen.");
            bossTrigger.TeleportPlayer();
            yield break;
        }

        EnemyPortal enemyPortal = FindFirstObjectByType<EnemyPortal>();

        if (enemyPortal != null)
        {
            Debug.Log("Mini boss portal was chosen.");
            enemyPortal.StartForcedDepthBattle();
            yield break;
        }

        if (bossTrigger == null)
        {
            Debug.LogWarning("Timer expired, but no boss room trigger was found. ");
            yield break;
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
