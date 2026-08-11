using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    [SerializeField] private Transform doorHinge;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 180f;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Quaternion targetRotation;

    void Awake()
    {
        if (doorHinge == null)
        {
            enabled = false;
            return;
        }

        closedRotation = doorHinge.localRotation;
        targetRotation = closedRotation;
    }

    // Update is called once per frame
    void Update()
    {
        doorHinge.localRotation = Quaternion.RotateTowards(doorHinge.localRotation,
        targetRotation,
        rotationSpeed * Time.deltaTime);
    }

     public void ToggleDoor()
    {
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

        Vector3 directionToPlayer = playerCamera.transform.position - transform.position;

        float playerSide = Vector3.Dot(transform.forward, directionToPlayer);

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
    }


}