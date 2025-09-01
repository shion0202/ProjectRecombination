using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmRapidCastProjectile : PartBaseArm
{
    [Header("속사 설정")]
    [SerializeField] protected GameObject castEffectPrefab;
    [SerializeField] protected float maxChargeTime = 2.0f;
    [SerializeField] protected float maxCastTime = 1.0f;
    protected float _currentChargeTime = 0.0f;
    private float _currentCastTime = 0.0f;

    protected override void Update()
    {
        _currentShootTime -= Time.deltaTime;

        if ((_owner.CurrentPlayerState & EPlayerState.Rotating) != 0) return;
        if (!_isShooting)
        {
            _currentCastTime -= Time.deltaTime;
            return;
        }

        if (_currentCastTime <= maxCastTime)
        {
            _currentCastTime += Time.deltaTime;
            return;
        }

        _currentChargeTime += Time.deltaTime;
        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.CombinedPartStats[partType][EStatType.IntervalBetweenShots].value) / (1.0f + _currentChargeTime / maxChargeTime);
            Shoot();
        }
    }

    public override void UseAbility()
    {
        base.UseAbility();

        if (_currentCastTime < 0.0f)
        {
            _currentCastTime = 0.0f;
        }
    }

    public override void UseCancleAbility()
    {
        base.UseCancleAbility();
        _currentChargeTime = 0.0f;
    }

    protected override void Shoot()
    {
        if (_currentChargeTime > maxChargeTime)
        {
            _currentChargeTime = maxChargeTime;
        }

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
}
