using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserArmA : PartArmBase
{
    [SerializeField] private GameObject effect;

    public override void UseAbility()
    {
        ChargeLaser();
    }

    public override void UseCancleAbility()
    {
        LaserShoot();
    }

    private void ChargeLaser()
    {
        effect.SetActive(true);
        _currentShootTime += Time.deltaTime;
    }

    private void LaserShoot()
    {
        // ��� ����
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        // 7: Enemy (�ӽ÷� LayerMask �Ű� �� ���� ��ȣ�� ����)
        RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 100.0f, 7);
        if (hits.Length > 0)
        {
            targetPoint = hits[0].point;
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

        effect.SetActive(false);

        foreach (var hit in hits)
        {
            // ������ ��ο� �ִ� ��� ������ ������
        }

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.from = gameObject;
            bulletComponent.damage = (int)_owner.Stats.TotalStats[EStatType.Attack].Value;
            bulletComponent.SetBullet(camShootDirection);
        }

        _currentShootTime = 0.0f;
    }
}
