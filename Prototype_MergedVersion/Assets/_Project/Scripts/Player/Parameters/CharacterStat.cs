using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    #region Variables
    private StatDictionary _baseStats = new();
    private Dictionary<EPartType, StatDictionary> _partStatDict = new();
    private StatDictionary _totalStats;
    #endregion

    #region Properties
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
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Part Stat은 파츠 부위 종류만큼 미리 생성
        // To-do: 추후에 베이스 파츠 기준으로 초기화하는 방식으로 수정 가능
        foreach (EPartType type in Enum.GetValues(typeof(EPartType)))
        {
            if (_partStatDict.ContainsKey(type)) continue;
            _partStatDict.Add(type, new StatDictionary());
        }

        // Base Stat 초기화
        CharacterParamData baseParam = Resources.Load<CharacterParamDataReader>("Params/CharacterParamData").DataList[0];
        foreach (EStatType type in Enum.GetValues(typeof(EStatType)))
        {
            _baseStats.Add(type, new StatData(type, baseParam[type], 9999, -9999));
        }

        CalculateTotalStats();
    }
    #endregion

    #region Public Methods
    // 파츠 교체 시 실행되는 함수
    // 파츠 교체와 함께 Total Stat을 갱신
    public void SetPartStats(EPartType type, StatDictionary partStats)
    {
        _partStatDict[type] = partStats;
        CalculateTotalStats();
    }

    public void CalculateStatsForced()
    {
        CalculateTotalStats();
    }
    #endregion

    #region Private Methods
    // Total Stat(최종적으로 사용하는 스탯)을 갱신하는 함수
    private void CalculateTotalStats()
    {
        _totalStats = new();

        // Stat Type 중 Base Stat에 해당하는 스탯을 Total Stats에 추가
        foreach (EStatType type in System.Enum.GetValues(typeof(EStatType)))
        {
            StatData baseData = _baseStats[type];
            if (baseData != null)
            {
                _totalStats.Add(type, new StatData(
                    baseData.StatType,
                    baseData.Value,
                    baseData.MaxValue,
                    baseData.MinValue
                ));
            }
        }

        // Stat Type 중 Part Stat에 해당하는 스탯을 Total Stats에 추가
        // 이미 Base Stat을 통해 추가된 상태라면 Modify 함수를 사용하여 값만 갱신
        foreach (StatDictionary partStats in _partStatDict.Values)
        {
            if (partStats.IsEmpty()) continue;

            foreach (EStatType type in System.Enum.GetValues(typeof(EStatType)))
            {
                StatData stat = partStats[type];
                if (stat != null)
                {
                    if (_totalStats[type] == null)
                    {
                        _totalStats.Add(type, new StatData(
                            stat.StatType,
                            stat.Value,
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
    #endregion
}
