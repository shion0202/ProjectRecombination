using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    private StatDictionary _baseStats = new();
    private Dictionary<EPartType, StatDictionary> _partStatDict = new();
    private StatDictionary _totalStats = new();

    public StatDictionary BaseStats
    {
        get { return _baseStats; }
        set { _baseStats = value; }
    }
    
    public Dictionary<EPartType, StatDictionary> PartStatDict
    {
        get { return _partStatDict; }
    }

    public StatDictionary TotalStats
    {
        get { return _totalStats; }
        set { _totalStats = value; }
    }

    private void Awake()
    {
        // partStat은 파츠 부위 종류만큼 미리 생성
        // 또는 추후에 베이스 파츠 기준으로 초기화
        foreach (EPartType type in Enum.GetValues(typeof(EPartType)))
        {
            _partStatDict.Add(type, new StatDictionary());
        }
    }

    private void SetPartStats(EPartType type, StatDictionary partStats)
    {
        _partStatDict[type] = partStats;
        CalculateTotalStats();
    }

    // 파츠 교체 시 실행
    private void CalculateTotalStats()
    {
        _totalStats = new();

        // Base 스탯 복사
        foreach (EStatType type in System.Enum.GetValues(typeof(EStatType)))
        {
            StatData baseData = _baseStats[type];
            if (baseData != null)
            {
                _totalStats.Add(type, new StatData(
                    baseData.StatType,
                    baseData.DefaultValue,
                    baseData.MaxValue,
                    baseData.MinValue
                ));
            }
        }

        // 파츠 스탯 합산
        foreach (var part in _partStatDict.Values)
        {
            foreach (EStatType type in System.Enum.GetValues(typeof(EStatType)))
            {
                var stat = part[type];
                if (stat != null)
                {
                    if (_totalStats[type] == null)
                    {
                        _totalStats.Add(type, new StatData(
                            stat.StatType,
                            stat.DefaultValue,
                            stat.MaxValue,
                            stat.MinValue
                        ));
                    }
                    else
                    {
                        _totalStats[type].Modify(stat.Value);
                    }
                }
            }
        }
    }
}
