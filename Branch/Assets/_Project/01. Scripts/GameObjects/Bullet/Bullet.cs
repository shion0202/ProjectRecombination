using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Monster.AI;
using Managers;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected Rigidbody _rb;

    [SerializeField] protected float bulletSpeed = 30.0f;
    private float _damage;
    private GameObject _from; // 발사 주체
    protected Vector3 _to; // 타겟 위치
    protected Transform _parent; // Spawn Point (Muzzle Flash 등의 Position을 위한 변수)
    protected Vector3 _targetDirection;

    [SerializeField] private float lifeTime = 5f; // 총알의 생명 시간
    private float _timer;

    [SerializeField] protected GameObject muzzleParticle;
    [SerializeField] protected GameObject impactParticle;
    [SerializeField] protected GameObject explosionEffectPrefab;
    [SerializeField] protected float explosionRange = 1.0f;
    [SerializeField] protected float explosionRadius = 2.0f;

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

    public float Damage
    {
        get => _damage;
        set => _damage = value;
    }

    public GameObject From
    {
        get => _from;
        set => _from = value;
    }

    public Transform Parent
    {
        get => _parent;
        set => _parent = value;
    }

    protected void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = GetComponentInChildren<Rigidbody>();
        }
    }

    protected virtual void Start()
    {
        _timer = lifeTime;
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        // 총알의 규칙
        // 1. 플레이어가 발사한 총알은 적에게만 데미지를 입힌다.
        // 2. 적이 발사한 총알은 플레이어에게만 데미지를 입힌다.
        // 3. 총알은 벽(또는 기타 오브젝트)에 닿으면 파괴된다.

        // 플레이어가 발사한 총알
        if (From.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            ShootByPlayer(other);
            return;
        }
        
        // 적이 발사한 총알
        if (From.CompareTag("Enemy") && other.CompareTag("Player"))
        {
            ShootByEnemy(other);
            return;
        }
        
        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            ImpactObstacle(other);
            return;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // 총알의 규칙
        // 1. 플레이어가 발사한 총알은 적에게만 데미지를 입힌다.
        // 2. 적이 발사한 총알은 플레이어에게만 데미지를 입힌다.
        // 3. 총알은 벽(또는 기타 오브젝트)에 닿으면 파괴된다.

        // 플레이어가 발사한 총알
        if (From.CompareTag("Player") && collision.gameObject.CompareTag("Enemy"))
        {
            ShootByPlayer(collision);
            return;
        }

        // 적이 발사한 총알
        if (From.CompareTag("Enemy") && collision.gameObject.CompareTag("Player"))
        {
            ShootByEnemy(collision);
            return;
        }

        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            ImpactObstacle(collision);
            return;
        }
    }

#if UNITY_EDITOR
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_to, explosionRange);

        if (explosionEffectPrefab != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
#endif

    public void Init(GameObject shooter, Vector3 start, Vector3 end, Vector3 direction, float damage)
    {
        _from = shooter;
        _to = end;
        _targetDirection = direction;
        _damage = damage;

        if (muzzleParticle)
        {
            // 필요할 경우 Pooling
            muzzleParticle = Instantiate(muzzleParticle, transform.position, Quaternion.LookRotation(-_targetDirection), Parent);
            Destroy(muzzleParticle, 2.0f); // Lifetime of muzzle effect.
        }

        SetBulletLogic(direction, start);
    }

    protected virtual void ShootByPlayer(Collider other)
    {
        DestroyBullet();
        TakeDamage(other.transform);
    }

    protected virtual void ShootByEnemy(Collider other)
    {
        DestroyBullet();

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(_damage);
        }
    }

    protected virtual void ImpactObstacle(Collider other)
    {
        DestroyBullet();
    }

    protected virtual void ShootByPlayer(Collision collision)
    {
        DestroyBullet();
        TakeDamage(collision.transform);
    }

    protected virtual void ShootByEnemy(Collision collision)
    {
        DestroyBullet();

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(_damage);
        }
    }

    protected virtual void ImpactObstacle(Collision collision)
    {
        DestroyBullet();
    }

    protected virtual void SetBulletLogic(Vector3 direction, Vector3 start)
    {
        _rb.velocity = direction * bulletSpeed;
    }

    protected virtual void DestroyBullet()
    {
        // 풀링 전 총알의 상태를 초기화
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _timer = lifeTime;

        if (impactParticle)
        {
            CreateImpaceEffect();
        }
        else if (explosionEffectPrefab)
        {
            Explode();
        }

        PoolManager.Instance.ReleaseObject(gameObject);
    }

    protected virtual void CreateImpaceEffect()
    {
        // 필요할 경우 Pooling
        GameObject impactP = Instantiate(impactParticle, transform.position, Quaternion.LookRotation(-transform.forward));
        Destroy(impactP, 2.0f);
    }

    protected virtual void CreateImpaceEffect(Collision collision)
    {
        // 필요할 경우 Pooling
        Vector3 contactP = collision.contacts[0].point;
        Vector3 contactN = collision.contacts[0].normal;
        GameObject iP = Instantiate(impactParticle, contactP, Quaternion.FromToRotation(Vector3.up, contactN));
        Destroy(iP, 5.0f);
    }

    protected virtual void Explode()
    {
        if (explosionEffectPrefab)
        {
            // 필요할 경우 Pooling
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        //PoolManager.Instance.ReleaseObject(gameObject);
    }

    protected void TakeDamage(Transform target, float coefficient = 1.0f)
    {
        AIController monster = target.GetComponent<AIController>();
        if (monster != null)
        {
            monster.OnHit(_damage * coefficient);
        }
        else
        {
            monster = target.transform.GetComponentInParent<AIController>();
            if (monster != null)
            {
                monster.OnHit(_damage * coefficient);
            }
        }
    }
}
