using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ECameraState
{
    Normal = 0,
    Zoom = 1
}

public class FollowCameraController : MonoBehaviour
{
    private CinemachineVirtualCamera _vcam;
    private CinemachineFramingTransposer _cameraBody;
    private CinemachinePOV _cameraAim;

    private PlayerController _owner;
    private Dictionary<ECameraState, FollowCameraData> _cameraSettings = new Dictionary<ECameraState, FollowCameraData>();

    private Transform _cameraTarget;
    public Transform CameraTarget
    {
        get { return _cameraTarget; }
    }

    [SerializeField] private ECameraState currentCameraState = ECameraState.Normal;
    public ECameraState CurrentCameraState
    {
        get { return currentCameraState; }
    }

    public bool IsZoomed
    {
        get { return currentCameraState == ECameraState.Zoom; }
        set
        {
            switch (value)
            {
                case true:
                    currentCameraState = ECameraState.Zoom;
                    break;
                case false:
                    currentCameraState = ECameraState.Normal;
                    break;
            }

            ApplyCameraSettings();
        }
    }

    private bool _isZoomedBefore = false; // Play other actions using zoom, replay zoom after action ended.

    [SerializeField] private float zoomSpeed = 10.0f;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineVirtualCamera>();

        for (int i = 0; i < Enum.GetNames(typeof(ECameraState)).Length; ++i)
        {
            _cameraSettings.Add((ECameraState)i, Resources.Load<FollowCameraData>($"Camera/FollowCameraData_{(ECameraState)i}"));
        }
    }

    public void SetFollowCamera(PlayerController owner)
    {
        _owner = owner;

        CameraTarget target = _owner.gameObject.GetComponentInChildren<CameraTarget>();
        if (target != null)
        {
            _cameraTarget = target.transform;
        }

        _cameraBody = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cameraAim = _vcam.GetCinemachineComponent<CinemachinePOV>();

        GetComponent<CinemachineVirtualCamera>().m_LookAt = owner.gameObject.GetComponentInChildren<CameraTarget>().transform;
        GetComponent<CinemachineVirtualCamera>().m_Follow = owner.gameObject.GetComponentInChildren<CameraTarget>().transform;

        ApplyCameraSettings();
    }

    private void ApplyCameraSettings()
    {
        // Set camera setting by camera state.
        _cameraAim.m_HorizontalAxis.m_MaxValue = _cameraSettings[currentCameraState].maxAimRangeX;
        _cameraAim.m_HorizontalAxis.m_MinValue = _cameraSettings[currentCameraState].minAimRangeX;
        _cameraAim.m_VerticalAxis.m_MaxValue = _cameraSettings[currentCameraState].maxAimRangeY;
        _cameraAim.m_VerticalAxis.m_MinValue = _cameraSettings[currentCameraState].minAimRangeY;
        _cameraAim.m_HorizontalAxis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
        _cameraAim.m_HorizontalAxis.m_MaxSpeed = _cameraSettings[currentCameraState].sensitivityX;
        _cameraAim.m_VerticalAxis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
        _cameraAim.m_VerticalAxis.m_MaxSpeed = _cameraSettings[currentCameraState].sensitivityY;
    }

    public void UpdateFollowCamera()
    {
        HandleZoom();
    }

    private void HandleZoom()
    {
        _vcam.m_Lens.FieldOfView = Mathf.Lerp(_vcam.m_Lens.FieldOfView, _cameraSettings[currentCameraState].FOV, zoomSpeed * Time.deltaTime);
        _cameraBody.m_ScreenX = Mathf.Lerp(_cameraBody.m_ScreenX, _cameraSettings[currentCameraState].screenX, zoomSpeed * Time.deltaTime);
        _cameraBody.m_ScreenY = Mathf.Lerp(_cameraBody.m_ScreenY, _cameraSettings[currentCameraState].screenY, zoomSpeed * Time.deltaTime);
        _cameraBody.m_CameraDistance = Mathf.Lerp(_cameraBody.m_CameraDistance, _cameraSettings[currentCameraState].cameraDistance, zoomSpeed * Time.deltaTime);
    }
}
