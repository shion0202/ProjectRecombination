using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Remove Missing Scripts")]
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

    [MenuItem("Assets/Selection Delete Missing Scripts")]
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
#endif
}
