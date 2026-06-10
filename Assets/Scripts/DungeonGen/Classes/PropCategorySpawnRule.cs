using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PropCategorySpawnRule
{
    public List<GameObject> prefabs;
    
    public int lowAmount;
    public int mediumAmount;
    public int highAmount;
    
    [Range(0, 100)] public int mediumThreshold;
    [Range(0, 100)] public int highThreshold;
}
