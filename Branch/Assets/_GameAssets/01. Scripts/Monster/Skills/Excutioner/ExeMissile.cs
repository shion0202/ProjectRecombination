using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class ExeMissile : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    private Vector3 _targetPosition;
    private float _lifeTime;
    private bool _isExplosion;

    private Vector3 _startPosition;
    private float _elapsed = 0.0f;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float _damage;

    private void Update()
    {
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, _elapsed / _lifeTime);
        _elapsed += Time.deltaTime;

        if (_elapsed >= _lifeTime)
        {
            Explosion();
        }
    }

    public void Init(Vector3 inTargetPosition, bool isExplosion, float lifeTime = 1.0f)
    {
        _targetPosition = inTargetPosition;
        _lifeTime = lifeTime;
        _isExplosion = isExplosion;

        _startPosition = transform.position;
        transform.LookAt(inTargetPosition);
    }

    private void Explosion()
    {
        if (_isExplosion)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, targetMask);
            ProcessExplosionColliders(colliders);

            Utils.Destroy(Utils.Instantiate(explosionPrefab, transform.position, Quaternion.identity), 2.0f);
        }

        Utils.Destroy(gameObject);
    }

    private void ProcessExplosionColliders(Collider[] colliders)
    {
        // 12개의 미사일이 동시에 터져도, 이 코루틴은 각 미사일별로 실행되며,
        // yield return null; 덕분에 매 프레임마다 하나의 대상만 처리합니다.
        foreach (Collider collider in colliders)
        {
            // 데미지 적용
            if (collider)
            {
                TakeDamage(collider.transform);
            }
        }
    }

    protected virtual void TakeDamage(Transform target, float coefficient = 1.0f)
    {
        IDamagable enemy = target.GetComponent<IDamagable>();
        if (enemy != null)
        {
            Transform otherParent = target.transform;
            enemy.ApplyDamage(_damage * coefficient, targetMask);
        }
        else
        {
            enemy = target.transform.GetComponentInParent<IDamagable>();
            if (enemy != null)
            {
                Transform otherParent = target.transform;
                enemy.ApplyDamage(_damage * coefficient, targetMask);
            }
        }
    }
}
