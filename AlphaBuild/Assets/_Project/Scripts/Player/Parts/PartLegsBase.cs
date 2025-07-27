using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartLegsBase : PartBase
{
    // ��� �ӵ� or ��� ���� �ð� ������ �ʿ�
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

    // ��� ��ü �� Ư���� ��Ȳ���� ��ø� ������ �����ؾ� �� �� ���
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
