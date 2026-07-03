// This class defines what information a generated room contains
// Made by andrew

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
 
}

public class EntranceRoomData
{
    public Vector2Int Tile;
    public Vector2Int Direction;

    public EntranceRoomData(Vector2Int tile, Vector2Int direction)
    {
        Tile = tile;
        Direction = direction;
    }
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