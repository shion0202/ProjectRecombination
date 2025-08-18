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

    private float previousGroundY = 0f;
    [SerializeField] private float largeGroundYChangeThreshold = 0.3f; // 큰 높이 변화 임계값

    protected override void Awake()
    {
        base.Awake();

        _partModifiers.Add(new StatModifier(EStatType.BaseMoveSpeed, EStatModifierType.PercentMul, 0.2f, this));
        _partModifiers.Add(new StatModifier(EStatType.Defence, EStatModifierType.Flat, 10, this));
    }

    protected void OnEnable()
    {
        _currentMoveVelocity = Vector3.zero;
        previousGroundY = groundY;
        isInit = false;
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
        RaycastHit hit;
        Vector3 rayOrigin = _owner.transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 10f))
        {
            groundY = hit.point.y;
        }
        else
        {
            if (!isInit)
            {
                groundY = _owner.transform.position.y;
                isInit = true;
            }
        }

        float groundYChange = Mathf.Abs(groundY - previousGroundY);
        previousGroundY = groundY;

        // groundY 변화량이 임계치를 초과하면 호버링 오프셋 0으로 처리
        float hoverOffset = 0f;
        if (groundYChange < largeGroundYChangeThreshold)
        {
            hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
        }
        else
        {
            // 큰 변화 시에는 고정된 호버 높이 유지 (oscillation 끔)
            hoverOffset = 0f;
        }

        float targetY = groundY + hoverHeight + hoverOffset;
        Vector3 moveDelta = Vector3.zero;
        moveDelta.y = targetY - (_owner.transform.localPosition.y);

        return moveDelta;
    }
}
