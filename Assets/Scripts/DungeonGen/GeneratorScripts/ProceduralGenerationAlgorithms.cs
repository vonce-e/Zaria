// Provides reusable random walk, corridor, direction, and binary space partitioning algorithms.
// Written by Andrew Burke.

using UnityEngine;
using System.Collections.Generic;

public static class ProceduralGenerationAlgorithms
{
    /// <summary>
    /// Creates a path by repeatedly moving from the previous position in a random cardinal direction.
    /// </summary>
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPosition);
        var previousPosition = startPosition;

        for (int i = 0; i < walkLength; i++)
        {
            var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
            path.Add(newPosition);
            previousPosition = newPosition;
        }
        return path;
    }

    /// <summary>
    /// Creates a straight corridor from a start position in one randomly selected cardinal direction.
    /// </summary>
    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);

        for (int i = 0; i < corridorLength; i++)
        {
            currentPosition += direction;
            corridor.Add(currentPosition);
        }
        return corridor;
    }

    /// <summary>
    /// Recursively divides a bounded space into rooms that satisfy the configured minimum dimensions.
    /// </summary>
    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList =  new List<BoundsInt>();

        roomsQueue.Enqueue(spaceToSplit);
        
        // While there are rooms to split
        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();

            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                // Split the room horizontally
                if (Random.value < 0.5f)
                {
                    // If the room has enough space to minimally cut into 2, then split it horizontally
                    if (room.size.y > minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    // If the space is enough to fit two rooms inside, then split it vertically.
                    else if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    // If the space is close to the min, stop deviding and add it to the list.
                    else if ( room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                
                // Split the room vertically
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        SplitVertically(minWidth, roomsQueue, room);
                    }
                    else if (room.size.y > minHeight * 2)
                    {
                        SplitHorizontally(minHeight, roomsQueue, room);
                    }
                    else if ( room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

    /// <summary>
    /// Splits a room along the X axis and queues both resulting bounds for further processing.
    /// </summary>
    private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        // Starts the room 1 unit from the min point from the border, with the room's x value
        var xSplit = Random.Range(1, room.size.x);
        
        // Defines the rooms bound, with the starting point as room.min, and the size of room 1 after it gets vertically split
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
            new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        
        // Then line the rooms into the queue
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }

    /// <summary>
    /// Splits a room along the Y axis and queues both resulting bounds for further processing.
    /// </summary>
    private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
    {
        // Starts the room from 1 unit from the min point from the border, with the room's y value
        var ySplit = Random.Range(1, room.size.y); // (minHeight, room.size.y - minHeight) < to obtain a grid like structure room
        
        // Defines the rooms bound, with the starting point as room.min, and the size of room 1 after it gets horizontally split
        BoundsInt room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
        BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
            new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        
        // Then line the rooms into the queue
        roomsQueue.Enqueue(room1);
        roomsQueue.Enqueue(room2);
    }
}


public static class Direction2D
{
    public static List<Vector2Int> CardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0, 1), // UP
        new Vector2Int(1, 0), // RIGHT
        new Vector2Int(0, -1), // DOWN
        new Vector2Int(-1, 0) // LEFT
    };

    /// <summary>
    /// Returns one randomly selected cardinal direction.
    /// </summary>
    public static Vector2Int GetRandomCardinalDirection()
    {
        return CardinalDirectionsList[Random.Range(0, CardinalDirectionsList.Count)];
    }
}
