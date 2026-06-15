using UnityEngine;
// This class is to allow the user to determine how common a prefab like wall or floor should spawn
// in the visualisation of the dungeon
// Made by andrew

[System.Serializable]
public class RoomVisualisationPrefabWeight
{
    public GameObject prefab;
    [Range(0,100)] public int weight;
}
