// Defines the layout patterns and spacing settings used to arrange props inside rooms.
// Written by Andrew Burke.

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Configures how valid prop tiles are distributed within a generated room.
/// </summary>
[System.Serializable]
public class RoomLayoutPattern
{
    public RoomLayoutOption layoutOption;

    [Min(1)]
    public int minimumSpacing = 3;

    [Min(0)]
    public int centerRadius = 2;
}

/// <summary>
/// Identifies the supported patterns for arranging props within a room.
/// </summary>
public enum RoomLayoutOption{
    Random,
    Perimeter,
    OpenCenter,
    CenterFocused
}
