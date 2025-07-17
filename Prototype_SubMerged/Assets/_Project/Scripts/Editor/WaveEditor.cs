using UnityEditor;
using UnityEngine;

namespace Jaeho.DungeonScript
{
    [CustomEditor(typeof(WaveController))]
    public class WaveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            var waveController = (WaveController)target;

            if (GUILayout.Button("Add Spawner"))
            {
                waveController.GenerateSpawner();
            }

            if (GUILayout.Button("Delete Last Spawner"))
            {
                waveController.DeleteLastSpawner();
            }
        }
    }
}