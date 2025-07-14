using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Inventory : MonoBehaviour
{
    private Dictionary<EEquipmentType, List<ItemData>> _items = new Dictionary<EEquipmentType, List<ItemData>>();
    private Dictionary<EEquipmentType, GameObject> _equippedItems = new Dictionary<EEquipmentType, GameObject>();

    [SerializeField] private Transform meshRoot;
    [SerializeField] private Transform boneRoot;
    private List<string> _boneList = new List<string>();
    private Dictionary<string, Transform> _boneMap = new Dictionary<string, Transform>();
    private Dictionary<EEquipmentType, string> _rootBones = new Dictionary<EEquipmentType, string>();
    private Dictionary<string, GameObject> _partMap = new Dictionary<string, GameObject>();

    public Dictionary<EEquipmentType, List<ItemData>> Items
    {
        get { return _items; }
    }

    public Dictionary<EEquipmentType, GameObject> EquippedItems
    {
        get { return _equippedItems; }
    }

    private void Awake()
    {
        for (int i = 0; i < Enum.GetNames(typeof(EEquipmentType)).Length; ++i)
        {
            _items.Add((EEquipmentType)i, new List<ItemData>());
        }

        foreach (Transform child in meshRoot)
        {
            TargetMeshBone target = child.GetComponent<TargetMeshBone>();
            if (target != null)
            {
                _partMap[child.name] = child.gameObject;
            }
        }

        _boneList = Resources.Load<CharacterBoneData>($"Bone/PlayerBoneData").boneNames;

        foreach (Transform bone in boneRoot.GetComponentsInChildren<Transform>())
        {
            if (_boneList.Contains(bone.name))
            {
                _boneMap.Add(bone.name, bone);
            }
        }

        ItemData BaseLegs = new ItemData { id = 1, itemName = "BaseLegs", equipmentType = EEquipmentType.Legs, partType = EPartType.Skinned };
        ItemData BallLegs = new ItemData { id = 2, itemName = "BallLegs", equipmentType = EEquipmentType.Legs, partType = EPartType.Static };
        _items[BaseLegs.equipmentType].Add(BaseLegs);
        _items[BallLegs.equipmentType].Add(BallLegs);
        EquipItem(BaseLegs);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipItem(_items[EEquipmentType.Legs][0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipItem(_items[EEquipmentType.Legs][1]);
        }
    }

    public void GetItem(ItemData newItem)
    {
        if (!_items[newItem.equipmentType].Contains(newItem))
        {
            _items[newItem.equipmentType].Add(newItem);
        }
    }

    public void RemoveItem(ItemData removeItem)
    {
        if (_items[removeItem.equipmentType].Contains(removeItem))
        {
            _items[removeItem.equipmentType].Remove(removeItem);
        }
    }

    public void EquipItem(ItemData equipItem)
    {
        if (!_items[equipItem.equipmentType].Contains(equipItem)) return;
        // RemoveItem(equipItem);

        GameObject postEquipment = null;
        GameObject currentEquipment = _partMap[equipItem.itemName];
        if (!_equippedItems.ContainsKey(equipItem.equipmentType))
        {
            _equippedItems.Add(equipItem.equipmentType, currentEquipment);
        }
        else
        {
            postEquipment = _equippedItems[equipItem.equipmentType];
            _equippedItems[equipItem.equipmentType] = currentEquipment;
        }

        if (postEquipment != null)
        {
            postEquipment.SetActive(false);
        }
        currentEquipment.SetActive(true);

        if (equipItem.partType == EPartType.Skinned)
        {
            SkinnedMeshRenderer smr = currentEquipment.GetComponent<SkinnedMeshRenderer>();
            List<Transform> meshTransforms = new List<Transform>();
            for (int i = 0; i < smr.bones.Length; ++i)
            {
                meshTransforms.Add(_boneMap[_boneList[i]]);
            }
            smr.bones = meshTransforms.ToArray();
            //smr.rootBone = rootBone;
        }
        else
        {
            currentEquipment.transform.SetParent(_boneMap[_boneList[0]]);

            // ��ġ �� ȸ�� �ʱ�ȭ
            currentEquipment.transform.localPosition = new Vector3(0.0f, -0.9f, -0.05f);
            currentEquipment.transform.localRotation = Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f));
        }
    }
}
