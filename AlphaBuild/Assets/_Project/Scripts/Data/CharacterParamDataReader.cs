using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleSheetsToUnity;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

// 캐릭터 파라미터 데이터를 불러와 저장하기 위한 구조체
[Serializable]
public struct CharacterParamData
{
    public int formID;
    public string formName;
    public string displayFormName;
    public float hp;
    public float attack;
    public float attackSpeed;
    public float range;
    public float defense;
    public float runSpeed;
    public float hpRecovery;

    public CharacterParamData(int formID, string formName, string displayFormName, float hp, float attack,
        float attackSpeed, float range, float defense, float runSpeed, float hpRecovery)
    {
        this.formID = formID;
        this.formName = formName;
        this.displayFormName = displayFormName;
        this.hp = hp;
        this.attack = attack;
        this.attackSpeed = attackSpeed;
        this.range = range;
        this.defense = defense;
        this.runSpeed = runSpeed;
        this.hpRecovery = hpRecovery;
    }
}

// 캐릭터 파라미터 데이터를 읽어오기 위한 클래스
[CreateAssetMenu(fileName = "CharacterParamData", menuName = "Scriptable Object/Character Param Data", order = 20)]
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
                case "formID":
                {
                    data.formID = int.Parse(list[i].value);
                    break;
                }
                case "formName":
                {
                    data.formName = list[i].value;
                    break;
                }
                case "displayFormName":
                {
                    data.displayFormName = list[i].value;
                    break;
                }
                case "hp":
                {
                    data.hp = float.Parse(list[i].value);
                    break;
                }
                case "attack":
                {
                    data.attack = float.Parse(list[i].value);
                    break;
                }
                case "attackSpeed":
                {
                    data.attackSpeed = float.Parse(list[i].value);
                    break;
                }
                case "range":
                {
                    data.range = float.Parse(list[i].value);
                    break;
                }
                case "defense":
                {
                    data.defense = float.Parse(list[i].value);
                    break;
                }
                case "runSpeed":
                {
                    data.runSpeed = float.Parse(list[i].value);
                    break;
                }
                case "hpRecovery":
                {
                    data.hpRecovery = float.Parse(list[i].value);
                    break;
                }
                default:
                {
                    Debug.LogWarning($"Unknown column: {list[i].columnId} with value: {list[i].value}");
                    break;
                }
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
