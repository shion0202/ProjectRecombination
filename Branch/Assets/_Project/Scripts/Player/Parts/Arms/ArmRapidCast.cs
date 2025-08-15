using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmRapidCast : PartBaseArm
{
    [SerializeField] protected float castingTime = 1.0f;
    [SerializeField] private float maxChargingTime = 5.0f;
    private float _currentCastingTime = 0.0f;
    private float _currentChargingTime = 0.0f;
    private Coroutine fadeCoroutine = null;

    protected override void Update()
    {
        if (!_isShooting)
        {
            _currentCastingTime -= Time.deltaTime;
            return;
        }

        if (_currentCastingTime <= castingTime)
        {
            _currentCastingTime += Time.deltaTime;
            return;
        }

        _currentShootTime -= Time.deltaTime;
        if (_currentChargingTime >= maxChargingTime)
        {
            _currentChargingTime = maxChargingTime;
        }
        else
        {
            _currentChargingTime += Time.deltaTime;
        }

        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.AttackSpeed].value + _owner.Stats.PartStats[PartType][EStatType.AttackSpeed].value) / (1.0f + _currentChargingTime / maxChargingTime);
            Shoot();
        }
    }

    public override void UseAbility()
    {
        base.UseAbility();

        if (_currentCastingTime < 0.0f)
        {
            _currentCastingTime = 0.0f;
        }
    }

    public override void UseCancleAbility()
    {
        base.UseCancleAbility();
        _currentChargingTime = 0.0f;
    }

    protected override void Shoot()
    {
        RaycastHit hit;
        Vector3 targetPoint = GetTargetPoint(out hit);

        lineRenderer.material.color = Color.white;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, bulletSpawnPoint.position);

        if (hit.collider != null)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;
            lineRenderer.SetPosition(1, bulletSpawnPoint.position + camShootDirection * 100.0f);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        fadeCoroutine = StartCoroutine(CoFadeOutLaser());

        if (hit.collider != null)
        {
            // 몬스터 피격 판정
            MonsterBase monster = hit.transform.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.TakeDamage((int)_owner.Stats.TotalStats[EStatType.Attack].value);
            }
            else
            {
                monster = hit.transform.GetComponentInParent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage((int)_owner.Stats.TotalStats[EStatType.Attack].value);
                }
            }
        }

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);

        Destroy(Instantiate(effectPrefab, targetPoint, Quaternion.identity), 0.5f);
        Destroy(Instantiate(bulletPrefab, targetPoint, Quaternion.identity), 0.5f);
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
