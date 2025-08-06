using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;

public class ArmRapidCast : PartBaseArm
{
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private float castingTime = 1.0f;
    private float _currentCastingTime = 0.0f;

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
        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.FireSpeed].Value + _owner.Stats.PartStatDict[PartType][EStatType.FireSpeed].Value);
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
    }

    protected override void Shoot()
    {
        Vector3 targetPoint = Vector3.zero;
        RaycastHit hit = GetTargetPoint(out targetPoint);

        // 몬스터 피격 판정
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

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);

        Destroy(Instantiate(effectPrefab, targetPoint, Quaternion.identity), 0.5f);
        Destroy(Instantiate(bulletPrefab, targetPoint, Quaternion.identity), 0.5f);
    }
}
