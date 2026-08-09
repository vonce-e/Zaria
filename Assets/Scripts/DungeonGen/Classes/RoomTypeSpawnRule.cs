// Defines room type size, quantity, probability, and internal-section generation settings.
// Written by Andrew Burke.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configures the size, quantity, probability, and internal-room settings for a room type.
/// </summary>
[System.Serializable]
public class RoomTypeSpawnRule
{
    public RoomType roomType;
    public InternalRoomSettings internalRoomSettings = new InternalRoomSettings();
    
    [Header("Define Room Size")]
    public int minRoomSize;
    public int maxRoomSize;
    
    [Header("Define Amount of Rooms")]
    public int lowAmount;
    public int mediumAmount;
    public int highAmount;
    
    [Range(0, 100)] public int mediumThreshold;
    [Range(0, 100)] public int highThreshold;
}

/// <summary>
/// Configures whether and how a generated room can be divided into internal sections.
/// </summary>
[System.Serializable]
public class InternalRoomSettings
{
    public bool enabled;

    [Range(0,100)]
    public int generationChance = 100;

    [Min(2)]
    public int minimumSectionWidth = 3;

    [Min(2)]
    public int minimumSectionHeight = 3;

    [Min(1)]
    public int doorwayWidth = 2;
}
