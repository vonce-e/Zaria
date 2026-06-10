using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomPropRule
{
    public RoomType roomType;
    
    public PropCategorySpawnRule cornerProps;
    public PropCategorySpawnRule innerTileProps;
    public PropCategorySpawnRule nearWallTileProps;
    public PropCategorySpawnRule wallMountedProps;
    
}
