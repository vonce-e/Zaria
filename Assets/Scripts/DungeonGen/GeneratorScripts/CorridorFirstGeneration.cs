// Generates a dungeon by creating corridors first and placing random-walk rooms at selected endpoints.
// Written by Andrew Burke.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstGeneration : SimpleRandomWalkGenerator
{
   [SerializeField] private int corridorLength = 14, corridorCount = 5;
   [SerializeField] [Min(1)] private int corridorWidth = 1;
   [SerializeField][Range(0.1f,1)] private float roomPercent = 0.8f;

   /// <summary>
   /// Starts corridor-first dungeon generation.
   /// </summary>
   protected override void RunProceduralGeneration()
   {
      CorridorFirstGenerator();
   }

   /// <summary>
   /// Creates corridors, places rooms at selected points and dead ends, and visualises the result.
   /// </summary>
   private void CorridorFirstGenerator()
   {
      HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
      HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
      
      CreateCorridors(floorPositions,  potentialRoomPositions);
      
      HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

      List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);
      
      CreateRoomsAtDeadEnds(deadEnds, roomPositions);
      
      floorPositions.UnionWith(roomPositions);
      
      visualiser.CreateFloorTiles(floorPositions);
   }

   /// <summary>
   /// Generates additional rooms at corridor dead ends that are not already covered by room floors.
   /// </summary>
   private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
   {
      foreach (var position in deadEnds)
      {
         if (!roomFloors.Contains(position))
         {
            var room = RunRandomWalk(randomWalkParameters, position);
            roomFloors.UnionWith(room);
         }
      }
   }

   /// <summary>
   /// Finds floor positions that connect to exactly one cardinal neighbour.
   /// </summary>
   private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
   {
      List<Vector2Int> deadEnds = new List<Vector2Int>();
      foreach (Vector2Int position in floorPositions)
      {
         int neighboursCount = 0;

         foreach (Vector2Int direction in Direction2D.CardinalDirectionsList)
         {
            if (floorPositions.Contains(position + direction))
               neighboursCount++;
         }
         
         if (neighboursCount == 1)
            deadEnds.Add(position);
      }
      
      return deadEnds;
   }

   /// <summary>
   /// Creates random-walk rooms at a shuffled subset of potential room positions.
   /// </summary>
   private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
   {
      HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
      int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

      List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();

      foreach (var roomPosition in roomsToCreate)
      {
         var roomFloor =  RunRandomWalk(randomWalkParameters, roomPosition);
         roomPositions.UnionWith(roomFloor);
      }
      return roomPositions;
   }

   /// <summary>
   /// Generates connected corridors and records their endpoints as potential room positions.
   /// </summary>
   private void CreateCorridors(HashSet<Vector2Int> floorPositions,  HashSet<Vector2Int> potentialRoomPositions)
   {
      var currentPosition = startPosition;
      potentialRoomPositions.Add(currentPosition);
      
      for (int i = 0; i < corridorCount; i++)
      {
         var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
         currentPosition = corridor[corridor.Count - 1];
         potentialRoomPositions.Add(currentPosition);
         floorPositions.UnionWith(WidenCorridor(corridor));
      }
   }

   /// <summary>
   /// Expands a one-tile corridor perpendicular to its travel direction.
   /// </summary>
   private HashSet<Vector2Int> WidenCorridor(List<Vector2Int> corridor)
   {
      HashSet<Vector2Int> wideCorridor = new HashSet<Vector2Int>();

      // Checks to see if a corridor has no direction next
      if (corridor.Count < 2)
      {
         wideCorridor.UnionWith(corridor);
         return wideCorridor;
      }

      Vector2Int direction = corridor[1] - corridor[0];
      Vector2Int perpendicularDirection = new Vector2Int(-direction.y, direction.x);

      foreach (Vector2Int position in corridor)
      {
         for (int widthOffset = 0; widthOffset < corridorWidth; widthOffset++)
         {
            wideCorridor.Add(position + perpendicularDirection * widthOffset);
         }
      }

      return wideCorridor;
   }
}
