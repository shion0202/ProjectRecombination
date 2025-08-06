using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Monster;
using UnityEngine;
using Unity.VisualScripting;

public class ArmLaserCharge : PartBaseArm
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject effect;
    [SerializeField] private Vector3 defaultImpulseValue = new Vector3(0.0f, 0.0f, 0.0f);
    private float maxAlphas = 1.0f;
    private Coroutine fadeCoroutine = null;

    protected override void Update()
    {
        if (!_isShooting) return;

        _currentShootTime += Time.deltaTime;
    }

    public override void UseAbility()
    {
        base.UseAbility();

        effect.SetActive(true);
    }

    public override void UseCancleAbility()
    {
        _isShooting = false;

        Shoot();
        _currentShootTime = 0.0f;
    }

    protected override void Shoot()
    {
        if (_currentShootTime > 0.0f)
        {
            // Max 값
            _currentShootTime = 1.0f;
        }

        Vector3 targetPoint = Vector3.zero;
        RaycastHit[] hits = GetMultiTargetPoint(out targetPoint);

        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;

        effect.SetActive(false);

        lineRenderer.material.color = Color.white;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, bulletSpawnPoint.position);
        lineRenderer.SetPosition(1, hits[0].point);

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        StartCoroutine(CoFadeOutLaser());

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

        //GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        //Bullet bulletComponent = bullet.GetComponent<Bullet>();
        //if (bulletComponent != null)
        //{
        //    bulletComponent.from = gameObject;
        //    bulletComponent.damage = (int)_owner.Stats.TotalStats[EStatType.Attack].Value;
        //    bulletComponent.SetBullet(camShootDirection);
        //}

        impulseSource.m_DefaultVelocity = defaultImpulseValue * _currentShootTime;
        _owner.ApplyRecoil(impulseSource, recoilX * _currentShootTime, recoilY * _currentShootTime);
    }

    private IEnumerator CoFadeOutLaser()
    {
        while (lineRenderer.material.color.a > 0.0f)
        {
            Color c = lineRenderer.material.color;
            c.a -= Time.deltaTime;
            lineRenderer.material.color = c;

            yield return null;

            if (lineRenderer.material.color.a <= 0.0f)
            {
                lineRenderer.enabled = false;
                fadeCoroutine = null;
                break;
            }
        }
    }
}
