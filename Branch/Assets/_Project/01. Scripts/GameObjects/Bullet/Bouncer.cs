using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : Bullet
{
    [SerializeField] protected int maxBounces = 3;          // 충돌 최대 횟수
    private int _bounceCount = 0;

    protected override void OnCollisionEnter(Collision collision)
    {
        _bounceCount++;
        base.OnCollisionEnter(collision);
        
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
