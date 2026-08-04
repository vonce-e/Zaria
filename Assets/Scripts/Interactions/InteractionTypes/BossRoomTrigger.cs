// This script handles the Boss room trigger.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Starts a boss battle.
/// </summary>
public class BossRoomTrigger : MonoBehaviour
{
    [Tooltip("The shared pool of enemy/boss prefabs.")]
    public EnemyPool enemyPool;

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

        if (enemyPool == null)
        {
            Debug.LogWarning("BossRoomTrigger : no EnemyPool assigned.");
            return;
        }

        // Pick a random boss for the current depth.
        int depth = RunManager.Instance.currentDepth;
        GameObject boss = enemyPool.GetRandomBoss(depth);
        if (boss == null) return;

        RunManager.Instance.LoadBattle(boss, battleSceneName, returnSceneName, true);
    }
}