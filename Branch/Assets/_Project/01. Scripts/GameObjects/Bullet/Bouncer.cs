using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : Bullet
{
    [SerializeField] protected int maxBounces = 3;          // 충돌 최대 횟수
    [SerializeField] protected float bounceFactor = 0.9f;   // 튕길 때 속도 감소율 (1.0은 완전 반사)
    private int _bounceCount = 0;
    private float _minBounceSpeed = 5.0f;

    protected override void OnCollisionEnter(Collision collision)
    {
        _bounceCount++;

        base.OnCollisionEnter(collision);
        Vector3 normal = collision.contacts[0].normal;   // 충돌면 법선 구하기

        // 중력 성분을 속도에서 빼고 반사 계산 후 다시 더하기
        Vector3 velocityWithoutGravity = _rb.velocity - Physics.gravity * Time.fixedDeltaTime;
        Vector3 reflectedVelocity = Vector3.Reflect(velocityWithoutGravity, normal);

        // 반사 속도에 튕김 세기를 곱해 최종 속도 계산
        Vector3 newVelocity = reflectedVelocity * bounceFactor + Physics.gravity * Time.fixedDeltaTime;

        // 최소 튕김 속도 적용(너무 느리면 속도 보정)
        if (newVelocity.magnitude < _minBounceSpeed)
        {
            newVelocity = newVelocity.normalized * _minBounceSpeed;
        }

        _rb.velocity = newVelocity;

        CreateImpaceEffect(collision);
        CheckBounceCount(collision);
    }

    protected override void ShootByPlayer(Collision collision)
    {
        CreateImpaceEffect(collision);
        TakeDamage(collision.transform);

        CheckBounceCount(collision);
    }

    protected override void ShootByEnemy(Collision collision)
    {
        CreateImpaceEffect(collision);
        var player = collision.transform.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(Damage);
        }

        CheckBounceCount(collision);
    }

    protected override void ImpactObstacle(Collision collision)
    {
        CreateImpaceEffect(collision);
        CheckBounceCount(collision);
    }

    protected bool CheckBounceCount(Collision collision)
    {
        if (_bounceCount >= maxBounces)
        {
            // 충돌 횟수 초과 시 투사체 파괴
            _bounceCount = 0;
            DestroyBullet(collision);
            return true;
        }

        return false;
    }
}
