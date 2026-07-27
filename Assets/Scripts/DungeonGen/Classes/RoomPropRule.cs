// Defines the prop categories and layout patterns assigned to each generated room type.
// Written by Andrew Burke.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the prop categories and layout patterns available to a specific room type.
/// </summary>
[System.Serializable]
public class RoomPropRule
{
    public RoomType roomType;
    public PropCategorySpawnRule cornerProps;
    public PropCategorySpawnRule innerTileProps;
    public PropCategorySpawnRule nearWallTileProps;
    public PropCategorySpawnRule wallMountedProps;
    public PropCategorySpawnRule ceilingProps;
    public List<RoomLayoutPattern> layoutPatterns;

}
