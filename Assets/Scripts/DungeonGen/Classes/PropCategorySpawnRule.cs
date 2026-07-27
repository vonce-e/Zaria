// Groups the prefab spawn rules used by one prop placement category.
// Written by Andrew Burke.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Groups the prefab spawn rules available to one prop placement category.
/// </summary>
[System.Serializable]
public class PropCategorySpawnRule
{
   public List<PropSpawnRule> prefabs;
}
