using Managers;
using UnityEngine;

public class GravityBullet : Bullet
{ 
    protected override void DestroyBullet() => Explode();

    protected override void StartBulletLogic(Vector3 direction, Vector3 start) => _rb.velocity = direction * bulletSpeed;

    protected override void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        PoolManager.Instance.ReleaseObject(gameObject);
    }
}
