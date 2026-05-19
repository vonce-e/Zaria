using System.Collections.Generic;
using UnityEngine;

public class Dungeon3DVisualiser : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform dungeonParent;
    [SerializeField] private float cellSize = 4f;
    [SerializeField] private float wallDistanceFromFloorCenter = 0.5f;
    
    /// <summary>
    /// This function creates floor prefabs for every grid position the random walk has visited.
    /// </summary>
    public void CreateFloorTiles(IEnumerable<Vector2Int> floorPositions)  
    {
        HashSet<Vector2Int> floorPositionSet = new HashSet<Vector2Int>(floorPositions);
        PaintFloorTiles(floorPositionSet);
        PaintWallTiles(floorPositionSet);
    }
    
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
    /// This will generate the wall tiles on every tile that has no neighbouring tile
    /// </summary>
    /// <param name="floorPositions"></param>
    private void PaintWallTiles(HashSet<Vector2Int> floorPositions)
    {
        if (wallPrefab == null)
            return;

        foreach (Vector2Int floorPosition in floorPositions)
        {
            TryCreateWall(floorPositions, floorPosition, Vector2Int.up, 0f);
            TryCreateWall(floorPositions, floorPosition, Vector2Int.right, 90f);
            TryCreateWall(floorPositions, floorPosition, Vector2Int.down, 180f);
            TryCreateWall(floorPositions, floorPosition, Vector2Int.left, 270f);
        }
    }
    
    private void TryCreateWall(HashSet<Vector2Int> floorPositions, Vector2Int floorPosition, Vector2Int direction, float yRotation)
    {
        Vector2Int neighbourPosition = floorPosition + direction;

        if (floorPositions.Contains(neighbourPosition))
            return;

        Vector3 wallPosition = GridToWorldPosition(floorPosition) + new Vector3(direction.x, 0f, direction.y) * wallDistanceFromFloorCenter * cellSize;
        Quaternion wallRotation = Quaternion.Euler(0f, yRotation, 0f);

        Instantiate(wallPrefab, wallPosition, wallRotation, GetDungeonParent());
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, 0f, gridPosition.y * cellSize);
    }

    private Transform GetDungeonParent()
    {
        return dungeonParent == null ? transform : dungeonParent;
    }

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
}
