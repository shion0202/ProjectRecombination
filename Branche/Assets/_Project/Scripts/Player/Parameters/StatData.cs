using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatType
{
    // CurrentHealth,  // 현재 HP
    MaxHp,          // 최대 HP
    Attack,
    FireSpeed,
    Defence,
    MoveSpeed,
    AttackSkillDamage,
    SkillSpeed,
    SkillCount,
    SkillCooldown,
    CooldownDecrease,
    AttackAilment,
}

// 개별 스탯 데이터 클래스
public class StatData
{
    private EStatType _statType;
    private float _defaultValue;
    private float _minValue;
    private float _maxValue;
    private float _value;

    public EStatType StatType { get { return _statType; } }
    public float DefaultValue { get { return _defaultValue; } }
    public float MinValue { get {  return _minValue; } }
    public float MaxValue { get { return _maxValue; } }

    public float Value
    {
        get => _value;
        set => _value = Mathf.Clamp(value, _minValue, _maxValue);
    }

    public StatData(EStatType statType, float defaultValue, float maxValue, float minValue)
    {
        _statType = statType;
        _defaultValue = defaultValue;
        _minValue = minValue;
        _maxValue = maxValue;

        _value = _defaultValue;
    }

    // 스탯 값의 증감 함수
    // 단순히 스탯 값을 변경하는 것은 Property로 처리
    public void Modify(float newValue)
    {
        _value = Mathf.Clamp(_value + newValue, _minValue, _maxValue);
    }

    public void Reset()
    {
        _value = _defaultValue;
    }
}
