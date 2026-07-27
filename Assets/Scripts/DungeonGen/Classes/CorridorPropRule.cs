// Defines the prop placement categories available along generated corridors.
// Written by Andrew Burke.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Defines the prop categories that can be spawned along generated corridors.
/// </summary>
[System.Serializable]
public class CorridorPropRule
{
    public PropCategorySpawnRule wallMountedProps;
    public PropCategorySpawnRule ceilingProps;
}
