using Monster.AI;
using UnityEngine;

namespace _Project.Scripts.GameObjects.Bullet
{
    public class Rocket : global::Bullet
    {
        // [SerializeField] private float lifeTime = 5f;        // 로켓의 생명 시간
        // [SerializeField] private int damage = 50;            // 폭발 데미지
        [SerializeField] private GameObject explosionEffect;    // 폭발 이펙트 프리팹
        [SerializeField] private float explosionForce = 700f;   // 폭발력
        [SerializeField] private TrailRenderer trailRenderer;
        
        private void Start()
        {
            // 로켓의 생명 시간 후에 자동으로 파괴
            // Destroy(gameObject, LifeTime);
            LifeTime = 10f;
            Timer = LifeTime;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            // 플레이어가 발사한 총알
            if (from.CompareTag("Player") && other.CompareTag("Enemy"))
            {
                // Debug.Log("is Monster");
                var monster = other.GetComponent<AIController>();
                if (monster != null)
                {
                    // 충돌 시 폭발 처리
                    Explode();
                }
                else
                {
                    monster = other.GetComponentInParent<AIController>();
                    if (monster != null)
                    {
                        // 충돌 시 폭발 처리
                        Explode();
                    }
                    else
                    {
                        Debug.Log("독한 놈 좀 뒤져라!!");
                    }
                }

                return;
            }
            
            // 벽(또는 기타 오브젝트)에 닿은 경우
            if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
            {
                // 충돌 시 폭발 처리
                Explode();
            }
            
        }

        private void OnDestroy()
        {
            // 폭발 이펙트 생성
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }
        }
        
        protected override void Explode()
        { 
            // 주변 오브젝트에 데미지와 힘 적용
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider collider in colliders)
            {
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }

                // 적에게 데미지 적용
                AIController monster = collider.GetComponent<AIController>();
                if (monster != null)
                {
                    monster.OnHit(damage);
                }
            }

            // 로켓 파괴
            Destroy(gameObject);
        }
    }
}