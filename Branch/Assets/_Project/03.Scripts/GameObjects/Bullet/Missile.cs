using Monster;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

public class Missile : Bullet
{
    [SerializeField] protected GameObject collisionBulletPrefab;
    [SerializeField] protected float turnSpeed = 120f; // 초당 회전 속도 (deg/s)
    protected Coroutine _projectileCoroutine = null;

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isCheckCollisionByBullet) return;

        // 플레이어가 발사한 총알
        if (from.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            DestroyBullet();
            return;
        }

        // 적이 발사한 총알
        if (from.CompareTag("Enemy") && other.CompareTag("Player"))
        {
            DestroyBullet();
            return;
        }

        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            DestroyBullet();
            return;
        }
    }

    protected override void DestroyBullet()
    {
        if (_projectileCoroutine != null)
        {
            StopCoroutine(_projectileCoroutine);
        }
        Explode();
    }

    protected override void StartBulletLogic(Vector3 direction, Vector3 start)
    {
        _projectileCoroutine = StartCoroutine(CoMissileRoutine(direction, start));
    }

    protected override void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            MonsterBase monster = collider.GetComponent<MonsterBase>();
            PlayerController player = from.GetComponent<PlayerController>();
            if (monster != null)
            {
                monster.TakeDamage((int)damage);
            }
            else
            {
                monster = collider.GetComponentInParent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage((int)damage);
                }
            }

            Destroy(Instantiate(collisionBulletPrefab, collider.transform.position, Quaternion.identity), 0.1f);
        }

        Destroy(gameObject);
    }

    IEnumerator CoMissileRoutine(Vector3 initialDir, Vector3 fromPos)
    {
        float elapsed = 0f;
        transform.forward = initialDir;

        float distance = Vector3.Distance(fromPos, _targetPos);     // 목표까지 거리
        float straightDistance = distance * 0.3f;                   // 직선 비행 거리: 거리의 절반 사용
        float initialFlightTime = straightDistance / bulletSpeed;   // 직선 비행 시간 = 거리 / 속도

        // 1. 초기 직선 비행 (거리 기반 시간)
        while (elapsed < initialFlightTime)
        {
            transform.position += transform.forward * bulletSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. 타겟 방향으로 부드럽게 선회하며 이동
        bool reached = false;
        while (!reached)
        {
            Vector3 dirToTarget = (_targetPos - transform.position).normalized;
            transform.forward = Vector3.RotateTowards(
                transform.forward,
                dirToTarget,
                Mathf.Deg2Rad * turnSpeed * Time.deltaTime,
                0f
            );

            transform.position += transform.forward * bulletSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _targetPos) < 1.0f)
            {
                Explode();
                reached = true;
            }

            yield return null;
        }
    }
}
