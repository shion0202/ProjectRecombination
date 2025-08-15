using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartBaseArm : PartBase
{
    [Header("사격")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected LineRenderer lineRenderer;
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected CinemachineImpulseSource impulseSource;
    [SerializeField] protected LayerMask ignoreMask = 0;
    protected float _currentShootTime = 0.0f;
    protected bool _isShooting = false;

    [Header("이펙트")]
    [SerializeField] protected GameObject effectPrefab;

    // 반동 관련 값을 스탯으로 관리할지?
    [Header("파라미터")]
    [SerializeField] protected float shootingRange = 100.0f;
    [SerializeField] protected float recoilX = 4.0f;
    [SerializeField] protected float recoilY = 2.0f;

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
            Shoot();
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.AttackSpeed].value + _owner.Stats.PartStats[PartType][EStatType.AttackSpeed].value);
        }
    }

    public override void UseAbility()
    {
        _isShooting = true;
    }

    public override void UseCancleAbility()
    {
        _isShooting = false;
    }

    public override void FinishActionForced()
    {
        _isShooting = false;
        _currentShootTime = 0.0f;
    }

    // Update를 통해 호출되는 사격 함수
    protected virtual void Shoot()
    {
        Vector3 targetPoint = GetTargetPoint(out RaycastHit hit);
        Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position);
        camShootDirection.Normalize();

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Init(_owner.gameObject, bulletSpawnPoint.position, Vector3.zero, camShootDirection, (int)_owner.Stats.CombinedPartStats[partType][EStatType.Attack].value);
        }

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);
    }

    // 카메라 기준 사격 방향을 결정하는 함수
    protected Vector3 GetTargetPoint(out RaycastHit hit)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = Vector3.zero;

        if (Physics.Raycast(ray, out hit, shootingRange))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * shootingRange;
        }

        return targetPoint;
    }

    protected RaycastHit[] GetMultiTargetPoint(out Vector3 targetPoint)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, shootingRange);
        if (hits.Length > 0)
        {
            targetPoint = hits[0].point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * shootingRange;
        }

        return hits;
    }
}
