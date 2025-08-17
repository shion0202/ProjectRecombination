using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoverLegs : PartLegsBase
{
    [Header("호버링 설정")]
    public float hoverHeight = 1.5f;
    public float hoverRange = 0.2f;
    public float hoverSpeed = 2.0f;
    public float hoverRiseSpeed = 2.0f;  // 상승 속도

    private float baseY;
    private float currentY;     // 현재 목표 hover 위치(상승 중)
    private float lastHoverY;
    private float hoverTime = 0f;
    private bool wasIdleLastFrame = false;

    protected override void Awake()
    {
        base.Awake();

        _partModifiers.Add(new StatModifier(EStatType.BaseMoveSpeed, EStatModifierType.PercentMul, 0.2f, this));
        _partModifiers.Add(new StatModifier(EStatType.Defence, EStatModifierType.Flat, 10, this));

        baseY = transform.position.y;
        currentY = baseY;  // 초기 위치부터 시작
        lastHoverY = baseY;
    }

    public override Vector3 GetMoveDirection(Vector2 moveInput, Transform characterTransform, Transform cameraTransform)
    {
        if (moveInput == Vector2.zero) return Vector3.zero;

        // 카메라 기준 이동 방향 처리
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        return moveDirection.normalized * _owner.Stats.TotalStats[EStatType.BaseMoveSpeed].value;
    }

    public float CalculateHoverDeltaY(bool isIdle, float groundY)
    {
        if (isIdle)
        {
            float targetY = groundY + hoverHeight;
            currentY = Mathf.MoveTowards(currentY, targetY, hoverRiseSpeed * Time.deltaTime);

            if (Mathf.Approximately(currentY, targetY))
            {
                hoverTime += Time.deltaTime;
            }
            else
            {
                hoverTime = 0f;
            }

            float offsetY = Mathf.Sin(hoverTime * hoverSpeed) * hoverRange;
            float hoverY = currentY + offsetY;

            float deltaY = hoverY - lastHoverY;
            lastHoverY = hoverY;

            wasIdleLastFrame = true;
            return deltaY;
        }
        else
        {
            wasIdleLastFrame = false;
            hoverTime = 0f;
            currentY = groundY;
            lastHoverY = currentY;
            return 0f;
        }
    }
}
