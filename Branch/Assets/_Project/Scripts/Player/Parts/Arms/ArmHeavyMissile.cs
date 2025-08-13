using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmHeavyMissile : PartBaseArm
{
    [Header("발사 세팅")]
    public GameObject missilePrefab;
    public Transform firePoint;
    public float maxRange = 60f;

    [Header("초기 랜덤 방향 세팅")]
    public float maxYawAngle = 90f; // 좌우 방향 최대 90도씩 = 180도 범위
    public float maxPitchAngle = 10f; // 상하 각도 범위 (조절 가능)

    public float minTargetOffsetRadius = 1f;
    public float maxTargetOffsetRadius = 5f;

    protected override void Update()
    {
        if (!_isShooting) return;

        _currentShootTime -= Time.deltaTime;
        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.FireSpeed].Value + _owner.Stats.PartStatDict[PartType][EStatType.FireSpeed].Value);
            Shoot();
        }
    }

    public override void UseAbility()
    {
        base.UseAbility();

    }

    public override void UseCancleAbility()
    {
        base.UseCancleAbility();
    }

    protected override void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxRange, ignoreMask))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * maxRange;

        // 반경도 매번 랜덤
        float randomRadius = Random.Range(minTargetOffsetRadius, maxTargetOffsetRadius);
        Vector3 randomOffset = Random.insideUnitSphere * randomRadius;

        // y축 방향 랜덤 오프셋이 불필요하다면:
        // randomOffset.y = 0f;

        // 최종 타겟 포인트
        Vector3 finalTargetPoint = targetPoint + randomOffset;

        // 발사 방향 (finalTargetPoint 기준)
        Vector3 randomDir = GetRandomDirection((finalTargetPoint - firePoint.position).normalized);

        GameObject missile = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);
        Bullet bulletComp = missile.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Init(_owner.gameObject, bulletSpawnPoint.position, targetPoint, randomDir, (int)_owner.Stats.TotalStats[EStatType.Attack].Value);
        }
    }

    protected Vector3 GetRandomDirection(Vector3 forward)
    {
        // 좌우 yaw -maxYaw ~ +maxYawdeg, 상하 pitch -maxPitch ~ +maxPitchdeg
        float roll = Random.Range(0.0f, maxPitchAngle);
        float yaw = Random.Range(-maxYawAngle, maxYawAngle);
        float pitch = Random.Range(0.0f, maxPitchAngle);
        Quaternion rot = Quaternion.Euler(pitch, yaw, roll);
        return rot * forward;
    }
}
