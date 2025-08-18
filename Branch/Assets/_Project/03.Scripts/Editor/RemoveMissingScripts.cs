using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 선택한 게임오브젝트에서 Missing Script를 제거하는 에디터 클래스
#if UNITY_EDITOR
public class RemoveMissingScripts : MonoBehaviour
{
    // 메뉴 항목을 통해 선택한 프리팹 또는 게임오브젝트에서 Missing Script를 제거
    [MenuItem("Tools/Remove Missing Scripts", priority = 100)]
    public static void DeleteMissingScriptInPrefabs()
    {
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        List<GameObject> list = new List<GameObject>();
        foreach (string prefabGUID in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUID);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            list.Add(prefab);
        }
        if (list.Count > 0)
            DeleteInternal(list);
    }

    // 우클릭 메뉴를 통해 선택한 프리팹에서 Missing Script를 제거
    [MenuItem("Assets/Selection Delete Missing Scripts", priority = 100)]
    public static void DeleteMissingScriptInSelections()
    {
        List<GameObject> list = Selection.gameObjects.ToList();
        if (list.Count > 0)
            DeleteInternal(list);
    }

    private static void DeleteInternal(List<GameObject> list)
    {
        int deleteCount = 0;
        foreach (GameObject obj in list)
            RecursiveDeleteMissingScript(obj, ref deleteCount);

        if (deleteCount > 0)
        {
            Debug.LogWarning($"{nameof(RemoveMissingScripts)} : Delete Missing Script Count {deleteCount} ");
            AssetDatabase.SaveAssets();
        }
    }

    private static void RecursiveDeleteMissingScript(GameObject obj, ref int deleteCount)
    {
        int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj);
        if (missingCount > 0)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            deleteCount++;
        }
        foreach (Transform childTransform in obj.GetComponentsInChildren<Transform>(true))
        {
            GameObject child = childTransform.gameObject;
            missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child);
            if (missingCount > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child);
                deleteCount++;
            }
        }
    }
}
#endif
