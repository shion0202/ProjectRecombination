using Cinemachine;
using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmLaserChase : PartBaseArm
{
    [SerializeField] private LineRenderer lineRenderer;

    protected override void Update()
    {
        if (!_isShooting) return;

        Shoot();
    }

    public override void UseAbility()
    {
        base.UseAbility();

        // 타게팅
    }

    public override void UseCancleAbility()
    {
        base.UseCancleAbility();

        //if (fadeCoroutine != null)
        //{
        //    StopCoroutine(fadeCoroutine);
        //    fadeCoroutine = null;
        //}
        //fadeCoroutine = StartCoroutine(CoFadeOutLaser());
    }

    // Update에서 실행
    protected override void Shoot()
    {
        Vector3 targetPoint = Vector3.zero;
        RaycastHit[] hits = GetMultiTargetPoint(out targetPoint);

        lineRenderer.material.color = Color.white;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, bulletSpawnPoint.position);

        //if (hit.collider != null)
        //{
        //    lineRenderer.SetPosition(1, hit.point);
        //}
        //else
        //{
        //    Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        //    lineRenderer.SetPosition(1, bulletSpawnPoint.position + camShootDirection * 100.0f);
        //}

        foreach (var hit in hits)
        {
            MonsterBase monster = hit.transform.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.TakeDamage((int)_owner.Stats.TotalStats[EStatType.Attack].Value);
            }
            else
            {
                monster = hit.transform.GetComponentInParent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage((int)_owner.Stats.TotalStats[EStatType.Attack].Value);
                }
            }
        }

        Destroy(Instantiate(bulletPrefab, targetPoint, Quaternion.identity), 0.5f);
    }
}
