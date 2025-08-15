using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 파츠 부위 Enum
[System.Flags]
public enum EPartType
{
    ArmL = 1 << 0,
    ArmR = 1 << 1,
    Legs = 1 << 2,
    Shoulder = 1 << 3,
}

// 파츠 연결을 위한 메시 종류 Enum
// Skinned Mesh Renderer를 사용하더라도 캐터필러처럼 움직이는 방식이 다르다면 Static을 사용
public enum EPartMeshType
{
    Skinned,
    Static
}

public abstract class PartBase : MonoBehaviour
{
    [Header("파츠 기본 설정")]
    [SerializeField, Range(3000, 3015)] protected int partId = 3000;
    [SerializeField] protected EPartType partType;
    [SerializeField] protected EPartMeshType meshType;

    protected PlayerController _owner;
    protected StatDictionary _stats = new();

    protected bool _isAnimating = true;

    public EPartType PartType => partType;
    public EPartMeshType MeshType => meshType;
    public StatDictionary Stats => _stats;
    public bool IsAnimating => _isAnimating;

    public abstract void UseAbility();
    public abstract void UseCancleAbility();
    public abstract void FinishActionForced();

    public void Init(PlayerController owner)
    {
        SetOwner(owner);
        SetPartStat();
    }

    public void SetOwner(PlayerController owner)
    {
        _owner = owner;
    }

    public void SetPartStat()
    {
        GoogleSheetLoader baseParam = Resources.Load<GoogleSheetLoader>("Params/ParamDatas");
        if (baseParam != null)
        {
            var row = baseParam.GetRow("CharacterParts", partId);
            if (row != null)
            {
                InitializeFromRow(row);
            }
            else
            {
                Debug.LogWarning($"Part Stat Index({partId}) 데이터가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("파츠 파라미터 데이터가 없습니다.");
        }
    }

    public void InitializeFromRow(RowData row)
    {
        _stats = row.Stats.Clone();
    }
}
