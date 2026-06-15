using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomVisualisationClass
{
  public RoomType roomType;
  public List<RoomVisualisationPrefabWeight> floorPrefabs; 
  public List<RoomVisualisationPrefabWeight> wallPrefabs;
  public List<RoomVisualisationPrefabWeight> ceilingPrefabs;
}
