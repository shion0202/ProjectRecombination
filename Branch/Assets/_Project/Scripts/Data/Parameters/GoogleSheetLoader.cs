using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

public enum EValueType
{
    Float,
    Int,
}

[System.Serializable]
public class RowData
{
    [System.Serializable]
    public struct StatEntry
    {
        public EStatType key;
        public StatData value;
    }

    [SerializeField] private List<StatEntry> statEntries = new List<StatEntry>();
    private StatDictionary _stats;
    private bool _isDirty = true;

    public StatDictionary Stats
    {
        get
        {
            if (_isDirty || _stats == null)
            {
                BuildDictionary();
            }
            return _stats;
        }
    }

    private void BuildDictionary()
    {
        _stats = new StatDictionary();
        foreach (var entry in statEntries)
        {
            _stats.SetStat(entry.value);
        }
        _isDirty = false;
    }

    public float GetStat(EStatType key)
    {
        var stat = Stats[key];
        return stat != null ? stat.value : 0.0f;
    }

    public void SetStat(EStatType key, float value)
    {
        var idx = statEntries.FindIndex(e => e.key == key);
        var newData = new StatData(key, value);

        if (idx >= 0)
        {
            statEntries[idx] = new StatEntry { key = key, value = newData };
        }
        else
        {
            statEntries.Add(new StatEntry { key = key, value = newData });
        }

        _isDirty = true;
    }
}

[CreateAssetMenu(fileName = "ParamData", menuName = "Scriptable Object/Parameter Data", order = 21)]
public class GoogleSheetLoader : ScriptableObject
{
    private static readonly Dictionary<string, EStatType> headerToStatType =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "Index", EStatType.Index },
            { "HP", EStatType.MaxHp },
            { "Damage", EStatType.Attack },
            { "MinSpeed", EStatType.MinimumMoveSpeed },
            { "MaxSpeed", EStatType.MaximumMoveSpeed },
            { "AttackSpeed", EStatType.AttackSpeed },
            { "Defence", EStatType.Defence },
            { "DetectiveRange", EStatType.DetectiveRange },

            { "partsID", EStatType.Index },
            { "partsType", EStatType.PartType },
            { "attack", EStatType.Attack },
            { "fireRapid", EStatType.AttackSpeed },
            { "addHp", EStatType.MaxHp },
            { "addDefence", EStatType.Defence },
            { "addSpeed", EStatType.MoveSpeed },
            { "cooldownDecrease", EStatType.CooldownDecrease },
            { "attackSkillDamage", EStatType.SkillDamage },
            { "skillSpeed", EStatType.SkillSpeed },
            { "skillCount", EStatType.SkillCount },
            { "skillCooldown", EStatType.SkillCooldown },
            { "attackAilment", EStatType.Ailment },
            // 필요하면 계속 추가
        };

    [Header("시트 주소")]
    [SerializeField] private string sheetLink;

    [Header("스프레드시트 GID")]
    [SerializeField] private string workSheetGid;

    [Header("불러온 데이터 목록")]
    [SerializeField] private List<RowData> datas = new();

    // 런타임 전용 Dictionary (Index → RowData)
    private Dictionary<int, RowData> _dataDict;
    public Dictionary<int, RowData> DataDict => _dataDict;

    public RowData GetRow(int index)
    {
        if (_dataDict != null && _dataDict.TryGetValue(index, out var row))
            return row;
        return null;
    }

    private void OnEnable()
    {
        if (_dataDict == null && datas != null && datas.Count > 0)
            BuildDictionary();
    }

    private void BuildDictionary()
    {
        _dataDict = new Dictionary<int, RowData>();
        for (int i = 0; i < datas.Count; i++)
        {
            _dataDict[(int)datas[i].GetStat(EStatType.Index)] = datas[i];
        }
    }

    private string GetSheetURL()
    {
        string sheetUrl = $"https://docs.google.com/spreadsheets/d/{sheetLink}/export?format=csv&gid={workSheetGid}";
        return sheetUrl;
    }

    private void ParseCsv(string csv)
    {
        datas.Clear();
        string[] lines = csv.Replace("\r", "").Split('\n');
        if (lines.Length < 2) return;
        string[] headers = lines[0].Trim().Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var values = line.Split(',');
            var row = new RowData();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                // 매핑 딕셔너리 우선 사용
                // 매핑에 없으면 Enum.TryParse 시도
                if (!headerToStatType.TryGetValue(headers[j], out var statType) && !Enum.TryParse(headers[j], true, out statType))
                {
                    Debug.LogWarning($"[Row {i}] 알 수 없는 StatType: {headers[j]}");
                    continue;
                }

                string raw = values[j];

                if (float.TryParse(raw, out float f))
                {
                    row.SetStat(statType, f);
                }
                else
                {
                    Debug.LogWarning($"[Row {i}] '{headers[j]}' 값 '{raw}'은 숫자가 아닙니다.");
                }
            }

            datas.Add(row);
        }

        BuildDictionary();
    }

#if UNITY_EDITOR
    // 에디터 버튼에서 실행할 메서드
    public void LoadDataFromSpreadSheet_Editor()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(CoLoadDataFromSpreadSheet());
    }

    public void ClearData_Editor()
    {
        datas.Clear();
        EditorUtility.SetDirty(this);
    }
#endif

    private IEnumerator CoLoadDataFromSpreadSheet()
    {
        using var www = UnityWebRequest.Get(GetSheetURL());
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ParseCsv(www.downloadHandler.text);
            Debug.Log($"Google Sheet Data Loaded: {datas.Count} rows");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
        else Debug.LogError($"Failed to load sheet: {www.error}");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GoogleSheetLoader))]
public class GoogleSheetDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var so = (GoogleSheetLoader)target;

        GUILayout.Space(10);

        if (GUILayout.Button("스프레드시트로부터 데이터 불러오기"))
        {
            so.LoadDataFromSpreadSheet_Editor();
        }

        if (GUILayout.Button("데이터 초기화"))
        {
            if (EditorUtility.DisplayDialog("데이터 초기화",
                "저장된 모든 데이터를 삭제하시겠습니까?", "네", "아니요"))
            {
                so.ClearData_Editor();
            }
        }
    }
}
#endif
