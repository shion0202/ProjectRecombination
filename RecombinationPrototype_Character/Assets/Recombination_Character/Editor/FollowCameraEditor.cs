using Cinemachine;
using System.IO;
using UnityEditor;
using UnityEngine;
using static UnityEditor.SceneView;

[CustomEditor(typeof(FollowCameraController))]
public class CubeGenerateButton : Editor
{
    private const string _savePathKey = "FollowCameraController_SavePath";
    private string _savePath;

    private void OnEnable()
    {
        _savePath = EditorPrefs.GetString(_savePathKey, "Assets/");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        _savePath = EditorGUILayout.TextField("���� ���", _savePath);
        if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("��ũ���ͺ� ������Ʈ ���� ��� ����", _savePath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    // Need to save path
                    _savePath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    if (_savePath != EditorPrefs.GetString(_savePathKey))
                    {
                        EditorPrefs.SetString(_savePathKey, _savePath);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("���", "Assets ���� ���� ��θ� ��� �����մϴ�.", "Ȯ��");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("���� ���� ����"))
        {
            SaveCameraSetting();
        }
    }

    private void SaveCameraSetting()
    {
        FollowCameraController controller = (FollowCameraController)target;
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
        Debug.Log($"���� �Ϸ�: {assetPath}");
    }
}
