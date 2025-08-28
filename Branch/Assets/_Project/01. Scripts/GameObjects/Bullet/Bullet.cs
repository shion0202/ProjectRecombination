using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected Rigidbody rb;

    [SerializeField] protected float bulletSpeed = 30.0f;
    [SerializeField] protected bool isCheckCollisionByBullet = true;
    private float _damage;
    private GameObject _from; // 발사 주체
    protected Vector3 _targetPos;

    [SerializeField] private float lifeTime = 5f; // 총알의 생명 시간
    private float _timer;

    [SerializeField] protected GameObject explosionEffectPrefab;
    [SerializeField] protected float explosionRange = 1.0f;
    [SerializeField] protected float explosionRadius = 2.0f;

    [SerializeField] protected GameObject muzzleParticle;
    [SerializeField] protected GameObject impactParticle;

    protected float LifeTime
    {
        get => lifeTime;
        set => lifeTime = value;
    }

    protected float Timer
    {
        get => _timer;
        set => _timer = value;
    }

    public float damage
    {
        get => _damage;
        set => _damage = value;
    }

    public GameObject from
    {
        get => _from;
        set => _from = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        _timer = lifeTime;

        transform.forward = _from.transform.forward;

        if (muzzleParticle)
        {
            muzzleParticle = Instantiate(muzzleParticle, transform.position, Quaternion.LookRotation(-_from.transform.forward));
            Destroy(muzzleParticle, 1.5f); // Lifetime of muzzle effect.
        }
    }

    protected virtual void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            DestroyBullet();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_targetPos, explosionRange);

        if (explosionEffectPrefab != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isCheckCollisionByBullet) return;

        // 총알의 규칙
        // 1. 플레이어가 발사한 총알은 적에게만 데미지를 입힌다.
        // 2. 적이 발사한 총알은 플레이어에게만 데미지를 입힌다.
        // 3. 총알은 벽(또는 기타 오브젝트)에 닿으면 파괴된다.

        if (other == null) return;

        // 플레이어가 발사한 총알
        if (from.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint, // 접점 위치
                Quaternion.LookRotation(effectDirection) // 추정 방향 정렬
            );
            Destroy(impactP, 5.0f);

            TakeDamage(other.transform);
            Destroy(gameObject); // 총알 파괴
            return;
        }
        
        // 적이 발사한 총알
        if (from.CompareTag("Enemy") && other.CompareTag("Player"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint, // 접점 위치
                Quaternion.LookRotation(effectDirection) // 추정 방향 정렬
            );
            Destroy(impactP, 5.0f);

            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(_damage);
                Destroy(gameObject); // 총알 파괴
            }

            return;
        }
        
        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint, // 접점 위치
                Quaternion.LookRotation(effectDirection) // 추정 방향 정렬
            );
            Destroy(impactP, 5.0f);

            Destroy(gameObject); // 총알 파괴
            return;
        }

        if (other.CompareTag("Breakable"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);
            GameObject impactP = Instantiate(
                impactParticle,
                contactPoint, // 접점 위치
                Quaternion.LookRotation(effectDirection) // 추정 방향 정렬
            );
            Destroy(impactP, 5.0f);

            foreach (Transform child in other.transform)
            {

                MeshCollider mCollider = child.GetComponent<MeshCollider>();
                if (mCollider != null)
                {
                    mCollider.enabled = true; // 충돌을 비활성화
                }

                Rigidbody orb = other.GetComponent<Rigidbody>();
                if (orb != null)
                {
                    orb.useGravity = true;
                    orb.AddExplosionForce(200.0f, transform.position, 20.0f);
                }
            }

            Destroy(gameObject);
        }
    }

    public void TakeDamage(Transform target, float coefficient = 1.0f)
    {
        MonsterBase monster = target.GetComponent<MonsterBase>();
        if (monster != null)
        {
            monster.TakeDamage((int)(_damage * coefficient));
        }
        else
        {
            monster = target.transform.GetComponentInParent<MonsterBase>();
            if (monster != null)
            {
                monster.TakeDamage((int)(_damage * coefficient));
            }
        }
    }

    public void Init(GameObject shooter, Vector3 start, Vector3 end, Vector3 direction, float damage)
    {
        _from = shooter;
        _targetPos = end;
        _damage = damage;

        StartBulletLogic(direction, start);
    }

    protected virtual void StartBulletLogic(Vector3 direction, Vector3 start)
    {
        rb.velocity = direction * bulletSpeed;
    }

    protected virtual void DestroyBullet()
    {
        Destroy(gameObject);
    }

    protected virtual void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
