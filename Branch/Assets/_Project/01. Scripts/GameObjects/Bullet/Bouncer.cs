using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : Bullet
{
    [SerializeField] protected int maxBounces = 3;          // 충돌 최대 횟수
    [SerializeField] protected float bounceFactor = 0.8f;   // 튕길 때 속도 감소율 (1.0은 완전 반사)
    private int bounceCount = 0;

    protected void OnCollisionEnter(Collision collision)
    {
        if (!isCheckCollisionByBullet) return;

        // 총알의 규칙
        // 1. 플레이어가 발사한 총알은 적에게만 데미지를 입힌다.
        // 2. 적이 발사한 총알은 플레이어에게만 데미지를 입힌다.
        // 3. 총알은 벽(또는 기타 오브젝트)에 닿으면 파괴된다.

        bounceCount++;

        // 충돌한 표면의 법선 벡터 얻기
        Vector3 normal = collision.contacts[0].normal;

        // 현재 속도 반사 방향 계산
        Vector3 reflectedVelocity = Vector3.Reflect(rb.velocity, normal);

        // 튕길 때 속도 감소 적용
        rb.velocity = reflectedVelocity * bounceFactor;

        // 플레이어가 발사한 총알
        if (from.CompareTag("Player") && collision.transform.CompareTag("Enemy"))
        {
            TakeDamage(collision.transform);

            // 충돌 횟수 초과 시 투사체 파괴
            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
            }
            return;
        }

        // 적이 발사한 총알
        if (from.CompareTag("Enemy") && collision.transform.CompareTag("Player"))
        {
            var player = collision.transform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }

            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
            }
            return;
        }

        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (collision.transform.CompareTag("Wall") || collision.transform.CompareTag("Obstacle"))
        {
            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
            }
            return;
        }

        if (bounceCount >= maxBounces)
        {
            Destroy(gameObject);
        }
    }
}
