using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;

public class ArmLaserMultiple : PartBaseArm
{
    [Header("다중 레이저 설정")]
    [SerializeField] protected GameObject targetPrefab;
    [SerializeField] protected float spreadAngle = 10.0f;
    [SerializeField] protected float maxCastTime = 1.0f;
    [SerializeField] protected int bulletPerShot = 3;
    protected Transform currentTarget = null;          // 현재 타겟
    protected float targetingProgress = 0f;             // 타겟팅 진행도 (0~maxCastTime)
    protected GameObject currentTargetIndicator = null;

    [SerializeField] private Color targetingInProgressColor = Color.yellow;
    [SerializeField] private Color targetingCompleteColor = Color.red;
    private Vector3 targetIndicatorStartPosOffset = new Vector3(0, 5f, 0);

    protected override void Update()
    {
        if (!_isShooting)
        {
            targetingProgress = 0f;
            ClearTargetIndicator();
            currentTarget = null;
            return;
        }

        _currentShootTime += Time.deltaTime;

        // 매 프레임 새로운 타겟 찾기
        Transform newTarget = FindTargetInView();

        if (newTarget != currentTarget)
        {
            // 타겟이 바뀌면 타겟 표시 초기화 및 진행도 초기화
            currentTarget = newTarget;
            ClearTargetIndicator();

            if (currentTarget != null)
            {
                CreateTargetIndicator();
                targetingProgress = 0f;
            }
        }
        else
        {
            if (currentTarget == null)
            {
                // 타겟이 없으면 진행도 초기화
                targetingProgress = 0f;
                return;
            }

            if (!IsTargetValid(currentTarget))
            {
                ClearTargetIndicator();
                currentTarget = null;
                targetingProgress = 0f;
                return;
            }

            targetingProgress += Time.deltaTime;
            targetingProgress = Mathf.Min(targetingProgress, maxCastTime);

            UpdateTargetIndicatorPositionAndColor();
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
        ClearTargetIndicator();
        currentTarget = null;
    }

    // 타겟 적 유효성 검사 (예: 사망 또는 비활성 체크)
    private bool IsTargetValid(Transform target)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        return true;
    }

    // 타겟팅 표시 생성 함수
    private void CreateTargetIndicator()
    {
        if (targetPrefab != null && currentTarget != null)
        {
            Vector3 startPos = (currentTarget.position + Vector3.up) + targetIndicatorStartPosOffset;
            currentTargetIndicator = Instantiate(targetPrefab, startPos, Quaternion.identity, currentTarget);
            currentTargetIndicator.transform.localScale = Vector3.one * (0.1f / currentTarget.transform.localScale.x);

            SetTargetIndicatorColor(targetingInProgressColor);
        }
    }

    private void UpdateTargetIndicatorPositionAndColor()
    {
        if (currentTargetIndicator == null || currentTarget == null) return;

        float t = targetingProgress / maxCastTime;
        Vector3 startPos = (currentTarget.position + Vector3.up) + targetIndicatorStartPosOffset;
        Vector3 endPos = (currentTarget.position + Vector3.up);

        // 부드럽게 lerp (선형 보간) 해서 내려오는 위치 계산
        Vector3 lerpedPos = Vector3.Lerp(startPos, endPos, t);

        currentTargetIndicator.transform.position = lerpedPos;

        // 타겟팅 완료 시 색 변경
        if (t >= 1f)
        {
            SetTargetIndicatorColor(targetingCompleteColor);
        }
        else
        {
            SetTargetIndicatorColor(targetingInProgressColor);
        }
    }

    private void SetTargetIndicatorColor(Color color)
    {
        SpriteRenderer sr = currentTargetIndicator.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }
    }

    // 타겟팅 표시 삭제 함수
    private void ClearTargetIndicator()
    {
        if (currentTargetIndicator != null)
        {
            Destroy(currentTargetIndicator);
            currentTargetIndicator = null;
        }
    }

    protected void ShootAtTarget(Transform target)
    {
        Vector3 shootDirection = (target.position - bulletSpawnPoint.position).normalized;

        // 여기서 예전 Shoot() 안에 총알 세 발 발사 로직을 새 방향으로 수정
        SpawnBullet(shootDirection);

        for (int i = 0; i < bulletPerShot - 1; i++)
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Camera cam = Camera.main;
        Transform bestTarget = null;
        float bestViewportDistance = float.MaxValue;

        Vector2 screenCenter = new Vector2(0.5f, 0.5f);

        foreach (var enemy in enemies)
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(enemy.transform.position);

            // 적이 카메라 앞에 있고 화면 내에 있어야 함
            bool inFront = viewportPos.z > 0;
            bool inView = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

            if (inFront && inView)
            {
                // 카메라와 적 간 거리 (사거리) 체크
                float distanceToCam = Vector3.Distance(cam.transform.position, enemy.transform.position);
                if (distanceToCam <= shootingRange)
                {
                    // 뷰포트 중앙과 적 위치 간 거리 계산
                    Vector2 enemyViewportPos2D = new Vector2(viewportPos.x, viewportPos.y);
                    float viewportDistance = Vector2.Distance(screenCenter, enemyViewportPos2D);

                    // 중앙과 가장 가까운 적 선택
                    if (viewportDistance < bestViewportDistance)
                    {
                        bestViewportDistance = viewportDistance;
                        bestTarget = enemy.transform;
                    }
                }
            }
        }

        return bestTarget;
    }
}
