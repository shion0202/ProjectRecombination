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
        // partStat�� ���� ���� ������ŭ �̸� ����
        // �Ǵ� ���Ŀ� ���̽� ���� �������� �ʱ�ȭ
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

    // ���� ��ü �� ����
    private void CalculateTotalStats()
    {
        _totalStats = new();

        // Base ���� ����
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

        // ���� ���� �ջ�
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
