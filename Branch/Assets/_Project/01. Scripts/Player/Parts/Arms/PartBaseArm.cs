using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartBaseArm : PartBase
{
    [Header("사격")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected CinemachineImpulseSource impulseSource;
    [SerializeField] protected LayerMask ignoreMask = 0;
    protected float _currentShootTime = 0.0f;
    protected bool _isShooting = false;

    [Header("이펙트")]
    [SerializeField] protected GameObject muzzleFlashEffectPrefab;
    [SerializeField] protected LineRenderer laserLineRenderer;
    [SerializeField] protected GameObject hitEffectPrefab;
    [SerializeField] protected GameObject projectileEffectPrefab;
    protected Color originalColor = Color.white;
    protected Coroutine fadeCoroutine = null;

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
            ignoreMask &= ~LayerMask.NameToLayer("Outline");
        }
    }

    protected virtual void Update()
    {
        _currentShootTime -= Time.deltaTime;
        if (!_isShooting) return;

        if (_currentShootTime <= 0.0f)
        {
            Shoot();
            _currentShootTime = (_owner.Stats.CombinedPartStats[partType][EStatType.IntervalBetweenShots].value);
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

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;

            laserLineRenderer.enabled = false;
        }
        
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
            bulletComponent.Init(_owner.gameObject, bulletSpawnPoint.position, Vector3.zero, camShootDirection, (int)_owner.Stats.CombinedPartStats[partType][EStatType.Damage].value);
        }

        _owner.ApplyRecoil(impulseSource, recoilX, recoilY);
    }

    // 카메라 기준 사격 방향을 결정하는 함수
    protected Vector3 GetTargetPoint(out RaycastHit hit)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = Vector3.zero;

        if (Physics.Raycast(ray, out hit, shootingRange, ignoreMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * shootingRange;
        }

        return targetPoint;
    }

    protected Vector3 GetTargetPoint(out RaycastHit[] hits)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = Vector3.zero;

        hits = Physics.RaycastAll(ray.origin, ray.direction, shootingRange, ignoreMask);
        if (hits.Length > 0)
        {
            targetPoint = hits[0].point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * shootingRange;
        }

        return targetPoint;
    }

    protected IEnumerator CoFadeOutLaser()
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        laserLineRenderer.GetPropertyBlock(propertyBlock);
        Color c = originalColor;

        while (c.a > 0.0f)
        {
            propertyBlock.SetColor("_Color", Color.red);
            laserLineRenderer.SetPropertyBlock(propertyBlock);

            c.a -= Time.deltaTime;
            if (c.a <= 0.0f) c.a = 0.0f;

            yield return null;
        }

        laserLineRenderer.enabled = false;
        fadeCoroutine = null;
    }
}
