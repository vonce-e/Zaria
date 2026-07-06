// This script handles the tab switching for the settings panel.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Shows one settings sub-panel (Game, Controls, Audio) and hides the others.
/// </summary>
public class SettingsTabs : MonoBehaviour
{
    public GameObject gamePanel;
    public GameObject controlsPanel;
    public GameObject audioPanel;

    void Start()
    {
        ShowGame();
    }

    /// <summary>
    /// Show the Game tab, hide the others
    /// </summary>
    public void ShowGame()
    {
        gamePanel.SetActive(true);
        controlsPanel.SetActive(false);
        audioPanel.SetActive(false);
    }

    /// <summary>
    /// Show the Controls tab, hide the others
    /// </summary>
    public void ShowControls()
    {
        gamePanel.SetActive(false);
        controlsPanel.SetActive(true);
        audioPanel.SetActive(false);
    }

    /// <summary>
    /// Show the Audio tab, hide the others
    /// </summary>
    public void ShowAudio()
    {
        gamePanel.SetActive(false);
        controlsPanel.SetActive(false);
        audioPanel.SetActive(true);
    }
}