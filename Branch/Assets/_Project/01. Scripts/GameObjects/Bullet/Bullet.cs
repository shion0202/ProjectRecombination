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
    protected Transform _parent; // Spawn Point (Muzzle Flash 등 Position을 위한 변수)
    protected Vector3 _targetDirection;
    protected Vector3 _targetPos;

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

    private void Awake()
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

    protected void OnEnable()
    {
        if (muzzleParticle)
        {
            // 필요할 경우 Pooling
            muzzleParticle = Instantiate(muzzleParticle, transform.position, Quaternion.LookRotation(-_targetDirection), Parent);
            Destroy(muzzleParticle, 2.0f); // Lifetime of muzzle effect.
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

#if UNITY_EDITOR
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
#endif

    protected virtual void OnTriggerEnter(Collider other)
    {
        // 총알의 규칙
        // 1. 플레이어가 발사한 총알은 적에게만 데미지를 입힌다.
        // 2. 적이 발사한 총알은 플레이어에게만 데미지를 입힌다.
        // 3. 총알은 벽(또는 기타 오브젝트)에 닿으면 파괴된다.

        // 플레이어가 발사한 총알
        if (From.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);
            
            DestroyBullet();
            
            TakeDamage(other.transform);
            
            return;
        }
        
        // 적이 발사한 총알
        if (From.CompareTag("Enemy") && other.CompareTag("Player"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);
            
            DestroyBullet();

            PlayerController player = other.GetComponent<PlayerController>();
            player?.TakeDamage(_damage);
            
            return;
        }
        
        // 벽(또는 기타 오브젝트)에 닿은 경우
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 effectDirection = (contactPoint - GetComponent<Collider>().transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(effectDirection);

            DestroyBullet();
            return;
        }
    }

    public void TakeDamage(Transform target, float coefficient = 1.0f)
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

    public void Init(GameObject shooter, Vector3 start, Vector3 end, Vector3 direction, float damage)
    {
        _from = shooter;
        _targetPos = end;
        _damage = damage;
        _targetDirection = direction;

        SetBulletLogic(direction, start);
    }

    protected virtual void SetBulletLogic(Vector3 direction, Vector3 start)
    {
        //_rb.velocity = direction * bulletSpeed;
        _rb.AddForce(direction * bulletSpeed);
    }

    protected virtual void DestroyBullet()
    {
        // 풀링 전 총알의 상태를 초기화
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _timer = lifeTime;
        if (impactParticle)
        {
            // 필요할 경우 Pooling
            GameObject impactP = Instantiate(impactParticle, transform.position, Quaternion.LookRotation(-transform.forward));
            Destroy(impactP, 2.0f);
        }
        
        PoolManager.Instance.ReleaseObject(gameObject);
    }

    protected virtual void Explode()
    {
        if (explosionEffectPrefab)
        {
            // 필요할 경우 Pooling
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        PoolManager.Instance.ReleaseObject(gameObject);
    }
}
