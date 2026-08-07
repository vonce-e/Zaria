// Generates a room-first dungeon with BSP rooms, corridors, room types, internal sections, doors, and props.
// Written by Andrew Burke.

using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.WSA;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class RoomFirstGeneration : SimpleRandomWalkGenerator
{
    // This declares the boundary of how we want to generate those rooms in a certain size
    [Header("Dungeon Dimensions")]
    [SerializeField]
    private int minRoomWidth;

    [SerializeField] private int minRoomHeight;
    [SerializeField] private int dungeonWidth = 20;
    [SerializeField] private int dungeonHeight = 20;

    [Header("Dungeon Properties")]
    [SerializeField]
    [Min(1)]
    private int corridorWidth;

    [SerializeField][Range(0, 10)] private int offset; // Offsets the rooms gen from the boundary box
    [SerializeField] private Transform propParent;
    bool randomWalkRooms = false; // Responsible to check if we want to use the random walk algorithm

    [Header("Room spawn rules")]
    [SerializeField]
    List<RoomTypeSpawnRule> roomTypeSpawnRules;

    [Header("Prop Spawn List")]
    [SerializeField] private List<RoomPropRule> roomProps;
    [SerializeField] private CorridorPropRule corridorPropRule;

    [Header("Prop spawn dimensions")]
    [SerializeField]
    private float cellSize;

    [SerializeField] private float propHeight;
    [SerializeField] private float wallPropHeight;
    [SerializeField] private float ceilingPropHeight;
    [SerializeField] private float distanceFromWall;
    [SerializeField] private float doorDistanceOffSet;

    [Header("Dungeon Items")]
    [SerializeField] private GameObject doorPrefab;

    [SerializeField] private GameObject bossPortalPrefab;

    [Header("Player information")]
    [SerializeField]
    private GameObject player;

    /// <summary>
    /// Generates a dungeon automatically when the scene starts.
    /// </summary>
    private void Start()
    {
        GenerateDungeon();
    }

    /// <summary>
    /// Runs the room-first generation pipeline.
    /// </summary>
    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    /// <summary>
    /// Generates and validates rooms, connects them with corridors, visualises the dungeon, and spawns its contents.
    /// </summary>
    private void CreateRooms()
    {

        int maxAttempts = 20;
        int currentAttempt = 0;
        bool validDungeonGenerated = false;

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        List<DungeonRoomData> generatedRooms = new List<DungeonRoomData>();
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        Dictionary<RoomType, int> wantedRooms = new Dictionary<RoomType, int>();

        while (currentAttempt < maxAttempts && validDungeonGenerated == false)
        {
            currentAttempt++;

            Debug.Log("Dungeon Generation Attempt: " + currentAttempt);

            floor = new HashSet<Vector2Int>();
            generatedRooms = new List<DungeonRoomData>();
            corridors = new HashSet<Vector2Int>();
            wantedRooms = new Dictionary<RoomType, int>();

            var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(new BoundsInt((Vector3Int)(startPosition),
               new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

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

            corridors = ConnectRooms(roomCenters);

            // Assign the corridors to the room data
            AssignCorridorDataToRoom(generatedRooms, corridors);
            AssignEntranceTiles(generatedRooms, corridors);
            AssignRoomType(generatedRooms, wantedRooms);
            // DebugRoomTypeSummary(generatedRooms);

            if (HasRequiredRoomSizes(generatedRooms, wantedRooms))
            {
                validDungeonGenerated = true;
            }
        }

        CreateInternalRoomSections(generatedRooms);

        // Spawns player
        PlayerSpawn(generatedRooms);

        floor.UnionWith(corridors);

        // Parse in the floor tiles to create the rooms and visually paint it
        visualiser.CreateTiles(floor, generatedRooms);

        // Spawns the door to connect the room and corridor and props
        SpawnDoors(generatedRooms);
        SpawnProps(generatedRooms);
        SpawnCorridorProps(corridors, floor, generatedRooms);
    }

    /// <summary>
    /// Moves the player to the centre of the generated spawn room and reserves that tile.
    /// </summary>
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
        if (roomProps == null || roomProps.Count == 0)
        {
            return;
        }

        foreach (DungeonRoomData roomData in generatedRooms)
        {
            RoomPropRule propRule = null;

            foreach (RoomPropRule rule in roomProps)
            {
                if (rule.roomType == roomData.TypeOfRoom)
                {
                    propRule = rule;
                    break;
                }
            }

            if (propRule == null)
            {
                continue;
            }

            // Default room layout
            RoomLayoutPattern roomLayoutPattern = null;

            // Assigns a room layout
            if (propRule.layoutPatterns != null && propRule.layoutPatterns.Count > 0)
            {
                int randomIndex = Random.Range(0, propRule.layoutPatterns.Count);
                roomLayoutPattern = propRule.layoutPatterns[randomIndex];
            }

            // Check for valid tiles before spawning on category
            List<Vector2Int> validNearWallTiles = GetValidTile(roomData, roomData.NearWallTiles, true);
            validNearWallTiles = LayoutOptionDecider(roomData, roomLayoutPattern, validNearWallTiles);
            SpawnPropCategoryNearWall(propRule.nearWallTileProps, validNearWallTiles, roomData, propHeight);

            List<Vector2Int> validInnerTiles = GetValidTile(roomData, roomData.InnerTiles);
            validInnerTiles = LayoutOptionDecider(roomData, roomLayoutPattern, validInnerTiles);
            SpawnPropCategoryOnTile(propRule.innerTileProps, validInnerTiles, roomData, propHeight);

            List<Vector2Int> validCornerTiles = GetValidTile(roomData, roomData.CornerTiles, false);
            validCornerTiles = LayoutOptionDecider(roomData, roomLayoutPattern, validCornerTiles);
            SpawnPropCategoryOnTile(propRule.cornerProps, validCornerTiles, roomData, propHeight);

            List<Vector2Int> validWallMountedTiles = GetValidTile(roomData, roomData.NearWallTiles, false);
            SpawnPropCategoryOnWall(propRule.wallMountedProps, validWallMountedTiles, roomData, wallPropHeight);

            List<Vector2Int> validCeilingTiles = GetValidTile(roomData, roomData.CeilingTiles);
            SpawnCategoryOnCeiling(propRule.ceilingProps, validCeilingTiles, roomData, ceilingPropHeight);
        }
    }

    /// <summary>
    /// Filters corridor-only tiles and spawns corridor props without placing them inside rooms.
    /// </summary>
    private void SpawnCorridorProps(HashSet<Vector2Int> corridor, HashSet<Vector2Int> floor, List<DungeonRoomData> roomData)
    {
        List<Vector2Int> validCorridorTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in corridor)
        {

            bool tileIsInRoom = false;

            foreach (DungeonRoomData room in roomData)
            {
                if (room.FloorTiles.Contains(tile))
                {
                    tileIsInRoom = true;
                    break;
                }
            }

            if (tileIsInRoom)
            {
                continue;
            }

            validCorridorTiles.Add(tile);
        }

        HashSet<Vector2Int> occupiedCorridorTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>(floor);

        // spawn the corridor props
        SpawnPropCategoryInCorridor(corridorPropRule.wallMountedProps, validCorridorTiles, occupiedCorridorTiles, floorTiles, wallPropHeight);
    }

    /// <summary>
    /// Spawns floor props on available tiles while respecting configured amounts and occupied-tile spacing.
    /// </summary>
    private void SpawnPropCategoryOnTile(PropCategorySpawnRule spawnRule, List<Vector2Int> validTiles,
       DungeonRoomData roomData, float spawnHeight)
    {
        if (spawnRule == null || spawnRule.prefabs == null || spawnRule.prefabs.Count == 0)
        {
            return;
        }

        if (validTiles == null || validTiles.Count == 0)
        {
            return;
        }

        foreach (PropSpawnRule propData in spawnRule.prefabs)
        {
            if (propData == null || propData.prefab == null)
            {
                return;
            }

            int propAmount = PropsToSpawn(propData);
            int spawnedAmt = 0;

            while (spawnedAmt < propAmount && validTiles.Count > 0)
            {
                if (validTiles.Count == 0)
                {
                    return;
                }

                // Choosing from a random select of tiles
                int randomTileIndex = Random.Range(0, validTiles.Count);
                Vector2Int tile = validTiles[randomTileIndex];

                // if (IsPropNearAnother(roomData.OccupiedTiles, tile, 1))
                // {
                //     validTiles.RemoveAt(randomTileIndex);
                //     continue;
                // }

                if (!TryGetPropFootPrint(propData, tile, roomData, out List<Vector2Int> footprintTiles, false))
                {
                    validTiles.RemoveAt(randomTileIndex);
                    continue;
                }

                GameObject prefab = propData.prefab;

                // Get tile position to spawn prefab at
                Vector3 tilePosition = new Vector3(tile.x * cellSize, spawnHeight, tile.y * cellSize);
                Instantiate(prefab, tilePosition, Quaternion.identity, propParent);

                // Adds the current tile to the occupied tiles
                foreach (Vector2Int footprintTile in footprintTiles)
                {
                    roomData.OccupiedTiles.Add(footprintTile);
                    validTiles.Remove(footprintTile);
                }

                spawnedAmt++;
            }
        }
    }

    /// <summary>
    /// Spawns floor props near room walls and rotates them to face the detected wall direction.
    /// </summary>
    private void SpawnPropCategoryNearWall(PropCategorySpawnRule spawnRule, List<Vector2Int> validTiles,
       DungeonRoomData roomData, float spawnHeight)
    {
        if (spawnRule == null || spawnRule.prefabs == null || spawnRule.prefabs.Count == 0)
        {
            return;
        }

        if (validTiles == null)
        {
            return;
        }

        foreach (PropSpawnRule propData in spawnRule.prefabs)
        {
            if (propData == null || propData.prefab == null)
            {
                continue;
            }

            int propAmount = PropsToSpawn(propData);
            int spawnedAmt = 0;

            int numberOfPasses = propData.mustSpawn ? 2 : 1;

            for (int pass = 0; pass < numberOfPasses && spawnedAmt < propAmount; pass++)
            {
                bool allowCorridorOverlap = pass == 1;

                List<Vector2Int> candidateTiles = new List<Vector2Int>(validTiles);

                while (spawnedAmt < propAmount && candidateTiles.Count > 0)
                {
                    // Choose a cnadidate from the list
                    int randomTileIndex = Random.Range(0, candidateTiles.Count);

                    Vector2Int tile = candidateTiles[randomTileIndex];

                    Vector2Int wallDirection = GetWallDirection(roomData.FloorTiles, tile);

                    if (wallDirection == Vector2Int.zero)
                    {
                        candidateTiles.RemoveAt(randomTileIndex);
                        continue;
                    }

                    Vector2Int footprintcenter = GetNearWallFootprintCenter(tile,
                    wallDirection,
                    propData);

                    if (!TryGetPropFootPrint(propData,
                    footprintcenter,
                    roomData,
                    out List<Vector2Int> footprintTiles,
                    allowCorridorOverlap))
                    {
                        candidateTiles.RemoveAt(randomTileIndex);
                        continue;
                    }
                    
                    // Choosing from a random select of prefabs
                    GameObject prefab = propData.prefab;

                    Quaternion wallRotation = GetWallRotation(wallDirection) * Quaternion.Euler(0f, 90f, 0f);

                    Vector3 tilePosition = new Vector3(tile.x * cellSize, spawnHeight, tile.y * cellSize);

                    Instantiate(prefab, tilePosition, wallRotation, propParent);

                    // Adds the current tile to the occupied tiles
                    foreach (Vector2Int footprintTile in footprintTiles)
                    {
                        roomData.OccupiedTiles.Add(footprintTile);
                        validTiles.Remove(footprintTile);
                        candidateTiles.Remove(footprintTile);
                    }

                    spawnedAmt++;
                    }   
            }
        }
    }

    /// <summary>
    /// Spawns wall-mounted props with the position offset and rotation of the nearest wall.
    /// </summary>
    private void SpawnPropCategoryOnWall(PropCategorySpawnRule spawnRule, List<Vector2Int> validTiles,
       DungeonRoomData roomData, float spawnHeight)
    {
        if (spawnRule == null || spawnRule.prefabs == null || spawnRule.prefabs.Count == 0)
        {
            return;
        }

        if (validTiles == null || validTiles.Count == 0)
        {
            return;
        }

        foreach (PropSpawnRule propData in spawnRule.prefabs)
        {
            if (propData == null || propData.prefab == null)
            {
                return;
            }

            int propAmount = PropsToSpawn(propData);
            int spawnedAmt = 0;

            while (spawnedAmt < propAmount && validTiles.Count > 0)
            {
                if (validTiles.Count == 0)
                {
                    return;
                }

                // Choosing from a random select of tiles
                int randomTileIndex = Random.Range(0, validTiles.Count);
                Vector2Int tile = validTiles[randomTileIndex];

                if (IsPropNearAnother(roomData.OccupiedTiles, tile, 1))
                {
                    validTiles.RemoveAt(randomTileIndex);
                    continue;
                }

                // Choosing from a random select of prefabs
                GameObject prefab = propData.prefab;

                Vector2Int wallDirection = GetWallDirection(roomData.FloorTiles, tile);
                Quaternion wallRotation = GetWallRotation(wallDirection);

                Vector3 tilePosition = new Vector3(tile.x * cellSize, spawnHeight, tile.y * cellSize);
                Vector3 wallOffset = new Vector3(wallDirection.x, 0, wallDirection.y) * (distanceFromWall * cellSize);

                Vector3 prefabPosition = tilePosition + wallOffset;

                Instantiate(prefab, prefabPosition, wallRotation, propParent);

                // Adds the current tile to the occupied tiles
                roomData.OccupiedTiles.Add(tile);
                validTiles.RemoveAt(randomTileIndex);

                spawnedAmt++;
            }
        }
    }

    /// <summary>
    /// Spawns ceiling props on available tiles, prioritising positions nearest the room centre.
    /// </summary>
    private void SpawnCategoryOnCeiling(PropCategorySpawnRule spawnRule, List<Vector2Int> validTiles,
       DungeonRoomData roomData, float spawnHeight)
    {
        if (spawnRule == null || spawnRule.prefabs == null || spawnRule.prefabs.Count == 0)
        {
            return;
        }

        if (validTiles == null || validTiles.Count == 0)
        {
            return;
        }

        foreach (PropSpawnRule propData in spawnRule.prefabs)
        {
            if (propData == null || propData.prefab == null)
            {
                return;
            }

            int propAmount = PropsToSpawn(propData);

            for (int i = 0; i < propAmount; i++)
            {
                if (validTiles.Count == 0)
                {
                    return;
                }

                // Find the tile closest to the room center
                int closestTileIndex = 0;
                float closestDistance = float.MaxValue;

                for (int tileIndex = 0; tileIndex < validTiles.Count; tileIndex++)
                {
                    Vector2Int currentTile = validTiles[tileIndex];

                    float distanceFromCenter = Vector2Int.Distance(currentTile, roomData.CenterPoint);

                    if (distanceFromCenter < closestDistance)
                    {
                        closestDistance = distanceFromCenter;
                        closestTileIndex = tileIndex;
                    }
                }

                Vector2Int tile = validTiles[closestTileIndex];

                // Choosing from a random select of prefabs
                GameObject prefab = propData.prefab;

                // Get a position to spawn at
                Vector3 tilePosition = new Vector3(tile.x * cellSize, spawnHeight, tile.y * cellSize);
                Instantiate(prefab, tilePosition, Quaternion.identity, propParent);

                // Adds the current tile to the occupied tiles
                roomData.OccupiedTiles.Add(tile);
                validTiles.RemoveAt(closestTileIndex);
            }
        }
    }

    /// <summary>
    /// Spawns wall-mounted props along corridor tiles while enforcing corridor-specific spacing.
    /// </summary>
    private void SpawnPropCategoryInCorridor(PropCategorySpawnRule spawnRule, List<Vector2Int> validTiles,
       HashSet<Vector2Int> occupiedTiles, HashSet<Vector2Int> floorTiles, float spawnHeight)
    {
        if (spawnRule == null || spawnRule.prefabs == null || spawnRule.prefabs.Count == 0)
        {
            return;
        }

        if (validTiles == null || validTiles.Count == 0)
        {
            return;
        }

        foreach (PropSpawnRule propData in spawnRule.prefabs)
        {
            if (propData == null || propData.prefab == null)
            {
                return;
            }

            int propAmount = PropsToSpawn(propData);
            int spawnedAmt = 0;

            while (spawnedAmt < propAmount && validTiles.Count > 0)
            {
                if (validTiles.Count == 0)
                {
                    return;
                }

                int randomTileIndex = Random.Range(0, validTiles.Count);
                Vector2Int tile = validTiles[randomTileIndex];

                if (IsPropNearAnother(occupiedTiles, tile, 4))
                {
                    validTiles.RemoveAt(randomTileIndex);
                    continue;
                }

                // Choose a prefab from the spawn rule
                GameObject prefab = propData.prefab;

                Vector2Int direction = GetWallDirection(floorTiles, tile);

                if (direction == Vector2Int.zero)
                {
                    validTiles.RemoveAt(randomTileIndex);
                    continue;
                }

                Quaternion wallRotation = GetWallRotation(direction);

                Vector3 tilePosition = new Vector3(tile.x * cellSize, spawnHeight, tile.y * cellSize);
                Vector3 wallOffset = new Vector3(direction.x, 0, direction.y) * (distanceFromWall * cellSize);

                Vector3 prefabPosition = tilePosition + wallOffset;

                Instantiate(prefab, prefabPosition, wallRotation, propParent);

                // Add tiles to the occupied tiles
                occupiedTiles.Add(tile);
                validTiles.RemoveAt(randomTileIndex);

                spawnedAmt++;
            }
        }
    }


    /// <summary>
    /// Calculates a prop count from the rule's low, medium, and high probability thresholds.
    /// </summary>
    private int PropsToSpawn(PropSpawnRule spawnRule)
    {
        int propChance = Random.Range(0, 100);
        int propAmount = 0;

        if (propChance <= spawnRule.mediumThreshold)
        {
            propAmount = spawnRule.lowAmount;
        }
        else if (propChance <= spawnRule.highThreshold)
        {
            propAmount = Random.Range(spawnRule.lowAmount, spawnRule.mediumAmount + 1);
        }
        else
        {
            propAmount = Random.Range(spawnRule.mediumAmount, spawnRule.highAmount + 1);
        }

        if (spawnRule.mustSpawn && propAmount < 1)
        {
            propAmount = 1;
        }

        return propAmount;
    }

    /// <summary>
    /// Attempts to spawn doorway prefabs at the entrances of every generated room.
    /// </summary>
    private void SpawnDoors(List<DungeonRoomData> generatedRooms)
    {
        HashSet<Vector2Int> spawnedDoorTiles = new HashSet<Vector2Int>();

        foreach (DungeonRoomData roomData in generatedRooms)
        {
            TrySpawnDoorPrefab(roomData, doorPrefab, spawnedDoorTiles);
        }
    }

    /// <summary>
    /// Groups valid entrance tiles and spawns a correctly positioned and rotated door for each usable group.
    /// </summary>
    private void TrySpawnDoorPrefab(DungeonRoomData roomData, GameObject prefab, HashSet<Vector2Int> spawnedDoorTiles)
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

        List<List<EntranceRoomData>> entranceGroups = GroupEntranceTiles(entranceToSpawn);

        foreach (List<EntranceRoomData> entranceGroup in entranceGroups)
        {
            if (entranceGroup.Count <= 1)
            {
                continue;
            }

            if (entranceGroup.Count > 2)
            {
                break;
            }

            bool doorIsTooClose = false;

            foreach (EntranceRoomData entranceTile in entranceGroup)
            {
                if (IsPropNearAnother(spawnedDoorTiles, entranceTile.Tile, 1))
                    {
                        doorIsTooClose = true;
                        break;
                    }
            }

            if (doorIsTooClose)
            {
                continue;
            }

            EntranceRoomData firstEntrance = entranceGroup[0];

            Vector3 doorPosition = GetGroupDoorPosition(entranceGroup);
            Quaternion doorRotation = GetWallRotation(firstEntrance.Direction);

            Instantiate(prefab, doorPosition, doorRotation, propParent);

            foreach (EntranceRoomData entranceTile in entranceGroup)
            {
                roomData.OccupiedTiles.Add(entranceTile.Tile);
                spawnedDoorTiles.Add(entranceTile.Tile);
            }
        }
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

    #region PropSpawning Helper Methods
    // <--------- Tile validity & Direction checkers --------->

    /// <summary>
    /// This method will check if the given tiles are not a occupied or corridor tile
    /// </summary>
    /// <param name="data"></param>
    /// <param name="givenTiles"></param>
    /// <returns></returns>
    private List<Vector2Int> GetValidTile(DungeonRoomData data, HashSet<Vector2Int> givenTiles, bool includeCorridorTiles = false)
    {
        List<Vector2Int> validTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in givenTiles)
        {
            bool isOccupied = data.OccupiedTiles.Contains(tile);
            bool corridorAllowed = includeCorridorTiles || !data.CorridorTiles.Contains(tile);

            if (!isOccupied && corridorAllowed)
            {
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    /// <summary>
    /// Checks the tiles that a prefab takes up
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="centerTile"></param>
    /// <param name="roomData"></param>
    /// <returns></returns>
    private bool TryGetPropFootPrint(PropSpawnRule propData, 
    Vector2Int centerTile, 
    DungeonRoomData roomData, 
    out List<Vector2Int> footprintTiles,
    bool allowCorridorOverlap)
    {
        footprintTiles = new List<Vector2Int>();
        
        // Checks if it has even proportions, returns false if it is
        if (propData.footprintWidth % 2 == 0 || propData.footprintDepth % 2 == 0)
        {
            return false;
        }

        int halfWidth = propData.footprintWidth / 2;
        int halfDepth = propData.footprintDepth / 2;

        // Checks against the room walls to avoid spawning props that spawn near each other or against walls
        for (int x = -halfWidth; x <= halfWidth; x++)
        {
            for (int y = -halfDepth; y <= halfDepth; y++)
            {
                Vector2Int footprintTile = centerTile + new Vector2Int(x, y);

                if (!roomData.FloorTiles.Contains(footprintTile))
                {
                    return false;
                }

                if (!allowCorridorOverlap && roomData.CorridorTiles.Contains(footprintTile))
                {
                    return false;
                }

                if (roomData.OccupiedTiles.Contains(footprintTile))
                {
                    return false;
                }

                footprintTiles.Add(footprintTile);
            }
        }

        // Validates against inner walls to avoid prefabs that phase through walls
        foreach(InnerWallData innerWall in roomData.InnerWalls)
        {
            Vector2Int firstSide = innerWall.Tile;
            Vector2Int secondSide = innerWall.Tile + innerWall.Direction;

            bool coversFirstSide = footprintTiles.Contains(firstSide);
            bool coversSecondSide = footprintTiles.Contains(secondSide);
            
            // Checks to see if the prefab phases through the wall
            if (coversFirstSide && coversSecondSide)
            {
                return false;
            }

            if (propData.wallClearance <= 0)
            {
                continue;
            }

            foreach (Vector2Int footprintTile in footprintTiles)
            {
                int firstSideDistance = Mathf.Max(
                    Mathf.Abs(footprintTile.x - firstSide.x),
                    Mathf.Abs(footprintTile.y - firstSide.y));

                int secondSideDistance = Mathf.Max(
                    Mathf.Abs(footprintTile.x - secondSide.x),
                    Mathf.Abs(footprintTile.y - secondSide.y));

                if (firstSideDistance < propData.wallClearance || secondSideDistance < propData.wallClearance)
                    {
                        return false;
                    }
            }

        }

        List<Vector2Int> doorwayTiles = new List<Vector2Int>();

        // Adds every entrance tile into a list
        foreach(EntranceRoomData entrance in roomData.EntranceTiles)
        {
            doorwayTiles.Add(entrance.Tile);    
        }

        // Adds all the doorway tiles into the list
        foreach (InnerDoorWayData innerDoor in roomData.InnerDoors)
        {
            foreach (Vector2Int doorTile in innerDoor.Tiles)
            {
                doorwayTiles.Add(doorTile);
            }
        }

        foreach (Vector2Int footprintTile in footprintTiles)
        {
            foreach (Vector2Int doorwayTile in doorwayTiles)
            {
                int distance = Mathf.Max(
                    Mathf.Abs(footprintTile.x - doorwayTile.x),
                    Mathf.Abs(footprintTile.y - doorwayTile.y));

                if (distance <= propData.doorWayClearance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Vector2Int GetNearWallFootprintCenter(Vector2Int tile, Vector2Int wallDirection, PropSpawnRule propData)
    {
        int footprintOffset;

        if (wallDirection == Vector2Int.up ||
            wallDirection == Vector2Int.down)
        {
            footprintOffset = propData.footprintDepth / 2;
        }
        else
        {
            footprintOffset = propData.footprintWidth / 2;
        }

        return tile - wallDirection * footprintOffset;
    }

    /// <summary>
    /// Filters valid prop tiles according to the selected room layout and its minimum spacing.
    /// </summary>
    private List<Vector2Int> LayoutOptionDecider(DungeonRoomData roomData, RoomLayoutPattern layoutOption, List<Vector2Int> validTiles)
    {
        List<Vector2Int> layoutTileCandidates = new List<Vector2Int>();

        if (layoutOption == null || layoutOption.layoutOption == RoomLayoutOption.Random)
        {
            return new List<Vector2Int>(validTiles);
        }

        foreach (Vector2Int tile in validTiles)
        {
            float distanceFromCenter = Vector2Int.Distance(tile, roomData.CenterPoint);

            bool tilesMatchLayout = false;

            if (layoutOption.layoutOption == RoomLayoutOption.Perimeter)
            {
                if (roomData.NearWallTiles.Contains(tile) || roomData.CornerTiles.Contains(tile))
                {
                    tilesMatchLayout = true;
                }
            }

            else if (layoutOption.layoutOption == RoomLayoutOption.OpenCenter)
            {
                if (distanceFromCenter > layoutOption.centerRadius)
                {
                    tilesMatchLayout = true;
                }
            }

            else if (layoutOption.layoutOption == RoomLayoutOption.CenterFocused)
            {
                if (distanceFromCenter <= layoutOption.centerRadius)
                {
                    tilesMatchLayout = true;
                }
            }

            if (tilesMatchLayout == true)
            {
                layoutTileCandidates.Add(tile);
            }
        }

        List<Vector2Int> selectedTiles = new List<Vector2Int>();

        foreach (Vector2Int candidateTile in layoutTileCandidates)
        {
            bool isCandidateTooClose = false;

            foreach (Vector2Int selectedTile in selectedTiles)
            {
                float distance = Vector2Int.Distance(candidateTile, selectedTile);

                if (distance < layoutOption.minimumSpacing)
                {
                    isCandidateTooClose = true;
                    break;
                }
            }

            if (isCandidateTooClose == false)
            {
                selectedTiles.Add(candidateTile);
            }
        }

        return selectedTiles;
    }

    /// <summary>
    /// Returns the cardinal direction of the first missing floor neighbour around a tile.
    /// </summary>
    private Vector2Int GetWallDirection(HashSet<Vector2Int> floorTiles, Vector2Int tile)
    {
        if (!floorTiles.Contains(tile + Vector2Int.up))
        {
            return Vector2Int.up;
        }

        if (!floorTiles.Contains(tile + Vector2Int.left))
        {
            return Vector2Int.left;
        }

        if (!floorTiles.Contains(tile + Vector2Int.down))
        {
            return Vector2Int.down;
        }

        if (!floorTiles.Contains(tile + Vector2Int.right))
        {
            return Vector2Int.right;
        }
        return Vector2Int.zero;
    }

    /// <summary>
    /// Converts a cardinal wall direction into the corresponding prefab rotation.
    /// </summary>
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

    /// <summary>
    /// Groups adjacent entrance tiles that face the same direction into individual doorway openings.
    /// </summary>
    private List<List<EntranceRoomData>> GroupEntranceTiles(List<EntranceRoomData> entranceTiles)
    {
        List<List<EntranceRoomData>> entranceGroup = new List<List<EntranceRoomData>>();
        List<EntranceRoomData> uncheckedGroup = new List<EntranceRoomData>(entranceTiles);

        while (uncheckedGroup.Count > 0)
        {
            EntranceRoomData firstEntranceTile = uncheckedGroup[0];
            uncheckedGroup.RemoveAt(0);

            List<EntranceRoomData> currentEntranceTiles = new List<EntranceRoomData>();
            Queue<EntranceRoomData> entrancesToCheck = new Queue<EntranceRoomData>();

            entrancesToCheck.Enqueue(firstEntranceTile);
            currentEntranceTiles.Add(firstEntranceTile);

            while (entrancesToCheck.Count > 0)
            {
                EntranceRoomData currentEntrance = entrancesToCheck.Dequeue();

                for (int i = uncheckedGroup.Count - 1; i >= 0; i--)
                {
                    EntranceRoomData entranceToCheck = uncheckedGroup[i];

                    bool sameDirection = entranceToCheck.Direction == firstEntranceTile.Direction;
                    bool besideDirection = AreEntranceTilesBesideEachother(currentEntrance, entranceToCheck);

                    if (sameDirection && besideDirection)
                    {
                        currentEntranceTiles.Add(entranceToCheck);
                        entrancesToCheck.Enqueue(entranceToCheck);
                        uncheckedGroup.RemoveAt(i);
                    }
                }
            }
            entranceGroup.Add(currentEntranceTiles);
        }


        return entranceGroup;
    }

    /// <summary>
    /// Checks whether two entrance tiles sit beside one another along the doorway width direction.
    /// </summary>
    private bool AreEntranceTilesBesideEachother(EntranceRoomData firstEntranceTile, EntranceRoomData tileToCheck)
    {
        Vector2Int sideDirection = GetSideDirection(firstEntranceTile.Direction);
        Vector2Int tileToRight = firstEntranceTile.Tile + sideDirection;
        Vector2Int tileToLeft = firstEntranceTile.Tile - sideDirection;

        if (tileToCheck.Tile == tileToRight)
        {
            return true;
        }

        if (tileToCheck.Tile == tileToLeft)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates the centred world-space position of a grouped doorway opening.
    /// </summary>
    private Vector3 GetGroupDoorPosition(List<EntranceRoomData> entranceGroup)
    {
        Vector2 entrancePosition = Vector2.zero;

        foreach (EntranceRoomData entrance in entranceGroup)
        {
            entrancePosition += entrance.Tile;
        }

        entrancePosition /= entranceGroup.Count;

        Vector2Int doorDirection = entranceGroup[0].Direction;

        Vector3 roomPosition = new Vector3(entrancePosition.x * cellSize, propHeight, entrancePosition.y * cellSize);
        Vector3 corridorOffset = new Vector3(doorDirection.x, 0, doorDirection.y) * (cellSize * 0.5f);

        return roomPosition + corridorOffset;
    }

    /// <summary>
    /// This method checks if a prop is next another prop, if it is, it will return true
    /// This is to avoid props spawning next to one another especially torches etc.
    /// </summary>
    /// <returns>True or false</returns>
    private bool IsPropNearAnother(HashSet<Vector2Int> occupiedTiles, Vector2Int tile, int distance)
    {

        for (int x = -distance; x <= distance; x++)
        {
            for (int y = -distance; y <= distance; y++)
            {
                Vector2Int currentTile = tile + new Vector2Int(x, y);

                if (occupiedTiles.Contains(currentTile))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // <--------- Boss room Tile checking & Veil spawning Methods --------->
    /// <summary>
    /// Returns the centre tile used to position the boss portal.
    /// </summary>
    private Vector2Int BossRoomCenter(DungeonRoomData bossRoom)
    {
        return bossRoom.CenterPoint;
    }

    /// <summary>
    /// Finds and returns the generated room assigned the boss room type.
    /// </summary>
    private DungeonRoomData GetBossRooms(List<DungeonRoomData> bossRoom)
    {
        if (bossRoom == null)
        {
            return null;
        }

        foreach (DungeonRoomData roomData in bossRoom)
        {
            if (roomData.TypeOfRoom == RoomType.Boss)
            {
                return roomData;
            }
        }

        return null;
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
    /// This method will add inner rooms into rooms that have inner room generation enabled
    /// </summary>
    private void CreateInternalRoomSections(List<DungeonRoomData> generatedRooms)
    {
      if (generatedRooms == null || roomTypeSpawnRules == null)
      {
         return;
      }

      foreach (DungeonRoomData room in generatedRooms)
      {
         // Check if the inner room is enabled
         RoomTypeSpawnRule spawnRule = null;

         if (room.FloorTiles == null || room.FloorTiles.Count == 0) {
            continue;
         }

         foreach (RoomTypeSpawnRule rule in roomTypeSpawnRules)
         {
            if (rule.roomType == room.TypeOfRoom)
            {
               spawnRule = rule;
               break;
            }
         }

         if (spawnRule == null)
         {
            continue;
         }

         // Checks if the inner rooms is enabled or null
         if (spawnRule.internalRoomSettings == null || !spawnRule.internalRoomSettings.enabled)
         {
            continue;
         }

         // Sees if the room should be generated
         int randomChance = Random.Range(0, 100);

         if (randomChance >= spawnRule.internalRoomSettings.generationChance)
         {
            continue;
         }

         // Find MinX, minY, maxX and maxY
         int minX = int.MaxValue;
         int minY = int.MaxValue;
         int maxX = int.MinValue;
         int maxY = int.MinValue;

         foreach (Vector2Int tile in room.FloorTiles)
         {
            if (tile.x < minX)
            {
               minX = tile.x;
            }

            if (tile.y < minY)
            {
               minY = tile.y;
            }
            
            if (tile.x > maxX)
            {
               maxX = tile.x;
            }

            if (tile.y > maxY)
            {
               maxY = tile.y;
            }
         }

         // Calculate the room boundary
         int roomWidth = maxX - minX + 1;
         int roomHeight = maxY - minY + 1;
         
         InternalRoomSettings settings = spawnRule.internalRoomSettings;

         // Checks to see if the room width & height is wide enough to cut into sections
         bool fitsVerticalSplit = 
         roomWidth >= settings.minimumSectionWidth * 2 && 
         roomHeight >= settings.minimumSectionHeight &&
         roomHeight >= settings.doorwayWidth + 2;
         
         bool fitsHorizontalSplit = 
         roomHeight >= settings.minimumSectionHeight * 2 && 
         roomWidth >= settings.minimumSectionWidth &&
         roomWidth >= settings.doorwayWidth + 2;

         List<int> validVerticalSplits = new List<int>();
         List<int> validHorizontalSplits = new List<int>();

         if (fitsVerticalSplit)
         {
            int minSplitX = minX + settings.minimumSectionWidth;
            int maxSplitX = maxX - settings.minimumSectionWidth + 1;

            for (int possibleSplitX = minSplitX; possibleSplitX <= maxSplitX; possibleSplitX++)
            {
               int possibleWallX = possibleSplitX - 1;
               bool blocksEntrance = false;

               foreach (EntranceRoomData entrance in room.EntranceTiles)
               {
                  bool entranceIsTopOrBottom = 
                  entrance.Direction == Vector2Int.up ||
                  entrance.Direction == Vector2Int.down;

                  bool dividerTouchesEntrance = 
                  entrance.Tile.x == possibleWallX ||
                  entrance.Tile.x == possibleSplitX;

                  if (entranceIsTopOrBottom && dividerTouchesEntrance)
                  {
                     blocksEntrance = true;
                     break;
                  }
               }

               if (blocksEntrance == false)
               {
                  validVerticalSplits.Add(possibleSplitX);
               }
            }
         }

         if (fitsHorizontalSplit)
         {
             // Defines the range of the right section of the room
            int minSplitY = minY + settings.minimumSectionHeight;
            int maxSplitY = maxY - settings.minimumSectionHeight + 1;

            for (int possibleSplitY = minSplitY; 
            possibleSplitY <= maxSplitY; 
            possibleSplitY++)
            {
               int possibleWallY = possibleSplitY - 1;
               bool blocksEntrance = false;

               foreach (EntranceRoomData entrance in room.EntranceTiles)
               {
                  bool entranceIsRightOrLeft = 
                  entrance.Direction == Vector2Int.left ||
                  entrance.Direction == Vector2Int.right;

                  bool dividerTouchesEntrance = 
                  entrance.Tile.y == possibleWallY ||
                  entrance.Tile.y == possibleSplitY;

                  if (entranceIsRightOrLeft && dividerTouchesEntrance)
                  {
                     blocksEntrance = true;
                     break;
                  }
               }

               if (blocksEntrance == false)
               {
                  validHorizontalSplits.Add(possibleSplitY);
               }
            }
         }
         
         bool canVerticallySplit = validVerticalSplits.Count > 0;
         bool canHorizontallySplit = validHorizontalSplits.Count > 0;

         // If can't split vertically or horizontally skip the room
         if (!canVerticallySplit && !canHorizontallySplit)
         {
            continue;
         }

         // This methods determine whether to split it vertically or horizontally
         bool splitVertically;

         if (canVerticallySplit && canHorizontallySplit)
         {
            splitVertically = Random.value < 0.5f;
         }
         else
         {
            splitVertically = canVerticallySplit;
         }

         // Holds the tile information of both sections after the cut 
         RoomSectionData roomSectionA = new RoomSectionData();
         RoomSectionData roomSectionB = new RoomSectionData();

         // <----- CUTS THE ROOM VERTICALLY ----->
         if (splitVertically)
         {
            // Select a random split 
            int randomSplitIndex = Random.Range(0, validVerticalSplits.Count);
            int splitX = validVerticalSplits[randomSplitIndex];

            // Group the tiles that are in Section A of the room ( Left ) and Section B of the room ( Right )
            foreach (Vector2Int tile in room.FloorTiles)
            {
               if (tile.x < splitX)
               {
                  roomSectionA.FloorTiles.Add(tile);
               }
               else
               {
                  roomSectionB.FloorTiles.Add(tile);
               }
            }

            // Calculate the wall position & direction
            int wallX = splitX - 1;
            Vector2Int wallDirection = Vector2Int.right;

            // Door position & direction
            int minDoorY = minY + 1;
            int maxDoorStartY = maxY - settings.doorwayWidth;
            int doorStartY = Random.Range(minDoorY, maxDoorStartY+1);

            InnerDoorWayData doorway = new InnerDoorWayData();
            doorway.Direction = wallDirection;

            // Checks if its a wall or a doorway gap
            for (int y = minY; y <= maxY; y++)
            {
               Vector2Int wallTile = new Vector2Int(wallX, y);

               bool isDoorwayTile = 
               y >= doorStartY && 
               y < doorStartY + settings.doorwayWidth;

               if (isDoorwayTile)
               {
                  // Adds doorway tile to the doorway data tiles
                  doorway.Tiles.Add(wallTile);

                  // Stops props from spawning on the doorway section
                  room.OccupiedTiles.Add(wallTile);
                  room.OccupiedTiles.Add(wallTile + wallDirection);
               }
               else
               {
                  room.InnerWalls.Add(new InnerWallData(wallTile, wallDirection));
               }
            }

            room.InnerDoors.Add(doorway);
         }

         // <----- CUTS THE ROOM HORIZONTALLY ----->
         else
         {
            // Select a random split from the list of available ones
            int randomSplitIndex = Random.Range(0, validHorizontalSplits.Count);
            int splitY = validHorizontalSplits[randomSplitIndex];

            // Group the tiles that are in Section A of the room ( bottom ) and Section B of the room ( up )
            foreach (Vector2Int tile in room.FloorTiles)
            {
               if (tile.y < splitY)
               {
                  roomSectionA.FloorTiles.Add(tile);
               }
               else
               {
                  roomSectionB.FloorTiles.Add(tile);
               }
            }

             // Calculate the wall position & direction
            int wallY = splitY - 1;
            Vector2Int wallDirection = Vector2Int.up;

            // Door position & direction
            int minDoorX = minX + 1;
            int maxDoorStartX = maxX - settings.doorwayWidth;
            int doorStartX = Random.Range(minDoorX, maxDoorStartX+1);

            InnerDoorWayData doorway = new InnerDoorWayData();
            doorway.Direction = wallDirection;

            // Checks if its a wall or a doorway gap
            for (int x = minX; x <= maxX; x++)
            {
               Vector2Int wallTile = new Vector2Int(x, wallY);

               bool isDoorwayTile = 
               x >= doorStartX && 
               x < doorStartX + settings.doorwayWidth;

               if (isDoorwayTile)
               {
                  // Adds doorway tile to the doorway data tiles
                  doorway.Tiles.Add(wallTile);

                  // Stops props from spawning on the doorway section
                  room.OccupiedTiles.Add(wallTile);
                  room.OccupiedTiles.Add(wallTile + wallDirection);
               }
               else
               {
                  room.InnerWalls.Add(new InnerWallData(wallTile, wallDirection));
               }
            }

            room.InnerDoors.Add(doorway);
         }

         // Adds the tiles to the room section data for room A ( Left if vertical / Down if horizontal )
         // And adds the tiles to the room section data for room B ( Right if vertical / Up if horizontal )
         room.RoomSections.Add(roomSectionA);
         room.RoomSections.Add(roomSectionB);

         Debug.Log(
            "Safe vertical splits: " + validVerticalSplits.Count +
            " | Safe horizontal splits: " + validHorizontalSplits.Count +
            " | Chosen: " + (splitVertically ? "Vertical" : "Horizontal")
         );

         // Debug.Log(
         //    room.TypeOfRoom +
         //    " | Parent: " + room.FloorTiles.Count +
         //    " | Section A: " + roomSectionA.FloorTiles.Count +
         //    " | Section B: " + roomSectionB.FloorTiles.Count +
         //    " | Split: " + (splitVertically ? "Vertical" : "Horizontal") +
         //    " | Walls: " + room.InnerWalls.Count +
         //    " | Doors: " + room.InnerDoors.Count +
         //    " | Door width: " + room.InnerDoors[0].Tiles.Count
         // );
      }
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
                roomData.CeilingTiles.Add(tile);
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
    private void AssignRoomType(List<DungeonRoomData> generatedRooms, Dictionary<RoomType, int> wantedRooms)
    {
        if (generatedRooms == null || generatedRooms.Count == 0)
        {
            return;
        }

        // Assigns spawn room, exit room and boss room
        AssignMandatoryRoomTypes(generatedRooms);

        // Assigns the other rooms like, treasure, shop and miniboss rooms
        AssignRoomsBasedOnRules(generatedRooms, wantedRooms);
    }

    /// <summary>
    /// This assigns the mandatory room types like spawn, exit and boss room
    /// </summary>
    private void AssignMandatoryRoomTypes(List<DungeonRoomData> generatedRooms)
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
        DungeonRoomData exitRoom = FindFarthestRoomFromStart(generatedRooms);

        if (exitRoom != null && exitRoom != spawnRoom)
        {
            exitRoom.TypeOfRoom = RoomType.Exit;
        }
    }

    /// <summary>
    /// This method assigns the other rooms like shop, treasure, miniboss rooms on the probability, minimum & maximum count set
    /// </summary>
    private void AssignRoomsBasedOnRules(List<DungeonRoomData> generatedRooms, Dictionary<RoomType, int> wantedRooms)
    {
        if (roomTypeSpawnRules == null || roomTypeSpawnRules.Count == 0)
        {
            return;
        }

        foreach (RoomTypeSpawnRule rule in roomTypeSpawnRules)
        {
            int randomChance = Random.Range(0, 100);
            int roomsToSpawn = 0;

            if (randomChance <= rule.mediumThreshold)
            {
                roomsToSpawn = rule.lowAmount;
            }
            else if (randomChance <= rule.highThreshold)
            {
                roomsToSpawn = Random.Range(rule.lowAmount + 1, rule.mediumAmount + 1);
            }
            else
            {
                roomsToSpawn = Random.Range(rule.mediumAmount + 1, rule.highAmount + 1);
            }

            Debug.Log(
               "Rule: " + rule.roomType +
               " | Chance: " + randomChance +
               " | Wants: " + roomsToSpawn +
               " | Size Range: " + rule.minRoomSize + "-" + rule.maxRoomSize
            );

            wantedRooms[rule.roomType] = roomsToSpawn;

            for (int i = 0; i < roomsToSpawn; i++)
            {
                DungeonRoomData roomData = FindRandomNormalRoomWithSize(generatedRooms, rule);

                if (roomData == null)
                {
                    break;
                }

                roomData.TypeOfRoom = rule.roomType;
            }
        }
    }

    /// <summary>
    /// This method will check to see if every room generated has the guaranteed must spawn rooms
    /// and fits within the allocated min and max room size
    /// </summary>
    /// <param name="generatedRooms">List of generated rooms and their types</param>
    /// <returns>True or false</returns>
    private bool HasRequiredRoomSizes(List<DungeonRoomData> generatedRooms, Dictionary<RoomType, int> wantedRooms)
    {
        foreach (RoomTypeSpawnRule rule in roomTypeSpawnRules)
        {
            int roomsWanted = 0;

            if (wantedRooms.ContainsKey(rule.roomType))
            {
                roomsWanted = wantedRooms[rule.roomType];
            }

            if (roomsWanted == 0)
            {
                continue;
            }

            int validRoomCount = 0;

            foreach (DungeonRoomData room in generatedRooms)
            {
                if (room.TypeOfRoom != rule.roomType)
                {
                    continue;
                }

                if (room.FloorTiles.Count >= rule.minRoomSize && room.FloorTiles.Count <= rule.maxRoomSize)
                {
                    validRoomCount++;
                }
            }

            if (validRoomCount < roomsWanted)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// This function assigns the tile that connects to the room and corridor
    /// </summary>
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

    /// <summary>
    /// Records a room entrance when the neighbouring tile is a corridor outside the room floor.
    /// </summary>
    private void CheckForEntranceDirection(DungeonRoomData roomData, HashSet<Vector2Int> corridors, Vector2Int tile, Vector2Int direction)
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

    /// <summary>
    /// Returns room entrances whose floor tiles have not already been occupied.
    /// </summary>
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

    /// <summary>
    /// This method will try to find the closest room to the beginning boundary of the dungeon (0,0)
    /// </summary>
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

    /// <summary>
    /// This method will find the furthest possible room from the beginning boundary of the dungeon (0,0)
    /// </summary>
    /// <param name="generatedRooms"></param>
    /// <returns></returns>
    private DungeonRoomData FindFarthestRoomFromStart(List<DungeonRoomData> generatedRooms)
    {
        DungeonRoomData farthestRoom = null;
        float farthestDistance = float.MinValue;

        foreach (DungeonRoomData room in generatedRooms)
        {
            if (room.TypeOfRoom != RoomType.Normal)
            {
                continue;
            }

            float distance = Vector2Int.Distance(startPosition, room.CenterPoint);

            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthestRoom = room;
            }
        }

        return farthestRoom;
    }


    /// <summary>
    /// This method will find any room that meets the size criteria of the room its trying to find,
    /// such as a treasure, shop or miniboss room.
    /// </summary>
    /// <returns>Returns the room that fits the size criteria</returns>
    private DungeonRoomData FindRandomNormalRoomWithSize(List<DungeonRoomData> generatedRooms, RoomTypeSpawnRule roomRule)
    {
        List<DungeonRoomData> normalRoom = new List<DungeonRoomData>();

        foreach (DungeonRoomData room in generatedRooms)
        {
            if (room.TypeOfRoom != RoomType.Normal)
            {
                continue;
            }

            int roomSize = room.FloorTiles.Count;

            if (roomSize < roomRule.minRoomSize)
            {
                continue;
            }

            if (roomSize > roomRule.maxRoomSize)
            {
                continue;
            }

            normalRoom.Add(room);
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
    /// <summary>
    /// Creates random-walk room floors constrained to their assigned BSP bounds.
    /// </summary>
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

    /// <summary>
    /// Connects room centres sequentially to their nearest remaining centre with widened corridors.
    /// </summary>
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

    /// <summary>
    /// Creates an L-shaped corridor from one room centre to another and applies the configured width.
    /// </summary>
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
            Vector2Int direction;

            if (i < corridor.Count - 1)
            {
                direction = corridor[i + 1] - corridor[i];
            }
            else
            {
                direction = corridor[i] - corridor[i - 1];
            }

            AddWidth(wideCorridor, corridor[i], direction);
        }

        return wideCorridor;
    }

    /// <summary>
    /// Adds tiles perpendicular to a corridor position until the configured corridor width is reached.
    /// </summary>
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
