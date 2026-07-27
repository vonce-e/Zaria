// Associates a dungeon visual prefab with the weight used when selecting a variation.
// Written by Andrew Burke.

using UnityEngine;

/// <summary>
/// Associates a visual prefab with its weighted chance of being selected.
/// </summary>
[System.Serializable]
public class RoomVisualisationPrefabWeight
{
    public GameObject prefab;
    [Range(0,100)] public int weight;
}
