// This will determine the probability for certain rooms to spawn, with minimum and max counts.
// Made by andrew
using UnityEngine;

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