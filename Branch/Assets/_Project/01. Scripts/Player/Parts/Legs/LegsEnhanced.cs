using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegsEnhanced : PartBaseLegs
{
    [Header("롤러 설정")]
    [SerializeField] private float accelRate = 8f;          // 가속 속도 (높을수록 빠른 출발)
    [SerializeField] private float decelRate = 5f;          // 감속 속도 (높을수록 빠른 멈춤)
    [SerializeField] private float turnSpeed = 90.0f;       // 선회 속도 (초당 도)
    private Vector3 _currentDirection = Vector3.forward;    // 이동 방향 보관
    private float _currentSpeed = 0.0f;

    protected void OnEnable()
    {
        _currentSpeed = 0.0f;

        if (_owner != null)
        {
            _currentDirection = _owner.transform.forward;
        }
    }

    public override Vector3 GetMoveDirection(Vector2 moveInput, Transform characterTransform, Transform cameraTransform)
    {
        if (_currentSpeed <= 0.0f)
        {
            _currentDirection = _owner.transform.forward;
        }

        // 목표 방향
        Vector3 targetDir = Vector3.zero;
        float targetSpeed = 0f;

        if (moveInput != Vector2.zero)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            targetDir = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            targetSpeed = (_owner.Stats.TotalStats[EStatType.WalkSpeed].value + _owner.Stats.TotalStats[EStatType.AddMoveSpeed].value);
        }
        else
        {
            // 입력 없을 때, 정지 방향은 현재 방향 유지
            targetDir = _currentDirection;
            targetSpeed = 0f;
        }

        // 선회 처리 (캐터필러와 동일)
        _currentDirection = Vector3.RotateTowards(
            _currentDirection,
            targetDir,
            turnSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f);

        // 가속/감속 처리
        if (_currentSpeed < targetSpeed)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, accelRate * Time.deltaTime);
        }
        else
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, decelRate * Time.deltaTime);
        }

        // 실제 이동 벡터 반환
        return _currentDirection * _currentSpeed;
    }
}
