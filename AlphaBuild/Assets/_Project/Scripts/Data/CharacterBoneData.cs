using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ĳ���� �� �����͸� �����ϱ� ���� ��ũ���ͺ� ������Ʈ Ŭ����
[CreateAssetMenu(menuName = "Scriptable Object/Character Bone Data", order = 10)]
public class CharacterBoneData : ScriptableObject
{
    public List<string> boneNames = new List<string>();
}
