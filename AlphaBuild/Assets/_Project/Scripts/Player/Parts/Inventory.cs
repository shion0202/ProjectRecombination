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
    private Dictionary<EPartType, GameObject> _equippedItems = new Dictionary<EPartType, GameObject>();

    [Header("Mesh and Bone Data")]
    [SerializeField] private Transform boneRoot;
    [SerializeField] private Transform meshRoot;
    private Dictionary<string, GameObject> _parts = new();
    private List<string> _boneList = new();
    private Dictionary<string, Transform> _boneMap = new();
    #endregion

    #region Properties
    public Dictionary<EPartType, List<PartBase>> Items
    {
        get { return _items; }
    }

    public Dictionary<EPartType, GameObject> EquippedItems
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
                _parts[child.name] = child.gameObject;
                SetPartBone(target);
                target.SetPartStat();
            }
        }

        foreach (PartBase part in baseParts)
        {
            GetItem(part);
            EquipItem(part);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_equippedItems[EPartType.Shoulder].name == _items[EPartType.Shoulder][0].name)
                return;

            EquipItem(_items[EPartType.Shoulder][0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (_equippedItems[EPartType.Shoulder].name == _items[EPartType.Shoulder][1].name)
                return;

            EquipItem(_items[EPartType.Shoulder][1]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (_equippedItems[EPartType.Legs].name == _items[EPartType.Legs][0].name)
                return;

            gameObject.GetComponent<PlayerController>().SetMovable(true);

            EquipItem(_items[EPartType.Legs][0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (_equippedItems[EPartType.Legs].name == _items[EPartType.Legs][1].name)
                return;

            gameObject.GetComponent<PlayerController>().SetMovable(false);

            EquipItem(_items[EPartType.Legs][1]);
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

        GameObject postEquipment = null;
        GameObject currentEquipment = _parts[equipItem.name];
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
            postEquipment.SetActive(false);
        currentEquipment.SetActive(true);

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

        // To-do: 파츠 별로 offset 값이 필요할 수 있다
        part.transform.localPosition = Vector3.zero;
        part.transform.localRotation = Quaternion.identity;
    }
    #endregion
}
