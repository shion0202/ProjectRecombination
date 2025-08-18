using _Project.Scripts.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class HoverLegs : PartLegsBase
{
    [Header("호버링 설정")]
    private Vector3 _currentMoveVelocity = Vector3.zero;
    [SerializeField] protected float acceleration = 10f; // 클수록 즉각적, 작을수록 둔함
    [SerializeField] protected float deceleration = 14f; // 클수록 급브레이크, 작을수록 천천히 멈춤

    public float groundY = 0.0f;
    public float hoverHeight = 1.5f;
    public float hoverRange = 0.2f;
    public float hoverSpeed = 2.0f;
    public bool isInit = false;

    protected override void Awake()
    {
        base.Awake();

        _partModifiers.Add(new StatModifier(EStatType.BaseMoveSpeed, EStatModifierType.PercentMul, 0.2f, this));
        _partModifiers.Add(new StatModifier(EStatType.Defence, EStatModifierType.Flat, 10, this));
    }

    protected void OnEnable()
    {
        _currentMoveVelocity = Vector3.zero;
    }

    public override Vector3 GetMoveDirection(Vector2 moveInput, Transform characterTransform, Transform cameraTransform)
    {
        // 카메라 기준으로 입력 방향 구하기
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 inputDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        float moveSpeed = _owner.Stats.TotalStats[EStatType.BaseMoveSpeed].value;
        Vector3 targetVelocity = inputDir * moveSpeed;

        float accel = (inputDir.sqrMagnitude > 0.01f) ? acceleration : deceleration; // 멈출 땐 감속값

        // 감가속(관성) 처리
        _currentMoveVelocity = Vector3.MoveTowards(_currentMoveVelocity, targetVelocity, accel * Time.deltaTime);

        return _currentMoveVelocity;
    }

    public Vector3 CalculateHoverDeltaY()
    {
        if (!isInit)
        {
            groundY = _owner.transform.position.y;
            isInit = true;
        }

        Vector3 moveDelta = Vector3.zero;

        float offsetY = Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
        // 기존 위치에서 Y축만 움직임 적용
        float targetY = groundY + hoverHeight + offsetY;
        moveDelta.y = targetY - (_owner.transform.localPosition.y);

        return moveDelta;
    }
}
