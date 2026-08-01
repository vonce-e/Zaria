// This script briefly flashes this unit's model when it takes damage.
// Made by Vonce Chew

using System.Collections;
using UnityEngine;

/// <summary>
/// Flashes the model by swapping its materials to another material.
/// </summary>
public class HitFlash : MonoBehaviour
{
    [Tooltip("The model root to flash. If empty, uses this object's children.")]
    public Transform modelRoot;

    [Tooltip("A material to flash to (e.g. a plain white or red URP/Lit material).")]
    public Material flashMaterial;

    [Tooltip("How long the flash lasts, in seconds.")]
    public float flashDuration = 0.1f;

    private Renderer[] _renderers;
    private Material[][] _originalMaterials;
    private Coroutine _flashing;

    void Awake()
    {
        Transform searchRoot = (modelRoot != null) ? modelRoot : transform;
        _renderers = searchRoot.GetComponentsInChildren<Renderer>();

        // Remember each renderer's original material array to restore later.
        _originalMaterials = new Material[_renderers.Length][];
        for (int i = 0; i < _renderers.Length; i++)
            _originalMaterials[i] = _renderers[i].sharedMaterials;
    }

    /// <summary>
    /// Flash the model and then restore its original material
    /// </summary>
    public void Flash()
    {
        if (_renderers == null || _renderers.Length == 0) return;
        if (flashMaterial == null)
        {
            Debug.LogWarning("HitFlash: no flashMaterial assigned.");
            return;
        }

        if (_flashing != null)
        {
            StopCoroutine(_flashing);
            RestoreMaterials(); // make sure the mat doesn't get stuck on the flash mat
        }
        _flashing = StartCoroutine(DoFlash());
    }

    private IEnumerator DoFlash()
    {
        // Swap every renderer to the flash material
        for (int i = 0; i < _renderers.Length; i++)
        {
            int slotCount = _renderers[i].sharedMaterials.Length;
            var flashArray = new Material[slotCount];
            for (int s = 0; s < slotCount; s++)
                flashArray[s] = flashMaterial;
            _renderers[i].sharedMaterials = flashArray;
        }

        yield return new WaitForSeconds(flashDuration);

        RestoreMaterials();
        _flashing = null;
    }

    private void RestoreMaterials()
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].sharedMaterials = _originalMaterials[i];
    }
}