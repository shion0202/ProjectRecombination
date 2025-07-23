using UnityEditor;
using UnityEngine;

// ������ ���ӿ�����Ʈ�� Bone �����͸� ��ũ���ͺ� ������Ʈ�� �����ϴ� ������ ��ũ��Ʈ
public class BoneLoader : MonoBehaviour
{
    [MenuItem("Tools/Save Bone Data To Scriptable Object", priority = 100)]
    public static void SaveBoneDataToSO()
    {
        Object selectedObj = Selection.activeObject;
        if (selectedObj == null)
        {
            Debug.LogWarning("������Ʈ�� �����ϼ���.");
            return;
        }

        GameObject targetGO = null;

        // 1. ������Ʈ â���� ������ ���� ���� ��
        if (PrefabUtility.IsPartOfPrefabAsset(selectedObj))
        {
            targetGO = PrefabUtility.InstantiatePrefab(selectedObj) as GameObject;
        }
        // 2. ������ GameObject ���� ��
        else if (selectedObj is GameObject)
        {
            targetGO = selectedObj as GameObject;
        }

        if (targetGO == null)
        {
            Debug.LogWarning("GameObject�� ã�� �� �����ϴ�.");
            return;
        }

        // ���� ��� �Է�
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Bone Data",
            "BoneData",
            "asset",
            "ScriptableObject�� ������ ��ο� ���ϸ��� �Է��ϼ���."
        );
        if (string.IsNullOrEmpty(path))
            return;

        // ScriptableObject ���� �� ������ �Է�
        CharacterBoneData boneMapSO = ScriptableObject.CreateInstance<CharacterBoneData>();

        var targetMeshBone = targetGO.GetComponentInChildren<TargetMeshBone>();
        var smr = targetMeshBone.gameObject.GetComponent<SkinnedMeshRenderer>();
        foreach (var bone in smr.bones)
        {
            if (bone == null)
            {
                boneMapSO.boneNames.Add("");
                continue;
            }

            boneMapSO.boneNames.Add(bone.name);
        }

        AssetDatabase.CreateAsset(boneMapSO, path);
        AssetDatabase.SaveAssets();

        // ������ ���¿��� �ӽ÷� �ν��Ͻ�ȭ�� ��� ����
        if (PrefabUtility.IsPartOfPrefabAsset(selectedObj) && targetGO != null)
        {
            GameObject.DestroyImmediate(targetGO);
        }

        Debug.Log($"Bone Data ���� �Ϸ�: {path}");
        AssetDatabase.Refresh();
    }
}
