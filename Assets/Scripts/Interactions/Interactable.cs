// This script makes an object interactable (press E to use it).
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Put this on anything the player can interact with (doors, portals, shop,
/// chests). Shows an outline when looked at, and runs the onInteract event
/// when the player interacts. Wire onInteract in the Inspector to whatever
/// should happen (e.g. open the shop).
/// </summary>
public class Interactable : MonoBehaviour
{
    private Outline outline;
    public string message; // text shown in the HUD prompt (e.g. "Open Shop")

    // What happens when the player interacts. Set this up in the Inspector.
    public UnityEvent onInteract;

    void Start()
    {
        // Grab the outline and turn it off until the player looks at this.
        outline = GetComponent<Outline>();
        DisableOutline();

    }

    /// <summary>
    /// Called by the player when they interact while looking at this.
    /// </summary>
    public void Interact()
    {
        onInteract.Invoke();
    }

    /// <summary>
    /// Show the highlight outline (player is looking at this).
    /// </summary>
    public void EnableOutline()
    {
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    /// <summary>
    /// Hide the highlight outline (player looked away).
    /// </summary>
    public void DisableOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}