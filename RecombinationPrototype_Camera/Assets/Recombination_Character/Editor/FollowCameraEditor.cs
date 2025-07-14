using Cinemachine;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FollowCameraController))]
public class CubeGenerateButton : Editor
{
    private const string _savePathKey = "FollowCameraController_SavePath";
    private string _savePath;

    private FollowCameraController _controller;
    private CinemachineVirtualCamera _vcam;
    private CinemachineFramingTransposer _cameraBody;
    private CinemachinePOV _cameraAim;

    private void OnEnable()
    {
        _controller = (FollowCameraController)target;

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

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("�ҷ�����"))
        {
            LoadCameraSetting();
        }

        if (GUILayout.Button("����"))
        {
            SaveCameraSetting();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SaveCameraSetting()
    { 
        string stateName = _controller.CurrentCameraState.ToString();
        string assetPath = Path.Combine(_savePath, $"FollowCameraData_{stateName}.asset");

        FollowCameraData setting = AssetDatabase.LoadAssetAtPath<FollowCameraData>(assetPath);
        if (setting == null)
        {
            setting = CreateInstance<FollowCameraData>();
            AssetDatabase.CreateAsset(setting, assetPath);
        }

        _vcam = _controller.GetComponent<CinemachineVirtualCamera>();
        _cameraBody = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cameraAim = _vcam.GetCinemachineComponent<CinemachinePOV>();

        setting.FOV = _vcam.m_Lens.FieldOfView;
        setting.screenX = _cameraBody.m_ScreenX;
        setting.screenY = _cameraBody.m_ScreenY;
        setting.cameraDistance = _cameraBody.m_CameraDistance;
        setting.maxAimRangeX = _cameraAim.m_HorizontalAxis.m_MaxValue;
        setting.minAimRangeX = _cameraAim.m_HorizontalAxis.m_MinValue;
        setting.maxAimRangeY = _cameraAim.m_VerticalAxis.m_MaxValue;
        setting.minAimRangeY = _cameraAim.m_VerticalAxis.m_MinValue;
        setting.sensitivityX = _cameraAim.m_HorizontalAxis.m_MaxSpeed;
        setting.sensitivityY = _cameraAim.m_VerticalAxis.m_MaxSpeed;

        EditorUtility.SetDirty(setting);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"���� �Ϸ�: {assetPath}");
    }

    private void LoadCameraSetting()
    {
        string stateName = _controller.CurrentCameraState.ToString();
        string assetPath = Path.Combine(_savePath, $"FollowCameraData_{stateName}.asset");

        FollowCameraData setting = AssetDatabase.LoadAssetAtPath<FollowCameraData>(assetPath);
        if (setting == null)
        {
            EditorUtility.DisplayDialog("�ҷ����� ����", $"��ο� {stateName}.asset ������ �����ϴ�.", "Ȯ��");
            return;
        }

        Undo.RecordObject(_controller, "ī�޶� ���� �ҷ�����");

        _vcam = _controller.GetComponent<CinemachineVirtualCamera>();
        _cameraBody = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cameraAim = _vcam.GetCinemachineComponent<CinemachinePOV>();

        _vcam.m_Lens.FieldOfView = setting.FOV;
        _cameraBody.m_ScreenX = setting.screenX;
        _cameraBody.m_ScreenY = setting.screenY;
        _cameraBody.m_CameraDistance = setting.cameraDistance;
        _cameraAim.m_HorizontalAxis.m_MaxValue = setting.maxAimRangeX;
        _cameraAim.m_HorizontalAxis.m_MinValue = setting.minAimRangeX;
        _cameraAim.m_VerticalAxis.m_MaxValue = setting.maxAimRangeY;
        _cameraAim.m_VerticalAxis.m_MinValue = setting.minAimRangeY;
        _cameraAim.m_HorizontalAxis.m_MaxSpeed = setting.sensitivityX;
        _cameraAim.m_VerticalAxis.m_MaxSpeed = setting.sensitivityY;

        EditorUtility.SetDirty(_controller);
        Debug.Log($"�ҷ����� �Ϸ�: {assetPath}");
    }
}
