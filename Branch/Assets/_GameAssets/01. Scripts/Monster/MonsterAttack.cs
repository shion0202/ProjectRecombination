using Managers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Monster.AI.Command
{
    [Serializable]
    public class MonsterAttack
    {
        public GameObject[] bulletPrefab;
        public Transform firePoint;
        private int _lastFiredFrame = -1; // 마지막으로 총알을 발사한 프레임 번호

        public void Fire(int bulletType, GameObject shooter, Vector3 start, Vector3 end, Vector3 direction, float damage)
        {
            // 유니티 엔진의 현재 프레임 번호가 직전 발사 프레임과 같다면 중복 호출이므로 무시
            if (Time.frameCount == _lastFiredFrame)
            {
                //Debug.LogWarning($"[Fire Prevented] Any State 또는 트랜지션 꼬임으로 인한 동일 프레임 중복 호출 차단: {Time.frameCount}");
                return;
            }

            _lastFiredFrame = Time.frameCount;

            if (bulletPrefab == null || firePoint is null || bulletType < 0 || bulletType >= bulletPrefab.Length)
            {
                //Debug.LogWarning("Bullet prefab or fire point is not assigned.");
                return;
            }
            if (bulletPrefab[bulletType] is null)
            {
                //Debug.LogWarning("Bullet prefab is not assigned.");
                return;
            }

            GameObject bullet = PoolManager.Instance.GetObject(bulletPrefab[bulletType], firePoint.position, Quaternion.LookRotation(direction));
            // bullet.transform.LookAt(direction * -1);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Init(shooter, null, start, end, direction, damage);
            }
            else
            {
                Debug.LogWarning("The instantiated bullet does not have a Bullet component.");
            }
        }
    }
}