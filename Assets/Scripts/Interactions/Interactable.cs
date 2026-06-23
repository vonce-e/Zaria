using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class Interactable : MonoBehaviour
{
    private Outline outline;
    public string message;

    public UnityEvent onInteract;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();

    }

    public void Interact()
    {
        onInteract.Invoke();
    }

    public void EnableOutline()
    {
        if (outline != null)
        {  
            outline.enabled = true;
        }
    }

    public void DisableOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
