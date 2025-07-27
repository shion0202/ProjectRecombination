using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverLegs : PartLegsBase
{
    [SerializeField] private float baseSpeed = 9.0f;

    public override void UseAbility()
    {
        if (_currentSkillCount >= maxSkillCount) return;

        Boost();
    }

    private void Boost()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        // 부스트 시작
        // 현재는 Base Stat을 변경하지만, 추후 비슷한 기믹이 많아질 경우 Buffer Stat을 추가할 수 있음
        _owner.Stats.BaseStats[EStatType.MoveSpeed].Value = baseSpeed;
        _owner.Stats.CalculateStatsForced();
        _skillCoroutine = StartCoroutine(CoCooldownBoost());
        Debug.Log("부스트 시작. 기본 이동 속도가 증가합니다.");
    }

    protected IEnumerator CoCooldownBoost()
    {
        ++_currentSkillCount;

        yield return new WaitForSeconds(skillTime);

        // 부스트 종료
        _owner.Stats.BaseStats[EStatType.MoveSpeed].Reset();
        _owner.Stats.CalculateStatsForced();
        Debug.Log("부스트 종료. 기본 이동 속도가 Base Stat으로 돌아갑니다.");

        yield return new WaitForSeconds(skillCooldown * (_currentSkillCount));

        Debug.Log("부스트 쿨타임 종료. 다시 부스트를 사용할 수 있습니다.");
        _currentSkillCount = 0;
        _skillCoroutine = null;
    }
}
