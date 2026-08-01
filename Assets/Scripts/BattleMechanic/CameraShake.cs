// This script handles the camera shake when the player takes damage.
// Made by Vonce Chew

using System.Collections;
using UnityEngine;

/// <summary>
/// Shakes this camera briefly.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Tooltip("Default shake strength (how far the camera jerks).")]
    public float defaultMagnitude = 0.15f;
    [Tooltip("Default shake length in seconds.")]
    public float defaultDuration = 0.2f;

    private Vector3 _startLocalPos;
    private Coroutine _shaking;

    void Awake()
    {
        Instance = this;
        _startLocalPos = transform.localPosition;
    }

    /// <summary>
    /// Shake with the default strength and duration.
    /// </summary>
    public void Shake()
    {
        Shake(defaultMagnitude, defaultDuration);
    }

    /// <summary>
    /// Shake with a custom strength and duration.
    /// </summary>
    /// <param name="magnitude">How far the camera jerks.</param>
    /// <param name="duration">How long the shake lasts, in seconds.</param>
    public void Shake(float magnitude, float duration)
    {
        // If already shaking, restart cleanly from the rest position.
        if (_shaking != null)
        {
            StopCoroutine(_shaking);
            transform.localPosition = _startLocalPos;
        }
        _shaking = StartCoroutine(DoShake(magnitude, duration));
    }

    private IEnumerator DoShake(float magnitude, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Random offset that fades out over the duration.
            float fade = 1f - (elapsed / duration);
            float offsetX = Random.Range(-1f, 1f) * magnitude * fade;
            float offsetY = Random.Range(-1f, 1f) * magnitude * fade;

            transform.localPosition = _startLocalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Always settle back to the exact rest position.
        transform.localPosition = _startLocalPos;
        _shaking = null;
    }
}