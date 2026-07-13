// This script handles the rebind button for the Interact key.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for rebinding the Interact key.
/// </summary>
public class InteractRebindButton : MonoBehaviour
{
    public TMP_Text bindingLabel;
    public Button rebindButton;

    private bool _waitingForKey = false;

    void Start()
    {
        if (rebindButton != null) rebindButton.onClick.AddListener(StartRebind);
        UpdateLabel();
    }

    /// <summary>
    /// Listens for the next key press
    /// </summary>
    public void StartRebind()
    {
        if (_waitingForKey) return;
        _waitingForKey = true;
        if (bindingLabel != null) bindingLabel.text = "Press a key...";
    }

    void Update()
    {
        if (!_waitingForKey || Keyboard.current == null) return;

        // Check every key; grab the first one pressed this frame.
        foreach (var key in Keyboard.current.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                Debug.Log($"Interact rebind captured: {key.keyCode}");   // add this

                if (KeybindManager.Instance != null)
                    KeybindManager.Instance.SaveInteractKey(key.keyCode);
                else
                    Debug.LogWarning("No KeybindManager - interact key not saved!");   // add this

                _waitingForKey = false;
                UpdateLabel();
                break;
            }
        }
    }

    /// <summary>
    /// Show the current interact key
    /// </summary>
    private void UpdateLabel()
    {
        if (bindingLabel != null)
            bindingLabel.text = Settings.interactKey.ToString();
    }
}