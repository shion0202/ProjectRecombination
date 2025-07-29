using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartArmBase : PartBase
{
    [SerializeField] protected CinemachineImpulseSource impulseSource;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected EPartType _currentPartType = EPartType.ArmL;
    [SerializeField] protected float recoilX = 4.0f;
    [SerializeField] protected float recoilY = 2.0f;
    protected float _currentShootTime = 0.0f;

    public override void FinishActionForced()
    {
        // 공격을 강제로 종료할 때 필요한 로직이 있다면 여기에 작성
    }

    public override void UseAbility()
    {
        _currentShootTime -= Time.deltaTime;
        if (_currentShootTime <= 0.0f)
        {
            _currentShootTime = (_owner.Stats.BaseStats[EStatType.FireSpeed].Value + _owner.Stats.PartStatDict[PartType][EStatType.FireSpeed].Value);
            Shoot();
        }
    }

    public override void UseCancleAbility()
    {
        // 공격 취소 시 필요한 로직이 있다면 여기에 작성
    }

    // Player의 Update에서 호출되는 공격 함수
    protected void Shoot()
    {
        // 사격 방향
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100.0f, 7))
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

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.from = gameObject;
            bulletComponent.damage = (int)_owner.Stats.TotalStats[EStatType.Attack].Value;
            bulletComponent.SetBullet(camShootDirection);
        }
    }
}
