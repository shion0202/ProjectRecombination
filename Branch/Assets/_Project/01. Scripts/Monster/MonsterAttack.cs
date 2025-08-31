using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Monster.AI.Command
{
    [Serializable]
    public class MonsterAttack
    {
        public GameObject bulletPrefab;
        public Transform firePoint;

        public void Fire(GameObject shooter, Vector3 start, Vector3 end, Vector3 direction, float damage)
        {
            if (bulletPrefab == null || firePoint == null)
            {
                Debug.LogWarning("Bullet prefab or fire point is not assigned.");
                return;
            }
            GameObject bullet = Object.Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Init(shooter, start, end, direction, damage);
            }
            else
            {
                Debug.LogWarning("The instantiated bullet does not have a Bullet component.");
            }
        }
    }
}