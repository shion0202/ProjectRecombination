using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMeshBone : MonoBehaviour
{
    [SerializeField] private EPartType parttype;
    public EPartType PartType { get { return parttype; } }
}
