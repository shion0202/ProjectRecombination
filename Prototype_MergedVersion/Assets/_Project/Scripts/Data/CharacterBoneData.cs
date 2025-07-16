using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Character Bone Data", order = 0)]
public class CharacterBoneData : ScriptableObject
{
    public List<string> boneNames = new List<string>();
}
