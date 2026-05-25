using Cinemachine;
using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RapidPlayer : MonoBehaviour, PlayerActions.IJumpAttackActionMapActions
{
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float time = 5.0f;
    private Vector2 _moveInput = Vector2.zero;
    private Rigidbody rb;
    private PlayerActions _playerActions;
    private Camera _cam;
    private float _currentTime = 0.0f;

    private PlayerController _owner;
    private LegsEnhanced _originalPart;

    protected CinemachineVirtualCamera vcam; // 컴포넌트 직접 캐싱을 위해 변경
    protected CinemachinePOV pov;
    protected CinemachineBrain brain;
    protected CinemachineBlendDefinition defaultBlend;

    private bool _isExiting = false;
    private Vector3 _originalPlayerPos; // 취소 시 복귀를 위한 위치 백업

    public PlayerController Owner
    {
        get => _owner;
        set => _owner = value;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _cam = Camera.main;

        // 기존 인풋 에셋 생성 및 콜백 연결 구조 유지
        _playerActions = new PlayerActions();
        _playerActions.JumpAttackActionMap.SetCallbacks(this);

        if (pov == null)
        {
            vcam = gameObject.GetComponentInChildren<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                pov = vcam.GetCinemachineComponent<CinemachinePOV>();
            }
        }

        brain = Camera.main.GetComponent<CinemachineBrain>();
        defaultBlend = brain.m_DefaultBlend;
        brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0.0f);
    }

    private void OnEnable()
    {
        _playerActions.JumpAttackActionMap.Enable();
        _currentTime = time;
    }

    private void Start()
    {
        GUIManager.Instance.GameUIController.SetLegsSkillTimer(new Color(0.45f, 0.59f, 0.59f));
        GUIManager.Instance.GameUIController.SetLegsSkillIcon(true);
        GUIManager.Instance.GameUIController.SetLegsSkillCooldown(true);
        GUIManager.Instance.GameUIController.RapidInfo.SetActive(true);
    }

    private void OnDisable()
    {
        _playerActions.JumpAttackActionMap.Disable();
    }

    private void Update()
    {
        if (_isExiting) return;

        _currentTime -= Time.deltaTime;
        GUIManager.Instance.GameUIController.SetLegsSkillCooldown(_currentTime);
        GUIManager.Instance.GameUIController.SetRapidCooldownText(_currentTime);
        if (_currentTime <= 0.0f)
        {
            Apply();
        }

        Move();
    }

    public void Init(PlayerController owner, LegsEnhanced origin, float horizontalValue)
    {
        _owner = owner;
        _originalPart = origin;
        _originalPlayerPos = _owner.transform.position;

        if (vcam != null && pov == null)
        {
            pov = vcam.GetCinemachineComponent<CinemachinePOV>();
        }
        if (pov != null)
        {
            pov.m_HorizontalAxis.Value = horizontalValue;
        }

        if (_owner != null && _owner.FollowCamera != null && pov != null)
        {
            _owner.FollowCamera.SyncCameraRotation(pov, true);
            _owner.FollowCamera.SetTargetPOV(pov);
        }

        if (_owner != null)
        {
            _owner.Controller.enabled = false;
            _owner.transform.position = Vector3.down * 9999f;
            _owner.Controller.enabled = true;
        }

        // 시네머신 브레인이 이 카메라의 존재를 인지하고 화면을 완전히 장악할 수 있도록 
        // 플레이어를 지하로 보내는 타이밍만 코루틴으로 '딱 1프레임' 뒤로 미룹니다.
        //StartCoroutine(TeleportOwnerToUndergroundNextFrame());
    }

    private void Move()
    {
        if (_moveInput == Vector2.zero)
        {
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(0f, currentVelocity.y, 0f);
            return;
        }

        Vector3 camForward = -_cam.transform.forward;
        Vector3 camRight = _cam.transform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * -_moveInput.y + camRight * _moveInput.x;
        rb.velocity = new Vector3(moveDirection.normalized.x * speed, rb.velocity.y, moveDirection.normalized.z * speed);
    }

    private void Apply()
    {
        if (_isExiting) return;
        _isExiting = true;

        // 지하에 있던 플레이어를 지상(현재 스킬 최종 위치)으로 먼저 소환
        _owner.Controller.enabled = false;
        _owner.transform.position = transform.position;
        _owner.ResetGravityAndFalling();
        Physics.SyncTransforms();
        _owner.Controller.enabled = true;

        _originalPart.IsAttack = true;
        RestoreCameraAngle();

        // 주도권을 돌려주기 전, 플레이어 카메라 내부의 지하 잔상 데이터를 완벽하게 워프 세탁합니다.
        // 복귀하기 전에 제어 대상을 다시 원래 플레이어 POV로 원상복구
        if (_owner.FollowCamera != null)
        {
            _owner.FollowCamera.SyncCameraRotation(pov, false);
            _owner.FollowCamera.SetTargetPOV(null);
            _owner.FollowCamera.WarpToTarget();
        }

        // 플레이어가 완전히 지상에 올라온 것을 확인한 뒤 우선순위를 원복하여 카메라 블렌딩 유도
        if (vcam != null) vcam.Priority = 0;

        _originalPart.OnJumpAttackWindowClosed();

        brain.m_DefaultBlend = defaultBlend;
        Utils.Destroy(gameObject);
    }

    public void OnApply(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Apply();
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_isExiting) return;
            _isExiting = true;

            // 1) 취소 시 플레이어를 원래 시전했던 제자리로 먼저 소환
            _owner.Controller.enabled = false;
            _owner.transform.position = _originalPlayerPos;
            _owner.ResetGravityAndFalling();
            Physics.SyncTransforms();
            _owner.Controller.enabled = true;

            _originalPart.IsAttack = false;
            RestoreCameraAngle();

            if (_owner.FollowCamera != null)
            {
                _owner.FollowCamera.SetTargetPOV(null);
                _owner.FollowCamera.WarpToTarget();
            }

            // 2) 플레이어 지상 안착 후 카메라 주도권 반환
            if (vcam != null) vcam.Priority = 0;

            _originalPart.OnJumpAttackWindowClosed();

            brain.m_DefaultBlend = defaultBlend;
            Utils.Destroy(gameObject);
        }
    }

    private void RestoreCameraAngle()
    {
        if (pov != null && _owner.FollowCamera != null)
        {
            Transform skillVcamTransform = pov.VirtualCamera.transform;
            Vector3 worldForward = skillVcamTransform.forward;
            float yaw = GetYawFromForward(worldForward);

            var playerPov = _owner.FollowCamera.CameraAim;
            playerPov.m_HorizontalAxis.Value = Mathf.Clamp(
                yaw,
                playerPov.m_HorizontalAxis.m_MinValue,
                playerPov.m_HorizontalAxis.m_MaxValue
            );
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (_isExiting) return;

        if (context.canceled)
        {
            _moveInput = Vector2.zero;
            return;
        }

        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }

    float GetYawFromForward(Vector3 forward)
    {
        float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        return Mathf.DeltaAngle(0f, yaw);
    }
}
