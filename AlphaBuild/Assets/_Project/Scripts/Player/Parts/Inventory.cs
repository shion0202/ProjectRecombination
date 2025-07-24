using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private List<PartBase> baseParts = new List<PartBase>();
    private Dictionary<EPartType, List<PartBase>> _items = new Dictionary<EPartType, List<PartBase>>();
    private Dictionary<EPartType, GameObject> _equippedItems = new Dictionary<EPartType, GameObject>();

    [Header("Test")]
    public PartBase BaseLegs;
    public PartBase BallLegs;
    public PartBase baseBody;
    public PartBase cloakBody;

    [Header("Mesh and Bone Data")]
    [SerializeField] private Transform meshRoot;
    [SerializeField] private Transform boneRoot;
    private Dictionary<string, GameObject> _parts = new Dictionary<string, GameObject>();   // Don't need
    private List<string> _boneList = new List<string>();
    private Dictionary<string, Transform> _boneMap = new Dictionary<string, Transform>();

    public Dictionary<EPartType, List<PartBase>> Items
    {
        get { return _items; }
    }

    public Dictionary<EPartType, GameObject> EquippedItems
    {
        get { return _equippedItems; }
    }

    private void Awake()
    {
        for (int i = 0; i < Enum.GetNames(typeof(EPartType)).Length; ++i)
        {
            _items.Add((EPartType)(1 << i), new List<PartBase>());
        }

        // Need to have parts in children.
        foreach (Transform child in meshRoot)
        {
            PartBase target = child.GetComponent<PartBase>();
            if (target != null)
            {
                _parts[child.name] = child.gameObject;
            }
        }

        // Get player's bone data.
        // Don;t need to load at here. (Consider data manager.)
        _boneList = Resources.Load<CharacterBoneData>($"Bone/LupaBoneData").boneNames;
        foreach (Transform bone in boneRoot.GetComponentsInChildren<Transform>())
        {
            if (_boneList.Contains(bone.name))
            {
                _boneMap.Add(bone.name, bone);
            }
        }

        // Add and equip base parts.
        _items[BaseLegs.PartType].Add(BaseLegs);
        _items[BallLegs.PartType].Add(BallLegs);
        _items[baseBody.PartType].Add(baseBody);
        _items[cloakBody.PartType].Add(cloakBody);
        
        foreach (PartBase part in baseParts)
        {
            EquipItem(part);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_equippedItems[EPartType.ShoulderL].name == _items[EPartType.ShoulderL][0].name)
                return;

            EquipItem(_items[EPartType.ShoulderL][0]);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (_equippedItems[EPartType.ShoulderL].name == _items[EPartType.ShoulderL][1].name)
                return;

            EquipItem(_items[EPartType.ShoulderL][1]);
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
        // RemoveItem(equipItem);

        GameObject postEquipment = null;
        GameObject currentEquipment = _parts[equipItem.name];
        if (!_equippedItems.ContainsKey(equipItem.PartType))
        {
            _equippedItems.Add(equipItem.PartType, currentEquipment);
        }
        else
        {
            postEquipment = _equippedItems[equipItem.PartType];
            _equippedItems[equipItem.PartType] = currentEquipment;
        }

        if (postEquipment != null)
        {
            postEquipment.SetActive(false);
        }
        currentEquipment.SetActive(true);

        // Don't need to set bone now
        // For example, can set bone when init inventory or character
        if (currentEquipment.GetComponent<SkinnedMeshRenderer>())
        {
            SkinnedMeshRenderer smr = currentEquipment.GetComponent<SkinnedMeshRenderer>();
            List<Transform> meshTransforms = new List<Transform>();
            for (int i = 0; i < smr.bones.Length; ++i)
            {
                meshTransforms.Add(_boneMap[_boneList[i]]);
            }
            smr.bones = meshTransforms.ToArray();
            //smr.rootBone = rootBone;

            foreach (SkinnedMeshRenderer child in currentEquipment.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                meshTransforms.Clear();
                for (int i = 0; i < smr.bones.Length; ++i)
                {
                    meshTransforms.Add(_boneMap[_boneList[i]]);
                }
                child.bones = meshTransforms.ToArray();
            }
        }
        else
        {
            currentEquipment.transform.SetParent(_boneMap[_boneList[0]]);

            currentEquipment.transform.localPosition = new Vector3(0.02f, 0.06f, -0.03f);
            currentEquipment.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        }
    }
}
