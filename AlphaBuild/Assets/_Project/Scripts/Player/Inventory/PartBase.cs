using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum EPartType
{
    Head = 1 << 0,
    Shoulder = 1 << 1,
    Body = 1 << 2,
    Arm = 1 << 3,
    Pelvis = 1 << 4,
    Legs = 1 << 5
}

public enum EPartMeshType
{
    Skinned,
    Static
}

public abstract class PartBase : MonoBehaviour
{
    protected PlayerController _owner;

    [SerializeField] protected EPartType partType;
    [SerializeField] protected EPartMeshType meshType;

    public EPartType PartType { get { return partType; } }
    public EPartMeshType MeshType { get { return meshType; } }

    public abstract void UseAbility(PlayerController owner);
}
