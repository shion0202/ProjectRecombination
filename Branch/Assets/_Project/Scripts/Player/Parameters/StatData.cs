using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatType
{
    Index,
    MaxHp,
    Attack,
    BaseMoveSpeed,
    BattleMoveSpeed,
    AttackSpeed,
    Defence,
    DetectiveRange,

    PartType,
    SkillDamage,
    SkillSpeed,
    SkillCount,
    SkillCooldown,
    CooldownDecrease,
    Ailment,
}

// 개별 스탯 데이터 클래스
[System.Serializable]
public class StatData
{
    public EStatType statType;
    public float value;
    public float minValue;
    public float maxValue;

    public StatData(EStatType type, float value, float min = float.MinValue, float max = float.MaxValue)
    {
        statType = type;
        minValue = min;
        maxValue = max;
        SetValue(value);
    }

    public void SetValue(float newValue)
    {
        value = Mathf.Clamp(newValue, minValue, maxValue);
    }

    public void AddValue(float add)
    {
        SetValue(value + add);
    }

    public float GetValue() => value;

    public StatData Clone()
    {
        return new StatData(statType, value, minValue, maxValue);
    }
}
