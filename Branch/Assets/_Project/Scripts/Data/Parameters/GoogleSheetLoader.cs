using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

[System.Serializable]
public class StatValue
{
    public enum ValueType { Float, Int, String, Bool }

    public ValueType Type;
    public float floatValue;
    public int intValue;
    public string stringValue;
    public bool boolValue;

    public object GetValue()
    {
        return Type switch
        {
            ValueType.Float => floatValue,
            ValueType.Int => intValue,
            ValueType.String => stringValue,
            ValueType.Bool => boolValue,
            _ => null
        };
    }

    public T GetValue<T>()
    {
        return (T)GetValue();
    }
}

[System.Serializable]
public class RowData
{
    [System.Serializable]
    public struct StatEntry
    {
        public EStatType key;
        public StatValue value;
    }

    [SerializeField] private List<StatEntry> statEntries = new List<StatEntry>();
    private Dictionary<EStatType, StatValue> _stats;

    public Dictionary<EStatType, StatValue> Stats
    {
        get
        {
            if (_stats == null)
            {
                _stats = new Dictionary<EStatType, StatValue>();
                foreach (var entry in statEntries)
                    _stats[entry.key] = entry.value;
            }
            return _stats;
        }
    }

    public T GetStat<T>(EStatType key)
    {
        if (Stats.TryGetValue(key, out var stat))
            return stat.GetValue<T>();
        return default;
    }

    public void SetStat(EStatType key, object value)
    {
        if (_stats == null) _stats = new Dictionary<EStatType, StatValue>();

        StatValue val = new StatValue();
        switch (value)
        {
            case float f:
                val.Type = StatValue.ValueType.Float;
                val.floatValue = f;
                break;
            case int i:
                val.Type = StatValue.ValueType.Int;
                val.intValue = i;
                break;
            case bool b:
                val.Type = StatValue.ValueType.Bool;
                val.boolValue = b;
                break;
            case string s:
                val.Type = StatValue.ValueType.String;
                val.stringValue = s;
                break;
        }

        _stats[key] = val;

        int index = statEntries.FindIndex(e => e.key.Equals(key));
        if (index >= 0)
            statEntries[index] = new StatEntry { key = key, value = val };
        else
            statEntries.Add(new StatEntry { key = key, value = val });
    }
}

[CreateAssetMenu(fileName = "ParamData", menuName = "Scriptable Object/Parameter Data", order = 21)]
public class GoogleSheetLoader : ScriptableObject
{
    private static readonly Dictionary<string, EStatType> headerToStatType =
        new Dictionary<string, EStatType>(StringComparer.OrdinalIgnoreCase)
        {
            { "Index", EStatType.Index },
            { "Name", EStatType.Name },
            { "HP", EStatType.MaxHp },
            { "Damage", EStatType.Attack },
            { "MinSpeed", EStatType.MinimumMoveSpeed },
            { "MaxSpeed", EStatType.MaximumMoveSpeed },
            { "AttackSpeed", EStatType.AttackSpeed },
            { "Defence", EStatType.Defence },
            { "DetectiveRange", EStatType.DetectiveRange },
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

    private void BuildDictionary()
    {
        _dataDict = new Dictionary<int, RowData>();
        for (int i = 0; i < datas.Count; i++)
        {
            _dataDict[i] = datas[i];
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
        string[] lines = csv.Split('\n');
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
                EStatType statType;

                // 매핑 딕셔너리 우선 사용
                if (!headerToStatType.TryGetValue(headers[j], out statType))
                {
                    // 매핑에 없으면 Enum.TryParse 시도
                    if (!Enum.TryParse(headers[j], true, out statType))
                    {
                        Debug.LogWarning($"[Row {i}] 알 수 없는 StatType: {headers[j]}");
                        continue;
                    }
                }

                string raw = values[j];

                if (int.TryParse(raw, out int intVal))
                    row.SetStat(statType, intVal);
                else if (float.TryParse(raw, out float f))
                    row.SetStat(statType, f);
                else if (bool.TryParse(raw, out bool b))
                    row.SetStat(statType, b);
                else
                    row.SetStat(statType, raw);
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
        using (UnityWebRequest www = UnityWebRequest.Get(GetSheetURL()))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string csvData = www.downloadHandler.text;
                ParseCsv(csvData);
                Debug.Log($"Google Sheet Data Loaded: {datas.Count} rows");
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this); // 저장 표시
#endif
            }
            else
            {
                Debug.LogError($"Failed to load sheet: {www.error}");
            }
        }
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
