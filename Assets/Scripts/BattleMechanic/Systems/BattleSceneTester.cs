// Test helper script.

using UnityEngine;

public class BattleSceneTester : MonoBehaviour
{
    [Tooltip("The boss prefab to spawn for this test (e.g. Vatrax).")]
    public GameObject testBossPrefab;

    void Start()
    {
        if (RunManager.Instance == null)
        {
            GameObject rm = new GameObject("RunManager (test)");
            rm.AddComponent<RunManager>();
        }

        if (RunManager.Instance.runState == null ||
            RunManager.Instance.runState.deck.Count == 0)
        {
            RunManager.Instance.StartNewRun();

            // test potions
            RunManager.Instance.runState.potions.Add(new PotionInstance(PotionId.PhantomGuard));
            RunManager.Instance.runState.potions.Add(new PotionInstance(PotionId.GlassCannon));
        }

        RunManager.Instance.pendingEncounterPrefab = testBossPrefab;

        Debug.Log("BattleSceneTester: run ready, boss queued. Walk into the trigger.");
    }
}