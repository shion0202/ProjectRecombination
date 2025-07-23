using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StatDictionary : MonoBehaviour
{
    private Dictionary<EStatType, StatData> _statDict = new();

    public StatData this[EStatType type]
    {
        get => _statDict[type];
    }

    public void Add(EStatType type, StatData data)
    {
        _statDict.Add(type, data);
    }
}
