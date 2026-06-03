using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PointerDebug : MonoBehaviour
{
    void Update()
    {
        if (EventSystem.current == null) return;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        if (results.Count > 0)
            Debug.Log($"Top hit: {results[0].gameObject.name} (under cursor: {results.Count} object(s))");
        else
            Debug.Log("Nothing under cursor");
    }
}