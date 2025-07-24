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
    ShoulderL = 1 << 3,
    ShoulderR = 1 << 4
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
    [SerializeField] protected EPartType partType;
    [SerializeField] protected EPartMeshType meshType;

    protected PlayerController _owner;
    protected StatDictionary _stats;

    public EPartType PartType => partType;
    public EPartMeshType MeshType => meshType;

    public abstract void UseAbility(PlayerController owner);
}
