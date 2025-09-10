using System.Collections;
using System.Collections.Generic;
using Monster;
using UnityEngine;

public class ArmLaserCharge : PartBaseArm
{
    [Header("차지 레이저 설정")]
    [SerializeField] private GameObject laserPrefab;  // LineRenderer 포함된 프리팹
    [SerializeField] protected GameObject chargeEffectPrefab;
    [SerializeField] protected float maxChargeTime = 2.0f;
    protected float _currentChargeTime = 0.0f;
    private GameObject currentLaserObject;
    private LineRenderer currentLaser;

    protected Vector3 defaultImpulseValue;

    protected override void Awake()
    {
        base.Awake();

        defaultImpulseValue = impulseSource.m_DefaultVelocity;
        originalColor = laserLineRenderer.material.color;
    }

    protected override void Update()
    {
        if ((_owner.CurrentPlayerState & EPlayerState.Rotating) != 0) return;
        if (!_isShooting) return;

        _currentShootTime += Time.deltaTime;
    }

    public override void UseAbility()
    {
        base.UseAbility();
        if (currentLaserObject == null)
        {
            currentLaserObject = Instantiate(laserPrefab);
            currentLaser = currentLaserObject.GetComponent<LineRenderer>();
        }
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

        if (currentLaser == null) return;

        currentLaser.SetPosition(0, bulletSpawnPoint.position);

        if (hits.Length > 0)
        {
            currentLaser.SetPosition(1, targetPoint);
        }
        else
        {
            Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;
            currentLaser.SetPosition(1, bulletSpawnPoint.position + camShootDirection * shootingRange);
        }

        currentLaserObject.gameObject.SetActive(true);

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
                TakeDamage(hit.transform);
                Destroy(Instantiate(bulletPrefab, hit.point, Quaternion.identity), 0.1f);
            }
        }

        impulseSource.m_DefaultVelocity = defaultImpulseValue * _currentChargeTime;
        _owner.ApplyRecoil(impulseSource, recoilX * _currentChargeTime, recoilY * _currentChargeTime);
    }

    protected IEnumerator CoDestroyLaser()
    {
        yield return new WaitForSeconds(0.5f);

        Destroy(currentLaserObject);
        currentLaser = null;
        currentLaserObject = null;
    }
}
