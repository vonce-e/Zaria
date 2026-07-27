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
    
    public int lowAmount;
    public int mediumAmount;
    public int highAmount;
    
    [Range(0, 100)] public int mediumThreshold;
    [Range(0, 100)] public int highThreshold;
}
