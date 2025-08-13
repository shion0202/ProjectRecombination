using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleSheetsToUnity;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class PartParamData
{
    public int partId;
    public string partName;
    public int attackAilment;
    public float fireSpeed;
    public float attackSkillDamage;
    public float skillSpeed;
    public float skillCount;
    public float skillCooldown;
    public float maxHp;
    public float attack;
    public float defence;
    public float moveSpeed;
    public float cooldownDecrease;

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
                    { EStatType.AttackSpeed, fireSpeed },
                    { EStatType.Defence, defence },
                    { EStatType.MoveSpeed, moveSpeed },
                    { EStatType.AttackSkillDamage, attackSkillDamage },
                    { EStatType.SkillSpeed, skillSpeed },
                    { EStatType.SkillCount, skillCount },
                    { EStatType.SkillCooldown, skillCooldown },
                    { EStatType.CooldownDecrease, cooldownDecrease },
                    { EStatType.AttackAilment, attackAilment },
                };
            }
            return _statDict.TryGetValue(type, out float value) ? value : 0.0f;
        }
    }
}

[CreateAssetMenu(fileName = "PartParamData", menuName = "Scriptable Object/Part Param Data", order = 21)]
public class PartParamDataReader : DataReaderBase
{
    [Header("스프레드시트에서 읽혀져 직렬화된 오브젝트")]
    [SerializeField] public List<PartParamData> DataList = new();

    internal void UpdateStats(List<GSTU_Cell> list, int formId)
    {
        PartParamData data = new();

        // 구글 스프레드시트 리스트를 순회하며 타입별 데이터를 불러온다.
        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].columnId)
            {
                case "partsID":
                    data.partId = int.Parse(list[i].value);
                    break;
                case "partsName":
                    data.partName = list[i].value;
                    break;
                case "attackAilment":
                    data.attackAilment = int.Parse(list[i].value);
                    break;
                case "fireRapid":
                    data.fireSpeed = float.Parse(list[i].value);
                    break;
                case "attackSkillDamage":
                    data.attackSkillDamage = float.Parse(list[i].value);
                    break;
                case "skillSpeed":
                    data.skillSpeed = float.Parse(list[i].value);
                    break;
                case "skillCount":
                    data.skillCount = float.Parse(list[i].value);
                    break;
                case "skillCooldown":
                    data.skillCooldown = float.Parse(list[i].value);
                    break;
                case "addHp":
                    data.maxHp = float.Parse(list[i].value);
                    break;
                case "attack":
                    data.attack = float.Parse(list[i].value);
                    break;
                case "addDefence":
                    data.defence = float.Parse(list[i].value);
                    break;
                case "addSpeed":
                    data.moveSpeed = float.Parse(list[i].value);
                    break;
                case "cooldownDecrease":
                    data.cooldownDecrease = float.Parse(list[i].value);
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
[CustomEditor(typeof(PartParamDataReader))]
public class PartParamDataReaderEditor : Editor
{
    PartParamDataReader data;

    void OnEnable()
    {
        data = (PartParamDataReader)target;
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
