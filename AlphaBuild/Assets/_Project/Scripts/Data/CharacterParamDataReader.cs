using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleSheetsToUnity;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 캐릭터 파라미터 데이터를 불러와 저장하기 위한 클래스
[Serializable]
public class CharacterParamData
{
    public float maxHp;
    public float attack;
    public float fireSpeed;
    public float defence;
    public float moveSpeed;

    private Dictionary<EStatType, float> _statDict;
    public float this[EStatType type]
    {
        get
        {
            if (_statDict == null)
            {
                _statDict = new Dictionary<EStatType, float>
                {
                    { EStatType.MaxHp, maxHp },
                    { EStatType.Attack, attack },
                    { EStatType.FireSpeed, fireSpeed },
                    { EStatType.Defence, defence },
                    { EStatType.MoveSpeed, moveSpeed }
                };
            }
            return _statDict.TryGetValue(type, out float value) ? value : 0.0f;
        }
    }
}

// 캐릭터 파라미터 데이터를 읽어오기 위한 클래스
[CreateAssetMenu(fileName = "CharacterParamData", menuName = "Scriptable Object/Character Param Data", order = 21)]
public class CharacterParamDataReader : DataReaderBase
{
    [Header("스프레드시트에서 읽혀져 직렬화된 오브젝트")]
    [SerializeField] public List<CharacterParamData> DataList = new List<CharacterParamData>();

    internal void UpdateStats(List<GSTU_Cell> list, int formId)
    {
        CharacterParamData data = new CharacterParamData();

        // 구글 스프레드시트 리스트를 순회하며 타입별 데이터를 불러온다.
        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].columnId)
            {
                case "hp":
                    data.maxHp = float.Parse(list[i].value);
                    break;
                case "attack":
                    data.attack = float.Parse(list[i].value);
                    break;
                case "fireRapid":
                    data.fireSpeed = float.Parse(list[i].value);
                    break;
                case "defence":
                    data.defence = float.Parse(list[i].value);
                    break;
                case "speed":
                    data.moveSpeed = float.Parse(list[i].value);
                    break;
                default:
                    Debug.LogWarning($"Unknown column: {list[i].columnId} at row {formId}");
                    break;
            }
        }

        DataList.Add(data);
    }
}

// GSTU 패키지를 사용하여 스프레드시트 데이터를 읽어오는 클래스
#if UNITY_EDITOR
[CustomEditor(typeof(CharacterParamDataReader))]
public class CharacterParamDataReaderEditor : Editor
{
    CharacterParamDataReader data;

    void OnEnable()
    {
        data = (CharacterParamDataReader)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("\n스프레드 시트 읽어오기");

        if (GUILayout.Button("데이터 읽기(API 호출)"))
        {
            UpdateStats(UpdateMethodOne);
            data.DataList.Clear();
        }
    }

    void UpdateStats(UnityAction<GstuSpreadSheet> callback, bool mergedCells = false)
    {
        SpreadsheetManager.Read(new GSTU_Search(data.associatedSheet, data.associatedWorksheet), callback, mergedCells);
    }

    void UpdateMethodOne(GstuSpreadSheet ss)
    {
        for (int i = data.START_ROW_LENGTH; i <= data.END_ROW_LENGTH; ++i)
        {
            data.UpdateStats(ss.rows[i], i);
        }

        EditorUtility.SetDirty(target);
    }
}
#endif
