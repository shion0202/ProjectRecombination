using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<ItemData> _items = new List<ItemData>();
    private Dictionary<EquipmentType, ItemData> _equippedItems = new Dictionary<EquipmentType, ItemData>();

    private MeshRenderer _meshRoot;
    private BoonRoot _boonRoot;
    private Dictionary<string, Transform> _boneMap = new Dictionary<string, Transform>();
    private List<Transform> _boneList = new List<Transform>();

    public Transform rootBone;

    public List<ItemData> Items
    {
        get { return _items; }
    }

    public Dictionary<EquipmentType, ItemData> EquippedItems
    {
        get { return _equippedItems; }
    }

    private void Awake()
    {
        _meshRoot = GetComponentInChildren<MeshRenderer>();
        _boonRoot = GetComponentInChildren<BoonRoot>();

        foreach (Transform bone in _boonRoot.GetComponentsInChildren<Transform>())
        {
            _boneMap[bone.name] = bone;
            _boneList.Add(bone);
        }

        ItemData item1 = new ItemData { id = 1, itemName = "BaseLegs", equipType = EquipmentType.Legs, equipPrefab = null };
        // item1.equipPrefab = Resources.Load<GameObject>("Prefabs/Equipment/BaseLegs");
        ItemData item2 = new ItemData { id = 2, itemName = "NewLegs", equipType = EquipmentType.Legs, equipPrefab = null };
        // item2.equipPrefab = Resources.Load<GameObject>("Prefabs/Equipment/NewLegs");

        _items.Add(item1);
        _items.Add(item2);
        EquipItem(item1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipItem(_items[0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipItem(_items[1]);
        }
    }

    public void GetItem(ItemData newItem)
    {
        if (!_items.Contains(newItem))
        {
            _items.Add(newItem);
        }
    }

    public void RemoveItem(ItemData removeItem)
    {
        if (_items.Contains(removeItem))
        {
            _items.Remove(removeItem);
        }
    }

    public void EquipItem(ItemData equipItem)
    {
        if (!_items.Contains(equipItem)) return;
        // RemoveItem(equipItem);

        string postItemName = "";
        if (!_equippedItems.ContainsKey(equipItem.equipType))
        {
            _equippedItems.Add(equipItem.equipType, equipItem);
        }
        else
        {
            postItemName = _equippedItems[equipItem.equipType].itemName;
            _equippedItems[equipItem.equipType] = equipItem;
        }

        // Need to refactoring
        GameObject currentEquipment = null;
        GameObject postEquipment = null;
        foreach (Transform child in _meshRoot.transform)
        {
            if (child.name == postItemName)
            {
                postEquipment = child.gameObject;
            }

            if (child.name == equipItem.itemName)
            {
                currentEquipment = child.gameObject;
            }

            if (currentEquipment != null && postEquipment != null)
            {
                break;
            }
        }

        if (postEquipment != null)
        {
            postEquipment.SetActive(false);
        }
        currentEquipment.SetActive(true);

        SkinnedMeshRenderer smr = currentEquipment.GetComponent<SkinnedMeshRenderer>();
        smr.bones = _boneList.ToArray();
        //Transform[] newBones = smr.bones.Select(b => b != null ? _boneMap[b.name] : null).ToArray();
        //smr.bones = newBones;
        //smr.rootBone = rootBone;
    }
}
