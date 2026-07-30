// This script bridges the interaction to the ShopUI.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Opens the ShopUI when the player interacts with this object.
/// </summary>
public class ShopInteraction : MonoBehaviour
{
    [Tooltip("Optional. If left empty, the UI is found in the scene at Start.")]
    public ShopUI shopUI;

    void Start()
    {
        if (shopUI == null)
            shopUI = FindObjectOfType<ShopUI>(true);  // include inactive
    }

    /// <summary>
    /// Called by the Interactable's onInteract event. Opens the shop panel for the current run.
    /// </summary>
    public void OpenShop()
    {
        ShopUI shopUI = FindObjectOfType<ShopUI>();
        if (shopUI != null && RunManager.Instance != null)
            shopUI.Open(RunManager.Instance.runState);
        else
            Debug.LogWarning("ShopUI or RunManager not found in scene.");
    }
}