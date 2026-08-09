// Defines the weighted floor, wall, and ceiling prefab variations for each room type.
// Written by Andrew Burke.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the weighted floor, wall, and ceiling prefab variations used by a room type.
/// </summary>
[System.Serializable]
public class RoomVisualisationClass
{
  public RoomType roomType;
  public List<RoomVisualisationPrefabWeight> floorPrefabs; 
  public List<RoomVisualisationPrefabWeight> wallPrefabs;
  public List<RoomVisualisationPrefabWeight> ceilingPrefabs;
  public List<RoomVisualisationPrefabWeight> passagePrefabs;
}
