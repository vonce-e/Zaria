// This script handles the rebind button for the Sprint and Jump keys.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for rebinding one Action.
/// </summary>
public class RebindButton : MonoBehaviour
{
    [Tooltip("The action to rebind (drag the Jump or Sprint action here).")]
    public InputActionReference actionReference;

    [Tooltip("Text that shows the current key.")]
    public TMP_Text bindingLabel;

    [Tooltip("The button the player clicks to start rebinding.")]
    public Button rebindButton;

    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    void Start()
    {
        if (rebindButton != null) rebindButton.onClick.AddListener(StartRebind);
        UpdateLabel();
    }

    /// <summary>
    /// Listens for a new key
    /// </summary>
    public void StartRebind()
    {
        if (actionReference == null) return;

        // Clean up any previous operation before starting a new one.
        _rebindOp?.Dispose();
        _rebindOp = null;

        if (bindingLabel != null) bindingLabel.text = "Press a key...";
        actionReference.action.Disable();

        _rebindOp = actionReference.action.PerformInteractiveRebinding(0)
            .WithControlsExcluding("<Mouse>/position")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(op => RebindDone())
            .OnCancel(op => RebindDone())
            .Start();
    }

    /// <summary>
    /// Finish rebinding : re-enable, update label, save.
    /// </summary>
    private void RebindDone()
    {
        _rebindOp.Dispose();
        actionReference.action.Enable();
        UpdateLabel();

        if (KeybindManager.Instance != null)
            KeybindManager.Instance.SaveActionOverrides();
    }

    /// <summary>
    /// Show the current key on the label
    /// </summary>
    private void UpdateLabel()
    {
        if (bindingLabel == null || actionReference == null) return;
        bindingLabel.text = InputControlPath.ToHumanReadableString(
            actionReference.action.bindings[0].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}