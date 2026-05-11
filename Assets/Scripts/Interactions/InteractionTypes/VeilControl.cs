// This script was made to control the veil
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class VeilControl : MonoBehaviour
{
    public GameObject veil;
    private Renderer _veilRenderer;
    private Material _veilMaterial;

    private float _fadeDuration = 1.5f;
    
    void Awake()
    {
        _veilRenderer = veil.GetComponent<Renderer>();
        _veilMaterial = _veilRenderer.material;
    }
    
    /// <summary>
    /// This method will disable the veil
    /// </summary>
    public void DisableVeil()
    {
        StartCoroutine(FadeOutVeil());
    }

    public IEnumerator FadeOutVeil()
    {
        float timer = 0;
        Color color = _veilMaterial.color;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            
            float percent = timer / _fadeDuration;

            color.a = Mathf.Lerp(1f, 0f, percent);

            _veilMaterial.color = color;

            yield return null;
        }
        
        veil.SetActive(false);
    } 
}
