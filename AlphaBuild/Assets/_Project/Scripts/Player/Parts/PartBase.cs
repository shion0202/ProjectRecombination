using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� ���� Enum
[System.Flags]
public enum EPartType
{
    ArmL = 1 << 0,
    ArmR = 1 << 1,
    Legs = 1 << 2,
    ShoulderL = 1 << 3,
    ShoulderR = 1 << 4
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
    [SerializeField] protected EPartType partType;
    [SerializeField] protected EPartMeshType meshType;

    protected PlayerController _owner;
    protected StatDictionary _stats;

    public EPartType PartType => partType;
    public EPartMeshType MeshType => meshType;

    public abstract void UseAbility(PlayerController owner);
}
