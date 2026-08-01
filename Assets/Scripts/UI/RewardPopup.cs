// This script handles a simple reward message that fades in and out
// Made by Vonce Chew

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// One shared popup for reward messages.
/// </summary>
public class RewardPopup : MonoBehaviour
{
    public static RewardPopup Instance;

    [Tooltip("The text that shows the reward message.")]
    public TMP_Text messageText;

    [Tooltip("CanvasGroup on the popup, used to fade it in and out.")]
    public CanvasGroup canvasGroup;

    [Header("Timing")]
    [Tooltip("Seconds to fade in.")]
    public float fadeInTime = 0.25f;
    [Tooltip("Seconds to stay fully visible.")]
    public float holdTime = 1.5f;
    [Tooltip("Seconds to fade out.")]
    public float fadeOutTime = 0.5f;

    private Coroutine _running;

    void Awake()
    {
        Instance = this;
        if (canvasGroup != null) canvasGroup.alpha = 0f;  // start hidden
    }

    /// <summary>
    /// Show a reward message
    /// </summary>
    /// <param name="message">The text to display, e.g. "Got 2 cards, 45 coins!"</param>
    public void Show(string message)
    {
        if (messageText != null) messageText.text = message;

        if (_running != null) StopCoroutine(_running);
        _running = StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        if (canvasGroup == null) yield break;

        // Fade in.
        yield return Fade(canvasGroup.alpha, 1f, fadeInTime);

        // Hold.
        yield return new WaitForSeconds(holdTime);

        // Fade out.
        yield return Fade(1f, 0f, fadeOutTime);

        _running = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}