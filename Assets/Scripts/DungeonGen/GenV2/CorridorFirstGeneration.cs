using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstGeneration : SimpleRandomWalkGenerator
{
   [SerializeField] private int corridorLength = 14, corridorCount = 5;
   [SerializeField] [Min(1)] private int corridorWidth = 1;
   [SerializeField][Range(0.1f,1)] private float roomPercent = 0.8f;
   
   protected override void RunProceduralGeneration()
   {
      CorridorFirstGenerator();
   }

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

   private HashSet<Vector2Int> WidenCorridor(List<Vector2Int> corridor)
   {
      HashSet<Vector2Int> wideCorridor = new HashSet<Vector2Int>();

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
