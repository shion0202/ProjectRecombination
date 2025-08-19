using System.Collections;
using System.Collections.Generic;
using Monster;
using UnityEngine;

public class ArmLaserCharge : PartBaseArm
{
    [Header("차지 레이저 설정")]
    [SerializeField] protected GameObject chargeEffectPrefab;
    [SerializeField] protected float maxChargeTime = 2.0f;
    protected float _currentChargeTime = 0.0f;

    protected Vector3 defaultImpulseValue;

    protected override void Awake()
    {
        base.Awake();

        defaultImpulseValue = impulseSource.m_DefaultVelocity;
        originalColor = laserLineRenderer.material.color;
    }

    protected override void Update()
    {
        if (!_isShooting) return;

        _currentShootTime += Time.deltaTime;
    }

    public override void UseAbility()
    {
        base.UseAbility();
        chargeEffectPrefab.SetActive(true);
    }

    public override void UseCancleAbility()
    {
        base.UseCancleAbility();

        chargeEffectPrefab.SetActive(false);
        Shoot();
        _currentShootTime = 0.0f;
    }

    protected override void Shoot()
    {
        if (_currentShootTime > maxChargeTime)
        {
            _currentShootTime = maxChargeTime;
        }
        _currentChargeTime = _currentShootTime / maxChargeTime;

        RaycastHit[] hits;
        Vector3 targetPoint = GetTargetPoint(out hits);
        laserLineRenderer.SetPosition(0, bulletSpawnPoint.position);

        if (hits.Length > 0)
        {
            laserLineRenderer.SetPosition(1, targetPoint);
        }
        else
        {
            Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;
            laserLineRenderer.SetPosition(1, bulletSpawnPoint.position + camShootDirection * shootingRange);
        }

        laserLineRenderer.enabled = true;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        fadeCoroutine = StartCoroutine(CoFadeOutLaser());

        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                MonsterBase monster = hit.transform.GetComponent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage((int)_owner.Stats.CombinedPartStats[partType][EStatType.Attack].value);
                }
                else
                {
                    monster = hit.transform.GetComponentInParent<MonsterBase>();
                    if (monster != null)
                    {
                        monster.TakeDamage((int)_owner.Stats.CombinedPartStats[partType][EStatType.Attack].value);
                    }
                }

                Destroy(Instantiate(bulletPrefab, hit.point, Quaternion.identity), 0.1f);
            }
        }

        impulseSource.m_DefaultVelocity = defaultImpulseValue * _currentChargeTime;
        _owner.ApplyRecoil(impulseSource, recoilX * _currentChargeTime, recoilY * _currentChargeTime);
    }
}
