using UnityEngine;
using UnityEditor;
using Managers;  // Inventory가 속한 네임스페이스 포함

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Inventory inventory = (Inventory)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("_items Dictionary", EditorStyles.boldLabel);

        if (inventory.Items == null || inventory.Items.Count == 0)
        {
            EditorGUILayout.LabelField("No items found.");
            return;
        }

        foreach (var pair in inventory.Items)
        {
            EditorGUILayout.LabelField(pair.Key.ToString(), EditorStyles.boldLabel);

            if (pair.Value == null || pair.Value.Count == 0)
            {
                EditorGUILayout.LabelField("  (empty)");
                continue;
            }

            foreach (var part in pair.Value)
            {
                if (part != null)
                {
                    EditorGUILayout.ObjectField("  Part", part, typeof(PartBase), true);
                }
            }
        }
    }
}
