// This trigger will be the one that triggers the battle to start when the player walks into the zone
using UnityEngine;
using System.Collections;

public class BattleZoneTrigger : MonoBehaviour
{
    private bool hasTriggeredBattle = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasTriggeredBattle)
            return;

        hasTriggeredBattle = true;
        StartCoroutine(BattleSystem.Instance.SetupBattle());
    }
}
