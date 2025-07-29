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
    #region Variables
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer _cameraBody;
    private CinemachinePOV _cameraAim;

    [SerializeField] private ECameraState currentCameraState = ECameraState.Normal;
    private Dictionary<ECameraState, FollowCameraData> _cameraSettings = new Dictionary<ECameraState, FollowCameraData>();
    private PlayerController _owner;
    private Transform _cameraTarget;
    private bool _isBeforeZoom = false;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilRecoverySpeed = 20.0f;
    private float _currentRecoilX = 0.0f;
    private float _currentRecoilY = 0.0f;
    #endregion

    #region Properties
    public ECameraState CurrentCameraState => currentCameraState;
    public Transform CameraTarget => _cameraTarget;

    public bool IsBeforeZoom
    {
        get { return _isBeforeZoom; }
        set { _isBeforeZoom = value; }
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
    #endregion

    #region Public Methods
    public void InitFollowCamera(PlayerController owner)
    {
        _owner = owner;

        CameraTarget target = _owner.gameObject.GetComponentInChildren<CameraTarget>();
        if (target != null)
        {
            _cameraTarget = target.transform;
        }

        vcam.m_LookAt = _cameraTarget;
        vcam.m_Follow = _cameraTarget;

        _cameraBody = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cameraAim = vcam.GetCinemachineComponent<CinemachinePOV>();

        foreach (ECameraState state in Enum.GetValues(typeof(ECameraState)))
        {
            _cameraSettings[state] = Resources.Load<FollowCameraData>($"Camera/FollowCameraData_{state}");
        }

        ApplyCameraSettings();
    }

    // Update에서 매 프레임마다 실행되는 카메라 관련 함수
    public void UpdateFollowCamera()
    {
        HandleZoom();
        HandleRecoil();
    }

    public void ApplyRecoil(CinemachineImpulseSource source, float recoilX, float recoilY)
    {
        _currentRecoilX += recoilX * (UnityEngine.Random.value > 0.5f ? 1 : -1);
        _currentRecoilY += recoilY;
        ApplyShake(source);
    }

    public void ApplyShake(CinemachineImpulseSource source)
    {
        source.m_DefaultVelocity.x = source.m_DefaultVelocity.x * (UnityEngine.Random.value > 0.5f ? 1 : -1);
        source.m_DefaultVelocity.y = source.m_DefaultVelocity.y * (UnityEngine.Random.value > 0.5f ? 1 : -1);
        source.GenerateImpulse();
    }
    #endregion

    #region Private Methods
    private void ApplyCameraSettings()
    {
        _cameraAim.m_HorizontalAxis.m_MaxValue = _cameraSettings[currentCameraState].maxAimRangeX;
        _cameraAim.m_HorizontalAxis.m_MinValue = _cameraSettings[currentCameraState].minAimRangeX;
        _cameraAim.m_VerticalAxis.m_MaxValue = _cameraSettings[currentCameraState].maxAimRangeY;
        _cameraAim.m_VerticalAxis.m_MinValue = _cameraSettings[currentCameraState].minAimRangeY;
        _cameraAim.m_HorizontalAxis.m_MaxSpeed = _cameraSettings[currentCameraState].sensitivityX;
        _cameraAim.m_VerticalAxis.m_MaxSpeed = _cameraSettings[currentCameraState].sensitivityY;
        _cameraAim.m_HorizontalAxis.m_AccelTime = _cameraSettings[currentCameraState].accelTimeX;
        _cameraAim.m_HorizontalAxis.m_DecelTime = _cameraSettings[currentCameraState].decelTimeX;
        _cameraAim.m_VerticalAxis.m_AccelTime = _cameraSettings[currentCameraState].accelTimeY;
        _cameraAim.m_VerticalAxis.m_DecelTime = _cameraSettings[currentCameraState].decelTimeY;
    }

    // 프레임 단위로 노말/줌 카메라로 위치를 전환하는 함수
    private void HandleZoom()
    {
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, _cameraSettings[currentCameraState].FOV, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
        _cameraBody.m_ScreenX = Mathf.Lerp(_cameraBody.m_ScreenX, _cameraSettings[currentCameraState].screenX, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
        _cameraBody.m_ScreenY = Mathf.Lerp(_cameraBody.m_ScreenY, _cameraSettings[currentCameraState].screenY, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
        _cameraBody.m_CameraDistance = Mathf.Lerp(_cameraBody.m_CameraDistance, _cameraSettings[currentCameraState].cameraDistance, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
    }

    private void HandleRecoil()
    {
        if (_currentRecoilX != 0 || _currentRecoilY > 0)
        {
            _cameraAim.m_HorizontalAxis.Value += _currentRecoilX * Time.deltaTime;
            _cameraAim.m_VerticalAxis.Value -= _currentRecoilY * Time.deltaTime;

            float recoveryStep = recoilRecoverySpeed * Time.deltaTime;
            _currentRecoilX = Mathf.MoveTowards(_currentRecoilX, 0, recoveryStep);
            _currentRecoilY = Mathf.Max(0, _currentRecoilY - recoveryStep);
        }
    }
    #endregion
}
