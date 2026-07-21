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

    public GameObject pendingRoomPrefab;

    // The scene to return to after a battle (the dungeon/next level)
    public string returnSceneName;

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
        runState = new PlayerRunState { playerLevel = 1 };

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

        Debug.Log("New run started.");
    }

    /// <summary>
    /// Queue an encounter and load the battle scene.
    /// </summary>
    /// <param name="encounterPrefab">The enemy/boss prefab to fight.</param>
    /// <param name="battleSceneName">The battle scene to load.</param>
    /// <param name="returnScene">Scene to come back to after winning.</param>
    public void LoadBattle(GameObject encounterPrefab, GameObject roomPrefab, string battleSceneName, string returnScene)
    {
        pendingEncounterPrefab = encounterPrefab;
        pendingRoomPrefab = roomPrefab;
        returnSceneName = returnScene;

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ChangeScene(battleSceneName);
        else
            Debug.LogError("SceneLoader.Instance is null - can't load the battle scene.");
    }
}