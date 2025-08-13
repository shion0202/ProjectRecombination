using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

[System.Serializable]
public class StatListView
{
    public List<StatData> stats = new List<StatData>();

    public void FromDictionary(StatDictionary dict)
    {
        stats.Clear();
        foreach (var s in dict.GetAllStats())
            stats.Add(s.Clone());
    }

    public StatDictionary ToDictionary()
    {
        var dict = new StatDictionary();
        foreach (var s in stats)
            dict.SetStat(s.Clone());
        return dict;
    }
}

[System.Serializable]
public class PartStatView
{
    public EPartType partType;
    public StatListView stats = new StatListView();
}

public class CharacterStat : MonoBehaviour
{
    #region Variables
    [Header("플레이어의 종합 능력치")]
    [SerializeField] private StatListView totalStatsView = new StatListView();

    [Header("플레이어의 기본 능력치")]
    [SerializeField] private StatListView baseStatsView = new StatListView();

    [Header("플레이어가 착용 중인 파츠 능력치")]
    [SerializeField] private List<PartStatView> partStatsView = new List<PartStatView>();

    [Header("플레이어에게 적용된 효과 목록")]
    [SerializeField] private List<StatModifier> modifiersView = new List<StatModifier>();

    // private float _currentTotalHealth;
    private float _currentBodyHealth;
    private float _currentPartHealth;
    
    private StatDictionary _baseStats = new();                              // 캐릭터 기본 스탯
    private Dictionary<EPartType, StatDictionary> _partStatDict = new();    // 파츠 부위별 스탯
    private List<StatModifier> _modifiers = new();                          // 버프, 스킬 등 기타 스탯
    private StatDictionary _totalStats;                                     // 캐릭터 + 파츠 + 기타 스탯
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
        // To-do: 베이스 파츠는 기본 장착 파츠이므로 이를 기준으로 초기화하도록 수정 가능
        foreach (EPartType type in Enum.GetValues(typeof(EPartType)))
        {
            if (_partStatDict.ContainsKey(type)) continue;
            _partStatDict[type] = new StatDictionary();
        }

        // Base Stat 초기화
        GoogleSheetLoader baseParam = Resources.Load<GoogleSheetLoader>("Params/ParamData_CharacterStats");
        if (baseParam != null && baseParam.DataDict != null && baseParam.DataDict.Count > 0)
        {
            InitializeFromRow(baseParam.DataDict[4001]);
        }
        else
        {
            Debug.LogWarning("Base Stat 데이터가 없습니다.");
        }
        
        _currentBodyHealth = _baseStats[EStatType.MaxHp].value;     // 캐릭터의 바디 체력 값 초기화
        _currentPartHealth = CalculatePartHealth();                 // 기본 파츠 체력 총합 초기화

        SyncToInspector();
    }
    #endregion

    #region Public Methods
    public void InitializeFromRow(RowData row)
    {
        _baseStats = row.Stats.Clone();
        CalculateTotalStats();
    }

    // 파츠 교체 시 실행되는 함수
    // 파츠 교체와 함께 Total Stat을 갱신
    public void SetPartStats(EPartType type, StatDictionary partStats)
    {
        _partStatDict[type] = partStats ?? new StatDictionary();
        CalculateTotalStats();
    }

    public void AddModifier(StatModifier mod)
    {
        _modifiers.Add(mod);
        CalculateTotalStats();
    }

    public void RemoveModifierFromSource(object source)
    {
        _modifiers.RemoveAll(m => m.source == source);
        CalculateTotalStats();
    }

    public void CalculateStatsForced()
    {
        CalculateTotalStats();
    }

    // 동기화 메서드들
    public void SyncToInspector()
    {
        baseStatsView.FromDictionary(_baseStats);
        totalStatsView.FromDictionary(_totalStats);

        partStatsView.Clear();
        foreach (var kvp in _partStatDict)
        {
            var ps = new PartStatView { partType = kvp.Key };
            ps.stats.FromDictionary(kvp.Value);
            partStatsView.Add(ps);
        }

        modifiersView = new List<StatModifier>(_modifiers);
    }

    public void SyncFromInspector()
    {
        _baseStats = baseStatsView.ToDictionary();

        _partStatDict.Clear();
        foreach (var pv in partStatsView)
            _partStatDict[pv.partType] = pv.stats.ToDictionary();

        _modifiers = new List<StatModifier>(modifiersView);

        CalculateTotalStats();
    }
    #endregion

    #region Private Methods
    // Total Stat(최종적으로 사용하는 스탯)을 갱신하는 함수
    private void CalculateTotalStats()
    {
        // Stat Type 중 Base Stat에 해당하는 스탯을 Total Stats에 추가
        _totalStats = _baseStats.Clone();

        // Stat Type 중 Part Stat에 해당하는 스탯을 Total Stats에 추가
        // 이미 Base Stat을 통해 추가된 상태라면 값만 갱신
        foreach (var partStats in _partStatDict.Values)
        {
            foreach (var stat in partStats.GetAllStats())
            {
                var target = _totalStats[stat.statType];
                if (target != null)
                {
                    target.AddValue(stat.value);
                }
                else
                {
                    _totalStats.SetStat(stat);
                }
            }
        }

        // Modifiers 적용 (Flat → PercentAdd → PercentMul 순)
        foreach (var stat in _totalStats.GetAllStats())
        {
            float finalValue = stat.value;

            foreach (var mod in _modifiers.Where(m => m.statType == stat.statType && m.modifierType == EStatModifierType.Flat))
            {
                finalValue += mod.value;
            }
            foreach (var mod in _modifiers.Where(m => m.statType == stat.statType && m.modifierType == EStatModifierType.PercentAdd))
            {
                finalValue += stat.value * mod.value;
            }
            foreach (var mod in _modifiers.Where(m => m.statType == stat.statType && m.modifierType == EStatModifierType.PercentMul))
            {
                finalValue *= (1 + mod.value);
            }

            stat.SetValue(finalValue);
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
                
                if (partStats[type].statType == EStatType.MaxHp)
                    result += partStats[type].value;
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
