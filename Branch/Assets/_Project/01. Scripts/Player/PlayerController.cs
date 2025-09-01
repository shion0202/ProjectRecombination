using Cinemachine;
using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

[Flags]
public enum EPlayerState
{
    Idle = 1 << 0,
    Moving = 1 << 1,
    Falling = 1 << 2,
    Dashing = 1 << 3,
    LeftShooting = 1 << 4,
    RightShooting = 1 << 5,
    Zooming = 1 << 6,
    Rotating = 1 << 7,

    RotateState = Moving | LeftShooting | RightShooting | Zooming,
    ActionState = Idle | Moving | Dashing | LeftShooting | RightShooting | Zooming,
    ShootState = LeftShooting | RightShooting,
}

public enum EHealRange
{
    All = 0,
    Body = 1,
    Part = 2,
}

public class PlayerController : MonoBehaviour, PlayerActions.IPlayerActionMapActions
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private CharacterStat stats;
    [SerializeField] private Inventory inventory;
    [SerializeField] private GameObject followCameraPrefab;
    private FollowCameraController _followCamera;

    [Header("State")]
    [SerializeField] private EPlayerState movementBlockMask = EPlayerState.Dashing;
    [SerializeField] private EPlayerState dashBlockMask;
    [SerializeField] private EPlayerState shootBlockMask = EPlayerState.Dashing;
    [SerializeField] private EPlayerState zoomBlockMask = EPlayerState.Dashing;
    private EPlayerState _currentPlayerState = EPlayerState.Idle;
    private EPlayerState _previousState = 0;
    private bool _isLeftAttackReady = false;
    private bool _isRightAttackReady = false;

    [Header("Movement")]
    [SerializeField, Range(0.01f, 100.0f)] private float rotationSpeed = 40.0f;
    private PlayerActions _playerActions;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _totalDirection = Vector3.zero;
    private bool _canMove = true;
    private ILegsMovement _currentMovement;

    [Header("Gravity")]
    [SerializeField] private Vector3 boxSize = new Vector3(0.2f, 0.01f, 0.2f);
    [SerializeField] private float gravityScale = 2.0f;
    [SerializeField] private LayerMask groundLayerMask;
    private bool _isGrounded = false;
    private bool _isOnPlatform = false;
    private Vector3 _fallVelocity;
    private Transform _postPlatform;
    private Vector3 _lastPlatformPosition = Vector3.zero;
    private Vector3 _platformVelocity;

    [Header("Dash")]
    private Vector3 _dashDirection = Vector3.zero;
    private float _dashSpeed = 0.0f;

    private static event Action OnInteractionKeyPressed;
    #endregion

    #region Properties
    public CharacterStat Stats
    {
        get => stats;
    }

    public FollowCameraController FollowCamera
    {
        get => _followCamera;
    }

    public CharacterController Controller
    {
        get => characterController;
        set => characterController = value;
    }

    public EPlayerState CurrentPlayerState
    {
        get { return _currentPlayerState; }
    }

    public Vector3 FallVelocity
    {
        get { return _fallVelocity; }
    }

    public Vector3 DashDirection
    {
        get { return _dashDirection; }
        set { _dashDirection = value; }
    }

    public float DashSpeed
    {
        get { return _dashSpeed; }
        set { _dashSpeed = value; }
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _playerActions = new PlayerActions();
        _playerActions.PlayerActionMap.SetCallbacks(this);

        _followCamera = FindFirstObjectByType<FollowCameraController>();
        if (_followCamera == null)
        {
            GameObject cameraObject = Instantiate(followCameraPrefab);
            cameraObject.name = followCameraPrefab.name;
            _followCamera = cameraObject.GetComponent<FollowCameraController>();
        }
        _followCamera.InitFollowCamera(gameObject);
        
        // 비트 마스크 방식으로 레이케스트를 관리할 레이어를 설정
        groundLayerMask = ~0;
        groundLayerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
        groundLayerMask &= ~(1 << LayerMask.NameToLayer("Outline"));
        groundLayerMask &= ~(1 << LayerMask.NameToLayer("Player"));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // stats.CurrentHealth = stats.TotalStats[EStatType.MaxHp].Value;
        // GUIManager.instance.SetHpSlider(stats.CurrentHealth, stats.TotalStats[EStatType.MaxHp].Value);
    }

    private void OnEnable()
    {
        _playerActions.PlayerActionMap.Enable();
    }

    private void Start()
    {
        ILegsMovement legsMovement = inventory.EquippedItems[EPartType.Legs] as ILegsMovement;
        _currentMovement = legsMovement;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        
        // Debug.Log("Player HP: " + stats.CurrentHealth);
        // GUI HP 바 갱신
        // TODO: Null Reference
        GUIManager.Instance.SetHpSlider(stats.CurrentHealth, stats.MaxHealth);

        AnimCheckShoot();
    }

    private void LateUpdate()
    {
        HandleMove();
        DashMove();
        HandleGravity();

        _followCamera.UpdateFollowCamera();
        RotateCharacter();

        characterController.Move(_totalDirection * Time.deltaTime);
        _totalDirection = Vector3.zero;
    }

    private void OnDisable()
    {
        _playerActions.PlayerActionMap.Disable();
    }
    #endregion

    #region Input Actions
    void PlayerActions.IPlayerActionMapActions.OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
        if ((_currentPlayerState & movementBlockMask) != 0) return;
    }

    void PlayerActions.IPlayerActionMapActions.OnDash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if ((_currentPlayerState & dashBlockMask) != 0) return;

            if (_moveInput == null || _moveInput == Vector2.zero)
            {
                Vector3 camForward = _followCamera.transform.forward;
                camForward.y = 0.0f;
                _dashDirection = camForward.normalized;
            }
            else
            {
                _dashDirection = CalculateInputDirection();
            }

            inventory.EquippedItems[EPartType.Legs].UseAbility();
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnShoulderSkill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if ((_currentPlayerState & dashBlockMask) != 0) return;

            inventory.EquippedItems[EPartType.Shoulder].UseAbility();
        }
    }

    // 현재 사용 X
    void PlayerActions.IPlayerActionMapActions.OnZoom(InputAction.CallbackContext context)
    {
        if ((_currentPlayerState & zoomBlockMask) != 0) return;

        if (context.started)
        {
            _currentPlayerState &= ~EPlayerState.LeftShooting;
            _currentPlayerState &= ~EPlayerState.RightShooting;
            _currentPlayerState |= EPlayerState.Zooming;
            _followCamera.IsBeforeZoom = true;
            _followCamera.IsZoomed = true;
            animator.SetBool("isAim", true);
        }

        if (context.canceled)
        {
            _currentPlayerState &= ~EPlayerState.LeftShooting;
            _currentPlayerState &= ~EPlayerState.RightShooting;
            _currentPlayerState &= ~EPlayerState.Zooming;
            _followCamera.IsBeforeZoom = false;
            _followCamera.IsZoomed = false;
            animator.SetBool("isAim", false);
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 바닥에 있을 때만 점프 실행
            if (_isGrounded || _isOnPlatform)
            {
                float jumpVelocity = 20.0f;  // 점프 힘, 적절히 조절 가능
                _fallVelocity.y = jumpVelocity;

                // 점프 상태 갱신
                _currentPlayerState |= EPlayerState.Falling;
            }
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnLeftAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _currentPlayerState |= EPlayerState.LeftShooting;
            Shoot();
        }

        if (context.canceled)
        {
            inventory.EquippedItems[EPartType.ArmL].UseCancleAbility();

            animator.SetBool("isLeftAttack", false);
            _previousState &= ~EPlayerState.LeftShooting;
            _currentPlayerState &= ~EPlayerState.LeftShooting;
            _isLeftAttackReady = false;  // 상태 초기화
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnRightAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _currentPlayerState |= EPlayerState.RightShooting;
            Shoot();
        }

        if (context.canceled)
        {
            inventory.EquippedItems[EPartType.ArmR].UseCancleAbility();

            animator.SetBool("isRightAttack", false);
            _previousState &= ~EPlayerState.RightShooting;
            _currentPlayerState &= ~EPlayerState.RightShooting;
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnInteraction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnInteractionKeyPressed?.Invoke();
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnMouseScroll(InputAction.CallbackContext context)
    {
        //_playerActions.PlayerActionMap.MouseScroll.performed += x => _followCamera.ScrollY = x.ReadValue<float>() * 0.02f * -1;
        if (context.performed)
        {
            float scrollValue = context.ReadValue<float>();
            _followCamera.ScrollY = scrollValue * -0.001f;
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnResetCamera(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _followCamera.ResetCamera();
        }
    }
    #endregion

    #region Public Methods
    public void Dash(float dashSpeed)
    {
        _dashSpeed = dashSpeed;

        _previousState = _currentPlayerState & EPlayerState.ShootState;
        animator.SetBool("isLeftAttack", false);
        animator.SetBool("isRightAttack", false);

        _currentPlayerState &= ~EPlayerState.ActionState;
        _currentPlayerState |= EPlayerState.Dashing;
    }

    public void FinishDash()
    {
        _dashDirection = Vector3.zero;
        _dashSpeed = 0.0f;

        _currentPlayerState |= _previousState;
        _currentPlayerState &= ~EPlayerState.Dashing;
        if ((_currentPlayerState & EPlayerState.LeftShooting) != 0)
        {
            Shoot();
        }
            
        if ((_currentPlayerState & EPlayerState.RightShooting) != 0)
        {
            Shoot();
        }

        _previousState = 0;
        SwitchStateToIdle();
    }

    public void ApplyRecoil(CinemachineImpulseSource source, float recoilX, float recoilY)
    {
        _followCamera.ApplyRecoil(source, recoilX, recoilY);
    }

    public void SetPartStat(PartBase part)
    {
        stats.SetPartStats(part);

        if (part.PartType == EPartType.Legs)
        {
            _currentMovement = part as ILegsMovement;
        }
    }

    public void TakeDamage(float takeDamage)
    {
        // 현재 HP의 값을 데미지 만큼 처리
        // stats.CurrentHealth = takeDamage;

        if (stats.CurrentHealth <= 0) return;

        var damage = (takeDamage - (stats.TotalStats[EStatType.Defence].value + stats.TotalStats[EStatType.AddDefence].value)) * stats.TotalStats[EStatType.DamageReductionRate].value;
        Debug.Log($"Player에게 {damage} 데미지! 효과는 굉장했다!");

        if (damage > 0)
        {
            if (stats.CurrentPartHealth > 0)        // 파츠 HP가 남아있으면 파츠를 우선 데미지 계산
            {
                stats.CurrentPartHealth -= damage;  
                
                // 계산 후 파츠 HP가 음수가 된 경우
                if (stats.CurrentPartHealth < 0)
                {
                    damage += stats.CurrentPartHealth;  // 바디에 적용할 데미지를 감소
                    stats.CurrentPartHealth = 0;        // 파츠 HP를 0으로 초기화
                }
                else
                {
                    damage = 0;
                }
            }

            if (stats.CurrentBodyHealth > 0)
            {
                stats.CurrentBodyHealth -= damage;
                if (stats.CurrentBodyHealth < 0)
                    stats.CurrentBodyHealth = 0;
            }
        }
        else
        {
            // TODO: 데미지가 음수일때 어떻게 처리할 것인지 논의 필요 (힐을 시킬 것인지 무시할 것인지)
        }

        Debug.Log($"HP: Body({stats.CurrentBodyHealth}), Part({stats.CurrentPartHealth})");
    }

    public void HealHp(float healAmount, EHealRange healRange = EHealRange.All)
    {
        if (healAmount <= 0.0f) return;
        if (stats.CurrentHealth <= 0) return;

        float amount = healAmount;

        if (healRange != EHealRange.Part)
        {
            // 계산 후 Body Hp가 최대치를 초과했을 경우
            float postHealth = stats.CurrentBodyHealth;
            stats.CurrentBodyHealth += amount;
            if (stats.CurrentBodyHealth > stats.MaxBodyHealth)
            {
                amount = stats.CurrentBodyHealth - postHealth;          // 파츠 회복량 감소
                stats.CurrentBodyHealth = stats.MaxBodyHealth;      // 바디 Hp를 최대값으로 초기화
            }
            else
            {
                amount = 0;
            }
        }

        if (healRange != EHealRange.Body)
        {
            stats.CurrentPartHealth = Mathf.Clamp(stats.CurrentPartHealth + amount, 0.0f, stats.MaxPartHealth);
        }
    }

    public static void RegisterEvent(Action action)
    {
        OnInteractionKeyPressed -= action;
        OnInteractionKeyPressed += action;
    }

    public static void UnregisterEvent(Action action)
    {
        OnInteractionKeyPressed -= action;
    }

    public void SetMovable(bool canMove)
    {
        _canMove = canMove;
    }

    public PartBase GetCurrentLegsPart()
    {
        return inventory.EquippedItems[EPartType.Legs];
    }

    // Ball Legs를 위한 임시 함수들
    public void PartJump(float jumpVelocity)
    {
        if (!_isGrounded) return;

        Vector3 forward = _followCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();

        _fallVelocity = forward * jumpVelocity * 0.5f;
        _fallVelocity.y = jumpVelocity;

        _fallVelocity.y = jumpVelocity;
        _totalDirection += _fallVelocity;
    }
    #endregion

    #region Private Methods
    private void HandleMove()
    {
        if (!_canMove) return;
        if ((_currentPlayerState & movementBlockMask) != 0) return;

        if (_moveInput == null || _moveInput == Vector2.zero)
        {
            SwitchStateToIdle();
        }
        else
        {
            _currentPlayerState &= ~EPlayerState.Idle;
            _currentPlayerState |= EPlayerState.Moving;
        }

        _moveDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;
        if (inventory.EquippedItems[EPartType.Legs].IsAnimating)
        {
            animator.SetFloat("moveX", _moveDirection.x);
            animator.SetFloat("moveY", _moveDirection.z);
            animator.SetFloat("moveMagnitude", _moveDirection.magnitude);
        }
        
        _totalDirection += _currentMovement.GetMoveDirection(_moveInput, transform, _followCamera.transform);
    }

    private void SwitchStateToIdle()
    {
        _currentPlayerState &= ~EPlayerState.Moving;
        _currentPlayerState |= EPlayerState.Idle;
        _moveDirection = Vector3.zero;
        animator.SetFloat("moveX", 0.0f);
        animator.SetFloat("moveY", 0.0f);
        animator.SetFloat("moveMagnitude", 0.0f);
    }

    private Vector3 CalculateInputDirection()
    {
        Vector3 camForward = _followCamera.transform.forward;
        Vector3 camRight = _followCamera.transform.right;
        camForward.y = 0.0f;
        camRight.y = 0.0f;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 camDirection = camForward * _moveInput.y + camRight * _moveInput.x;

        return camDirection.normalized;
    }

    private void DashMove()
    {
        if (!_canMove) return;
        if (_currentPlayerState != EPlayerState.Dashing) return;

        _totalDirection += _dashDirection * _dashSpeed;
        _followCamera.CameraTarget.position = transform.position + new Vector3(0.0f, 1.2f, 0.0f);
    }

    private void HandleGravity()
    {
        UpdatePlatformMovement();

        if (_currentMovement is LegsHover hoverLegs)
        {
            Vector3 hoverDelta = hoverLegs.CalculateHoverDeltaY();
            _totalDirection += hoverDelta;
            return;
        }

        if ((_isGrounded || _isOnPlatform) && _fallVelocity.y <= 0.0f)
        {
            _currentPlayerState &= ~EPlayerState.Falling;
            _fallVelocity = Vector3.zero;
            _totalDirection += _platformVelocity;
            return;
        }
        _currentPlayerState |= EPlayerState.Falling;
        _fallVelocity.y += -9.8f * gravityScale * Time.deltaTime;
        _totalDirection += _fallVelocity;
    }

    private void UpdatePlatformMovement()
    {
        _isGrounded = Physics.CheckBox(groundCheck.position, boxSize, Quaternion.identity, groundLayerMask);
        _isOnPlatform = IsOnMovingPlatform(out Transform platform);

        if (_isOnPlatform)
        {
            if (_postPlatform != platform)
            {
                _postPlatform = platform;
                _lastPlatformPosition = platform.position;
            }

            if (_lastPlatformPosition == Vector3.zero)
            {
                _lastPlatformPosition = platform.position;
            }

            Vector3 platformDelta = platform.position - _lastPlatformPosition;
            _platformVelocity = platformDelta / Time.deltaTime;
            _lastPlatformPosition = platform.position;
        }
        else
        {
            _platformVelocity = Vector3.zero;
            _lastPlatformPosition = Vector3.zero;
        }
    }

    private bool IsOnMovingPlatform(out Transform platform)
    {
        platform = null;
        // 캐릭터 중심에서 하단으로 BoxCast 진행
        RaycastHit hit;
        Vector3 boxCastOrigin = groundCheck.position;
        float castDistance = 0.3f; // 짧게 설정

        if (Physics.BoxCast(boxCastOrigin, boxSize, Vector3.down, out hit, Quaternion.identity, castDistance))
        {
            // 플랫폼 판정: 'Platform' 태그를 가진 경우만
            if (hit.collider.CompareTag("Platform"))
            {
                platform = hit.collider.transform;
                return true;
            }
        }
        return false;
    }

    private void RotateCharacter()
    {
        if ((_currentPlayerState & EPlayerState.RotateState) == 0)
        {
            _currentPlayerState &= ~EPlayerState.Rotating;
            return;
        }

        Vector3 lookDirection = _followCamera.transform.forward;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.1f)
        {
            // 회전 중 상태 활성화
            _currentPlayerState |= EPlayerState.Rotating;

            // 목표 회전 방향
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // 현재 회전과 목표 회전 사이의 각도
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

            // Slerp로 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 각도가 충분히 줄면 회전 종료 처리
            if (angleDifference < 1f)  // 1도 이내면 회전 완료
            {
                transform.rotation = targetRotation;  // 완전히 맞춤
                _currentPlayerState &= ~EPlayerState.Rotating;
            }
        }
        else
        {
            _currentPlayerState &= ~EPlayerState.Rotating;
        }
    }

    private void Shoot()
    {
        if ((_currentPlayerState & shootBlockMask) != 0) return;
        _currentPlayerState |= EPlayerState.Rotating;
        _isLeftAttackReady = false;
        _isRightAttackReady = false;

        if ((_currentPlayerState & EPlayerState.LeftShooting) != 0)
        {
            if (inventory.EquippedItems[EPartType.ArmL].IsAnimating)
            {
                animator.SetBool("isLeftAttack", true);
            }
        }

        if ((_currentPlayerState & EPlayerState.RightShooting) != 0)
        {
            if (inventory.EquippedItems[EPartType.ArmR].IsAnimating)
            {
                animator.SetBool("isRightAttack", true);
            }
            inventory.EquippedItems[EPartType.ArmR].UseAbility();
        }
    }

    private void AnimCheckShoot()
    {
        // Left attack 애니메이션 상태 체크
        if ((_currentPlayerState & EPlayerState.LeftShooting) != 0 && !_isLeftAttackReady)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(2);
            if (stateInfo.IsName("Shoot") && stateInfo.normalizedTime >= 0.9f)
            {
                _isLeftAttackReady = true;
                inventory.EquippedItems[EPartType.ArmL].UseAbility();
            }
        }
    }
    #endregion
}
