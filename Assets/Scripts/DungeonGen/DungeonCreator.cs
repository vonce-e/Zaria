using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateDungeon();
    }

    public void CreateDungeon()
    {
        DungeonGenerator dungeonGenerator = new DungeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = dungeonGenerator.CalculateRooms(maxIterations, roomWidthMin, roomLengthMin);
    }
}
