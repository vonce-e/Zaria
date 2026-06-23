// Made by Andrew Burke to for the players interaction system

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float playerReach = 5f;
    public Transform raycastPoint;

    private Interactable _currentInteractable;
    
    // Update is called once per frame
    void Update()
    {
        CheckInteraction();
        
        if (Keyboard.current.eKey.wasPressedThisFrame && _currentInteractable != null)
        {
            _currentInteractable.Interact();
            Debug.Log("E key was pressed");
        }
    }
    
    /// <summary>
    /// This method casts a ray that check if the object is an interactable and allow for interaction
    /// </summary>
    void CheckInteraction()
    {
        RaycastHit hit;
        Ray ray = new Ray(raycastPoint.position, raycastPoint.forward);

        if (Physics.Raycast(ray, out hit, playerReach))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();

                if (_currentInteractable && newInteractable != _currentInteractable)
                {
                    _currentInteractable.DisableOutline();
                }
                
                if (newInteractable.enabled)
                {
                    SetNewCurrentInteractable(newInteractable);
                }
                else
                {
                    DisableCurrentInteractable();
                }

                Debug.DrawRay(
                    raycastPoint.position,
                    transform.forward * playerReach,
                    Color.green
                );
                
            }
            else
            {
                DisableCurrentInteractable();
            }
        }
        else
        {
            DisableCurrentInteractable();
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        _currentInteractable = newInteractable;
        _currentInteractable.EnableOutline();
        HUDController.instance.EnableInteractionText(_currentInteractable.message);
    }

    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (_currentInteractable)
        {
            _currentInteractable.DisableOutline();
            _currentInteractable = null;    
        }
    }
}
