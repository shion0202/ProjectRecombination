using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("발사 세팅")]
    public GameObject missilePrefab;
    public Transform firePoint;
    public float maxRange = 60f;
    public float flightTime = 1.5f; // 전체 비행 시간

    [Header("초기 랜덤 방향 세팅")]
    public float maxYawAngle = 90f; // 좌우 방향 최대 90도씩 = 180도 범위
    public float maxPitchAngle = 10f; // 상하 각도 범위 (조절 가능)

    [Header("속도 및 회전 설정")]
    public float missileSpeed = 30f;
    public float turnSpeed = 90f; // 초당 회전 속도 (deg/s)

    [Header("폭발 이펙트")]
    public GameObject explosionEffectPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            FireMissile();
        }
    }

    void FireMissile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxRange))
            targetPoint = hit.point;
        else
            targetPoint = ray.origin + ray.direction * maxRange;

        GameObject missile = Instantiate(missilePrefab, firePoint.position, Quaternion.identity);

        // 목표까지 거리
        float distance = Vector3.Distance(firePoint.position, targetPoint);

        // 직선 비행 거리: 거리의 절반 사용
        float straightDistance = distance * 0.5f;
        // 직선 비행 시간 = 거리 / 속도
        float initialFlightTime = straightDistance / missileSpeed;

        // 캐릭터 정면 기준 랜덤 방향 생성
        Vector3 randomDir = GetRandomDirection(firePoint.forward);

        StartCoroutine(MissileRoutine(missile, randomDir, targetPoint, initialFlightTime));
    }

    Vector3 GetRandomDirection(Vector3 forward)
    {
        // 좌우 yaw -maxYaw ~ +maxYawdeg, 상하 pitch -maxPitch ~ +maxPitchdeg
        float yaw = Random.Range(-maxYawAngle, maxYawAngle);
        float pitch = Random.Range(-maxPitchAngle, maxPitchAngle);
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        return rot * forward;
    }

    IEnumerator MissileRoutine(GameObject missile, Vector3 initialDir, Vector3 target, float initialFlightTime)
    {
        float elapsed = 0f;
        missile.transform.forward = initialDir;

        // 1. 초기 직선 비행 (거리 기반 시간)
        while (elapsed < initialFlightTime)
        {
            missile.transform.position += missile.transform.forward * missileSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 타겟 방향으로 부드럽게 선회하며 이동
        bool reached = false;
        while (!reached)
        {
            Vector3 dirToTarget = (target - missile.transform.position).normalized;
            missile.transform.forward = Vector3.RotateTowards(
                missile.transform.forward,
                dirToTarget,
                Mathf.Deg2Rad * turnSpeed * Time.deltaTime,
                0f
            );

            missile.transform.position += missile.transform.forward * missileSpeed * Time.deltaTime;

            if (Vector3.Distance(missile.transform.position, target) < 1.5f)
            {
                Explode(missile);
                reached = true;
            }

            yield return null;
        }
    }

    void Explode(GameObject missile)
    {
        if (explosionEffectPrefab)
            Instantiate(explosionEffectPrefab, missile.transform.position, Quaternion.identity);

        Destroy(missile);
    }
}
