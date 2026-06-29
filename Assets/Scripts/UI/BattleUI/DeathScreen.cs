// This script handles the death screen.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The death screen
/// </summary>
public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance;

    [Header("UI")]
    public GameObject screenRoot;     // the death screen panel, hidden until death
    public Button mainMenuButton;

    [Header("Scene")]
    [Tooltip("Name of the main menu scene to return to.")]
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (screenRoot != null) screenRoot.SetActive(false);
    }

    private void Start()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMenu);
    }

    /// <summary>
    /// Show the death screen
    /// </summary>
    public void Show()
    {
        if (screenRoot != null) screenRoot.SetActive(true);

        // Free the cursor so the player can click the button.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Wipe the run and load the main menu
    /// </summary>
    private void ReturnToMenu()
    {
        // End the run : next time Play is pressed, StartNewRun makes a fresh run.
        if (RunManager.Instance != null)
            Destroy(RunManager.Instance.gameObject);

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.ChangeScene(mainMenuSceneName);
    }
}