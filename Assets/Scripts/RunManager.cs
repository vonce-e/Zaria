// This script persists the player's run across scene loads.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// A DontDestroyOnLoad singleton that owns the run's PlayerRunState and the pending encounter. 
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    // The one run state shared by every scene this run
    public PlayerRunState runState = new PlayerRunState();

    // The enemy/boss prefab the next battle scene should spawn
    public GameObject pendingEncounterPrefab;

    // The scene to return to after a battle (the dungeon/next level)
    public string returnSceneName;

    [Tooltip("How deep into the run (1 = first map). Increments per map.")]
    public int currentDepth = 1;

    [Tooltip("True if the pending battle is a boss fight (used to advance depth on win).")]
    public bool pendingIsBoss;

    [Header("Dungeon Timer")] 
    [SerializeField] private float timePerDepth = 480f;

    public float remainingDungeonTime;
    [HideInInspector] public bool dungeonTimerExpired;

    [Header("Tutorial Check")]
    // Checks if they have done the tutorial
    public bool hasPlayedTutorial;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Begin a brand-new run : fresh deck/coins/XP.
    /// </summary>
    public void StartNewRun()
    {
        runState = new PlayerRunState { playerLevel = 1 }; // fresh run, empties deck

        // Starting deck
        runState.deck.Add(new CardInstance(CardId.Slash));
        runState.deck.Add(new CardInstance(CardId.Slash));
        runState.deck.Add(new CardInstance(CardId.Slash));
        runState.deck.Add(new CardInstance(CardId.QuickJab));
        runState.deck.Add(new CardInstance(CardId.TwinStrike));
        runState.deck.Add(new CardInstance(CardId.Guard));
        runState.deck.Add(new CardInstance(CardId.Guard));
        runState.deck.Add(new CardInstance(CardId.Brace));
        runState.deck.Add(new CardInstance(CardId.DiceRoll));
        runState.deck.Add(new CardInstance(CardId.Energize));
        
        // Starting potion
        runState.potions.Add(new PotionInstance(PotionId.Recharge));

        // Bind the reward service to this run so chest/combat rewards use the real deck.
        if (CardRewardService.Instance != null)
            CardRewardService.Instance.BindRun(runState);
        
        // Reset the player's timer & depth
        currentDepth = 1;
        remainingDungeonTime = timePerDepth;
        dungeonTimerExpired = false;

        Debug.Log("New run started.");
    }

    /// <summary>
    /// Advance to the next map's depth
    /// </summary>
    public void AdvanceDepth()
    {
        currentDepth++;

        // Reset the timer for the new depth
        remainingDungeonTime = timePerDepth;
        dungeonTimerExpired = false;
        
        Debug.Log($"Advanced to depth {currentDepth}.");
    }

    /// <summary>
    /// Queue an encounter and load the battle scene.
    /// </summary>
    /// <param name="encounterPrefab">The enemy/boss prefab to fight.</param>
    /// <param name="battleSceneName">The battle scene to load.</param>
    /// <param name="returnScene">Scene to come back to after winning.</param>
    public void LoadBattle(GameObject encounterPrefab, string battleSceneName, string returnScene, bool isBoss)
    {
        pendingEncounterPrefab = encounterPrefab;
        returnSceneName = returnScene;
        pendingIsBoss = isBoss;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ChangeScene(battleSceneName);
        else
            Debug.LogError("SceneLoader.Instance is null - can't load the battle scene.");
    }
}