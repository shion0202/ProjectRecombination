using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMeshBone : MonoBehaviour
{
    [SerializeField] private EEquipmentType type;
    public EEquipmentType Type { get { return type; } }
}
