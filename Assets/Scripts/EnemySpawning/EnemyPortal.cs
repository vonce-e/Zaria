// This script picks a random enemy from the EnemyPool
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Starts a normal enemy battle.
/// </summary>
public class EnemyPortal : MonoBehaviour
{
    [Tooltip("The shared pool of enemy prefabs.")]
    public EnemyPool enemyPool;

    [Tooltip("The normal enemy room geometry (fill once room prefabs exist).")]
    public GameObject roomPrefab;

    [Tooltip("Name of the battle scene to load.")]
    public string battleSceneName = "BattleScene";

    /// <summary>
    /// Pick a random enemy and start the battle.
    /// </summary>
    public void StartBattle()
    {
        if (enemyPool == null)
        {
            Debug.LogWarning("EnemyPortal has no EnemyPool assigned.");
            return;
        }
        if (RunManager.Instance == null)
        {
            Debug.LogWarning("No RunManager - can't start a battle.");
            return;
        }

        int depth = RunManager.Instance.currentDepth;
        GameObject enemy = enemyPool.GetRandomEnemy(depth);
        if (enemy == null) return;

        string returnScene = UnityEngine.SceneManagement
            .SceneManager.GetActiveScene().name;

        RunManager.Instance.LoadBattle(enemy, roomPrefab, battleSceneName, returnScene, false);
    }
}