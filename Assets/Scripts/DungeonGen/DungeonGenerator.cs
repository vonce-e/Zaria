// This script was made to then give the steps on how to create those rooms by coordinating the dimensions and layout
// Made by andrew

using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator
{
    List<RoomNode> allSpaceNodes = new List<RoomNode>();
    private int _dungeonWidth;
    private int _dungeonLength;

    public DungeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this._dungeonWidth = dungeonWidth;
        this._dungeonLength = dungeonLength;
    }

    public List<Node> CalculateRooms(int maxIterations, int roomWidthMin, int roomLengthMin)
    {
        BinarySpacePartitioner bsp = new BinarySpacePartitioner(_dungeonWidth, _dungeonLength);
        allSpaceNodes = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
        
        // This will find the lowest nodes that have no children and that will be the room
        List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);

        // This will then parse the room spaces to create the room
        RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
        List<RoomNode> roomList = roomGenerator.GenerateRoomInGivenSpaces(roomSpaces); 
        
        return new List<Node>(roomList);
    }
}