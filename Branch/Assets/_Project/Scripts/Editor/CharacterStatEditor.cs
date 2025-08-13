#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterStat))]
public class CharacterStatEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var script = (CharacterStat)target;
        GUILayout.Space(10);
        if (GUILayout.Button("런타임 → 인스펙터 동기화"))
        {
            script.SyncToInspector();
        }
        if (GUILayout.Button("인스펙터 → 런타임 반영"))
        {
            script.SyncFromInspector();
            script.CalculateStatsForced();
        }
    }
}
#endif
