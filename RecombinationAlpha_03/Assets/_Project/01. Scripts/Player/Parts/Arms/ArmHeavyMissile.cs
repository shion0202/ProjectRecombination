using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmHeavyMissile : PartBaseArm
{
    [Header("발사 범위")]
    [SerializeField] protected float maxYawAngle = 90f; // 좌우 방향 최대 90도씩 = 180도 범위
    [SerializeField] protected float maxPitchAngle = 10f; // 상하 각도 범위 (조절 가능)
    [SerializeField] protected float targetRadius = 5f;  // 타겟 주변 랜덤 목표 위치 반경

    protected override void Shoot()
    {
        Vector3 targetPoint = GetTargetPoint(out RaycastHit hit);

        // 타겟 주변 반경 내 랜덤 위치 생성
        Vector3 randomOffset = Random.insideUnitSphere * targetRadius;
        randomOffset.y = 0;  // 수평 오차만 적용하려면 y축 제거

        Vector3 randomizedTargetPoint = targetPoint + randomOffset;

        Vector3 camShootDirection = (randomizedTargetPoint - bulletSpawnPoint.position).normalized;

        Vector3 randomDir = GetRandomDirection(camShootDirection);

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Init(_owner.gameObject, bulletSpawnPoint.position, randomizedTargetPoint, randomDir,
                (int)_owner.Stats.CombinedPartStats[partType][EStatType.Damage].value);
        }

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);
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
