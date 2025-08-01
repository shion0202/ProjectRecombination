using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverLegs : PartLegsBase
{
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private float baseSpeed = 9.0f;
    private GameObject effect = null;

    public override void UseAbility()
    {
        if (_currentSkillCount >= maxSkillCount) return;

        Boost();
    }

    public override void FinishActionForced()
    {
        base.FinishActionForced();
        Destroy(effect);
    }

    private void Boost()
    {
        if (_skillCoroutine != null)
        {
            StopCoroutine(_skillCoroutine);
            _skillCoroutine = null;
        }

        // �ν�Ʈ ����
        // ����� Base Stat�� ����������, ���� ����� ����� ������ ��� Buffer Stat�� �߰��� �� ����
        _owner.Stats.BaseStats[EStatType.MoveSpeed].Value = baseSpeed;
        _owner.Stats.CalculateStatsForced();
        _skillCoroutine = StartCoroutine(CoCooldownBoost());
        
        effect = Instantiate(effectPrefab, _owner.transform.position, Quaternion.identity, _owner.transform);
    }

    protected IEnumerator CoCooldownBoost()
    {
        ++_currentSkillCount;

        yield return new WaitForSeconds(skillTime);

        // �ν�Ʈ ����
        _owner.Stats.BaseStats[EStatType.MoveSpeed].Reset();
        _owner.Stats.CalculateStatsForced();
        Destroy(effect);

        yield return new WaitForSeconds(skillCooldown * (_currentSkillCount));

        Debug.Log("�ν�Ʈ ��Ÿ�� ����. �ٽ� �ν�Ʈ�� ����� �� �ֽ��ϴ�.");
        _currentSkillCount = 0;
        _skillCoroutine = null;
    }
}
