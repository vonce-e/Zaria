using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleRandomWalkGenerator : AbstractDungeonGenerator
{
    [SerializeField] protected SimpleRandomWalkData randomWalkParameters;
    
    protected override void RunProceduralGeneration()
    {
        // Iterate through how many positions to walk over to generate the map tiles
        HashSet<Vector2Int> floorPositions = RunRandomWalk(randomWalkParameters, startPosition);

        if (visualiser == null)
        {
            Debug.LogWarning("SimpleRandomWalkGenerator is missing a Dungeon3DVisualiser reference.");
            return;
        }
        
        // Parse through all the positions that has been walked to, and then instantiate the prefabs
        visualiser.CreateFloorTiles(floorPositions);
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkData parameters, Vector2Int position)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < randomWalkParameters.iterations; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, randomWalkParameters.walkLength);
            floorPositions.UnionWith(path);

            if (randomWalkParameters.startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
        }
        return floorPositions;
    }
}
