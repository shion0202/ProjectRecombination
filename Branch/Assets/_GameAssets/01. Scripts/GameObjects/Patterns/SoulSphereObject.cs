using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulSphereObject : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float lifeTime = 10.0f;
    [Tooltip("폭발 시 플레이어에게 데미지를 주는 반경. 이 범위 밖이면 피해를 받지 않는다.")]
    [SerializeField] private float explosionRadius = 5.0f;
    private DamagableObject _damageableObject;
    private float _currentTime = 0.0f;
    private float _damage;

    // 추후 Pooling할 경우 이벤트 등록 과정 수정 필요
    private void Start()
    {
        _damageableObject = gameObject.GetComponent<DamagableObject>();
        if (_damageableObject)
        {
            _damageableObject.OnObjectDied -= OnDieByPlayer;
            _damageableObject.OnObjectDied += OnDieByPlayer;
        }
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;
        if (_currentTime >= lifeTime)
        {
            ExplosionObject();
        }
    }

    void OnDestroy()
    {
        if (_damageableObject)
        {
            _damageableObject.OnObjectDied -= OnDieByPlayer;  // 이벤트 구독 해제
        }
    }

    public void Init(float inDamage, float inLifeTime = -1.0f)
    {
        _damage = inDamage;
        if (inLifeTime > 0.0f)
        {
            lifeTime = inLifeTime;
        }
    }

    private void ExplosionObject()
    {
        GameObject playerObject = Managers.MonsterManager.Instance.Player;
        if (playerObject != null)
        {
            // 폭발 반경 안에 있을 때만 데미지를 준다. (거리와 무관하게 피격되던 버그 수정)
            float sqrDistance = (playerObject.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance <= explosionRadius * explosionRadius
                && playerObject.TryGetComponent(out PlayerController player))
            {
                player.ApplyDamage(_damage, 1 << playerObject.layer);
            }
        }

        Utils.Destroy(Utils.Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1.5f);
        Utils.Destroy(gameObject);
    }

    private void OnDieByPlayer()
    {
        Utils.Destroy(gameObject);
    }
}
