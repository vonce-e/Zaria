// This script handles loading and saving keybinding changes.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central keybind persistence.
/// </summary>
public class KeybindManager : MonoBehaviour
{
    public static KeybindManager Instance;

    [Tooltip("The Input Actions asset holding Jump/Sprint (from StarterAssets).")]
    public InputActionAsset inputActions;

    private const string OVERRIDES_KEY = "input_overrides";
    private const string INTERACT_KEY  = "interact_key";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAll();
    }

    /// <summary>
    /// Load saved Input Action overrides and the Interact key.
    /// </summary>
    private void LoadAll()
    {
        // Jump/Sprint rebinds
        if (inputActions != null && PlayerPrefs.HasKey(OVERRIDES_KEY))
        {
            string json = PlayerPrefs.GetString(OVERRIDES_KEY);
            inputActions.LoadBindingOverridesFromJson(json);
        }

        // Interact key
        if (PlayerPrefs.HasKey(INTERACT_KEY))
            Settings.interactKey = (Key)PlayerPrefs.GetInt(INTERACT_KEY);
    }

    /// <summary>
    /// Save all current Input Action overrides
    /// </summary>
    public void SaveActionOverrides()
    {
        if (inputActions == null) return;
        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(OVERRIDES_KEY, json);
    }

    /// <summary>
    /// Save the chosen Interact key
    /// </summary>
    /// <param name="key">The new interact key.</param>
    public void SaveInteractKey(Key key)
    {
        Settings.interactKey = key;
        PlayerPrefs.SetInt(INTERACT_KEY, (int)key);
    }
}