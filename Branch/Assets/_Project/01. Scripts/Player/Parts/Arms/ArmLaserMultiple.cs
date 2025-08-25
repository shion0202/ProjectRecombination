using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class ArmLaserMultiple : PartBaseArm
{
    [Header("다중 레이저 설정")]
    [SerializeField] protected float spreadAngle = 10.0f;
    [SerializeField] protected float maxCastTime = 1.0f;
    protected Transform currentTarget = null;          // 현재 타겟
    protected float targetingProgress = 0f;             // 타겟팅 진행도 (0~maxCastTime)

    protected override void Update()
    {
        if (!_isShooting)
        {
            targetingProgress = 0f;
            currentTarget = null;
            return;
        }

        _currentShootTime += Time.deltaTime;

        // 타겟팅 대상 갱신
        if (currentTarget == null)
        {
            currentTarget = FindTargetInView();
        }

        if (currentTarget != null)
        {
            targetingProgress += Time.deltaTime;
            targetingProgress = Mathf.Min(targetingProgress, maxCastTime);

            // 타겟 위치 UI 등 시각화 가능 (선택사항)
        }
        else
        {
            targetingProgress = 0f; // 대상 없으면 초기화
        }
    }

    public override void UseCancleAbility()
    {
        base.UseCancleAbility();

        if (targetingProgress >= maxCastTime && currentTarget != null)
        {
            ShootAtTarget(currentTarget);
        }

        _currentShootTime = 0.0f;
        targetingProgress = 0f;
        currentTarget = null;
    }

    protected void ShootAtTarget(Transform target)
    {
        Vector3 shootDirection = (target.position - bulletSpawnPoint.position).normalized;

        // 여기서 예전 Shoot() 안에 총알 세 발 발사 로직을 새 방향으로 수정
        SpawnBullet(shootDirection);

        for (int i = 0; i < 2; i++)
        {
            Vector3 randomizedDirection = GetRandomSpreadDirection(shootDirection, spreadAngle);
            SpawnBullet(randomizedDirection);
        }

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);
    }

    protected void SpawnBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Init(
                _owner.gameObject,
                bulletSpawnPoint.position,
                Vector3.zero,
                direction,
                (int)_owner.Stats.CombinedPartStats[partType][EStatType.Damage].value
            );
        }
    }

    // 랜덤 각도 spread를 적용한 방향 벡터
    protected Vector3 GetRandomSpreadDirection(Vector3 baseDirection, float spreadAngle)
    {
        // 랜덤 각도 산출
        float halfAngle = spreadAngle / 2f;
        float randomYaw = Random.Range(-halfAngle, halfAngle);   // 좌우 회전
        float randomPitch = Random.Range(-halfAngle, halfAngle); // 상하 회전

        // yaw, pitch 회전을 기준 방향에 적용
        Quaternion rotation = Quaternion.Euler(randomPitch, randomYaw, 0);
        Vector3 randomizedDirection = rotation * baseDirection;

        return randomizedDirection.normalized;
    }

    protected Transform FindTargetInView()
    {
        // 예: 적 태그가 "Enemy"일 경우
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Camera cam = Camera.main;
        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(enemy.transform.position);
            bool inView = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z > 0;
            if (inView)
            {
                // 카메라와 적 간 거리 기준 정렬
                float dist = Vector3.Distance(cam.transform.position, enemy.transform.position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestTarget = enemy.transform;
                }
            }
        }

        return bestTarget;
    }
}
