using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartLegsBase : PartBase
{
    // 대시 속도 or 대시 지속 시간 정보가 필요
    [SerializeField] protected float dashDistance = 5.0f;
    [SerializeField] protected float skillTime = 0.5f;
    [SerializeField] protected float skillCooldown = 3.0f;
    [SerializeField] protected int maxSkillCount = 1;
    protected int _currentSkillCount = 0;
    protected Coroutine _skillCoroutine = null;

    public override void UseAbility()
    {
        if (_currentSkillCount >= maxSkillCount) return;

        Dash();
    }

    public override void UseCancleAbility()
    {
        // 대시 취소 시 필요한 로직이 있다면 여기에 작성
        // 예를 들어, 대시 중에 다른 행동을 할 수 있도록 하는 로직 등
    }

    // 장비 교체 등 특수한 상황에서 대시를 강제로 종료해야 할 때 사용
    public override void FinishActionForced()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        _currentSkillCount = 0;
        _owner.FinishDash();
    }

    protected void Dash()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        _owner.Dash(dashDistance / skillTime);
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
