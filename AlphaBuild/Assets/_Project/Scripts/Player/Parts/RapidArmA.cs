using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RapidArmA : PartArmBase
{
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private float castingTime = 1.0f;
    private float _currentCastingTime = 0.0f;

    public override void UseAbility()
    {
        if (_currentCastingTime <= castingTime)
        {
            _currentCastingTime += Time.deltaTime;
            return;
        }

        _currentShootTime -= Time.deltaTime;
        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.FireSpeed].Value + _owner.Stats.PartStatDict[PartType][EStatType.FireSpeed].Value);
            RapidShoot();
        }
    }

    public override void UseCancleAbility()
    {
        _currentCastingTime = 0.0f;
        _currentShootTime = 0.0f;
    }

    private void RapidShoot()
    {
        // 사격 방향
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;
        RaycastHit hit;

        // 7: Enemy (임시로 LayerMask 신경 안 쓰고 번호로 지정)
        if (Physics.Raycast(ray.origin, ray.direction, out hit, 100.0f, _shootLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100.0f;
        }
        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(camShootDirection, Vector3.up);
        targetRotation.x = 0.0f;
        targetRotation.z = 0.0f;
        transform.rotation = targetRotation;

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);

        // 맞은 적에게 데미지
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

        Vector3 recoilPoint = new Vector3(targetPoint.x + Random.Range(-1f, 1f), 
                                           targetPoint.y + Random.Range(-1f, 1f), 
                                           targetPoint.z + Random.Range(-1f, 1f));
        Destroy(Instantiate(effectPrefab, targetPoint, Quaternion.identity), 0.5f);
    }
}
