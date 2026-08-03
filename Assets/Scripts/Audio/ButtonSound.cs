// This script plays the button click SFX.
// Made by Vonce Chew

using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    // Wire this to the button's OnClick instead of the AudioManager directly.
    public void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.ButtonClick();
    }
}