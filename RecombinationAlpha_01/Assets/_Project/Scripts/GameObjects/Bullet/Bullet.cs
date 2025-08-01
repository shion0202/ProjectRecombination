using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;

public class Bullet : MonoBehaviour
{
    private int _damage;
    private GameObject _from; // 발사 주체
    [SerializeField] private float lifeTime = 5f; // 총알의 생명 시간
    private float _timer;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private bool isCollision = true;
    [SerializeField] private float bulletSpeed = 30.0f;

    private void Start()
    {
        _timer = lifeTime;
    }

    private void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
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

    public int damage 
    {
        get => _damage;
        set => _damage = value;
    }
    
    public GameObject from
    {
        get => _from;
        set => _from = value;
    }

    public void SetBullet(Vector3 direction)
    {
        rb.velocity = direction * bulletSpeed;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isCollision)
            return;

        // 총알의 규칙
        // 1. 플레이어가 발사한 총알은 적에게만 데미지를 입힌다.
        // 2. 적이 발사한 총알은 플레이어에게만 데미지를 입힌다.
        // 3. 총알은 벽(또는 기타 오브젝트)에 닿으면 파괴된다.

        // 플레이어가 발사한 총알
        if (from.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            // Debug.Log("is Monster");
            var monster = other.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.TakeDamage(_damage);
                Destroy(gameObject); // 총알 파괴
            }
            else
            {
                monster = other.GetComponentInParent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage(_damage);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("독한 놈 좀 뒤져라!!");
                }
            }

            return;
        }
        
        // 적이 발사한 총알
        if (from.CompareTag("Enemy") && other.CompareTag("Player"))
        {
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
            Destroy(gameObject); // 총알 파괴
            return;
        }
    }
}
