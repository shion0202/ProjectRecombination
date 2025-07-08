using Cinemachine;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEditor.SceneView;

[CustomEditor(typeof(FollowCameraController))]
public class CubeGenerateButton : Editor
{
    private string _savePath = "Assets/";
    FollowCameraController controller;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        controller = (FollowCameraController)target;

        EditorGUILayout.BeginHorizontal();
        _savePath = EditorGUILayout.TextField("저장 경로", _savePath);
        if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("스크립터블 오브젝트 저장 경로 선택", _savePath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    // Need to save path
                    _savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("경고", "Assets 폴더 내의 경로만 사용 가능합니다.", "확인");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("현재 설정 저장"))
        {
            SaveCameraSetting();
        }
    }

    private void SaveCameraSetting()
    {
        string stateName = controller.CameraState.ToString();
        string assetPath = Path.Combine(_savePath, $"FollowCameraData_{stateName}.asset");

        FollowCameraData setting = AssetDatabase.LoadAssetAtPath<FollowCameraData>(assetPath);
        if (setting == null)
        {
            setting = CreateInstance<FollowCameraData>();
            AssetDatabase.CreateAsset(setting, assetPath);
        }

        CinemachineVirtualCamera vcam = controller.GetComponent<CinemachineVirtualCamera>();
        setting.FOV = vcam.m_Lens.FieldOfView;

        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"저장 완료: {assetPath}");
    }
}
