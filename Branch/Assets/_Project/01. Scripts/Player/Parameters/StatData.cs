using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatType
{
    ID,
    Name,
    MaxHp,
    Damage,
    WalkSpeed,
    RunSpeed,
    Defence,
    Range,                  // 범위 (예: 적을 인식하는 거리)
    PartType,               // 파츠 타입 (예: 등/어깨, 팔, 다리)
    IntervalBetweenShots,   // 탄 발사 간격 시간값
    AddHp,
    AddDefence,
    AddMoveSpeed,
    CooldownReduction,      // 쿨타임 감소 값
    StatusEffectType,       // 상태이상 타입 (예: 중독, 기절 등)
    
    // 몬스터 스킬
    SkillType,            // 스킬 타입 (예: 근거리 공격, 원거리 공격, 방어 등)
    CooldownTime,         // 스킬 쿨타임
    AnimSpeed,          // 애니메이션 속도
    
    
    // TODO: 스킬은 다시 정의해야함
    // SkillDamage,
    // SkillSpeed,
    // SkillCount,
    // SkillCooldown,
    // CooldownDecrease,
    // Ailment,
}

// 개별 스탯 데이터 클래스
[System.Serializable]
public class StatData
{
    public EStatType statType;
    public string stringValue;
    public float value;
    public float minValue;
    public float maxValue;

    public StatData(EStatType type, /* string stringValue = "", */ float value = 0f, float min = float.MinValue, float max = float.MaxValue)
    {
        statType = type;
        minValue = min;
        maxValue = max;
        SetValue(value);
        // SetStringValue(stringValue);
    }

    public void SetValue(float newValue)
    {
        value = Mathf.Clamp(newValue, minValue, maxValue);
    }
    
    public void SetStringValue(string newStringValue)
    {
        stringValue = newStringValue;
    }

    public void AddValue(float add)
    {
        SetValue(value + add);
    }

    public float GetValue() => value;

    public StatData Clone()
    {
        return new StatData(statType, /* stringValue,*/ value, minValue, maxValue);
    }
}
