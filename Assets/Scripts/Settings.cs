// This script handles the Game settings.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Central settings handler
/// </summary>
public class Settings : MonoBehaviour
{
    [Header("Game")]
    public Image brightnessOverlay;   // a full-screen black Image, alpha = darkness

    [Header("Controls")]
    [Tooltip("Other scripts (camera) read these values.")]
    public static float mouseSensitivity = 1f;
    public static bool invertY = false;

    [Header("Audio (hooks into a mixer later)")]
    public AudioMixer audioMixer;

    // PlayerPrefs keys
    private const string KEY_BRIGHTNESS  = "set_brightness";
    private const string KEY_SENSITIVITY = "set_sensitivity";
    private const string KEY_INVERTY     = "set_inverty";
    private const string KEY_MASTER      = "set_master";
    private const string KEY_MUSIC       = "set_music";
    private const string KEY_SFX         = "set_sfx";

    void Start()
    {
        LoadAll();
    }

    /// <summary>
    /// Load every saved setting and apply it.
    /// </summary>
    private void LoadAll()
    {
        OnBrightnessChanged(PlayerPrefs.GetFloat(KEY_BRIGHTNESS, 0f));
        OnSensitivityChanged(PlayerPrefs.GetFloat(KEY_SENSITIVITY, 1f));
        OnInvertYChanged(PlayerPrefs.GetInt(KEY_INVERTY, 0) == 1);
        OnMasterVolumeChanged(PlayerPrefs.GetFloat(KEY_MASTER, 0.8f));
        OnMusicVolumeChanged(PlayerPrefs.GetFloat(KEY_MUSIC, 0.8f));
        OnSfxVolumeChanged(PlayerPrefs.GetFloat(KEY_SFX, 0.8f));
    }

    /// <summary>
    /// Brightness slider 0 (normal) to 1 (dark).
    /// </summary>
    /// <param name="value">Darkness amount, 0-1.</param>
    public void OnBrightnessChanged(float value)
    {
        if (brightnessOverlay != null)
        {
            Color c = brightnessOverlay.color;
            c.a = value;
            brightnessOverlay.color = c;
        }
        PlayerPrefs.SetFloat(KEY_BRIGHTNESS, value);
    }

    /// <summary>
    /// Colorblind mode
    /// </summary>
    public void OnColorblindModeChanged(int mode)
    {
        Debug.Log($"Colorblind mode {mode} selected (not implemented yet).");
    }

    /// <summary>
    /// Mouse sensitivity
    /// </summary>
    /// <param name="value">Sensitivity multiplier.</param>
    public void OnSensitivityChanged(float value)
    {
        mouseSensitivity = value;
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, value);
    }

    /// <summary>
    /// Invert vertical look
    /// </summary>
    /// <param name="inverted">True to invert the Y axis.</param>
    public void OnInvertYChanged(bool inverted)
    {
        invertY = inverted;
        PlayerPrefs.SetInt(KEY_INVERTY, inverted ? 1 : 0);
    }

    /// <summary>
    /// Keybinds
    /// </summary>
    public void OnRebindKey()
    {
        Debug.Log("Keybind rebinding not implemented yet.");
    }


    /// <summary>
    /// Master volume slider 0-1
    /// </summary>
    public void OnMasterVolumeChanged(float value)
    {
        ApplyMixerVolume("Master", value);
        PlayerPrefs.SetFloat(KEY_MASTER, value);
    }

    /// <summary>
    /// Music volume slider 0-1 
    /// </summary>
    public void OnMusicVolumeChanged(float value)
    {
        ApplyMixerVolume("Music", value);
        PlayerPrefs.SetFloat(KEY_MUSIC, value);
    }

    /// <summary>
    /// SFX volume slider 0-1
    /// </summary>
    public void OnSfxVolumeChanged(float value)
    {
        ApplyMixerVolume("SFX", value);
        PlayerPrefs.SetFloat(KEY_SFX, value);
    }

    /// <summary>
    /// Convert a 0-1 slider value to decibels and set the mixer param.
    /// </summary>
    private void ApplyMixerVolume(string exposedParam, float value01)
    {
        if (audioMixer == null) return;

        // 0 -> silent (-80 dB), 1 -> 0 dB.
        float dB = (value01 <= 0.0001f) ? -80f : Mathf.Log10(value01) * 20f;
        audioMixer.SetFloat(exposedParam, dB);
    }
}