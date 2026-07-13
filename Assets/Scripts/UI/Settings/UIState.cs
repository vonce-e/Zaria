// This script tracks whether there are any blocking UI panel.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Checks if a ui panel is open.
/// </summary>
public static class UIState
{
    /// <summary>
    /// True while any blocking panel (shop/blacksmith/settings) is open
    /// </summary>
    public static bool IsPanelOpen { get; private set; }

    /// <summary>
    /// Call when a panel opens
    /// </summary>
    public static void PanelOpened()
    {
        IsPanelOpen = true;
    }

    /// <summary>
    /// Call when a panel closes
    /// </summary>
    public static void PanelClosed()
    {
        IsPanelOpen = false;
    }
}