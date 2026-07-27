// Converts generated dungeon tile data into floor, wall, ceiling, and internal-wall GameObjects.
// Written by Andrew Burke.

using System;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon3DVisualiser : MonoBehaviour
{   
    [Header("Prefab Backups")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    [Header("Room Visualisation Prefabs")]
    [SerializeField] List<RoomVisualisationClass> roomVisuals;

    [Header("Dungeon Properties")]
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private float cellSize = 4f;
    [SerializeField] private float ceilingHeight = 4f;
    [SerializeField] private float wallDistanceFromFloorCenter = 0.5f;
    
    /// <summary>
    /// This function creates floor prefabs for every grid position the random walk has visited.
    /// </summary>
    public void CreateFloorTiles(IEnumerable<Vector2Int> floorPositions)  
    {
        HashSet<Vector2Int> floorPositionSet = new HashSet<Vector2Int>(floorPositions);
        PaintFloorTiles(floorPositionSet);
        PaintWallTiles(floorPositionSet);
        PaintCeilingTiles(floorPositionSet);
    }

    /// <summary>
    /// Creates room-aware floors, corridor floors, walls, internal walls, and ceilings.
    /// </summary>
    public void CreateTiles(IEnumerable<Vector2Int> floorPositions, List<DungeonRoomData> generatedRooms)  
    {
        HashSet<Vector2Int> floorPositionSet = new HashSet<Vector2Int>(floorPositions);
        
        PaintRoomFloorTiles(generatedRooms);
        PaintCorridorTiles(floorPositionSet, generatedRooms);
        PaintWallTilesNew(floorPositionSet, generatedRooms);
        PaintCeilingTiles(floorPositionSet);
    }

    #region Room Visualisation Test
    
    /// <summary>
    /// This method will paint the room floors depending on what room type it is and what floor prefab it needs
    /// </summary>
    /// <param name="generatedRooms"></param>
    private void PaintRoomFloorTiles(List<DungeonRoomData> generatedRooms)
    {
        if (generatedRooms == null || generatedRooms.Count == 0)
        {
            return;
        }

        foreach (DungeonRoomData room in generatedRooms)
        {
            GameObject selectedFloorPrefab = floorPrefab;
            RoomVisualisationClass selectedVisualRule = null;

            foreach (RoomVisualisationClass visualRule in roomVisuals)
            {
                if (visualRule.roomType == room.TypeOfRoom)
                {
                    selectedVisualRule = visualRule;
                    break;
                }
            }

            // Paints the floor tile based on the position of the floor tile
            foreach (Vector2Int tile in room.FloorTiles)
            {
                if (selectedVisualRule != null)
                {
                    selectedFloorPrefab = GetRandomPrefabWeight(selectedVisualRule.floorPrefabs, floorPrefab);
                }

                Instantiate(selectedFloorPrefab, GridToWorldPosition(tile), Quaternion.identity, GetDungeonParent());
            }
        }

    }

    /// <summary>
    /// Paints exterior and internal walls with visual variations selected for each room type.
    /// </summary>
    private void PaintWallTilesNew(HashSet<Vector2Int> floorPositions, List<DungeonRoomData> generatedRooms)
    {
       if(IsMissingFloorOrRoomData(floorPositions, generatedRooms))
        {
            return;
        }

       HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

        foreach(DungeonRoomData room in generatedRooms)
        {
            GameObject selectedWallPrefab = wallPrefab;
            RoomVisualisationClass selectedVisualRule = null;

            foreach(RoomVisualisationClass visualRule in roomVisuals)
            {
                if (visualRule.roomType == room.TypeOfRoom)
                {
                    selectedVisualRule = visualRule;
                    break;
                }
            }

            foreach(Vector2Int floorPosition in room.FloorTiles)
            {
                if (selectedVisualRule != null)
                {
                    selectedWallPrefab  = GetRandomPrefabWeight(selectedVisualRule.wallPrefabs, wallPrefab);
                    TryCreateWall(selectedWallPrefab, floorPositions, floorPosition, Vector2Int.up, 0f);
                    TryCreateWall(selectedWallPrefab, floorPositions, floorPosition, Vector2Int.right, 90f);
                    TryCreateWall(selectedWallPrefab, floorPositions, floorPosition, Vector2Int.down, 180f);
                    TryCreateWall(selectedWallPrefab, floorPositions, floorPosition, Vector2Int.left, 270f);

                    roomTiles.Add(floorPosition);
                }   
            }

            foreach (InnerWallData innerWall in room.InnerWalls)
            {
                GameObject innerWallPrefab = wallPrefab;

                if (selectedVisualRule != null)
                {
                    innerWallPrefab = GetRandomPrefabWeight(selectedVisualRule.wallPrefabs, wallPrefab);
                }

                if (innerWallPrefab == null)
                {
                    continue;
                }

                Vector3 wallPos = GridToWorldPosition(innerWall.Tile) +
                new Vector3(innerWall.Direction.x, 0f, innerWall.Direction.y) *
                wallDistanceFromFloorCenter * cellSize;

                float wallRotation = 0f;

                if (innerWall.Direction == Vector2Int.right)
                {
                    wallRotation = 90f;
                }
                else if (innerWall.Direction == Vector2Int.down)
                {
                    wallRotation = 180f;
                }

                else if (innerWall.Direction == Vector2Int.left)
                {
                    wallRotation = 270f;
                }

                Quaternion rotation = Quaternion.Euler(0f, wallRotation, 0f);
                Instantiate(innerWallPrefab, wallPos, rotation, GetDungeonParent());
            }
        }

        foreach (Vector2Int position in floorPositions)
        {
            if (roomTiles.Contains(position))
            {
                continue;
            }

            TryCreateWall(wallPrefab, floorPositions, position, Vector2Int.up, 0f);
            TryCreateWall(wallPrefab, floorPositions, position, Vector2Int.right, 90f);
            TryCreateWall(wallPrefab, floorPositions, position, Vector2Int.down, 180f);
            TryCreateWall(wallPrefab, floorPositions, position, Vector2Int.left, 270f);
        }
    }

    /// <summary>
    /// Paints floor tiles that belong to corridors rather than generated rooms.
    /// </summary>
    private void PaintCorridorTiles(HashSet<Vector2Int> floorPositions, List<DungeonRoomData> generatedRooms)
    {
        if(IsMissingFloorOrRoomData(floorPositions, generatedRooms))
        {
            return;
        }

        HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

        foreach (DungeonRoomData roomData in generatedRooms)
        {
            roomTiles.UnionWith(roomData.FloorTiles);
        }

        foreach (Vector2Int position in floorPositions)
        {
            if (roomTiles.Contains(position))
            {
                continue;
            }

            Instantiate(floorPrefab, GridToWorldPosition(position), Quaternion.identity, GetDungeonParent());
        }
    }

    /// <summary>
    /// Helper method to ensure the floor positions or generated rooms are not null to avoid crashes
    /// </summary>
    /// <returns>True or false</returns>
    private bool IsMissingFloorOrRoomData(HashSet<Vector2Int> floorPositions, List<DungeonRoomData> generatedRooms)
{
    if (floorPositions == null || floorPositions.Count == 0)
    {
        return true;
    }

    if (generatedRooms == null || generatedRooms.Count == 0)
    {
        return true;
    }

    return false;
}

    /// <summary>
    /// Selects a valid prefab using the configured weights or returns the fallback prefab.
    /// </summary>
    private GameObject GetRandomPrefabWeight(List<RoomVisualisationPrefabWeight> prefabRules, GameObject fallBackPrefab)
    {
       if (prefabRules == null || prefabRules.Count == 0)
        {
            return fallBackPrefab;
        } 

        int totalWeight = 0;

        foreach (RoomVisualisationPrefabWeight prefabRule in prefabRules)
        {
            if (prefabRule.prefab != null)
            {
                totalWeight += prefabRule.weight;
            }
        }

        if (totalWeight <= 0)
        {
            return fallBackPrefab;
        }

        int randomWeight = UnityEngine.Random.Range(0, 100);
        int currentWeight = 0;

        foreach(RoomVisualisationPrefabWeight rule in prefabRules)
        {
            if (rule.prefab == null)
            {
                continue;
            }

            currentWeight += rule.weight;

            if (randomWeight < currentWeight)
            {
                return rule.prefab;
            }
        }

        return fallBackPrefab;
    }

    #endregion
    
    #region Room Visualisation code Old
    /// <summary>
    /// This will generate the floor tiles by taking in the floor positions that has been generated
    /// </summary>
    /// <param name="floorPositions"></param>
    private void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        if (floorPrefab == null)
        {
            Debug.LogWarning("Dungeon3DVisualiser is missing a floor prefab.");
            return;
        }
        
        // Paints the tiles
        foreach (Vector2Int position in floorPositions)
        {
            Instantiate(floorPrefab, GridToWorldPosition(position), Quaternion.identity, GetDungeonParent());
        }
    }
    
    /// <summary>
    /// This method will generate the ceiling tiles in relation to the floor positions of the grid
    /// </summary>
    /// <param name="floorPositions"> x,y coordinates of where the random walk algorithm has walked over</param>
    private void PaintCeilingTiles(IEnumerable<Vector2Int> floorPositions)
    {
        if (floorPrefab == null)
        {
            Debug.LogWarning("Dungeon3DVisualiser is missing a floor prefab.");
            return;
        }

        foreach (Vector2Int position in floorPositions)
        {
            Vector3 ceilingPosition = GridToWorldPosition(position) + new Vector3(0,1,0) * ceilingHeight;
            Quaternion ceilingRotation = Quaternion.Euler(180f, 0f, 0f);
            Instantiate(floorPrefab, ceilingPosition, ceilingRotation, GetDungeonParent());
        }
    }
    
    /// <summary>
    /// This will generate the wall tiles on every tile that has no neighbouring tile
    /// </summary>
    /// <param name="floorPositions"></param>
    private void PaintWallTiles(HashSet<Vector2Int> floorPositions)
    {
        if (wallPrefab == null)
            return;

        foreach (Vector2Int floorPosition in floorPositions)
        {
            TryCreateWall(wallPrefab, floorPositions, floorPosition, Vector2Int.up, 0f);
            TryCreateWall(wallPrefab, floorPositions, floorPosition, Vector2Int.right, 90f);
            TryCreateWall(wallPrefab, floorPositions, floorPosition, Vector2Int.down, 180f);
            TryCreateWall(wallPrefab, floorPositions, floorPosition, Vector2Int.left, 270f);
        }
    }
    
    /// <summary>
    /// Creates an exterior wall when the neighbouring position in the requested direction has no floor.
    /// </summary>
    private void TryCreateWall(GameObject wallPrefab,HashSet<Vector2Int> floorPositions, Vector2Int floorPosition, Vector2Int direction, float yRotation)
    {
        if(wallPrefab == null)
        {
            return;
        }

        Vector2Int neighbourPosition = floorPosition + direction;

        if (floorPositions.Contains(neighbourPosition))
            return;

        Vector3 wallPosition = GridToWorldPosition(floorPosition) + new Vector3(direction.x, 0f, direction.y) * wallDistanceFromFloorCenter * cellSize;
        Quaternion wallRotation = Quaternion.Euler(0f, yRotation, 0f);

        Instantiate(wallPrefab, wallPosition, wallRotation, GetDungeonParent());
    }

    #endregion

    #region Helper Methods
    /// <summary>
    /// Converts a dungeon grid coordinate into its corresponding world-space position.
    /// </summary>
    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, 0f, gridPosition.y * cellSize);
    }

    /// <summary>
    /// Returns the configured dungeon parent or this visualiser's transform as a fallback.
    /// </summary>
    private Transform GetDungeonParent()
    {
        return dungeonParent == null ? transform : dungeonParent;
    }

    /// <summary>
    /// Removes all previously generated visual objects from the dungeon parent.
    /// </summary>
    public void Clear()
    {
        Transform parent = GetDungeonParent();

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    #endregion
}
