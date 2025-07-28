using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartArmBase : PartBase
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] protected EPartType _currentPartType = EPartType.ArmL;

    [SerializeField] private float bulletSpeed = 20.0f;
    [SerializeField] private float shootCooldown = 0.5f;
    private float _lastShootTime = -Mathf.Infinity;

    public override void FinishActionForced()
    {
        
    }

    public override void UseAbility()
    {
        Debug.Log("빵야빵야");
        // Shoot();
    }

    // Player의 Update에서 호출되는 공격 함수
    protected void Shoot()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100.0f))
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

        Inventory inven = GetComponent<Inventory>();
        PartBase ability = inven.EquippedItems[EPartType.Shoulder].GetComponent<PartBase>();
        if (ability != null)
        {
            ability.UseAbility();
        }

        // 반동 인자 필요
        _owner.ApplyRecoil();

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.transform.localScale = Vector3.one * 0.1f;
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.from = gameObject;
            bulletComponent.damage = (int)1;
        }
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = camShootDirection * bulletSpeed;
        }
        Destroy(bullet, 3.0f);
        _lastShootTime = Time.time;
    }
}
