using System.Collections.Generic;
using UnityEngine;

public class SafeZoneObject : MonoBehaviour
{
    // 현재 안전지대 안에 있는 플레이어들. 파괴 시 남아있는 플레이어의 무적을 직접 해제한다.
    private readonly HashSet<PlayerController> _occupants = new();

    // 폭발 순간 등 외부에서 "이 플레이어가 지금 안전지대 안에 있는가"를 무적 플래그에 의존하지 않고 직접 확인한다.
    public bool IsProtecting(PlayerController player)
    {
        return player != null && _occupants.Contains(player);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player && _occupants.Add(player))
        {
            player.AddInvincibility();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.transform.CompareTag("Player")) return;
        PlayerController player = other.GetComponent<PlayerController>();
        if (player && _occupants.Remove(player))
        {
            player.RemoveInvincibility();
        }
    }

    private void OnDisable()
    {
        // OnTriggerExit이 보장되지 않는 파괴/비활성화 시, 남아있는 플레이어의 무적을 정리한다.
        foreach (PlayerController player in _occupants)
        {
            if (player) player.RemoveInvincibility();
        }
        _occupants.Clear();
    }
}
