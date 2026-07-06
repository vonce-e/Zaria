// This script handles the opening and closing of the settings panel.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Toggles the settings panel with the Escape key
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Tooltip("The settings panel to show/hide (the whole SettingsPanel).")]
    public GameObject settingsPanel;

    private bool _isOpen = false;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            Toggle();
    }

    /// <summary>
    /// Toggle the panel open/closed
    /// </summary>
    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    /// <summary>
    /// Open settings : show panel, pause, free cursor.
    /// </summary>
    public void Open()
    {
        _isOpen = true;
        if (settingsPanel != null) settingsPanel.SetActive(true);

        Time.timeScale = 0f;                    // pause the game
        Cursor.lockState = CursorLockMode.None; // free the cursor
        Cursor.visible = true;
    }

    /// <summary>
    /// Close settings: hide panel, resume, re-lock cursor.
    /// </summary>
    public void Close()
    {
        _isOpen = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;                      // resume the game
        Cursor.lockState = CursorLockMode.Locked; // re-lock for first-person
        Cursor.visible = false;
    }
}