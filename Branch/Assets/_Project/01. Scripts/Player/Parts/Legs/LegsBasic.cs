using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class LegsBasic : PartBaseLegs
{
    [SerializeField] protected GameObject dashEffectPrefab;

    protected override void Awake()
    {
        base.Awake();
        _legsAnimType = EAnimationType.Base;
    }

    public override void UseAbility()
    {
        if (_currentSkillCount >= maxSkillCount) return;
        Dash();
    }

    public override void FinishActionForced()
    {
        base.FinishActionForced();
        _owner.FinishDash();
    }

    public override Vector3 GetMoveDirection(Vector2 moveInput, Transform characterTransform, Transform cameraTransform)
    {
        if (moveInput == Vector2.zero) return Vector3.zero;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;
        return moveDirection.normalized * (_owner.Stats.TotalStats[EStatType.WalkSpeed].value + _owner.Stats.TotalStats[EStatType.AddMoveSpeed].value);
    }

    protected void Dash()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        LookCameraDirection();
        _owner.Dash(skillRange / skillTime);
        _skillCoroutine = StartCoroutine(CoHandleDash());
    }

    protected void LookCameraDirection()
    {
        Camera cam = Camera.main;
        Vector3 lookDirection = cam.transform.forward;
        lookDirection.y = 0; // 수평 방향으로만 회전
        if (lookDirection != Vector3.zero)
            _owner.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    protected IEnumerator CoHandleDash()
    {
        Vector3 direction = -_owner.DashDirection.normalized;
        Quaternion effectRotation = Quaternion.LookRotation(direction, Vector3.up);
        GameObject go = Instantiate(dashEffectPrefab, _owner.transform.position + (Vector3.up * 1.0f) + (direction * -3.0f), effectRotation);
        Destroy(go, 2.0f);
        ++_currentSkillCount;

        yield return new WaitForSeconds(skillTime);

        _owner.FinishDash();

        yield return new WaitForSeconds((skillCooldown * (_currentSkillCount)) - _owner.Stats.TotalStats[EStatType.CooldownReduction].value);

        _currentSkillCount = 0;
        _skillCoroutine = null;
    }
}
