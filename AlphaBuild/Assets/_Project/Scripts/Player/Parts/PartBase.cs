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
    [SerializeField] protected int partId = 3000;
    [SerializeField] protected EPartType partType;
    [SerializeField] protected EPartMeshType meshType;

    protected PlayerController _owner;
    protected StatDictionary _stats = new();

    public EPartType PartType => partType;
    public EPartMeshType MeshType => meshType;
    public StatDictionary Stats => _stats;

    public abstract void UseAbility();
    public abstract void UseCancleAbility();
    public abstract void FinishActionForced();

    public virtual void SetOwner(PlayerController owner)
    {
        _owner = owner;
    }

    public void SetPartStat()
    {
        // 임시 파츠를 위한 값
        if (partId % 1000 == 0)
        {
            _stats.Add(EStatType.Attack, new StatData(EStatType.Attack, 0, 9999, -9999));
            return;
        }

        PartParamData baseParam = Resources.Load<PartParamDataReader>("Params/PartParamData").DataList[partId - (3000 + 1)];
        foreach (EStatType type in Enum.GetValues(typeof(EStatType)))
        {
            _stats.Add(type, new StatData(type, baseParam[type], 9999, -9999));
        }
    }
}
