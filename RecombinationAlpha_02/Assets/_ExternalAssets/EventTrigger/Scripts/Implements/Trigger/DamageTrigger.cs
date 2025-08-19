using UnityEngine;

public class DamageTrigger : BaseEventTrigger
{
    [Header("오브젝트의 체력 (공격 당하는 횟수)")]
    public int objectHP = 1;

    [Header("피격 쿨타임")]
    public float damageCooldown = 1f;

    private float lastDamagedTime = -999f;

    // 외부에서 데미지를 줄 때 호출하는 함수
    public bool ApplyDamage(int amount)
    {
        if (Time.time < lastDamagedTime + damageCooldown)
        {
            return false; // 쿨타임 중이면 무시
        }

        lastDamagedTime = Time.time;
        objectHP -= amount;

        if (objectHP <= 0)
        {
            Execute(this.gameObject);
        }
        return true;
    }
}