using UnityEngine;
using System.Collections.Generic;

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
