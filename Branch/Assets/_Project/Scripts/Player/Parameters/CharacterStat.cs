using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StatPair
{
    public string StatName;
    public float StatValue;

    public StatPair(string name, float value)
    {
        StatName = name;
        StatValue = value;
    }
}

public class CharacterStat : MonoBehaviour
{
    #region Variables
    [SerializeField, Tooltip("플레이어의 현재 파라미터")] private List<StatPair> _totalStatList = new();
    // private float _currentTotalHealth;
    private float _currentBodyHealth;
    private float _currentPartHealth;
    
    private StatDictionary _baseStats = new();                              // 캐릭터 기본 스탯
    private Dictionary<EPartType, StatDictionary> _partStatDict = new();    // 파츠 부위별 스탯
    private StatDictionary _totalStats;                                     // 캐릭터 + 파츠 스탯
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
    
    // 현재 체력값의 Getter/Setter
    public float CurrentHealth => _currentBodyHealth + _currentPartHealth;
    
    public float CurrentPartHealth
    {
        get => _currentPartHealth;
        set => _currentPartHealth = value;
    }
    public float CurrentBodyHealth
    {
        get => _currentBodyHealth;
        set => _currentBodyHealth = value;
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

        CalculateTotalStats();                              // 능력치 계산
        
        _currentBodyHealth = _baseStats[EStatType.MaxHp].Value; // 캐릭터의 바디 체력 값 초기화
        _currentPartHealth = CalculatePartHealth();             // 기본 파츠 체력 총합 초기화
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

        VisualizeStats();
    }

    private void VisualizeStats()
    {
        _totalStatList.Clear();

        foreach (StatData stat in _totalStats.StatDict.Values)
        {
            _totalStatList.Add(new StatPair(stat.StatType.ToString(), stat.Value));
        }
    }
    
    // 캐릭터가 착용 중인 파츠별 HP 값을 총합하여 반환
    private float CalculatePartHealth()
    {
        var result = 0f;
        
        foreach (StatDictionary partStats in _partStatDict.Values)
        {
            if (partStats.IsEmpty()) continue;

            foreach (EStatType type in System.Enum.GetValues(typeof(EStatType)))
            {
                if (partStats[type] == null)  continue;
                
                if (partStats[type].StatType == EStatType.MaxHp)
                    result += partStats[type].Value;
            }
        }
        
        return result;
    }

    private float CalculateBodyHealth()
    {
        return 0f;
    }
    
    #endregion
}
