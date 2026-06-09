// This will determine the probability for certain rooms to spawn, with minimum and max counts.
// Made by andrew
using UnityEngine;

[System.Serializable]
public class RoomTypeSpawnRule
{
    public RoomType roomType;
    public int lowAmount;
    public int mediumAmount;
    public int highAmount;
    
    [Range(0, 100)] public int mediumThreshold;
    [Range(0, 100)] public int highThreshold;
}
