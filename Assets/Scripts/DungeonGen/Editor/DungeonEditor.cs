// Adds an editor button for generating a dungeon from an AbstractDungeonGenerator component.
// Written by Andrew Burke.

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class DungeonEditor : Editor
{
   AbstractDungeonGenerator generator;

   /// <summary>
   /// Caches the dungeon generator targeted by this custom inspector.
   /// </summary>
   private void Awake()
   {
      generator = (AbstractDungeonGenerator)target;
   }

   /// <summary>
   /// Draws the default inspector and adds a button for generating the dungeon in the editor.
   /// </summary>
   public override void OnInspectorGUI()
   {
      base.OnInspectorGUI();

      if (GUILayout.Button("Create Dungeon"))
      {
         generator.GenerateDungeon();
      } 
   }
}
