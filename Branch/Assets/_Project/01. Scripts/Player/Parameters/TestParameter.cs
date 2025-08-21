using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestParameter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // 파라미터를 설정하는 로직
                player.TakeDamage(100);
            }
            else
            {
                Debug.LogWarning("PlayerController component not found on the collided object.");
            }

            return;
        }
    }
}
