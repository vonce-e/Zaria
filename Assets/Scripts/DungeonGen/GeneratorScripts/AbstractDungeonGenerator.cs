using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
   [SerializeField] protected Dungeon3DVisualiser visualiser = null;
   [SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

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

   protected abstract void RunProceduralGeneration();
}
