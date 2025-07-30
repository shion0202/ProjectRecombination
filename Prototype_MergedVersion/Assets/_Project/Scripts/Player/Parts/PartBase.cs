using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� ���� Enum
[Flags]
public enum EPartType
{
    ArmL = 1 << 0,
    ArmR = 1 << 1,
    Legs = 1 << 2,
    Shoulder = 1 << 3,
}

// ���� ������ ���� �޽� ���� Enum
// Skinned Mesh Renderer�� ����ϴ��� ĳ���ʷ�ó�� �����̴� ����� �ٸ��ٸ� Static�� ���
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

    public void SetOwner(PlayerController owner)
    {
        _owner = owner;
    }

    public void SetPartStat()
    {
        // �ӽ� ������ ���� ��
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
