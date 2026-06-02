// This script was made to generate the rooms using ProceduralGenerationAlgorithm.cs to use binary space partionining to split the rooms
// Made by Andrew

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class RoomFirstGeneration : SimpleRandomWalkGenerator
{
   // This declares the boundary of how we want to generate those rooms in a certain size
   [Header("Dungeon Dimensions")]
   [SerializeField] private int minRoomWidth;
   [SerializeField] private int minRoomHeight;
   [SerializeField] private int dungeonWidth = 20;
   [SerializeField] private int dungeonHeight = 20;
   
   [Header("Dungeon Properties")]
   [SerializeField] [Min(1)] private int corridorWidth;
   [SerializeField] [Range(0, 10)] private int offset; // Offsets the rooms gen from the boundary box
   [SerializeField] private Transform propParent;
   bool randomWalkRooms = false; // Responsible to check if we want to use the random walk algorithm
   
   [Header("Prop Spawn List")] 
   [SerializeField] private RoomPropSet spawnRoomProps;
   [SerializeField] private RoomPropSet treasureRoomProps;
   [SerializeField] private RoomPropSet bossRoomProps;
   [SerializeField] private RoomPropSet normalRoomProps;
   
   [Header("Prop spawn dimensions")]
   [SerializeField] private float cellSize;
   [SerializeField] private float propHeight;
   [SerializeField] private float wallPropHeight;
   [SerializeField] private float distanceFromWall;
   [SerializeField] private float doorDistanceOffSet;

   [Header("Dungeon Items")] 
   [SerializeField] private GameObject doorPrefab;
   
   [Header("Dungeon Enemy List")]
   
   [Header("Player information")] 
   [SerializeField] private GameObject player;

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
      AssignEntranceTiles(generatedRooms, corridors);
      AssignRoomType(generatedRooms);
      
      // Spawns player
      PlayerSpawn(generatedRooms);
      
      floor.UnionWith(corridors);
      
      // Parse in the floor tiles to create the rooms and visually paint it
      visualiser.CreateFloorTiles(floor);
      
      // Spawns the door to connect the room and corridor and props
      SpawnDoors(generatedRooms);
      SpawnProps(generatedRooms);
   }
   
   private void PlayerSpawn(List<DungeonRoomData> roomData)
   {
      DungeonRoomData spawnRoom = null;
      
      if (player == null)
      {
         return;
      }
      
      CharacterController controller = player.GetComponent<CharacterController>();

      if (controller == null)
      {
         return;
      }

      if (roomData == null || roomData.Count == 0)
      {
         return;
      }

      foreach (DungeonRoomData room in roomData)
      {
         if (room.TypeOfRoom == RoomType.Spawn)
         {
            spawnRoom = room;
            break;
         }
      }

      if (spawnRoom == null)
      {
         return;
      }
      
      // Room positions
      Vector2Int roomCenter = spawnRoom.CenterPoint;
      Vector3 teleportPosition = new Vector3(roomCenter.x * cellSize, 0, roomCenter.y * cellSize);
      
      // Disables the player controller & teleports the player
      controller.enabled = false;
      player.transform.position = teleportPosition;
      
      // Enable it again
      controller.enabled = true;
      
      // Removed the spawn point from the tile list
      spawnRoom.OccupiedTiles.Add(roomCenter);
   }
   
   #region Prop spawning LOGIC
   /// <summary>
   /// This method will spawn the props at a random tile with a random prefab that is stored in lists for corner tiles, inner tiles and near wall tiles.
   /// </summary>
   /// <param name="generatedRooms">This stores all the given rooms of the dungoen</param>
   private void SpawnProps(List<DungeonRoomData> generatedRooms)
   {
      foreach (DungeonRoomData roomData in generatedRooms)
      {
         if (roomData.TypeOfRoom == RoomType.Spawn)
         {
            SpawnPrefabsForSpawn(roomData, spawnRoomProps);
         }
         else if (roomData.TypeOfRoom == RoomType.Treasure)
         {
            SpawnPrefabsForTreasureRoom(roomData, treasureRoomProps);
         }
         else if (roomData.TypeOfRoom == RoomType.Boss)
         {
            SpawnPrefabsForBossRoom(roomData, bossRoomProps);
         }
         else
         {
            SpawnPrefabsForNormalRoom(roomData, normalRoomProps);
         }
      }
   }

   private void SpawnDoors(List<DungeonRoomData> generatedRooms)
   {
      foreach (DungeonRoomData roomData in generatedRooms)
      {
         TrySpawnDoorPrefab(roomData, doorPrefab);
      }
   }

   private void SpawnPrefabsForSpawn(DungeonRoomData roomData, RoomPropSet propSet)
   {
      if (propSet == null)
      {
         return;
      }
      
      // Get valid list of tiles that prefabs can be spawned on
      List<Vector2Int> validCornerTiles = GetValidTile(roomData, roomData.CornerTiles);
      List<Vector2Int> validWallTiles = GetValidTile(roomData, roomData.NearWallTiles);
      
      // Instantiate the prefabs on the given tile
      TrySpawnPropAtTile(propSet.cornerProps, validCornerTiles, roomData, propSet.minCornerProps,  propSet.maxCornerProps);
      TrySpawnPropAtWall(propSet.nearWallTileProps, validWallTiles, roomData, wallPropHeight, propSet.minNearWallTileProps, propSet.maxNearWallTileProps);
   }

   private void SpawnPrefabsForTreasureRoom(DungeonRoomData roomData, RoomPropSet propSet)
   {
      if (propSet == null)
      {
         return;
      }
      
      // Get valid list of tiles that prefabs can be spawned on
      List<Vector2Int> validCornerTiles = GetValidTile(roomData, roomData.CornerTiles);
      List<Vector2Int> validInnerTiles =  GetValidTile(roomData, roomData.InnerTiles);
      List<Vector2Int> validWallTiles = GetValidTile(roomData, roomData.NearWallTiles);
      
      // Instantiate the prefabs on the given tile
      TrySpawnPropAtTile(propSet.cornerProps, validCornerTiles, roomData, propSet.minCornerProps, propSet.maxCornerProps);
      TrySpawnPropAtTile(propSet.innerTileProps, validInnerTiles, roomData, propSet.minInnerTileProps, propSet.maxInnerTileProps);
      TrySpawnPropAtWall(propSet.nearWallTileProps, validWallTiles, roomData, wallPropHeight,  propSet.minNearWallTileProps, propSet.maxNearWallTileProps);
   }

   private void SpawnPrefabsForBossRoom(DungeonRoomData roomData, RoomPropSet propSet)
   {
      if (propSet == null)
      {
         return;
      }
      
      List<Vector2Int> validInnerTiles =  GetValidTile(roomData, roomData.InnerTiles);
      TrySpawnPropAtTile(propSet.innerTileProps, validInnerTiles, roomData, propSet.minInnerTileProps, propSet.maxInnerTileProps);
   }

   private void SpawnPrefabsForNormalRoom(DungeonRoomData roomData, RoomPropSet propSet)
   {
      if (propSet == null)
      {
         return;
      }
      
      // Get valid list of tiles that prefabs can be spawned on
      List<Vector2Int> validCornerTiles = GetValidTile(roomData, roomData.CornerTiles);
      List<Vector2Int> validInnerTiles =  GetValidTile(roomData, roomData.InnerTiles);
      List<Vector2Int> validWallTiles = GetValidTile(roomData, roomData.NearWallTiles);
      
      // Instantiate the prefabs on the given tile
      TrySpawnPropAtTile(propSet.cornerProps, validCornerTiles, roomData, propSet.minCornerProps, propSet.maxCornerProps);
      TrySpawnPropAtTile(propSet.innerTileProps, validInnerTiles, roomData, propSet.minInnerTileProps, propSet.maxInnerTileProps);
      TrySpawnPropAtWall(propSet.nearWallTileProps, validWallTiles, roomData, wallPropHeight,propSet.minNearWallTileProps,propSet.maxNearWallTileProps);
   }

   private void TrySpawnDoorPrefab(DungeonRoomData roomData, GameObject prefab) 
   {
      if (prefab == null)
      {
         return;
      }

      if (roomData == null)
      {
         return;
      }
      
      List<EntranceRoomData> entranceToSpawn = AnalyzeDoorCandidateTiles(roomData);

      if (entranceToSpawn == null || entranceToSpawn.Count == 0)
      {
         return;
      }
      
      // Chooses a random tile from the corridor tiles to choose from
      int tileIndex = Random.Range(0, entranceToSpawn.Count);
      EntranceRoomData entrance = entranceToSpawn[tileIndex];
         
      // Gets the tile position and tile rotation
      Vector2Int wallDirection = entrance.Direction;
      Vector2Int sideDirection = GetDoorSideOffSetDirection(roomData, entrance);
      Quaternion doorRotation = GetWallRotation(wallDirection);
      
      Vector3 tilePosition = new Vector3(entrance.Tile.x * cellSize, propHeight, entrance.Tile.y * cellSize);
      Vector3 doorOffset = new Vector3(wallDirection.x, 0, wallDirection.y) * (cellSize * distanceFromWall);
      Vector3 sideDoorOffset = new Vector3(sideDirection.x, 0, sideDirection.y) * doorDistanceOffSet;
      
      Vector3 doorPosition = tilePosition + doorOffset + sideDoorOffset;
      
      // Instantiate
      Instantiate(prefab, doorPosition, doorRotation, propParent);
      roomData.OccupiedTiles.Add(entrance.Tile);
   }
   
   /// <summary>
   /// This method will try to spawn a random prefab at a random tile in the dungeon in the different rooms
   /// </summary>
   /// <param name="objectToSpawn">Prefab to spawn</param>
   /// <param name="givenTiles">The tiles that exist in the different dungeon rooms</param>
   /// <param name="roomData"></param>
   private void TrySpawnPropAtTile(List<GameObject> objectToSpawn,  List<Vector2Int> givenTiles, DungeonRoomData roomData, int minCount, int maxCount)
   {
      if (objectToSpawn == null || objectToSpawn.Count == 0)
      {
         return;
      }
      
      if (givenTiles == null || givenTiles.Count == 0)
      {
         return;
      }
      
      int maxSpawnCount = Random.Range(minCount, maxCount+1);

      for (int i = 0; i < maxSpawnCount; i++)
      {
         if (givenTiles.Count == 0)
         {
            return;
         }
         // Choosing from a random select of tiles
         int randomTileIndex = Random.Range(0, givenTiles.Count);
         Vector2Int tile = givenTiles[randomTileIndex];
         
         // Choosing from a random select of prefabs
         int randomPrefabIndex = Random.Range(0, objectToSpawn.Count);
         GameObject randomPrefab = objectToSpawn[randomPrefabIndex];

         // Get tile position to spawn prefab at
         Vector3 tilePosition = new Vector3(tile.x * cellSize, propHeight, tile.y * cellSize);
         Instantiate(randomPrefab, tilePosition, Quaternion.identity, propParent);
         
         // Adds the current tile to the occupied tiles
         roomData.OccupiedTiles.Add(tile);
         givenTiles.RemoveAt(randomTileIndex);
      }
   }

   private void TrySpawnPropAtWall(List<GameObject> objectToSpawn, List<Vector2Int> givenTiles, DungeonRoomData roomData, float spawnHeight, int minCount, int maxCount)
   {
      if (objectToSpawn == null || objectToSpawn.Count == 0)
      {
         return;
      }
      
      if (givenTiles == null || givenTiles.Count == 0)
      {
         return;
      }
      
      int maxSpawnCount = Random.Range(minCount, maxCount+1);

      for (int i = 0; i < maxSpawnCount; i++)
      {
         if (givenTiles.Count == 0)
         {
            return;
         }
         
         // Choosing from a random select of tiles
         int randomTileIndex = Random.Range(0, givenTiles.Count);
         Vector2Int tile = givenTiles[randomTileIndex];
         
         // Choosing from a random select of prefabs
         int randomPrefabIndex = Random.Range(0, objectToSpawn.Count);
         GameObject randomPrefab = objectToSpawn[randomPrefabIndex];
         
         Vector2Int wallDirection = GetWallDirection(roomData, tile);
         Quaternion wallRotation = GetWallRotation(wallDirection);

         Vector3 tilePosition = new Vector3(tile.x * cellSize, spawnHeight, tile.y * cellSize);
         Vector3 wallOffset = new Vector3(wallDirection.x, 0, wallDirection.y) * (distanceFromWall * cellSize);
         
         Vector3 prefabPosition = tilePosition + wallOffset;
         
         Instantiate(randomPrefab, prefabPosition, wallRotation, propParent);
         
         // Adds the current tile to the occupied tiles
         roomData.OccupiedTiles.Add(tile);
         givenTiles.RemoveAt(randomTileIndex);
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
   /// This method will check if the door needs to move to the left or right depending
   /// on which tile its instantiating on to make it aligned in the middle of both tiles
   /// </summary>
   /// <param name="roomData">Generated rooms</param>
   /// <param name="tile">Current tile to check</param>
   /// <returns>Returns the positional vector to move left or right or down or up</returns>
   private Vector2Int GetDoorSideOffSetDirection(DungeonRoomData roomData, EntranceRoomData tile)
   {
      Vector2Int sideDirection = GetSideDirection(tile.Direction);
      
      Vector2Int tileToRight = tile.Tile + sideDirection;
      Vector2Int tileToLeft = tile.Tile - sideDirection;
      
      bool hasEntranceTile = CheckForEntranceTile(roomData, tileToRight, tile.Direction);
      bool hasOppositeEntranceTile =  CheckForEntranceTile(roomData, tileToLeft, tile.Direction);
      
      if (hasEntranceTile)
      {
         return sideDirection;
      }

      if (hasOppositeEntranceTile)
      {
         return -sideDirection;
      }
      
      return Vector2Int.zero;
   }

   /// <summary>
   /// Gets the perpendicular direction of the tile to see if the door prefab
   /// needs to move to the right or left for its offset
   /// </summary>
   /// <param name="direction">Up, Down, Left or Right</param>
   private Vector2Int GetSideDirection(Vector2Int direction)
   {
      if (direction == Vector2Int.up || direction == Vector2Int.down)
      {
         return Vector2Int.right;
      }

      if (direction == Vector2Int.left || direction == Vector2Int.right)
      {
         return Vector2Int.up;
      }
      
      return Vector2Int.zero;
   }

   private bool CheckForEntranceTile(DungeonRoomData roomData, Vector2Int tile, Vector2Int direction)
   {
      foreach (EntranceRoomData room in roomData.EntranceTiles)
      {
         if (room.Tile == tile && room.Direction == direction)
         {
            return true;
         }
      }

      return false;
   }
   
   /// <summary>
   /// This method clears the props that sit inside the PropParent in the hierachy
   /// </summary>
   private void PropParentChildrenCleaner()
   {
      if (propParent == null)
      {
         return;
      }
      
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
   
   /// <summary>
   /// This will give a room its assigned type, such as spawn point, boss room etc.
   /// </summary>
   /// <param name="generatedRooms">List of generated rooms in the level</param>
   private void AssignRoomType(List<DungeonRoomData> generatedRooms)
   {
      if (generatedRooms == null || generatedRooms.Count == 0)
      {
         return;
      }
      
      // Assigns the room to be the spawn room
      DungeonRoomData spawnRoom = FindClosestRoomToStart(generatedRooms);

      if (spawnRoom != null)
      {
         spawnRoom.TypeOfRoom = RoomType.Spawn;
      }
      
      // Assigns the room to be the boss room
      DungeonRoomData bossRoom = FindFarthestRoomFromStart(generatedRooms);

      if (bossRoom != null && bossRoom != spawnRoom)
      {
         bossRoom.TypeOfRoom = RoomType.Boss;
      }
      
      // Assigns the room to be a treasure room
      DungeonRoomData treasureRoom = FindRandomNormalRoom(generatedRooms);

      if (treasureRoom != null)
      {
         treasureRoom.TypeOfRoom = RoomType.Treasure;
      }
   }

   /// <summary>
   /// This function assigns the tile that connects to the room and corridor
   /// </summary>
   /// <param name="generatedRooms">List of all the generated rooms</param>
   /// <param name="corridors">Hash set of the corridors that links the rooms</param>
   private void AssignEntranceTiles(List<DungeonRoomData> generatedRooms, HashSet<Vector2Int> corridors)
   {
      foreach (DungeonRoomData roomData in generatedRooms)
      {
         foreach (Vector2Int tile in roomData.FloorTiles)
         {
            CheckForEntranceDirection(roomData, corridors, tile, Vector2Int.up);
            CheckForEntranceDirection(roomData, corridors, tile, Vector2Int.down);
            CheckForEntranceDirection(roomData, corridors, tile, Vector2Int.left);
            CheckForEntranceDirection(roomData, corridors, tile, Vector2Int.right);
         }
      }
   }

   private void CheckForEntranceDirection(DungeonRoomData roomData, HashSet<Vector2Int> corridors ,Vector2Int tile, Vector2Int direction)
   {
      // Points to the corresponding tile
      Vector2Int neighbourTile = tile + direction;

      bool isNeighbourTileCorridor = corridors.Contains(neighbourTile);
      bool isNeighbourTileRoomTile = roomData.FloorTiles.Contains(neighbourTile);

      if (isNeighbourTileCorridor && !isNeighbourTileRoomTile)
      {
         roomData.EntranceTiles.Add(new EntranceRoomData(tile, direction));
      }
   }

   private List<EntranceRoomData> AnalyzeDoorCandidateTiles(DungeonRoomData roomData)
   {
      List<EntranceRoomData> candidateDoorTiles = new List<EntranceRoomData>();

      foreach (EntranceRoomData entranceTile in roomData.EntranceTiles)
      {
         if (!roomData.OccupiedTiles.Contains(entranceTile.Tile))
         {
            candidateDoorTiles.Add(entranceTile);
         }
      }
      
      return candidateDoorTiles;
   } 
   
   private DungeonRoomData FindClosestRoomToStart(List<DungeonRoomData> generatedRooms)
   {
      DungeonRoomData closestRoom = null;
      float closestDistance = float.MaxValue;

      foreach (DungeonRoomData room in generatedRooms)
      {
         float distance = Vector2Int.Distance(startPosition, room.CenterPoint);

         if (distance < closestDistance)
         {
            closestDistance = distance;
            closestRoom = room;
         }
      }
      
      return closestRoom;
   }
   
   private DungeonRoomData FindFarthestRoomFromStart(List<DungeonRoomData> generatedRooms)
   {
      DungeonRoomData farthestRoom = null;
      float farthestDistance = float.MinValue;

      foreach (DungeonRoomData room in generatedRooms)
      {
         float distance = Vector2Int.Distance(startPosition, room.CenterPoint);

         if (distance > farthestDistance)
         {
            farthestDistance = distance;
            farthestRoom = room;
         }
      }

      return farthestRoom;
   }

   private DungeonRoomData FindRandomNormalRoom(List<DungeonRoomData> generatedRooms)
   {
      List<DungeonRoomData> normalRoom = new List<DungeonRoomData>();

      foreach (DungeonRoomData room in generatedRooms)
      {
         if (room.TypeOfRoom == RoomType.Normal)
         {
            normalRoom.Add(room);
         }
      }

      if (normalRoom.Count == 0)
      {
         return null;
      }
      
      int randomRoomIndex = Random.Range(0, normalRoom.Count);
      
      return normalRoom[randomRoomIndex];
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
