using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;

public class ProjectileBlade : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private float lifeTime = 3f;
    private float timer = 0f;

    private GameObject _from;
    private float _damage = 20.0f;

    public void Init(Vector3 direction, float moveSpeed, GameObject from)
    {
        moveDirection = direction.normalized;
        speed = moveSpeed;
        _from = from;
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // 플레이어가 발사한 총알
        if (_from.CompareTag("Player") && other.CompareTag("Enemy"))
        {
            TakeDamage(other.transform);
            Destroy(gameObject); // 총알 파괴
            return;
        }

        // 적이 발사한 총알
        if (_from.CompareTag("Enemy") && other.CompareTag("Player"))
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
}
