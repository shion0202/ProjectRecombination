using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player)
            {
                player.SetPlayerState(EPlayerState.Invincibility, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            // 테스트 매니저가 부여한 무적(SceneTestInvincibility)은 유지하고, 안전지대 무적만 해제한다.
            if (player && !TestManager.PlayerInvincible)
            {
                player.SetPlayerState(EPlayerState.Invincibility, false);
            }
        }
    }
}
