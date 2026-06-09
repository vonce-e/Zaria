using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomPropSet
{
    public List<GameObject> cornerProps;
    public int minCornerProps;
    public int maxCornerProps;
    
    public List<GameObject> innerTileProps;
    public int minInnerTileProps;
    public int maxInnerTileProps;
    
    public List<GameObject> nearWallTileProps;
    public int minNearWallTileProps;
    public int maxNearWallTileProps;
}
