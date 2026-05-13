using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator
{
    RoomNode rootNode;
    List<RoomNode> allSpaceNodes = new List<RoomNode>();
    private int _dungeonWidth;
    private int _dungeonLength;

    public DungeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this._dungeonWidth = dungeonWidth;
        this._dungeonLength = dungeonLength;
    }

    internal object CalculateRooms(int maxIterations, int roomWidth, int roomLength)
    {
        throw new NotImplementedException();
    }
    
}
