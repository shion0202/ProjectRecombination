using Cinemachine;
using Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

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
    Spawning = 1 << 8,
    Dead = 1 << 9,

    RotateState = Moving | LeftShooting | RightShooting | Zooming,
    ActionState = Idle | Moving | Dashing | LeftShooting | RightShooting | Zooming,
    ShootState = LeftShooting | RightShooting,
    UnmanipulableState = Spawning | Dead,
}

public enum EHealRange
{
    All = 0,
    Body = 1,
    Part = 2,
}

public enum EAnimationType
{
    Base = 0,
    Hover = 1,
    Roller = 2,
    Caterpillar = 3,
    ShootingBase = 4,
    ShootingHover = 5,
    ShootingRoller = 6,
    ShootingCaterpillar = 7,
}

[Serializable]
public struct BaseAnimation
{
    public AnimatorOverrideController overrideController;
    public bool isOnlyLoop;
}

public class PlayerController : MonoBehaviour, PlayerActions.IPlayerActionMapActions
{
    #region Variables
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private RigBuilder rigBuilder;
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
    [SerializeField, Range(0.0f, 100.0f)] private float jumpVelocity = 50.0f;
    [SerializeField, Range(0.01f, 100.0f)] private float rotationSpeed = 40.0f;
    private PlayerActions _playerActions;
    private Vector2 _moveInput;
    private Vector3 _moveDirection;
    private Vector3 _totalDirection = Vector3.zero;
    private bool _canMove = true;
    private ILegsMovement _currentMovement;
    private Vector2 _postMoveInput = Vector2.zero;
    private Vector2 _currentMoveInput = Vector2.zero;

    [Header("Gravity")]
    [SerializeField] private Vector3 boxSize = new Vector3(0.2f, 0.1f, 0.2f);
    [SerializeField] private float gravityScale = 2.0f;
    [SerializeField] private LayerMask groundLayerMask;
    private bool _isGrounded = false;
    private bool _isOnPlatform = false;
    private Vector3 _fallVelocity;
    private Transform _postPlatform;
    private Vector3 _lastPlatformPosition = Vector3.zero;
    private Vector3 _platformVelocity;
    private float _groundCheckBufferTime = 0.1f;  // 0.1초까지 낙하 감지 지연
    private float _groundCheckTimer = 0.0f;

    [Header("Dash")]
    private Vector3 _dashDirection = Vector3.zero;
    private float _dashSpeed = 0.0f;

    [Header("Animation")]
    [SerializeField] private List<BaseAnimation> animations = new();
    [SerializeField] private List<GameObject> aimObjects = new();
    private int _currentAnimationIndex = 0;
    private bool _isPlaySpawnAnimation = false;
    private EAnimationType _currentAnimType = EAnimationType.Base;
    private EAnimationType _postAnimType = EAnimationType.Base;

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

    public EAnimationType PostAnimType
    {
        get { return _postAnimType; }
        set { _postAnimType = value; }
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

        SetOvrrideAnimator(EAnimationType.Base);

        foreach (var obj in aimObjects)
        {
            MultiAimConstraint aimObj = obj.GetComponent<MultiAimConstraint>();
            if (aimObj != null)
            {
                aimObj.weight = 0.0f;
            }
        }

        // stats.CurrentHealth = stats.TotalStats[EStatType.MaxHp].Value;
        // GUIManager.instance.SetHpSlider(stats.CurrentHealth, stats.TotalStats[EStatType.MaxHp].Value);
    }

    private void OnEnable()
    {
        _playerActions.PlayerActionMap.Enable();

        PlayerSpawnAnimation();
    }

    private void Start()
    {
        ILegsMovement legsMovement = inventory.EquippedItems[EPartType.Legs][0] as ILegsMovement;
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
        CheckSpawnAnimationEnd();

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
        if ((_currentPlayerState & movementBlockMask) != 0) return;

        _moveInput = context.ReadValue<Vector2>();
        if (_moveInput != null && _moveInput != Vector2.zero)
        {
            _postMoveInput = _moveInput;
        }
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

            inventory.EquippedItems[EPartType.Legs][0].UseAbility();
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnShoulderSkill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if ((_currentPlayerState & dashBlockMask) != 0) return;

            inventory.EquippedItems[EPartType.Shoulder][0].UseAbility();
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

    // 테스트용 점프 함수
    void PlayerActions.IPlayerActionMapActions.OnJump(InputAction.CallbackContext context)
    {
        if ((_currentPlayerState & EPlayerState.UnmanipulableState) != 0) return;

        if (context.started)
        {
            // 바닥에 있을 때만 점프 실행
            if (_isGrounded || _isOnPlatform)
            {
                _fallVelocity.y = jumpVelocity;

                // 점프 상태 갱신
                _currentPlayerState |= EPlayerState.Falling;
            }
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnLeftAttack(InputAction.CallbackContext context)
    {
        if ((_currentPlayerState & EPlayerState.UnmanipulableState) != 0) return;

        if (context.started)
        {
            _currentPlayerState |= EPlayerState.LeftShooting;
            Shoot();
        }

        if (context.canceled)
        {
            CancleAttack(true);
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnRightAttack(InputAction.CallbackContext context)
    {
        if ((_currentPlayerState & EPlayerState.UnmanipulableState) != 0) return;
        if (!inventory.EquippedItems[EPartType.ArmR][0].IsAnimating) return;

        if (context.started)
        {
            _currentPlayerState |= EPlayerState.RightShooting;
            Shoot();
        }

        if (context.canceled)
        {
            CancleAttack(false);
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnInteraction(InputAction.CallbackContext context)
    {
        if ((_currentPlayerState & EPlayerState.UnmanipulableState) != 0) return;

        if (context.started)
        {
            OnInteractionKeyPressed?.Invoke();
        }
    }

    void PlayerActions.IPlayerActionMapActions.OnMouseScroll(InputAction.CallbackContext context)
    {
        //if ((_currentPlayerState & EPlayerState.UnmanipulableState) != 0) return;

        //if (context.performed)
        //{
        //    // 120 또는 -120 (-0.008을 곱하면 1과 근사한 0.96 또는 -0.96)
        //    float scrollValue = context.ReadValue<float>();
        //    _followCamera.ScrollY = scrollValue * -0.008f;
        //}
    }

    void PlayerActions.IPlayerActionMapActions.OnResetCamera(InputAction.CallbackContext context)
    {
        //if (context.started)
        //{
        //    _followCamera.ResetCamera();
        //}
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

        animator.SetBool("isDashing", true);

        if (_moveInput == null || _moveInput == Vector2.zero)
        {
            animator.SetFloat("dashX", 0.0f);
            animator.SetFloat("dashY", 1.0f);
        }
        else
        {
            animator.SetFloat("dashX", _moveInput.x);
            animator.SetFloat("dashY", _moveInput.y);
        }
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

        animator.SetBool("isDashing", false);

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
        // Debug.Log($"Player에게 {damage} 데미지! 효과는 굉장했다!");

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

        // Debug.Log($"HP: Body({stats.CurrentBodyHealth}), Part({stats.CurrentPartHealth})");

        if (stats.CurrentHealth <= 0)
        {
            // 사망 로직
            _currentPlayerState = 0;
            _currentPlayerState |= EPlayerState.Dead;

            SetMovable(false);

            animator.SetTrigger("deadTrigger");

            // 사격 중 또는 스킬 시전 중 사망하는 경우 고려
            inventory.EquippedItems[EPartType.ArmL][0].UseCancleAbility();
            animator.SetBool("isLeftAttack", false);
            _isLeftAttackReady = false;
            Stats.RemoveModifier(this);
            SetOvrrideAnimator(_postAnimType);
            MultiAimConstraint aimObj = aimObjects[0].GetComponent<MultiAimConstraint>();
            if (aimObj != null)
            {
                aimObj.weight = 0.0f;
            }

            inventory.EquippedItems[EPartType.ArmR][0].UseCancleAbility();
            animator.SetBool("isRightAttack", false);
            SetOvrrideAnimator(_postAnimType);
            aimObj = aimObjects[1].GetComponent<MultiAimConstraint>();
            if (aimObj != null)
            {
                aimObj.weight = 0.0f;
            }
        }
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
        return inventory.EquippedItems[EPartType.Legs][0];
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

    public bool SetOvrrideAnimator(EAnimationType type)
    {
        if (animations.Count <= (int)type) return false;

        _currentAnimType = type;

        animator.runtimeAnimatorController = animations[(int)type].overrideController;
        animator.SetBool("isOnlyLoop", animations[(int)type].isOnlyLoop);

        rigBuilder.enabled = false;
        rigBuilder.enabled = true;

        MultiAimConstraint aimObj = aimObjects[0].GetComponent<MultiAimConstraint>();
        if (aimObj != null)
        {
            aimObj.weight = _isLeftAttackReady ? 1.0f : 0.0f;
        }

        aimObj = aimObjects[0].GetComponent<MultiAimConstraint>();
        if (aimObj != null)
        {
            aimObj.weight = _isRightAttackReady ? 1.0f : 0.0f;
        }

        for (int i = 2; i < aimObjects.Count; ++i)
        {
            aimObj = aimObjects[i].GetComponent<MultiAimConstraint>();
            if (aimObj != null)
            {
                aimObj.weight = 0.6f;
            }
        }

        return true;
    }
    #endregion

    #region Private Methods
    private void HandleMove()
    {
        if (!_canMove) return;
        if ((_currentPlayerState & movementBlockMask) != 0) return;

        // Lerp 블렌딩 적용 (0.1f는 변화 속도, 필요에 따라 수정)
        _currentMoveInput = Vector2.Lerp(_currentMoveInput, _moveInput, 0.1f);

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
        if (inventory.EquippedItems[EPartType.Legs][0].IsAnimating)
        {
            animator.SetFloat("moveX", _currentMoveInput.x);
            animator.SetFloat("moveY", _currentMoveInput.y);
            animator.SetFloat("moveMagnitude", _moveDirection.magnitude);
        }
        
        _totalDirection += _currentMovement.GetMoveDirection(_moveInput, transform, _followCamera.transform);
    }

    private void SwitchStateToIdle()
    {
        _currentPlayerState &= ~EPlayerState.Moving;
        _currentPlayerState |= EPlayerState.Idle;
        _moveDirection = Vector3.zero;
        animator.SetFloat("moveX", _postMoveInput.x);
        animator.SetFloat("moveY", _postMoveInput.y);
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
        // 대시 중이라면 중력 무시
        if ((_currentPlayerState & EPlayerState.Dashing) != 0)
        {
            // 대시 중에는 중력 벡터를 초기화하거나 그대로 유지(중력 영향 없음)
            _fallVelocity = Vector3.zero;
            _groundCheckTimer = 0.0f;
            return;
        }

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
            animator.SetBool("isFalling", false);
            _fallVelocity.y = -5.0f * gravityScale * Time.deltaTime; // 약간의 하강력 유지로 땅에 붙어있게 함
            _totalDirection += _platformVelocity;
            _groundCheckTimer = 0.0f;

            _totalDirection += _fallVelocity;
            return;
        }

        _groundCheckTimer += Time.deltaTime;
        if (_groundCheckTimer >= _groundCheckBufferTime)
        {
            _currentPlayerState |= EPlayerState.Falling;
            animator.SetBool("isFalling", true);
        }

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
        if (!_canMove) return;

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

            if (angleDifference < 10f)
            {
                _currentPlayerState &= ~EPlayerState.Rotating;
            }

            // 각도가 충분히 줄면 회전 종료 처리
            if (angleDifference < 1f)  // 1도 이내면 회전 완료
            {
                transform.rotation = targetRotation;  // 완전히 맞춤
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
            _postAnimType = _currentAnimType;
            SetOvrrideAnimator(_postAnimType + 4);
            if (inventory.EquippedItems[EPartType.ArmL][0].IsAnimating)
            {
                animator.SetBool("isLeftAttack", true);
            }
            stats.AddModifier(new StatModifier(EStatType.WalkSpeed, EStatModifierType.PercentMul, -0.3f, this));

            _followCamera.CurrentCameraState = (ECameraState)(_currentAnimType);
        }

        if ((_currentPlayerState & EPlayerState.RightShooting) != 0)
        {
            if (inventory.EquippedItems[EPartType.ArmR][0].IsAnimating)
            {
                animator.SetBool("isRightAttack", true);
            }

            _postAnimType = _currentAnimType;
            SetOvrrideAnimator(_postAnimType + 4);
            stats.AddModifier(new StatModifier(EStatType.WalkSpeed, EStatModifierType.PercentMul, -0.3f, this));

            _followCamera.CurrentCameraState = (ECameraState)(_currentAnimType);
        }
    }

    private void AnimCheckShoot()
    {
        // Left attack 애니메이션 상태 체크
        if ((_currentPlayerState & EPlayerState.LeftShooting) != 0 && !_isLeftAttackReady)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);
            if (stateInfo.IsName("Shoot") && !animator.IsInTransition(1))
            {
                _isLeftAttackReady = true;
                MultiAimConstraint aimObj = aimObjects[0].GetComponent<MultiAimConstraint>();
                if (aimObj != null)
                {
                    aimObj.weight = 1.0f;
                }

                inventory.EquippedItems[EPartType.ArmL][0].UseAbility();
            }
        }

        if ((_currentPlayerState & EPlayerState.RightShooting) != 0 && !_isRightAttackReady)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(2);
            if (stateInfo.IsName("Shoot") && !animator.IsInTransition(2))
            {
                _isRightAttackReady = true;
                MultiAimConstraint aimObj = aimObjects[1].GetComponent<MultiAimConstraint>();
                if (aimObj != null)
                {
                    aimObj.weight = 1.0f;
                }
                inventory.EquippedItems[EPartType.ArmR][0].UseAbility();
            }
        }   
    }

    private void PlayerSpawnAnimation()
    {
        SetMovable(false);
        animator.SetBool("isSpawning", true);
        _currentPlayerState |= EPlayerState.Spawning;
    }

    private void CheckSpawnAnimationEnd()
    {
        if ((_currentPlayerState & EPlayerState.Spawning) == 0) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // 스폰 애니메이션 이름 또는 해시코드로 비교
        if (!stateInfo.IsName("Spawn"))
        {
            // 스폰 애니메이션이 종료됨
            SetMovable(true);
            animator.SetBool("isSpawning", false);
            _currentPlayerState &= ~EPlayerState.Spawning;

            for (int i = 2; i < aimObjects.Count; ++i)
            {
                MultiAimConstraint aimObj = aimObjects[i].GetComponent<MultiAimConstraint>();
                if (aimObj != null)
                {
                    aimObj.weight = 0.6f;
                }
            }
        }
    }

    private void CancleAttack(bool isLeft)
    {
        if (isLeft)
        {
            inventory.EquippedItems[EPartType.ArmL][0].UseCancleAbility();

            animator.SetBool("isLeftAttack", false);
            _previousState &= ~EPlayerState.LeftShooting;
            _currentPlayerState &= ~EPlayerState.LeftShooting;
            _isLeftAttackReady = false;  // 상태 초기화

            MultiAimConstraint aimObj = aimObjects[0].GetComponent<MultiAimConstraint>();
            if (aimObj != null)
            {
                aimObj.weight = 0.0f;
            }
        }
        else
        {
            inventory.EquippedItems[EPartType.ArmR][0].UseCancleAbility();

            animator.SetBool("isRightAttack", false);
            _previousState &= ~EPlayerState.RightShooting;
            _currentPlayerState &= ~EPlayerState.RightShooting;
            _isRightAttackReady = false;

            MultiAimConstraint aimObj = aimObjects[1].GetComponent<MultiAimConstraint>();
            if (aimObj != null)
            {
                aimObj.weight = 0.0f;
            }
        }

        if ((_currentPlayerState & EPlayerState.ShootState) == 0)
        {
            Stats.RemoveModifier(this);
            SetOvrrideAnimator(_postAnimType);
            _followCamera.CurrentCameraState = (ECameraState)(_currentAnimType);
        }
    }
    #endregion
}
