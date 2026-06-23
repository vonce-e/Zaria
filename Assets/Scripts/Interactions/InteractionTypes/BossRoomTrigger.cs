using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BossRoomTrigger : MonoBehaviour
{
    public void TeleportPlayer()
    {
        SceneLoader.Instance.ChangeScene("BossRoom");
    }
}
