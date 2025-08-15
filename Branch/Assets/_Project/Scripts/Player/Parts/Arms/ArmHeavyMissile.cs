using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmHeavyMissile : PartBaseArm
{
    [Header("초기 랜덤 방향 세팅")]
    [SerializeField] protected float maxYawAngle = 90f; // 좌우 방향 최대 90도씩 = 180도 범위
    [SerializeField] protected float maxPitchAngle = 10f; // 상하 각도 범위 (조절 가능)

    protected override void Shoot()
    {
        Vector3 targetPoint = GetTargetPoint(out RaycastHit hit);
        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position);
        camShootDirection.Normalize();
        Vector3 randomDir = GetRandomDirection(camShootDirection);   // 발사 방향 (targetPoint 기준)

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComp = bullet.GetComponent<Bullet>();
        if (bulletComp != null)
        {
            bulletComp.Init(_owner.gameObject, bulletSpawnPoint.position, targetPoint, randomDir, (int)_owner.Stats.CombinedPartStats[partType][EStatType.Attack].value);
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
