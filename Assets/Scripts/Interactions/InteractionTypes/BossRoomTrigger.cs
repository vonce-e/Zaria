// This script handles the Boss room trigger.
// Made by Vonce Chew

using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Tooltip("The boss prefab to fight.")]
    public GameObject bossPrefab;

    [Tooltip("Name of the battle scene to load.")]
    public string battleSceneName = "BattleScene";

    [Tooltip("Name of the dungeon scene to return to after winning.")]
    public string returnSceneName;

    public void TeleportPlayer()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogWarning("BossRoomTrigger : no RunManager.");
            return;
        }
        RunManager.Instance.LoadBattle(bossPrefab, battleSceneName, returnSceneName);
    }
}