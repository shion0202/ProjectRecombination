using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    #region Variables
    [Header("Inventory")]
    [SerializeField] private PlayerController owner;
    [SerializeField] private List<PartBase> baseParts = new List<PartBase>();
    private Dictionary<EPartType, List<PartBase>> _items = new Dictionary<EPartType, List<PartBase>>();
    private Dictionary<EPartType, PartBase> _equippedItems = new Dictionary<EPartType, PartBase>();
    private int partIndex = 0;

    [Header("Mesh and Bone Data")]
    [SerializeField] private Transform boneRoot;
    [SerializeField] private Transform meshRoot;
    private Dictionary<string, PartBase> _parts = new();    // Mesh Root에 자식으로 있는 모든 파츠
    private Dictionary<EPartType, List<string>> _boneList = new();
    private Dictionary<EPartType, Dictionary<string, Transform>> _boneMap = new();
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
            _boneList.Add((EPartType)(1 << i), new List<string>());
            _boneMap.Add((EPartType)(1 << i), new Dictionary<string, Transform>());
        }

        foreach (EPartType partType in Enum.GetValues(typeof(EPartType)))
        {
            _boneList[partType] = Resources.Load<CharacterBoneData>($"Bone/Player{partType.ToString()}BoneData").boneNames;

            foreach (Transform bone in boneRoot.GetComponentsInChildren<Transform>())
            {
                if (_boneList[partType].Contains(bone.name))
                {
                    _boneMap[partType].Add(bone.name, bone);
                }
            }
        }

        for (int i = 0; i < meshRoot.childCount; ++i)
        {
            PartBase target = meshRoot.GetChild(i).GetComponent<PartBase>();
            if (target != null)
            {
                _parts[meshRoot.GetChild(i).name] = target;
                SetPartBone(target);
                if (target.MeshType == EPartMeshType.Static)
                {
                    --i;
                }

                target.Init(owner);
                target.gameObject.SetActive(false);

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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            partIndex = 0;
            EquipItem(_items[EPartType.Shoulder][partIndex]);
            EquipItem(_items[EPartType.ArmL][partIndex]);
            EquipItem(_items[EPartType.ArmR][partIndex]);
            EquipItem(_items[EPartType.Legs][partIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            partIndex = 1;
            EquipItem(_items[EPartType.Shoulder][partIndex]);
            EquipItem(_items[EPartType.ArmL][partIndex]);
            EquipItem(_items[EPartType.ArmR][partIndex]);
            EquipItem(_items[EPartType.Legs][partIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            partIndex = 2;
            EquipItem(_items[EPartType.Shoulder][partIndex]);
            EquipItem(_items[EPartType.ArmL][partIndex]);
            EquipItem(_items[EPartType.ArmR][partIndex]);
            EquipItem(_items[EPartType.Legs][partIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            partIndex = 3;
            EquipItem(_items[EPartType.Shoulder][partIndex]);
            EquipItem(_items[EPartType.ArmL][partIndex]);
            EquipItem(_items[EPartType.ArmR][partIndex]);
            EquipItem(_items[EPartType.Legs][partIndex]);
        }

        // GUI 갱신 스크립트
        GUIManager.Instance.SetLeftPartText(_items[EPartType.ArmL][partIndex].name);
        GUIManager.Instance.SetRightPartText(_items[EPartType.ArmR][partIndex].name);
        GUIManager.Instance.SetLegsText(_items[EPartType.Legs][partIndex].name);
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
	
	// TODO: 파츠별,파리미터별 적용 방식 디테일한 논의 필요
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
            owner.Stats.RemoveModifierFromSource(postEquipment);
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
            meshTransforms.Add(_boneMap[part.PartType][_boneList[part.PartType][i]]);
        }
        smr.bones = meshTransforms.ToArray();
        //smr.rootBone = rootBone;
    }

    private void SetStaticMeshBone(PartBase part)
    {
        MeshRenderer mr = part.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            SkinnedMeshRenderer smr = part.GetComponent<SkinnedMeshRenderer>();
            if (smr == null) return;

            part.transform.SetParent(smr.rootBone);
        }
        else
        {
            part.transform.SetParent(boneRoot);
        }

        part.transform.localPosition = part.StaticOffset;
        part.transform.localRotation = Quaternion.Euler(part.StaticRotation);
    }
    #endregion
}
