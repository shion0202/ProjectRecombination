using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Variables
    [Header("Inventory")]
    [SerializeField] private PlayerController owner;
    [SerializeField] private List<PartBase> baseParts = new List<PartBase>();
    private Dictionary<EPartType, List<PartBase>> _items = new Dictionary<EPartType, List<PartBase>>();
    private Dictionary<EPartType, PartBase> _equippedItems = new Dictionary<EPartType, PartBase>();
    private int legsIndex = 0;  // 테스트용 Legs 인벤토리 인덱스

    [Header("Mesh and Bone Data")]
    [SerializeField] private Transform boneRoot;
    [SerializeField] private Transform meshRoot;
    private Dictionary<string, PartBase> _parts = new();    // Mesh Root에 자식으로 있는 모든 파츠
    private List<string> _boneList = new();
    private Dictionary<string, Transform> _boneMap = new();
    #endregion

    #region Properties
    public Dictionary<EPartType, List<PartBase>> Items
    {
        get { return _items; }
    }

    public Dictionary<EPartType, PartBase> EquippedItems
    {
        get { return _equippedItems; }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        for (int i = 0; i < Enum.GetNames(typeof(EPartType)).Length; ++i)
        {
            _items.Add((EPartType)(1 << i), new List<PartBase>());
        }

        _boneList = Resources.Load<CharacterBoneData>($"Bone/LupaBoneData").boneNames;
        foreach (Transform bone in boneRoot.GetComponentsInChildren<Transform>())
        {
            if (_boneList.Contains(bone.name))
            {
                _boneMap.Add(bone.name, bone);
            }
        }

        foreach (Transform child in meshRoot)
        {
            PartBase target = child.GetComponent<PartBase>();
            if (target != null)
            {
                _parts[child.name] = target;
                SetPartBone(target);
                target.SetOwner(owner);
                target.SetPartStat();

                // 테스트용으로 MeshRoot에 있는 모든 파츠를 인벤토리에 추가
                GetItem(target);
            }
        }

        foreach (PartBase part in baseParts)
        {
            // GetItem(part);
            EquipItem(part);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            legsIndex = (legsIndex + 1) % _items[EPartType.Legs].Count;
            EquipItem(_items[EPartType.Legs][legsIndex]);
        }
    }
    #endregion

    #region Public Methods
    public void GetItem(PartBase newItem)
    {
        if (!_items[newItem.PartType].Contains(newItem))
        {
            _items[newItem.PartType].Add(newItem);
        }
    }

    public bool RemoveItem(PartBase removeItem)
    {
        return _items[removeItem.PartType].Remove(removeItem);
    }

    public void EquipItem(PartBase equipItem)
    {
        if (!_items[equipItem.PartType].Contains(equipItem)) return;

        PartBase postEquipment = null;
        PartBase currentEquipment = _parts[equipItem.name];
        if (!_equippedItems.ContainsKey(equipItem.PartType))
        {
            // For base parts
            _equippedItems.Add(equipItem.PartType, currentEquipment);
        }
        else
        {
            postEquipment = _equippedItems[equipItem.PartType];
            _equippedItems[equipItem.PartType] = currentEquipment;
        }

        if (postEquipment != null)
        {
            postEquipment.FinishActionForced();
            postEquipment.gameObject.SetActive(false);
        }
        currentEquipment.gameObject.SetActive(true);

        // 스탯 반영
        owner.SetPartStat(equipItem);
    }
    #endregion

    #region Private Methods
    private void SetPartBone(PartBase part)
    {
        EPartMeshType meshType = part.MeshType;
        switch (meshType)
        {
            case EPartMeshType.Skinned:
                SetSkinnedMeshBone(part);
                break;
            case EPartMeshType.Static:
                SetStaticMeshBone(part);
                break;
        }
    }

    private void SetSkinnedMeshBone(PartBase part)
    {
        SkinnedMeshRenderer smr = part.GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError($"SkinnedMeshRenderer not found on {part.name}. Please ensure it has a SkinnedMeshRenderer component.");
            return;
        }

        List<Transform> meshTransforms = new List<Transform>();
        for (int i = 0; i < smr.bones.Length; ++i)
        {
            meshTransforms.Add(_boneMap[_boneList[i]]);
        }
        smr.bones = meshTransforms.ToArray();
        //smr.rootBone = rootBone;
    }

    private void SetStaticMeshBone(PartBase part)
    {
        part.transform.SetParent(_boneMap[_boneList[0]]);

        // To-do: 파츠 별로 offset 값이 필요
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;
    }
    #endregion
}
