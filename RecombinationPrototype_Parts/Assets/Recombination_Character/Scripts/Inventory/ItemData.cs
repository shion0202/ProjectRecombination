using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEquipmentType
{
    Body,
    LeftArm,
    RightArm,
    Arms,
    Legs
}

public enum EPartType
{
    Skinned,
    Static
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public EEquipmentType equipmentType;
    public EPartType partType;
}
