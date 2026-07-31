// This script handles the parry/dodge timing bar.
// Made by Vonce Chew

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// The three possible results of a timing-bar attempt.
/// </summary>
public enum ParryDodgeResult { Hit, Dodge, Parry }

/// <summary>
/// Drives the parry/dodge timing bar. Call Show() with a callback; the bar
/// sweeps a marker, listens for space key to be clicked, and reports the result back.
/// </summary>
public class ParryDodgeBar : MonoBehaviour
{
    [Header("UI references")]
    [Tooltip("Parent object that holds the whole bar. Toggled on/off.")]
    public GameObject barRoot;
    [Tooltip("The moving marker (a thin vertical Image).")]
    public RectTransform marker;
    [Tooltip("The track the marker slides along (defines left/right extent).")]
    public RectTransform track;
    [Tooltip("The dodge zone Image (larger).")]
    public RectTransform dodgeZone;
    [Tooltip("The parry zone Image (smaller, sits inside the dodge zone).")]
    public RectTransform parryZone;

    [Header("Timing")]
    [Tooltip("How long one full left-to-right sweep takes, in seconds.")]
    public float sweepDuration = 1.2f;
    [Tooltip("How long the bar stays up waiting for input, in seconds.")]
    public float window = 1.2f;

    [Header("Zone sizes (as fraction of track width, 0-1)")]
    [Range(0.05f, 0.9f)] public float dodgeZoneWidth = 0.30f;
    [Range(0.02f, 0.5f)] public float parryZoneWidth = 0.10f;

    [Header("Randomized zone position")]
    [Tooltip("If on, the zone appears at a random spot each time instead of the centre.")]
    public bool randomizeZonePosition = true;
    [Range(0f, 0.4f)] public float maxCentreOffset = 0.25f;

    private bool _resolved;
    private ParryDodgeResult _result;

    // Where the zone centre sits this attempt (0-1).
    private float _zoneCentre = 0.5f;

    void Awake()
    {
        if (barRoot != null) barRoot.SetActive(false);
    }

    /// <summary>
    /// Show the bar and run one timing attempt. Calls onComplete with the
    /// result when the player presses space or the window expires.
    /// </summary>
    /// <param name="onComplete">Called with Hit, Dodge, or Parry.</param>
    public void Show(Action<ParryDodgeResult> onComplete)
    {
        StartCoroutine(RunBar(onComplete));
    }

    /// <summary>
    /// Position the zones, sweep the marker, watch for space key clicked, then report.
    /// </summary>
    private IEnumerator RunBar(Action<ParryDodgeResult> onComplete)
    {
        _resolved = false;
        _result = ParryDodgeResult.Hit;  // default if they never press

        if (randomizeZonePosition)
            _zoneCentre = 0.5f + UnityEngine.Random.Range(-maxCentreOffset, maxCentreOffset);
        else
            _zoneCentre = 0.5f;

        LayoutZones();

        if (barRoot != null) barRoot.SetActive(true);

        float trackWidth = track.rect.width;
        float halfTrack = trackWidth * 0.5f;
        float elapsed = 0f;

        while (elapsed < window && !_resolved)
        {
            // Marker position: ping-pong 0..1 across the sweep
            float t = Mathf.PingPong(elapsed / sweepDuration, 1f);
            float x = Mathf.Lerp(-halfTrack, halfTrack, t);
            marker.anchoredPosition = new Vector2(x, marker.anchoredPosition.y);

            // Check for the press
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                _result = EvaluatePress(t);
                _resolved = true;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (barRoot != null) barRoot.SetActive(false);

        onComplete?.Invoke(_result);
    }

    /// <summary>
    /// Given the marker's normalised position (0-1), decide whether it
    /// landed in the parry zone, the dodge zone, or neither.
    /// </summary>
    /// <param name="markerT">Marker position along the track, 0 to 1.</param>
    private ParryDodgeResult EvaluatePress(float markerT)
    {
        // Both zones are centred on the track (0.5). Compare distance.
        float distFromCentre = Mathf.Abs(markerT - _zoneCentre);

        if (distFromCentre <= parryZoneWidth * 0.5f)
            return ParryDodgeResult.Parry;
        if (distFromCentre <= dodgeZoneWidth * 0.5f)
            return ParryDodgeResult.Dodge;
        return ParryDodgeResult.Hit;
    }

    /// <summary>
    /// Size and centre the dodge and parry zone images so they visually
    /// match the hit-detection in EvaluatePress.
    /// </summary>
    private void LayoutZones()
    {
        float trackWidth = track.rect.width;
        float centreX = (_zoneCentre - 0.5f) * trackWidth;

        if (dodgeZone != null)
        {
            dodgeZone.sizeDelta = new Vector2(trackWidth * dodgeZoneWidth, dodgeZone.sizeDelta.y);
            dodgeZone.anchoredPosition = new Vector2(centreX, dodgeZone.anchoredPosition.y);
        }

        if (parryZone != null)
        {
            parryZone.sizeDelta = new Vector2(trackWidth * parryZoneWidth, parryZone.sizeDelta.y);
            parryZone.anchoredPosition = new Vector2(centreX, parryZone.anchoredPosition.y);
        }
    }
}