// This script was made to generate the rooms using ProceduralGenerationAlgorithm.cs to use binary space partionining to split the rooms
// Made by Andrew

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomFirstGeneration : SimpleRandomWalkGenerator
{
   // This declares the boundary of how we want to generate those rooms in a certain size
   [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;
   [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;
   [SerializeField] [Min(1)] private int corridorWidth = 1;
   [SerializeField][Range(0,10)] private int offset = 1; // Offsets the rooms gen from the boundary box
   [SerializeField] bool randomWalkRooms = false; // Responsible to check if we want to use the random walk algorithm
   [SerializeField] private Transform propParent;
   
   
   // Temporary
   // [SerializeField] private GameObject cornerTileMarker;
   // [SerializeField] private GameObject innerTileMarker;
   // [SerializeField] private GameObject centerPointMarker;
   // [SerializeField] private GameObject nearWallTileMarker;
   // private float markerHeight = 0.2f;
   
   [Header("Prefab spawning testing")]
   [SerializeField] private List<GameObject> cornerProps;
   [SerializeField] private List<GameObject> innerTileProps;
   [SerializeField] private List<GameObject> nearWallTileProps;
   [SerializeField] private float cellSize;
   [SerializeField] private float propHeight;
   [SerializeField] private float wallPropHeight;
   [SerializeField] private float distanceFromWall;

   protected override void RunProceduralGeneration()
   {
      CreateRooms();
   }

   private void CreateRooms()
   {
      var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)(startPosition),
         new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);
      
      HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
      List<DungeonRoomData> generatedRooms = new List<DungeonRoomData>();
      
      if (randomWalkRooms)
      {
         floor = CreateRoomsRandomly(roomsList);
      }
      else
      {
         generatedRooms = CreateSimpleRoomData(roomsList);
         
         foreach (DungeonRoomData roomData in generatedRooms)
         {
            floor.UnionWith(roomData.FloorTiles);
         }
      }
      
      // This will contain the x,y center coordinates of each room
      List<Vector2Int> roomCenters = new List<Vector2Int>();
      foreach (var room in roomsList)
      {
         roomCenters.Add((Vector2Int)(Vector3Int.RoundToInt(room.center)));
      }
      PropParentChildrenCleaner();
      
      HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
      
      // Assign the corridors to the room data
      AssignCorridorDataToRoom(generatedRooms, corridors);
      
      floor.UnionWith(corridors);
      
      // Parse in the floor tiles to create the rooms and visually paint it
      visualiser.CreateFloorTiles(floor);
      
      // Spawns the prefabs
      SpawnProps(generatedRooms);
   }
   
   #region Prop spawnning LOGIC
   /// <summary>
   /// This method will spawn the props at a random tile with a random prefab that is stored in lists for corner tiles, inner tiles and near wall tiles.
   /// </summary>
   /// <param name="generatedRooms">This stores all the given rooms of the dungoen</param>
   private void SpawnProps(List<DungeonRoomData> generatedRooms)
   {
         foreach (DungeonRoomData roomData in generatedRooms)
         {
            // Get valid list of tiles that prefabs can be spawned on
            List<Vector2Int> validCornerTiles = GetValidTile(roomData, roomData.CornerTiles);
            List<Vector2Int> validInnerTiles = GetValidTile(roomData, roomData.InnerTiles);
            List<Vector2Int> validWallTiles = GetValidTile(roomData, roomData.NearWallTiles);
            
            // Instantiate the prefabs on the given tile
            TrySpawnPropAtTile(cornerProps, validCornerTiles, roomData, propHeight);
            TrySpawnPropAtTile(innerTileProps, validInnerTiles, roomData, propHeight);
            TrySpawnPropAtTile(nearWallTileProps, validWallTiles, roomData, wallPropHeight);
            
         }
   }
   
   /// <summary>
   /// This method will check if the given tiles are not a occupied or corridor tile
   /// </summary>
   /// <param name="data"></param>
   /// <param name="givenTiles"></param>
   /// <returns></returns>
   private List<Vector2Int> GetValidTile(DungeonRoomData data, HashSet<Vector2Int> givenTiles)
   {
      List<Vector2Int> validTiles = new List<Vector2Int>();

      foreach (Vector2Int tile in givenTiles)
      {
         if (data.CorridorTiles.Contains(tile) == false && data.OccupiedTiles.Contains(tile) == false)
         {
            validTiles.Add(tile);
         }
      }
      
      return validTiles;
   }
   
   /// <summary>
   /// This method will try to spawn a random prefab at a random tile in the dungeon in the different rooms
   /// </summary>
   /// <param name="objectToSpawn">Prefab to spawn</param>
   /// <param name="givenTiles">The tiles that exist in the different dungeon rooms</param>
   /// <param name="roomData"></param>
   private void TrySpawnPropAtTile(List<GameObject> objectToSpawn,  List<Vector2Int> givenTiles, DungeonRoomData roomData, float spawnHeight)
   {
      if (objectToSpawn == null || objectToSpawn.Count == 0)
      {
         return;
      }
      
      if (givenTiles == null || givenTiles.Count == 0)
      {
         return;
      }
      
      // Choosing from a random select of tiles
      int randomTileIndex = Random.Range(0, givenTiles.Count);
      Vector2Int tile = givenTiles[randomTileIndex];
      
      // Choosing from a random select of prefabs
      int randomPrefabIndex = Random.Range(0, objectToSpawn.Count);
      GameObject randomPrefab = objectToSpawn[randomPrefabIndex];

      // Instantiate object
      InstantiateProps(randomPrefab, tile, spawnHeight, roomData);
      
      // Adds the current tile to the occupied tiles
      roomData.OccupiedTiles.Add(tile);
   }

   private Vector2Int GetWallDirection(DungeonRoomData roomData, Vector2Int tile)
   {
      if (!roomData.FloorTiles.Contains(tile + Vector2Int.up))
      {
         return Vector2Int.up;
      }

      if (!roomData.FloorTiles.Contains(tile + Vector2Int.left))
      {
         return Vector2Int.left;
      }

      if (!roomData.FloorTiles.Contains(tile + Vector2Int.down))
      {
         return Vector2Int.down;
      }

      if (!roomData.FloorTiles.Contains(tile + Vector2Int.right))
      {
         return Vector2Int.right;
      }
      return Vector2Int.zero;
   }

   private Quaternion GetWallRotation(Vector2Int tileRotation)
   {          
      if (tileRotation == Vector2Int.up)
      {
         return Quaternion.Euler(0f, 180f, 0f);
      }

      if (tileRotation == Vector2Int.down)
      {
         return Quaternion.Euler(0f, 0f, 0f);
      }

      if (tileRotation == Vector2Int.right)
      {
         return Quaternion.Euler(0f, 270f, 0f);
      }

      if (tileRotation == Vector2Int.left)
      {
         return Quaternion.Euler(0, 90f, 0f);
      }
      
      return Quaternion.identity;
   }
   
   /// <summary>
   /// This helper method spawns the prefab at the given coordinates
   /// </summary>
   /// <param name="prefab"></param>
   /// <param name="position"></param>
   private void InstantiateProps(GameObject prefab, Vector2Int position, float height, DungeonRoomData roomData)
   {
      Vector2Int wallDirection = GetWallDirection(roomData, position);
      Quaternion wallRotation = GetWallRotation(wallDirection);

      Vector3 tilePosition = new Vector3(position.x * cellSize, height, position.y * cellSize);
      Vector3 wallOffset = new Vector3(wallDirection.x, 0, wallDirection.y) * distanceFromWall * cellSize;
      
      Vector3 prefabPosition = tilePosition + wallOffset;
      
      Instantiate(prefab, prefabPosition, wallRotation, propParent);
   }
   
   /// <summary>
   /// This method clears the props that sit inside the PropParent in the hierachy
   /// </summary>
   private void PropParentChildrenCleaner()
   {
      int noOfChildren = propParent.childCount;

      for (int i = noOfChildren - 1; i >= 0; i--)
      {
         DestroyImmediate(propParent.GetChild(i).gameObject);
      }
   }
   
   #endregion

   #region Room Data
   /// <summary>
   /// This will store the rooms data, that has its bounds, center etc to help provide info to place prefabs and enemies
   /// </summary>
   /// <param name="roomsList">Contains the room data that has been split with the algorithm for the dungeon</param>
   /// <returns>Returns back the data of the room</returns>
   private List<DungeonRoomData> CreateSimpleRoomData(List<BoundsInt> roomsList)
   {
      List<DungeonRoomData> generatedRooms = new List<DungeonRoomData>();

      foreach (var room in roomsList)
      {
         DungeonRoomData roomData = new DungeonRoomData();

         roomData.Bounds = room;
         roomData.CenterPoint = (Vector2Int)(Vector3Int.RoundToInt(room.center));

         for (int col = 0; col < room.size.x - offset; col++) 
         {
            for (int row = 0; row < room.size.y - offset; row++)
            {
               Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
               roomData.FloorTiles.Add(position);
            }
         }
         AnalyzeRoomTilesData(roomData);
         generatedRooms.Add(roomData);
      }
      
      return generatedRooms;
   }
   
   /// <summary>
   /// This method will find out the tilese are wall tiles, corner tiles, inner tiles and etc to give more data for the room
   /// for better prefab placement.
   /// </summary>
   /// <param name="roomData">This contains the data of the rooms made in the BSP</param>
   private void AnalyzeRoomTilesData(DungeonRoomData roomData)
   {
      foreach (Vector2Int tile in roomData.FloorTiles)
      {
         // This defines the checks needed for if there are neighbouring tiles
         bool hasUp = roomData.FloorTiles.Contains(tile + Vector2Int.up);
         bool hasDown = roomData.FloorTiles.Contains(tile + Vector2Int.down);
         bool hasLeft = roomData.FloorTiles.Contains(tile + Vector2Int.left);
         bool hasRight = roomData.FloorTiles.Contains(tile + Vector2Int.right);
         int missingNeighbourTileCount = 0;
         
         // This checks whether there are tiles nearby tile X
         if (!hasUp) missingNeighbourTileCount++;
         if (!hasDown) missingNeighbourTileCount++;
         if (!hasLeft) missingNeighbourTileCount++;
         if (!hasRight) missingNeighbourTileCount++;

         if (missingNeighbourTileCount >= 2)
         {
            roomData.CornerTiles.Add(tile);
         }
         else if (missingNeighbourTileCount == 1)
         {
            roomData.NearWallTiles.Add(tile);
         }
         else
         {
            roomData.InnerTiles.Add(tile);
         }
      }
   }
   
   /// <summary>
   /// This function analyzes the corridor tiles and adds it to the dungeon room data
   /// </summary>
   /// <param name="generatedRooms">contains all the generated rooms</param>
   /// <param name="corridors">contains the generated corridors</param>
   private void AssignCorridorDataToRoom(List<DungeonRoomData> generatedRooms, HashSet<Vector2Int> corridors)
   {
      // Loops through the generated rooms
      foreach (DungeonRoomData roomData in generatedRooms)
      {
         // Loops through the corridor tiles
         foreach (Vector2Int tile in corridors)
         {
            // Checks to see if the room data has the corridor tile inside, if it doesn't add it. 
            if (roomData.FloorTiles.Contains(tile))
            {
               roomData.CorridorTiles.Add(tile);
            }
         }
      }
   }
   #endregion
   
   #region Create Room Functions
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
   #endregion
   
   
}
