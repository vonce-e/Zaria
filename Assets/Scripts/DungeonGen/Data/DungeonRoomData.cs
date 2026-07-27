// Stores the boundaries, tile categories, entrances, and internal sections of a generated room.
// Written by Andrew Burke.

using UnityEngine;
using System.Collections.Generic;

public class DungeonRoomData
{
    // Room data - Boundaries
    public BoundsInt Bounds;
    public Vector2Int CenterPoint;
    
    // Position vectors of the tiles
    public HashSet<Vector2Int> FloorTiles =  new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CornerTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> NearWallTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> InnerTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CeilingTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> CorridorTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> OccupiedTiles = new HashSet<Vector2Int>();
    public List<EntranceRoomData> EntranceTiles = new List<EntranceRoomData>();

    public RoomType TypeOfRoom = RoomType.Normal;

    // This holds the data for the inner rooms like walls, tiles and entrances
    public List<RoomSectionData> RoomSections = new List<RoomSectionData>();
    public List<InnerWallData> InnerWalls = new List<InnerWallData>();
    public List<InnerDoorWayData> InnerDoors = new List<InnerDoorWayData>();
 
}

/// <summary>
/// This defines where the "entrance" of the room is where the door will sit
/// </summary>
public class EntranceRoomData
{
    public Vector2Int Tile;
    public Vector2Int Direction;

    /// <summary>
    /// Creates entrance data for a room tile and the direction leading into its corridor.
    /// </summary>
    public EntranceRoomData(Vector2Int tile, Vector2Int direction)
    {
        Tile = tile;
        Direction = direction;
    }
}

/// <summary>
/// This will hold the data of the inner rooms
/// </summary>
public class RoomSectionData
{
    public HashSet<Vector2Int> FloorTiles = new HashSet<Vector2Int>();
    public Vector2Int CenterPoint;
}

/// <summary>
/// This method will store the direction and position that the inner wall will be placed at
/// in accordance to the objects and tiles around it
/// </summary>
public class InnerWallData
{
    public Vector2Int Tile;
    public Vector2Int Direction;

    /// <summary>
    /// Creates internal wall data from a supporting floor tile and the direction the wall faces.
    /// </summary>
    public InnerWallData(Vector2Int tile, Vector2Int direction)
    {
        Tile = tile;
        Direction = direction;
    }
}

public class InnerDoorWayData
{
    public List<Vector2Int> Tiles = new List<Vector2Int>();
    public Vector2Int Direction;
}

/// <summary>
/// This defines the room types that are in the game
/// </summary>
public enum RoomType
{
    Normal,
    Spawn,
    Treasure,
    Shop,
    Blacksmith,
    MiniBoss,
    Boss,
    Exit
}
