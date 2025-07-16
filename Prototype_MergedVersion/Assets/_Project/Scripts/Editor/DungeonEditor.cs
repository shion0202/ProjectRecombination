using UnityEditor;
using UnityEngine;

namespace Jaeho.DungeonScript
{
    [CustomEditor(typeof(DungeonManager))]
    public class DungeonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var dungeonManager = (DungeonManager)target;

            if (GUILayout.Button("Generate Normal Stage"))
            {
                dungeonManager.AddLastStage();
            }
            
            if (GUILayout.Button("Generate Boss Stage"))
            {
                dungeonManager.AddBossStage();
            }

            if (GUILayout.Button("Remove Without Boss Stage"))
            {
                dungeonManager.RemoveLastStageWithoutBossStage();
            }

            if (GUILayout.Button("Remove Boss Stage"))
            {
                dungeonManager.RemoveBossStage();
            }
        }
    }
}