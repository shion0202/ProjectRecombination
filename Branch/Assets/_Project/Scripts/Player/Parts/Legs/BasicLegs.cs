using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BasicLegs : PartLegsBase
{
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
        return moveDirection.normalized * _owner.Stats.TotalStats[EStatType.BaseMoveSpeed].value;
    }

    protected void Dash()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        _owner.Dash(skillRange / skillTime);
        _skillCoroutine = StartCoroutine(CoHandleDash());
    }

    protected IEnumerator CoHandleDash()
    {
        ++_currentSkillCount;

        yield return new WaitForSeconds(skillTime);

        _owner.FinishDash();

        yield return new WaitForSeconds(skillCooldown * (_currentSkillCount));

        _currentSkillCount = 0;
        _skillCoroutine = null;
    }
}
