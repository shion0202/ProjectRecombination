using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Monster;
using UnityEngine;
using Unity.VisualScripting;

public class ArmLaserCharge : PartBaseArm
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject effect;
    [SerializeField] private Vector3 defaultImpulseValue = new Vector3(0.0f, 0.0f, 0.0f);
    private float maxAlphas = 1.0f;
    private Coroutine fadeCoroutine = null;
    private Color originalColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        originalColor = lineRenderer.material.color;
    }

    protected override void Update()
    {
        if (!_isShooting) return;

        _currentShootTime += Time.deltaTime;
    }

    public override void UseAbility()
    {
        base.UseAbility();

        effect.SetActive(true);
    }

    public override void UseCancleAbility()
    {
        _isShooting = false;

        Shoot();
        _currentShootTime = 0.0f;
    }

    protected override void Shoot()
    {
        if (_currentShootTime > 1.0f)
        {
            // Max 값
            _currentShootTime = 1.0f;
        }

        Vector3 targetPoint = Vector3.zero;
        RaycastHit[] hits = GetMultiTargetPoint(out targetPoint);

        effect.SetActive(false);

        lineRenderer.material.color = originalColor;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, bulletSpawnPoint.position);

        if (hits.Length > 0)
        {
            lineRenderer.SetPosition(1, targetPoint);
        }
        else
        {
            Vector3 camShootDirection = (targetPoint - bulletSpawnPoint.position).normalized;
            lineRenderer.SetPosition(1, bulletSpawnPoint.position + camShootDirection * 100.0f);
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        fadeCoroutine = StartCoroutine(CoFadeOutLaser());

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                MonsterBase monster = hit.transform.GetComponent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage((int)_owner.Stats.TotalStats[EStatType.Attack].Value);
                }
                else
                {
                    monster = hit.transform.GetComponentInParent<MonsterBase>();
                    if (monster != null)
                    {
                        monster.TakeDamage((int)_owner.Stats.TotalStats[EStatType.Attack].Value);
                    }
                }

                Destroy(Instantiate(bulletPrefab, hit.point, Quaternion.identity), 0.5f);
            }
        }

        impulseSource.m_DefaultVelocity = defaultImpulseValue * _currentShootTime;
        _owner.ApplyRecoil(impulseSource, recoilX * _currentShootTime, recoilY * _currentShootTime);
    }

    private IEnumerator CoFadeOutLaser()
    {
        while (lineRenderer.material.color.a > 0.0f)
        {
            Color c = lineRenderer.material.color;
            c.a -= Time.deltaTime;
            lineRenderer.material.color = c;

            yield return null;

            if (lineRenderer.material.color.a <= 0.0f)
            {
                lineRenderer.enabled = false;
                fadeCoroutine = null;
                break;
            }
        }
    }
}
