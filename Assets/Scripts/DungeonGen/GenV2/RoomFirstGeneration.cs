// This script was made to generate the rooms using ProceduralGenerationAlgorithm.cs to use binary space partionining to split the rooms
// Made by Andrew

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomFirstGeneration : SimpleRandomWalkGenerator
{
   // This declares the boundary of how we want to generate those rooms in a certain size
   [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;
   [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;
   [SerializeField] [Min(1)] private int corridorWidth = 1;
   [SerializeField][Range(0,10)] private int offset = 1; // Offsets the rooms gen from the boundary box
   [SerializeField] bool randomWalkRooms = false; // Responsible to check if we want to use the random walk algorithim

   protected override void RunProceduralGeneration()
   {
      CreateRooms();
   }

   private void CreateRooms()
   {
      var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)(startPosition),
         new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);
      
      HashSet<Vector2Int> floor = new  HashSet<Vector2Int>();

      if (randomWalkRooms)
      {
         floor = CreateRoomsRandomly(roomsList);
      }
      else
      {
         floor = CreateSimpleRooms(roomsList);
      }
      
      // This will contain the x,y center coordinates of each room
      List<Vector2Int> roomCenters = new List<Vector2Int>();
      foreach (var room in roomsList)
      {
         roomCenters.Add((Vector2Int)(Vector3Int.RoundToInt(room.center)));
      }

      HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
      floor.UnionWith(corridors);
      
      // Parse in the floor tiles to create the rooms and visually paint it
      visualiser.CreateFloorTiles(floor);
   }

   private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
   {
      HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

      for (int i = 0; i < roomsList.Count; i++)
      {
         var roomBounds = roomsList[i];
         var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
         var roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
         foreach (var position in roomFloor)
         {
            if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) 
               && position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
            {
               floor.Add(position);
            }
         }
      }
      return floor;
   }

   private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
   {
      HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
      var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
      roomCenters.Remove(currentRoomCenter);

      while (roomCenters.Count > 0)
      {
         // Finds the closest center
         Vector2Int closestCenter = FindClosestPointTo(currentRoomCenter, roomCenters);
         roomCenters.Remove(closestCenter);
         
         // Creates the corridor
         HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closestCenter);
         currentRoomCenter = closestCenter;
         corridors.UnionWith(newCorridor);
      }

      return corridors;
   }
   
   private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
   {
      List<Vector2Int> corridor = new List<Vector2Int>();
      
      var position = currentRoomCenter;
      corridor.Add(position);

      while (position.y != destination.y)
      {
         if (destination.y > position.y)
         {
            position += Vector2Int.up;
         }
         else if (destination.y < position.y)
         {
            position += Vector2Int.down;
         }

         corridor.Add(position);
      }

      while (position.x != destination.x)
      {
         if (destination.x > position.x)
         {
            position += Vector2Int.right;
         }
         else if (destination.x < position.x)
         {
            position += Vector2Int.left;
         }
         
         corridor.Add(position);
      }
      
      return WidenCorridor(corridor);
   }
   
   /// <summary>
   /// This method will widen the corridor path as the room is generated based on the width given 
   /// </summary>
   /// <param name="corridor"></param>
   /// <returns></returns>
   private HashSet<Vector2Int> WidenCorridor(List<Vector2Int> corridor)
   {
      HashSet<Vector2Int> wideCorridor = new HashSet<Vector2Int>();
      
      // Safety check to avoid a corridor that has no direction next
      if (corridor.Count < 2)
      {
         wideCorridor.UnionWith(corridor);
         return wideCorridor;
      }

      for (int i = 0; i < corridor.Count; i++)
      {
         // Checks to see the direction of the previous tile to see where it came from
         if (i > 0)
         {
            AddWidth(wideCorridor, corridor[i], corridor[i] - corridor[i - 1]);
         }
         
         // If it has a next tile, see the next tile to see wher to go
         if (i < corridor.Count - 1)
         {
            AddWidth(wideCorridor, corridor[i], corridor[i+1] - corridor[i]);
         }
      }
      
      return wideCorridor;
   }

   private void AddWidth(HashSet<Vector2Int> wideCorridor, Vector2Int position, Vector2Int direction)
   {
      Vector2Int perpendicularDirection = new Vector2Int(-direction.y, direction.x);

      for (int widthOffset = 0; widthOffset < corridorWidth; widthOffset++)
      {
         wideCorridor.Add(position + perpendicularDirection * widthOffset);
      }
   }
   
   /// <summary>
   /// This method will find the closest center in each room to connect the next closest room with a corridor subsequently
   /// </summary>
   /// <param name="currentRoomCenter">This contains the x,y coordinates of the current room's center</param>
   /// <param name="roomCenters">This contains the list of all the rooms centers</param>
   /// <returns>Returns the closest rooms center</returns>
   private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
   {
      Vector2Int closest = Vector2Int.zero;
      float distance = float.MaxValue;

      foreach (var position in roomCenters)
      {
         float currentDistance = Vector2.Distance(position, currentRoomCenter);
         
         // Find the closest point
         if (currentDistance < distance)
         {
            distance = currentDistance;
            closest = position;
         }
      }
      return closest;
   }

   private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
   {
      HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

      foreach (var room in roomsList)
      {
         for (int col = 0; col < room.size.x - offset; col++)
         {
            for (int row = 0; row < room.size.y - offset; row++)
            {
               Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
               floor.Add(position);
            }
         }
      }
      return floor;
   }
}
