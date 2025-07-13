using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType
{
    Body,
    LeftArm,
    RightArm,
    Arms,
    Legs
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string itemName;
    public EquipmentType equipType;
    public GameObject equipPrefab;
}
