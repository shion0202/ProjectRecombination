using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Monster;

public class CaterpillarLegs : PartLegsBase
{
    [Header("캐터필러 설정")]
    [SerializeField] protected GameObject impactEffectPrefab;
    [SerializeField] private float turnSpeed = 120.0f;
    private Vector3 _currentMoveDirection = Vector3.forward;

    protected override void Awake()
    {
        base.Awake();

        _partModifiers.Add(new StatModifier(EStatType.WalkSpeed, EStatModifierType.PercentMul, -0.2f, this));
        _partModifiers.Add(new StatModifier(EStatType.Defence, EStatModifierType.Flat, 10, this));

        _isAnimating = false;
    }

    protected void OnEnable()
    {
        if (_owner != null)
        {
            _currentMoveDirection = _owner.transform.forward;
        }
    }

    protected void OnDrawGizmos()
    {
        if (_owner == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_owner.transform.position, skillRange);
    }

    public override void UseAbility()
    {
        Impact();
    }

    public override void FinishActionForced()
    {
        base.FinishActionForced();
        _owner.SetMovable(true);
    }

    public override Vector3 GetMoveDirection(Vector2 moveInput, Transform characterTransform, Transform cameraTransform)
    {
        if (moveInput == Vector2.zero) return Vector3.zero;

        // 카메라 기준 목표 이동 방향 설정
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 targetDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        // 현재 하체 방향에서 목표 방향까지 서서히 lerp (캐터필러 느낌)
        _currentMoveDirection = Vector3.RotateTowards(
            _currentMoveDirection,
            targetDirection,
            turnSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f);

        HandleCylinderDirection(_currentMoveDirection);

        return _currentMoveDirection * _owner.Stats.TotalStats[EStatType.WalkSpeed].value; // 좌우가 서서히 꺾이는 이동방향
    }

    protected void Impact()
    {
        if (_skillCoroutine != null) return;
        _skillCoroutine = StartCoroutine(CoImpartRoutine());
    }

    public void HandleCylinderDirection(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.001f) return;

        // 이동 방향을 향하는 회전 값 생성 (y축 회전)
        Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);

        // 기본회전 오프셋을 곱해 줌 (나중에 기본Rotation을 적용)
        targetRot = targetRot * Quaternion.Euler(0.0f, 0.0f, 90.0f);

        // 현재 회전과 보간해서 부드럽게 회전 처리
        gameObject.transform.rotation = Quaternion.Slerp(
            gameObject.transform.rotation,
            targetRot,
            Time.deltaTime * 10f);
    }

    protected IEnumerator CoImpartRoutine()
    {
        Destroy(Instantiate(impactEffectPrefab, _owner.transform.position, Quaternion.identity), 5.0f);

        // 적 탐지 및 데미지 적용
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, skillRange);
        foreach (Collider hit in hitColliders)
        {
            MonsterBase monster = hit.transform.GetComponent<MonsterBase>();
            if (monster != null)
            {
                monster.TakeDamage((int)skillDamage);
                // TODO: 적 기절 (2초)
            }
            else
            {
                monster = hit.transform.GetComponentInParent<MonsterBase>();
                if (monster != null)
                {
                    monster.TakeDamage((int)skillDamage);
                    // TODO: 적 기절 (2초)
                }
            }
        }

        // 파츠 HP 모두 회복
        _owner.HealHp(9999, EHealRange.Part);
        Debug.Log("캐터필러 스킬 효과: 파츠 Hp 모두 회복");

        // 이동 불가 적용
        _owner.SetMovable(false);
        Debug.Log("캐터필러 스킬 효과: 이동 불가");
        yield return new WaitForSeconds(skillDuration);
        _owner.SetMovable(true);                            // skillDuration 끝나면 다시 이동 가능
        Debug.Log("캐터필러 스킬 효과: 이동 불가 해제");

        // 쿨타임 시작
        yield return new WaitForSeconds(skillCooldown);
        _skillCoroutine = null;
        Debug.Log("캐터필러 스킬 쿨타임 종료");
    }
}
