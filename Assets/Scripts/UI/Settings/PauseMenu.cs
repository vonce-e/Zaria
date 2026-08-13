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
    [SerializeField] private GameObject endRunConfirmationPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isOpen = false;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (endRunConfirmationPanel != null)
        {
            endRunConfirmationPanel.SetActive(false);
        }
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
        if (_isOpen) { Close(); return; }
        if (UIState.IsPanelOpen) return;   // don't open settings over shop/blacksmith
        Open();
    }

    /// <summary>
    /// Open settings : show panel, pause, free cursor.
    /// </summary>
    public void Open()
    {
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        _isOpen = true;
        if (settingsPanel != null) settingsPanel.SetActive(true);

        Time.timeScale = 0f; // pause the game
        Cursor.lockState = CursorLockMode.None; // free the cursor
        Cursor.visible = true;

        UIState.PanelOpened();
    }

    /// <summary>
    /// Close settings: hide panel, resume, re-lock cursor.
    /// </summary>
    public void Close()
    {
        _isOpen = false;
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f; // resume the game
        Cursor.lockState = previousCursorLockMode; // re-lock for first-person
        Cursor.visible = previousCursorVisible;

        UIState.PanelClosed();
    }

    public void CancelEndRun()
    {
        if (endRunConfirmationPanel != null)
        {
            endRunConfirmationPanel.SetActive(false);
        }
    }

    public void ConfirmEndRun()
    {
        Time.timeScale = 1f;
        UIState.PanelClosed();

        if (RunManager.Instance != null)
        {
            Destroy(RunManager.Instance.gameObject);
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.ChangeScene(mainMenuSceneName);
        }
    }
}