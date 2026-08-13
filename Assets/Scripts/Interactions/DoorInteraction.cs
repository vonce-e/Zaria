// This script handles opening and closing a door.
// Made by Vonce Chew

using UnityEngine;

/// <summary>
/// Swings a door open/closed when interacted with. The door opens away from
/// the player (whichever side they're standing on) so it never swings into
/// them. Wire ToggleDoor() to the Interactable's onInteract event.
/// </summary>
public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private Transform doorHinge; // the part that rotates
    [SerializeField] private float openAngle = 90f; // how far it swings
    [SerializeField] private float rotationSpeed = 180f; // degrees per second

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation; // where the hinge is trying to rotate to

    void Awake()
    {
        // No hinge assigned = nothing to rotate, so switch this script off.
        if (doorHinge == null)
        {
            enabled = false;
            return;
        }

        // Remember the starting (closed) rotation.
        closedRotation = doorHinge.localRotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        // Smoothly rotate the hinge toward its target each frame.
        doorHinge.localRotation = Quaternion.RotateTowards(doorHinge.localRotation,
        targetRotation,
        rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Open the door if closed, close it if open. Called on interact.
    /// </summary>
    public void ToggleDoor()
    {
        // Already open? Just close it.
        if (isOpen)
        {
            targetRotation = closedRotation;
            isOpen = false;
            return;
        }

        Camera playerCamera = Camera.main;

        if (playerCamera == null)
        {
            return;
        }

        // Work out which side of the door the player is on.
        Vector3 directionToPlayer = playerCamera.transform.position - transform.position;
        float playerSide = Vector3.Dot(transform.forward, directionToPlayer);

        // Open away from the player so it doesn't swing into them.
        float openingDirection;

        if (playerSide >= 0f)
        {
            openingDirection = openAngle;
        }
        else
        {
            openingDirection = -openAngle;
        }

        openRotation = closedRotation * Quaternion.Euler(0f, openingDirection, 0f);

        targetRotation = openRotation;
        isOpen = true;

        // Play the door sound.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.DoorOpen();
        }
    }
}