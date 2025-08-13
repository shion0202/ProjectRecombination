using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStatModifierType
{
    Flat,           // 고정값 (+10 + 10)
    PercentAdd,     // 합연산 (+10% + 10%)
    PercentMul      // 곱연산 (×1.1 × 1.1)
}

public class StatModifier : MonoBehaviour
{
    public EStatType statType;
    public EStatModifierType modifierType;
    public float value;
    public object source;                       // 이 Modifier를 만든 객체 (버프, 스킬 등)

    public StatModifier(EStatType type, EStatModifierType modifierType, float value, object source = null)
    {
        statType = type;
        this.modifierType = modifierType;
        this.value = value;
        this.source = source;
    }
}
