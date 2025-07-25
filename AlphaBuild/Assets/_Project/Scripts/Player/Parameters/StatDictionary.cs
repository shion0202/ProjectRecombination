using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 개별 스탯 데이터들을 Dictionary 형태로 관리하는 클래스
public class StatDictionary
{
    private Dictionary<EStatType, StatData> _statDict = new();

    // Indexer 문법: 외부에서 StatDictionary[EStatType] 형태로 접근 가능
    public StatData this[EStatType type]
    {
        get => _statDict[type];
    }

    public void Add(EStatType type, StatData data)
    {
        _statDict.Add(type, data);
    }

    public bool IsEmpty()
    {
        return _statDict.Count == 0;
    }
}
