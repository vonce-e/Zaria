// Defines a prop prefab and the probability thresholds that control its spawn amount.
// Written by Andrew Burke.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines a prop prefab and the probability thresholds that determine how many instances spawn.
/// </summary>
[System.Serializable]
public class PropSpawnRule
{
    public GameObject prefab;

    public bool mustSpawn;
    
    [Header("Spawn Chance")]
    public int lowAmount;
    public int mediumAmount;
    public int highAmount;
    
    [Range(0, 100)] public int mediumThreshold;
    [Range(0, 100)] public int highThreshold;

    [Header("Dimensions")]
    [Min(1)] public int footprintWidth = 1;
    [Min(1)] public int footprintDepth = 1;

    [Min(0)] public int wallClearance = 0;
    [Min(0)] public int doorWayClearance = 0;
}
