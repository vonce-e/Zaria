// This script bridges the interaction to the BlacksmithUI.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Opens the BlacksmithUI when the player interacts with this object.
/// </summary>
public class BlacksmithInteraction : MonoBehaviour
{
    [Tooltip("Optional. If left empty, the UI is found in the scene at Start.")]
    public BlacksmithUI blacksmithUI;

    void Start()
    {
        if (blacksmithUI == null)
            blacksmithUI = FindObjectOfType<BlacksmithUI>(true);  // include inactive
    }

    /// <summary>
    /// Opens the blacksmith panel for the current run.
    /// </summary>
    public void OpenBlacksmith()
    {
        if (blacksmithUI == null)
        {
            Debug.LogWarning("BlacksmithInteraction: no BlacksmithUI found in scene.");
            return;
        }
        if (RunManager.Instance == null)
        {
            Debug.LogWarning("BlacksmithInteraction: no RunManager - is a run active?");
            return;
        }
        blacksmithUI.Open(RunManager.Instance.runState);
    }
}