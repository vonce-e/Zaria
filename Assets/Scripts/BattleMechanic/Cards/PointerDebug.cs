// This script is a debug helper to see what UI is under the mouse.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Debug tool. Every frame it fires a UI raycast at the mouse position and
/// logs whatever is under the cursor. Useful for finding invisible UI elements
/// that block clicks (like a stray Image with Raycast Target left on).
/// </summary>
public class PointerDebug : MonoBehaviour
{
    void Update()
    {
        if (EventSystem.current == null) return;

        // Build a pointer event at the current mouse position.
        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        // Raycast the UI and collect everything the cursor is over.
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        // Log the top-most hit (that's the one that would receive the click).
        if (results.Count > 0)
            Debug.Log($"Top hit: {results[0].gameObject.name} (under cursor: {results.Count} object(s))");
        else
            Debug.Log("Nothing under cursor");
    }
}