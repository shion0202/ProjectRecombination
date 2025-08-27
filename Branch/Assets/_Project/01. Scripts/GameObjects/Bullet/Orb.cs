using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : Bullet
{
    [SerializeField] private GameObject bladePrefab;    // 칼날 투사체 프리팹
    [SerializeField] private GameObject collisionBulletPrefab; // 충돌 이펙트 프리팹
    [SerializeField] private float bladeInterval = 0.5f;  // 칼날 발사 주기
    [SerializeField] private int bladesOnDeath = 10;    // 소멸 시 발사 칼날 수
    [SerializeField] private float bladeSpeed = 15f;    // 칼날 속도

    private float bladeTimer = 0f;

    protected override void Start()
    {
        base.Start();
        bladeTimer = bladeInterval;
    }

    protected override void Update()
    {
        base.Update();

        bladeTimer += Time.deltaTime;

        // 주기적으로 랜덤 방향 칼날 발사
        if (bladeTimer >= bladeInterval)
        {
            bladeTimer = 0f;
            FireRandomBlade();
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isCheckCollisionByBullet) return;
        if (other == null) return;

        // 플레이어가 발사한 총알
        if (from.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            TakeDamage(other.transform);
            return;
        }

        // 적이 발사한 총알
        if (from.CompareTag("Enemy") && other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            return;
        }

        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            DestroyBullet();
            return;
        }
    }

    private void FireRandomBlade()
    {
        // 랜덤 방향 (평면 기준)
        float angle = Random.Range(0f, 360f);
        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

        GameObject blade = Instantiate(bladePrefab, transform.position, Quaternion.LookRotation(direction));
        ProjectileBlade bladeComp = blade.GetComponent<ProjectileBlade>();
        if (bladeComp != null)
        {
            bladeComp.Init(direction, bladeSpeed, from);
        }
    }

    protected override void DestroyBullet()
    {
        ExplodeBlades();
        base.DestroyBullet();
    }

    private void ExplodeBlades()
    {
        Destroy(Instantiate(explosionEffectPrefab, transform.position, Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f))), 3.0f);
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            TakeDamage(collider.transform);
            Destroy(Instantiate(collisionBulletPrefab, collider.transform.position, Quaternion.identity), 0.1f);
        }

        // 구체 소멸 시 다방향 칼날 일제 발사
        for (int i = 0; i < bladesOnDeath; i++)
        {
            float angle = 360f / bladesOnDeath * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            GameObject blade = Instantiate(bladePrefab, transform.position, Quaternion.LookRotation(direction));
            ProjectileBlade bladeComp = blade.GetComponent<ProjectileBlade>();
            if (bladeComp != null)
            {
                bladeComp.Init(direction, bladeSpeed, from);
            }
        }
    }
}
