using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BossRoomTrigger : MonoBehaviour
{
    [SerializeField] Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TeleportPlayer(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        // Get their character controller
        CharacterController playerCC = player.GetComponent<CharacterController>();

        if (playerCC != null)
        {
            playerCC.enabled = false;
        }

        player.transform.position = teleportTarget.position;
        player.transform.rotation = teleportTarget.rotation;

        if (playerCC != null)
        {
            playerCC.enabled = true;
        }
    }
}
