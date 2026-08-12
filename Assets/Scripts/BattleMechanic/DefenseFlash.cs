using UnityEngine;
using System.Collections;

public class DefenseFlash : MonoBehaviour
{
    [SerializeField] private Transform modelRoot;
    [SerializeField] private Material defenseMaterial;
    [SerializeField] private float effectDuration = 0.5f;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Coroutine currentEffect;

    private void Awake()
    {
        Transform searchRoot = modelRoot != null ? modelRoot : transform;

        renderers = searchRoot.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;
        }
    }

    public void ShowDefenseEffect()
    {
        if (defenseMaterial == null)
        {
            Debug.LogWarning("Defense material has not been assigned.");
            return;
        }

        if (currentEffect != null)
        {
            StopCoroutine(currentEffect);
            RestoreOriginalMaterials();
        }

        currentEffect = StartCoroutine(FlashBlue());
    }

    private IEnumerator FlashBlue()
    {
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

        yield return new WaitForSeconds(effectDuration);

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