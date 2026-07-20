using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoomLayoutPattern
{
    public RoomLayoutOption layoutOption;

    [Min(1)]
    public int minimumSpacing = 3;

    [Min(0)]
    public int centerRadius = 2;
}

public enum RoomLayoutOption{
    Random,
    Perimeter,
    OpenCenter,
    CenterFocused
}


