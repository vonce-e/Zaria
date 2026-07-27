// Defines the shared entry point and visualiser references used by dungeon generators.
// Written by Andrew Burke.

using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
   [SerializeField] protected Dungeon3DVisualiser visualiser = null;
   [SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

   /// <summary>
   /// Clears the previous dungeon visualisation and starts a new generation run.
   /// </summary>
   public void GenerateDungeon()
   {
      if (visualiser == null)
      {
         Debug.LogWarning("Visualiser is missing");
         return;
      }
      
      visualiser.Clear();
      RunProceduralGeneration();
   }

   /// <summary>
   /// Runs the generation algorithm implemented by a concrete dungeon generator.
   /// </summary>
   protected abstract void RunProceduralGeneration();
}
