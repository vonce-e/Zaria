// This script flashes an enemy blue when it defends.
// Made by Vonce Chew

using UnityEngine;
using System.Collections;

/// <summary>
/// Flashes the model to a blue material for a moment, then restores its normal
/// materials. Works like HitFlash but for defending. Call ShowDefenseEffect()
/// from combat when the enemy blocks.
/// </summary>
public class DefenseFlash : MonoBehaviour
{
    [SerializeField] private Transform modelRoot; // model to flash (uses this object if empty)
    [SerializeField] private Material defenseMaterial; // the blue flash material
    [SerializeField] private float effectDuration = 0.5f;

    private Renderer[] renderers;
    private Material[][] originalMaterials; // each renderer's normal materials, saved to restore
    private Coroutine currentEffect;

    private void Awake()
    {
        // Find all the model's renderers (under modelRoot, or this object if none set).
        Transform searchRoot = modelRoot != null ? modelRoot : transform;

        renderers = searchRoot.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length][];

        // Remember each renderer's original materials so we can put them back after the flash.
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;
        }
    }

    /// <summary>
    /// Flash the model blue. Called by combat when the enemy defends.
    /// </summary>
    public void ShowDefenseEffect()
    {
        if (defenseMaterial == null)
        {
            Debug.LogWarning("Defense material has not been assigned.");
            return;
        }

        // If already flashing, stop and restore first so we don't get stuck blue.
        if (currentEffect != null)
        {
            StopCoroutine(currentEffect);
            RestoreOriginalMaterials();
        }

        currentEffect = StartCoroutine(FlashBlue());
    }

    private IEnumerator FlashBlue()
    {
        // Swap every renderer's materials to the blue one.
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] blueMaterials =
                new Material[renderers[i].sharedMaterials.Length];

            for (int materialIndex = 0;
                 materialIndex < blueMaterials.Length;
                 materialIndex++)
            {
                blueMaterials[materialIndex] = defenseMaterial;
            }

            renderers[i].sharedMaterials = blueMaterials;
        }

        // Hold the blue for a bit.
        yield return new WaitForSeconds(effectDuration);

        // Put the normal materials back.
        RestoreOriginalMaterials();
        currentEffect = null;
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials = originalMaterials[i];
        }
    }
}