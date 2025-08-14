using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartBaseArm : PartBase
{
    [SerializeField] protected CinemachineImpulseSource impulseSource;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected EPartType _currentPartType = EPartType.ArmL;
    [SerializeField] protected float recoilX = 4.0f;
    [SerializeField] protected float recoilY = 2.0f;
    protected float _currentShootTime = 0.0f;
    protected bool _isShooting = false;

    [SerializeField] protected LayerMask ignoreMask = 0;

    protected virtual void Awake()
    {
        if (ignoreMask == 0)
        {
            ignoreMask |= 1;
            ignoreMask &= ~LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    protected virtual void Update()
    {
        if (!_isShooting) return;

        _currentShootTime -= Time.deltaTime;
        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.AttackSpeed].value + _owner.Stats.PartStatDict[PartType][EStatType.AttackSpeed].value);
            Shoot();
        }
    }

    public override void UseAbility()
    {
        _isShooting = true;
    }

    public override void UseCancleAbility()
    {
        _isShooting = false;
        //_currentShootTime = 0.0f;
    }

    public override void FinishActionForced()
    {
        // 공격을 강제로 종료할 때 필요한 로직이 있다면 여기에 작성
    }

    // Update에서 호출되는 사격 함수
    protected virtual void Shoot()
    {
        Vector3 targetPoint = Vector3.zero;
        GetTargetPoint(out targetPoint);

        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position);
        camShootDirection.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Init(_owner.gameObject, bulletSpawnPoint.position, Vector3.zero, camShootDirection, (int)_owner.Stats.TotalStats[EStatType.Attack].value);
        }

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);
    }

    // 카메라 기준 사격 방향을 결정하는 함수
    protected RaycastHit GetTargetPoint(out Vector3 targetPoint)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100.0f;
        }

        return hit;
    }

    protected RaycastHit[] GetMultiTargetPoint(out Vector3 targetPoint)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 100.0f);
        if (hits.Length > 0)
        {
            targetPoint = hits[0].point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * 100.0f;
        }

        return hits;
    }
}
