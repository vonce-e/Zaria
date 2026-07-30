// This script handles the central pool of enemy and boss prefabs.
// Made by Vonce Chew

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject holding all normal-enemy and boss prefabs.
/// </summary>
[CreateAssetMenu(menuName = "Zaria/Enemy Pool")]
public class EnemyPool : ScriptableObject
{
    [Tooltip("All normal enemy prefabs (Hound, Ash Guard, etc.).")]
    public List<GameObject> normalEnemies = new List<GameObject>();

    [Tooltip("All boss prefabs (Vatrax, Null, etc.).")]
    public List<GameObject> bosses = new List<GameObject>();

    /// <summary>
    /// Pick a random normal enemy.
    /// </summary>
    /// <param name="depth">Current run depth (1 = first map).</param>
    public GameObject GetRandomEnemy(int depth)
    {
        if (normalEnemies == null || normalEnemies.Count == 0)
        {
            Debug.LogWarning("EnemyPool has no normal enemies assigned.");
            return null;
        }
        int index = Random.Range(0, normalEnemies.Count);
        return normalEnemies[index];
    }

    /// <summary>
    /// Pick a random boss.
    /// </summary>
    /// <param name="depth">Current run depth (1 = first map).</param>
    public GameObject GetRandomBoss(int depth)
    {
        if (bosses == null || bosses.Count == 0)
        {
            Debug.LogWarning("EnemyPool has no bosses assigned.");
            return null;
        }
        int index = Random.Range(0, bosses.Count);
        return bosses[index];
    }
}