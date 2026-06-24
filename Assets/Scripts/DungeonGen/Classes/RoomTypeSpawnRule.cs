// This will determine the probability for certain rooms to spawn, with minimum and max counts.
// Made by andrew
using UnityEngine;

[System.Serializable]
public class RoomTypeSpawnRule
{
    public RoomType roomType;
    
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
