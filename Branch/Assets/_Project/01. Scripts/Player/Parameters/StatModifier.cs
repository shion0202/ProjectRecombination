using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStackType
{
    Flat,           // 고정값 (+10 + 10)
    PercentAdd,     // 합연산 (+10% + 10%)
    PercentMul      // 곱연산 (×1.1 × 1.1)
}

[Serializable]
public class StatModifier
{
    public EStatType statType;
    public EStackType modifierType;
    public float value;
    public object source;                       // 이 Modifier를 만든 객체 (버프, 스킬 등)

    public StatModifier(EStatType type, EStackType modifierType, float value, object source = null)
    {
        statType = type;
        this.modifierType = modifierType;
        this.value = value;
        this.source = source;
    }
}
