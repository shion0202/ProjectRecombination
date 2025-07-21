using UnityEditor;
using UnityEngine;

namespace Jaeho.DungeonScript
{
    [CustomEditor(typeof(StageController))]
    public class StageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var stageController = (StageController)target;

            if (GUILayout.Button("Generate Wave"))
            {
                stageController.AddLastWave();
            }
            
            if (GUILayout.Button("Remove Wave"))
            {
                stageController.RemoveLastWave();
            }
        }
    }
}