using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ECameraState
{
    Normal = 0,
    Zoom = 1
}

[ExecuteInEditMode]
public class FollowCameraController : MonoBehaviour
{
    #region Variables
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer _cameraBody;
    private CinemachinePOV _cameraAim;

    [SerializeField] private ECameraState currentCameraState = ECameraState.Normal;
    private Dictionary<ECameraState, FollowCameraData> _cameraSettings = new Dictionary<ECameraState, FollowCameraData>();
    private GameObject _owner;
    private Transform _cameraTarget;
    private bool _isBeforeZoom = false;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilRecoverySpeed = 20.0f;
    private float _currentRecoilX = 0.0f;
    private float _currentRecoilY = 0.0f;

    [Header("Gizmos")]
    private Color deadZoneColor = Color.red;
    private Color softZoneColor = Color.blue;
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

    #region Editor Methods
#if UNITY_EDITOR
    private void Update()
    {
        foreach (ECameraState state in Enum.GetValues(typeof(ECameraState)))
        {
            _cameraSettings[state] = Resources.Load<FollowCameraData>($"Camera/FollowCameraData_{state}");
        }

        _cameraBody = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        _cameraAim = vcam.GetCinemachineComponent<CinemachinePOV>();

        ApplyCameraSettings();
    }

    void OnDrawGizmos()
    {
        if (vcam == null) return;

        var framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Dead Zone
        Gizmos.color = deadZoneColor;
        DrawScreenRect(cam, framingTransposer.m_DeadZoneWidth, framingTransposer.m_DeadZoneHeight, framingTransposer.m_CameraDistance);

        // Soft Zone
        Gizmos.color = softZoneColor;
        DrawScreenRect(cam, framingTransposer.m_SoftZoneWidth, framingTransposer.m_SoftZoneHeight, framingTransposer.m_CameraDistance);
    }

    void DrawScreenRect(Camera cam, float width, float height, float distance)
    {
        float width_world = 2.56f;
        float height_world = 1.43f;

        float w = width * width_world * distance;
        float h = height * height_world * distance;
        Vector3 center = cam.transform.position + cam.transform.forward * (distance + 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(w, h, 0.01f));
    }
#endif
    #endregion

    #region Public Methods
    public void InitFollowCamera(GameObject owner)
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

        _cameraAim.m_HorizontalAxis.Value = owner.transform.localEulerAngles.y;

        ApplyCameraSettings();
    }

    // Update에서 매 프레임마다 실행되는 카메라 관련 함수
    public void UpdateFollowCamera()
    {
        ApplyCameraSettings();
        HandleRecoil();
    }

    public void ApplyRecoil(CinemachineImpulseSource source, float recoilX, float recoilY)
    {
        _currentRecoilX = 0.0f;
        _currentRecoilY = 0.0f;

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
        vcam.m_Lens.FieldOfView = _cameraSettings[currentCameraState].FOV;
        _cameraBody.m_ScreenX = _cameraSettings[currentCameraState].screenX;
        _cameraBody.m_ScreenY = _cameraSettings[currentCameraState].screenY;
        _cameraBody.m_CameraDistance = _cameraSettings[currentCameraState].cameraDistance;

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

        _cameraBody.m_TrackedObjectOffset = _cameraSettings[currentCameraState].trackedOffset;
        _cameraBody.m_LookaheadTime = _cameraSettings[currentCameraState].lookaheadTime;
        _cameraBody.m_LookaheadSmoothing = _cameraSettings[currentCameraState].lookaheadSmoothing;
        _cameraBody.m_LookaheadIgnoreY = _cameraSettings[currentCameraState].ignoreLookaheadY;
        _cameraBody.m_XDamping = _cameraSettings[currentCameraState].dampingX;
        _cameraBody.m_YDamping = _cameraSettings[currentCameraState].dampingY;
        _cameraBody.m_ZDamping = _cameraSettings[currentCameraState].dampingZ;
        _cameraBody.m_TargetMovementOnly = _cameraSettings[currentCameraState].targetMovementOnly;
        _cameraBody.m_DeadZoneWidth = _cameraSettings[currentCameraState].deadZoneWidth;
        _cameraBody.m_DeadZoneHeight = _cameraSettings[currentCameraState].deadZoneHeight;
        _cameraBody.m_DeadZoneDepth = _cameraSettings[currentCameraState].deadZoneDepth;
        _cameraBody.m_SoftZoneWidth = _cameraSettings[currentCameraState].softZoneWidth;
        _cameraBody.m_SoftZoneHeight = _cameraSettings[currentCameraState].softZoneHeight;
        _cameraBody.m_BiasX = _cameraSettings[currentCameraState].softZoneOffsetX;
        _cameraBody.m_BiasY = _cameraSettings[currentCameraState].softZoneOffsetY;
    }

    // 프레임 단위로 노말/줌 카메라로 위치를 전환하는 함수
    private void HandleZoom()
    {
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, _cameraSettings[currentCameraState].FOV, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
        _cameraBody.m_ScreenX = Mathf.Lerp(_cameraBody.m_ScreenX, _cameraSettings[currentCameraState].screenX, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
        _cameraBody.m_ScreenY = Mathf.Lerp(_cameraBody.m_ScreenY, _cameraSettings[currentCameraState].screenY, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);
        _cameraBody.m_CameraDistance = Mathf.Lerp(_cameraBody.m_CameraDistance, _cameraSettings[currentCameraState].cameraDistance, _cameraSettings[currentCameraState].convertSpeed * Time.deltaTime);

        ApplyCameraSettings();
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
