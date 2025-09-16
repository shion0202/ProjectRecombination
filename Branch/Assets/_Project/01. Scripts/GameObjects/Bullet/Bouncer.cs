using Managers;
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
        bounceCount++;

        // 충돌한 표면의 법선 벡터 얻기
        Vector3 normal = collision.contacts[0].normal;

        // 현재 속도 반사 방향 계산
        Vector3 reflectedVelocity = Vector3.Reflect(_rb.velocity, normal);

        // 튕길 때 속도 감소 적용
        _rb.velocity = reflectedVelocity * bounceFactor;

        // 플레이어가 발사한 총알
        if (From.CompareTag("Player") && collision.transform.CompareTag("Enemy"))
        {
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 contactNormal = collision.contacts[0].normal;
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint,
                Quaternion.FromToRotation(Vector3.up, contactNormal)
            );
            Destroy(impactP, 5.0f);

            TakeDamage(collision.transform);

            // 충돌 횟수 초과 시 투사체 파괴
            if (bounceCount >= maxBounces)
            {
                PoolManager.Instance.ReleaseObject(gameObject);
            }
            return;
        }

        // 적이 발사한 총알
        if (From.CompareTag("Enemy") && collision.transform.CompareTag("Player"))
        {
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 contactNormal = collision.contacts[0].normal;
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint,
                Quaternion.FromToRotation(Vector3.up, contactNormal)
            );
            Destroy(impactP, 5.0f);

            var player = collision.transform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(Damage);
            }

            if (bounceCount >= maxBounces)
            {
                PoolManager.Instance.ReleaseObject(gameObject);
            }
            return;
        }

        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (collision.transform.CompareTag("Wall") || collision.transform.CompareTag("Obstacle"))
        {
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 contactNormal = collision.contacts[0].normal;
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint,
                Quaternion.FromToRotation(Vector3.up, contactNormal)
            );
            Destroy(impactP, 5.0f);

            if (bounceCount >= maxBounces)
            {
                PoolManager.Instance.ReleaseObject(gameObject);
            }
            return;
        }

        Vector3 contactP = collision.contacts[0].point;
        Vector3 contactN = collision.contacts[0].normal;
        GameObject iP = Instantiate(
            impactParticle,
            contactP,
            Quaternion.FromToRotation(Vector3.up, contactN)
        );
        Destroy(iP, 5.0f);

        if (bounceCount >= maxBounces)
        {
            PoolManager.Instance.ReleaseObject(gameObject);
        }
    }
}
